// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEngine.Core.Agents
{
    public enum RelationshipType
    {
        Family,
        Romantic,
        Friendship,
        Hostile,
        Neutral,
        Unknown
    }

    public enum EmotionType
    {
        Fear,
        Joy,
        Serenity,
        Shame,
        Anger,
        Sadness,
        Love,
        Curiosity
    }

    public class Relationship
    {
        public string TargetEntityId { get; set; }
        public string TargetEntityName { get; set; }
        public RelationshipType Type { get; set; }
        public float Strength { get; set; } // -1.0 (hostile) to 1.0 (close)
        public DateTime LastInteraction { get; set; }
        public List<string> SharedMemories { get; set; }

        public Relationship(string entityId, string entityName, RelationshipType type = RelationshipType.Unknown)
        {
            TargetEntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            TargetEntityName = entityName ?? "Unknown";
            Type = type;
            Strength = 0f;
            LastInteraction = DateTime.UtcNow;
            SharedMemories = new List<string>();
        }
    }

    public class AgentSoul
    {
        private Dictionary<string, Relationship> _relationships;
        private Dictionary<EmotionType, float> _emotions;
        private Dictionary<string, float> _personalityTraits;
        
        public Dictionary<string, Relationship> Relationships => _relationships;
        public Dictionary<EmotionType, float> Emotions => _emotions;
        public Dictionary<string, float> PersonalityTraits => _personalityTraits;

        public AgentSoul()
        {
            _relationships = new Dictionary<string, Relationship>();
            _emotions = new Dictionary<EmotionType, float>();
            _personalityTraits = new Dictionary<string, float>();

            InitializeDefaultEmotions();
            InitializeDefaultPersonality();
        }

        private void InitializeDefaultEmotions()
        {
            _emotions[EmotionType.Fear] = 0f;
            _emotions[EmotionType.Joy] = 0f;
            _emotions[EmotionType.Serenity] = 0.5f;
            _emotions[EmotionType.Shame] = 0f;
            _emotions[EmotionType.Anger] = 0f;
            _emotions[EmotionType.Sadness] = 0f;
            _emotions[EmotionType.Love] = 0f;
            _emotions[EmotionType.Curiosity] = 0.3f;
        }

        private void InitializeDefaultPersonality()
        {
            _personalityTraits["openness"] = 0.5f;
            _personalityTraits["conscientiousness"] = 0.5f;
            _personalityTraits["extraversion"] = 0.5f;
            _personalityTraits["agreeableness"] = 0.5f;
            _personalityTraits["neuroticism"] = 0.5f;
            _personalityTraits["bravery"] = 0.5f;
            _personalityTraits["generosity"] = 0.5f;
        }

        public void AddOrUpdateRelationship(string entityId, string entityName, RelationshipType type, float strength = 0f)
        {
            if (_relationships.ContainsKey(entityId))
            {
                var rel = _relationships[entityId];
                rel.Type = type;
                rel.Strength = strength;
                rel.LastInteraction = DateTime.UtcNow;
            }
            else
            {
                _relationships[entityId] = new Relationship(entityId, entityName, type)
                {
                    Strength = strength
                };
            }
        }

        public void UpdateRelationshipStrength(string entityId, float delta)
        {
            if (_relationships.ContainsKey(entityId))
            {
                _relationships[entityId].Strength = Math.Clamp(
                    _relationships[entityId].Strength + delta, -1f, 1f);
                _relationships[entityId].LastInteraction = DateTime.UtcNow;
            }
        }

        public float GetRelationshipStrength(string entityId)
        {
            return _relationships.ContainsKey(entityId) ? _relationships[entityId].Strength : 0f;
        }

        public Relationship? GetRelationship(string entityId)
        {
            return _relationships.ContainsKey(entityId) ? _relationships[entityId] : null;
        }

        public List<Relationship> GetRelationshipsByType(RelationshipType type)
        {
            return _relationships.Values.Where(r => r.Type == type).ToList();
        }

        public List<Relationship> GetStrongestRelationships(int count = 5)
        {
            return _relationships.Values.OrderByDescending(r => Math.Abs(r.Strength)).Take(count).ToList();
        }

        public void SetEmotion(EmotionType emotion, float value)
        {
            _emotions[emotion] = Math.Clamp(value, 0f, 1f);
        }

        public void AdjustEmotion(EmotionType emotion, float delta)
        {
            _emotions[emotion] = Math.Clamp(_emotions[emotion] + delta, 0f, 1f);
        }

        public float GetEmotion(EmotionType emotion)
        {
            return _emotions.ContainsKey(emotion) ? _emotions[emotion] : 0f;
        }

        public EmotionType GetDominantEmotion()
        {
            return _emotions.OrderByDescending(e => e.Value).First().Key;
        }

        public string GetEmotionalStateDescription()
        {
            var dominant = GetDominantEmotion();
            var value = _emotions[dominant];

            if (value < 0.2f)
                return "calm and neutral";

            return dominant switch
            {
                EmotionType.Fear => "fearful and anxious",
                EmotionType.Joy => "joyful and happy",
                EmotionType.Serenity => "serene and peaceful",
                EmotionType.Shame => "ashamed and embarrassed",
                EmotionType.Anger => "angry and frustrated",
                EmotionType.Sadness => "sad and melancholic",
                EmotionType.Love => "loving and affectionate",
                EmotionType.Curiosity => "curious and interested",
                _ => "emotionally mixed"
            };
        }

        public void SetPersonalityTrait(string trait, float value)
        {
            _personalityTraits[trait.ToLower()] = Math.Clamp(value, 0f, 1f);
        }

        public float GetPersonalityTrait(string trait)
        {
            return _personalityTraits.ContainsKey(trait.ToLower()) 
                ? _personalityTraits[trait.ToLower()] 
                : 0.5f;
        }

        public void AddSharedMemory(string entityId, string memoryDescription)
        {
            if (_relationships.ContainsKey(entityId))
            {
                _relationships[entityId].SharedMemories.Add(memoryDescription);
                _relationships[entityId].LastInteraction = DateTime.UtcNow;
            }
        }

        public string DescribeRelationships()
        {
            if (_relationships.Count == 0)
                return "I don't know anyone yet.";

            var description = "My relationships:\n";
            foreach (var rel in _relationships.Values.OrderByDescending(r => Math.Abs(r.Strength)))
            {
                var strengthDesc = rel.Strength > 0.5f ? "close" : rel.Strength > 0 ? "friendly" : 
                                   rel.Strength < -0.5f ? "hostile" : "distant";
                description += $"- {rel.TargetEntityName}: {rel.Type} ({strengthDesc})\n";
            }
            return description;
        }

        public void DecayEmotions(float decayRate = 0.01f)
        {
            // Slowly decay all emotions toward baseline
            foreach (var emotion in _emotions.Keys.ToList())
            {
                if (emotion == EmotionType.Serenity)
                {
                    // Serenity tends toward 0.5
                    _emotions[emotion] = _emotions[emotion] * (1 - decayRate) + 0.5f * decayRate;
                }
                else
                {
                    // Other emotions tend toward 0
                    _emotions[emotion] = _emotions[emotion] * (1 - decayRate);
                }
            }
        }
    }
}
