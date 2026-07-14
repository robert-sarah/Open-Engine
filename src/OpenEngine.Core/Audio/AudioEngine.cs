// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenEngine.Core.Audio
{
    // On conserve uniquement les énumérations qui ne sont pas forcément dupliquées 
    // ailleurs ou qui servent de référence globale.
    public enum AudioFormat { WAV, MP3, OGG, FLAC, AAC }
    public enum AudioType { Music, SFX, Voice, Ambient, UI }

    public class AudioEngine
    {
        private static AudioEngine _instance;
        public static AudioEngine Instance => _instance ?? (_instance = new AudioEngine());

        // Utilisation des classes métier définies dans leurs fichiers propres
        public List<AudioSource> Sources { get; set; }
        public List<AudioClip> Clips { get; set; }
        public AudioListener Listener { get; set; }
        public AudioMixer MasterMixer { get; set; }
        public int MaxSources { get; set; }
        public bool IsInitialized { get; set; }

        private AudioEngine()
        {
            Sources = new List<AudioSource>();
            Clips = new List<AudioClip>();
            Listener = new AudioListener();
            MasterMixer = new AudioMixer();
            MaxSources = 32;
            IsInitialized = false;
        }

        public bool Initialize()
        {
            MasterMixer.AddGroup("Music", new AudioMixerGroup { Name = "Music", Volume = 0.8f });
            MasterMixer.AddGroup("SFX", new AudioMixerGroup { Name = "SFX", Volume = 1.0f });
            MasterMixer.AddGroup("Voice", new AudioMixerGroup { Name = "Voice", Volume = 1.0f });
            MasterMixer.AddGroup("Ambient", new AudioMixerGroup { Name = "Ambient", Volume = 0.6f });
            MasterMixer.AddGroup("UI", new AudioMixerGroup { Name = "UI", Volume = 1.0f });

            IsInitialized = true;
            return true;
        }

        public void Shutdown()
        {
            foreach (var source in Sources) source.Stop();
            Sources.Clear();
            Clips.Clear();
            IsInitialized = false;
        }

        public AudioSource CreateSource(string name)
        {
            if (Sources.Count >= MaxSources) return null;
            var source = new AudioSource { Name = name };
            Sources.Add(source);
            return source;
        }

        public void RemoveSource(AudioSource source)
        {
            source.Stop();
            Sources.Remove(source);
        }

        public AudioClip LoadClip(string filePath, AudioFormat format = AudioFormat.WAV)
        {
            var clip = new AudioClip { Name = System.IO.Path.GetFileNameWithoutExtension(filePath), FilePath = filePath, Format = format };
            if (clip.Load()) { Clips.Add(clip); return clip; }
            return null;
        }

        public void UnloadClip(AudioClip clip)
        {
            clip.Unload();
            Clips.Remove(clip);
        }

        public void PlayOneShot(AudioClip clip, float volume = 1.0f)
        {
            var source = CreateSource($"OneShot_{Guid.NewGuid()}");
            if (source != null) { source.SetClip(clip); source.Volume = volume; source.Play(); }
        }

        public void PlayAtPosition(AudioClip clip, Vector3 position, float volume = 1.0f)
        {
            var source = CreateSource($"3D_{Guid.NewGuid()}");
            if (source != null) { source.SetClip(clip); source.Volume = volume; source.Is3D = true; source.Position = position; source.Play(); }
        }

        public void Update(float deltaTime)
        {
            foreach (var source in Sources)
            {
                if (source.Is3D && source.IsPlaying) Update3DSource(source);
            }
        }

        private void Update3DSource(AudioSource source)
        {
            var distance = Vector3.Distance(source.Position, Listener.Position);
            var attenuation = CalculateAttenuation(distance, source.MinDistance, source.MaxDistance, source.RolloffMode);
            var doppler = CalculateDoppler(source.Position, source.Velocity, Listener.Position, Listener.Velocity);
            
            source.Volume *= attenuation;
            source.Pitch *= doppler;
        }

        private float CalculateAttenuation(float distance, float minDistance, float maxDistance, float rolloff)
        {
            if (distance <= minDistance) return 1.0f;
            if (distance >= maxDistance) return 0.0f;
            var normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);
            return 1.0f - (float)Math.Pow(normalizedDistance, rolloff);
        }

        private float CalculateDoppler(Vector3 sourcePos, Vector3 sourceVel, Vector3 listenerPos, Vector3 listenerVel)
        {
            const float speedOfSound = 343.0f;
            var sourceToListener = Vector3.Normalize(listenerPos - sourcePos);
            var sourceVelComponent = Vector3.Dot(sourceVel, sourceToListener);
            var listenerVelComponent = Vector3.Dot(listenerVel, sourceToListener);
            return Math.Clamp((speedOfSound + listenerVelComponent) / (speedOfSound + sourceVelComponent), 0.5f, 2.0f);
        }

        public void SetListenerPosition(Vector3 position) => Listener.Position = position;
        public void SetListenerOrientation(Vector3 forward, Vector3 up) { Listener.Forward = forward; Listener.Up = up; }
    }
}