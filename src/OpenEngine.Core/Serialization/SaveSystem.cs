// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenEngine.Core.Serialization
{
    public class SaveSystem
    {
        private string _saveDirectory;
        private Dictionary<string, SaveData> _saveSlots;
        private JsonSerializerOptions _serializerOptions;

        public string SaveDirectory => _saveDirectory;
        public Dictionary<string, SaveData> SaveSlots => _saveSlots;

        public SaveSystem()
        {
            _saveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenEngine", "Saves");
            _saveSlots = new Dictionary<string, SaveData>();
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }

            LoadSaveSlots();
        }

        public void SetSaveDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                _saveDirectory = path;
            }
            else
            {
                Directory.CreateDirectory(path);
                _saveDirectory = path;
            }
        }

        public bool SaveGame(string slotName, SaveData data)
        {
            try
            {
                var filePath = Path.Combine(_saveDirectory, $"{slotName}.save");
                var jsonData = JsonSerializer.Serialize(data, _serializerOptions);
                File.WriteAllText(filePath, jsonData);

                _saveSlots[slotName] = data;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save game: {ex.Message}");
                return false;
            }
        }

        public SaveData? LoadGame(string slotName)
        {
            try
            {
                var filePath = Path.Combine(_saveDirectory, $"{slotName}.save");
                if (!File.Exists(filePath))
                    return null;

                var jsonData = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<SaveData>(jsonData, _serializerOptions);
                
                if (data != null)
                {
                    _saveSlots[slotName] = data;
                }

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load game: {ex.Message}");
                return null;
            }
        }

        public bool DeleteSave(string slotName)
        {
            try
            {
                var filePath = Path.Combine(_saveDirectory, $"{slotName}.save");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                _saveSlots.Remove(slotName);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete save: {ex.Message}");
                return false;
            }
        }

        public bool SaveExists(string slotName)
        {
            return _saveSlots.ContainsKey(slotName) || 
                   File.Exists(Path.Combine(_saveDirectory, $"{slotName}.save"));
        }

        public List<string> GetSaveSlots()
        {
            return new List<string>(_saveSlots.Keys);
        }

        private void LoadSaveSlots()
        {
            _saveSlots.Clear();

            if (!Directory.Exists(_saveDirectory))
                return;

            foreach (var file in Directory.GetFiles(_saveDirectory, "*.save"))
            {
                var slotName = Path.GetFileNameWithoutExtension(file);
                var data = LoadGame(slotName);
                if (data != null)
                {
                    _saveSlots[slotName] = data;
                }
            }
        }

        public void CreateAutoSave(SaveData data)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            SaveGame($"AutoSave_{timestamp}", data);
        }

        public void CreateQuickSave(SaveData data)
        {
            SaveGame("QuickSave", data);
        }

        public SaveData? LoadQuickSave()
        {
            return LoadGame("QuickSave");
        }

        public void ClearAllSaves()
        {
            foreach (var slotName in _saveSlots.Keys)
            {
                DeleteSave(slotName);
            }
        }
    }

    public class SaveData
    {
        public string SlotName { get; set; }
        public DateTime SaveTime { get; set; }
        public string SceneName { get; set; }
        public Dictionary<string, object> EntityData { get; set; }
        public Dictionary<string, object> GlobalData { get; set; }
        public string ScreenshotPath { get; set; }
        public int PlayTime { get; set; }
        public string Version { get; set; }

        public SaveData()
        {
            SlotName = "";
            SaveTime = DateTime.UtcNow;
            SceneName = "";
            EntityData = new Dictionary<string, object>();
            GlobalData = new Dictionary<string, object>();
            ScreenshotPath = "";
            PlayTime = 0;
            Version = "1.0.0";
        }
    }
}
