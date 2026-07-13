// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenEngine.LLM.Providers
{
    public class OllamaProvider : ILLMProvider
    {
        public string ProviderName => "Ollama";
        public bool IsAvailable => _httpClient != null && _isInitialized;

        private HttpClient _httpClient;
        private bool _isInitialized;
        private readonly string _baseUrl;
        private readonly string _defaultModel;

        public OllamaProvider(string baseUrl = "http://localhost:11434", string defaultModel = "llama2")
        {
            _baseUrl = baseUrl;
            _defaultModel = defaultModel;
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                _httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(5)
                };

                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
                _isInitialized = response.IsSuccessStatusCode;
            }
            catch
            {
                _isInitialized = false;
            }
        }

        public async Task<string> GenerateResponseAsync(string prompt, Dictionary<string, object> modelSettings = null)
        {
            if (!_isInitialized)
                await InitializeAsync();

            if (!_isInitialized)
                throw new InvalidOperationException("Ollama provider is not available");

            try
            {
                var request = new
                {
                    model = modelSettings?.ContainsKey("model") == true
                        ? modelSettings["model"].ToString()
                        : _defaultModel,
                    prompt = prompt,
                    stream = false,
                    options = new
                    {
                        temperature = modelSettings?.ContainsKey("temperature") == true
                            ? Convert.ToSingle(modelSettings["temperature"])
                            : 0.7f,
                        num_predict = modelSettings?.ContainsKey("max_tokens") == true
                            ? Convert.ToInt32(modelSettings["max_tokens"])
                            : 512
                    }
                };

                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("response", out JsonElement responseElement))
                {
                    return responseElement.GetString();
                }

                return jsonResponse;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task ShutdownAsync()
        {
            _httpClient?.Dispose();
            _httpClient = null;
            _isInitialized = false;
            await Task.CompletedTask;
        }
    }
}
