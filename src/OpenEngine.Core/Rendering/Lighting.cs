// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Rendering
{
    public enum LightType { Directional, Point, Spot, Area }
    public enum ShadowType { None, Hard, Soft, PCSS }
    public enum LightMode { Realtime, Mixed, Baked }

    public class Light
    {
        public string Name { get; set; }
        public LightType Type { get; set; }
        public Color Color { get; set; }
        public float Intensity { get; set; }
        public float Range { get; set; }
        public float SpotAngle { get; set; }
        public float InnerSpotAngle { get; set; }
        public ShadowType Shadows { get; set; }
        public LightMode Mode { get; set; }
        public bool CastShadows { get; set; }
        public float ShadowBias { get; set; }
        public float ShadowNormalBias { get; set; }
        public float ShadowNearPlane { get; set; }
        public int CookieSize { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public Light()
        {
            Type = LightType.Point;
            Color = Color.White;
            Intensity = 1f;
            Range = 10f;
            SpotAngle = 30f;
            InnerSpotAngle = 20f;
            Shadows = ShadowType.Soft;
            Mode = LightMode.Realtime;
            CastShadows = true;
            ShadowBias = 0.05f;
            ShadowNormalBias = 0.4f;
            ShadowNearPlane = 0.2f;
            CookieSize = 512;
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        public Vector3 GetDirection()
        {
            return Rotation * Vector3.Forward;
        }

        public void SetDirection(Vector3 direction)
        {
            var lookRotation = Quaternion.LookRotation(direction, Vector3.Up);
            Rotation = lookRotation;
        }
    }

    public class LightingSystem
    {
        private List<Light> _lights;
        private Light _sunLight;
        private Color _ambientColor;
        private float _ambientIntensity;

        public List<Light> Lights => _lights;
        public Light SunLight => _sunLight;
        public Color AmbientColor => _ambientColor;
        public float AmbientIntensity => _ambientIntensity;

        public LightingSystem()
        {
            _lights = new List<Light>();
            _ambientColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            _ambientIntensity = 0.5f;

            CreateSunLight();
        }

        private void CreateSunLight()
        {
            _sunLight = new Light
            {
                Name = "Sun",
                Type = LightType.Directional,
                Color = Color.White,
                Intensity = 1.2f,
                Shadows = ShadowType.Soft,
                CastShadows = true,
                Position = new Vector3(0, 100, 0),
                Rotation = Quaternion.Euler(45f, 0f, 0f)
            };
        }

        public void AddLight(Light light)
        {
            if (light != null && !_lights.Contains(light))
            {
                _lights.Add(light);
            }
        }

        public void RemoveLight(Light light)
        {
            _lights.Remove(light);
        }

        public void SetAmbientLight(Color color, float intensity)
        {
            _ambientColor = color;
            _ambientIntensity = intensity;
        }

        public void UpdateSunPosition(float timeOfDay)
        {
            // timeOfDay: 0 = midnight, 0.5 = noon, 1 = midnight
            float angle = (timeOfDay - 0.25f) * 360f;
            _sunLight.Rotation = Quaternion.Euler(angle, 0f, 0f);

            // Adjust color based on time (sunrise/sunset)
            if (timeOfDay < 0.25f || timeOfDay > 0.75f)
            {
                _sunLight.Color = new Color(1f, 0.6f, 0.4f, 1f); // Orange/red
                _sunLight.Intensity = 0.3f;
            }
            else if (timeOfDay > 0.3f && timeOfDay < 0.7f)
            {
                _sunLight.Color = Color.White;
                _sunLight.Intensity = 1.2f;
            }
            else
            {
                _sunLight.Color = new Color(1f, 0.9f, 0.8f, 1f);
                _sunLight.Intensity = 0.8f;
            }
        }

        public List<Light> GetLightsAffectingPosition(Vector3 position)
        {
            var affectingLights = new List<Light>();

            foreach (var light in _lights)
            {
                if (light.Type == LightType.Directional)
                {
                    affectingLights.Add(light);
                }
                else
                {
                    float distance = Vector3.Distance(light.Position, position);
                    if (distance <= light.Range)
                    {
                        affectingLights.Add(light);
                    }
                }
            }

            return affectingLights;
        }
    }

    public static class QuaternionExtensions
    {
        public static Quaternion LookRotation(Vector3 direction, Vector3 up)
        {
            Vector3 forward = Vector3.Normalize(direction);
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            Vector3 newUp = Vector3.Cross(forward, right);

            float m00 = right.X;
            float m01 = right.Y;
            float m02 = right.Z;
            float m10 = newUp.X;
            float m11 = newUp.Y;
            float m12 = newUp.Z;
            float m20 = forward.X;
            float m21 = forward.Y;
            float m22 = forward.Z;

            float trace = m00 + m11 + m22;

            if (trace > 0f)
            {
                float s = (float)System.Math.Sqrt(trace + 1f);
                float invS = 0.5f / s;
                return new Quaternion(
                    (m12 - m21) * invS,
                    (m20 - m02) * invS,
                    (m01 - m10) * invS,
                    0.5f * s
                );
            }
            else
            {
                if (m00 >= m11 && m00 >= m22)
                {
                    float s = (float)System.Math.Sqrt(1f + m00 - m11 - m22);
                    float invS = 0.5f / s;
                    return new Quaternion(
                        0.5f * s,
                        (m01 + m10) * invS,
                        (m02 + m20) * invS,
                        (m12 - m21) * invS
                    );
                }
                else if (m11 > m22)
                {
                    float s = (float)System.Math.Sqrt(1f + m11 - m00 - m22);
                    float invS = 0.5f / s;
                    return new Quaternion(
                        (m01 + m10) * invS,
                        0.5f * s,
                        (m12 + m21) * invS,
                        (m20 - m02) * invS
                    );
                }
                else
                {
                    float s = (float)System.Math.Sqrt(1f + m22 - m00 - m11);
                    float invS = 0.5f / s;
                    return new Quaternion(
                        (m02 + m20) * invS,
                        (m12 + m21) * invS,
                        0.5f * s,
                        (m01 - m10) * invS
                    );
                }
            }
        }
    }
}
