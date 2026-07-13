// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Intent
{
    public enum ActionVerb
    {
        MOVE_TO,
        ATTACK,
        TRADE,
        HARVEST,
        IDLE,
        ESCAPE,
        DEFEND,
        GATHER,
        BUILD,
        RESEARCH,
        EXPLORE,
        PATROL,
        RETREAT,
        FOLLOW,
        GUARD
    }

    public struct EngineActionEvent : IEquatable<EngineActionEvent>
    {
        public string ActorId { get; set; }
        public ActionVerb Action { get; set; }
        public string? TargetEntityId { get; set; }
        public Vector3 TargetCoordinates3D { get; set; }
        public float IntensityValue { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

        public EngineActionEvent(string actorId, ActionVerb action)
        {
            ActorId = actorId ?? throw new ArgumentNullException(nameof(actorId));
            Action = action;
            TargetEntityId = null;
            TargetCoordinates3D = Vector3.Zero;
            IntensityValue = 1.0f;
            Metadata = new Dictionary<string, object>();
        }

        public bool Equals(EngineActionEvent other)
        {
            return ActorId == other.ActorId
                && Action == other.Action
                && TargetEntityId == other.TargetEntityId
                && TargetCoordinates3D == other.TargetCoordinates3D
                && System.Math.Abs(IntensityValue - other.IntensityValue) < float.Epsilon;
        }

        public override bool Equals(object? obj)
            => obj is EngineActionEvent other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(ActorId, Action, TargetEntityId, TargetCoordinates3D, IntensityValue);

        public static bool operator ==(EngineActionEvent left, EngineActionEvent right)
            => left.Equals(right);

        public static bool operator !=(EngineActionEvent left, EngineActionEvent right)
            => !left.Equals(right);

        public override string ToString()
        {
            return $"ActionEvent: {ActorId} {Action} {(TargetEntityId != null ? TargetEntityId : TargetCoordinates3D.ToString())}";
        }
    }

    public class UniversalIntentParser
    {
        private static readonly Dictionary<string, ActionVerb> KeywordMap = new Dictionary<string, ActionVerb>(StringComparer.OrdinalIgnoreCase)
        {
            { "move", ActionVerb.MOVE_TO },
            { "go", ActionVerb.MOVE_TO },
            { "travel", ActionVerb.MOVE_TO },
            { "head", ActionVerb.MOVE_TO },
            { "approach", ActionVerb.MOVE_TO },
            { "attack", ActionVerb.ATTACK },
            { "fight", ActionVerb.ATTACK },
            { "engage", ActionVerb.ATTACK },
            { "strike", ActionVerb.ATTACK },
            { "trade", ActionVerb.TRADE },
            { "barter", ActionVerb.TRADE },
            { "exchange", ActionVerb.TRADE },
            { "harvest", ActionVerb.HARVEST },
            { "collect", ActionVerb.HARVEST },
            { "gather", ActionVerb.GATHER },
            { "mine", ActionVerb.HARVEST },
            { "idle", ActionVerb.IDLE },
            { "wait", ActionVerb.IDLE },
            { "stay", ActionVerb.IDLE },
            { "stop", ActionVerb.IDLE },
            { "escape", ActionVerb.ESCAPE },
            { "flee", ActionVerb.ESCAPE },
            { "run", ActionVerb.ESCAPE },
            { "retreat", ActionVerb.RETREAT },
            { "defend", ActionVerb.DEFEND },
            { "protect", ActionVerb.DEFEND },
            { "guard", ActionVerb.GUARD },
            { "build", ActionVerb.BUILD },
            { "construct", ActionVerb.BUILD },
            { "research", ActionVerb.RESEARCH },
            { "study", ActionVerb.RESEARCH },
            { "explore", ActionVerb.EXPLORE },
            { "scout", ActionVerb.EXPLORE },
            { "patrol", ActionVerb.PATROL }
        };

        private static readonly Regex CoordinateRegex = new Regex(
            @"position|coords?|location|at|to\s*(?::|=)?\s*\(?\s*([+-]?\d*\.?\d+)\s*[,;]\s*([+-]?\d*\.?\d+)\s*[,;]\s*([+-]?\d*\.?\d+)\s*\)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex TargetEntityRegex = new Regex(
            @"target|toward|to|at|attack|defend|follow\s+(entity|unit|object|ship|station|building)?\s*['""]?([\w_-]+)['""]?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex IntensityRegex = new Regex(
            @"intensity|speed|power|force|strength\s*(?::|=)?\s*(\d*\.?\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public List<EngineActionEvent> ParseIntent(string rawInput, string actorId)
        {
            var actions = new List<EngineActionEvent>();

            if (string.IsNullOrWhiteSpace(rawInput))
                return actions;

            var sentences = rawInput.Split(new[] { '.', '!', '?', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var sentence in sentences)
            {
                var action = ParseSingleSentence(sentence.Trim(), actorId);
                if (action.HasValue)
                    actions.Add(action.Value);
            }

            if (actions.Count == 0)
            {
                var fallback = FallbackRegexParse(rawInput, actorId);
                if (fallback.HasValue)
                    actions.Add(fallback.Value);
            }

            return actions;
        }

        private EngineActionEvent? ParseSingleSentence(string sentence, string actorId)
        {
            ActionVerb? detectedVerb = null;

            foreach (var kvp in KeywordMap)
            {
                if (sentence.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    detectedVerb = kvp.Value;
                    break;
                }
            }

            if (!detectedVerb.HasValue)
                return null;

            var actionEvent = new EngineActionEvent(actorId, detectedVerb.Value);

            var coordMatch = CoordinateRegex.Match(sentence);
            if (coordMatch.Success && coordMatch.Groups.Count >= 4)
            {
                if (float.TryParse(coordMatch.Groups[1].Value, out float x)
                    && float.TryParse(coordMatch.Groups[2].Value, out float y)
                    && float.TryParse(coordMatch.Groups[3].Value, out float z))
                {
                    actionEvent.TargetCoordinates3D = new Vector3(x, y, z);
                }
            }

            var targetMatch = TargetEntityRegex.Match(sentence);
            if (targetMatch.Success && targetMatch.Groups.Count >= 3)
            {
                actionEvent.TargetEntityId = targetMatch.Groups[2].Value;
            }

            var intensityMatch = IntensityRegex.Match(sentence);
            if (intensityMatch.Success && intensityMatch.Groups.Count >= 2)
            {
                if (float.TryParse(intensityMatch.Groups[1].Value, out float intensity))
                {
                    actionEvent.IntensityValue = System.Math.Clamp(intensity, 0, 10);
                }
            }

            return actionEvent;
        }

        public EngineActionEvent? FallbackRegexParse(string rawInput, string actorId)
        {
            string lowerInput = rawInput.ToLowerInvariant();

            foreach (var kvp in KeywordMap)
            {
                if (lowerInput.Contains(kvp.Key))
                {
                    return new EngineActionEvent(actorId, kvp.Value);
                }
            }

            return null;
        }

        public bool TryExtractCoordinates(string input, out Vector3 coordinates)
        {
            coordinates = Vector3.Zero;
            var match = CoordinateRegex.Match(input);
            if (match.Success && match.Groups.Count >= 4)
            {
                if (float.TryParse(match.Groups[1].Value, out float x)
                    && float.TryParse(match.Groups[2].Value, out float y)
                    && float.TryParse(match.Groups[3].Value, out float z))
                {
                    coordinates = new Vector3(x, y, z);
                    return true;
                }
            }
            return false;
        }

        public List<string> ExtractEntityIds(string input)
        {
            var ids = new List<string>();
            var matches = TargetEntityRegex.Matches(input);
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                    ids.Add(match.Groups[2].Value);
            }
            return ids;
        }
    }
}
