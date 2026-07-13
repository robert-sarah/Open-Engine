// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.GodMode
{
    public enum GodApproach { Benevolent, Hostile, Neutral, Absent }
    public enum DivineCommandType { Procreate, Gather, Build, Explore, Worship, Punishment, Reward }

    public class DivineCommand
    {
        public string Id { get; set; }
        public DivineCommandType Type { get; set; }
        public string Message { get; set; }
        public string? TargetAgentId { get; set; }
        public bool IsFulfilled { get; set; }
        public DateTime IssuedAt { get; set; }

        public DivineCommand(string message, DivineCommandType type)
        {
            Id = Guid.NewGuid().ToString();
            Message = message;
            Type = type;
            IssuedAt = DateTime.UtcNow;
        }
    }

    public class GodModeController
    {
        public GodApproach CurrentApproach { get; set; }
        public string GodName { get; set; }
        public bool IsActive { get; set; }
        public List<DivineCommand> CommandHistory { get; }

        public GodModeController()
        {
            CurrentApproach = GodApproach.Neutral;
            GodName = "The Creator";
            IsActive = false;
            CommandHistory = new List<DivineCommand>();
        }

        public DivineCommand IssueCommand(string message, DivineCommandType type, string? targetId = null)
        {
            var command = new DivineCommand(message, type) { TargetAgentId = targetId };
            CommandHistory.Add(command);
            return command;
        }

        public void SetApproach(GodApproach approach)
        {
            CurrentApproach = approach;
        }

        public string GetCommandMessage(DivineCommandType type)
        {
            return type switch
            {
                DivineCommandType.Procreate => CurrentApproach == GodApproach.Benevolent 
                    ? "I bless you with abundance. Create new life." 
                    : "Reproduce or face extinction.",
                DivineCommandType.Gather => "Collect resources for your survival.",
                DivineCommandType.Build => "Build shelter and tools.",
                DivineCommandType.Explore => "Explore the world around you.",
                DivineCommandType.Worship => "Acknowledge my presence.",
                DivineCommandType.Punishment => "You have displeased me. Face the consequences.",
                DivineCommandType.Reward => "You have pleased me. Receive my blessing.",
                _ => "Hear my words."
            };
        }
    }
}
