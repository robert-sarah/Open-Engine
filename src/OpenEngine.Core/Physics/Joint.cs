// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Physics
{
    public enum JointType
    {
        Fixed,
        Spring,
        Hinge,
        Slider,
        Character
    }

    public class Joint
    {
        public string Id { get; set; }
        public JointType Type { get; set; }
        public string ConnectedBodyId { get; set; }
        public Vector3 Anchor { get; set; }
        public Vector3 ConnectedAnchor { get; set; }
        public bool EnableCollision { get; set; }
        public bool EnablePreprocessing { get; set; }
        public float BreakForce { get; set; }
        public float BreakTorque { get; set; }

        public Joint(string id, JointType type)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Type = type;
            ConnectedBodyId = "";
            Anchor = Vector3.Zero;
            ConnectedAnchor = Vector3.Zero;
            EnableCollision = false;
            EnablePreprocessing = true;
            BreakForce = float.MaxValue;
            BreakTorque = float.MaxValue;
        }
    }

    public class HingeJoint : Joint
    {
        public Vector3 Axis { get; set; }
        public bool UseMotor { get; set; }
        public float MotorForce { get; set; }
        public float MotorSpeed { get; set; }
        public bool UseLimits { get; set; }
        public float MinLimit { get; set; }
        public float MaxLimit { get; set; }

        public HingeJoint(string id) : base(id, JointType.Hinge)
        {
            Axis = Vector3.Forward;
            UseMotor = false;
            MotorForce = 0f;
            MotorSpeed = 0f;
            UseLimits = false;
            MinLimit = 0f;
            MaxLimit = 90f;
        }
    }

    public class SpringJoint : Joint
    {
        public float Spring { get; set; }
        public float Damper { get; set; }
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public float Tolerance { get; set; }

        public SpringJoint(string id) : base(id, JointType.Spring)
        {
            Spring = 10f;
            Damper = 5f;
            MinDistance = 0f;
            MaxDistance = float.MaxValue;
            Tolerance = 0.01f;
        }
    }

    public class FixedJoint : Joint
    {
        public FixedJoint(string id) : base(id, JointType.Fixed) { }
    }
}
