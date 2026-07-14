using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Rendering
{
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
