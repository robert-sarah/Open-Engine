// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Graphics
{
    public class Material
    {
        public string Name { get; set; }
        public Shader Shader { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public RenderQueue RenderQueue { get; set; }
        public bool IsTransparent { get; set; }
        public float Opacity { get; set; }

        public Material()
        {
            Properties = new Dictionary<string, object>();
            RenderQueue = RenderQueue.Geometry;
            IsTransparent = false;
            Opacity = 1f;
        }

        public void SetFloat(string name, float value)
        {
            Properties[name] = value;
        }

        public void SetVector(string name, Math.Vector3 value)
        {
            Properties[name] = value;
        }

        public void SetColor(string name, Math.Color value)
        {
            Properties[name] = value;
        }

        public void SetTexture(string name, Texture value)
        {
            Properties[name] = value;
        }

        public float GetFloat(string name, float defaultValue = 0f)
        {
            if (Properties.TryGetValue(name, out var value) && value is float f)
                return f;
            return defaultValue;
        }

        public Math.Color GetColor(string name, Math.Color? defaultValue = null)
        {
            if (Properties.TryGetValue(name, out var value) && value is Math.Color c)
                return c;
            return defaultValue ?? Math.Color.White;
        }
    }

    public enum RenderQueue
    {
        Background = 1000,
        Geometry = 2000,
        Transparent = 3000,
        Overlay = 4000
    }

    public class Texture
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TextureFormat Format { get; set; }
        public FilterMode FilterMode { get; set; }
        public WrapMode WrapMode { get; set; }

        public Texture()
        {
            FilterMode = FilterMode.Bilinear;
            WrapMode = WrapMode.Repeat;
        }
    }

    public enum TextureFormat
    {
        RGBA32,
        RGB24,
        ARGB32,
        Alpha8,
        DXT1,
        DXT5,
        BC7,
        ASTC_4x4,
        ASTC_6x6,
        ASTC_8x8
    }

    public enum FilterMode
    {
        Point,
        Bilinear,
        Trilinear
    }

    public enum WrapMode
    {
        Repeat,
        Clamp,
        Mirror
    }
}
