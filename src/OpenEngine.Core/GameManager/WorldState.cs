// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.GameManager
{
    public enum WeatherType
    {
        Clear,
        Cloudy,
        Rain,
        Storm,
        Snow
    }

    public enum ResourceType
    {
        Berries,
        Wood,
        Flint,
        Water,
        AnimalSkin,
        Meat,
        Stone
    }

    public class ResourceNode
    {
        public string Id { get; set; }
        public ResourceType Type { get; set; }
        public Vector3 Position { get; set; }
        public float Amount { get; set; }
        public float MaxAmount { get; set; }
        public bool IsDepleted => Amount <= 0;

        public ResourceNode(string id, ResourceType type, Vector3 position, float amount)
        {
            Id = id;
            Type = type;
            Position = position;
            Amount = amount;
            MaxAmount = amount;
        }
    }

    public class WorldState
    {
        public Dictionary<string, SimEntity> AllEntities { get; }
        public List<ResourceNode> ResourceNodes { get; }
        public WeatherType CurrentWeather { get; set; }
        public float Temperature { get; set; }
        public DateTime WorldTime { get; set; }
        public bool IsDay { get; set; }
        public float DayProgress { get; set; } // 0.0 to 1.0 through the day

        public WorldState()
        {
            AllEntities = new Dictionary<string, SimEntity>();
            ResourceNodes = new List<ResourceNode>();
            CurrentWeather = WeatherType.Clear;
            Temperature = 20f;
            WorldTime = DateTime.UtcNow;
            IsDay = true;
            DayProgress = 0.5f;
        }

        public void AddEntity(SimEntity entity)
        {
            if (entity != null && !string.IsNullOrEmpty(entity.Id))
            {
                AllEntities[entity.Id] = entity;
            }
        }

        public SimEntity? GetEntity(string id)
        {
            return AllEntities.ContainsKey(id) ? AllEntities[id] : null;
        }

        public List<SimEntity> GetEntitiesInRadius(Vector3 center, float radius)
        {
            float sqrRadius = radius * radius;
            return AllEntities.Values
                .Where(e => Vector3.SqrDistance(e.Position3D, center) <= sqrRadius)
                .ToList();
        }

        public ResourceNode? FindNearestResource(Vector3 position, ResourceType type, float maxDistance = 100f)
        {
            return ResourceNodes
                .Where(r => r.Type == type && !r.IsDepleted)
                .OrderBy(r => Vector3.SqrDistance(position, r.Position))
                .FirstOrDefault(r => Vector3.Distance(position, r.Position) <= maxDistance);
        }

        public List<ResourceNode> FindResourcesOfType(ResourceType type)
        {
            return ResourceNodes.Where(r => r.Type == type && !r.IsDepleted).ToList();
        }

        public string GetVisibleContext(string agentId)
        {
            var agent = GetEntity(agentId);
            if (agent == null)
                return "Unknown location.";

            var nearbyEntities = GetEntitiesInRadius(agent.Position3D, 20f);
            var nearbyResources = ResourceNodes
                .Where(r => Vector3.Distance(agent.Position3D, r.Position) <= 20f && !r.IsDepleted)
                .ToList();

            var context = $"Location: {agent.Position3D}\n";
            context += $"Weather: {CurrentWeather}\n";
            context += $"Time: {(IsDay ? "Day" : "Night")}\n";
            context += $"Temperature: {Temperature}°C\n\n";

            if (nearbyEntities.Count > 0)
            {
                context += "Nearby entities:\n";
                foreach (var entity in nearbyEntities.Where(e => e.Id != agentId))
                {
                    var dist = Vector3.Distance(agent.Position3D, entity.Position3D);
                    context += $"- {entity.Name} ({entity.Type}) - {dist:F1} units away\n";
                }
                context += "\n";
            }

            if (nearbyResources.Count > 0)
            {
                context += "Nearby resources:\n";
                foreach (var resource in nearbyResources)
                {
                    var dist = Vector3.Distance(agent.Position3D, resource.Position);
                    context += $"- {resource.Type} - {dist:F1} units away (amount: {resource.Amount:F1})\n";
                }
            }

            return context;
        }

        public void AddResourceNode(ResourceNode node)
        {
            if (node != null)
            {
                ResourceNodes.Add(node);
            }
        }

        public void RemoveResourceNode(string nodeId)
        {
            ResourceNodes.RemoveAll(r => r.Id == nodeId);
        }

        public void UpdateWeather()
        {
            // Simple weather transition logic
            var random = new Random();
            var roll = random.NextDouble();

            if (CurrentWeather == WeatherType.Clear)
            {
                if (roll < 0.1) CurrentWeather = WeatherType.Cloudy;
                else if (roll < 0.15) CurrentWeather = WeatherType.Rain;
            }
            else if (CurrentWeather == WeatherType.Cloudy)
            {
                if (roll < 0.3) CurrentWeather = WeatherType.Clear;
                else if (roll < 0.5) CurrentWeather = WeatherType.Rain;
            }
            else if (CurrentWeather == WeatherType.Rain)
            {
                if (roll < 0.4) CurrentWeather = WeatherType.Cloudy;
                else if (roll < 0.5) CurrentWeather = WeatherType.Storm;
                else if (roll < 0.6) CurrentWeather = WeatherType.Clear;
            }
            else if (CurrentWeather == WeatherType.Storm)
            {
                if (roll < 0.3) CurrentWeather = WeatherType.Rain;
                else if (roll < 0.4) CurrentWeather = WeatherType.Cloudy;
            }

            // Update temperature based on weather
            Temperature = CurrentWeather switch
            {
                WeatherType.Clear => 20f + (float)random.NextDouble() * 10f,
                WeatherType.Cloudy => 15f + (float)random.NextDouble() * 8f,
                WeatherType.Rain => 12f + (float)random.NextDouble() * 6f,
                WeatherType.Storm => 8f + (float)random.NextDouble() * 5f,
                WeatherType.Snow => -5f + (float)random.NextDouble() * 10f,
                _ => Temperature
            };
        }

        public void AdvanceTime(float hours)
        {
            WorldTime = WorldTime.AddHours(hours);
            DayProgress = (DayProgress + hours / 24f) % 1f;
            IsDay = DayProgress >= 0.25f && DayProgress <= 0.75f;
        }
    }
}
