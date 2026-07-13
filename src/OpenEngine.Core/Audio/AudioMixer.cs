// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Audio
{
    public class AudioMixer
    {
        public string Name { get; set; }
        public List<AudioMixerGroup> Groups { get; set; }
        public float MasterVolume { get; set; }
        public int SampleRate { get; set; }

        public AudioMixer()
        {
            Groups = new List<AudioMixerGroup>();
            MasterVolume = 1f;
            SampleRate = 48000;
        }

        public AudioMixerGroup FindGroup(string name)
        {
            return Groups.Find(g => g.Name == name);
        }

        public AudioMixerGroup CreateGroup(string name)
        {
            var group = new AudioMixerGroup(name);
            Groups.Add(group);
            return group;
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = Math.Clamp(volume, 0f, 1f);
        }

        public void ClearGroups()
        {
            Groups.Clear();
        }
    }

    public class AudioMixerGroup
    {
        public string Name { get; set; }
        public AudioMixer AudioMixer { get; set; }
        public List<AudioMixerEffect> Effects { get; set; }
        public float Volume { get; set; }

        public AudioMixerGroup(string name)
        {
            Name = name;
            Effects = new List<AudioMixerEffect>();
            Volume = 1f;
        }

        public void AddEffect(AudioMixerEffect effect)
        {
            if (effect != null)
            {
                Effects.Add(effect);
            }
        }

        public void SetVolume(float volume)
        {
            Volume = Math.Clamp(volume, 0f, 1f);
        }

        public void RemoveEffect(AudioMixerEffect effect)
        {
            Effects.Remove(effect);
        }
    }

    public enum AudioMixerEffectType
    {
        LowPass,
        HighPass,
        Reverb,
        Echo,
        Distortion,
        Chorus,
        Flanger,
        Compressor,
        Equalizer,
        PitchShifter
    }

    public class AudioMixerEffect
    {
        public AudioMixerEffectType Type { get; set; }
        public Dictionary<string, float> Parameters { get; set; }
        public bool Enabled { get; set; }

        public AudioMixerEffect(AudioMixerEffectType type)
        {
            Type = type;
            Parameters = new Dictionary<string, float>();
            Enabled = true;
        }

        public void SetParameter(string name, float value)
        {
            Parameters[name] = value;
        }

        public float GetParameter(string name)
        {
            return Parameters.ContainsKey(name) ? Parameters[name] : 0f;
        }
    }
}
