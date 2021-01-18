﻿using ImGuiNET;
using LeaderEditor.Data;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using System.Linq;

namespace LeaderEditor
{
    public class SceneHierachy : WindowComponent
    {
        public static List<Entity> SceneObjects { 
            get 
            {
                List<Entity> entitys = new List<Entity>();

                entitys.AddRange(Application.Main.WorldEntities.Where(x => x.Tag != "Editor"));
                entitys.AddRange(Application.Main.WorldEntities_Transparent);
                entitys.AddRange(Application.Main.GuiEntities);

                return entitys;
            } 
        }

        public static Entity SelectedEntity = null;

        private static readonly Dictionary<RenderHint, string> renderHintText = new Dictionary<RenderHint, string>()
        {
            { RenderHint.Opaque, "[Opaque]" },
            { RenderHint.Transparent, "[Transparent]" },
            { RenderHint.Gui, "[Gui]" }
        };

        private string[] objectTypes = { "Opaque", "Transparent", "Gui" };
        private string currentType = "Opaque";

        public override void EditorStart()
        {
            ImGuiController.RegisterImGui(OnImGui);

            MainMenuBar.RegisterWindow("Scene Hierachy", this);
        }

        public override void EditorUpdate()
        {
            //delete object
            if (Input.GetKeyDown(Keys.Delete) && SelectedEntity != null)
            {
                SelectedEntity.Destroy();
                SceneObjects.Remove(SelectedEntity);

                SelectedEntity = null;
            }

            if (Input.GetKeyDown(Keys.L) && SelectedEntity != null)
            {
                SelectedEntity.MoveTo(RenderHint.Transparent);
            }
        }

        private void OnImGui()
        {
            //render scene hierachy gui
            if (IsOpen)
                if (ImGui.Begin("Scene Hierachy", ref IsOpen))
                {
                    //select
                    ImGui.SetNextItemWidth(120.0f);

                    if (ImGui.BeginCombo("##combo", currentType))
                    {
                        foreach (string typeStr in objectTypes)
                        {
                            if (ImGui.Selectable(typeStr, currentType == typeStr))
                                currentType = typeStr;
                        }
                        ImGui.EndCombo();
                    }

                    if (SelectedEntity != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Go to {SelectedEntity.Name}"))
                        {
                            EditorCamera.Main.LookAt(SelectedEntity.Transform.Position);
                        }
                    }

                    //draw all objects
                    RenderObjectTree();
                    ImGui.End();
                }
        }

        //new object function
        private void CreateNewObject(Entity parent)
        {
            if (string.IsNullOrEmpty(AssetLoader.LoadedProjectDir))
                return;

            RenderHint renderHint = RenderHint.Opaque;

            switch (currentType)
            {
                case "Opaque":
                    renderHint = RenderHint.Opaque;
                    break;
                case "Transparent":
                    renderHint = RenderHint.Transparent;
                    break;
                case "Gui":
                    renderHint = RenderHint.Gui;
                    break;
            }

            Entity go = new Entity("New Entity", renderHint);

            go.Parent = parent;
        }

        private int index = 0;

        private void RenderObjectTree()
        {
            List<Entity> _sceneObjects = SceneObjects;

            index = 0;
            if (ImGui.BeginChild("Scene"))
            {
                for (int i = 0; i < SceneObjects.Count; i++)
                {
                    var go = _sceneObjects[i];

                    if (go.Parent == null)
                        RecursivelyRender(go);
                }

                if (!ImGui.IsAnyItemHovered() && ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                        ImGui.OpenPopup("Entity Menu");

                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        SelectedEntity = null;
                }

                if (ImGui.BeginPopup("Entity Menu")) 
                { 
                    if (ImGui.MenuItem("New Entity"))
                        Application.Main.ExecuteNextUpdate(() => CreateNewObject(null));

                    ImGui.EndPopup();
                }

                ImGui.EndChild();
            }
        }

        private void RecursivelyRender(Entity en)
        {
            ImGui.PushID(en.Name + index);

            index++;

            ImGuiTreeNodeFlags nodeFlags = ImGuiTreeNodeFlags.OpenOnArrow;

            if (SelectedEntity == en)
                nodeFlags |= ImGuiTreeNodeFlags.Selected;

            bool nodeOpen = ImGui.TreeNodeEx(en.Name, nodeFlags);

            if (ImGui.IsItemClicked())
                SelectedEntity = en;

            if (ImGui.BeginPopupContextItem("Entity Popup"))
            {
                if (ImGui.MenuItem("New Entity"))
                    Application.Main.ExecuteNextUpdate(() => CreateNewObject(en));

                if (ImGui.MenuItem("Delete"))
                    Application.Main.ExecuteNextUpdate(() => en.Destroy());

                ImGui.EndPopup();
            }

            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1.0f));
            ImGui.Text(renderHintText[en.RenderHint]);
            ImGui.PopStyleColor();

            if (nodeOpen)
            {
                for (int i = 0; i < en.Children.Count; i++)
                    RecursivelyRender(en.Children[i]);

                ImGui.TreePop();
            }

            ImGui.PopID();
        }
    }
}
