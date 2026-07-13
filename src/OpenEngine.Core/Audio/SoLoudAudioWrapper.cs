// Created By Levi Enama
// SoLoud Audio Wrapper for Open Engine
using SoLoud;
using System.Numerics;

namespace OpenEngine.Core.Audio
{
    public class SoLoudAudioWrapper : IDisposable
    {
        private Soloud _soloud;
        private bool _disposed;

        public Soloud Soloud => _soloud;

        public SoLoudAudioWrapper()
        {
            _soloud = new Soloud();
        }

        public bool Initialize()
        {
            var result = _soloud.Init();
            return result == 0;
        }

        public void Update()
        {
            _soloud.Update();
        }

        public uint LoadAudio(string path)
        {
            var audio = new Wav();
            var result = audio.Load(path);
            if (result != 0)
            {
                return 0;
            }
            return _soloud.Play(audio);
        }

        public uint PlaySound(string path, float volume = 1f, float pan = 0f, bool paused = false)
        {
            var audio = new Wav();
            var result = audio.Load(path);
            if (result != 0)
            {
                return 0;
            }

            return _soloud.Play(audio, volume, pan, paused);
        }

        public uint Play3DSound(string path, Vector3 position, Vector3 velocity)
        {
            var audio = new Wav();
            var result = audio.Load(path);
            if (result != 0)
            {
                return 0;
            }

            var handle = _soloud.Play3d(audio, position.X, position.Y, position.Z);
            _soloud.Set3dSourceVelocity(handle, velocity.X, velocity.Y, velocity.Z);
            return handle;
        }

        public void StopSound(uint handle)
        {
            _soloud.Stop(handle);
        }

        public void StopAll()
        {
            _soloud.StopAll();
        }

        public void SetVolume(uint handle, float volume)
        {
            _soloud.SetVolume(handle, volume);
        }

        public void SetPan(uint handle, float pan)
        {
            _soloud.SetPan(handle, pan);
        }

        public void Set3dPosition(uint handle, Vector3 position)
        {
            _soloud.Set3dSourcePosition(handle, position.X, position.Y, position.Z);
        }

        public void Set3dVelocity(uint handle, Vector3 velocity)
        {
            _soloud.Set3dSourceVelocity(handle, velocity.X, velocity.Y, velocity.Z);
        }

        public void SetListenerPosition(Vector3 position, Vector3 velocity, Vector3 forward, Vector3 up)
        {
            _soloud.Set3dListenerPosition(position.X, position.Y, position.Z);
            _soloud.Set3dListenerVelocity(velocity.X, velocity.Y, velocity.Z);
            _soloud.Set3dListenerAt(forward.X, forward.Y, forward.Z);
            _soloud.Set3dListenerUp(up.X, up.Y, up.Z);
        }

        public void PauseSound(uint handle)
        {
            _soloud.SetPause(handle, true);
        }

        public void ResumeSound(uint handle)
        {
            _soloud.SetPause(handle, false);
        }

        public bool IsSoundPlaying(uint handle)
        {
            return _soloud.IsValidVoiceHandle(handle);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _soloud?.Deinit();
                _soloud?.Dispose();
                _disposed = true;
            }
        }
    }
}
