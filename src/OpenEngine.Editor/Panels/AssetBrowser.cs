// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenEngine.Editor.Panels
{
    public enum AssetType
    {
        Scene,
        Prefab,
        Material,
        Texture,
        Mesh,
        Audio,
        Script,
        Shader,
        Animation,
        Font,
        Folder,
        Unknown
    }

    public class AssetItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public AssetType Type { get; set; }
        public DateTime LastModified { get; set; }
        public long Size { get; set; }
        public bool IsFolder { get; set; }

        public AssetItem(string name, string path, AssetType type)
        {
            Name = name;
            Path = path;
            Type = type;
            LastModified = File.Exists(path) ? File.GetLastWriteTime(path) : DateTime.Now;
            Size = File.Exists(path) ? new FileInfo(path).Length : 0;
            IsFolder = type == AssetType.Folder;
        }
    }

    public class AssetBrowser
    {
        public string RootPath { get; set; }
        public string CurrentPath { get; set; }
        public List<AssetItem> CurrentAssets { get; set; }
        public List<string> SearchHistory { get; set; }
        public AssetViewMode ViewMode { get; set; }
        public string SearchQuery { get; set; }
        public AssetItem SelectedAsset { get; set; }

        public event EventHandler<AssetItem> OnAssetSelected;
        public event EventHandler<AssetItem> OnAssetDoubleClicked;
        public event EventHandler OnPathChanged;

        public AssetBrowser()
        {
            RootPath = "Assets";
            CurrentPath = RootPath;
            CurrentAssets = new List<AssetItem>();
            SearchHistory = new List<string>();
            ViewMode = AssetViewMode.Grid;
            SearchQuery = "";
        }

        public void SetRootPath(string path)
        {
            if (Directory.Exists(path))
            {
                RootPath = path;
                CurrentPath = path;
                RefreshAssets();
                OnPathChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void NavigateToPath(string path)
        {
            if (Directory.Exists(path))
            {
                CurrentPath = path;
                RefreshAssets();
                OnPathChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void NavigateUp()
        {
            var parent = Directory.GetParent(CurrentPath);
            if (parent != null)
            {
                NavigateToPath(parent.FullName);
            }
        }

        public void NavigateToRoot()
        {
            NavigateToPath(RootPath);
        }

        public void RefreshAssets()
        {
            CurrentAssets.Clear();

            if (!Directory.Exists(CurrentPath))
                return;

            // Add folders first
            foreach (var dir in Directory.GetDirectories(CurrentPath))
            {
                var dirInfo = new DirectoryInfo(dir);
                CurrentAssets.Add(new AssetItem(dirInfo.Name, dir, AssetType.Folder));
            }

            // Add files
            foreach (var file in Directory.GetFiles(CurrentPath))
            {
                var fileInfo = new FileInfo(file);
                var type = GetAssetType(fileInfo.Extension);
                CurrentAssets.Add(new AssetItem(fileInfo.Name, file, type));
            }
        }

        private AssetType GetAssetType(string extension)
        {
            return extension.ToLower() switch
            {
                ".unity" => AssetType.Scene,
                ".prefab" => AssetType.Prefab,
                ".mat" => AssetType.Material,
                ".png" or ".jpg" or ".jpeg" or ".tga" or ".bmp" => AssetType.Texture,
                ".obj" or ".fbx" or ".gltf" => AssetType.Mesh,
                ".wav" or ".mp3" or ".ogg" => AssetType.Audio,
                ".cs" => AssetType.Script,
                ".shader" => AssetType.Shader,
                ".anim" or ".controller" => AssetType.Animation,
                ".ttf" or ".otf" => AssetType.Font,
                _ => AssetType.Unknown
            };
        }

        public void SelectAsset(AssetItem asset)
        {
            SelectedAsset = asset;
            OnAssetSelected?.Invoke(this, asset);
        }

        public void DoubleClickAsset(AssetItem asset)
        {
            if (asset.IsFolder)
            {
                NavigateToPath(asset.Path);
            }
            else
            {
                OnAssetDoubleClicked?.Invoke(this, asset);
            }
        }

        public void SearchAssets(string query)
        {
            SearchQuery = query;
            
            if (string.IsNullOrWhiteSpace(query))
            {
                RefreshAssets();
                return;
            }

            var searchResults = new List<AssetItem>();
            SearchInDirectory(RootPath, query.ToLower(), searchResults);
            CurrentAssets = searchResults;
        }

        private void SearchInDirectory(string directory, string query, List<AssetItem> results)
        {
            if (!Directory.Exists(directory))
                return;

            foreach (var dir in Directory.GetDirectories(directory))
            {
                var dirInfo = new DirectoryInfo(dir);
                if (dirInfo.Name.ToLower().Contains(query))
                {
                    results.Add(new AssetItem(dirInfo.Name, dir, AssetType.Folder));
                }
                SearchInDirectory(dir, query, results);
            }

            foreach (var file in Directory.GetFiles(directory))
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Name.ToLower().Contains(query))
                {
                    var type = GetAssetType(fileInfo.Extension);
                    results.Add(new AssetItem(fileInfo.Name, file, type));
                }
            }
        }

        public void CreateFolder(string name)
        {
            var newPath = Path.Combine(CurrentPath, name);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
                RefreshAssets();
            }
        }

        public void DeleteAsset(AssetItem asset)
        {
            try
            {
                if (asset.IsFolder)
                {
                    Directory.Delete(asset.Path, true);
                }
                else
                {
                    File.Delete(asset.Path);
                }
                RefreshAssets();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting asset: {ex.Message}");
            }
        }

        public void RenameAsset(AssetItem asset, string newName)
        {
            try
            {
                var newPath = Path.Combine(Path.GetDirectoryName(asset.Path), newName);
                if (asset.IsFolder)
                {
                    Directory.Move(asset.Path, newPath);
                }
                else
                {
                    File.Move(asset.Path, newPath);
                }
                RefreshAssets();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming asset: {ex.Message}");
            }
        }

        public void SetViewMode(AssetViewMode mode)
        {
            ViewMode = mode;
        }
    }

    public enum AssetViewMode { Grid, List, Column }
}
