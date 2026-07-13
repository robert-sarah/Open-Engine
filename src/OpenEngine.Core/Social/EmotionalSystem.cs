// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEngine.Core.Social
{
    public enum EmotionType { Fear, Joy, Serenity, Shame, Anger, Sadness, Love, Curiosity, Pride }

    public class EmotionalState
    {
        public Dictionary<EmotionType, float> CurrentEmotions { get; }
        
        public EmotionalState()
        {
            CurrentEmotions = new Dictionary<EmotionType, float>
            {
                { EmotionType.Fear, 0f },
                { EmotionType.Joy, 0f },
                { EmotionType.Serenity, 0.5f },
                { EmotionType.Shame, 0f },
                { EmotionType.Anger, 0f },
                { EmotionType.Sadness, 0f },
                { EmotionType.Love, 0f },
                { EmotionType.Curiosity, 0.3f },
                { EmotionType.Pride, 0f }
            };
        }

        public void SetEmotion(EmotionType emotion, float value)
        {
            CurrentEmotions[emotion] = Math.Clamp(value, 0f, 1f);
        }

        public void AdjustEmotion(EmotionType emotion, float delta)
        {
            CurrentEmotions[emotion] = Math.Clamp(CurrentEmotions[emotion] + delta, 0f, 1f);
        }

        public float GetEmotion(EmotionType emotion)
        {
            return CurrentEmotions.ContainsKey(emotion) ? CurrentEmotions[emotion] : 0f;
        }

        public EmotionType GetDominantEmotion()
        {
            return CurrentEmotions.OrderByDescending(e => e.Value).First().Key;
        }

        public void DecayEmotions(float deltaTime)
        {
            foreach (var emotion in CurrentEmotions.Keys.ToList())
            {
                var baseline = emotion == EmotionType.Serenity ? 0.5f : 0f;
                CurrentEmotions[emotion] = Math.Clamp(
                    CurrentEmotions[emotion] * (1f - 0.02f * deltaTime) + baseline * 0.02f * deltaTime, 0f, 1f);
            }
        }

        public string GetDescription()
        {
            var dominant = GetDominantEmotion();
            var value = CurrentEmotions[dominant];
            if (value < 0.2f) return "calm";
            return dominant.ToString().ToLower();
        }
    }
}
