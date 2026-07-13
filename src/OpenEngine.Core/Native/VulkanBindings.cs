// Created By Levi Enama
// P/Invoke Wrapper for Vulkan Native Bindings
using System;
using System.Runtime.InteropServices;

namespace OpenEngine.Core.Native
{
    public static class VulkanBindings
    {
        private const string VulkanLibrary = "vulkan_bindings";

        #region Instance Management

        [DllImport(VulkanLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern int VK_CreateInstance([MarshalAs(UnmanagedType.LPStr)] string applicationName, uint applicationVersion);

        [DllImport(VulkanLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr VK_PickPhysicalDevice();

        [DllImport(VulkanLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern int VK_CreateLogicalDevice();

        #endregion

        #region Shader Management

        [DllImport(VulkanLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr VK_CreateShaderModule([MarshalAs(UnmanagedType.LPStr)] string code, ulong codeSize);

        #endregion

        #region Buffer Management

        [DllImport(VulkanLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr VK_CreateBuffer(ulong size, uint usage, uint properties, out IntPtr bufferMemory);

        #endregion

        #region Cleanup

        [DllImport(VulkanLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern void VK_Shutdown();

        #endregion
    }

    public class VulkanContext : IDisposable
    {
        private bool _disposed;

        public VulkanContext(string applicationName = "Open Engine", uint applicationVersion = 1)
        {
            int result = VulkanBindings.VK_CreateInstance(applicationName, applicationVersion);
            if (result != 0)
            {
                throw new InvalidOperationException("Failed to create Vulkan instance");
            }

            IntPtr physicalDevice = VulkanBindings.VK_PickPhysicalDevice();
            if (physicalDevice == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to pick physical device");
            }

            result = VulkanBindings.VK_CreateLogicalDevice();
            if (result != 0)
            {
                throw new InvalidOperationException("Failed to create logical device");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                VulkanBindings.VK_Shutdown();
                _disposed = true;
            }
        }
    }
}
