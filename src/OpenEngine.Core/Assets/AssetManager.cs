// Created By Levi Enama
// Asset Manager for Managing Game Assets
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenEngine.Core.Assets
{
    public class AssetManager
    {
        private static AssetManager _instance;
        private readonly Dictionary<string, glTFImportResult> _loadedModels;
        private readonly Dictionary<string, byte[]> _loadedTextures;
        private readonly Dictionary<string, AnimationClip> _loadedAnimations;
        private readonly Dictionary<string, MeshData> _loadedMeshes;
        private readonly string _assetsPath;

        public static AssetManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssetManager();
                }
                return _instance;
            }
        }

        private AssetManager()
        {
            _loadedModels = new Dictionary<string, glTFImportResult>();
            _loadedTextures = new Dictionary<string, byte[]>();
            _loadedAnimations = new Dictionary<string, AnimationClip>();
            _loadedMeshes = new Dictionary<string, MeshData>();
            _assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
            
            // Create assets directory if it doesn't exist
            if (!Directory.Exists(_assetsPath))
            {
                Directory.CreateDirectory(_assetsPath);
            }
        }

        public string AssetsPath => _assetsPath;

        public glTFImportResult LoadModel(string filePath)
        {
            string fullPath = Path.IsPathRooted(filePath) 
                ? filePath 
                : Path.Combine(_assetsPath, filePath);

            string cacheKey = fullPath.ToLower();

            if (_loadedModels.ContainsKey(cacheKey))
            {
                return _loadedModels[cacheKey];
            }

            try
            {
                var result = glTFImporter.Import(fullPath);
                _loadedModels[cacheKey] = result;
                
                // Cache individual meshes
                foreach (var mesh in result.Meshes)
                {
                    string meshKey = $"{cacheKey}_{mesh.Name}";
                    _loadedMeshes[meshKey] = mesh;
                }

                // Cache individual animations
                foreach (var animation in result.Animations)
                {
                    string animKey = $"{cacheKey}_{animation.Name}";
                    _loadedAnimations[animKey] = animation;
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load model: {filePath}", ex);
            }
        }

        public async Task<glTFImportResult> LoadModelAsync(string filePath)
        {
            return await Task.Run(() => LoadModel(filePath));
        }

        public MeshData LoadMesh(string modelPath, string meshName)
        {
            string cacheKey = $"{modelPath.ToLower()}_{meshName}";
            
            if (_loadedMeshes.ContainsKey(cacheKey))
            {
                return _loadedMeshes[cacheKey];
            }

            // Load the full model first
            var model = LoadModel(modelPath);
            
            foreach (var mesh in model.Meshes)
            {
                if (mesh.Name == meshName)
                {
                    return mesh;
                }
            }

            throw new InvalidOperationException($"Mesh not found: {meshName}");
        }

        public AnimationClip LoadAnimation(string modelPath, string animationName)
        {
            string cacheKey = $"{modelPath.ToLower()}_{animationName}";
            
            if (_loadedAnimations.ContainsKey(cacheKey))
            {
                return _loadedAnimations[cacheKey];
            }

            // Load the full model first
            var model = LoadModel(modelPath);
            
            foreach (var animation in model.Animations)
            {
                if (animation.Name == animationName)
                {
                    return animation;
                }
            }

            throw new InvalidOperationException($"Animation not found: {animationName}");
        }

        public byte[] LoadTexture(string filePath)
        {
            string fullPath = Path.IsPathRooted(filePath) 
                ? filePath 
                : Path.Combine(_assetsPath, filePath);

            string cacheKey = fullPath.ToLower();

            if (_loadedTextures.ContainsKey(cacheKey))
            {
                return _loadedTextures[cacheKey];
            }

            try
            {
                var data = File.ReadAllBytes(fullPath);
                _loadedTextures[cacheKey] = data;
                return data;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load texture: {filePath}", ex);
            }
        }

        public async Task<byte[]> LoadTextureAsync(string filePath)
        {
            return await Task.Run(() => LoadTexture(filePath));
        }

        public void UnloadModel(string filePath)
        {
            string fullPath = Path.IsPathRooted(filePath) 
                ? filePath 
                : Path.Combine(_assetsPath, filePath);

            string cacheKey = fullPath.ToLower();

            if (_loadedModels.ContainsKey(cacheKey))
            {
                var model = _loadedModels[cacheKey];
                
                // Remove cached meshes
                foreach (var mesh in model.Meshes)
                {
                    string meshKey = $"{cacheKey}_{mesh.Name}";
                    _loadedMeshes.Remove(meshKey);
                }

                // Remove cached animations
                foreach (var animation in model.Animations)
                {
                    string animKey = $"{cacheKey}_{animation.Name}";
                    _loadedAnimations.Remove(animKey);
                }

                _loadedModels.Remove(cacheKey);
            }
        }

        public void UnloadTexture(string filePath)
        {
            string fullPath = Path.IsPathRooted(filePath) 
                ? filePath 
                : Path.Combine(_assetsPath, filePath);

            string cacheKey = fullPath.ToLower();
            _loadedTextures.Remove(cacheKey);
        }

        public void UnloadAll()
        {
            _loadedModels.Clear();
            _loadedTextures.Clear();
            _loadedAnimations.Clear();
            _loadedMeshes.Clear();
        }

        public void ClearCache()
        {
            UnloadAll();
        }

        public bool IsModelLoaded(string filePath)
        {
            string fullPath = Path.IsPathRooted(filePath) 
                ? filePath 
                : Path.Combine(_assetsPath, filePath);

            return _loadedModels.ContainsKey(fullPath.ToLower());
        }

        public bool IsTextureLoaded(string filePath)
        {
            string fullPath = Path.IsPathRooted(filePath) 
                ? filePath 
                : Path.Combine(_assetsPath, filePath);

            return _loadedTextures.ContainsKey(fullPath.ToLower());
        }

        public int LoadedModelCount => _loadedModels.Count;
        public int LoadedTextureCount => _loadedTextures.Count;
        public int LoadedAnimationCount => _loadedAnimations.Count;
        public int LoadedMeshCount => _loadedMeshes.Count;
    }
}
