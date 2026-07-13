// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Audio
{
    public enum AudioFormat
    {
        WAV,
        MP3,
        OGG,
        FLAC,
        AAC
    }

    public enum AudioType
    {
        Music,
        SFX,
        Voice,
        Ambient,
        UI
    }

    public class AudioClip
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public AudioFormat Format { get; set; }
        public AudioType Type { get; set; }
        public float Duration { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public bool Is3D { get; set; }
        public bool IsLooping { get; set; }
        public bool IsLoaded { get; set; }

        public AudioClip()
        {
            Format = AudioFormat.WAV;
            Type = AudioType.SFX;
            Is3D = false;
            IsLooping = false;
            IsLoaded = false;
        }

        public virtual bool Load()
        {
            // Base implementation
            IsLoaded = true;
            return true;
        }

        public virtual void Unload()
        {
            IsLoaded = false;
        }
    }

    public class AudioSource
    {
        public string Name { get; set; }
        public AudioClip Clip { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public bool IsLooping { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public float Pan { get; set; }
        public float Priority { get; set; }
        public bool Is3D { get; set; }
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public float RolloffMode { get; set; }
        public System.Numerics.Vector3 Position { get; set; }
        public System.Numerics.Vector3 Velocity { get; set; }

        public AudioSource()
        {
            Volume = 1.0f;
            Pitch = 1.0f;
            Pan = 0.0f;
            Priority = 128;
            Is3D = false;
            MinDistance = 1.0f;
            MaxDistance = 500.0f;
            RolloffMode = 1.0f;
            Position = System.Numerics.Vector3.Zero;
            Velocity = System.Numerics.Vector3.Zero;
        }

        public virtual void Play()
        {
            if (Clip == null) return;
            IsPlaying = true;
            IsPaused = false;
        }

        public virtual void Stop()
        {
            IsPlaying = false;
            IsPaused = false;
        }

        public virtual void Pause()
        {
            IsPaused = true;
        }

        public virtual void Resume()
        {
            IsPaused = false;
        }

        public virtual void SetClip(AudioClip clip)
        {
            Clip = clip;
        }
    }

    public class AudioListener
    {
        public System.Numerics.Vector3 Position { get; set; }
        public System.Numerics.Vector3 Velocity { get; set; }
        public System.Numerics.Vector3 Forward { get; set; }
        public System.Numerics.Vector3 Up { get; set; }

        public AudioListener()
        {
            Position = System.Numerics.Vector3.Zero;
            Velocity = System.Numerics.Vector3.Zero;
            Forward = System.Numerics.Vector3.UnitZ;
            Up = System.Numerics.Vector3.UnitY;
        }
    }

    public class AudioMixer
    {
        public string Name { get; set; }
        public Dictionary<string, AudioMixerGroup> Groups { get; set; }
        public float MasterVolume { get; set; }

        public AudioMixer()
        {
            Groups = new Dictionary<string, AudioMixerGroup>();
            MasterVolume = 1.0f;
        }

        public void AddGroup(string name, AudioMixerGroup group)
        {
            Groups[name] = group;
        }

        public AudioMixerGroup GetGroup(string name)
        {
            return Groups.TryGetValue(name, out var group) ? group : null;
        }

        public void SetGroupVolume(string name, float volume)
        {
            if (Groups.TryGetValue(name, out var group))
            {
                group.Volume = volume;
            }
        }
    }

    public class AudioMixerGroup
    {
        public string Name { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool Mute { get; set; }
        public bool BypassEffects { get; set; }
        public List<AudioEffect> Effects { get; set; }

        public AudioMixerGroup()
        {
            Volume = 1.0f;
            Pitch = 1.0f;
            Mute = false;
            BypassEffects = false;
            Effects = new List<AudioEffect>();
        }

        public void AddEffect(AudioEffect effect)
        {
            Effects.Add(effect);
        }

        public void RemoveEffect(AudioEffect effect)
        {
            Effects.Remove(effect);
        }
    }

    public class AudioEffect
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public Dictionary<string, float> Parameters { get; set; }

        public AudioEffect()
        {
            IsEnabled = true;
            Parameters = new Dictionary<string, float>();
        }

        public virtual void Apply()
        {
            // Base implementation
        }
    }

    public class ReverbEffect : AudioEffect
    {
        public float RoomSize { get; set; }
        public float Damping { get; set; }
        public float WetLevel { get; set; }
        public float DryLevel { get; set; }
        public float DecayTime { get; set; }

        public ReverbEffect()
        {
            Name = "Reverb";
            RoomSize = 0.5f;
            Damping = 0.5f;
            WetLevel = 0.3f;
            DryLevel = 0.7f;
            DecayTime = 1.5f;
        }
    }

    public class ChorusEffect : AudioEffect
    {
        public float Rate { get; set; }
        public float Depth { get; set; }
        public float Feedback { get; set; }
        public float Delay { get; set; }

        public ChorusEffect()
        {
            Name = "Chorus";
            Rate = 1.0f;
            Depth = 0.5f;
            Feedback = 0.25f;
            Delay = 0.01f;
        }
    }

    public class DistortionEffect : AudioEffect
    {
        public float Level { get; set; }
        public float Edge { get; set; }
        public float Gain { get; set; }
        public float LowPassFilter { get; set; }

        public DistortionEffect()
        {
            Name = "Distortion";
            Level = 0.5f;
            Edge = 0.5f;
            Gain = 0.0f;
            LowPassFilter = 8000.0f;
        }
    }

    public class AudioEngine
    {
        private static AudioEngine _instance;
        public static AudioEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AudioEngine();
                }
                return _instance;
            }
        }

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
            // Initialize default mixer groups
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
            foreach (var source in Sources)
            {
                source.Stop();
            }
            Sources.Clear();
            Clips.Clear();
            IsInitialized = false;
        }

        public AudioSource CreateSource(string name)
        {
            if (Sources.Count >= MaxSources)
            {
                Console.WriteLine("Maximum audio sources reached");
                return null;
            }

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
            var clip = new AudioClip
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath,
                Format = format
            };

            if (clip.Load())
            {
                Clips.Add(clip);
                return clip;
            }

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
            if (source != null)
            {
                source.SetClip(clip);
                source.Volume = volume;
                source.Play();
            }
        }

        public void PlayAtPosition(AudioClip clip, System.Numerics.Vector3 position, float volume = 1.0f)
        {
            var source = CreateSource($"3D_{Guid.NewGuid()}");
            if (source != null)
            {
                source.SetClip(clip);
                source.Volume = volume;
                source.Is3D = true;
                source.Position = position;
                source.Play();
            }
        }

        public void Update(float deltaTime)
        {
            // Update 3D audio
            foreach (var source in Sources)
            {
                if (source.Is3D && source.IsPlaying)
                {
                    Update3DSource(source);
                }
            }
        }

        private void Update3DSource(AudioSource source)
        {
            // Calculate distance-based attenuation
            var distance = System.Numerics.Vector3.Distance(source.Position, Listener.Position);
            var attenuation = CalculateAttenuation(distance, source.MinDistance, source.MaxDistance, source.RolloffMode);
            
            // Apply Doppler effect
            var doppler = CalculateDoppler(source.Position, source.Velocity, Listener.Position, Listener.Velocity);
            
            // Apply to source
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

        private float CalculateDoppler(System.Numerics.Vector3 sourcePos, System.Numerics.Vector3 sourceVel,
            System.Numerics.Vector3 listenerPos, System.Numerics.Vector3 listenerVel)
        {
            const float speedOfSound = 343.0f;
            
            var sourceToListener = System.Numerics.Vector3.Normalize(listenerPos - sourcePos);
            var sourceVelComponent = System.Numerics.Vector3.Dot(sourceVel, sourceToListener);
            var listenerVelComponent = System.Numerics.Vector3.Dot(listenerVel, sourceToListener);
            
            var dopplerFactor = (speedOfSound + listenerVelComponent) / (speedOfSound + sourceVelComponent);
            return Math.Clamp(dopplerFactor, 0.5f, 2.0f);
        }

        public void SetListenerPosition(System.Numerics.Vector3 position)
        {
            Listener.Position = position;
        }

        public void SetListenerOrientation(System.Numerics.Vector3 forward, System.Numerics.Vector3 up)
        {
            Listener.Forward = forward;
            Listener.Up = up;
        }
    }
}
