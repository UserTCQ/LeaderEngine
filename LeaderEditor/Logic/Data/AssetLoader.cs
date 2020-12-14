﻿using System;
using System.Collections.Generic;
using System.IO;
using LeaderEditor.Compilation;
using Microsoft.CodeAnalysis.Emit;
using System.Text;
using LeaderEngine;
using Assimp;
using System.Linq;

using Mesh = LeaderEngine.Mesh;

namespace LeaderEditor.Data
{
    public static class AssetLoader
    {
        private static List<Type> loadedTypes = new List<Type>();
        public static string LoadedProjectDir;

        public static void LoadProject(string prjPath)
        {
            SceneHierachy.SceneObjects.ForEach(x => x.Destroy());
            SceneHierachy.SceneObjects.Clear();

            loadedTypes.ForEach(x => Inspector.SerializeableComponents.Remove(x));
            loadedTypes.Clear();

            LoadedProjectDir = Path.GetDirectoryName(prjPath);

            string scriptsDir = Path.Combine(LoadedProjectDir, "Scripts");
            Directory.CreateDirectory(scriptsDir);

            string[] sourcePaths = Directory.GetFiles(scriptsDir, "*.cs", SearchOption.AllDirectories);

            if (sourcePaths.Length == 0)
                return;

            string[] sources = new string[sourcePaths.Length];

            for (int i = 0; i < sourcePaths.Length; i++)
                sources[i] = File.ReadAllText(sourcePaths[i]);

            EmitResult compilationResult;

            Compiler compiler = new Compiler();
            Type[] types = compiler.Compile(sources, out compilationResult);

            if (compilationResult.Success)
            {
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Component)))
                    {
                        loadedTypes.Add(type);
                        Inspector.SerializeableComponents.Add(type, null);
                    }
                }
            }
        }

        public static string LoadAsset(string path)
        {
            if (!string.IsNullOrEmpty(LoadedProjectDir))
            {
                string fileName = Path.GetFileName(path);
                Directory.CreateDirectory(Path.Combine(LoadedProjectDir, "Assets"));
                string newPath = Path.Combine(LoadedProjectDir, "Assets", fileName);

                if (!File.Exists(newPath))
                    File.Copy(path, newPath);

                return newPath;
            }
            return null;
        }

        public static Mesh LoadModel(string path)
        {
            AssimpContext importer = new AssimpContext();

            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate);

            List<VertexArray> vertexArrays = new List<VertexArray>();

            foreach (var mesh in scene.Meshes)
            {
                List<uint> indices = IntToUint(mesh.GetIndices()).ToList();
                List<float> vertices = new List<float>();
                var verts = mesh.Vertices;

                List <Vector3D> uvs = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0] : null;
                var material = scene.Materials[mesh.MaterialIndex];

                for (int i = 0; i < verts.Count; i++)
                {
                    Vector3D vert = verts[i];
                    Vector3D uv = (uvs != null) ? uvs[i] : new Vector3D(0, 0, 0);

                    vertices.Add(vert.X);
                    vertices.Add(vert.Y);
                    vertices.Add(vert.Z);

                    vertices.Add(material.ColorDiffuse.R);
                    vertices.Add(material.ColorDiffuse.G);
                    vertices.Add(material.ColorDiffuse.B);

                    vertices.Add(uv.X);
                    vertices.Add(1.0f - uv.Y);
                }

                VertexArray vertArray = new VertexArray(vertices.ToArray(), indices.ToArray(), new VertexAttrib[]
                {
                    new VertexAttrib { location = 0, size = 3 },
                    new VertexAttrib { location = 1, size = 3 },
                    new VertexAttrib { location = 2, size = 2 }
                });

                if (!string.IsNullOrEmpty(material.TextureDiffuse.FilePath))
                    vertArray.SetTexture(new Texture().FromFile(material.TextureDiffuse.FilePath));

                vertexArrays.Add(vertArray);
            }

            return new Mesh(vertexArrays.ToArray()); 
        }

        private static byte[] TexelsToBytes(Texel[] texels)
        {
            List<byte> bytes = new List<byte>();

            foreach (var texel in texels) 
            {
                bytes.Add(texel.R);
                bytes.Add(texel.G);
                bytes.Add(texel.B);
                bytes.Add(texel.A);
            }

            return bytes.ToArray();
        }

        private static uint[] IntToUint(int[] ints)
        {
            uint[] uints = new uint[ints.Length];

            for (int i = 0; i < ints.Length; i++)
            {
                uints[i] = (uint)ints[i];
            }

            return uints;
        }
    }
}
