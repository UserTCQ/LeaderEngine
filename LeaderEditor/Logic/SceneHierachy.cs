﻿using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using System;
using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text;
using LeaderEditor.Data;
using OpenTK.Windowing.Common;

namespace LeaderEditor
{
    public class SceneHierachy : WindowComponent
    {
        public static List<GameObject> SceneObjects = new List<GameObject>();
        public static GameObject SelectedObject { 
            get 
            { 
                if (SelectedObjectIndex < SceneObjects.Count && SelectedObjectIndex > -1)
                    return SceneObjects[SelectedObjectIndex];

                return null;
            } 
        }

        private static int SelectedObjectIndex = -1;

        public override void Start()
        {
            ImGuiController.main.OnImGui += OnImGui;
            Application.main.UpdateFrame += UpdateSceneHierachy;

            MainMenuBar.RegisterWindow("Scene Hierachy", this);
        }

        public void UpdateSceneHierachy(FrameEventArgs e)
        {
            //delete object
            if (InputManager.GetKeyDown(Keys.Delete) && SelectedObject != null)
            {
                SelectedObject.Destroy();
                SceneObjects.Remove(SelectedObject);

                if (SceneObjects.Count <= SelectedObjectIndex)
                    SelectedObjectIndex = -1;
            }

            //press right ctrl to deselect
            if (InputManager.GetKeyDown(Keys.RightControl))
                SelectedObjectIndex = -1;
        }

        private void OnImGui()
        {
            //render scene hierachy gui
            if (IsOpen)
                if (ImGui.Begin("Scene Hierachy", ref IsOpen))
                {
                    //new object button
                    if (ImGui.Button("New Object") && !string.IsNullOrEmpty(AssetLoader.LoadedProjectDir))
                        CreateNewObject();

                    if (SelectedObject != null)
                    {
                        ImGui.SameLine();
                        if (ImGui.Button($"Go to {SelectedObject.Name}"))
                        {
                            EditorCamera.main.LookAt(SelectedObject.Transform.Position);
                        }
                    }

                    //draw all objects
                    for (int i = 0; i < SceneObjects.Count; i++)
                    {
                        var go = SceneObjects[i];

                        ImGui.PushID(go.Name + i);

                        if (ImGui.Selectable(go.Name, i == SelectedObjectIndex, ImGuiSelectableFlags.DontClosePopups))
                            SelectedObjectIndex = i;

                        ImGui.PopID();
                    }
                    ImGui.End();
                }
        }

        //new object function
        private void CreateNewObject()
        {
            SceneObjects.Add(new GameObject("New GameObject"));
        }
    }
}
