// Created By Levi Enama
using System;
using OpenEngine.Core.Engine;

namespace OpenEngine.Editor.Panels
{
    public class GameView
    {
        public OpenSimulationEngine Engine { get; }
        public bool IsPlaying { get; private set; }
        public bool IsPaused { get; private set; }
        public float TimeScale { get; set; }
        public int MaximizeOnPlay { get; set; }
        public bool AudioEnabled { get; set; }
        public bool GizmosEnabled { get; set; }
        public int TargetFramerate { get; set; }

        public event EventHandler OnPlayModeChanged;
        public event EventHandler OnPauseChanged;

        public GameView(OpenSimulationEngine engine)
        {
            Engine = engine;
            TimeScale = 1f;
            MaximizeOnPlay = 0;
            AudioEnabled = true;
            GizmosEnabled = false;
            TargetFramerate = 60;
        }

        public void Play()
        {
            if (!IsPlaying)
            {
                IsPlaying = true;
                IsPaused = false;
                Engine.Initialize();
                OnPlayModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                IsPaused = !IsPaused;
                OnPauseChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Stop()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                IsPaused = false;
                Engine.Shutdown();
                TimeScale = 1f;
                OnPlayModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Step()
        {
            if (IsPlaying && IsPaused)
            {
                Engine.Update(1f / TargetFramerate);
            }
        }

        public void SetTimeScale(float scale)
        {
            TimeScale = Math.Max(0f, scale);
        }

        public void Update(float deltaTime)
        {
            if (IsPlaying && !IsPaused)
            {
                Engine.Update(deltaTime * TimeScale);
            }
        }

        public void SetTargetFramerate(int framerate)
        {
            TargetFramerate = Math.Max(1, framerate);
        }

        public void ToggleAudio()
        {
            AudioEnabled = !AudioEnabled;
        }

        public void ToggleGizmos()
        {
            GizmosEnabled = !GizmosEnabled;
        }

        public string GetPlayModeStatus()
        {
            if (!IsPlaying) return "Stopped";
            if (IsPaused) return "Paused";
            return "Playing";
        }
    }
}
