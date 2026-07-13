// Created By Levi Enama
using System;
using Silk.NET.OpenGL;
using OpenEngine.Core.Math;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Engine;

namespace OpenEngine.Editor.Graphics;

public class Renderer3D : IDisposable
{
    private GL _gl;
    private uint _shaderProgram;
    private uint _vao;
    private uint _vbo;

    public Camera MainCamera { get; } = new();

    private readonly float[] _cubeVertices =
    {
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, -1.0f,
         0.5f, -0.5f, -0.5f,  0.0f, 0.0f, -1.0f,
         0.5f,  0.5f, -0.5f,  0.0f, 0.0f, -1.0f,
         0.5f,  0.5f, -0.5f,  0.0f, 0.0f, -1.0f,
        -0.5f,  0.5f, -0.5f,  0.0f, 0.0f, -1.0f,
        -0.5f, -0.5f, -0.5f,  0.0f, 0.0f, -1.0f,
        
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 1.0f,
         0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f,
         0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f,
        -0.5f,  0.5f,  0.5f,  0.0f, 0.0f, 1.0f,
        -0.5f, -0.5f,  0.5f,  0.0f, 0.0f, 1.0f
    };

    public Renderer3D(GL gl)
    {
        _gl = gl;
        Initialize();
    }

    private void Initialize()
    {
        uint vShader = CompileShader(ShaderType.VertexShader, VertexShaderSource);
        uint fShader = CompileShader(ShaderType.FragmentShader, FragmentShaderSource);
        
        _shaderProgram = _gl.CreateProgram();
        _gl.AttachShader(_shaderProgram, vShader);
        _gl.AttachShader(_shaderProgram, fShader);
        _gl.LinkProgram(_shaderProgram);

        _gl.DeleteShader(vShader);
        _gl.DeleteShader(fShader);

        _gl.GenVertexArrays(1, out _vao);
        _gl.GenBuffers(1, out _vbo);
        
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        unsafe
        {
            fixed (float* v = _cubeVertices)
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_cubeVertices.Length * sizeof(float)), v, BufferUsageARB.StaticDraw);
        }
        
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (void*)(3 * sizeof(float)));
        _gl.EnableVertexAttribArray(1);

        _gl.Enable(EnableCap.DepthTest);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
        if (success == 0)
        {
            string infoLog = _gl.GetShaderInfoLog(shader);
            throw new InvalidOperationException($"Shader compilation failed: {infoLog}");
        }
        return shader;
    }

    public void Render(OpenSimulationEngine engine)
    {
        _gl.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _gl.UseProgram(_shaderProgram);

        var view = MainCamera.GetViewMatrix();
        var proj = MainCamera.GetProjectionMatrix(1600f/900f);
        
        int viewLoc = _gl.GetUniformLocation(_shaderProgram, "view");
        int projLoc = _gl.GetUniformLocation(_shaderProgram, "proj");
        int modelLoc = _gl.GetUniformLocation(_shaderProgram, "model");
        int colorLoc = _gl.GetUniformLocation(_shaderProgram, "objectColor");

        unsafe
        {
            _gl.UniformMatrix4(viewLoc, 1, false, (float*)&view);
            _gl.UniformMatrix4(projLoc, 1, false, (float*)&proj);
        }

        _gl.BindVertexArray(_vao);
        foreach (var entity in engine.Entities.Values)
        {
            var model = GetModelMatrix(entity.Position3D, entity.Rotation3D, entity.Scale3D);
            var color = GetEntityColor(entity.Type);

            unsafe { _gl.UniformMatrix4(modelLoc, 1, false, (float*)&model); }
            _gl.Uniform3(colorLoc, color.X, color.Y, color.Z);
            
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }
    }

    private Matrix4x4 GetModelMatrix(Vector3 pos, Vector3 rot, Vector3 scale)
    {
        var translation = Matrix4x4.Translate(pos);
        var rotation = Matrix4x4.RotateX(rot.X) * Matrix4x4.RotateY(rot.Y) * Matrix4x4.RotateZ(rot.Z);
        var scaling = Matrix4x4.Scale(scale);
        return scaling * rotation * translation;
    }

    private Vector3 GetEntityColor(string type)
    {
        return type.ToLower() switch
        {
            "starship" => new Vector3(0, 0.8f, 1),
            "spacestation" => new Vector3(0.2f, 0.8f, 0.2f),
            "asteroid" => new Vector3(0.6f, 0.5f, 0.4f),
            "warship" => new Vector3(1, 0.2f, 0.2f),
            _ => new Vector3(0.8f, 0.8f, 0.8f)
        };
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteProgram(_shaderProgram);
    }

    private const string VertexShaderSource = @"
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
uniform mat4 view;
uniform mat4 proj;
uniform mat4 model;
void main() { gl_Position = proj * view * model * vec4(aPos, 1.0); }
";
    private const string FragmentShaderSource = @"
#version 330 core
out vec4 FragColor;
uniform vec3 objectColor;
void main() { FragColor = vec4(objectColor, 1.0); }
";
}

public class Camera
{
    public Vector3 Position { get; set; } = new(0, 3, 10);
    public Vector3 Target { get; set; } = new(0, 0, -1);
    public Vector3 Up { get; set; } = new(0, 1, 0);
    public float Fov { get; set; } = 45f;
    public float NearPlane { get; set; } = 0.1f;
    public float FarPlane { get; set; } = 1000f;

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.LookAt(Position, Position + Target, Up);
    }

    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        return Matrix4x4.Perspective(Fov * MathF.PI / 180, aspectRatio, NearPlane, FarPlane);
    }
}

public struct Matrix4x4
{
    public float M11, M12, M13, M14;
    public float M21, M22, M23, M24;
    public float M31, M32, M33, M34;
    public float M41, M42, M43, M44;

    public static Matrix4x4 Identity => new()
    {
        M11 = 1, M22 = 1, M33 = 1, M44 = 1
    };

    public static Matrix4x4 Translate(Vector3 v)
    {
        return new Matrix4x4
        {
            M11 = 1, M22 = 1, M33 = 1, M44 = 1,
            M41 = v.X, M42 = v.Y, M43 = v.Z
        };
    }

    public static Matrix4x4 Scale(Vector3 v)
    {
        return new Matrix4x4
        {
            M11 = v.X, M22 = v.Y, M33 = v.Z, M44 = 1
        };
    }

    public static Matrix4x4 RotateX(float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);
        return new Matrix4x4
        {
            M11 = 1, M22 = cos, M23 = sin, M32 = -sin, M33 = cos, M44 = 1
        };
    }

    public static Matrix4x4 RotateY(float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);
        return new Matrix4x4
        {
            M11 = cos, M13 = -sin, M22 = 1, M31 = sin, M33 = cos, M44 = 1
        };
    }

    public static Matrix4x4 RotateZ(float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);
        return new Matrix4x4
        {
            M11 = cos, M12 = sin, M21 = -sin, M22 = cos, M33 = 1, M44 = 1
        };
    }

    public static Matrix4x4 LookAt(Vector3 eye, Vector3 target, Vector3 up)
    {
        Vector3 f = (target - eye).Normalized;
        Vector3 s = Vector3.Cross(f, up).Normalized;
        Vector3 u = Vector3.Cross(s, f);

        return new Matrix4x4
        {
            M11 = s.X, M12 = u.X, M13 = -f.X, M14 = 0,
            M21 = s.Y, M22 = u.Y, M23 = -f.Y, M24 = 0,
            M31 = s.Z, M32 = u.Z, M33 = -f.Z, M34 = 0,
            M41 = -Vector3.Dot(s, eye),
            M42 = -Vector3.Dot(u, eye),
            M43 = Vector3.Dot(f, eye),
            M44 = 1
        };
    }

    public static Matrix4x4 Perspective(float fov, float aspectRatio, float near, float far)
    {
        float tanHalfFov = MathF.Tan(fov / 2f);
        return new Matrix4x4
        {
            M11 = 1f / (aspectRatio * tanHalfFov),
            M22 = 1f / tanHalfFov,
            M33 = -(far + near) / (far - near),
            M34 = -1,
            M43 = -(2 * far * near) / (far - near)
        };
    }

    public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
    {
        return new Matrix4x4
        {
            M11 = a.M11*b.M11 + a.M12*b.M21 + a.M13*b.M31 + a.M14*b.M41,
            M12 = a.M11*b.M12 + a.M12*b.M22 + a.M13*b.M32 + a.M14*b.M42,
            M13 = a.M11*b.M13 + a.M12*b.M23 + a.M13*b.M33 + a.M14*b.M43,
            M14 = a.M11*b.M14 + a.M12*b.M24 + a.M13*b.M34 + a.M14*b.M44,
            
            M21 = a.M21*b.M11 + a.M22*b.M21 + a.M23*b.M31 + a.M24*b.M41,
            M22 = a.M21*b.M12 + a.M22*b.M22 + a.M23*b.M32 + a.M24*b.M42,
            M23 = a.M21*b.M13 + a.M22*b.M23 + a.M23*b.M33 + a.M24*b.M43,
            M24 = a.M21*b.M14 + a.M22*b.M24 + a.M23*b.M34 + a.M24*b.M44,
            
            M31 = a.M31*b.M11 + a.M32*b.M21 + a.M33*b.M31 + a.M34*b.M41,
            M32 = a.M31*b.M12 + a.M32*b.M22 + a.M33*b.M32 + a.M34*b.M42,
            M33 = a.M31*b.M13 + a.M32*b.M23 + a.M33*b.M33 + a.M34*b.M43,
            M34 = a.M31*b.M14 + a.M32*b.M24 + a.M33*b.M34 + a.M34*b.M44,
            
            M41 = a.M41*b.M11 + a.M42*b.M21 + a.M43*b.M31 + a.M44*b.M41,
            M42 = a.M41*b.M12 + a.M42*b.M22 + a.M43*b.M32 + a.M44*b.M42,
            M43 = a.M41*b.M13 + a.M42*b.M23 + a.M43*b.M33 + a.M44*b.M43,
            M44 = a.M41*b.M14 + a.M42*b.M24 + a.M43*b.M34 + a.M44*b.M44
        };
    }
}
