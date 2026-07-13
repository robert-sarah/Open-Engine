// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Environment
{
    public enum ResourceType
    {
        Berries,
        Wood,
        Flint,
        Water,
        AnimalSkin,
        Meat,
        Stone,
        Fiber,
        Herbs
    }

    public class ResourceNode
    {
        public string Id { get; set; }
        public ResourceType Type { get; set; }
        public Vector3 Position { get; set; }
        public float Amount { get; set; }
        public float MaxAmount { get; set; }
        public float RegenerationRate { get; set; }
        public DateTime LastHarvested { get; set; }
        public bool IsDepleted => Amount <= 0;

        public ResourceNode(string id, ResourceType type, Vector3 position, float amount, float regenRate = 0f)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Type = type;
            Position = position;
            Amount = amount;
            MaxAmount = amount;
            RegenerationRate = regenRate;
            LastHarvested = DateTime.UtcNow;
        }

        public float Harvest(float amount)
        {
            var harvested = Math.Min(Amount, amount);
            Amount -= harvested;
            LastHarvested = DateTime.UtcNow;
            return harvested;
        }

        public void Regenerate(float deltaTime)
        {
            if (Amount < MaxAmount && RegenerationRate > 0)
            {
                var regenAmount = RegenerationRate * deltaTime;
                Amount = Math.Min(Amount + regenAmount, MaxAmount);
            }
        }
    }

    public class ResourceManager
    {
        private List<ResourceNode> _resourceNodes;
        private Dictionary<ResourceType, float> _spawnRates;
        private Dictionary<ResourceType, float> _defaultAmounts;
        private Random _random;

        public List<ResourceNode> ResourceNodes => _resourceNodes;

        public ResourceManager()
        {
            _resourceNodes = new List<ResourceNode>();
            _random = new Random();
            _spawnRates = new Dictionary<ResourceType, float>
            {
                { ResourceType.Berries, 0.1f },
                { ResourceType.Wood, 0.15f },
                { ResourceType.Flint, 0.05f },
                { ResourceType.Water, 0.08f },
                { ResourceType.Stone, 0.1f },
                { ResourceType.Herbs, 0.07f }
            };

            _defaultAmounts = new Dictionary<ResourceType, float>
            {
                { ResourceType.Berries, 10f },
                { ResourceType.Wood, 5f },
                { ResourceType.Flint, 3f },
                { ResourceType.Water, 20f },
                { ResourceType.Stone, 8f },
                { ResourceType.Herbs, 5f }
            };
        }

        public void AddResourceNode(ResourceNode node)
        {
            if (node != null && !_resourceNodes.Any(r => r.Id == node.Id))
            {
                _resourceNodes.Add(node);
            }
        }

        public void RemoveResourceNode(string nodeId)
        {
            _resourceNodes.RemoveAll(r => r.Id == nodeId);
        }

        public ResourceNode? FindNearestResource(Vector3 position, ResourceType type, float maxDistance = 100f)
        {
            return _resourceNodes
                .Where(r => r.Type == type && !r.IsDepleted)
                .OrderBy(r => Vector3.SqrDistance(position, r.Position))
                .FirstOrDefault(r => Vector3.Distance(position, r.Position) <= maxDistance);
        }

        public List<ResourceNode> FindResourcesInRadius(Vector3 center, float radius, ResourceType? type = null)
        {
            var query = _resourceNodes.Where(r => !r.IsDepleted);
            
            if (type.HasValue)
            {
                query = query.Where(r => r.Type == type.Value);
            }

            return query.Where(r => Vector3.Distance(center, r.Position) <= radius).ToList();
        }

        public List<ResourceNode> FindResourcesOfType(ResourceType type)
        {
            return _resourceNodes.Where(r => r.Type == type && !r.IsDepleted).ToList();
        }

        public float? HarvestResource(string nodeId, float amount)
        {
            var node = _resourceNodes.FirstOrDefault(r => r.Id == nodeId);
            if (node == null || node.IsDepleted)
                return null;

            return node.Harvest(amount);
        }

        public void UpdateResources(float deltaTime)
        {
            foreach (var resource in _resourceNodes)
            {
                resource.Regenerate(deltaTime);
            }

            // Randomly spawn new resources
            SpawnRandomResources();
        }

        private void SpawnRandomResources()
        {
            foreach (var spawnRate in _spawnRates)
            {
                if (_random.NextDouble() < spawnRate.Value * 0.01f)
                {
                    var position = new Vector3(
                        (float)_random.NextDouble() * 200 - 100,
                        0,
                        (float)_random.NextDouble() * 200 - 100
                    );

                    var amount = _defaultAmounts.ContainsKey(spawnRate.Key) 
                        ? _defaultAmounts[spawnRate.Key] 
                        : 5f;

                    var node = new ResourceNode(
                        Guid.NewGuid().ToString(),
                        spawnRate.Key,
                        position,
                        amount,
                        amount * 0.1f // 10% regeneration rate
                    );

                    AddResourceNode(node);
                }
            }
        }

        public void SpawnResourceCluster(ResourceType type, Vector3 center, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                var angle = (float)i / count * Math.PI * 2;
                var distance = (float)_random.NextDouble() * radius;
                var position = new Vector3(
                    center.X + (float)Math.Cos(angle) * distance,
                    center.Y,
                    center.Z + (float)Math.Sin(angle) * distance
                );

                var amount = _defaultAmounts.ContainsKey(type) 
                    ? _defaultAmounts[type] 
                    : 5f;

                var node = new ResourceNode(
                    Guid.NewGuid().ToString(),
                    type,
                    position,
                    amount,
                    amount * 0.1f
                );

                AddResourceNode(node);
            }
        }

        public void ClearDepletedResources()
        {
            _resourceNodes.RemoveAll(r => r.IsDepleted);
        }

        public Dictionary<ResourceType, int> GetResourceCounts()
        {
            return _resourceNodes
                .Where(r => !r.IsDepleted)
                .GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public string GetResourceSummary()
        {
            var counts = GetResourceCounts();
            var summary = "Resource Summary:\n";
            
            foreach (var count in counts)
            {
                summary += $"- {count.Key}: {count.Value} nodes\n";
            }

            summary += $"\nTotal resources: {_resourceNodes.Count(r => !r.IsDepleted)}";
            return summary;
        }
    }
}
