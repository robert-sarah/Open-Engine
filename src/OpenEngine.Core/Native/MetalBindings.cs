// Created By Levi Enama
// P/Invoke Wrapper for Metal Native Bindings (iOS/macOS)
using System;
using System.Runtime.InteropServices;
using System.Numerics;

namespace OpenEngine.Core.Native
{
    public static class MetalBindings
    {
        private const string MetalLibrary = "MetalRenderer";

        #region Device Management

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Metal_CreateDevice();

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Metal_DestroyDevice(IntPtr device);

        #endregion

        #region Command Queue

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Metal_CreateCommandQueue(IntPtr device);

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Metal_DestroyCommandQueue(IntPtr commandQueue);

        #endregion

        #region Pipeline State

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Metal_CreateRenderPipeline(IntPtr device, [MarshalAs(UnmanagedType.LPStr)] string vertexShader, [MarshalAs(UnmanagedType.LPStr)] string fragmentShader);

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Metal_DestroyRenderPipeline(IntPtr pipeline);

        #endregion

        #region Buffers

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Metal_CreateBuffer(IntPtr device, float[] vertices, int vertexCount);

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Metal_DestroyBuffer(IntPtr buffer);

        #endregion

        #region Rendering

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Metal_CreateDrawable(IntPtr layer);

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Metal_BeginFrame(IntPtr drawable);

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Metal_EndFrame(IntPtr drawable);

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Metal_DrawPrimitives(IntPtr pipeline, IntPtr vertexBuffer, int vertexCount);

        #endregion

        #region Layer Management

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Metal_CreateCAMetalLayer(IntPtr view);

        [DllImport(MetalLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Metal_DestroyCAMetalLayer(IntPtr layer);

        #endregion
    }

    public class MetalContext : IDisposable
    {
        private IntPtr _device;
        private IntPtr _commandQueue;
        private bool _disposed;

        public MetalContext()
        {
            _device = MetalBindings.Metal_CreateDevice();
            if (_device == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create Metal device");
            }

            _commandQueue = MetalBindings.Metal_CreateCommandQueue(_device);
            if (_commandQueue == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create Metal command queue");
            }
        }

        public IntPtr Device => _device;
        public IntPtr CommandQueue => _commandQueue;

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_commandQueue != IntPtr.Zero)
                {
                    MetalBindings.Metal_DestroyCommandQueue(_commandQueue);
                }
                if (_device != IntPtr.Zero)
                {
                    MetalBindings.Metal_DestroyDevice(_device);
                }
                _disposed = true;
            }
        }
    }

    public class MetalRenderPipeline : IDisposable
    {
        private IntPtr _pipeline;
        private bool _disposed;

        public MetalRenderPipeline(IntPtr device, string vertexShader, string fragmentShader)
        {
            _pipeline = MetalBindings.Metal_CreateRenderPipeline(device, vertexShader, fragmentShader);
            if (_pipeline == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create Metal render pipeline");
            }
        }

        public IntPtr Pipeline => _pipeline;

        public void Dispose()
        {
            if (!_disposed)
            {
                MetalBindings.Metal_DestroyRenderPipeline(_pipeline);
                _disposed = true;
            }
        }
    }

    public class MetalBuffer : IDisposable
    {
        private IntPtr _buffer;
        private bool _disposed;

        public MetalBuffer(IntPtr device, float[] vertices)
        {
            _buffer = MetalBindings.Metal_CreateBuffer(device, vertices, vertices.Length);
            if (_buffer == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create Metal buffer");
            }
        }

        public IntPtr Buffer => _buffer;

        public void Dispose()
        {
            if (!_disposed)
            {
                MetalBindings.Metal_DestroyBuffer(_buffer);
                _disposed = true;
            }
        }
    }
}
