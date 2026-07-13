// Created By Levi Enama
// Thread Manager for Background Systems (Physics, Audio, AI, Networking)
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenEngine.Core.Threading
{
    public class ThreadManager : IDisposable
    {
        private static ThreadManager _instance;
        private bool _disposed;

        // Physics thread
        private Thread _physicsThread;
        private bool _physicsRunning;
        private float _physicsDeltaTime;
        private Action<float> _physicsUpdate;

        // Audio thread
        private Thread _audioThread;
        private bool _audioRunning;
        private Action _audioUpdate;

        // AI thread
        private Thread _aiThread;
        private bool _aiRunning;
        private float _aiDeltaTime;
        private Action<float> _aiUpdate;

        // Networking thread
        private Thread _networkThread;
        private bool _networkRunning;
        private Action _networkUpdate;

        // Synchronization
        private readonly object _lock = new object();

        public static ThreadManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ThreadManager();
                }
                return _instance;
            }
        }

        private ThreadManager()
        {
            _disposed = false;
        }

        public void StartPhysicsThread(float fixedDeltaTime, Action<float> updateCallback)
        {
            lock (_lock)
            {
                if (_physicsRunning) return;

                _physicsDeltaTime = fixedDeltaTime;
                _physicsUpdate = updateCallback;
                _physicsRunning = true;

                _physicsThread = new Thread(PhysicsLoop)
                {
                    Name = "Physics Thread",
                    IsBackground = true
                };
                _physicsThread.Start();
            }
        }

        private void PhysicsLoop()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            long targetTicks = (long)(_physicsDeltaTime * 10000000);

            while (_physicsRunning)
            {
                stopwatch.Restart();

                _physicsUpdate?.Invoke(_physicsDeltaTime);

                long elapsed = stopwatch.ElapsedTicks;
                long remaining = targetTicks - elapsed;

                if (remaining > 0)
                {
                    Thread.SpinWait((int)(remaining / 100));
                }
            }
        }

        public void StopPhysicsThread()
        {
            lock (_lock)
            {
                _physicsRunning = false;
                _physicsThread?.Join();
                _physicsThread = null;
            }
        }

        public void StartAudioThread(Action updateCallback)
        {
            lock (_lock)
            {
                if (_audioRunning) return;

                _audioUpdate = updateCallback;
                _audioRunning = true;

                _audioThread = new Thread(AudioLoop)
                {
                    Name = "Audio Thread",
                    IsBackground = true
                };
                _audioThread.Start();
            }
        }

        private void AudioLoop()
        {
            while (_audioRunning)
            {
                _audioUpdate?.Invoke();
                Thread.Sleep(10); // 100 Hz for audio
            }
        }

        public void StopAudioThread()
        {
            lock (_lock)
            {
                _audioRunning = false;
                _audioThread?.Join();
                _audioThread = null;
            }
        }

        public void StartAIThread(float updateInterval, Action<float> updateCallback)
        {
            lock (_lock)
            {
                if (_aiRunning) return;

                _aiDeltaTime = updateInterval;
                _aiUpdate = updateCallback;
                _aiRunning = true;

                _aiThread = new Thread(AILoop)
                {
                    Name = "AI Thread",
                    IsBackground = true
                };
                _aiThread.Start();
            }
        }

        private void AILoop()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            long targetTicks = (long)(_aiDeltaTime * 10000000);

            while (_aiRunning)
            {
                stopwatch.Restart();

                _aiUpdate?.Invoke(_aiDeltaTime);

                long elapsed = stopwatch.ElapsedTicks;
                long remaining = targetTicks - elapsed;

                if (remaining > 0)
                {
                    Thread.Sleep((int)(remaining / 10000));
                }
            }
        }

        public void StopAIThread()
        {
            lock (_lock)
            {
                _aiRunning = false;
                _aiThread?.Join();
                _aiThread = null;
            }
        }

        public void StartNetworkThread(Action updateCallback)
        {
            lock (_lock)
            {
                if (_networkRunning) return;

                _networkUpdate = updateCallback;
                _networkRunning = true;

                _networkThread = new Thread(NetworkLoop)
                {
                    Name = "Network Thread",
                    IsBackground = true
                };
                _networkThread.Start();
            }
        }

        private void NetworkLoop()
        {
            while (_networkRunning)
            {
                _networkUpdate?.Invoke();
                Thread.Sleep(16); // ~60 Hz for networking
            }
        }

        public void StopNetworkThread()
        {
            lock (_lock)
            {
                _networkRunning = false;
                _networkThread?.Join();
                _networkThread = null;
            }
        }

        public void StopAll()
        {
            StopPhysicsThread();
            StopAudioThread();
            StopAIThread();
            StopNetworkThread();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopAll();
                _disposed = true;
            }
        }
    }
}
