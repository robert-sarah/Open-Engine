// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Intent;
using OpenEngine.Core.Math;
using OpenEngine.Core.Physics;
using OpenEngine.Core.Scripting;

namespace OpenEngine.Core.Engine
{
    public class SimulationTickEventArgs : EventArgs
    {
        public int TickCount { get; }
        public TimeSpan DeltaTime { get; }
        public DateTime Timestamp { get; }

        public SimulationTickEventArgs(int tickCount, TimeSpan deltaTime, DateTime timestamp)
        {
            TickCount = tickCount;
            DeltaTime = deltaTime;
            Timestamp = timestamp;
        }
    }

    public class OpenSimulationEngine
    {
        public event EventHandler<SimulationTickEventArgs>? OnTick;
        public event EventHandler<string>? OnGodPanelAlert;

        public Dictionary<string, SimEntity> Entities { get; }
        public Dictionary<string, SystemicConnection> Connections { get; }
        public List<string> GodPanelAlerts { get; }
        public int TickCount { get; private set; }
        public float TickRate { get; }
        public bool IsRunning { get; private set; }
        public DateTime StartTime { get; private set; }
        private DateTime _lastTickTime;
        private readonly object _lock = new object();
        
        public PhysicsSystem Physics { get; }
        public ScriptEngine Scripting { get; }

        public OpenSimulationEngine(float tickRate = 60.0f)
        {
            Entities = new Dictionary<string, SimEntity>();
            Connections = new Dictionary<string, SystemicConnection>();
            GodPanelAlerts = new List<string>();
            TickRate = System.Math.Clamp(tickRate, 1, 1000);
            TickCount = 0;
            IsRunning = false;
            Physics = new PhysicsSystem();
            Scripting = new ScriptEngine();
        }

        public void Initialize()
        {
            lock (_lock)
            {
                TickCount = 0;
                StartTime = DateTime.UtcNow;
                _lastTickTime = StartTime;
                IsRunning = true;
            }
        }

        public void Update()
        {
            if (!IsRunning)
                return;

            lock (_lock)
            {
                DateTime currentTime = DateTime.UtcNow;
                TimeSpan deltaTime = currentTime - _lastTickTime;
                float dt = (float)deltaTime.TotalSeconds;
                TickCount++;
                _lastTickTime = currentTime;

                Physics.Update(dt);
                Scripting.UpdateAll(dt);
                
                var args = new SimulationTickEventArgs(TickCount, deltaTime, currentTime);
                OnTick?.Invoke(this, args);
            }
        }

        public void Shutdown()
        {
            lock (_lock)
            {
                IsRunning = false;
            }
        }

        public string GenerateEntityId()
        {
            return $"entity_{Guid.NewGuid():N}";
        }

        public string GenerateConnectionId()
        {
            return $"conn_{Guid.NewGuid():N}";
        }

        public SimEntity CreateEntity(string name, string type)
        {
            string id = GenerateEntityId();
            var entity = new SimEntity(id, name, type);
            lock (_lock)
            {
                Entities[id] = entity;
            }
            return entity;
        }

        public bool TryGetEntity(string id, out SimEntity entity)
        {
            lock (_lock)
            {
                return Entities.TryGetValue(id, out entity);
            }
        }

        public SimEntity GetEntity(string id)
        {
            TryGetEntity(id, out SimEntity entity);
            return entity;
        }

        public bool RemoveEntity(string id)
        {
            lock (_lock)
            {
                if (Entities.Remove(id))
                {
                    var connectionsToRemove = Connections.Values
                        .Where(c => c.SourceId == id || c.TargetId == id)
                        .Select(c => c.Id)
                        .ToList();

                    foreach (var connId in connectionsToRemove)
                        Connections.Remove(connId);

                    return true;
                }
                return false;
            }
        }

        public SystemicConnection CreateConnection(string sourceId, string targetId, ConnectionPredicate predicate)
        {
            if (!Entities.ContainsKey(sourceId) || !Entities.ContainsKey(targetId))
                throw new ArgumentException("Source or target entity not found");

            string id = GenerateConnectionId();
            var connection = new SystemicConnection(id, sourceId, targetId, predicate);
            lock (_lock)
            {
                Connections[id] = connection;
            }
            return connection;
        }

        public void AddGodPanelAlert(string alert)
        {
            lock (_lock)
            {
                GodPanelAlerts.Add(alert);
            }
            OnGodPanelAlert?.Invoke(this, alert);
        }

        public void ClearGodPanelAlerts()
        {
            lock (_lock)
            {
                GodPanelAlerts.Clear();
            }
        }

        public List<SimEntity> GetEntitiesByType(string type)
        {
            lock (_lock)
            {
                return Entities.Values.Where(e => e.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        public List<SimEntity> GetEntitiesByTag(string tag)
        {
            lock (_lock)
            {
                return Entities.Values.Where(e => e.HasTag(tag)).ToList();
            }
        }

        public List<SystemicConnection> GetConnectionsFromEntity(string entityId)
        {
            lock (_lock)
            {
                return Connections.Values.Where(c => c.SourceId == entityId).ToList();
            }
        }

        public List<SystemicConnection> GetConnectionsToEntity(string entityId)
        {
            lock (_lock)
            {
                return Connections.Values.Where(c => c.TargetId == entityId).ToList();
            }
        }

        public List<SimEntity> GetEntitiesInRadius(Vector3 center, float radius)
        {
            lock (_lock)
            {
                float sqrRadius = radius * radius;
                return Entities.Values
                    .Where(e => Vector3.SqrDistance(e.Position3D, center) <= sqrRadius)
                    .ToList();
            }
        }

        public List<SimEntity> GetChildren(string parentId)
        {
            lock (_lock)
            {
                return Entities.Values.Where(e => e.ParentId == parentId).ToList();
            }
        }

        public string Serialize3DContextToMarkdown(string entityId)
        {
            var sb = new StringBuilder();
            lock (_lock)
            {
                if (!Entities.TryGetValue(entityId, out SimEntity focusEntity))
                {
                    sb.AppendLine("# Entity Not Found");
                    return sb.ToString();
                }

                sb.AppendLine($"# Context for: {focusEntity.Name} ({focusEntity.Id})");
                sb.AppendLine();

                sb.AppendLine("## Position and Attributes:");
                sb.AppendLine($"- Type: {focusEntity.Type}");
                sb.AppendLine($"- Position: {focusEntity.Position3D}");
                sb.AppendLine($"- Rotation: {focusEntity.Rotation3D}");
                sb.AppendLine($"- Scale: {focusEntity.Scale3D}");
                sb.AppendLine();

                if (focusEntity.Attributes.Count > 0)
                {
                    sb.AppendLine("### Attributes:");
                    foreach (var attr in focusEntity.Attributes)
                        sb.AppendLine($"- {attr.Key}: {attr.Value:F2}");
                    sb.AppendLine();
                }

                var incoming = GetConnectionsToEntity(entityId);
                var outgoing = GetConnectionsFromEntity(entityId);
                if (incoming.Count > 0 || outgoing.Count > 0)
                {
                    sb.AppendLine("## Connections:");
                    if (outgoing.Count > 0)
                    {
                        sb.AppendLine("### Outgoing:");
                        foreach (var conn in outgoing)
                        {
                            if (Entities.TryGetValue(conn.TargetId, out SimEntity target))
                            {
                                float distance = Vector3.Distance(focusEntity.Position3D, target.Position3D);
                                sb.AppendLine($"- {conn.Predicate} {target.Name} (Distance: {distance:F2} units)");
                            }
                        }
                    }
                    if (incoming.Count > 0)
                    {
                        sb.AppendLine("### Incoming:");
                        foreach (var conn in incoming)
                        {
                            if (Entities.TryGetValue(conn.SourceId, out SimEntity source))
                            {
                                float distance = Vector3.Distance(focusEntity.Position3D, source.Position3D);
                                sb.AppendLine($"- {source.Name} {conn.Predicate} this (Distance: {distance:F2} units)");
                            }
                        }
                    }
                    sb.AppendLine();
                }

                var nearby = Entities.Values
                    .Where(e => e.Id != entityId)
                    .Select(e => new { Entity = e, Distance = Vector3.Distance(focusEntity.Position3D, e.Position3D) })
                    .OrderBy(x => x.Distance)
                    .Take(10)
                    .ToList();

                if (nearby.Count > 0)
                {
                    sb.AppendLine("## Nearby Entities:");
                    foreach (var item in nearby)
                    {
                        sb.AppendLine($"- {item.Entity.Name} ({item.Entity.Type} - {item.Distance:F2} units away");
                    }
                    sb.AppendLine();
                }

                if (GodPanelAlerts.Count > 0)
                {
                    sb.AppendLine("## Global Alerts:");
                    foreach (var alert in GodPanelAlerts)
                    {
                        sb.AppendLine($"- ⚠️ {alert}");
                    }
                }
            }
            return sb.ToString();
        }

        public List<EngineActionEvent>? GetPendingActions()
        {
            return new List<EngineActionEvent>();
        }
    }
}
