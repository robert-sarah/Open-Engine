// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenEngine.Core.Prefabs
{
    public class Prefab
    {
        public string Name { get; set; }
        public string AssetPath { get; set; }
        public PrefabInstance Root { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public DateTime LastModified { get; set; }

        public Prefab()
        {
            Properties = new Dictionary<string, object>();
            LastModified = DateTime.UtcNow;
        }
    }

    public class PrefabInstance
    {
        public string EntityId { get; set; }
        public string PrefabId { get; set; }
        public Dictionary<string, object> Overrides { get; set; }
        public List<PrefabInstance> Children { get; set; }
        public bool IsPrefabInstance { get; set; }

        public PrefabInstance()
        {
            Overrides = new Dictionary<string, object>();
            Children = new List<PrefabInstance>();
            IsPrefabInstance = true;
        }
    }

    public class PrefabSystem
    {
        private Dictionary<string, Prefab> _prefabs;
        private Dictionary<string, PrefabInstance> _instances;
        private string _prefabDirectory;
        private JsonSerializerOptions _serializerOptions;

        public PrefabSystem()
        {
            _prefabs = new Dictionary<string, Prefab>();
            _instances = new Dictionary<string, PrefabInstance>();
            _prefabDirectory = "Assets/Prefabs";
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (!Directory.Exists(_prefabDirectory))
            {
                Directory.CreateDirectory(_prefabDirectory);
            }
        }

        public void SetPrefabDirectory(string path)
        {
            _prefabDirectory = path;
            if (!Directory.Exists(_prefabDirectory))
            {
                Directory.CreateDirectory(_prefabDirectory);
            }
        }

        public Prefab CreatePrefab(string name, PrefabInstance root)
        {
            var prefab = new Prefab
            {
                Name = name,
                AssetPath = Path.Combine(_prefabDirectory, $"{name}.prefab"),
                Root = root
            };

            _prefabs[name] = prefab;
            SavePrefab(prefab);
            return prefab;
        }

        public PrefabInstance Instantiate(string prefabName, string entityId)
        {
            if (!_prefabs.ContainsKey(prefabName))
            {
                Console.WriteLine($"Prefab '{prefabName}' not found");
                return null;
            }

            var prefab = _prefabs[prefabName];
            return ClonePrefabInstance(prefab.Root, entityId);
        }

        private PrefabInstance ClonePrefabInstance(PrefabInstance source, string newEntityId)
        {
            var instance = new PrefabInstance
            {
                EntityId = newEntityId,
                PrefabId = source.PrefabId,
                Overrides = new Dictionary<string, object>(source.Overrides),
                IsPrefabInstance = true
            };

            foreach (var child in source.Children)
            {
                var childId = Guid.NewGuid().ToString();
                instance.Children.Add(ClonePrefabInstance(child, childId));
            }

            _instances[newEntityId] = instance;
            return instance;
        }

        public bool SavePrefab(Prefab prefab)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(prefab, _serializerOptions);
                File.WriteAllText(prefab.AssetPath, jsonData);
                prefab.LastModified = DateTime.UtcNow;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save prefab: {ex.Message}");
                return false;
            }
        }

        public Prefab? LoadPrefab(string prefabName)
        {
            var path = Path.Combine(_prefabDirectory, $"{prefabName}.prefab");
            if (!File.Exists(path))
                return null;

            try
            {
                var jsonData = File.ReadAllText(path);
                var prefab = JsonSerializer.Deserialize<Prefab>(jsonData, _serializerOptions);
                if (prefab != null)
                {
                    _prefabs[prefabName] = prefab;
                }
                return prefab;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load prefab: {ex.Message}");
                return null;
            }
        }

        public bool ApplyPrefabOverrides(string instanceId, Dictionary<string, object> overrides)
        {
            if (!_instances.ContainsKey(instanceId))
                return false;

            foreach (var overrideData in overrides)
            {
                _instances[instanceId].Overrides[overrideData.Key] = overrideData.Value;
            }

            return true;
        }

        public void RevertPrefabOverrides(string instanceId)
        {
            if (_instances.ContainsKey(instanceId))
            {
                _instances[instanceId].Overrides.Clear();
            }
        }

        public void ApplyPrefabChanges(string instanceId)
        {
            if (!_instances.ContainsKey(instanceId))
                return;

            var instance = _instances[instanceId];
            var prefabName = instance.PrefabId;

            if (_prefabs.ContainsKey(prefabName))
            {
                var prefab = _prefabs[prefabName];
                prefab.Root = instance;
                SavePrefab(prefab);
            }
        }

        public bool DeletePrefab(string prefabName)
        {
            if (!_prefabs.ContainsKey(prefabName))
                return false;

            var prefab = _prefabs[prefabName];
            if (File.Exists(prefab.AssetPath))
            {
                File.Delete(prefab.AssetPath);
            }

            _prefabs.Remove(prefabName);
            return true;
        }

        public List<string> GetPrefabNames()
        {
            return new List<string>(_prefabs.Keys);
        }

        public Prefab? GetPrefab(string prefabName)
        {
            return _prefabs.ContainsKey(prefabName) ? _prefabs[prefabName] : null;
        }

        public PrefabInstance? GetInstance(string instanceId)
        {
            return _instances.ContainsKey(instanceId) ? _instances[instanceId] : null;
        }

        public void DestroyInstance(string instanceId)
        {
            _instances.Remove(instanceId);
        }
    }
}
