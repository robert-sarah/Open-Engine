// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Physics;

public class PhysicsSystem
{
    public float Gravity { get; set; } = -9.81f;
    public float TimeScale { get; set; } = 1.0f;

    private readonly Dictionary<string, PhysicsBody> _bodies = new();

    public void AddBody(SimEntity entity, PhysicsBodyType type = PhysicsBodyType.Dynamic)
    {
        _bodies[entity.Id] = new PhysicsBody(entity, type);
    }

    public void RemoveBody(string entityId) => _bodies.Remove(entityId);

    public void Update(float deltaTime)
    {
        float dt = deltaTime * TimeScale;
        
        foreach (var (id, body) in _bodies)
        {
            if (body.Type == PhysicsBodyType.Static) continue;

            if (body.Type == PhysicsBodyType.Dynamic)
                body.Velocity = body.Velocity with { Y = body.Velocity.Y + Gravity * dt };

            body.Entity.Position3D += body.Velocity * dt;
        }
    }

    public void ApplyForce(string entityId, Vector3 force)
    {
        if (_bodies.TryGetValue(entityId, out var body) && body.Type != PhysicsBodyType.Static)
            body.Velocity += force;
    }
}

public class PhysicsBody
{
    public SimEntity Entity { get; }
    public PhysicsBodyType Type { get; }
    public Vector3 Velocity { get; set; } = Vector3.Zero;
    public float Mass { get; set; } = 1.0f;

    public PhysicsBody(SimEntity entity, PhysicsBodyType type)
    {
        Entity = entity;
        Type = type;
    }
}

public enum PhysicsBodyType
{
    Static,
    Kinematic,
    Dynamic
}
