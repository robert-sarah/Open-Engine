// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenEngine.LLM.Providers
{
    public interface ILLMProvider
    {
        string ProviderName { get; }
        bool IsAvailable { get; }
        Task<string> GenerateResponseAsync(string prompt, Dictionary<string, object> modelSettings = null);
        Task InitializeAsync();
        Task ShutdownAsync();
    }
}
