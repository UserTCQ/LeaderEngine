﻿using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace LeaderEngine
{
    public class ForwardRenderer : GLRenderer
    {
        private Dictionary<DrawType, List<GLDrawData>> drawLists = new Dictionary<DrawType, List<GLDrawData>>()
        {
            { DrawType.Opaque, new List<GLDrawData>() },
            { DrawType.Transparent, new List<GLDrawData>() },
            { DrawType.GUI, new List<GLDrawData>() }
        };

        private Framebuffer ppFramebuffer;

        private Mesh ppMesh;
        private Shader ppShader;

        public override void Init()
        {
            ppFramebuffer = new Framebuffer("PostProcessFBO", ViewportSize.X, ViewportSize.Y, new Attachment[]
            {
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.Rgba,
                    PixelFormat = PixelFormat.Rgba,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.ColorAttachment0,
                    TextureParamsInt = new TextureParamInt[]
                    {
                        new TextureParamInt { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Linear },
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Linear }
                    }
                },
                new Attachment
                {
                    Draw = false,
                    PixelInternalFormat = PixelInternalFormat.DepthComponent,
                    PixelFormat = PixelFormat.DepthComponent,
                    PixelType = PixelType.Float,
                    FramebufferAttachment = FramebufferAttachment.DepthAttachment,
                    TextureParamsInt = new TextureParamInt[]
                    {
                        new TextureParamInt { ParamName = TextureParameterName.TextureMinFilter, Param = (int)TextureMinFilter.Nearest },
                        new TextureParamInt { ParamName = TextureParameterName.TextureMagFilter, Param = (int)TextureMagFilter.Nearest }
                    }
                }
            });

            ppMesh = new Mesh("PostProcessQuad");
            ppMesh.LoadMesh(new Vertex[] 
            {
                new Vertex { Position = new Vector3(1.0f, 1.0f, 0.0f), UV = new Vector2(1.0f, 1.0f) },
                new Vertex { Position = new Vector3(1.0f, -1.0f, 0.0f), UV = new Vector2(1.0f, 0.0f) },
                new Vertex { Position = new Vector3(-1.0f, -1.0f, 0.0f), UV = new Vector2(0.0f, 0.0f) },
                new Vertex { Position = new Vector3(-1.0f, 1.0f, 0.0f), UV = new Vector2(0.0f, 1.0f) }
            },
            new uint[]
            {
                0, 1, 3,
                1, 2, 3
            });

            ppShader = Shader.FromSourceFile("PostProcessShader",
                Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/post-process.vert"),
                Path.Combine(AppContext.BaseDirectory, "EngineAssets/Shaders/post-process.frag"));

            Logger.Log("Renderer initialized.", true);
        }

        public override void PushDrawData(DrawType drawType, GLDrawData drawData)
        {
            drawLists[drawType].Add(drawData);
        }

        public override void Update()
        {
            ppFramebuffer.Resize(ViewportSize.X, ViewportSize.Y);
        }

        public override void Render()
        {
            if (Camera.Main == null)
                return;

            //set matrices
            DataManager.CurrentScene.SceneRootEntities.ForEach(en => en.Transform.CalculateModelMatrixRecursively());

            Camera.Main.CalculateViewProjection(out Matrix4 view, out Matrix4 projection);

            //call all render funcs
            DataManager.CurrentScene.SceneRootEntities.ForEach(en => en.RecursivelyRender(view, projection));

            ppFramebuffer.Begin();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //render opaque
            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.FrontFace(FrontFaceDirection.Ccw);

            var opDrawList = drawLists[DrawType.Opaque];

            opDrawList.ForEach(drawData =>
            {
                Mesh mesh = drawData.Mesh;
                Shader shader = drawData.Shader;
                Material material = drawData.Material;
                UniformData uniforms = drawData.Uniforms;

                if (mesh == null || shader == null || uniforms == null)
                    return;

                mesh.Use();
                shader.Use();

                material?.Use(shader);
                uniforms.Use(shader);

                GL.DrawElements(mesh.PrimitiveType, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
            });

            //render transparent
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            var trDrawList = drawLists[DrawType.Transparent];

            trDrawList.ForEach(drawData =>
            {
                Mesh mesh = drawData.Mesh;
                Shader shader = drawData.Shader;
                Material material = drawData.Material;
                UniformData uniforms = drawData.Uniforms;

                if (mesh == null || shader == null || uniforms == null)
                    return;

                mesh.Use();
                shader.Use();

                material?.Use(shader);
                uniforms.Use(shader);

                GL.DrawElements(mesh.PrimitiveType, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
            });

            ppFramebuffer.End();

            //reset states
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            //post process
            ppMesh.Use();
            ppShader.Use();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ppFramebuffer.GetTexture(FramebufferAttachment.ColorAttachment0));

            GL.DrawElements(PrimitiveType.Triangles, ppMesh.IndicesCount, DrawElementsType.UnsignedInt, 0);

            ClearDrawList();
        }

        private void ClearDrawList()
        {
            foreach (var kvp in drawLists)
                kvp.Value.Clear();
        }
    }
}
