// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Rendering
{
    public enum RenderMode { Opaque, Cutout, Transparent, Fade }
    public enum BlendMode { Alpha, Additive, Multiply, Subtractive }
    public enum CullMode { Back, Front, Off }

    public class Material
    {
        public string Name { get; set; }
        public Shader Shader { get; set; }
        public Dictionary<string, Texture> Textures { get; set; }
        public Dictionary<string, float> FloatProperties { get; set; }
        public Dictionary<string, Vector4> VectorProperties { get; set; }
        public Dictionary<string, Color> ColorProperties { get; set; }
        public RenderMode RenderMode { get; set; }
        public bool DoubleSided { get; set; }
        public bool EnableInstancing { get; set; }
        public int RenderQueue { get; set; }

        public Material()
        {
            Textures = new Dictionary<string, Texture>();
            FloatProperties = new Dictionary<string, float>();
            VectorProperties = new Dictionary<string, Vector4>();
            ColorProperties = new Dictionary<string, Color>();
            RenderMode = RenderMode.Opaque;
            DoubleSided = false;
            EnableInstancing = false;
            RenderQueue = 2000;
        }

        public void SetTexture(string propertyName, Texture texture)
        {
            Textures[propertyName] = texture;
        }

        public void SetFloat(string propertyName, float value)
        {
            FloatProperties[propertyName] = value;
        }

        public void SetVector(string propertyName, Vector4 value)
        {
            VectorProperties[propertyName] = value;
        }

        public void SetColor(string propertyName, Color value)
        {
            ColorProperties[propertyName] = value;
        }

        public Texture GetTexture(string propertyName)
        {
            return Textures.ContainsKey(propertyName) ? Textures[propertyName] : null;
        }

        public float GetFloat(string propertyName)
        {
            return FloatProperties.ContainsKey(propertyName) ? FloatProperties[propertyName] : 0f;
        }

        public Vector4 GetVector(string propertyName)
        {
            return VectorProperties.ContainsKey(propertyName) ? VectorProperties[propertyName] : Vector4.Zero;
        }

        public Color GetColor(string propertyName)
        {
            return ColorProperties.ContainsKey(propertyName) ? ColorProperties[propertyName] : Color.White;
        }

        public bool HasProperty(string propertyName)
        {
            return Textures.ContainsKey(propertyName) ||
                   FloatProperties.ContainsKey(propertyName) ||
                   VectorProperties.ContainsKey(propertyName) ||
                   ColorProperties.ContainsKey(propertyName);
        }
    }

    public class Shader
    {
        public string Name { get; set; }
        public string VertexShaderCode { get; set; }
        public string FragmentShaderCode { get; set; }
        public List<ShaderProperty> Properties { get; set; }
        public List<ShaderPass> Passes { get; set; }
        public bool IsSupported { get; set; }

        public Shader()
        {
            Properties = new List<ShaderProperty>();
            Passes = new List<ShaderPass>();
            IsSupported = true;
        }

        public void AddProperty(ShaderProperty property)
        {
            Properties.Add(property);
        }

        public void AddPass(ShaderPass pass)
        {
            Passes.Add(pass);
        }(string propertyName)
        {
            return Properties.Find(p => p.Name == propertyName);
        }
    }

    public class ShaderProperty
    {
        public string Name { get; set; }
        public ShaderPropertyType Type { get; set; }
        public object DefaultValue { get; set; }
        public string Description { get; set; }

        public ShaderProperty(string name, ShaderPropertyType type, object defaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
        }
    }

    public enum ShaderPropertyType { Float, Vector, Color, Texture, Range, Int }

    public class ShaderPass
    {
        public string Name { get; set; }
        public string VertexShader { get; set; }
        public string FragmentShader { get; set; }
        public BlendMode BlendMode { get; set; }
        public CullMode CullMode { get; set; }
        public bool ZWrite { get; set; }
        public int ZTest { get; set; }

        public ShaderPass()
        {
            BlendMode = BlendMode.Alpha;
            CullMode = CullMode.Back;
            ZWrite = true;
            ZTest = 4; // LEqual
        }
    }

    public class Texture
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TextureFormat Format { get; set; }
        public FilterMode FilterMode { get; set; }
        public TextureWrapMode WrapMode { get; set; }
        public bool MipMaps { get; set; }
        public bool AnisoLevel { get; set; }

        public Texture()
        {
            FilterMode = FilterMode.Bilinear;
            WrapMode = TextureWrapMode.Repeat;
            MipMaps = true;
            AnisoLevel = true;
        }
    }

    public enum TextureFormat { RGBA32, RGB24, ARGB32, Alpha8, DXT1, DXT5, BC7 }
    public enum FilterMode { Point, Bilinear, Trilinear }
    public enum TextureWrapMode { Repeat, Clamp, Mirror, Once }

    public struct Color
    {
        public float R, G, B, A;
        public Color(float r, float g, float b, float a = 1f) { R = r; G = g; B = b; A = a; }
        public static Color White => new Color(1, 1, 1, 1);
        public static Color Black => new Color(0, 0, 0, 1);
        public static Color Red => new Color(1, 0, 0, 1);
        public static Color Green => new Color(0, 1, 0, 1);
        public static Color Blue => new Color(0, 0, 1, 1);
    }

    public struct Vector4
    {
        public float X, Y, Z, W;
        public Vector4(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
        public static Vector4 Zero => new Vector4(0, 0, 0, 0);
        public static Vector4 One => new Vector4(1, 1, 1, 1);
    }
}
