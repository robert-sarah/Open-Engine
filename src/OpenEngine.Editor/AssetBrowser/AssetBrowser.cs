// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenEngine.Editor.AssetBrowser
{
    public enum AssetType { Scene, Prefab, Material, Texture, Mesh, Audio, Script, Folder, Unknown }

    public class AssetItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public AssetType Type { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; set; }

        public AssetItem(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            Size = new FileInfo(path).Length;
            LastModified = File.GetLastWriteTime(path);
            Type = DetermineType(path);
        }

        private AssetType DetermineType(string path)
        {
            var extension = System.IO.Path.GetExtension(path).ToLower();
            switch (extension)
            {
                case ".scene": return AssetType.Scene;
                case ".prefab": return AssetType.Prefab;
                case ".mat": return AssetType.Material;
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".tga":
                case ".bmp": return AssetType.Texture;
                case ".obj":
                case ".fbx":
                case ".gltf": return AssetType.Mesh;
                case ".wav":
                case ".mp3":
                case ".ogg": return AssetType.Audio;
                case ".cs": return AssetType.Script;
                default:
                    if (Directory.Exists(path)) return AssetType.Folder;
                    return AssetType.Unknown;
            }
        }
    }

    public class AssetBrowser
    {
        private string _currentPath;
        private List<AssetItem> _currentAssets;
        private List<string> _favoritePaths;

        public string CurrentPath => _currentPath;
        public List<AssetItem> CurrentAssets => _currentAssets;
        public List<string> FavoritePaths => _favoritePaths;

        public AssetBrowser()
        {
            _currentPath = Directory.GetCurrentDirectory();
            _currentAssets = new List<AssetItem>();
            _favoritePaths = new List<string>();
            Refresh();
        }

        public void SetPath(string path)
        {
            if (Directory.Exists(path))
            {
                _currentPath = path;
                Refresh();
            }
        }

        public void Refresh()
        {
            _currentAssets.Clear();

            if (!Directory.Exists(_currentPath)) return;

            // Add folders
            foreach (var dir in Directory.GetDirectories(_currentPath))
            {
                _currentAssets.Add(new AssetItem(dir));
            }

            // Add files
            foreach (var file in Directory.GetFiles(_currentPath))
            {
                _currentAssets.Add(new AssetItem(file));
            }
        }

        public void NavigateUp()
        {
            var parent = Directory.GetParent(_currentPath);
            if (parent != null)
            {
                SetPath(parent.FullName);
            }
        }

        public void AddToFavorites(string path)
        {
            if (!_favoritePaths.Contains(path))
            {
                _favoritePaths.Add(path);
            }
        }

        public void RemoveFromFavorites(string path)
        {
            _favoritePaths.Remove(path);
        }
    }
}
