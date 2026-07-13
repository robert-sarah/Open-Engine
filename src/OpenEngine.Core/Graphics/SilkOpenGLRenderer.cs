// Created By Levi Enama
// Silk.NET OpenGL Renderer Wrapper
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenEngine.Core.Graphics
{
    public class SilkOpenGLRenderer : IDisposable
    {
        private GL _gl;
        private IWindow? _window;
        private Dictionary<string, uint> _shaderPrograms;
        private Dictionary<string, uint> _vertexArrays;
        private Dictionary<string, uint> _vertexBuffers;
        private Dictionary<string, uint> _textures;
        private Dictionary<string, uint> _framebuffers;
        private bool _disposed;

        public GL GL => _gl;
        public IWindow? Window => _window;

        public SilkOpenGLRenderer(IWindow? window)
        {
            _window = window;
            _shaderPrograms = new Dictionary<string, uint>();
            _vertexArrays = new Dictionary<string, uint>();
            _vertexBuffers = new Dictionary<string, uint>();
            _textures = new Dictionary<string, uint>();
            _framebuffers = new Dictionary<string, uint>();
            _disposed = false;
            
            if (_window != null)
            {
                _gl = GL.GetApi(_window);
            }
        }

        public void Initialize()
        {
            _gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            _gl.Enable(EnableCap.DepthTest);
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public void Clear()
        {
            _gl.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void SetViewport(uint x, uint y, uint width, uint height)
        {
            _gl.Viewport(x, y, width, height);
        }

        public uint CreateShader(ShaderType type, string source)
        {
            uint shader = _gl.CreateShader(type);
            _gl.ShaderSource(shader, source);
            _gl.CompileShader(shader);

            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
            {
                _gl.GetShaderInfoLog(shader, out string infoLog);
                Console.WriteLine($"Shader compilation error: {infoLog}");
                _gl.DeleteShader(shader);
                return 0;
            }

            return shader;
        }

        public uint CreateProgram(uint vertexShader, uint fragmentShader)
        {
            uint program = _gl.CreateProgram();
            _gl.AttachShader(program, vertexShader);
            _gl.AttachShader(program, fragmentShader);
            _gl.LinkProgram(program);

            _gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
            {
                _gl.GetProgramInfoLog(program, out string infoLog);
                Console.WriteLine($"Program linking error: {infoLog}");
                return 0;
            }

            return program;
        }

        public void UseProgram(uint program)
        {
            _gl.UseProgram(program);
        }

        public void DeleteProgram(uint program)
        {
            _gl.DeleteProgram(program);
        }

        public uint CreateVertexArray()
        {
            uint vao = _gl.GenVertexArray();
            _gl.BindVertexArray(vao);
            return vao;
        }

        public uint CreateBuffer()
        {
            return _gl.GenBuffer();
        }

        public void BindBuffer(BufferTargetARB target, uint buffer)
        {
            _gl.BindBuffer(target, buffer);
        }

        public void BufferData<T>(BufferTargetARB target, Span<T> data, BufferUsageARB usage) where T : unmanaged
        {
            _gl.BufferData(target, data, usage);
        }

        public void VertexAttribPointer(uint index, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
        {
            _gl.VertexAttribPointer(index, size, type, normalized, stride, (void*)offset);
        }

        public void EnableVertexAttribArray(uint index)
        {
            _gl.EnableVertexAttribArray(index);
        }

        public void DrawArrays(PrimitiveType type, int first, int count)
        {
            _gl.DrawArrays(type, first, count);
        }

        public void DrawElements(PrimitiveType type, uint count, DrawElementsType indicesType, int offset)
        {
            _gl.DrawElements(type, count, indicesType, (void*)offset);
        }

        public void DeleteVertexArray(uint vao)
        {
            _gl.DeleteVertexArray(vao);
        }

        public void DeleteBuffer(uint buffer)
        {
            _gl.DeleteBuffer(buffer);
        }

        public int GetUniformLocation(uint program, string name)
        {
            return _gl.GetUniformLocation(program, name);
        }

        public void Uniform1(int location, float value)
        {
            _gl.Uniform1(location, value);
        }

        public void Uniform2(int location, Vector2 value)
        {
            _gl.Uniform2(location, value.X, value.Y);
        }

        public void Uniform3(int location, Vector3 value)
        {
            _gl.Uniform3(location, value.X, value.Y, value.Z);
        }

        public void Uniform4(int location, Vector4 value)
        {
            _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
        }

        public void UniformMatrix4(int location, bool transpose, Matrix4x4 value)
        {
            _gl.UniformMatrix4(location, 1, transpose, value);
        }

        public uint CreateTexture()
        {
            return _gl.GenTexture();
        }

        public void BindTexture(TextureTarget target, uint texture)
        {
            _gl.BindTexture(target, texture);
        }

        public void TexImage2D<T>(TextureTarget target, int level, InternalFormat internalFormat, uint width, uint height, int border, PixelFormat format, PixelType type, Span<T> pixels) where T : unmanaged
        {
            _gl.TexImage2D(target, level, internalFormat, width, height, border, format, type, pixels);
        }

        public void TexParameter(TextureTarget target, TextureParameterName pname, int param)
        {
            _gl.TexParameterI(target, pname, param);
        }

        public void GenerateMipmap(TextureTarget target)
        {
            _gl.GenerateMipmap(target);
        }

        public void DeleteTexture(uint texture)
        {
            _gl.DeleteTexture(texture);
        }

        public uint CreateFramebuffer()
        {
            return _gl.GenFramebuffer();
        }

        public void BindFramebuffer(FramebufferTarget target, uint framebuffer)
        {
            _gl.BindFramebuffer(target, framebuffer);
        }

        public void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget texTarget, uint texture, int level)
        {
            _gl.FramebufferTexture2D(target, attachment, texTarget, texture, level);
        }

        public void DeleteFramebuffer(uint framebuffer)
        {
            _gl.DeleteFramebuffer(framebuffer);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}

        public void GenerateMipmap(TextureTarget target)
        {
            _gl.GenerateMipmap(target);
        }

        public void DeleteTexture(uint texture)
        {
            _gl.DeleteTexture(texture);
        }

        public uint CreateFramebuffer()
        {
            return _gl.GenFramebuffer();
        }

        public void BindFramebuffer(FramebufferTarget target, uint framebuffer)
        {
            _gl.BindFramebuffer(target, framebuffer);
        }

        public void FramebufferTexture2D(FramebufferTarget target, FramebufferAttachment attachment, TextureTarget texTarget, uint texture, int level)
        {
            _gl.FramebufferTexture2D(target, attachment, texTarget, texture, level);
        }

        public void DeleteFramebuffer(uint framebuffer)
        {
            _gl.DeleteFramebuffer(framebuffer);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}
