// Created By Levi Enama
using System;

namespace OpenEngine.Core.Math
{
    public class RectTransform
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 AnchorMin { get; set; }
        public Vector2 AnchorMax { get; set; }
        public Vector2 Pivot { get; set; }
        public Vector2 Rotation { get; set; }
        public Vector2 Scale { get; set; }

        public RectTransform()
        {
            Position = Vector2.Zero;
            Size = new Vector2(100, 100);
            AnchorMin = new Vector2(0.5f, 0.5f);
            AnchorMax = new Vector2(0.5f, 0.5f);
            Pivot = new Vector2(0.5f, 0.5f);
            Rotation = Vector2.Zero;
            Scale = Vector2.One;
        }

        public Vector2 GetWorldPosition(Vector2 parentSize)
        {
            return new Vector2(
                parentSize.X * AnchorMin.X + Position.X,
                parentSize.Y * AnchorMin.Y + Position.Y
            );
        }

        public Vector2 GetWorldSize(Vector2 parentSize)
        {
            return new Vector2(
                parentSize.X * (AnchorMax.X - AnchorMin.X) + Size.X,
                parentSize.Y * (AnchorMax.Y - AnchorMin.Y) + Size.Y
            );
        }
    }

    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
            => new Vector2(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator -(Vector2 a, Vector2 b)
            => new Vector2(a.X - b.X, a.Y - b.Y);

        public static Vector2 operator *(Vector2 a, float scalar)
            => new Vector2(a.X * scalar, a.Y * scalar);

        public static Vector2 operator /(Vector2 a, float scalar)
            => new Vector2(a.X / scalar, a.Y / scalar);

        public static bool operator ==(Vector2 a, Vector2 b)
            => a.Equals(b);

        public static bool operator !=(Vector2 a, Vector2 b)
            => !a.Equals(b);

        public bool Equals(Vector2 other)
        {
            return System.Math.Abs(X - other.X) < float.Epsilon
                && System.Math.Abs(Y - other.Y) < float.Epsilon;
        }

        public override bool Equals(object? obj)
            => obj is Vector2 other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(X, Y);

        public override string ToString()
            => $"({X:F2}, {Y:F2})";
    }
}
