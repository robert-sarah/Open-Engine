// Created By Levi Enama
using System;

namespace OpenEngine.Core.Math
{
    public struct Color : IEquatable<Color>
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public static Color White => new Color(1, 1, 1, 1);
        public static Color Black => new Color(0, 0, 0, 1);
        public static Color Red => new Color(1, 0, 0, 1);
        public static Color Green => new Color(0, 1, 0, 1);
        public static Color Blue => new Color(0, 0, 1, 1);
        public static Color Yellow => new Color(1, 1, 0, 1);
        public static Color Cyan => new Color(0, 1, 1, 1);
        public static Color Magenta => new Color(1, 0, 1, 1);
        public static Color Gray => new Color(0.5f, 0.5f, 0.5f, 1);
        public static Color Clear => new Color(0, 0, 0, 0);

        public Color(float r, float g, float b, float a = 1f)
        {
            R = System.Math.Clamp(r, 0f, 1f);
            G = System.Math.Clamp(g, 0f, 1f);
            B = System.Math.Clamp(b, 0f, 1f);
            A = System.Math.Clamp(a, 0f, 1f);
        }

        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            A = a / 255f;
        }

        public Color(int r, int g, int b, int a = 255)
        {
            R = System.Math.Clamp(r / 255f, 0f, 1f);
            G = System.Math.Clamp(g / 255f, 0f, 1f);
            B = System.Math.Clamp(b / 255f, 0f, 1f);
            A = System.Math.Clamp(a / 255f, 0f, 1f);
        }

        public static Color Lerp(Color a, Color b, float t)
        {
            t = System.Math.Clamp(t, 0f, 1f);
            return new Color(
                a.R + (b.R - a.R) * t,
                a.G + (b.G - a.G) * t,
                a.B + (b.B - a.B) * t,
                a.A + (b.A - a.A) * t
            );
        }

        public static Color operator +(Color a, Color b)
            => new Color(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);

        public static Color operator -(Color a, Color b)
            => new Color(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);

        public static Color operator *(Color a, Color b)
            => new Color(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);

        public static Color operator *(Color a, float scalar)
            => new Color(a.R * scalar, a.G * scalar, a.B * scalar, a.A);

        public static bool operator ==(Color a, Color b)
            => a.Equals(b);

        public static bool operator !=(Color a, Color b)
            => !a.Equals(b);

        public bool Equals(Color other)
        {
            return System.Math.Abs(R - other.R) < float.Epsilon
                && System.Math.Abs(G - other.G) < float.Epsilon
                && System.Math.Abs(B - other.B) < float.Epsilon
                && System.Math.Abs(A - other.A) < float.Epsilon;
        }

        public override bool Equals(object? obj)
            => obj is Color other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(R, G, B, A);

        public override string ToString()
            => $"RGBA({R:F2}, {G:F2}, {B:F2}, {A:F2})";

        public uint ToRGBA32()
        {
            return ((uint)(A * 255) << 24) |
                   ((uint)(R * 255) << 16) |
                   ((uint)(G * 255) << 8) |
                   ((uint)(B * 255));
        }

        public uint ToARGB32()
        {
            return ((uint)(B * 255) << 16) |
                   ((uint)(G * 255) << 8) |
                   ((uint)(R * 255)) |
                   ((uint)(A * 255) << 24);
        }
    }
}
