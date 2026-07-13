// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Graphics.Shaders
{
    public enum ShaderType
    {
        Vertex,
        Fragment,
        Geometry,
        Compute,
        TessellationControl,
        TessellationEvaluation
    }

    public enum ShaderLanguage
    {
        GLSL,
        HLSL,
        SPIRV,
        Metal
    }

    public class Shader
    {
        public string Name { get; set; }
        public ShaderLanguage Language { get; set; }
        public Dictionary<ShaderType, string> ShaderSources { get; set; }
        public Dictionary<string, ShaderUniform> Uniforms { get; set; }
        public Dictionary<string, ShaderAttribute> Attributes { get; set; }
        public bool IsCompiled { get; set; }
        public uint ProgramId { get; set; }

        public Shader()
        {
            ShaderSources = new Dictionary<ShaderType, string>();
            Uniforms = new Dictionary<string, ShaderUniform>();
            Attributes = new Dictionary<string, ShaderAttribute>();
            Language = ShaderLanguage.GLSL;
        }

        public void SetShaderSource(ShaderType type, string source)
        {
            ShaderSources[type] = source;
        }

        public virtual bool Compile()
        {
            // Base implementation - override in platform-specific implementations
            IsCompiled = true;
            return true;
        }

        public virtual void Bind()
        {
            // Base implementation
        }

        public virtual void Unbind()
        {
            // Base implementation
        }

        public void SetUniform(string name, int value)
        {
            if (Uniforms.ContainsKey(name))
            {
                Uniforms[name].IntValue = value;
            }
        }

        public void SetUniform(string name, float value)
        {
            if (Uniforms.ContainsKey(name))
            {
                Uniforms[name].FloatValue = value;
            }
        }

        public void SetUniform(string name, float x, float y, float z, float w)
        {
            if (Uniforms.ContainsKey(name))
            {
                Uniforms[name].Vec4Value = new float[] { x, y, z, w };
            }
        }

        public void SetUniformMatrix4(string name, float[] matrix)
        {
            if (Uniforms.ContainsKey(name))
            {
                Uniforms[name].Matrix4Value = matrix;
            }
        }
    }

    public class ShaderUniform
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Location { get; set; }
        public int IntValue { get; set; }
        public float FloatValue { get; set; }
        public float[] Vec4Value { get; set; }
        public float[] Matrix4Value { get; set; }

        public ShaderUniform()
        {
            Vec4Value = new float[4];
            Matrix4Value = new float[16];
        }
    }

    public class ShaderAttribute
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Location { get; set; }
        public int Size { get; set; }
    }

    public class ShaderLibrary
    {
        private static Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        public static void RegisterShader(string name, Shader shader)
        {
            _shaders[name] = shader;
        }

        public static Shader GetShader(string name)
        {
            return _shaders.TryGetValue(name, out var shader) ? shader : null;
        }

        public static void UnregisterShader(string name)
        {
            _shaders.Remove(name);
        }

        public static IEnumerable<Shader> GetAllShaders()
        {
            return _shaders.Values;
        }
    }

    public static class BuiltInShaders
    {
        public static string StandardVertexGLSL = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uNormalMatrix;

out vec3 FragPos;
out vec3 Normal;
out vec2 TexCoord;

void main()
{
    FragPos = vec3(uModel * vec4(aPosition, 1.0));
    Normal = mat3(uNormalMatrix) * aNormal;
    TexCoord = aTexCoord;
    gl_Position = uProjection * uView * vec4(FragPos, 1.0);
}";

        public static string StandardFragmentGLSL = @"
#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;
in vec2 TexCoord;

uniform vec3 uViewPos;
uniform vec3 uLightPos;
uniform vec3 uLightColor;
uniform vec3 uObjectColor;
uniform sampler2D uTexture;

void main()
{
    // Ambient
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * uLightColor;

    // Diffuse
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(uLightPos - FragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * uLightColor;

    // Specular
    float specularStrength = 0.5;
    vec3 viewDir = normalize(uViewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * uLightColor;

    vec3 result = (ambient + diffuse + specular) * uObjectColor;
    FragColor = vec4(result, 1.0);
}";

        public static string PBRVertexGLSL = @"
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;
layout (location = 3) in vec3 aTangent;

out VS_OUT {
    vec3 FragPos;
    vec2 TexCoord;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} vs_out;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;
uniform mat3 uNormalMatrix;
uniform vec3 uLightPos;
uniform vec3 uViewPos;

void main()
{
    vs_out.FragPos = vec3(uModel * vec4(aPosition, 1.0));
    vs_out.TexCoord = aTexCoord;
    
    vec3 T = normalize(vec3(uModel * vec4(aTangent, 0.0)));
    vec3 N = normalize(vec3(uModel * vec4(aNormal, 0.0)));
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T);
    
    mat3 TBN = transpose(mat3(T, B, N));
    vs_out.TangentLightPos = TBN * uLightPos;
    vs_out.TangentViewPos = TBN * uViewPos;
    vs_out.TangentFragPos = TBN * vs_out.FragPos;
    
    gl_Position = uProjection * uView * vec4(vs_out.FragPos, 1.0);
}";

        public static string PBRFragmentGLSL = @"
#version 330 core
out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec2 TexCoord;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

uniform sampler2D uAlbedoMap;
uniform sampler2D uNormalMap;
uniform sampler2u uMetallicMap;
uniform sampler2D uRoughnessMap;
uniform sampler2D uAOMap;

uniform vec3 uLightPos;
uniform vec3 uLightColor;
uniform vec3 uViewPos;

const float PI = 3.14159265359;

vec3 getNormalFromMap()
{
    vec3 tangentNormal = texture(uNormalMap, fs_in.TexCoord).xyz;
    vec3 Q1 = dFdx(fs_in.FragPos);
    vec3 Q2 = dFdy(fs_in.FragPos);
    vec2 st1 = dFdx(fs_in.TexCoord);
    vec2 st2 = dFdy(fs_in.TexCoord);
    vec3 N = normalize(Q1 * st2.t - Q2 * st1.t);
    vec3 T = normalize(Q1 * st1.s - Q2 * st1.s);
    vec3 B = -cross(N, T);
    mat3 TBN = mat3(T, B, N);
    return normalize(TBN * tangentNormal);
}

float DistributionGGX(vec3 N, vec3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;
    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = PI * denom * denom;
    return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;
    float nom = NdotV;
    float denom = NdotV * (1.0 - k) + k;
    return nom / denom;
}

float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);
    return ggx1 * ggx2;
}

vec3 fresnelSchlick(float cosTheta, vec3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

void main()
{
    vec3 albedo = pow(texture(uAlbedoMap, fs_in.TexCoord).rgb, vec3(2.2));
    float metallic = texture(uMetallicMap, fs_in.TexCoord).r;
    float roughness = texture(uRoughnessMap, fs_in.TexCoord).r;
    float ao = texture(uAOMap, fs_in.TexCoord).r;

    vec3 N = getNormalFromMap();
    vec3 V = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);

    vec3 F0 = vec3(0.04);
    F0 = mix(F0, albedo, metallic);

    vec3 Lo = vec3(0.0);
    vec3 L = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    vec3 H = normalize(V + L);
    float distance = length(fs_in.TangentLightPos - fs_in.TangentFragPos);
    float attenuation = 1.0 / (distance * distance);
    vec3 radiance = uLightColor * attenuation;

    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    vec3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);

    vec3 kS = F;
    vec3 kD = vec3(1.0) - kS;
    kD *= 1.0 - metallic;

    vec3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    vec3 specular = numerator / denominator;

    vec3 NdotL = max(dot(N, L), 0.0);
    Lo += (kD * albedo / PI + specular) * radiance * NdotL;

    vec3 ambient = vec3(0.03) * albedo * ao;
    vec3 color = ambient + Lo;

    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0 / 2.2));

    FragColor = vec4(color, 1.0);
}";

        public static string ComputeParticleGLSL = @"
#version 430 core

layout (local_size_x = 128, local_size_y = 1, local_size_z = 1) in;

struct Particle {
    vec4 position;
    vec4 velocity;
    vec4 color;
    float life;
    float size;
};

layout (std430, binding = 0) buffer ParticleBuffer {
    Particle particles[];
};

uniform float uDeltaTime;
uniform vec3 uGravity;
uniform vec3 uEmitterPosition;
uniform uint uMaxParticles;
uniform uint uEmissionRate;

void main()
{
    uint index = gl_GlobalInvocationID.x;
    if (index >= uMaxParticles) return;

    Particle p = particles[index];
    
    if (p.life <= 0.0)
    {
        // Respawn particle
        p.position = vec4(uEmitterPosition, 1.0);
        p.velocity = vec4(
            (fract(sin(dot(vec2(index, index * 2.0), vec2(12.9898, 78.233))) * 43758.5453) - 0.5) * 2.0,
            (fract(sin(dot(vec2(index * 3.0, index), vec2(12.9898, 78.233))) * 43758.5453) - 0.5) * 2.0,
            (fract(sin(dot(vec2(index, index * 4.0), vec2(12.9898, 78.233))) * 43758.5453) - 0.5) * 2.0 + 1.0,
            0.0
        );
        p.life = 1.0;
        p.size = 1.0;
        p.color = vec4(1.0, 1.0, 1.0, 1.0);
    }
    else
    {
        // Update particle
        p.velocity += vec4(uGravity * uDeltaTime, 0.0);
        p.position += p.velocity * uDeltaTime;
        p.life -= uDeltaTime * 0.5;
        p.size *= 0.99;
    }

    particles[index] = p;
}";
    }
}
