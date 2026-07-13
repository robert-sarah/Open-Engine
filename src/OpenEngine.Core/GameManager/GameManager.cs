// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;
using OpenEngine.Core.Agents;
using OpenEngine.LLM.Providers;

namespace OpenEngine.Core.GameManager
{
    public class GameManager
    {
        private WorldState _worldState;
        private RulesEngine _rulesEngine;
        private ILLMProvider? _llmProvider;
        private Dictionary<string, AgentBrain> _agentBrains;

        public WorldState WorldState => _worldState;
        public RulesEngine RulesEngine => _rulesEngine;

        public GameManager(ILLMProvider? llmProvider = null)
        {
            _worldState = new WorldState();
            _rulesEngine = new RulesEngine();
            _llmProvider = llmProvider;
            _agentBrains = new Dictionary<string, AgentBrain>();
        }

        public void SetLLMProvider(ILLMProvider provider)
        {
            _llmProvider = provider;
            foreach (var brain in _agentBrains.Values)
            {
                brain.SetLLMProvider(provider);
            }
        }

        public void RegisterAgent(AgentBrain brain)
        {
            if (brain != null && !_agentBrains.ContainsKey(brain.AgentId))
            {
                _agentBrains[brain.AgentId] = brain;
                if (_llmProvider != null)
                {
                    brain.SetLLMProvider(_llmProvider);
                }
            }
        }

        public AgentBrain? GetAgentBrain(string agentId)
        {
            return _agentBrains.ContainsKey(agentId) ? _agentBrains[agentId] : null;
        }

        public async Task<string> ProcessAgentTurnAsync(string agentId)
        {
            var brain = GetAgentBrain(agentId);
            if (brain == null)
                return "Agent not found.";

            var entity = _worldState.GetEntity(agentId);
            if (entity == null)
                return "Agent entity not found in world.";

            // Update agent's emotional state
            brain.UpdateEmotionalState();

            // Get current context
            var context = _worldState.GetVisibleContext(agentId);
            var situation = BuildCurrentSituation(entity);

            // Generate intent from agent
            var intent = await brain.GenerateIntentAsync(context, situation);

            // Translate intent to actions and execute
            var result = await TranslateAndExecuteIntentAsync(agentId, intent);

            // Provide feedback to agent
            brain.ProcessObservation(result, 0.1f);

            return result;
        }

        private string BuildCurrentSituation(SimEntity entity)
        {
            var sb = new StringBuilder();
            
            // Check survival needs
            entity.TryGetAttribute("hunger", out float hunger);
            entity.TryGetAttribute("energy", out float energy);
            entity.TryGetAttribute("health", out float health);

            sb.AppendLine($"My current state:");
            sb.AppendLine($"- Hunger: {hunger:F0}/100");
            sb.AppendLine($"- Energy: {energy:F0}/100");
            sb.AppendLine($"- Health: {health:F0}/100");

            // Check inventory
            if (entity.TryGetComponent<Dictionary<string, float>>("inventory", out var inventory))
            {
                sb.AppendLine($"\nMy inventory:");
                foreach (var item in inventory)
                {
                    sb.AppendLine($"- {item.Key}: {item.Value:F1}");
                }
            }

            return sb.ToString();
        }

        private async Task<string> TranslateAndExecuteIntentAsync(string agentId, string intent)
        {
            if (_llmProvider == null)
            {
                return ExecuteSimpleIntent(agentId, intent);
            }

            var prompt = BuildTranslationPrompt(agentId, intent);
            
            try
            {
                var translation = await _llmProvider.GenerateResponseAsync(prompt);
                return ExecuteTranslatedActions(agentId, translation);
            }
            catch
            {
                return ExecuteSimpleIntent(agentId, intent);
            }
        }

        private string BuildTranslationPrompt(string agentId, string intent)
        {
            var entity = _worldState.GetEntity(agentId);
            var context = _worldState.GetVisibleContext(agentId);

            var sb = new StringBuilder();
            sb.AppendLine("You are the Game Manager / Dungeon Master for a simulation.");
            sb.AppendLine();
            sb.AppendLine("Agent Intent:");
            sb.AppendLine(intent);
            sb.AppendLine();
            sb.AppendLine("World Context:");
            sb.AppendLine(context);
            sb.AppendLine();
            sb.AppendLine("Your task: Translate this intent into specific game actions.");
            sb.AppendLine("Respond with a structured action plan in this format:");
            sb.AppendLine("ACTION: [action_type]");
            sb.AppendLine("TARGET: [target_entity_or_resource]");
            sb.AppendLine("COORDINATES: [x,y,z if applicable]");
            sb.AppendLine("DESCRIPTION: [natural language description of what happened]");
            sb.AppendLine();
            sb.AppendLine("Available action types: MOVE, HARVEST, ATTACK, CRAFT, GIVE, REST, SOCIALIZE");

            return sb.ToString();
        }

        private string ExecuteTranslatedActions(string agentId, string translation)
        {
            var entity = _worldState.GetEntity(agentId);
            if (entity == null)
                return "Entity not found.";

            // Parse the translation and execute
            var lines = translation.Split('\n');
            var actionType = "UNKNOWN";
            var target = "";
            var description = "Action processed.";

            foreach (var line in lines)
            {
                if (line.StartsWith("ACTION:"))
                    actionType = line.Substring(7).Trim();
                else if (line.StartsWith("TARGET:"))
                    target = line.Substring(7).Trim();
                else if (line.StartsWith("DESCRIPTION:"))
                    description = line.Substring(12).Trim();
            }

            return ExecuteAction(agentId, actionType, target, description);
        }

        private string ExecuteSimpleIntent(string agentId, string intent)
        {
            var lowerIntent = intent.ToLower();

            if (lowerIntent.Contains("move") || lowerIntent.Contains("go"))
            {
                return ExecuteAction(agentId, "MOVE", "", "You moved to a new location.");
            }
            else if (lowerIntent.Contains("gather") || lowerIntent.Contains("harvest") || lowerIntent.Contains("collect"))
            {
                return ExecuteAction(agentId, "HARVEST", "berries", "You gathered some resources.");
            }
            else if (lowerIntent.Contains("attack") || lowerIntent.Contains("fight"))
            {
                return ExecuteAction(agentId, "ATTACK", "", "You attempted to attack.");
            }
            else if (lowerIntent.Contains("craft") || lowerIntent.Contains("build"))
            {
                return ExecuteAction(agentId, "CRAFT", "", "You attempted to craft something.");
            }
            else if (lowerIntent.Contains("give") || lowerIntent.Contains("share"))
            {
                return ExecuteAction(agentId, "GIVE", "", "You shared something with someone.");
            }
            else if (lowerIntent.Contains("rest") || lowerIntent.Contains("sleep"))
            {
                return ExecuteAction(agentId, "REST", "", "You rested for a while.");
            }
            else if (lowerIntent.Contains("talk") || lowerIntent.Contains("speak"))
            {
                return ExecuteAction(agentId, "SOCIALIZE", "", "You socialized with others.");
            }

            return ExecuteAction(agentId, "IDLE", "", "You stood still and observed.");
        }

        private string ExecuteAction(string agentId, string actionType, string target, string description)
        {
            var entity = _worldState.GetEntity(agentId);
            if (entity == null)
                return "Entity not found.";

            var result = new StringBuilder();
            result.AppendLine($"Action: {actionType}");

            switch (actionType.ToUpper())
            {
                case "MOVE":
                    result.AppendLine(description);
                    result.AppendLine("You moved through the world.");
                    break;

                case "HARVEST":
                    var resource = _worldState.FindNearestResource(entity.Position3D, ResourceType.Berries, 50f);
                    if (resource != null)
                    {
                        var harvestResult = _rulesEngine.ProcessHarvest(resource.Type, 5f, "hands");
                        result.AppendLine($"Harvested {harvestResult.Results["amount"]} {resource.Type}");
                        resource.Amount -= (float)harvestResult.Results["amount"];
                    }
                    else
                    {
                        result.AppendLine("No resources found nearby.");
                    }
                    break;

                case "ATTACK":
                    result.AppendLine(description);
                    result.AppendLine("Combat mechanics would be applied here.");
                    break;

                case "CRAFT":
                    result.AppendLine(description);
                    result.AppendLine("Crafting mechanics would be applied here.");
                    break;

                case "GIVE":
                    result.AppendLine(description);
                    result.AppendLine("You gave something to someone.");
                    break;

                case "REST":
                    entity.AddAttribute("energy", Math.Min(100f, entity.Attributes.GetValueOrDefault("energy", 50f) + 20f));
                    result.AppendLine("You rested and recovered some energy.");
                    break;

                case "SOCIALIZE":
                    result.AppendLine(description);
                    result.AppendLine("You interacted with others.");
                    break;

                default:
                    result.AppendLine("You waited and observed.");
                    break;
            }

            return result.ToString();
        }

        public void UpdateWorld(float deltaTime)
        {
            _worldState.UpdateWeather();
            _worldState.AdvanceTime(deltaTime);

            // Regenerate resources over time
            RegenerateResources();
        }

        private void RegenerateResources()
        {
            var random = new Random();
            foreach (var resource in _worldState.ResourceNodes)
            {
                if (resource.IsDepleted && random.NextDouble() < 0.01f)
                {
                    resource.Amount = resource.MaxAmount * 0.5f;
                }
            }
        }

        public string GetWorldSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== World State ===");
            sb.AppendLine($"Weather: {_worldState.CurrentWeather}");
            sb.AppendLine($"Temperature: {_worldState.Temperature:F1}°C");
            sb.AppendLine($"Time: {(_worldState.IsDay ? "Day" : "Night")}");
            sb.AppendLine($"Entities: {_worldState.AllEntities.Count}");
            sb.AppendLine($"Resources: {_worldState.ResourceNodes.Count}");
            sb.AppendLine($"Registered Agents: {_agentBrains.Count}");

            return sb.ToString();
        }
    }
}
