// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEngine.Core.Social
{
    public enum RelationshipType
    {
        Family,
        Romantic,
        Friendship,
        Hostile,
        Neutral,
        Unknown,
        Mentor,
        Student
    }

    public class SocialRelationship
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public RelationshipType Type { get; set; }
        public float Strength { get; set; } // -1.0 to 1.0
        public DateTime EstablishedDate { get; set; }
        public DateTime LastInteraction { get; set; }
        public List<string> SharedMemories { get; set; }
        public int InteractionCount { get; set; }

        public SocialRelationship(string entityId, string entityName, RelationshipType type = RelationshipType.Unknown)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            EntityName = entityName ?? "Unknown";
            Type = type;
            Strength = 0f;
            EstablishedDate = DateTime.UtcNow;
            LastInteraction = DateTime.UtcNow;
            SharedMemories = new List<string>();
            InteractionCount = 0;
        }

        public void RecordInteraction(string memory)
        {
            LastInteraction = DateTime.UtcNow;
            InteractionCount++;
            if (!string.IsNullOrEmpty(memory))
            {
                SharedMemories.Add(memory);
            }
        }
    }

    public class RelationshipManager
    {
        private Dictionary<string, SocialRelationship> _relationships;
        private Dictionary<string, List<string>> _familyGroups;
        private Dictionary<string, string> _entityToFamilyMap;

        public Dictionary<string, SocialRelationship> Relationships => _relationships;
        public Dictionary<string, List<string>> FamilyGroups => _familyGroups;

        public RelationshipManager()
        {
            _relationships = new Dictionary<string, SocialRelationship>();
            _familyGroups = new Dictionary<string, List<string>>();
            _entityToFamilyMap = new Dictionary<string, string>();
        }

        public void AddOrUpdateRelationship(string entityId, string entityName, RelationshipType type, float strength = 0f)
        {
            if (_relationships.ContainsKey(entityId))
            {
                var rel = _relationships[entityId];
                rel.Type = type;
                rel.Strength = Math.Clamp(strength, -1f, 1f);
                rel.LastInteraction = DateTime.UtcNow;
            }
            else
            {
                _relationships[entityId] = new SocialRelationship(entityId, entityName, type)
                {
                    Strength = Math.Clamp(strength, -1f, 1f)
                };
            }
        }

        public SocialRelationship? GetRelationship(string entityId)
        {
            return _relationships.ContainsKey(entityId) ? _relationships[entityId] : null;
        }

        public float GetRelationshipStrength(string entityId)
        {
            return _relationships.ContainsKey(entityId) ? _relationships[entityId].Strength : 0f;
        }

        public void AdjustRelationshipStrength(string entityId, float delta)
        {
            if (_relationships.ContainsKey(entityId))
            {
                _relationships[entityId].Strength = Math.Clamp(
                    _relationships[entityId].Strength + delta, -1f, 1f);
                _relationships[entityId].LastInteraction = DateTime.UtcNow;
            }
        }

        public void RecordInteraction(string entityId, string memory)
        {
            if (_relationships.ContainsKey(entityId))
            {
                _relationships[entityId].RecordInteraction(memory);
            }
        }

        public List<SocialRelationship> GetRelationshipsByType(RelationshipType type)
        {
            return _relationships.Values.Where(r => r.Type == type).ToList();
        }

        public List<SocialRelationship> GetStrongestRelationships(int count = 5)
        {
            return _relationships.Values
                .OrderByDescending(r => Math.Abs(r.Strength))
                .ThenByDescending(r => r.InteractionCount)
                .Take(count)
                .ToList();
        }

        public List<SocialRelationship> GetHostileRelationships()
        {
            return _relationships.Values.Where(r => r.Strength < -0.3f).ToList();
        }

        public List<SocialRelationship> GetFriendlyRelationships()
        {
            return _relationships.Values.Where(r => r.Strength > 0.3f).ToList();
        }

        public void CreateFamilyGroup(string familyId, List<string> memberIds)
        {
            if (!_familyGroups.ContainsKey(familyId))
            {
                _familyGroups[familyId] = new List<string>();
            }

            foreach (var memberId in memberIds)
            {
                if (!_familyGroups[familyId].Contains(memberId))
                {
                    _familyGroups[familyId].Add(memberId);
                    _entityToFamilyMap[memberId] = familyId;
                }
            }
        }

        public List<string> GetFamilyMembers(string familyId)
        {
            return _familyGroups.ContainsKey(familyId) ? _familyGroups[familyId] : new List<string>();
        }

        public string? GetFamilyId(string entityId)
        {
            return _entityToFamilyMap.ContainsKey(entityId) ? _entityToFamilyMap[entityId] : null;
        }

        public List<string> GetFamilyMembersOfEntity(string entityId)
        {
            var familyId = GetFamilyId(entityId);
            return familyId != null ? GetFamilyMembers(familyId) : new List<string>();
        }

        public bool AreRelated(string entityId1, string entityId2)
        {
            var family1 = GetFamilyId(entityId1);
            var family2 = GetFamilyId(entityId2);
            return family1 != null && family1 == family2;
        }

        public void DecayRelationships(float decayRate = 0.01f)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            
            foreach (var rel in _relationships.Values.ToList())
            {
                // Decay strength over time without interaction
                var daysSinceInteraction = (DateTime.UtcNow - rel.LastInteraction).TotalDays;
                if (daysSinceInteraction > 7)
                {
                    rel.Strength = rel.Strength * (1f - decayRate);
                }

                // Remove very old, weak relationships
                if (rel.EstablishedDate < cutoff && Math.Abs(rel.Strength) < 0.1f)
                {
                    _relationships.Remove(rel.EntityId);
                }
            }
        }

        public string GetRelationshipSummary()
        {
            if (_relationships.Count == 0)
                return "No relationships established.";

            var summary = $"Relationships ({_relationships.Count}):\n";
            
            var byType = _relationships.Values.GroupBy(r => r.Type);
            foreach (var group in byType)
            {
                summary += $"\n{group.Key} ({group.Count()}):\n";
                foreach (var rel in group.OrderByDescending(r => Math.Abs(r.Strength)))
                {
                    var strengthDesc = rel.Strength > 0.5f ? "close" : rel.Strength > 0 ? "friendly" :
                                       rel.Strength < -0.5f ? "hostile" : "distant";
                    summary += $"  - {rel.EntityName}: {strengthDesc} ({rel.Strength:F2})\n";
                }
            }

            return summary;
        }

        public List<string> GetRecommendedSocialActions(string entityId)
        {
            var recommendations = new List<string>();
            var hostile = GetHostileRelationships();
            var friendly = GetFriendlyRelationships();
            var weak = _relationships.Values.Where(r => Math.Abs(r.Strength) < 0.3f).ToList();

            if (hostile.Count > 0)
            {
                recommendations.Add($"Consider resolving conflicts with {hostile.Count} hostile entities");
            }

            if (friendly.Count > 0)
            {
                recommendations.Add($"Strengthen bonds with {friendly.Count} friendly entities");
            }

            if (weak.Count > 3)
            {
                recommendations.Add($"Many weak relationships - consider focusing on key connections");
            }

            return recommendations;
        }
    }
}
