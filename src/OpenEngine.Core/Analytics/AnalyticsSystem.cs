// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OpenEngine.Core.Analytics
{
    public enum AnalyticsEventType
    {
        SessionStart,
        SessionEnd,
        LevelStart,
        LevelComplete,
        LevelFailed,
        Purchase,
        AdImpression,
        AdClick,
        CustomEvent,
        Error,
        Crash
    }

    public class AnalyticsEvent
    {
        public string EventName { get; set; }
        public AnalyticsEventType EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }

        public AnalyticsEvent(string eventName, AnalyticsEventType eventType)
        {
            EventName = eventName;
            EventType = eventType;
            Timestamp = DateTime.UtcNow;
            Parameters = new Dictionary<string, object>();
            UserId = "";
            SessionId = "";
        }
    }

    public class AnalyticsSystem
    {
        private List<AnalyticsEvent> _events;
        private string _userId;
        private string _sessionId;
        private bool _enabled;
        private string _dataDirectory;
        private JsonSerializerOptions _serializerOptions;

        public bool Enabled => _enabled;
        public string UserId => _userId;
        public string SessionId => _sessionId;

        public AnalyticsSystem()
        {
            _events = new List<AnalyticsEvent>();
            _userId = GenerateUserId();
            _sessionId = GenerateSessionId();
            _enabled = true;
            _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenEngine", "Analytics");
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }

        public void SetUserId(string userId)
        {
            _userId = userId;
        }

        public void StartSession()
        {
            _sessionId = GenerateSessionId();
            TrackEvent("SessionStart", AnalyticsEventType.SessionStart);
        }

        public void EndSession()
        {
            TrackEvent("SessionEnd", AnalyticsEventType.SessionEnd);
            FlushEvents();
        }

        public void TrackEvent(string eventName, AnalyticsEventType eventType, Dictionary<string, object> parameters = null)
        {
            if (!_enabled)
                return;

            var analyticsEvent = new AnalyticsEvent(eventName, eventType)
            {
                UserId = _userId,
                SessionId = _sessionId
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    analyticsEvent.Parameters[param.Key] = param.Value;
                }
            }

            _events.Add(analyticsEvent);
        }

        public void TrackCustomEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            TrackEvent(eventName, AnalyticsEventType.CustomEvent, parameters);
        }

        public void TrackLevelStart(string levelName)
        {
            TrackEvent("LevelStart", AnalyticsEventType.LevelStart, new Dictionary<string, object>
            {
                { "level_name", levelName }
            });
        }

        public void TrackLevelComplete(string levelName, float duration, int score)
        {
            TrackEvent("LevelComplete", AnalyticsEventType.LevelComplete, new Dictionary<string, object>
            {
                { "level_name", levelName },
                { "duration", duration },
                { "score", score }
            });
        }

        public void TrackLevelFailed(string levelName, string reason)
        {
            TrackEvent("LevelFailed", AnalyticsEventType.LevelFailed, new Dictionary<string, object>
            {
                { "level_name", levelName },
                { "reason", reason }
            });
        }

        public void TrackPurchase(string itemId, float price, string currency)
        {
            TrackEvent("Purchase", AnalyticsEventType.Purchase, new Dictionary<string, object>
            {
                { "item_id", itemId },
                { "price", price },
                { "currency", currency }
            });
        }

        public void TrackError(string errorMessage, string stackTrace)
        {
            TrackEvent("Error", AnalyticsEventType.Error, new Dictionary<string, object>
            {
                { "error_message", errorMessage },
                { "stack_trace", stackTrace }
            });
        }

        public void FlushEvents()
        {
            if (_events.Count == 0)
                return;

            try
            {
                var fileName = $"analytics_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
                var filePath = Path.Combine(_dataDirectory, fileName);
                var jsonData = JsonSerializer.Serialize(_events, _serializerOptions);
                File.WriteAllText(filePath, jsonData);

                _events.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to flush analytics events: {ex.Message}");
            }
        }

        public List<AnalyticsEvent> GetEvents()
        {
            return new List<AnalyticsEvent>(_events);
        }

        public Dictionary<string, int> GetEventCounts()
        {
            var counts = new Dictionary<string, int>();
            foreach (var evt in _events)
            {
                if (counts.ContainsKey(evt.EventName))
                {
                    counts[evt.EventName]++;
                }
                else
                {
                    counts[evt.EventName] = 1;
                }
            }
            return counts;
        }

        private string GenerateUserId()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateSessionId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
