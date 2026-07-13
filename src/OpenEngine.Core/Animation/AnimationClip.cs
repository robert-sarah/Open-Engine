// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Animation
{
    public class AnimationCurve
    {
        public List<Keyframe> Keys { get; set; }

        public AnimationCurve()
        {
            Keys = new List<Keyframe>();
        }

        public float Evaluate(float time)
        {
            if (Keys.Count == 0) return 0f;
            if (Keys.Count == 1) return Keys[0].Value;

            for (int i = 0; i < Keys.Count - 1; i++)
            {
                if (time >= Keys[i].Time && time <= Keys[i + 1].Time)
                {
                    var t = (time - Keys[i].Time) / (Keys[i + 1].Time - Keys[i].Time);
                    return Lerp(Keys[i].Value, Keys[i + 1].Value, t);
                }
            }

            return Keys[Keys.Count - 1].Value;
        }

        private float Lerp(float a, float b, float t) => a + (b - a) * t;
    }

    public struct Keyframe
    {
        public float Time;
        public float Value;
        public float InTangent;
        public float OutTangent;

        public Keyframe(float time, float value, float inTangent = 0f, float outTangent = 0f)
        {
            Time = time;
            Value = value;
            InTangent = inTangent;
            OutTangent = outTangent;
        }
    }

    public class AnimationClip
    {
        public string Name { get; set; }
        public float Length { get; set; }
        public float FrameRate { get; set; }
        public WrapMode WrapMode { get; set; }
        public Dictionary<string, AnimationCurve> Curves { get; set; }
        public Dictionary<string, List<TransformSnapshot>> BoneTransforms { get; set; }
        public bool Legacy { get; set; }

        public AnimationClip()
        {
            Length = 1f;
            FrameRate = 30f;
            WrapMode = WrapMode.Loop;
            Curves = new Dictionary<string, AnimationCurve>();
            BoneTransforms = new Dictionary<string, List<TransformSnapshot>>();
            Legacy = false;
        }

        public void SampleAnimation(float time, Dictionary<string, Transform> boneTransforms)
        {
            var normalizedTime = GetNormalizedTime(time);

            foreach (var curve in Curves)
            {
                var value = curve.Value.Evaluate(normalizedTime);
                // Apply value to transform property
            }
        }

        private float GetNormalizedTime(float time)
        {
            return WrapMode switch
            {
                WrapMode.Loop => time % Length,
                WrapMode.Clamp => Math.Clamp(time, 0f, Length),
                WrapMode.PingPong => Math.Abs((time % (Length * 2)) - Length),
                WrapMode.Once => Math.Clamp(time, 0f, Length),
                _ => time % Length
            };
        }
    }

    public enum WrapMode
    {
        Once,
        Loop,
        PingPong,
        ClampForever,
        Clamp
    }

    public struct TransformSnapshot
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public float Time;

        public TransformSnapshot(Vector3 position, Quaternion rotation, Vector3 scale, float time)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Time = time;
        }
    }

    public struct Transform
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public static Transform Identity => new Transform(Vector3.Zero, Quaternion.Identity, Vector3.One);
    }
}
