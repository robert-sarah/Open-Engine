// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Physics
{
    public enum ColliderType
    {
        Box,
        Sphere,
        Capsule,
        Mesh,
        Terrain,
        Wheel
    }

    public class Collider
    {
        public string EntityId { get; set; }
        public ColliderType Type { get; set; }
        public bool IsTrigger { get; set; }
        public Vector3 Center { get; set; }
        public bool Enabled { get; set; }
        public PhysicMaterial Material { get; set; }

        public Collider(string entityId, ColliderType type)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            Type = type;
            IsTrigger = false;
            Center = Vector3.Zero;
            Enabled = true;
            Material = new PhysicMaterial();
        }

        public virtual bool Raycast(Ray ray, out RaycastHit hit) { hit = default; return false; }
        public virtual bool OverlapPoint(Vector3 point) => false;
        public virtual bool OverlapSphere(Vector3 position, float radius) => false;
    }

    public class BoxCollider : Collider
    {
        public Vector3 Size { get; set; }

        public BoxCollider(string entityId) : base(entityId, ColliderType.Box)
        {
            Size = Vector3.One;
        }
    }

    public class SphereCollider : Collider
    {
        public float Radius { get; set; }

        public SphereCollider(string entityId) : base(entityId, ColliderType.Sphere)
        {
            Radius = 0.5f;
        }

        public override bool OverlapSphere(Vector3 position, float radius)
        {
            return Vector3.Distance(position, Center) < (Radius + radius);
        }
    }

    public class CapsuleCollider : Collider
    {
        public float Radius { get; set; }
        public float Height { get; set; }
        public int Direction { get; set; } // 0=X, 1=Y, 2=Z

        public CapsuleCollider(string entityId) : base(entityId, ColliderType.Capsule)
        {
            Radius = 0.5f;
            Height = 2f;
            Direction = 1;
        }
    }

    public class PhysicMaterial
    {
        public float DynamicFriction { get; set; }
        public float StaticFriction { get; set; }
        public float Bounciness { get; set; }
        public float FrictionCombine { get; set; }
        public float BounceCombine { get; set; }

        public PhysicMaterial()
        {
            DynamicFriction = 0.6f;
            StaticFriction = 0.6f;
            Bounciness = 0f;
            FrictionCombine = 2f;
            BounceCombine = 2f;
        }
    }

    public struct RaycastHit
    {
        public Vector3 Point;
        public Vector3 Normal;
        public float Distance;
        public string ColliderEntityId;
        public int TriangleIndex;
    }

    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction.Normalized();
        }

        public Vector3 GetPoint(float distance) => Origin + Direction * distance;
    }

    public static class Time
    {
        public static float deltaTime { get; set; } = 0.016f;
        public static float fixedDeltaTime { get; set; } = 0.02f;
        public static float time { get; set; }
    }
}
