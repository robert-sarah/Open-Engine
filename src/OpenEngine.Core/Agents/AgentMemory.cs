// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenEngine.Core.Agents
{
    public class MemoryEntry
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public float EmotionalImpact { get; set; }
        public List<string> RelatedEntityIds { get; set; }
        public string Location { get; set; }
        public bool IsImportant { get; set; }
        public int AccessCount { get; set; }

        public MemoryEntry(string description, float emotionalImpact = 0f)
        {
            Timestamp = DateTime.UtcNow;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            EmotionalImpact = emotionalImpact;
            RelatedEntityIds = new List<string>();
            Location = "";
            IsImportant = false;
            AccessCount = 0;
        }
    }

    public class AgentMemory
    {
        private List<MemoryEntry> _memories;
        private int _maxMemories;
        private TimeSpan _retentionPeriod;

        public List<MemoryEntry> Memories => _memories.OrderByDescending(m => m.Timestamp).ToList();
        public int MemoryCount => _memories.Count;

        public AgentMemory(int maxMemories = 1000, TimeSpan? retentionPeriod = null)
        {
            _memories = new List<MemoryEntry>();
            _maxMemories = maxMemories;
            _retentionPeriod = retentionPeriod ?? TimeSpan.FromDays(30);
        }

        public void AddMemory(string description, float emotionalImpact = 0f, bool isImportant = false)
        {
            var memory = new MemoryEntry(description, emotionalImpact)
            {
                IsImportant = isImportant
            };
            _memories.Add(memory);

            if (_memories.Count > _maxMemories)
            {
                ForgetOldMemories();
            }
        }

        public void AddRelatedEntity(string memoryDescription, string entityId)
        {
            var memory = _memories.FirstOrDefault(m => m.Description.Contains(memoryDescription));
            if (memory != null && !memory.RelatedEntityIds.Contains(entityId))
            {
                memory.RelatedEntityIds.Add(entityId);
            }
        }

        public List<MemoryEntry> RecallMemories(string? keyword = null, int limit = 10)
        {
            var relevant = _memories.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                relevant = relevant.Where(m => 
                    m.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    m.RelatedEntityIds.Any(id => id.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
            }

            relevant = relevant.OrderByDescending(m => m.IsImportant)
                              .ThenByDescending(m => Math.Abs(m.EmotionalImpact))
                              .ThenByDescending(m => m.Timestamp);

            var result = relevant.Take(limit).ToList();
            
            foreach (var memory in result)
            {
                memory.AccessCount++;
            }

            return result;
        }

        public List<MemoryEntry> RecallMemoriesAboutEntity(string entityId, int limit = 10)
        {
            var relevant = _memories.Where(m => m.RelatedEntityIds.Contains(entityId))
                                   .OrderByDescending(m => m.Timestamp)
                                   .Take(limit)
                                   .ToList();

            foreach (var memory in relevant)
            {
                memory.AccessCount++;
            }

            return relevant;
        }

        public List<MemoryEntry> GetRecentMemories(TimeSpan timeSpan, int limit = 20)
        {
            var cutoff = DateTime.UtcNow - timeSpan;
            return _memories.Where(m => m.Timestamp >= cutoff)
                           .OrderByDescending(m => m.Timestamp)
                           .Take(limit)
                           .ToList();
        }

        public List<MemoryEntry> GetEmotionalMemories(float minImpact = 0.5f, int limit = 10)
        {
            return _memories.Where(m => Math.Abs(m.EmotionalImpact) >= minImpact)
                           .OrderByDescending(m => Math.Abs(m.EmotionalImpact))
                           .Take(limit)
                           .ToList();
        }

        public void ForgetOldMemories()
        {
            var cutoff = DateTime.UtcNow - _retentionPeriod;
            var toKeep = _memories.Where(m => m.IsImportant || m.Timestamp >= cutoff).ToList();
            
            if (toKeep.Count > _maxMemories)
            {
                var nonImportant = toKeep.Where(m => !m.IsImportant)
                                       .OrderBy(m => m.AccessCount)
                                       .Take(toKeep.Count - _maxMemories)
                                       .ToList();
                toKeep = toKeep.Except(nonImportant).ToList();
            }
            
            _memories = toKeep;
        }

        public string SummarizeMemories(int limit = 5)
        {
            var recent = RecallMemories(limit: limit);
            if (recent.Count == 0)
                return "I have no memories yet.";

            var summary = "My recent memories:\n";
            foreach (var memory in recent)
            {
                var impact = memory.EmotionalImpact > 0 ? "(positive)" : memory.EmotionalImpact < 0 ? "(negative)" : "";
                summary += $"- {memory.Description} {impact}\n";
            }
            return summary;
        }
    }
}
