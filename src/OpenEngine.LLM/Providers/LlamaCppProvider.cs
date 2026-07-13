// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
        private Process _serverProcess;

        public LlamaCppProvider(string modelPath, string executablePath = "llama-server")
        {
            _modelPath = modelPath;
            _executablePath = executablePath;
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
                await Task.Delay(2000);

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

                var mockResponse = new StringBuilder();
                mockResponse.AppendLine("Based on your context, I recommend the following action:");
                mockResponse.AppendLine();
                mockResponse.AppendLine("1. Assess the immediate threat level");
                mockResponse.AppendLine("2. Move to a safer location if necessary");
                mockResponse.AppendLine("3. Gather more intelligence about the situation");
                mockResponse.AppendLine();
                mockResponse.AppendLine("Please choose your next action carefully.");

                await Task.Delay(100);
                return mockResponse.ToString();
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
            }
        }
    }
}
