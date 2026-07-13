// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenEngine.LLM.Providers;

namespace OpenEngine.Core.Agents
{
    public class AgentBrain
    {
        public string AgentId { get; }
        public string AgentName { get; }
        public AgentMemory Memory { get; }
        public AgentSoul Soul { get; }
        private ILLMProvider? _llmProvider;

        public AgentBrain(string agentId, string agentName, ILLMProvider? llmProvider = null)
        {
            AgentId = agentId ?? throw new ArgumentNullException(nameof(agentId));
            AgentName = agentName ?? throw new ArgumentNullException(nameof(agentName));
            Memory = new AgentMemory();
            Soul = new AgentSoul();
            _llmProvider = llmProvider;
        }

        public void SetLLMProvider(ILLMProvider provider)
        {
            _llmProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void ProcessObservation(string observation, float emotionalImpact = 0f, bool isImportant = false)
        {
            Memory.AddMemory(observation, emotionalImpact, isImportant);
            
            // Update emotions based on observation
            if (emotionalImpact > 0.3f)
                Soul.AdjustEmotion(EmotionType.Joy, emotionalImpact * 0.5f);
            else if (emotionalImpact < -0.3f)
                Soul.AdjustEmotion(EmotionType.Fear, Math.Abs(emotionalImpact) * 0.5f);
        }

        public void ProcessInteraction(string otherAgentId, string otherAgentName, string interactionDescription, float impact)
        {
            // Add to memory
            Memory.AddMemory($"Interacted with {otherAgentName}: {interactionDescription}", impact);
            Memory.AddRelatedEntity($"Interacted with {otherAgentName}", otherAgentId);

            // Update relationship
            if (!Soul.Relationships.ContainsKey(otherAgentId))
            {
                Soul.AddOrUpdateRelationship(otherAgentId, otherAgentName, RelationshipType.Unknown);
            }

            Soul.UpdateRelationshipStrength(otherAgentId, impact * 0.1f);
            Soul.AddSharedMemory(otherAgentId, interactionDescription);

            // Determine relationship type based on accumulated interactions
            var rel = Soul.GetRelationship(otherAgentId);
            if (rel != null)
            {
                if (rel.Strength > 0.7f)
                    rel.Type = RelationshipType.Friendship;
                else if (rel.Strength < -0.5f)
                    rel.Type = RelationshipType.Hostile;
            }
        }

        public async Task<string> GenerateIntentAsync(string worldContext, string currentSituation)
        {
            if (_llmProvider == null)
            {
                return GenerateFallbackIntent(currentSituation);
            }

            var prompt = BuildIntentPrompt(worldContext, currentSituation);
            
            try
            {
                var response = await _llmProvider.GenerateResponseAsync(prompt);
                return response.Trim();
            }
            catch (Exception ex)
            {
                // Fallback to simple intent generation if LLM fails
                return GenerateFallbackIntent(currentSituation);
            }
        }

        private string BuildIntentPrompt(string worldContext, string currentSituation)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine($"You are {AgentName}, an agent in a simulated world.");
            sb.AppendLine($"Your current emotional state: {Soul.GetEmotionalStateDescription()}");
            sb.AppendLine();
            sb.AppendLine("Your recent memories:");
            sb.AppendLine(Memory.SummarizeMemories(5));
            sb.AppendLine();
            sb.AppendLine("Your relationships:");
            sb.AppendLine(Soul.DescribeRelationships());
            sb.AppendLine();
            sb.AppendLine("Current situation:");
            sb.AppendLine(currentSituation);
            sb.AppendLine();
            sb.AppendLine("World context:");
            sb.AppendLine(worldContext);
            sb.AppendLine();
            sb.AppendLine("Based on your memories, emotions, relationships, and the current situation, ");
            sb.AppendLine("express what you want to do in natural language. Be specific but avoid coordinates.");
            sb.AppendLine("For example: 'I want to gather berries from the nearby bush' or 'I want to check if the fire is still burning'");
            sb.AppendLine("Respond only with your intention, no additional text.");

            return sb.ToString();
        }

        private string GenerateFallbackIntent(string currentSituation)
        {
            // Simple rule-based intent generation when LLM is unavailable
            var dominantEmotion = Soul.GetDominantEmotion();
            
            if (dominantEmotion == EmotionType.Fear && Soul.GetEmotion(EmotionType.Fear) > 0.5f)
            {
                return "I want to stay near the fire and be cautious.";
            }
            
            if (dominantEmotion == EmotionType.Hunger)
            {
                return "I want to find food.";
            }

            if (dominantEmotion == EmotionType.Joy && Soul.GetEmotion(EmotionType.Joy) > 0.5f)
            {
                return "I want to share this moment with others.";
            }

            return "I want to observe my surroundings and see what I can do to help.";
        }

        public void UpdateEmotionalState()
        {
            // Decay emotions over time
            Soul.DecayEmotions(0.02f);

            // Adjust emotions based on relationships
            var hostileRels = Soul.GetRelationshipsByType(RelationshipType.Hostile);
            if (hostileRels.Count > 0)
            {
                Soul.AdjustEmotion(EmotionType.Fear, 0.1f * hostileRels.Count);
            }

            var closeRels = Soul.GetStrongestRelationships(3);
            if (closeRels.Count > 0)
            {
                Soul.AdjustEmotion(EmotionType.Joy, 0.05f * closeRels.Count);
            }
        }

        public string GetInternalState()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {AgentName}'s Internal State ===");
            sb.AppendLine();
            sb.AppendLine("Emotional State:");
            sb.AppendLine(Soul.GetEmotionalStateDescription());
            sb.AppendLine();
            sb.AppendLine("Dominant Emotion:");
            sb.AppendLine(Soul.GetDominantEmotion().ToString());
            sb.AppendLine();
            sb.AppendLine("Memory Count:");
            sb.AppendLine(Memory.MemoryCount.ToString());
            sb.AppendLine();
            sb.AppendLine("Relationship Count:");
            sb.AppendLine(Soul.Relationships.Count.ToString());
            sb.AppendLine();
            sb.AppendLine("Recent Memories:");
            sb.AppendLine(Memory.SummarizeMemories(3));
            
            return sb.ToString();
        }

        public void ReflectOnEvent(string eventDescription, float impact)
        {
            Memory.AddMemory($"Reflection: {eventDescription}", impact, true);
            
            if (impact > 0.5f)
            {
                Soul.AdjustEmotion(EmotionType.Joy, 0.2f);
                Soul.AdjustEmotion(EmotionType.Serenity, 0.1f);
            }
            else if (impact < -0.5f)
            {
                Soul.AdjustEmotion(EmotionType.Sadness, 0.2f);
                Soul.AdjustEmotion(EmotionType.Fear, 0.1f);
            }
        }
    }
}
