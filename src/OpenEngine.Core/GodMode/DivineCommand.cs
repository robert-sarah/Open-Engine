// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;
using OpenEngine.Core.Entities;

namespace OpenEngine.Core.GodMode
{
    public enum DivineCommandType
    {
        Blessing,        // Positive effect
        Curse,           // Negative effect
        Miracle,         // Special event
        Punishment,      // Direct punishment
        Revelation,      // Information reveal
        Intervention,    // Direct world change
        Summon,          // Spawn entity
        Destroy,         // Remove entity
        ControlWeather,  // Weather manipulation
        ControlTime      // Time manipulation
    }

    public enum DivineSeverity { Minor, Moderate, Major, Catastrophic }

    public class DivineCommand
    {
        public string Id { get; set; }
        public DivineCommandType Type { get; set; }
        public DivineSeverity Severity { get; set; }
        public string Description { get; set; }
        public string TargetEntityId { get; set; }
        public Vector3 TargetPosition { get; set; }
        public float Duration { get; set; }
        public float Magnitude { get; set; }
        public DateTime CastTime { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public DivineCommand()
        {
            Id = Guid.NewGuid().ToString();
            Parameters = new Dictionary<string, object>();
            CastTime = DateTime.UtcNow;
            IsActive = true;
            Magnitude = 1f;
        }

        public void Execute(GodModeController controller)
        {
            switch (Type)
            {
                case DivineCommandType.Blessing:
                    ExecuteBlessing(controller);
                    break;
                case DivineCommandType.Curse:
                    ExecuteCurse(controller);
                    break;
                case DivineCommandType.Miracle:
                    ExecuteMiracle(controller);
                    break;
                case DivineCommandType.Punishment:
                    ExecutePunishment(controller);
                    break;
                case DivineCommandType.Revelation:
                    ExecuteRevelation(controller);
                    break;
                case DivineCommandType.Intervention:
                    ExecuteIntervention(controller);
                    break;
                case DivineCommandType.Summon:
                    ExecuteSummon(controller);
                    break;
                case DivineCommandType.Destroy:
                    ExecuteDestroy(controller);
                    break;
                case DivineCommandType.ControlWeather:
                    ExecuteControlWeather(controller);
                    break;
                case DivineCommandType.ControlTime:
                    ExecuteControlTime(controller);
                    break;
            }
        }

        private void ExecuteBlessing(GodModeController controller)
        {
            if (!string.IsNullOrEmpty(TargetEntityId))
            {
                var entity = controller.GetEntity(TargetEntityId);
                if (entity != null)
                {
                    // Apply blessing effect
                    entity.Health = Math.Min(entity.Health + 20 * Magnitude, 100);
                    controller.AddDivineFavor(5);
                }
            }
        }

        private void ExecuteCurse(GodModeController controller)
        {
            if (!string.IsNullOrEmpty(TargetEntityId))
            {
                var entity = controller.GetEntity(TargetEntityId);
                if (entity != null)
                {
                    // Apply curse effect
                    entity.Health = Math.Max(entity.Health - 15 * Magnitude, 0);
                    controller.AddDivineFavor(-5);
                }
            }
        }

        private void ExecuteMiracle(GodModeController controller)
        {
            // Spawn resources or heal all entities
            controller.AddDivineFavor(10);
        }

        private void ExecutePunishment(GodModeController controller)
        {
            if (!string.IsNullOrEmpty(TargetEntityId))
            {
                var entity = controller.GetEntity(TargetEntityId);
                if (entity != null)
                {
                    entity.Health = 0;
                    controller.AddDivineFavor(-10);
                }
            }
        }

        private void ExecuteRevelation(GodModeController controller)
        {
            // Reveal information to agents
            controller.AddDivineFavor(2);
        }

        private void ExecuteIntervention(GodModeController controller)
        {
            // Direct world change
            controller.AddDivineFavor(3);
        }

        private void ExecuteSummon(GodModeController controller)
        {
            // Spawn entity at position
            controller.AddDivineFavor(5);
        }

        private void ExecuteDestroy(GodModeController controller)
        {
            if (!string.IsNullOrEmpty(TargetEntityId))
            {
                controller.RemoveEntity(TargetEntityId);
                controller.AddDivineFavor(-5);
            }
        }

        private void ExecuteControlWeather(GodModeController controller)
        {
            if (Parameters.TryGetValue("weatherType", out var weather))
            {
                controller.SetWeather(weather.ToString());
                controller.AddDivineFavor(3);
            }
        }

        private void ExecuteControlTime(GodModeController controller)
        {
            if (Parameters.TryGetValue("timeScale", out var scale))
            {
                if (scale is float s)
                {
                    controller.SetTimeScale(s);
                    controller.AddDivineFavor(2);
                }
            }
        }
    }

    public class DivineCommandBuilder
    {
        private DivineCommand _command;

        public DivineCommandBuilder()
        {
            _command = new DivineCommand();
        }

        public DivineCommandBuilder ofType(DivineCommandType type)
        {
            _command.Type = type;
            return this;
        }

        public DivineCommandBuilder withSeverity(DivineSeverity severity)
        {
            _command.Severity = severity;
            return this;
        }

        public DivineCommandBuilder targeting(string entityId)
        {
            _command.TargetEntityId = entityId;
            return this;
        }

        public DivineCommandBuilder atPosition(Vector3 position)
        {
            _command.TargetPosition = position;
            return this;
        }

        public DivineCommandBuilder withMagnitude(float magnitude)
        {
            _command.Magnitude = magnitude;
            return this;
        }

        public DivineCommandBuilder withDuration(float duration)
        {
            _command.Duration = duration;
            return this;
        }

        public DivineCommandBuilder withParameter(string key, object value)
        {
            _command.Parameters[key] = value;
            return this;
        }

        public DivineCommandBuilder withDescription(string description)
        {
            _command.Description = description;
            return this;
        }

        public DivineCommand Build()
        {
            return _command;
        }
    }
}
