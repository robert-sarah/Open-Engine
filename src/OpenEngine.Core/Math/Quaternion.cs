// Created By Levi Enama
using System;

namespace OpenEngine.Core.Math
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public static Quaternion Identity => new Quaternion(0, 0, 0, 1);

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float Magnitude => (float)System.Math.Sqrt(X * X + Y * Y + Z * Z + W * W);

        public Quaternion Normalized
        {
            get
            {
                float mag = Magnitude;
                if (mag < float.Epsilon)
                    return Identity;
                return new Quaternion(X / mag, Y / mag, Z / mag, W / mag);
            }
        }

        public static Quaternion Euler(float x, float y, float z)
        {
            float cx = (float)System.Math.Cos(x * 0.5f);
            float sx = (float)System.Math.Sin(x * 0.5f);
            float cy = (float)System.Math.Cos(y * 0.5f);
            float sy = (float)System.Math.Sin(y * 0.5f);
            float cz = (float)System.Math.Cos(z * 0.5f);
            float sz = (float)System.Math.Sin(z * 0.5f);

            return new Quaternion(
                sx * cy * cz + cx * sy * sz,
                cx * sy * cz - sx * cy * sz,
                cx * cy * sz + sx * sy * cz,
                cx * cy * cz - sx * sy * sz
            );
        }

        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            float halfAngle = angle * 0.5f;
            float s = (float)System.Math.Sin(halfAngle);
            return new Quaternion(axis.X * s, axis.Y * s, axis.Z * s, (float)System.Math.Cos(halfAngle));
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
                a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
                a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W,
                a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z
            );
        }

        public static Vector3 operator *(Quaternion q, Vector3 v)
        {
            float x = q.X * 2f;
            float y = q.Y * 2f;
            float z = q.Z * 2f;
            float xx = q.X * x;
            float yy = q.Y * y;
            float zz = q.Z * z;
            float xy = q.X * y;
            float xz = q.X * z;
            float yz = q.Y * z;
            float wx = q.W * x;
            float wy = q.W * y;
            float wz = q.W * z;

            return new Vector3(
                (1f - (yy + zz)) * v.X + (xy - wz) * v.Y + (xz + wy) * v.Z,
                (xy + wz) * v.X + (1f - (xx + zz)) * v.Y + (yz - wx) * v.Z,
                (xz - wy) * v.X + (yz + wx) * v.Y + (1f - (xx + yy)) * v.Z
            );
        }

        public static bool operator ==(Quaternion a, Quaternion b)
            => a.Equals(b);

        public static bool operator !=(Quaternion a, Quaternion b)
            => !a.Equals(b);

        public bool Equals(Quaternion other)
        {
            return System.Math.Abs(X - other.X) < float.Epsilon
                && System.Math.Abs(Y - other.Y) < float.Epsilon
                && System.Math.Abs(Z - other.Z) < float.Epsilon
                && System.Math.Abs(W - other.W) < float.Epsilon;
        }

        public override bool Equals(object? obj)
            => obj is Quaternion other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(X, Y, Z, W);

        public override string ToString()
            => $"({X:F2}, {Y:F2}, {Z:F2}, {W:F2})";
    }
}
