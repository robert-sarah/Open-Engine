// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Particles
{
    public class ParticleModule
    {
        public bool Enabled { get; set; }
        public ParticleModuleType Type { get; set; }

        public ParticleModule(ParticleModuleType type)
        {
            Type = type;
            Enabled = false;
        }
    }

    public enum ParticleModuleType
    {
        Emission,
        Shape,
        VelocityOverLifetime,
        ColorOverLifetime,
        SizeOverLifetime,
        RotationOverLifetime,
        ExternalForces,
        Noise,
        Collision,
        Trigger,
        SubEmitters,
        TextureSheetAnimation,
        Trails,
        Lights
    }

    public class EmissionModule : ParticleModule
    {
        public float RateOverTime { get; set; }
        public MinMaxCurve RateOverDistance { get; set; }
        public int BurstsCount { get; set; }

        public EmissionModule() : base(ParticleModuleType.Emission)
        {
            RateOverTime = 10f;
            RateOverDistance = new MinMaxCurve();
            BurstsCount = 0;
        }
    }

    public class ShapeModule : ParticleModule
    {
        public ParticleSystemShapeType ShapeType { get; set; }
        public Vector3 Scale { get; set; }
        public float Angle { get; set; }
        public float Radius { get; set; }
        public Vector3 Position { get; set; }
        public bool RandomDirection { get; set; }

        public ShapeModule() : base(ParticleModuleType.Shape)
        {
            ShapeType = ParticleSystemShapeType.Cone;
            Scale = Vector3.One;
            Angle = 25f;
            Radius = 1f;
            Position = Vector3.Zero;
            RandomDirection = false;
        }
    }

    public class ColorOverLifetimeModule : ParticleModule
    {
        public Gradient ColorGradient { get; set; }

        public ColorOverLifetimeModule() : base(ParticleModuleType.ColorOverLifetime)
        {
            ColorGradient = new Gradient();
        }
    }

    public class SizeOverLifetimeModule : ParticleModule
    {
        public MinMaxCurve Size { get; set; }

        public SizeOverLifetimeModule() : base(ParticleModuleType.SizeOverLifetime)
        {
            Size = new MinMaxCurve();
        }
    }

    public class VelocityOverLifetimeModule : ParticleModule
    {
        public MinMaxCurve X { get; set; }
        public MinMaxCurve Y { get; set; }
        public MinMaxCurve Z { get; set; }
        public bool Space { get; set; }

        public VelocityOverLifetimeModule() : base(ParticleModuleType.VelocityOverLifetime)
        {
            X = new MinMaxCurve();
            Y = new MinMaxCurve();
            Z = new MinMaxCurve();
            Space = false;
        }
    }

    public class MinMaxCurve
    {
        public float Constant { get; set; }
        public float ConstantMin { get; set; }
        public float ConstantMax { get; set; }
        public AnimationCurve Curve { get; set; }
        public AnimationCurve CurveMin { get; set; }
        public AnimationCurve CurveMax { get; set; }

        public MinMaxCurve()
        {
            Constant = 1f;
            ConstantMin = 0f;
            ConstantMax = 1f;
            Curve = new AnimationCurve();
            CurveMin = new AnimationCurve();
            CurveMax = new AnimationCurve();
        }

        public float Evaluate(float time)
        {
            return Constant;
        }
    }

    public class Gradient
    {
        public List<GradientKey> Keys { get; set; }

        public Gradient()
        {
            Keys = new List<GradientKey>
            {
                new GradientKey(0f, Color.White),
                new GradientKey(1f, Color.White)
            };
        }

        public Color Evaluate(float time)
        {
            if (Keys.Count == 0) return Color.White;
            if (Keys.Count == 1) return Keys[0].Color;

            for (int i = 0; i < Keys.Count - 1; i++)
            {
                if (time >= Keys[i].Time && time <= Keys[i + 1].Time)
                {
                    var t = (time - Keys[i].Time) / (Keys[i + 1].Time - Keys[i].Time);
                    return LerpColor(Keys[i].Color, Keys[i + 1].Color, t);
                }
            }

            return Keys[Keys.Count - 1].Color;
        }

        private Color LerpColor(Color a, Color b, float t)
        {
            return new Color(
                a.R + (b.R - a.R) * t,
                a.G + (b.G - a.G) * t,
                a.B + (b.B - a.B) * t,
                a.A + (b.A - a.A) * t
            );
        }
    }

    public struct GradientKey
    {
        public float Time;
        public Color Color;

        public GradientKey(float time, Color color)
        {
            Time = time;
            Color = color;
        }
    }

    public struct Color
    {
        public float R, G, B, A;
        public Color(float r, float g, float b, float a = 1f) { R = r; G = g; B = b; A = a; }
        public static Color White => new Color(1, 1, 1, 1);
        public static Color Black => new Color(0, 0, 0, 1);
    }

    public class AnimationCurve
    {
        public AnimationCurve() { }
    }
}
