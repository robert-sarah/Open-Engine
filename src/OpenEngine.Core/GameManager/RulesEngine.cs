// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.GameManager
{
    public class ActionResult
    {
        public bool Success { get; set; }
        public string Description { get; set; }
        public float TimeElapsed { get; set; }
        public Dictionary<string, object> Results { get; set; }

        public ActionResult(bool success, string description, float timeElapsed = 0f)
        {
            Success = success;
            Description = description;
            TimeElapsed = timeElapsed;
            Results = new Dictionary<string, object>();
        }
    }

    public class RulesEngine
    {
        private Dictionary<ResourceType, float> _resourceValues;
        private Dictionary<string, float> _hungerRates;
        private Dictionary<string, float> _movementSpeeds;

        public RulesEngine()
        {
            _resourceValues = new Dictionary<ResourceType, float>
            {
                { ResourceType.Berries, 5f },
                { ResourceType.Meat, 15f },
                { ResourceType.Water, 3f },
                { ResourceType.Wood, 2f },
                { ResourceType.Flint, 1f },
                { ResourceType.AnimalSkin, 8f },
                { ResourceType.Stone, 2f }
            };

            _hungerRates = new Dictionary<string, float>
            {
                { "human", 1.0f },
                { "adult", 1.0f },
                { "child", 0.7f },
                { "elder", 0.8f }
            };

            _movementSpeeds = new Dictionary<string, float>
            {
                { "human", 5f },
                { "adult", 5f },
                { "child", 3f },
                { "elder", 3f }
            };
        }

        public float CalculateHunger(string entityType, float timeHours)
        {
            var rate = _hungerRates.ContainsKey(entityType) ? _hungerRates[entityType] : 1.0f;
            return rate * timeHours;
        }

        public float CalculateMovementTime(string entityType, float distance)
        {
            var speed = _movementSpeeds.ContainsKey(entityType) ? _movementSpeeds[entityType] : 5f;
            return distance / speed;
        }

        public ActionResult ResolveCombat(string attackerId, string defenderId, float attackerStrength, float defenderStrength)
        {
            var random = new Random();
            var attackerRoll = attackerStrength * (0.8f + (float)random.NextDouble() * 0.4f);
            var defenderRoll = defenderStrength * (0.8f + (float)random.NextDouble() * 0.4f);

            var result = new ActionResult(true, "", 0.5f);

            if (attackerRoll > defenderRoll * 1.2f)
            {
                result.Success = true;
                result.Description = "Attack successful. Defender is wounded.";
                result.Results["damage"] = attackerRoll - defenderRoll;
                result.Results["defender_status"] = "wounded";
            }
            else if (defenderRoll > attackerRoll * 1.2f)
            {
                result.Success = false;
                result.Description = "Attack failed. Attacker is wounded.";
                result.Results["damage"] = defenderRoll - attackerRoll;
                result.Results["attacker_status"] = "wounded";
            }
            else
            {
                result.Success = false;
                result.Description = "Attack failed. Both parties are evenly matched.";
                result.Results["damage"] = 0f;
                result.Results["status"] = "stalemate";
            }

            return result;
        }

        public ActionResult ProcessHarvest(ResourceType type, float amount, string toolType = "none")
        {
            var baseAmount = amount;
            var toolBonus = toolType.ToLower() switch
            {
                "axe" => 1.5f,
                "basket" => 2.0f,
                "hands" => 1.0f,
                _ => 1.0f
            };

            var harvested = baseAmount * toolBonus;
            var timeRequired = baseAmount / toolBonus;

            return new ActionResult(true, $"Harvested {harvested:F1} {type} using {toolType}", timeRequired)
            {
                Results = { ["amount"] = harvested, ["type"] = type.ToString() }
            };
        }

        public ActionResult ProcessCrafting(string recipe, Dictionary<string, float> availableMaterials)
        {
            var recipes = GetCraftingRecipes();
            
            if (!recipes.ContainsKey(recipe))
            {
                return new ActionResult(false, $"Unknown recipe: {recipe}");
            }

            var required = recipes[recipe];
            var canCraft = true;
            var missing = new List<string>();

            foreach (var material in required)
            {
                if (!availableMaterials.ContainsKey(material.Key) || availableMaterials[material.Key] < material.Value)
                {
                    canCraft = false;
                    missing.Add($"{material.Key} (need {material.Value})");
                }
            }

            if (!canCraft)
            {
                return new ActionResult(false, $"Missing materials: {string.Join(", ", missing)}");
            }

            return new ActionResult(true, $"Successfully crafted {recipe}", 2f)
            {
                Results = { ["item"] = recipe }
            };
        }

        private Dictionary<string, Dictionary<string, float>> GetCraftingRecipes()
        {
            return new Dictionary<string, Dictionary<string, float>>
            {
                {
                    "shelter", new Dictionary<string, float>
                    {
                        { "wood", 10f },
                        { "animal_skin", 5f }
                    }
                },
                {
                    "axe", new Dictionary<string, float>
                    {
                        { "flint", 2f },
                        { "wood", 1f }
                    }
                },
                {
                    "basket", new Dictionary<string, float>
                    {
                        { "wood", 3f },
                        { "animal_skin", 1f }
                    }
                },
                {
                    "bed", new Dictionary<string, float>
                    {
                        { "wood", 5f },
                        { "animal_skin", 3f }
                    }
                }
            };
        }

        public float GetResourceValue(ResourceType type)
        {
            return _resourceValues.ContainsKey(type) ? _resourceValues[type] : 1f;
        }

        public ActionResult CheckSurvivalNeeds(float hunger, float energy, float health)
        {
            var issues = new List<string>();

            if (hunger > 80f)
                issues.Add("Starving");
            else if (hunger > 50f)
                issues.Add("Very hungry");

            if (energy < 20f)
                issues.Add("Exhausted");
            else if (energy < 40f)
                issues.Add("Tired");

            if (health < 30f)
                issues.Add("Critically wounded");
            else if (health < 60f)
                issues.Add("Wounded");

            if (issues.Count == 0)
            {
                return new ActionResult(true, "All survival needs are met", 0f);
            }

            return new ActionResult(false, $"Survival issues: {string.Join(", ", issues)}", 0f);
        }

        public float CalculateFireConsumption(WeatherType weather, float timeHours)
        {
            var baseRate = 2f; // wood per hour
            var weatherMultiplier = weather switch
            {
                WeatherType.Clear => 1.0f,
                WeatherType.Cloudy => 1.2f,
                WeatherType.Rain => 2.0f,
                WeatherType.Storm => 3.0f,
                WeatherType.Snow => 1.5f,
                _ => 1.0f
            };

            return baseRate * weatherMultiplier * timeHours;
        }
    }
}
