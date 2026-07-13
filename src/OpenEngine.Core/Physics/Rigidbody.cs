// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Physics
{
    public enum RigidbodyType { Static, Kinematic, Dynamic }
    public enum CollisionDetectionMode { Discrete, Continuous, ContinuousDynamic }
    public enum ForceMode { Force, Impulse, VelocityChange, Acceleration }

    public class Rigidbody
    {
        public string EntityId { get; set; }
        public RigidbodyType Type { get; set; }
        public float Mass { get; set; }
        public float Drag { get; set; }
        public float AngularDrag { get; set; }
        public bool UseGravity { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 AngularVelocity { get; set; }
        public Vector3 CenterOfMass { get; set; }
        public CollisionDetectionMode CollisionDetection { get; set; }
        public bool IsSleeping { get; set; }
        public bool DetectCollisions { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public Rigidbody(string entityId)
        {
            EntityId = entityId;
            Type = RigidbodyType.Dynamic;
            Mass = 1f;
            Drag = 0f;
            AngularDrag = 0.05f;
            UseGravity = true;
            Velocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            CenterOfMass = Vector3.Zero;
            CollisionDetection = CollisionDetectionMode.Discrete;
            IsSleeping = false;
            DetectCollisions = true;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            if (Type == RigidbodyType.Static || Type == RigidbodyType.Kinematic) return;
            var acceleration = force / Mass;
            Velocity += acceleration * Time.fixedDeltaTime;
        }

        public void AddTorque(Vector3 torque, ForceMode mode = ForceMode.Force)
        {
            if (Type == RigidbodyType.Static || Type == RigidbodyType.Kinematic) return;
            var angularAcceleration = torque / Mass;
            AngularVelocity += angularAcceleration * Time.fixedDeltaTime;
        }

        public void SetVelocity(Vector3 velocity) => Velocity = velocity;
        public void MovePosition(Vector3 position) => Position = position;
        public void Sleep() { IsSleeping = true; Velocity = Vector3.Zero; }
        public void WakeUp() => IsSleeping = false;
    }
}
