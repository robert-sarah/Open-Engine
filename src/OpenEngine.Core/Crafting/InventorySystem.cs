// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEngine.Core.Crafting
{
    public class InventoryItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public float Amount { get; set; }
        public float WeightPerUnit { get; set; }
        public float MaxStack { get; set; }

        public float TotalWeight => Amount * WeightPerUnit;

        public InventoryItem(string name, string type, float amount, float weightPerUnit = 1f, float maxStack = 100f)
        {
            Id = Guid.NewGuid().ToString();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? "misc";
            Amount = amount;
            WeightPerUnit = weightPerUnit;
            MaxStack = maxStack;
        }
    }

    public class InventorySystem
    {
        private Dictionary<string, InventoryItem> _items;
        private float _maxCapacity;
        private float _currentWeight;

        public Dictionary<string, InventoryItem> Items => _items;
        public float MaxCapacity => _maxCapacity;
        public float CurrentWeight => _currentWeight;
        public float AvailableCapacity => _maxCapacity - _currentWeight;

        public InventorySystem(float maxCapacity = 100f)
        {
            _items = new Dictionary<string, InventoryItem>();
            _maxCapacity = maxCapacity;
            _currentWeight = 0f;
        }

        public bool AddItem(string name, string type, float amount, float weightPerUnit = 1f, float maxStack = 100f)
        {
            var totalWeight = amount * weightPerUnit;

            if (_currentWeight + totalWeight > _maxCapacity)
            {
                return false;
            }

            if (_items.ContainsKey(name))
            {
                var existing = _items[name];
                var canAdd = Math.Min(amount, existing.MaxStack - existing.Amount);
                existing.Amount += canAdd;
                _currentWeight += canAdd * weightPerUnit;
                return canAdd >= amount;
            }
            else
            {
                var item = new InventoryItem(name, type, amount, weightPerUnit, maxStack);
                _items[name] = item;
                _currentWeight += totalWeight;
                return true;
            }
        }

        public bool RemoveItem(string name, float amount)
        {
            if (!_items.ContainsKey(name))
            {
                return false;
            }

            var item = _items[name];
            if (item.Amount < amount)
            {
                return false;
            }

            item.Amount -= amount;
            _currentWeight -= amount * item.WeightPerUnit;

            if (item.Amount <= 0)
            {
                _items.Remove(name);
            }

            return true;
        }

        public bool HasItem(string name)
        {
            return _items.ContainsKey(name) && _items[name].Amount > 0;
        }

        public float GetItemCount(string name)
        {
            return _items.ContainsKey(name) ? _items[name].Amount : 0f;
        }

        public Dictionary<string, float> GetItemAmounts()
        {
            return _items.ToDictionary(i => i.Key, i => i.Value.Amount);
        }

        public bool TransferItem(string itemName, float amount, InventorySystem targetInventory)
        {
            if (!HasItem(itemName) || GetItemCount(itemName) < amount)
            {
                return false;
            }

            var item = _items[itemName];
            var weightPerUnit = item.WeightPerUnit;
            var maxStack = item.MaxStack;

            if (targetInventory.AddItem(itemName, item.Type, amount, weightPerUnit, maxStack))
            {
                RemoveItem(itemName, amount);
                return true;
            }

            return false;
        }

        public void SetMaxCapacity(float capacity)
        {
            _maxCapacity = Math.Max(0, capacity);
        }

        public List<InventoryItem> GetItemsByType(string type)
        {
            return _items.Values.Where(i => i.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public string GetInventorySummary()
        {
            if (_items.Count == 0)
            {
                return "Inventory is empty.";
            }

            var summary = $"Inventory ({_currentWeight:F1}/{_maxCapacity:F1} capacity):\n";
            foreach (var item in _items.Values)
            {
                summary += $"- {item.Name}: {item.Amount:F1} ({item.TotalWeight:F1} weight)\n";
            }
            return summary;
        }

        public void Clear()
        {
            _items.Clear();
            _currentWeight = 0f;
        }
    }
}
