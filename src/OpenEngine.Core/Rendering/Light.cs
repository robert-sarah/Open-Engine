// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Rendering
{
    public enum LightType { Directional, Point, Spot, Area }
    public enum LightShadows { None, Hard, Soft }

    public class Light
    {
        public string EntityId { get; set; }
        public LightType Type { get; set; }
        public Color Color { get; set; }
        public float Intensity { get; set; }
        public float Range { get; set; }
        public float SpotAngle { get; set; }
        public float InnerSpotAngle { get; set; }
        public LightShadows Shadows { get; set; }
        public float ShadowStrength { get; set; }
        public float ShadowBias { get; set; }
        public float ShadowNormalBias { get; set; }
        public float ShadowNearPlane { get; set; }
        public bool BakeShadows { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public Light(string entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            Type = LightType.Point;
            Color = Color.White;
            Intensity = 1f;
            Range = 10f;
            SpotAngle = 30f;
            InnerSpotAngle = 20f;
            Shadows = LightShadows.Soft;
            ShadowStrength = 1f;
            ShadowBias = 0.05f;
            ShadowNormalBias = 0.4f;
            ShadowNearPlane = 0.2f;
            BakeShadows = false;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        public Vector3 GetDirection()
        {
            return Rotation * Vector3.Forward;
        }

        public void SetColor(Color color)
        {
            Color = color;
        }

        public void SetIntensity(float intensity)
        {
            Intensity = Math.Max(0f, intensity);
        }

        public void SetRange(float range)
        {
            Range = Math.Max(0f, range);
        }
    }
}
