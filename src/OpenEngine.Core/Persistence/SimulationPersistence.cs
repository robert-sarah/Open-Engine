// Created By Levi Enama
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using OpenEngine.Core.Engine;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Persistence;

public static class SimulationPersistence
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task SaveAsync(this OpenSimulationEngine engine, string filePath)
    {
        var saveData = new SimulationSaveData
        {
            TickCount = engine.TickCount,
            GodPanelAlerts = new List<string>(engine.GodPanelAlerts)
        };

        foreach (var entity in engine.Entities.Values)
        {
            saveData.Entities.Add(new SimulationSaveData.SimEntityData
            {
                Id = entity.Id,
                Name = entity.Name,
                Type = entity.Type,
                ParentId = entity.ParentId,
                Position = entity.Position3D,
                Rotation = entity.Rotation3D,
                Scale = entity.Scale3D,
                Attributes = new Dictionary<string, float>(entity.Attributes),
                Capabilities = new List<string>(entity.Capabilities),
                Tags = new List<string>(entity.Tags)
            });
        }

        foreach (var conn in engine.Connections.Values)
        {
            saveData.Connections.Add(new SimulationSaveData.SystemicConnectionData
            {
                Id = conn.Id,
                SourceId = conn.SourceId,
                TargetId = conn.TargetId,
                Predicate = conn.Predicate,
                Strength = conn.Strength,
                IsBidirectional = conn.IsBidirectional
            });
        }

        var json = JsonSerializer.Serialize(saveData, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public static async Task<OpenSimulationEngine> LoadAsync(string filePath, float tickRate = 60.0f)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var saveData = JsonSerializer.Deserialize<SimulationSaveData>(json, JsonOptions);
        if (saveData == null) throw new InvalidDataException("Failed to deserialize save file.");

        var engine = new OpenSimulationEngine(tickRate);
        
        foreach (var entityData in saveData.Entities)
        {
            var entity = new SimEntity(entityData.Id, entityData.Name, entityData.Type)
            {
                ParentId = entityData.ParentId,
                Position3D = entityData.Position,
                Rotation3D = entityData.Rotation,
                Scale3D = entityData.Scale
            };
            foreach (var kvp in entityData.Attributes)
                entity.Attributes[kvp.Key] = kvp.Value;
            entity.Capabilities.AddRange(entityData.Capabilities);
            entity.Tags.AddRange(entityData.Tags);
            engine.Entities[entity.Id] = entity;
        }

        foreach (var connData in saveData.Connections)
        {
            var conn = new SystemicConnection(connData.Id, connData.SourceId, connData.TargetId, connData.Predicate)
            {
                Strength = connData.Strength,
                IsBidirectional = connData.IsBidirectional
            };
            engine.Connections[conn.Id] = conn;
        }

        foreach (var alert in saveData.GodPanelAlerts)
            engine.GodPanelAlerts.Add(alert);

        engine.Initialize();
        return engine;
    }
}
