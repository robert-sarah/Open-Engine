// Created By Levi Enama
using System;

namespace OpenEngine.Core.Entities
{
    public enum ConnectionPredicate
    {
        ORBITS,
        TRADES_WITH,
        HOSTILE_TO,
        ALLIED_WITH,
        CONSUMES_ENERGY,
        PRODUCES_ENERGY,
        REPLENISHES_AT,
        DEFENDS,
        ATTACKS,
        FOLLOWS,
        LEADS,
        CONTAINS,
        BELONGS_TO,
        COMMUNICATES_WITH,
        SHARES_RESOURCES_WITH,
        COMPETES_WITH,
        COOPERATES_WITH,
        DOCKED_AT
    }

    public class SystemicConnection
    {
        public string Id { get; }
        public string SourceId { get; }
        public string TargetId { get; }
        public ConnectionPredicate Predicate { get; set; }
        public float Strength { get; set; }
        public bool IsBidirectional { get; set; }
        public DateTime CreatedAt { get; }
        public DateTime? UpdatedAt { get; private set; }
        public Dictionary<string, object> Metadata { get; }

        public SystemicConnection(string id, string sourceId, string targetId, ConnectionPredicate predicate)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            SourceId = sourceId ?? throw new ArgumentNullException(nameof(sourceId));
            TargetId = targetId ?? throw new ArgumentNullException(nameof(targetId));
            Predicate = predicate;
            Strength = 1.0f;
            IsBidirectional = false;
            CreatedAt = DateTime.UtcNow;
            Metadata = new Dictionary<string, object>();
        }

        public void UpdateStrength(float newStrength)
        {
            Strength = System.Math.Clamp(newStrength, 0, 1);
            UpdatedAt = DateTime.UtcNow;
        }

        public SystemicConnection? CreateReverse()
        {
            if (!IsBidirectional)
                return null;

            return new SystemicConnection(Id + "_rev", TargetId, SourceId, Predicate)
            {
                Strength = Strength,
                IsBidirectional = true
            };
        }

        public override string ToString()
        {
            return $"Connection[{Id}]: {SourceId} {Predicate} {TargetId} (Strength: {Strength:F2})";
        }
    }
}
