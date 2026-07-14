// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.GodMode
{
    public enum GodApproach { Benevolent, Hostile, Neutral, Absent }

    // Renommage pour éviter le conflit avec le DivineCommand du moteur physique
    public enum AgentDirectiveType 
    { 
        Procreate, 
        Gather, 
        Build, 
        Explore, 
        Worship, 
        Punishment, 
        Reward 
    }

    public class AgentDirective
    {
        public string Id { get; set; }
        public AgentDirectiveType Type { get; set; }
        public string Message { get; set; }
        public string? TargetAgentId { get; set; }
        public bool IsFulfilled { get; set; }
        public DateTime IssuedAt { get; set; }

        public AgentDirective(string message, AgentDirectiveType type)
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
        
        // Utilisation de la classe renommée
        public List<AgentDirective> CommandHistory { get; }

        public GodModeController()
        {
            CurrentApproach = GodApproach.Neutral;
            GodName = "The Creator";
            IsActive = false;
            CommandHistory = new List<AgentDirective>();
        }

        // Signature mise à jour pour refléter le changement de type
        public AgentDirective IssueCommand(string message, AgentDirectiveType type, string? targetId = null)
        {
            var directive = new AgentDirective(message, type) { TargetAgentId = targetId };
            CommandHistory.Add(directive);
            return directive;
        }

        public void SetApproach(GodApproach approach)
        {
            CurrentApproach = approach;
        }

        public string GetCommandMessage(AgentDirectiveType type)
        {
            return type switch
            {
                AgentDirectiveType.Procreate => CurrentApproach == GodApproach.Benevolent 
                    ? "I bless you with abundance. Create new life." 
                    : "Reproduce or face extinction.",
                AgentDirectiveType.Gather => "Collect resources for your survival.",
                AgentDirectiveType.Build => "Build shelter and tools.",
                AgentDirectiveType.Explore => "Explore the world around you.",
                AgentDirectiveType.Worship => "Acknowledge my presence.",
                AgentDirectiveType.Punishment => "You have displeased me. Face the consequences.",
                AgentDirectiveType.Reward => "You have pleased me. Receive my blessing.",
                _ => "Hear my words."
            };
        }
    }
}