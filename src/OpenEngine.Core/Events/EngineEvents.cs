// Created By Levi Enama
// Event System for Core → Editor Communication
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Events
{
    public enum EngineEventType
    {
        Log,
        Warning,
        Error,
        EntityCreated,
        EntityRemoved,
        EntityModified,
        PhysicsCollision,
        AudioPlayed,
        NetworkConnected,
        NetworkDisconnected,
        NetworkMessage,
        SimulationTick,
        RenderingFrame,
        ResourceLoaded,
        ResourceUnloaded
    }

    public class EngineEventArgs : EventArgs
    {
        public EngineEventType Type { get; set; }
        public string Message { get; set; }
        public string EntityId { get; set; }
        public object Data { get; set; }
        public DateTime Timestamp { get; set; }

        public EngineEventArgs(EngineEventType type, string message = "", string entityId = "", object data = null)
        {
            Type = type;
            Message = message;
            EntityId = entityId;
            Data = data;
            Timestamp = DateTime.UtcNow;
        }
    }

    public class EngineEventBus
    {
        private static EngineEventBus _instance;
        private readonly Dictionary<EngineEventType, List<EventHandler<EngineEventArgs>>> _eventHandlers;
        private readonly object _lock = new object();

        public static EngineEventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EngineEventBus();
                }
                return _instance;
            }
        }

        private EngineEventBus()
        {
            _eventHandlers = new Dictionary<EngineEventType, List<EventHandler<EngineEventArgs>>>();
            foreach (EngineEventType type in Enum.GetValues(typeof(EngineEventType)))
            {
                _eventHandlers[type] = new List<EventHandler<EngineEventArgs>>();
            }
        }

        public void Subscribe(EngineEventType type, EventHandler<EngineEventArgs> handler)
        {
            lock (_lock)
            {
                if (!_eventHandlers[type].Contains(handler))
                {
                    _eventHandlers[type].Add(handler);
                }
            }
        }

        public void Unsubscribe(EngineEventType type, EventHandler<EngineEventArgs> handler)
        {
            lock (_lock)
            {
                _eventHandlers[type].Remove(handler);
            }
        }

        public void Publish(EngineEventArgs args)
        {
            lock (_lock)
            {
                var handlers = _eventHandlers[args.Type].ToArray();
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler?.Invoke(this, args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in event handler: {ex.Message}");
                    }
                }
            }
        }

        public void Log(string message)
        {
            Publish(new EngineEventArgs(EngineEventType.Log, message));
        }

        public void Warning(string message)
        {
            Publish(new EngineEventArgs(EngineEventType.Warning, message));
        }

        public void Error(string message)
        {
            Publish(new EngineEventArgs(EngineEventType.Error, message));
        }

        public void EntityCreated(string entityId)
        {
            Publish(new EngineEventArgs(EngineEventType.EntityCreated, entityId: entityId));
        }

        public void EntityRemoved(string entityId)
        {
            Publish(new EngineEventArgs(EngineEventType.EntityRemoved, entityId: entityId));
        }

        public void EntityModified(string entityId, object data = null)
        {
            Publish(new EngineEventArgs(EngineEventType.EntityModified, entityId: entityId, data: data));
        }

        public void PhysicsCollision(string entityId1, string entityId2)
        {
            Publish(new EngineEventArgs(EngineEventType.PhysicsCollision, 
                $"Collision: {entityId1} <-> {entityId2}", 
                data: new { Entity1 = entityId1, Entity2 = entityId2 }));
        }

        public void AudioPlayed(string soundId)
        {
            Publish(new EngineEventArgs(EngineEventType.AudioPlayed, soundId));
        }

        public void SimulationTick(int tickCount)
        {
            Publish(new EngineEventArgs(EngineEventType.SimulationTick, $"Tick: {tickCount}", data: tickCount));
        }

        public void RenderingFrame(float deltaTime)
        {
            Publish(new EngineEventArgs(EngineEventType.RenderingFrame, $"Frame: {deltaTime:F4}s", data: deltaTime));
        }
    }
}
