// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenEngine.LLM.Providers
{
    public enum ModelType
    {
        GPT,
        Nemotron,
        Ollama,
        LlamaCpp,
        HuggingFace,
        Custom
    }

    public class ModelConfig
    {
        public string ModelName { get; set; }
        public ModelType Type { get; set; }
        public string Endpoint { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool IsDefault { get; set; }

        public ModelConfig(string modelName, ModelType type, string endpoint = "")
        {
            ModelName = modelName ?? throw new ArgumentNullException(nameof(modelName));
            Type = type;
            Endpoint = endpoint;
            Parameters = new Dictionary<string, object>();
            IsDefault = false;
        }
    }

    public class ModelManager
    {
        private Dictionary<string, ILLMProvider> _providers;
        private Dictionary<string, ModelConfig> _modelConfigs;
        private string _defaultModelId;

        public Dictionary<string, ModelConfig> ModelConfigs => _modelConfigs;
        public string DefaultModelId => _defaultModelId;

        public ModelManager()
        {
            _providers = new Dictionary<string, ILLMProvider>();
            _modelConfigs = new Dictionary<string, ModelConfig>();
            _defaultModelId = "";
        }

        public void RegisterModel(string modelId, ModelConfig config, ILLMProvider provider)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            _modelConfigs[modelId] = config;
            _providers[modelId] = provider;

            if (config.IsDefault || string.IsNullOrEmpty(_defaultModelId))
            {
                _defaultModelId = modelId;
            }
        }

        public ILLMProvider? GetProvider(string modelId)
        {
            return _providers.ContainsKey(modelId) ? _providers[modelId] : null;
        }

        public ILLMProvider? GetDefaultProvider()
        {
            return !string.IsNullOrEmpty(_defaultModelId) ? GetProvider(_defaultModelId) : null;
        }

        public void SetDefaultModel(string modelId)
        {
            if (_providers.ContainsKey(modelId))
            {
                _defaultModelId = modelId;
                foreach (var config in _modelConfigs.Values)
                {
                    config.IsDefault = false;
                }
                _modelConfigs[modelId].IsDefault = true;
            }
        }

        public async Task<string> GenerateResponseAsync(string prompt, string? modelId = null)
        {
            var providerId = modelId ?? _defaultModelId;
            var provider = GetProvider(providerId);

            if (provider == null)
            {
                throw new InvalidOperationException($"No provider found for model: {providerId ?? "default"}");
            }

            var config = _modelConfigs[providerId];
            return await provider.GenerateResponseAsync(prompt, config.Parameters);
        }

        public List<string> GetAvailableModels()
        {
            return new List<string>(_modelConfigs.Keys);
        }

        public ModelConfig? GetModelConfig(string modelId)
        {
            return _modelConfigs.ContainsKey(modelId) ? _modelConfigs[modelId] : null;
        }

        public void UpdateModelParameters(string modelId, Dictionary<string, object> parameters)
        {
            if (_modelConfigs.ContainsKey(modelId))
            {
                foreach (var param in parameters)
                {
                    _modelConfigs[modelId].Parameters[param.Key] = param.Value;
                }
            }
        }

        public void RemoveModel(string modelId)
        {
            if (_modelConfigs.ContainsKey(modelId))
            {
                _providers[modelId]?.Shutdown();
                _providers.Remove(modelId);
                _modelConfigs.Remove(modelId);

                if (_defaultModelId == modelId)
                {
                    _defaultModelId = _modelConfigs.Count > 0 ? _modelConfigs.Keys.First() : "";
                }
            }
        }

        public void ShutdownAll()
        {
            foreach (var provider in _providers.Values)
            {
                provider.Shutdown();
            }
            _providers.Clear();
            _modelConfigs.Clear();
            _defaultModelId = "";
        }

        public string GetModelsSummary()
        {
            var summary = $"Available Models ({_modelConfigs.Count}):\n";
            foreach (var config in _modelConfigs.Values)
            {
                var isDefault = config.IsDefault ? " [DEFAULT]" : "";
                summary += $"- {config.ModelName} ({config.Type}){isDefault}\n";
                summary += $"  Endpoint: {config.Endpoint}\n";
            }
            return summary;
        }
    }
}
