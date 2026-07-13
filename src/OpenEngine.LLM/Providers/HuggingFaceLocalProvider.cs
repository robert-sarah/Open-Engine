// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenEngine.LLM.Providers
{
    public class HuggingFaceLocalProvider : ILLMProvider
    {
        public string ProviderName => "HuggingFaceLocal";
        public bool IsAvailable => _httpClient != null && _isInitialized;

        private HttpClient _httpClient;
        private bool _isInitialized;
        private readonly string _baseUrl;
        private readonly string _modelName;

        public HuggingFaceLocalProvider(string baseUrl = "http://localhost:5000", string modelName = "local-model")
        {
            _baseUrl = baseUrl;
            _modelName = modelName;
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

                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
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

            try
            {
                if (!_isInitialized)
                {
                    return GenerateMockResponse(prompt);
                }

                var request = new
                {
                    inputs = prompt,
                    parameters = new
                    {
                        temperature = modelSettings?.ContainsKey("temperature") == true
                            ? Convert.ToSingle(modelSettings["temperature"])
                            : 0.7,
                        max_new_tokens = modelSettings?.ContainsKey("max_tokens") == true
                            ? Convert.ToInt32(modelSettings["max_tokens"])
                            : 512,
                        return_full_text = false
                    }
                };

                string json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/generate", content);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception ex)
            {
                return GenerateMockResponse(prompt);
            }
        }

        private string GenerateMockResponse(string prompt)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Analyzing the simulation context...");
            sb.AppendLine();

            if (prompt.Contains("threat", StringComparison.OrdinalIgnoreCase)
                || prompt.Contains("danger", StringComparison.OrdinalIgnoreCase)
                || prompt.Contains("attack", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("⚠️ THREAT DETECTED");
                sb.AppendLine("Recommended actions:");
                sb.AppendLine("1. Move to defensive position");
                sb.AppendLine("2. Activate shield systems");
                sb.AppendLine("3. Notify allied units");
                sb.AppendLine("4. Assess retaliation options");
            }
            else if (prompt.Contains("trade", StringComparison.OrdinalIgnoreCase)
                     || prompt.Contains("resource", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("💹 ECONOMIC ANALYSIS");
                sb.AppendLine("Recommended actions:");
                sb.AppendLine("1. Survey local resource availability");
                sb.AppendLine("2. Establish trade routes");
                sb.AppendLine("3. Negotiate favorable terms");
            }
            else if (prompt.Contains("explore", StringComparison.OrdinalIgnoreCase)
                     || prompt.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine("🔍 EXPLORATION PROTOCOL");
                sb.AppendLine("Recommended actions:");
                sb.AppendLine("1. Send scout units");
                sb.AppendLine("2. Map the terrain");
                sb.AppendLine("3. Identify points of interest");
            }
            else
            {
                sb.AppendLine("📋 GENERAL ANALYSIS");
                sb.AppendLine("Recommended actions:");
                sb.AppendLine("1. Maintain current position");
                sb.AppendLine("2. Monitor surroundings");
                sb.AppendLine("3. Await further instructions");
            }

            return sb.ToString();
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
