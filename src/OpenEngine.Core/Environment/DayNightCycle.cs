// Created By Levi Enama
using System;

namespace OpenEngine.Core.Environment
{
    public class DayNightCycle
    {
        public DateTime WorldTime { get; private set; }
        public float DayLengthHours { get; set; }
        public bool IsDay { get; private set; }
        public float DayProgress { get; private set; } // 0.0 to 1.0 through the day
        public float LightLevel { get; private set; } // 0.0 to 1.0

        private DateTime _simulationStartTime;
        private float _timeAcceleration; // How many real seconds = 1 in-game hour

        public DayNightCycle(float dayLengthHours = 24f, float timeAcceleration = 60f)
        {
            DayLengthHours = dayLengthHours;
            _timeAcceleration = timeAcceleration;
            WorldTime = DateTime.UtcNow;
            _simulationStartTime = DateTime.UtcNow;
            IsDay = true;
            DayProgress = 0.5f;
            LightLevel = 1.0f;
            UpdateDayNightStatus();
        }

        public void AdvanceTime(float deltaTime)
        {
            // Convert real deltaTime to in-game hours
            var inGameHours = deltaTime / _timeAcceleration;
            WorldTime = WorldTime.AddHours(inGameHours);

            // Calculate day progress (0 = midnight, 0.5 = noon, 1 = midnight)
            var totalHours = WorldTime.Hour + WorldTime.Minute / 60f + WorldTime.Second / 3600f;
            DayProgress = totalHours / 24f;

            UpdateDayNightStatus();
        }

        private void UpdateDayNightStatus()
        {
            // Day is from 6:00 (0.25) to 18:00 (0.75)
            IsDay = DayProgress >= 0.25f && DayProgress <= 0.75f;

            // Calculate light level based on time of day
            // Peak light at noon (0.5), no light at midnight (0 or 1)
            if (IsDay)
            {
                // Sunrise to noon: 0.25 -> 0.5, light goes 0 -> 1
                // Noon to sunset: 0.5 -> 0.75, light goes 1 -> 0
                if (DayProgress <= 0.5f)
                {
                    LightLevel = (DayProgress - 0.25f) / 0.25f;
                }
                else
                {
                    LightLevel = 1f - (DayProgress - 0.5f) / 0.25f;
                }
            }
            else
            {
                // Night: light is very low (moonlight)
                LightLevel = 0.1f;
            }

            LightLevel = Math.Clamp(LightLevel, 0f, 1f);
        }

        public void SetTimeAcceleration(float acceleration)
        {
            _timeAcceleration = Math.Max(1f, acceleration);
        }

        public void SetWorldTime(DateTime time)
        {
            WorldTime = time;
            UpdateDayNightStatus();
        }

        public string GetTimeDescription()
        {
            var hour = WorldTime.Hour;
            var period = hour >= 12 ? "PM" : "AM";
            var displayHour = hour % 12;
            if (displayHour == 0) displayHour = 12;

            var description = $"{displayHour:00}:{WorldTime.Minute:00} {period}";

            if (IsDay)
            {
                if (DayProgress < 0.3f)
                    description += " (Early Morning)";
                else if (DayProgress < 0.45f)
                    description += " (Morning)";
                else if (DayProgress < 0.55f)
                    description += " (Midday)";
                else if (DayProgress < 0.7f)
                    description += " (Afternoon)";
                else
                    description += " (Late Afternoon)";
            }
            else
            {
                if (DayProgress < 0.1f || DayProgress > 0.9f)
                    description += " (Midnight)";
                else if (DayProgress < 0.2f)
                    description += " (Late Night)";
                else if (DayProgress < 0.3f)
                    description += " (Early Morning)";
                else
                    description += " (Evening)";
            }

            return description;
        }

        public bool IsDawn()
        {
            return DayProgress >= 0.2f && DayProgress <= 0.3f;
        }

        public bool IsDusk()
        {
            return DayProgress >= 0.7f && DayProgress <= 0.8f;
        }

        public bool IsMidnight()
        {
            return DayProgress < 0.05f || DayProgress > 0.95f;
        }

        public bool IsNoon()
        {
            return DayProgress >= 0.45f && DayProgress <= 0.55f;
        }

        public float GetVisibilityModifier()
        {
            return LightLevel;
        }

        public void SkipToDawn()
        {
            var targetHour = 6;
            var newTime = new DateTime(WorldTime.Year, WorldTime.Month, WorldTime.Day, targetHour, 0, 0);
            if (WorldTime.Hour >= targetHour)
            {
                newTime = newTime.AddDays(1);
            }
            SetWorldTime(newTime);
        }

        public void SkipToNoon()
        {
            var targetHour = 12;
            var newTime = new DateTime(WorldTime.Year, WorldTime.Month, WorldTime.Day, targetHour, 0, 0);
            if (WorldTime.Hour >= targetHour)
            {
                newTime = newTime.AddDays(1);
            }
            SetWorldTime(newTime);
        }

        public void SkipToDusk()
        {
            var targetHour = 18;
            var newTime = new DateTime(WorldTime.Year, WorldTime.Month, WorldTime.Day, targetHour, 0, 0);
            if (WorldTime.Hour >= targetHour)
            {
                newTime = newTime.AddDays(1);
            }
            SetWorldTime(newTime);
        }

        public void SkipToMidnight()
        {
            var targetHour = 0;
            var newTime = new DateTime(WorldTime.Year, WorldTime.Month, WorldTime.Day, targetHour, 0, 0);
            if (WorldTime.Hour >= targetHour)
            {
                newTime = newTime.AddDays(1);
            }
            SetWorldTime(newTime);
        }
    }
}
