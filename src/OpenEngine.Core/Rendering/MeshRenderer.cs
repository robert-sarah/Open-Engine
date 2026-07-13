// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Rendering
{
    public class MeshRenderer
    {
        public string EntityId { get; set; }
        public Material Material { get; set; }
        public Mesh Mesh { get; set; }
        public bool CastShadows { get; set; }
        public bool ReceiveShadows { get; set; }
        public int LightmapIndex { get; set; }
        public int RealtimeLightmapIndex { get; set; }
        public bool EnableInstancing { get; set; }
        public MotionVectorGenerationMode MotionVectors { get; set; }
        public bool AllowOcclusionWhenDynamic { get; set; }

        public MeshRenderer(string entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            CastShadows = true;
            ReceiveShadows = true;
            LightmapIndex = -1;
            RealtimeLightmapIndex = -1;
            EnableInstancing = false;
            MotionVectors = MotionVectorGenerationMode.Camera;
            AllowOcclusionWhenDynamic = false;
        }
    }

    public class Mesh
    {
        public string Name { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<int> Triangles { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector2> UV { get; set; }
        public List<Vector4> Tangents { get; set; }
        public List<Color> Colors { get; set; }
        public List<BoneWeight> BoneWeights { get; set; }
        public List<BlendShape> BlendShapes { get; set; }
        public Bounds Bounds { get; set; }

        public Mesh()
        {
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            Normals = new List<Vector3>();
            UV = new List<Vector2>();
            Tangents = new List<Vector4>();
            Colors = new List<Color>();
            BoneWeights = new List<BoneWeight>();
            BlendShapes = new List<BlendShape>();
            Bounds = new Bounds();
        }

        public void RecalculateBounds()
        {
            if (Vertices.Count == 0) return;

            var min = Vertices[0];
            var max = Vertices[0];

            foreach (var vertex in Vertices)
            {
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            Bounds = new Bounds((min + max) * 0.5f, max - min);
        }

        public void RecalculateNormals()
        {
            Normals.Clear();
            for (int i = 0; i < Vertices.Count; i++)
            {
                Normals.Add(Vector3.Up);
            }
        }
    }

    public struct BoneWeight
    {
        public int BoneIndex0, BoneIndex1, BoneIndex2, BoneIndex3;
        public float Weight0, Weight1, Weight2, Weight3;
    }

    public class BlendShape
    {
        public string Name { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector3> Tangents { get; set; }

        public BlendShape()
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Tangents = new List<Vector3>();
        }
    }

    public enum MotionVectorGenerationMode { Camera, Object, ForceNoMotionVectors }

    public struct Bounds
    {
        public Vector3 Center;
        public Vector3 Size;

        public Bounds(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }

        public bool Contains(Vector3 point)
        {
            var halfSize = Size * 0.5f;
            var min = Center - halfSize;
            var max = Center + halfSize;
            return point.X >= min.X && point.X <= max.X &&
                   point.Y >= min.Y && point.Y <= max.Y &&
                   point.Z >= min.Z && point.Z <= max.Z;
        }
    }

    public struct Vector2
    {
        public float X, Y;
        public Vector2(float x, float y) { X = x; Y = y; }
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);
    }
}
