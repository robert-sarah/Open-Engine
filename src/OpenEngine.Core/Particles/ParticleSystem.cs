// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Particles
{
    public enum ParticleSystemShapeType { Box, Sphere, Hemisphere, Cone, Circle, Edge }
    public enum ParticleSystemSimulationSpace { Local, World }
    public enum ParticleSystemScalingMode { Local, Hierarchy, Shape }
    public enum ParticleSystemRenderMode { Billboard, Stretched, Mesh }

    public class ParticleSystem
    {
        public string EntityId { get; set; }
        public int MaxParticles { get; set; }
        public float Duration { get; set; }
        public bool Looping { get; set; }
        public bool Prewarm { get; set; }
        public float StartDelay { get; set; }
        public float StartLifetime { get; set; }
        public float StartSpeed { get; set; }
        public float StartSize { get; set; }
        public float StartRotation { get; set; }
        public float StartColor { get; set; }
        public float GravityModifier { get; set; }
        public int MaxEmissionRate { get; set; }
        public ParticleSystemSimulationSpace SimulationSpace { get; set; }
        public ParticleSystemScalingMode ScalingMode { get; set; }
        public bool PlayOnAwake { get; set; }
        public bool EmissionEnabled { get; set; }
        public ParticleSystemShapeType ShapeType { get; set; }
        public ParticleSystemRenderMode RenderMode { get; set; }
        public Material Material { get; set; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }

        private List<Particle> _particles;
        private float _elapsedTime;
        private float _emissionAccumulator;

        public ParticleSystem(string entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            MaxParticles = 1000;
            Duration = 5f;
            Looping = true;
            Prewarm = false;
            StartDelay = 0f;
            StartLifetime = 5f;
            StartSpeed = 5f;
            StartSize = 1f;
            StartRotation = 0f;
            StartColor = 1f;
            GravityModifier = 0f;
            MaxEmissionRate = 50;
            SimulationSpace = ParticleSystemSimulationSpace.Local;
            ScalingMode = ParticleSystemScalingMode.Local;
            PlayOnAwake = true;
            EmissionEnabled = true;
            ShapeType = ParticleSystemShapeType.Cone;
            RenderMode = ParticleSystemRenderMode.Billboard;
            _particles = new List<Particle>();
            _elapsedTime = 0f;
            _emissionAccumulator = 0f;
        }

        public void Play()
        {
            IsPlaying = true;
            IsPaused = false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Stop()
        {
            IsPlaying = false;
            IsPaused = false;
            _particles.Clear();
            _elapsedTime = 0f;
        }

        public void Clear()
        {
            _particles.Clear();
        }

        public void Emit(int count)
        {
            for (int i = 0; i < count && _particles.Count < MaxParticles; i++)
            {
                var particle = CreateParticle();
                _particles.Add(particle);
            }
        }

        private Particle CreateParticle()
        {
            return new Particle
            {
                Position = Vector3.Zero,
                Velocity = GetEmissionDirection() * StartSpeed,
                Lifetime = StartLifetime,
                Age = 0f,
                Size = StartSize,
                Rotation = StartRotation,
                Color = StartColor
            };
        }

        private Vector3 GetEmissionDirection()
        {
            return ShapeType switch
            {
                ParticleSystemShapeType.Sphere => UnityEngine.Random.onUnitSphere,
                ParticleSystemShapeType.Cone => new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 1f, UnityEngine.Random.Range(-0.5f, 0.5f)).Normalized(),
                _ => Vector3.Up
            };
        }

        public void Update(float deltaTime)
        {
            if (!IsPlaying || IsPaused)
                return;

            _elapsedTime += deltaTime;

            if (_elapsedTime < StartDelay)
                return;

            if (EmissionEnabled)
            {
                var emissionRate = MaxEmissionRate / Duration;
                _emissionAccumulator += emissionRate * deltaTime;
                var emitCount = (int)_emissionAccumulator;
                if (emitCount > 0)
                {
                    Emit(emitCount);
                    _emissionAccumulator -= emitCount;
                }
            }

            UpdateParticles(deltaTime);

            if (!Looping && _elapsedTime > Duration && _particles.Count == 0)
            {
                Stop();
            }
        }

        private void UpdateParticles(float deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                particle.Age += deltaTime;

                if (particle.Age >= particle.Lifetime)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                particle.Velocity.Y -= GravityModifier * deltaTime;
                particle.Position += particle.Velocity * deltaTime;
                particle.Rotation += deltaTime * 90f;

                _particles[i] = particle;
            }
        }

        public int ParticleCount => _particles.Count;
    }

    public struct Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Lifetime;
        public float Age;
        public float Size;
        public float Rotation;
        public float Color;
    }

    public static class UnityEngine
    {
        public static class Random
        {
            private static System.Random _random = new System.Random();

            public static float Range(float min, float max)
            {
                return (float)_random.NextDouble() * (max - min) + min;
            }

            public static Vector3 onUnitSphere
            {
                get
                {
                    var theta = Range(0f, Mathf.PI * 2f);
                    var phi = Range(0f, Mathf.PI);
                    return new Vector3(
                        (float)Math.Sin(phi) * (float)Math.Cos(theta),
                        (float)Math.Cos(phi),
                        (float)Math.Sin(phi) * (float)Math.Sin(theta)
                    );
                }
            }
        }
    }

    public static class Mathf
    {
        public const float PI = 3.14159265f;
    }
}
