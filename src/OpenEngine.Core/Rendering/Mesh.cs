using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Rendering
{
    public class Mesh
    {
        public string Name { get; set; }
        public List<Vector3> Vertices { get; set; }
        public List<int> Triangles { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Vector2> UVs { get; set; }
        public Bounds Bounds { get; set; }

        public Mesh()
        {
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            Normals = new List<Vector3>();
            UVs = new List<Vector2>();
            Bounds = new Bounds();
        }

        public void RecalculateNormals()
        {
            Normals.Clear();
            for (int i = 0; i < Vertices.Count; i++)
                Normals.Add(Vector3.Up);

            for (int i = 0; i < Triangles.Count; i += 3)
            {
                int i0 = Triangles[i];
                int i1 = Triangles[i + 1];
                int i2 = Triangles[i + 2];

                Vector3 normal = Vector3.Normalize(Vector3.Cross(
                    Vertices[i1] - Vertices[i0],
                    Vertices[i2] - Vertices[i0]
                ));

                Normals[i0] = Vector3.Normalize(Normals[i0] + normal);
                Normals[i1] = Vector3.Normalize(Normals[i1] + normal);
                Normals[i2] = Vector3.Normalize(Normals[i2] + normal);
            }
        }

        public void RecalculateBounds()
        {
            if (Vertices.Count == 0) return;

            Vector3 min = Vertices[0];
            Vector3 max = Vertices[0];

            for (int i = 1; i < Vertices.Count; i++)
            {
                min.X = Math.Min(min.X, Vertices[i].X);
                min.Y = Math.Min(min.Y, Vertices[i].Y);
                min.Z = Math.Min(min.Z, Vertices[i].Z);
                max.X = Math.Max(max.X, Vertices[i].X);
                max.Y = Math.Max(max.Y, Vertices[i].Y);
                max.Z = Math.Max(max.Z, Vertices[i].Z);
            }

            Bounds.Center = (min + max) * 0.5f;
            Bounds.Size = max - min;
        }
    }

    public struct Bounds
    {
        public Vector3 Center { get; set; }
        public Vector3 Size { get; set; }
        public Vector3 Extents => Size * 0.5f;
        public Vector3 Min => Center - Extents;
        public Vector3 Max => Center + Extents;

        public Bounds(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }

        public bool Contains(Vector3 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y &&
                   point.Z >= Min.Z && point.Z <= Max.Z;
        }
    }

    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);

        public Vector2(float x, float y) { X = x; Y = y; }
    }
}
