// Created By Levi Enama
using System;

namespace OpenEngine.Core.Environment
{
    public enum WeatherType
    {
        Clear,
        Cloudy,
        Rain,
        Storm,
        Snow,
        Fog
    }

    public class WeatherSystem
    {
        public WeatherType CurrentWeather { get; private set; }
        public float Temperature { get; private set; }
        public float WindSpeed { get; private set; }
        public float Humidity { get; private set; }
        public float Visibility { get; private set; }
        
        private Random _random;
        private float _weatherChangeChance;

        public WeatherSystem()
        {
            _random = new Random();
            _weatherChangeChance = 0.05f;
            CurrentWeather = WeatherType.Clear;
            Temperature = 20f;
            WindSpeed = 5f;
            Humidity = 50f;
            Visibility = 1.0f;
        }

        public void UpdateWeather()
        {
            // Chance to change weather
            if (_random.NextDouble() < _weatherChangeChance)
            {
                ChangeWeather();
            }

            // Update environmental factors based on current weather
            UpdateEnvironmentalFactors();
        }

        private void ChangeWeather()
        {
            var roll = _random.NextDouble();

            switch (CurrentWeather)
            {
                case WeatherType.Clear:
                    if (roll < 0.3) CurrentWeather = WeatherType.Cloudy;
                    else if (roll < 0.35) CurrentWeather = WeatherType.Rain;
                    else if (roll < 0.37) CurrentWeather = WeatherType.Fog;
                    break;

                case WeatherType.Cloudy:
                    if (roll < 0.4) CurrentWeather = WeatherType.Clear;
                    else if (roll < 0.6) CurrentWeather = WeatherType.Rain;
                    else if (roll < 0.7) CurrentWeather = WeatherType.Fog;
                    else if (roll < 0.75) CurrentWeather = WeatherType.Storm;
                    break;

                case WeatherType.Rain:
                    if (roll < 0.3) CurrentWeather = WeatherType.Cloudy;
                    else if (roll < 0.4) CurrentWeather = WeatherType.Clear;
                    else if (roll < 0.5) CurrentWeather = WeatherType.Storm;
                    break;

                case WeatherType.Storm:
                    if (roll < 0.3) CurrentWeather = WeatherType.Rain;
                    else if (roll < 0.4) CurrentWeather = WeatherType.Cloudy;
                    else if (roll < 0.5) CurrentWeather = WeatherType.Clear;
                    break;

                case WeatherType.Snow:
                    if (roll < 0.3) CurrentWeather = WeatherType.Clear;
                    else if (roll < 0.4) CurrentWeather = WeatherType.Cloudy;
                    break;

                case WeatherType.Fog:
                    if (roll < 0.4) CurrentWeather = WeatherType.Clear;
                    else if (roll < 0.5) CurrentWeather = WeatherType.Cloudy;
                    else if (roll < 0.6) CurrentWeather = WeatherType.Rain;
                    break;
            }
        }

        private void UpdateEnvironmentalFactors()
        {
            // Temperature based on weather
            var targetTemp = CurrentWeather switch
            {
                WeatherType.Clear => 20f + (float)_random.NextDouble() * 10f,
                WeatherType.Cloudy => 15f + (float)_random.NextDouble() * 8f,
                WeatherType.Rain => 12f + (float)_random.NextDouble() * 6f,
                WeatherType.Storm => 8f + (float)_random.NextDouble() * 5f,
                WeatherType.Snow => -5f + (float)_random.NextDouble() * 10f,
                WeatherType.Fog => 10f + (float)_random.NextDouble() * 5f,
                _ => Temperature
            };

            Temperature = Temperature * 0.9f + targetTemp * 0.1f;

            // Wind speed
            var targetWind = CurrentWeather switch
            {
                WeatherType.Storm => 30f + (float)_random.NextDouble() * 20f,
                WeatherType.Rain => 15f + (float)_random.NextDouble() * 10f,
                WeatherType.Snow => 10f + (float)_random.NextDouble() * 10f,
                _ => 5f + (float)_random.NextDouble() * 5f
            };

            WindSpeed = WindSpeed * 0.8f + targetWind * 0.2f;

            // Humidity
            var targetHumidity = CurrentWeather switch
            {
                WeatherType.Rain => 90f + (float)_random.NextDouble() * 10f,
                WeatherType.Storm => 95f + (float)_random.NextDouble() * 5f,
                WeatherType.Fog => 100f,
                WeatherType.Clear => 30f + (float)_random.NextDouble() * 20f,
                WeatherType.Cloudy => 60f + (float)_random.NextDouble() * 20f,
                WeatherType.Snow => 70f + (float)_random.NextDouble() * 15f,
                _ => Humidity
            };

            Humidity = Humidity * 0.9f + targetHumidity * 0.1f;

            // Visibility
            Visibility = CurrentWeather switch
            {
                WeatherType.Clear => 1.0f,
                WeatherType.Cloudy => 0.8f,
                WeatherType.Rain => 0.5f,
                WeatherType.Storm => 0.3f,
                WeatherType.Fog => 0.2f,
                WeatherType.Snow => 0.6f,
                _ => Visibility
            };
        }

        public bool ExtinguishesFire()
        {
            return CurrentWeather == WeatherType.Rain || CurrentWeather == WeatherType.Storm;
        }

        public bool AffectsMovement()
        {
            return CurrentWeather == WeatherType.Storm || CurrentWeather == WeatherType.Snow;
        }

        public float GetMovementModifier()
        {
            return CurrentWeather switch
            {
                WeatherType.Clear => 1.0f,
                WeatherType.Cloudy => 0.95f,
                WeatherType.Rain => 0.8f,
                WeatherType.Storm => 0.5f,
                WeatherType.Snow => 0.6f,
                WeatherType.Fog => 0.9f,
                _ => 1.0f
            };
        }

        public string GetWeatherDescription()
        {
            return CurrentWeather switch
            {
                WeatherType.Clear => "Clear skies with good visibility",
                WeatherType.Cloudy => "Overcast with occasional clouds",
                WeatherType.Rain => "Rain falling, fires may extinguish",
                WeatherType.Storm => "Severe storm with high winds",
                WeatherType.Snow => "Snow falling, cold conditions",
                WeatherType.Fog => "Dense fog limiting visibility",
                _ => "Unknown weather conditions"
            };
        }

        public void SetWeather(WeatherType weather)
        {
            CurrentWeather = weather;
            UpdateEnvironmentalFactors();
        }

        public void ForceWeatherChange()
        {
            ChangeWeather();
            UpdateEnvironmentalFactors();
        }
    }
}
