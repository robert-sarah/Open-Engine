// Created By Levi Enama
// P/Invoke Wrapper for OpenGL Native Bindings
using System;
using System.Runtime.InteropServices;
using System.Numerics;

namespace OpenEngine.Core.Native
{
    public static class OpenGLBindings
    {
        private const string OpenGLLibrary = "opengl_bindings";

        #region Context Management

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_CreateContext(IntPtr window, int width, int height, int major, int minor);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_SetViewport(int x, int y, int width, int height);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_ClearColor(float r, float g, float b, float a);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_Clear(int mask);

        #endregion

        #region Shader Management

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_CreateShader([MarshalAs(UnmanagedType.LPStr)] string vertexSource, [MarshalAs(UnmanagedType.LPStr)] string fragmentSource);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_UseShader(IntPtr shader);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_DeleteShader(IntPtr shader);

        #endregion

        #region Uniform Management

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_SetUniformInt(IntPtr shader, [MarshalAs(UnmanagedType.LPStr)] string name, int value);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_SetUniformFloat(IntPtr shader, [MarshalAs(UnmanagedType.LPStr)] string name, float value);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_SetUniformVec3(IntPtr shader, [MarshalAs(UnmanagedType.LPStr)] string name, float x, float y, float z);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_SetUniformVec4(IntPtr shader, [MarshalAs(UnmanagedType.LPStr)] string name, float x, float y, float z, float w);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_SetUniformMat4(IntPtr shader, [MarshalAs(UnmanagedType.LPStr)] string name, float[] matrix);

        #endregion

        #region Buffer Management

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_CreateMesh(float[] vertices, int vertexCount, uint[] indices, int indexCount);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_DrawMesh(IntPtr mesh);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_DeleteMesh(IntPtr mesh);

        #endregion

        #region Texture Management

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_CreateTexture(byte[] data, int width, int height, int channels);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_BindTexture(IntPtr texture, int unit);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_DeleteTexture(IntPtr texture);

        #endregion

        #region Framebuffer Management

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_CreateFramebuffer(int width, int height);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_BindFramebuffer(IntPtr fbo);

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_DeleteFramebuffer(IntPtr fbo);

        #endregion

        #region Cleanup

        [DllImport(OpenGLLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_Shutdown();

        #endregion

        #region Constants

        public const int GL_COLOR_BUFFER_BIT = 0x00004000;
        public const int GL_DEPTH_BUFFER_BIT = 0x00000100;
        public const int GL_STENCIL_BUFFER_BIT = 0x00000400;

        #endregion
    }

    public class OpenGLContext : IDisposable
    {
        private IntPtr _context;
        private bool _disposed;

        public OpenGLContext(IntPtr window, int width, int height, int major = 4, int minor = 5)
        {
            _context = OpenGLBindings.GL_CreateContext(window, width, height, major, minor);
            if (_context == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create OpenGL context");
            }
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            OpenGLBindings.GL_SetViewport(x, y, width, height);
        }

        public void ClearColor(float r, float g, float b, float a)
        {
            OpenGLBindings.GL_ClearColor(r, g, b, a);
        }

        public void Clear(bool color = true, bool depth = true, bool stencil = false)
        {
            int mask = 0;
            if (color) mask |= OpenGLBindings.GL_COLOR_BUFFER_BIT;
            if (depth) mask |= OpenGLBindings.GL_DEPTH_BUFFER_BIT;
            if (stencil) mask |= OpenGLBindings.GL_STENCIL_BUFFER_BIT;
            OpenGLBindings.GL_Clear(mask);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                OpenGLBindings.GL_Shutdown();
                _disposed = true;
            }
        }
    }

    public class OpenGLShader : IDisposable
    {
        private IntPtr _shader;
        private bool _disposed;

        public OpenGLShader(string vertexSource, string fragmentSource)
        {
            _shader = OpenGLBindings.GL_CreateShader(vertexSource, fragmentSource);
            if (_shader == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create shader");
            }
        }

        public void Use()
        {
            OpenGLBindings.GL_UseShader(_shader);
        }

        public void SetInt(string name, int value)
        {
            OpenGLBindings.GL_SetUniformInt(_shader, name, value);
        }

        public void SetFloat(string name, float value)
        {
            OpenGLBindings.GL_SetUniformFloat(_shader, name, value);
        }

        public void SetVec3(string name, Vector3 value)
        {
            OpenGLBindings.GL_SetUniformVec3(_shader, name, value.X, value.Y, value.Z);
        }

        public void SetVec4(string name, Vector4 value)
        {
            OpenGLBindings.GL_SetUniformVec4(_shader, name, value.X, value.Y, value.Z, value.W);
        }

        public void SetMat4(string name, Matrix4x4 value)
        {
            float[] matrix = new float[16];
            matrix[0] = value.M11; matrix[1] = value.M12; matrix[2] = value.M13; matrix[3] = value.M14;
            matrix[4] = value.M21; matrix[5] = value.M22; matrix[6] = value.M23; matrix[7] = value.M24;
            matrix[8] = value.M31; matrix[9] = value.M32; matrix[10] = value.M33; matrix[11] = value.M34;
            matrix[12] = value.M41; matrix[13] = value.M42; matrix[14] = value.M43; matrix[15] = value.M44;
            OpenGLBindings.GL_SetUniformMat4(_shader, name, matrix);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                OpenGLBindings.GL_DeleteShader(_shader);
                _disposed = true;
            }
        }
    }

    public class OpenGLMesh : IDisposable
    {
        private IntPtr _mesh;
        private bool _disposed;

        public OpenGLMesh(float[] vertices, uint[] indices)
        {
            _mesh = OpenGLBindings.GL_CreateMesh(vertices, vertices.Length, indices, indices.Length);
            if (_mesh == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create mesh");
            }
        }

        public void Draw()
        {
            OpenGLBindings.GL_DrawMesh(_mesh);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                OpenGLBindings.GL_DeleteMesh(_mesh);
                _disposed = true;
            }
        }
    }

    public class OpenGLTexture : IDisposable
    {
        private IntPtr _texture;
        private bool _disposed;

        public OpenGLTexture(byte[] data, int width, int height, int channels)
        {
            _texture = OpenGLBindings.GL_CreateTexture(data, width, height, channels);
            if (_texture == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create texture");
            }
        }

        public void Bind(int unit = 0)
        {
            OpenGLBindings.GL_BindTexture(_texture, unit);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                OpenGLBindings.GL_DeleteTexture(_texture);
                _disposed = true;
            }
        }
    }

    public class OpenGLFramebuffer : IDisposable
    {
        private IntPtr _fbo;
        private bool _disposed;

        public OpenGLFramebuffer(int width, int height)
        {
            _fbo = OpenGLBindings.GL_CreateFramebuffer(width, height);
            if (_fbo == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create framebuffer");
            }
        }

        public void Bind()
        {
            OpenGLBindings.GL_BindFramebuffer(_fbo);
        }

        public static void Unbind()
        {
            OpenGLBindings.GL_BindFramebuffer(IntPtr.Zero);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                OpenGLBindings.GL_DeleteFramebuffer(_fbo);
                _disposed = true;
            }
        }
    }
}
