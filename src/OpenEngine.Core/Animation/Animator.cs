// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Animation
{
    public class Animator
    {
        public string AvatarId { get; set; }
        public Avatar Avatar { get; set; }
        public AnimationController RuntimeController { get; set; }
        public Dictionary<string, AnimationClip> Clips { get; set; }
        public float Speed { get; set; }
        public bool UpdateMode { get; set; }
        public bool CullingMode { get; set; }

        private float _currentTime;
        private string _currentClipName;

        public Animator()
        {
            Clips = new Dictionary<string, AnimationClip>();
            Speed = 1f;
            UpdateMode = true;
            CullingMode = true;
            _currentTime = 0f;
            _currentClipName = "";
        }

        public void AddClip(AnimationClip clip)
        {
            if (clip != null && !string.IsNullOrEmpty(clip.Name))
            {
                Clips[clip.Name] = clip;
            }
        }

        public void Play(string stateName)
        {
            if (Clips.ContainsKey(stateName))
            {
                _currentClipName = stateName;
                _currentTime = 0f;
            }
        }

        public void CrossFade(string stateName, float normalizedTransitionDuration)
        {
            Play(stateName);
        }

        public bool IsPlaying(string stateName)
        {
            return _currentClipName == stateName;
        }

        public void SetBool(string name, bool value)
        {
            RuntimeController?.SetParameter(name, value);
        }

        public void SetFloat(string name, float value)
        {
            RuntimeController?.SetParameter(name, value);
        }

        public void SetTrigger(string name)
        {
            RuntimeController?.SetTrigger(name);
        }

        public void Update(float deltaTime)
        {
            if (!UpdateMode || string.IsNullOrEmpty(_currentClipName))
                return;

            if (Clips.ContainsKey(_currentClipName))
            {
                var clip = Clips[_currentClipName];
                _currentTime += deltaTime * Speed;

                if (_currentTime > clip.Length && clip.WrapMode == WrapMode.Loop)
                {
                    _currentTime = 0f;
                }

                RuntimeController?.Update(deltaTime);
            }
        }
    }

    public class Avatar
    {
        public string Name { get; set; }
        public Dictionary<string, Transform> BoneTransforms { get; set; }

        public Avatar()
        {
            BoneTransforms = new Dictionary<string, Transform>();
        }
    }
}
