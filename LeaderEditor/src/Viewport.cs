﻿using ImGuiNET;
using LeaderEditor.Gui;
using LeaderEngine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Numerics;
using Shader = LeaderEngine.Shader;

namespace LeaderEditor
{
    public class Viewport : WindowComponent
    {
        private Framebuffer framebuffer;

        public Vector2 ViewportSize;

        private float[] vertices =
        {
            -1.0f, -1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f, 0.0f,
             1.0f, -1.0f, 0.0f
        };

        private uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private VertexArray gridVertArray;
        private Shader gridShader;

        public override void EditorStart()
        {
            framebuffer = new Framebuffer(1280, 720);

            Application.main.SceneRender += SceneRender;
            Application.main.PostSceneRender += PostSceneRender;
            Application.main.PostProcess += PostProcess;
            Application.main.PostGuiRender += PostGuiRender;

            ImGuiController.main.OnImGui += OnImGui;

            //setup grid rendering
            gridVertArray = new VertexArray(vertices, indices, new VertexAttrib[]
            {
                new VertexAttrib { location = 0, size = 3 }
            });

            gridShader = Shader.FromSourceFile(AppContext.BaseDirectory + "DefaultAssets/Shaders/Editor/grid-vs.glsl", AppContext.BaseDirectory + "DefaultAssets/Shaders/Editor/grid-fs.glsl");
            //end setup

            MainMenuBar.RegisterWindow("Viewport", this);
        }

        private void SceneRender()
        {
            //resize viewport and postprocessor
            Application.main.ResizeViewport((int)ViewportSize.X, (int)ViewportSize.Y);
            Application.main.PostProcessor.Resize((int)ViewportSize.X, (int)ViewportSize.Y);
        }

        private void PostSceneRender()
        {
            if (EditorController.Mode != EditorController.EditorMode.Editor)
                return;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            gridShader.SetMatrix4("v", RenderingGlobals.View);
            gridShader.SetMatrix4("p", RenderingGlobals.Projection);

            gridShader.Use();
            gridVertArray.Use();

            GL.DrawElements(PrimitiveType.Triangles, gridVertArray.GetVerticesCount(), DrawElementsType.UnsignedInt, 0);

            GL.Disable(EnableCap.Blend);
        }

        private void PostProcess()
        {
            //resize framebuffer to match viewport
            framebuffer.Resize((int)ViewportSize.X, (int)ViewportSize.Y);

            //render scene to a framebuffer
            framebuffer.Begin();

            //clear buffers
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void PostGuiRender()
        {
            //end the render
            framebuffer.End();

            GL.Viewport(0, 0, Application.main.Size.X, Application.main.Size.Y);
        }


        private void OnImGui()
        {
            if (IsOpen)
                if (ImGui.Begin("Viewport", ref IsOpen, ImGuiWindowFlags.NoCollapse))
                {
                    if (ImGui.IsWindowFocused())
                    {
                        if (Input.GetKeyDown(Keys.P))
                            if (EditorController.Mode == EditorController.EditorMode.Editor)
                                EditorController.Mode = EditorController.EditorMode.Play;
                            else EditorController.Mode = EditorController.EditorMode.Editor;

                        if (EditorController.Mode == EditorController.EditorMode.Editor)
                            EditorCamera.main.UpdateCamMove();
                    }

                    //display to framebuffer texture on gui
                    ImGui.Image((IntPtr)framebuffer.GetColorTexture(), ViewportSize = ImGui.GetContentRegionAvail(), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 0.0f));
                    ImGui.End();
                }
        }
    }
}