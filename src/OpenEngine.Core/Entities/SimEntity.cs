// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Entities
{
    public class SimEntity
    {
        public string Id { get; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string? ParentId { get; set; }
        public Vector3 Position3D { get; set; }
        public Vector3 Rotation3D { get; set; }
        public Vector3 Scale3D { get; set; }
        public Dictionary<string, float> Attributes { get; }
        public List<string> Capabilities { get; }
        public List<string> Tags { get; }
        public Dictionary<string, object> Components { get; }

        public SimEntity(string id, string name, string type)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            ParentId = null;
            Position3D = Vector3.Zero;
            Rotation3D = Vector3.Zero;
            Scale3D = Vector3.One;
            Attributes = new Dictionary<string, float>();
            Capabilities = new List<string>();
            Tags = new List<string>();
            Components = new Dictionary<string, object>();
        }

        public void AddAttribute(string key, float value)
        {
            Attributes[key] = value;
        }

        public bool TryGetAttribute(string key, out float value)
        {
            return Attributes.TryGetValue(key, out value);
        }

        public void AddCapability(string capability)
        {
            if (!Capabilities.Contains(capability))
                Capabilities.Add(capability);
        }

        public void AddTag(string tag)
        {
            if (!Tags.Contains(tag))
                Tags.Add(tag);
        }

        public bool HasTag(string tag) => Tags.Contains(tag);

        public void AddComponent(string key, object component)
        {
            Components[key] = component ?? throw new ArgumentNullException(nameof(component));
        }

        public T GetComponent<T>(string key) where T : class
        {
            if (Components.TryGetValue(key, out object component))
                return component as T;
            return null;
        }

        public bool TryGetComponent<T>(string key, out T component) where T : class
        {
            component = GetComponent<T>(key);
            return component != null;
        }

        public void RemoveComponent(string key)
        {
            Components.Remove(key);
        }

        public SimEntity Clone()
        {
            var clone = new SimEntity(Id + "_clone", Name, Type)
            {
                ParentId = ParentId,
                Position3D = Position3D,
                Rotation3D = Rotation3D,
                Scale3D = Scale3D
            };

            foreach (var attr in Attributes)
                clone.Attributes[attr.Key] = attr.Value;

            clone.Capabilities.AddRange(Capabilities);
            clone.Tags.AddRange(Tags);

            foreach (var comp in Components)
                clone.Components[comp.Key] = comp.Value;

            return clone;
        }

        public override string ToString()
        {
            return $"SimEntity[{Id}] {Name} ({Type}) @ {Position3D}";
        }
    }
}
