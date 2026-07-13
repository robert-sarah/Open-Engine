// Created By Levi Enama
// Animation Loader for glTF Animations
using System;
using System.Collections.Generic;
using System.Numerics;
using SharpGLTF.Schema2;

namespace OpenEngine.Core.Assets
{
    public class AnimationClip
    {
        public string Name { get; set; }
        public float Duration { get; set; }
        public List<AnimationChannel> Channels { get; set; }
        public Dictionary<string, AnimationSampler> Samplers { get; set; }

        public AnimationClip()
        {
            Channels = new List<AnimationChannel>();
            Samplers = new Dictionary<string, AnimationSampler>();
        }
    }

    public class AnimationChannel
    {
        public string TargetNode { get; set; }
        public AnimationChannelType Type { get; set; }
        public AnimationSampler Sampler { get; set; }
    }

    public enum AnimationChannelType
    {
        Translation,
        Rotation,
        Scale,
        Weights
    }

    public class AnimationSampler
    {
        public string Name { get; set; }
        public List<float> Input { get; set; } // Time values
        public List<Vector3> OutputVector3 { get; set; }
        public List<Quaternion> OutputQuaternion { get; set; }
        public AnimationSamplerInterpolation Interpolation { get; set; }
    }

    public enum AnimationSamplerInterpolation
    {
        Linear,
        Step,
        CubicSpline
    }

    public class AnimationLoader
    {
        public static AnimationClip LoadAnimation(SharpGLTF.Schema2.Animation gltfAnimation, ModelRoot model)
        {
            var clip = new AnimationClip
            {
                Name = gltfAnimation.Name ?? "UnnamedAnimation"
            };

            // Load samplers
            foreach (var sampler in gltfAnimation.Samplers)
            {
                var animationSampler = LoadSampler(sampler, model);
                clip.Samplers[sampler.LogicalIndex.ToString()] = animationSampler;
            }

            // Load channels
            foreach (var channel in gltfAnimation.Channels)
            {
                var animationChannel = LoadChannel(channel, model, clip.Samplers);
                clip.Channels.Add(animationChannel);
            }

            // Calculate duration
            clip.Duration = CalculateDuration(clip);

            return clip;
        }

        private static AnimationSampler LoadSampler(AnimationSamplerDef sampler, ModelRoot model)
        {
            var animationSampler = new AnimationSampler
            {
                Name = sampler.LogicalIndex.ToString(),
                Interpolation = ParseInterpolation(sampler.Interpolation)
            };

            // Load input (time values)
            var inputAccessor = model.LogicalAccessors[sampler.InputAccessor];
            animationSampler.Input = new List<float>(inputAccessor.Count);
            
            for (int i = 0; i < inputAccessor.Count; i++)
            {
                animationSampler.Input.Add(inputAccessor.AsSingleArray()[i]);
            }

            // Load output (values)
            var outputAccessor = model.LogicalAccessors[sampler.OutputAccessor];
            
            if (outputAccessor.Type == SharpGLTF.Schema2.AccessorType.VEC3)
            {
                animationSampler.OutputVector3 = new List<Vector3>(outputAccessor.Count);
                var values = outputAccessor.AsVector3Array();
                
                for (int i = 0; i < outputAccessor.Count; i++)
                {
                    animationSampler.OutputVector3.Add(values[i]);
                }
            }
            else if (outputAccessor.Type == SharpGLTF.Schema2.AccessorType.VEC4)
            {
                animationSampler.OutputQuaternion = new List<Quaternion>(outputAccessor.Count);
                var values = outputAccessor.AsVector4Array();
                
                for (int i = 0; i < outputAccessor.Count; i++)
                {
                    animationSampler.OutputQuaternion.Add(new Quaternion(values[i].X, values[i].Y, values[i].Z, values[i].W));
                }
            }

            return animationSampler;
        }

        private static AnimationChannel LoadChannel(AnimationChannelDef channel, ModelRoot model, Dictionary<string, AnimationSampler> samplers)
        {
            var animationChannel = new AnimationChannel
            {
                TargetNode = model.LogicalNodes[channel.TargetNode].Name,
                Type = ParseChannelType(channel.TargetPath),
                Sampler = samplers[channel.Sampler.LogicalIndex.ToString()]
            };

            return animationChannel;
        }

        private static AnimationSamplerInterpolation ParseInterpolation(string interpolation)
        {
            return interpolation switch
            {
                "LINEAR" => AnimationSamplerInterpolation.Linear,
                "STEP" => AnimationSamplerInterpolation.Step,
                "CUBICSPLINE" => AnimationSamplerInterpolation.CubicSpline,
                _ => AnimationSamplerInterpolation.Linear
            };
        }

        private static AnimationChannelType ParseChannelType(string path)
        {
            return path switch
            {
                "translation" => AnimationChannelType.Translation,
                "rotation" => AnimationChannelType.Rotation,
                "scale" => AnimationChannelType.Scale,
                "weights" => AnimationChannelType.Weights,
                _ => AnimationChannelType.Translation
            };
        }

        private static float CalculateDuration(AnimationClip clip)
        {
            float maxTime = 0f;
            
            foreach (var channel in clip.Channels)
            {
                if (channel.Sampler?.Input != null && channel.Sampler.Input.Count > 0)
                {
                    maxTime = Math.Max(maxTime, channel.Sampler.Input[^1]);
                }
            }

            return maxTime;
        }

        public static Vector3 SampleVector3(AnimationSampler sampler, float time)
        {
            if (sampler.OutputVector3 == null || sampler.OutputVector3.Count == 0)
                return Vector3.Zero;

            if (sampler.Input.Count == 1)
                return sampler.OutputVector3[0];

            // Find the two keyframes surrounding the current time
            int index = 0;
            while (index < sampler.Input.Count - 1 && sampler.Input[index + 1] < time)
            {
                index++;
            }

            if (index >= sampler.Input.Count - 1)
                return sampler.OutputVector3[^1];

            float t0 = sampler.Input[index];
            float t1 = sampler.Input[index + 1];
            float alpha = (time - t0) / (t1 - t0);

            if (sampler.Interpolation == AnimationSamplerInterpolation.Step)
            {
                return sampler.OutputVector3[index];
            }

            // Linear interpolation
            return Vector3.Lerp(sampler.OutputVector3[index], sampler.OutputVector3[index + 1], alpha);
        }

        public static Quaternion SampleQuaternion(AnimationSampler sampler, float time)
        {
            if (sampler.OutputQuaternion == null || sampler.OutputQuaternion.Count == 0)
                return Quaternion.Identity;

            if (sampler.Input.Count == 1)
                return sampler.OutputQuaternion[0];

            // Find the two keyframes surrounding the current time
            int index = 0;
            while (index < sampler.Input.Count - 1 && sampler.Input[index + 1] < time)
            {
                index++;
            }

            if (index >= sampler.Input.Count - 1)
                return sampler.OutputQuaternion[^1];

            float t0 = sampler.Input[index];
            float t1 = sampler.Input[index + 1];
            float alpha = (time - t0) / (t1 - t0);

            if (sampler.Interpolation == AnimationSamplerInterpolation.Step)
            {
                return sampler.OutputQuaternion[index];
            }

            // Spherical linear interpolation
            return Quaternion.Slerp(sampler.OutputQuaternion[index], sampler.OutputQuaternion[index + 1], alpha);
        }
    }
}
