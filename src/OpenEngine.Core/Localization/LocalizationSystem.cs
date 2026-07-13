// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace OpenEngine.Core.Localization
{
    public class LocalizationSystem
    {
        private Dictionary<string, Dictionary<string, string>> _localizedStrings;
        private string _currentLanguage;
        private string _defaultLanguage;
        private string _localizationDirectory;
        private JsonSerializerOptions _serializerOptions;

        public string CurrentLanguage => _currentLanguage;
        public List<string> AvailableLanguages { get; private set; }

        public event Action<string> OnLanguageChanged;

        public LocalizationSystem()
        {
            _localizedStrings = new Dictionary<string, Dictionary<string, string>>();
            _currentLanguage = "en";
            _defaultLanguage = "en";
            _localizationDirectory = "Assets/Localization";
            AvailableLanguages = new List<string>();
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (!Directory.Exists(_localizationDirectory))
            {
                Directory.CreateDirectory(_localizationDirectory);
            }

            LoadAvailableLanguages();
        }

        public void SetLocalizationDirectory(string path)
        {
            _localizationDirectory = path;
            if (!Directory.Exists(_localizationDirectory))
            {
                Directory.CreateDirectory(_localizationDirectory);
            }
            LoadAvailableLanguages();
        }

        public void SetLanguage(string languageCode)
        {
            if (_localizedStrings.ContainsKey(languageCode))
            {
                _currentLanguage = languageCode;
                CultureInfo.CurrentCulture = new CultureInfo(languageCode);
                CultureInfo.CurrentUICulture = new CultureInfo(languageCode);
                OnLanguageChanged?.Invoke(languageCode);
            }
        }

        public void SetDefaultLanguage(string languageCode)
        {
            _defaultLanguage = languageCode;
        }

        public string Localize(string key, params object[] args)
        {
            if (_localizedStrings.ContainsKey(_currentLanguage) && 
                _localizedStrings[_currentLanguage].ContainsKey(key))
            {
                var localized = _localizedStrings[_currentLanguage][key];
                return args.Length > 0 ? string.Format(localized, args) : localized;
            }

            if (_localizedStrings.ContainsKey(_defaultLanguage) && 
                _localizedStrings[_defaultLanguage].ContainsKey(key))
            {
                var localized = _localizedStrings[_defaultLanguage][key];
                return args.Length > 0 ? string.Format(localized, args) : localized;
            }

            return key;
        }

        public void AddLocalizedString(string languageCode, string key, string value)
        {
            if (!_localizedStrings.ContainsKey(languageCode))
            {
                _localizedStrings[languageCode] = new Dictionary<string, string>();
            }
            _localizedStrings[languageCode][key] = value;
        }

        public bool LoadLanguageFile(string languageCode)
        {
            var filePath = Path.Combine(_localizationDirectory, $"{languageCode}.json");
            if (!File.Exists(filePath))
                return false;

            try
            {
                var jsonData = File.ReadAllText(filePath);
                var strings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData, _serializerOptions);
                
                if (strings != null)
                {
                    _localizedStrings[languageCode] = strings;
                    if (!AvailableLanguages.Contains(languageCode))
                    {
                        AvailableLanguages.Add(languageCode);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load language file: {ex.Message}");
            }

            return false;
        }

        public bool SaveLanguageFile(string languageCode)
        {
            if (!_localizedStrings.ContainsKey(languageCode))
                return false;

            try
            {
                var filePath = Path.Combine(_localizationDirectory, $"{languageCode}.json");
                var jsonData = JsonSerializer.Serialize(_localizedStrings[languageCode], _serializerOptions);
                File.WriteAllText(filePath, jsonData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save language file: {ex.Message}");
                return false;
            }
        }

        private void LoadAvailableLanguages()
        {
            AvailableLanguages.Clear();
            
            if (!Directory.Exists(_localizationDirectory))
                return;

            foreach (var file in Directory.GetFiles(_localizationDirectory, "*.json"))
            {
                var languageCode = Path.GetFileNameWithoutExtension(file);
                if (!AvailableLanguages.Contains(languageCode))
                {
                    AvailableLanguages.Add(languageCode);
                    LoadLanguageFile(languageCode);
                }
            }
        }

        public void ReloadAllLanguages()
        {
            LoadAvailableLanguages();
        }

        public Dictionary<string, string> GetAllStringsForLanguage(string languageCode)
        {
            return _localizedStrings.ContainsKey(languageCode) 
                ? new Dictionary<string, string>(_localizedStrings[languageCode]) 
                : new Dictionary<string, string>();
        }

        public bool HasKey(string key)
        {
            return _localizedStrings.ContainsKey(_currentLanguage) && 
                   _localizedStrings[_currentLanguage].ContainsKey(key);
        }

        public bool HasKeyInLanguage(string key, string languageCode)
        {
            return _localizedStrings.ContainsKey(languageCode) && 
                   _localizedStrings[languageCode].ContainsKey(key);
        }

        public void RemoveKey(string key)
        {
            if (_localizedStrings.ContainsKey(_currentLanguage))
            {
                _localizedStrings[_currentLanguage].Remove(key);
            }
        }

        public void ClearLanguage(string languageCode)
        {
            if (_localizedStrings.ContainsKey(languageCode))
            {
                _localizedStrings[languageCode].Clear();
            }
        }
    }

    public class LocalizedString
    {
        private string _key;
        private LocalizationSystem _localization;

        public LocalizedString(string key, LocalizationSystem localization)
        {
            _key = key;
            _localization = localization;
        }

        public override string ToString()
        {
            return _localization.Localize(_key);
        }

        public static implicit operator string(LocalizedString localized)
        {
            return localized?.ToString() ?? "";
        }
    }
}
