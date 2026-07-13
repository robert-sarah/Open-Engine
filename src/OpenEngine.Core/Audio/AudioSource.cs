// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Audio
{
    public enum AudioRolloffMode { Logarithmic, Linear, Custom }
    public enum AudioSpatialBlend { 2D, 3D }

    public class AudioSource
    {
        public string EntityId { get; set; }
        public AudioClip Clip { get; set; }
        public bool Loop { get; set; }
        public bool Mute { get; set; }
        public bool PlayOnAwake { get; set; }
        public int Priority { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public float StereoPan { get; set; }
        public float SpatialBlend { get; set; }
        public AudioRolloffMode RolloffMode { get; set; }
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public AnimationCurve RolloffCurve { get; set; }
        public Vector3 Position { get; set; }
        public bool IsPlaying { get; private set; }
        public float Time { get; set; }

        public AudioSource(string entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            Loop = false;
            Mute = false;
            PlayOnAwake = false;
            Priority = 128;
            Volume = 1f;
            Pitch = 1f;
            StereoPan = 0f;
            SpatialBlend = 0f;
            RolloffMode = AudioRolloffMode.Logarithmic;
            MinDistance = 1f;
            MaxDistance = 500f;
            RolloffCurve = new AnimationCurve();
            Position = Vector3.Zero;
            IsPlaying = false;
            Time = 0f;
        }

        public void Play()
        {
            if (Clip != null)
            {
                IsPlaying = true;
                Time = 0f;
            }
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Stop()
        {
            IsPlaying = false;
            Time = 0f;
        }

        public void PlayOneShot(AudioClip clip, float volumeScale = 1f)
        {
            if (clip != null)
            {
                // Play clip once without affecting current clip
            }
        }

        public void PlayDelayed(float delay)
        {
            // Schedule play after delay
        }

        public void PlayScheduled(double time)
        {
            // Schedule play at specific time
        }

        public void SetScheduledStartTime(double time)
        {
            // Set scheduled start time
        }

        public void SetSpatialBlend(float blend)
        {
            SpatialBlend = Math.Clamp(blend, 0f, 1f);
        }

        public void SetCurve(string type, AnimationCurve curve)
        {
            if (type == "CustomRolloff")
            {
                RolloffCurve = curve;
            }
        }

        public float GetVolume()
        {
            return Mute ? 0f : Volume;
        }

        public void Update(float deltaTime)
        {
            if (IsPlaying && Clip != null)
            {
                Time += deltaTime * Pitch;
                if (Time >= Clip.Length && !Loop)
                {
                    Stop();
                }
            }
        }
    }

    public class AudioClip
    {
        public string Name { get; set; }
        public float Length { get; set; }
        public int Channels { get; set; }
        public int Frequency { get; set; }
        public int Samples { get; set; }
        public bool LoadInBackground { get; set; }
        public bool Compressed { get; set; }

        public AudioClip()
        {
            Length = 0f;
            Channels = 2;
            Frequency = 44100;
            Samples = 0;
            LoadInBackground = false;
            Compressed = false;
        }
    }

    public class AnimationCurve
    {
        public AnimationCurve() { }
    }
}
