// Created By Levi Enama
using System;

namespace OpenEngine.Core.Math
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vector3 Zero => new Vector3(0, 0, 0);
        public static Vector3 One => new Vector3(1, 1, 1);
        public static Vector3 Up => new Vector3(0, 1, 0);
        public static Vector3 Down => new Vector3(0, -1, 0);
        public static Vector3 Left => new Vector3(-1, 0, 0);
        public static Vector3 Right => new Vector3(1, 0, 0);
        public static Vector3 Forward => new Vector3(0, 0, 1);
        public static Vector3 Back => new Vector3(0, 0, -1);

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float Magnitude => (float)System.Math.Sqrt(X * X + Y * Y + Z * Z);
        public float SqrMagnitude => X * X + Y * Y + Z * Z;

        public Vector3 Normalized
        {
            get
            {
                float mag = Magnitude;
                if (mag < float.Epsilon)
                    return Zero;
                return new Vector3(X / mag, Y / mag, Z / mag);
            }
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return (float)System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
            => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3 operator -(Vector3 a, Vector3 b)
            => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vector3 operator -(Vector3 a)
            => new Vector3(-a.X, -a.Y, -a.Z);

        public static Vector3 operator *(Vector3 a, float scalar)
            => new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);

        public static Vector3 operator /(Vector3 a, float scalar)
        {
            if (System.Math.Abs(scalar) < float.Epsilon)
                throw new DivideByZeroException();
            return new Vector3(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }

        public static bool operator ==(Vector3 a, Vector3 b)
            => a.Equals(b);

        public static bool operator !=(Vector3 a, Vector3 b)
            => !a.Equals(b);

        public bool Equals(Vector3 other)
        {
            return System.Math.Abs(X - other.X) < float.Epsilon
                && System.Math.Abs(Y - other.Y) < float.Epsilon
                && System.Math.Abs(Z - other.Z) < float.Epsilon;
        }

        public override bool Equals(object? obj)
            => obj is Vector3 other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(X, Y, Z);

        public override string ToString()
            => $"({X:F2}, {Y:F2}, {Z:F2})";

        public Vector3 Lerp(Vector3 target, float t)
        {
            t = System.Math.Clamp(t, 0, 1);
            return new Vector3(
                X + (target.X - X) * t,
                Y + (target.Y - Y) * t,
                Z + (target.Z - Z) * t
            );
        }
    }
}
