// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenEngine.LLM.Providers
{
    public class LlamaCppProvider : ILLMProvider
    {
        public string ProviderName => "LlamaCpp";
        public bool IsAvailable => _isInitialized;

        private bool _isInitialized;
        private readonly string _modelPath;
        private readonly string _executablePath;
        private readonly string _serverUrl;
        private Process _serverProcess;
        private readonly HttpClient _httpClient;

        public LlamaCppProvider(string modelPath, string executablePath = "llama-server", string serverUrl = "http://localhost:8080")
        {
            _modelPath = modelPath;
            _executablePath = executablePath;
            _serverUrl = serverUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                if (!File.Exists(_modelPath))
                {
                    _isInitialized = false;
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = _executablePath,
                    Arguments = $"-m \"{_modelPath}\" -c 4096 --port 8080",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _serverProcess = Process.Start(startInfo);
                await Task.Delay(3000); // Wait for server to start

                _isInitialized = _serverProcess != null && !_serverProcess.HasExited;
            }
            catch
            {
                _isInitialized = false;
            }
        }

        public async Task<string> GenerateResponseAsync(string prompt, Dictionary<string, object> modelSettings = null)
        {
            if (!_isInitialized)
            {
                return "LlamaCpp provider not available. Please ensure the model is accessible.";
            }

            try
            {
                var temperature = 0.7f;
                var maxTokens = 512;

                if (modelSettings != null)
                {
                    if (modelSettings.ContainsKey("temperature"))
                        temperature = Convert.ToSingle(modelSettings["temperature"]);
                    if (modelSettings.ContainsKey("max_tokens"))
                        maxTokens = Convert.ToInt32(modelSettings["max_tokens"]);
                }

                var requestBody = new
                {
                    prompt = prompt,
                    n_predict = maxTokens,
                    temperature = temperature,
                    stream = false
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_serverUrl}/completion", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return $"Error: Server returned {response.StatusCode}";
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LlamaCppResponse>(responseJson);
                
                return result?.content ?? "No response generated";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                    await _serverProcess.WaitForExitAsync();
                }
            }
            catch { }
            finally
            {
                _serverProcess?.Dispose();
                _serverProcess = null;
                _isInitialized = false;
                _httpClient?.Dispose();
            }
        }

        private class LlamaCppResponse
        {
            public string content { get; set; }
            public string stop { get; set; }
            public int tokens_predicted { get; set; }
            public int tokens_evaluated { get; set; }
        }
    }
}
