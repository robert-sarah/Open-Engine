// Created By Levi Enama
// Shader Loader for GLSL Shaders
using System;
using System.IO;
using System.Reflection;

namespace OpenEngine.Core.Graphics
{
    public static class ShaderLoader
    {
        public static string LoadShader(string shaderName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"OpenEngine.Core.Shaders.GLSL.{shaderName}.glsl";
                
                var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                
                // Fallback to file loading
                string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                string shaderPath = Path.Combine(basePath, "Shaders", "GLSL", $"{shaderName}.glsl");
                
                if (File.Exists(shaderPath))
                {
                    return File.ReadAllText(shaderPath);
                }
                
                throw new FileNotFoundException($"Shader not found: {shaderName}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load shader: {shaderName}", ex);
            }
        }

        public static string LoadShaderFromFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load shader from file: {filePath}", ex);
            }
        }

        public static (string vertex, string fragment) LoadPBRShaders()
        {
            string vertex = LoadShader("pbr_vertex");
            string fragment = LoadShader("pbr_fragment");
            return (vertex, fragment);
        }

        public static (string vertex, string fragment) LoadSkyboxShaders()
        {
            string vertex = LoadShader("skybox");
            string fragment = LoadShader("skybox_fragment");
            return (vertex, fragment);
        }

        public static string LoadDeferredLightingShader()
        {
            return LoadShader("deferred_lighting");
        }

        public static string LoadBloomShader()
        {
            return LoadShader("bloom");
        }

        public static string LoadParticleComputeShader()
        {
            return LoadShader("particle_compute");
        }
    }
}
