// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Threading;
using OpenEngine.Core.Engine;

namespace OpenEngine.Editor.Workspace
{
    public class EditorWorkspace
    {
        public OpenSimulationEngine Engine { get; }
        public string WorkspaceName { get; set; }
        public bool IsRunning { get; private set; }

        private Thread _simulationThread;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler OnWorkspaceInitialized;
        public event EventHandler<string> OnLogMessage;

        public EditorWorkspace(string workspaceName = "Open Engine")
        {
            WorkspaceName = workspaceName;
            Engine = new OpenSimulationEngine(60.0f);
        }

        public void Initialize()
        {
            LogMessage($"Initializing {WorkspaceName}...");
            Engine.Initialize();
            IsRunning = true;
            OnWorkspaceInitialized?.Invoke(this, EventArgs.Empty);
            LogMessage("Workspace initialized successfully!");
        }

        public void StartSimulationLoop()
        {
            if (_simulationThread != null && _simulationThread.IsAlive)
                return;

            _cancellationTokenSource = new CancellationTokenSource();
            _simulationThread = new Thread(() => SimulationLoop(_cancellationTokenSource.Token))
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _simulationThread.Start();
            LogMessage("Simulation loop started");
        }

        private void SimulationLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && Engine.IsRunning)
            {
                Engine.Update();
                Thread.Sleep(16);
            }
        }

        public void StopSimulationLoop()
        {
            _cancellationTokenSource?.Cancel();
            _simulationThread?.Join(TimeSpan.FromSeconds(2));
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            LogMessage("Simulation loop stopped");
        }

        public void Shutdown()
        {
            StopSimulationLoop();
            Engine.Shutdown();
            IsRunning = false;
            LogMessage("Workspace shutdown complete");
        }

        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke(this, $"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
