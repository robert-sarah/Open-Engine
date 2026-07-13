// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Audio
{
    public class AudioManager
    {
        private Dictionary<string, AudioSource> _audioSources;
        private List<AudioListener> _listeners;
        private AudioMixer _masterMixer;
        private float _globalVolume;
        private int _maxPlayingSources;

        public float GlobalVolume => _globalVolume;
        public AudioMixer MasterMixer => _masterMixer;

        public AudioManager()
        {
            _audioSources = new Dictionary<string, AudioSource>();
            _listeners = new List<AudioListener>();
            _masterMixer = new AudioMixer();
            _globalVolume = 1f;
            _maxPlayingSources = 32;
        }

        public void RegisterAudioSource(AudioSource source)
        {
            if (source != null && !_audioSources.ContainsKey(source.EntityId))
            {
                _audioSources[source.EntityId] = source;
            }
        }

        public void UnregisterAudioSource(string entityId)
        {
            _audioSources.Remove(entityId);
        }

        public void AddListener(AudioListener listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void RemoveListener(AudioListener listener)
        {
            _listeners.Remove(listener);
        }

        public void SetMasterMixer(AudioMixer mixer)
        {
            _masterMixer = mixer ?? throw new ArgumentNullException(nameof(mixer));
        }

        public void SetGlobalVolume(float volume)
        {
            _globalVolume = Math.Clamp(volume, 0f, 1f);
        }

        public void Update(float deltaTime)
        {
            foreach (var source in _audioSources.Values)
            {
                source.Update(deltaTime);
            }

            // Apply 3D spatial audio calculations
            UpdateSpatialAudio();
        }

        private void UpdateSpatialAudio()
        {
            if (_listeners.Count == 0)
                return;

            var listener = _listeners[0];

            foreach (var source in _audioSources.Values)
            {
                if (source.SpatialBlend > 0f)
                {
                    var distance = Vector3.Distance(listener.Position, source.Position);
                    var attenuation = CalculateAttenuation(distance, source);
                    // Apply attenuation to source volume
                }
            }
        }

        private float CalculateAttenuation(float distance, AudioSource source)
        {
            return source.RolloffMode switch
            {
                AudioRolloffMode.Logarithmic => source.MinDistance / (source.MinDistance + distance),
                AudioRolloffMode.Linear => 1f - (distance - source.MinDistance) / (source.MaxDistance - source.MinDistance),
                _ => 1f
            };
        }

        public void PlaySoundAtPosition(string clipName, Vector3 position, float volume = 1f)
        {
            // Create temporary audio source for one-shot sound
        }

        public void StopAll()
        {
            foreach (var source in _audioSources.Values)
            {
                source.Stop();
            }
        }

        public void PauseAll()
        {
            foreach (var source in _audioSources.Values)
            {
                source.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (var source in _audioSources.Values)
            {
                source.Play();
            }
        }
    }
}
