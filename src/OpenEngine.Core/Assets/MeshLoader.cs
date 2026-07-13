// Created By Levi Enama
// Mesh Loader for glTF Geometries and Materials
using System;
using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

namespace OpenEngine.Core.Assets
{
    public class MeshData
    {
        public string Name { get; set; }
        public List<VertexPositionNormalTexture1> Vertices { get; set; }
        public List<uint> Indices { get; set; }
        public MaterialData Material { get; set; }
        public Vector3 Center { get; set; }
        public float Radius { get; set; }

        public MeshData()
        {
            Vertices = new List<VertexPositionNormalTexture1>();
            Indices = new List<uint>();
            Material = new MaterialData();
        }
    }

    public class MaterialData
    {
        public string Name { get; set; }
        public Vector3 Albedo { get; set; }
        public float Metallic { get; set; }
        public float Roughness { get; set; }
        public float NormalScale { get; set; }
        public float OcclusionStrength { get; set; }
        public Vector3 Emissive { get; set; }
        public float AlphaCutoff { get; set; }
        public bool DoubleSided { get; set; }
        
        public string AlbedoTexture { get; set; }
        public string NormalTexture { get; set; }
        public string MetallicRoughnessTexture { get; set; }
        public string OcclusionTexture { get; set; }
        public string EmissiveTexture { get; set; }

        public MaterialData()
        {
            Albedo = Vector3.One;
            Metallic = 0.0f;
            Roughness = 1.0f;
            NormalScale = 1.0f;
            OcclusionStrength = 1.0f;
            Emissive = Vector3.Zero;
            AlphaCutoff = 0.5f;
            DoubleSided = false;
        }
    }

    public class MeshLoader
    {
        public static MeshData LoadMeshFromPrimitive(MeshPrimitive primitive, Material material)
        {
            var meshData = new MeshData
            {
                Name = primitive.Name ?? "UnnamedMesh"
            };

            // Load material
            if (material != null)
            {
                meshData.Material = LoadMaterial(material);
            }

            // Load vertices
            var positions = primitive.GetVertexAccessor("POSITION").AsVector3Array();
            var normals = primitive.GetVertexAccessor("NORMAL")?.AsVector3Array();
            var texCoords = primitive.GetVertexAccessor("TEXCOORD_0")?.AsVector2Array();

            int vertexCount = positions.Count;
            meshData.Vertices.Capacity = vertexCount;

            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = new VertexPositionNormalTexture1
                {
                    Position = positions[i],
                    Normal = normals != null && i < normals.Count ? normals[i] : Vector3.UnitY,
                    Texture0 = texCoords != null && i < texCoords.Count ? texCoords[i] : Vector2.Zero
                };
                meshData.Vertices.Add(vertex);
            }

            // Load indices
            if (primitive.TryGetIndicesAccessor(out var indicesAccessor))
            {
                var indices = indicesAccessor.AsUInt16Array();
                meshData.Indices.Capacity = indices.Count;
                
                foreach (var index in indices)
                {
                    meshData.Indices.Add(index);
                }
            }
            else
            {
                // Generate indices if not present
                for (int i = 0; i < vertexCount; i++)
                {
                    meshData.Indices.Add((uint)i);
                }
            }

            // Calculate bounding sphere
            CalculateBoundingSphere(meshData);

            return meshData;
        }

        public static MaterialData LoadMaterial(Material material)
        {
            var materialData = new MaterialData
            {
                Name = material.Name ?? "UnnamedMaterial"
            };

            if (material.FindChannel("BaseColor") != null)
            {
                var baseColor = material.FindChannel("BaseColor").Color;
                materialData.Albedo = new Vector3(baseColor.X, baseColor.Y, baseColor.Z);
            }

            if (material.FindChannel("MetallicRoughness") != null)
            {
                var mr = material.FindChannel("MetallicRoughness").Color;
                materialData.Metallic = mr.X;
                materialData.Roughness = mr.Y;
            }

            if (material.FindChannel("Normal") != null)
            {
                materialData.NormalTexture = material.FindChannel("Normal").Texture?.LogicalParent?.LogicalParent?.Name ?? "";
            }

            if (material.FindChannel("Occlusion") != null)
            {
                materialData.OcclusionTexture = material.FindChannel("Occlusion").Texture?.LogicalParent?.LogicalParent?.Name ?? "";
            }

            if (material.FindChannel("Emissive") != null)
            {
                var emissive = material.FindChannel("Emissive").Color;
                materialData.Emissive = new Vector3(emissive.X, emissive.Y, emissive.Z);
                materialData.EmissiveTexture = material.FindChannel("Emissive").Texture?.LogicalParent?.LogicalParent?.Name ?? "";
            }

            materialData.DoubleSided = material.DoubleSided;
            materialData.AlphaCutoff = material.AlphaCutoff;

            return materialData;
        }

        private static void CalculateBoundingSphere(MeshData meshData)
        {
            if (meshData.Vertices.Count == 0)
            {
                meshData.Center = Vector3.Zero;
                meshData.Radius = 0;
                return;
            }

            Vector3 min = meshData.Vertices[0].Position;
            Vector3 max = meshData.Vertices[0].Position;

            foreach (var vertex in meshData.Vertices)
            {
                min = Vector3.Min(min, vertex.Position);
                max = Vector3.Max(max, vertex.Position);
            }

            meshData.Center = (min + max) * 0.5f;
            meshData.Radius = Vector3.Distance(min, max) * 0.5f;
        }
    }
}
