// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEngine.Core.Social
{
    public enum TribeRole
    {
        Leader,
        Elder,
        Hunter,
        Gatherer,
        Builder,
        Healer,
        Child,
        Unassigned
    }

    public class TribeMember
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public TribeRole Role { get; set; }
        public DateTime JoinedDate { get; set; }
        public float ContributionScore { get; set; }
        public List<string> Achievements { get; set; }

        public TribeMember(string entityId, string entityName, TribeRole role = TribeRole.Unassigned)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            EntityName = entityName ?? "Unknown";
            Role = role;
            JoinedDate = DateTime.UtcNow;
            ContributionScore = 0f;
            Achievements = new List<string>();
        }

        public void AddAchievement(string achievement)
        {
            Achievements.Add(achievement);
            ContributionScore += 10f;
        }

        public void AddContribution(float amount)
        {
            ContributionScore += amount;
        }
    }

    public class Tribe
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LeaderId { get; set; }
        public Dictionary<string, TribeMember> Members { get; set; }
        public Dictionary<string, float> SharedResources { get; set; }
        public DateTime FoundedDate { get; set; }
        public List<string> Traditions { get; set; }
        public Dictionary<string, float> CulturalValues { get; set; }

        public Tribe(string name, string leaderId, string leaderName)
        {
            Id = Guid.NewGuid().ToString();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            LeaderId = leaderId ?? throw new ArgumentNullException(nameof(leaderId));
            Members = new Dictionary<string, TribeMember>();
            SharedResources = new Dictionary<string, float>();
            FoundedDate = DateTime.UtcNow;
            Traditions = new List<string>();
            CulturalValues = new Dictionary<string, float>();

            // Add leader as first member
            Members[leaderId] = new TribeMember(leaderId, leaderName, TribeRole.Leader);
        }

        public void AddMember(string entityId, string entityName, TribeRole role = TribeRole.Unassigned)
        {
            if (!Members.ContainsKey(entityId))
            {
                Members[entityId] = new TribeMember(entityId, entityName, role);
            }
        }

        public void RemoveMember(string entityId)
        {
            if (Members.ContainsKey(entityId) && entityId != LeaderId)
            {
                Members.Remove(entityId);
            }
        }

        public void SetMemberRole(string entityId, TribeRole role)
        {
            if (Members.ContainsKey(entityId))
            {
                Members[entityId].Role = role;
            }
        }

        public TribeMember? GetMember(string entityId)
        {
            return Members.ContainsKey(entityId) ? Members[entityId] : null;
        }

        public List<TribeMember> GetMembersByRole(TribeRole role)
        {
            return Members.Values.Where(m => m.Role == role).ToList();
        }

        public void AddSharedResource(string resourceType, float amount)
        {
            if (SharedResources.ContainsKey(resourceType))
            {
                SharedResources[resourceType] += amount;
            }
            else
            {
                SharedResources[resourceType] = amount;
            }
        }

        public bool ConsumeSharedResource(string resourceType, float amount)
        {
            if (SharedResources.ContainsKey(resourceType) && SharedResources[resourceType] >= amount)
            {
                SharedResources[resourceType] -= amount;
                if (SharedResources[resourceType] <= 0)
                {
                    SharedResources.Remove(resourceType);
                }
                return true;
            }
            return false;
        }

        public void AddTradition(string tradition)
        {
            if (!Traditions.Contains(tradition))
            {
                Traditions.Add(tradition);
            }
        }

        public void SetCulturalValue(string valueName, float strength)
        {
            CulturalValues[valueName] = Math.Clamp(strength, 0f, 1f);
        }

        public float GetCulturalValue(string valueName)
        {
            return CulturalValues.ContainsKey(valueName) ? CulturalValues[valueName] : 0.5f;
        }

        public void ElectNewLeader(string newLeaderId)
        {
            if (Members.ContainsKey(newLeaderId))
            {
                LeaderId = newLeaderId;
                SetMemberRole(newLeaderId, TribeRole.Leader);
                if (Members.ContainsKey(LeaderId) && Members[LeaderId].EntityId != newLeaderId)
                {
                    SetMemberRole(Members[LeaderId].EntityId, TribeRole.Elder);
                }
            }
        }

        public string GetTribeSummary()
        {
            var summary = $"Tribe: {Name}\n";
            summary += $"Leader: {Members[LeaderId]?.EntityName ?? "Unknown"}\n";
            summary += $"Members: {Members.Count}\n";
            summary += $"Founded: {FoundedDate:yyyy-MM-dd}\n";
            summary += $"\nShared Resources:\n";
            foreach (var resource in SharedResources)
            {
                summary += $"  - {resource.Key}: {resource.Value:F1}\n";
            }
            summary += $"\nTraditions: {Traditions.Count}\n";
            if (Traditions.Count > 0)
            {
                foreach (var tradition in Traditions)
                {
                    summary += $"  - {tradition}\n";
                }
            }
            return summary;
        }
    }

    public class TribalSystem
    {
        private Dictionary<string, Tribe> _tribes;
        private Dictionary<string, string> _entityToTribeMap;

        public Dictionary<string, Tribe> Tribes => _tribes;

        public TribalSystem()
        {
            _tribes = new Dictionary<string, Tribe>();
            _entityToTribeMap = new Dictionary<string, string>();
        }

        public Tribe CreateTribe(string name, string leaderId, string leaderName)
        {
            var tribe = new Tribe(name, leaderId, leaderName);
            _tribes[tribe.Id] = tribe;
            _entityToTribeMap[leaderId] = tribe.Id;
            return tribe;
        }

        public Tribe? GetTribe(string tribeId)
        {
            return _tribes.ContainsKey(tribeId) ? _tribes[tribeId] : null;
        }

        public Tribe? GetTribeByMember(string entityId)
        {
            if (_entityToTribeMap.ContainsKey(entityId))
            {
                var tribeId = _entityToTribeMap[entityId];
                return _tribes.ContainsKey(tribeId) ? _tribes[tribeId] : null;
            }
            return null;
        }

        public bool AddMemberToTribe(string tribeId, string entityId, string entityName, TribeRole role = TribeRole.Unassigned)
        {
            var tribe = GetTribe(tribeId);
            if (tribe != null)
            {
                tribe.AddMember(entityId, entityName, role);
                _entityToTribeMap[entityId] = tribeId;
                return true;
            }
            return false;
        }

        public bool RemoveMemberFromTribe(string entityId)
        {
            var tribe = GetTribeByMember(entityId);
            if (tribe != null)
            {
                tribe.RemoveMember(entityId);
                _entityToTribeMap.Remove(entityId);
                return true;
            }
            return false;
        }

        public List<Tribe> GetAllTribes()
        {
            return _tribes.Values.ToList();
        }

        public Dictionary<string, int> GetTribePopulationStats()
        {
            return _tribes.ToDictionary(t => t.Key, t => t.Value.Members.Count);
        }

        public void TransferResourceBetweenTribes(string fromTribeId, string toTribeId, string resourceType, float amount)
        {
            var fromTribe = GetTribe(fromTribeId);
            var toTribe = GetTribe(toTribeId);

            if (fromTribe != null && toTribe != null)
            {
                if (fromTribe.ConsumeSharedResource(resourceType, amount))
                {
                    toTribe.AddSharedResource(resourceType, amount);
                }
            }
        }
    }
}
