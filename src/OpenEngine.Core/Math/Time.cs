// Created By Levi Enama
using System;

namespace OpenEngine.Core.Math
{
    public static class Time
    {
        private static float _deltaTime;
        private static float _fixedDeltaTime = 0.02f;
        private static float _timeScale = 1f;
        private static float _timeSinceStartup;
        private static int _frameCount;

        public static float DeltaTime => _deltaTime * _timeScale;
        public static float UnscaledDeltaTime => _deltaTime;
        public static float FixedDeltaTime => _fixedDeltaTime * _timeScale;
        public static float UnscaledFixedDeltaTime => _fixedDeltaTime;
        public static float TimeScale => _timeScale;
        public static float TimeSinceStartup => _timeSinceStartup;
        public static int FrameCount => _frameCount;

        public static void Update(float deltaTime)
        {
            _deltaTime = deltaTime;
            _timeSinceStartup += deltaTime * _timeScale;
            _frameCount++;
        }

        public static void SetTimeScale(float scale)
        {
            _timeScale = System.Math.Clamp(scale, 0f, 10f);
        }

        public static void Reset()
        {
            _timeSinceStartup = 0f;
            _frameCount = 0;
            _timeScale = 1f;
        }
    }
}
