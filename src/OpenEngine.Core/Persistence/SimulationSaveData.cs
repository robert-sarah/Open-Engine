// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;
using System.Text.Json.Serialization;

namespace OpenEngine.Core.Persistence;

public class SimulationSaveData
{
    public string Version { get; set; } = "1.0.0";
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public List<SimEntityData> Entities { get; set; } = new();
    public List<SystemicConnectionData> Connections { get; set; } = new();
    public List<string> GodPanelAlerts { get; set; } = new();
    public int TickCount { get; set; }

    public class SimEntityData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? ParentId { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public Dictionary<string, float> Attributes { get; set; } = new();
        public List<string> Capabilities { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    public class SystemicConnectionData
    {
        public string Id { get; set; } = string.Empty;
        public string SourceId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public ConnectionPredicate Predicate { get; set; }
        public float Strength { get; set; }
        public bool IsBidirectional { get; set; }
    }
}
