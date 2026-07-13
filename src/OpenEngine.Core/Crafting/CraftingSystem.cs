// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEngine.Core.Crafting
{
    public enum ItemType
    {
        Shelter,
        Axe,
        Basket,
        Bed,
        Spear,
        Torch,
        Clothing,
        Tool
    }

    public class CraftingRecipe
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ItemType ResultType { get; set; }
        public Dictionary<string, float> RequiredMaterials { get; set; }
        public float CraftingTime { get; set; }
        public string Description { get; set; }

        public CraftingRecipe(string name, ItemType resultType, Dictionary<string, float> materials, float time = 2f)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            ResultType = resultType;
            RequiredMaterials = materials ?? new Dictionary<string, float>();
            CraftingTime = time;
            Description = $"Craft a {name}";
        }

        public bool CanCraft(Dictionary<string, float> availableMaterials)
        {
            foreach (var material in RequiredMaterials)
            {
                if (!availableMaterials.ContainsKey(material.Key) || availableMaterials[material.Key] < material.Value)
                {
                    return false;
                }
            }
            return true;
        }

        public List<string> GetMissingMaterials(Dictionary<string, float> availableMaterials)
        {
            var missing = new List<string>();
            foreach (var material in RequiredMaterials)
            {
                if (!availableMaterials.ContainsKey(material.Key) || availableMaterials[material.Key] < material.Value)
                {
                    var needed = material.Value - (availableMaterials.ContainsKey(material.Key) ? availableMaterials[material.Key] : 0);
                    missing.Add($"{material.Key} (need {needed:F1} more)");
                }
            }
            return missing;
        }
    }

    public class CraftingSystem
    {
        private List<CraftingRecipe> _recipes;
        private Dictionary<string, float> _globalMaterialAvailability;

        public List<CraftingRecipe> Recipes => _recipes;

        public CraftingSystem()
        {
            _recipes = new List<CraftingRecipe>();
            _globalMaterialAvailability = new Dictionary<string, float>();
            InitializeDefaultRecipes();
        }

        private void InitializeDefaultRecipes()
        {
            _recipes.Add(new CraftingRecipe("Shelter", ItemType.Shelter, new Dictionary<string, float>
            {
                { "wood", 10f },
                { "animal_skin", 5f }
            }, 5f));

            _recipes.Add(new CraftingRecipe("Axe", ItemType.Axe, new Dictionary<string, float>
            {
                { "flint", 2f },
                { "wood", 1f }
            }, 1f));

            _recipes.Add(new CraftingRecipe("Basket", ItemType.Basket, new Dictionary<string, float>
            {
                { "wood", 3f },
                { "fiber", 2f }
            }, 1.5f));

            _recipes.Add(new CraftingRecipe("Bed", ItemType.Bed, new Dictionary<string, float>
            {
                { "wood", 5f },
                { "animal_skin", 3f }
            }, 2f));

            _recipes.Add(new CraftingRecipe("Spear", ItemType.Spear, new Dictionary<string, float>
            {
                { "wood", 2f },
                { "flint", 1f },
                { "fiber", 1f }
            }, 1.5f));

            _recipes.Add(new CraftingRecipe("Torch", ItemType.Torch, new Dictionary<string, float>
            {
                { "wood", 1f },
                { "fiber", 1f }
            }, 0.5f));

            _recipes.Add(new CraftingRecipe("Clothing", ItemType.Clothing, new Dictionary<string, float>
            {
                { "animal_skin", 3f },
                { "fiber", 2f }
            }, 2f));
        }

        public void AddRecipe(CraftingRecipe recipe)
        {
            if (recipe != null)
            {
                _recipes.Add(recipe);
            }
        }

        public CraftingRecipe? GetRecipe(string recipeName)
        {
            return _recipes.FirstOrDefault(r => r.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));
        }

        public CraftingRecipe? GetRecipeById(string id)
        {
            return _recipes.FirstOrDefault(r => r.Id == id);
        }

        public List<CraftingRecipe> GetAvailableRecipes(Dictionary<string, float> availableMaterials)
        {
            return _recipes.Where(r => r.CanCraft(availableMaterials)).ToList();
        }

        public List<CraftingRecipe> GetRecipesByType(ItemType type)
        {
            return _recipes.Where(r => r.ResultType == type).ToList();
        }

        public bool TryCraft(string recipeName, Dictionary<string, float> inventory, out string result)
        {
            result = "";
            var recipe = GetRecipe(recipeName);
            
            if (recipe == null)
            {
                result = $"Recipe '{recipeName}' not found.";
                return false;
            }

            if (!recipe.CanCraft(inventory))
            {
                var missing = recipe.GetMissingMaterials(inventory);
                result = $"Cannot craft {recipeName}. Missing: {string.Join(", ", missing)}";
                return false;
            }

            // Consume materials
            foreach (var material in recipe.RequiredMaterials)
            {
                inventory[material.Key] -= material.Value;
                if (inventory[material.Key] <= 0)
                {
                    inventory.Remove(material.Key);
                }
            }

            result = $"Successfully crafted {recipeName} in {recipe.CraftingTime} hours.";
            return true;
        }

        public void UpdateGlobalMaterialAvailability(string material, float amount)
        {
            if (_globalMaterialAvailability.ContainsKey(material))
            {
                _globalMaterialAvailability[material] += amount;
            }
            else
            {
                _globalMaterialAvailability[material] = amount;
            }

            if (_globalMaterialAvailability[material] <= 0)
            {
                _globalMaterialAvailability.Remove(material);
            }
        }

        public Dictionary<string, float> GetGlobalMaterialAvailability()
        {
            return new Dictionary<string, float>(_globalMaterialAvailability);
        }

        public string GetRecipesSummary()
        {
            var summary = "Available Crafting Recipes:\n";
            foreach (var recipe in _recipes)
            {
                summary += $"\n{recipe.Name} ({recipe.CraftingTime}h):\n";
                foreach (var material in recipe.RequiredMaterials)
                {
                    summary += $"  - {material.Key}: {material.Value}\n";
                }
            }
            return summary;
        }
    }
}
