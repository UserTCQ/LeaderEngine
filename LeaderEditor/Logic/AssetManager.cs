﻿using ImGuiNET;
using LeaderEngine;
using System.Numerics;
using System.Windows.Forms;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace LeaderEditor
{
    public class AssetManager : Component
    {
        public static Prefab SelectedPrefab;
        public static Mesh SelectedMesh;
        public static Texture SelectedTexture;
        public static Material SelectedMaterial;
        public static AudioClip SelectedClip;
        public static Cubemap SelectedCubemap;

        private void Start()
        {
            //register ImGui
            ImGuiController.RegisterImGui(ImGuiRenderer);
        }

        private void ImGuiRenderer()
        {
            if (ImGui.Begin("Asset Manager"))
            {
                if (Input.GetKeyDown(Keys.I) && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
                    SelectedPrefab?.Instantiate();

                if (ImGui.BeginChild("cubemaps", new Vector2(210f, 0f), true))
                {
                    ImGui.Text("Cubemaps");

                    ImGui.SameLine();

                    if (ImGui.Button("Import Cubemap", new Vector2(115f, 0f)))
                    {
                        var aiw = new AssetImporterWizard("cubemap-importer");
                        aiw.Title = "Import Cubemap";
                    }

                    var assetImporter = AssetImporterWizard.GetAssetImporter("cubemap-importer");

                    if (assetImporter != null)
                    {
                        if (assetImporter.Begin())
                        {
                            string right = assetImporter.OpenFileDialog("Right", "Image|*.jpg;*.png");
                            string left = assetImporter.OpenFileDialog("Left", "Image|*.jpg;*.png");
                            string top = assetImporter.OpenFileDialog("Top", "Image|*.jpg;*.png");
                            string bottom = assetImporter.OpenFileDialog("Bottom", "Image|*.jpg;*.png");
                            string back = assetImporter.OpenFileDialog("Back", "Image|*.jpg;*.png");
                            string front = assetImporter.OpenFileDialog("Front", "Image|*.jpg;*.png");

                            assetImporter.End();

                            if (assetImporter.Finished())
                            {
                                Cubemap.FromFile("cubemap", right, left, top, bottom, back, front);

                                assetImporter.Dispose();
                            }
                        }
                    }

                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-cb-win"))
                    {
                        foreach (var a in GlobalData.Cubemaps)
                            if (ImGui.Selectable(a.Value.Name, SelectedCubemap == a.Value))
                                SelectedCubemap = a.Value;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("clips", new Vector2(210f, 0f), true))
                {
                    ImGui.Text("Audio Clips");

                    ImGui.SameLine();

                    if (ImGui.Button("Import Audio", new Vector2(100f, 0f)))
                    {
                        var aiw = new AssetImporterWizard("audio-importer");
                        aiw.Title = "Import Audio";
                    }

                    var assetImporter = AssetImporterWizard.GetAssetImporter("audio-importer");

                    if (assetImporter != null)
                    {
                        if (assetImporter.Begin())
                        {
                            string path = assetImporter.OpenFileDialog("Audio Clip", "Audio File|*.wav");

                            if (assetImporter.Finished())
                            {
                                AudioClip.FromFile("audio clip", path);

                                assetImporter.Dispose();
                            }
                        }

                        assetImporter.End();
                    }

                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-ac-win"))
                    {
                        foreach (var a in GlobalData.AudioClips)
                            if (ImGui.Selectable(a.Value.Name, SelectedClip == a.Value))
                                SelectedClip = a.Value;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("prefabs", new Vector2(210f, 0f), true))
                {
                    ImGui.Text("Prefabs");

                    ImGui.SameLine();

                    ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - 100f);

                    if (ImGui.Button("Import Model", new Vector2(100f, 0f)))
                    {
                        using (var ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "3D Model|*.fbx;*.obj";

                            ofd.ShowDialog();

                            if (!string.IsNullOrEmpty(ofd.FileName))
                            {
                                AssetImporter.LoadModelFromFile(ofd.FileName);
                            }
                        }
                    }

                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-prefabs-win"))
                    {
                        foreach (var p in GlobalData.Prefabs)
                            if (ImGui.Selectable(p.Value.Name, SelectedPrefab == p.Value))
                                SelectedPrefab = p.Value;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("meshes", new Vector2(210f, 0f), true))
                {
                    ImGui.Text("Meshes");
                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-meshes-win"))
                    {
                        foreach (var m in GlobalData.Meshes)
                            if (ImGui.Selectable(m.Value.Name, SelectedMesh == m.Value))
                                SelectedMesh = m.Value;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("textures", new Vector2(210f, 0f), true))
                {
                    ImGui.Text("Textures");
                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-tex-win"))
                    {
                        foreach (var t in GlobalData.Textures)
                            if (ImGui.Selectable(t.Value.Name, SelectedTexture == t.Value))
                                SelectedTexture = t.Value;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.SameLine();
                if (ImGui.BeginChild("materials", new Vector2(210f, 0f), true))
                {
                    ImGui.Text("Materials");
                    ImGui.Separator();

                    if (ImGui.BeginChild("sub-mats-win"))
                    {
                        foreach (var m in GlobalData.Materials)
                            if (ImGui.Selectable(m.Value.Name, SelectedMaterial == m.Value))
                                SelectedMaterial = m.Value;
                        ImGui.EndChild();
                    }

                    ImGui.EndChild();
                }
                ImGui.End();
            }
        }
    }
}
