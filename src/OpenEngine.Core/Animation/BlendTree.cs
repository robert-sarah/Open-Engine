// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Animation
{
    public enum BlendTreeType { Simple1D, SimpleDirectional2D, FreeformDirectional2D, FreeformCartesian2D, Direct }

    public class BlendTree
    {
        public string Name { get; set; }
        public BlendTreeType Type { get; set; }
        public BlendParameter Parameter { get; set; }
        public BlendParameter ParameterY { get; set; }
        public List<BlendTreeChild> Children { get; set; }
        public float Threshold { get; set; }
        public float NormalizeBlendValues { get; set; }

        public BlendTree()
        {
            Children = new List<BlendTreeChild>();
            Threshold = 0.01f;
            NormalizeBlendValues = 1f;
        }

        public void AddChild(AnimationClip clip, float position, float positionY = 0f)
        {
            Children.Add(new BlendTreeChild
            {
                Clip = clip,
                Position = position,
                PositionY = positionY,
                Threshold = 0.1f
            });
        }

        public void AddChild(BlendTree childTree, float position, float positionY = 0f)
        {
            Children.Add(new BlendTreeChild
            {
                ChildTree = childTree,
                Position = position,
                PositionY = positionY,
                Threshold = 0.1f
            });
        }

        public AnimationState Evaluate(float parameterValue, float parameterValueY = 0f)
        {
            if (Children.Count == 0) return null;

            switch (Type)
            {
                case BlendTreeType.Simple1D:
                    return Evaluate1D(parameterValue);
                case BlendTreeType.SimpleDirectional2D:
                case BlendTreeType.FreeformDirectional2D:
                    return Evaluate2DDirectional(parameterValue, parameterValueY);
                case BlendTreeType.FreeformCartesian2D:
                    return Evaluate2DCartesian(parameterValue, parameterValueY);
                case BlendTreeType.Direct:
                    return EvaluateDirect(parameterValue);
                default:
                    return Children[0].Clip?.States.Count > 0 ? Children[0].Clip.States[0] : null;
            }
        }

        private AnimationState Evaluate1D(float value)
        {
            // Find the two closest children
            Children.Sort((a, b) => a.Position.CompareTo(b.Position));

            for (int i = 0; i < Children.Count - 1; i++)
            {
                if (value >= Children[i].Position && value <= Children[i + 1].Position)
                {
                    float t = (value - Children[i].Position) / (Children[i + 1].Position - Children[i].Position);
                    
                    if (Children[i].Clip != null && Children[i + 1].Clip != null)
                    {
                        return BlendStates(Children[i].Clip.States[0], Children[i + 1].Clip.States[0], t);
                    }
                }
            }

            return Children[0].Clip?.States.Count > 0 ? Children[0].Clip.States[0] : null;
        }

        private AnimationState Evaluate2DDirectional(float x, float y)
        {
            float angle = (float)System.Math.Atan2(y, x);
            float magnitude = (float)System.Math.Sqrt(x * x + y * y);

            BlendTreeChild closest = null;
            float closestDist = float.MaxValue;

            foreach (var child in Children)
            {
                float childAngle = (float)System.Math.Atan2(child.PositionY, child.Position);
                float angleDiff = System.Math.Abs(angle - childAngle);
                if (angleDiff > System.Math.PI) angleDiff = 2 * (float)System.Math.PI - angleDiff;

                if (angleDiff < closestDist)
                {
                    closestDist = angleDiff;
                    closest = child;
                }
            }

            return closest?.Clip?.States.Count > 0 ? closest.Clip.States[0] : null;
        }

        private AnimationState Evaluate2DCartesian(float x, float y)
        {
            BlendTreeChild closest = null;
            float closestDist = float.MaxValue;

            foreach (var child in Children)
            {
                float dx = x - child.Position;
                float dy = y - child.PositionY;
                float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = child;
                }
            }

            return closest?.Clip?.States.Count > 0 ? closest.Clip.States[0] : null;
        }

        private AnimationState EvaluateDirect(float value)
        {
            foreach (var child in Children)
            {
                if (System.Math.Abs(value - child.Position) < child.Threshold)
                {
                    return child.Clip?.States.Count > 0 ? child.Clip.States[0] : null;
                }
            }
            return Children[0].Clip?.States.Count > 0 ? Children[0].Clip.States[0] : null;
        }

        private AnimationState BlendStates(AnimationState a, AnimationState b, float t)
        {
            if (a == null) return b;
            if (b == null) return a;

            return new AnimationState
            {
                Name = a.Name + "_Blended",
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Rotation = Quaternion.Slerp(a.Rotation, b.Rotation, t),
                Scale = Vector3.Lerp(a.Scale, b.Scale, t)
            };
        }
    }

    public class BlendTreeChild
    {
        public AnimationClip Clip { get; set; }
        public BlendTree ChildTree { get; set; }
        public float Position { get; set; }
        public float PositionY { get; set; }
        public float Threshold { get; set; }
        public float TimeScale { get; set; }
    }

    public class BlendParameter
    {
        public string Name { get; set; }
        public float Value { get; set; }
        public BlendParameterType Type { get; set; }

        public BlendParameter(string name, BlendParameterType type = BlendParameterType.Float)
        {
            Name = name;
            Type = type;
            Value = 0f;
        }
    }

    public enum BlendParameterType { Float, Int, Bool, Trigger }

    public static class QuaternionExtensions
    {
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
        {
            t = System.Math.Clamp(t, 0f, 1f);
            
            float dot = a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
            
            if (dot < 0f)
            {
                b = new Quaternion(-b.X, -b.Y, -b.Z, -b.W);
                dot = -dot;
            }

            if (dot > 0.9995f)
            {
                return new Quaternion(
                    a.X + t * (b.X - a.X),
                    a.Y + t * (b.Y - a.Y),
                    a.Z + t * (b.Z - a.Z),
                    a.W + t * (b.W - a.W)
                ).Normalized;
            }

            float theta0 = (float)System.Math.Acos(dot);
            float theta = theta0 * t;
            float sinTheta = (float)System.Math.Sin(theta);
            float sinTheta0 = (float)System.Math.Sin(theta0);

            float s0 = (float)System.Math.Cos(theta) - dot * sinTheta / sinTheta0;
            float s1 = sinTheta / sinTheta0;

            return new Quaternion(
                s0 * a.X + s1 * b.X,
                s0 * a.Y + s1 * b.Y,
                s0 * a.Z + s1 * b.Z,
                s0 * a.W + s1 * b.W
            );
        }
    }
}
