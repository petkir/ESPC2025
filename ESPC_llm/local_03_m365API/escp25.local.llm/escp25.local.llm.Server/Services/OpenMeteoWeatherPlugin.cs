using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace escp25.local.llm.Server.Services;

/// <summary>
/// Plugin for Open-Meteo Weather API - provides weather forecasts and historical data
/// </summary>
public class OpenMeteoWeatherPlugin
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private const string BaseUrl = "https://api.open-meteo.com/v1";

    public OpenMeteoWeatherPlugin(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [KernelFunction("GetCurrentWeather")]
    [Description("Get current weather conditions for a location")]
    [return: Description("Current weather information including temperature, humidity, wind speed, etc.")]
    public async Task<string> GetCurrentWeatherAsync(
        [Description("Latitude of the location")] double latitude,
        [Description("Longitude of the location")] double longitude,
        [Description("Temperature unit (celsius or fahrenheit), default: celsius")] string temperatureUnit = "celsius")
    {
        try
        {
            var url = $"{BaseUrl}/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover,pressure_msl,surface_pressure,wind_speed_10m,wind_direction_10m,wind_gusts_10m&temperature_unit={temperatureUnit}&wind_speed_unit=kmh&precipitation_unit=mm";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current weather from Open-Meteo");
            return $"Error getting current weather: {ex.Message}";
        }
    }

    [KernelFunction("GetWeatherForecast")]
    [Description("Get weather forecast for the next 7 days")]
    [return: Description("7-day weather forecast with daily highs, lows, and conditions")]
    public async Task<string> GetWeatherForecastAsync(
        [Description("Latitude of the location")] double latitude,
        [Description("Longitude of the location")] double longitude,
        [Description("Number of forecast days (1-16), default: 7")] int days = 7,
        [Description("Temperature unit (celsius or fahrenheit), default: celsius")] string temperatureUnit = "celsius")
    {
        try
        {
            days = Math.Max(1, Math.Min(days, 16)); // Limit to 1-16 days
            var url = $"{BaseUrl}/forecast?latitude={latitude}&longitude={longitude}&daily=weather_code,temperature_2m_max,temperature_2m_min,apparent_temperature_max,apparent_temperature_min,sunrise,sunset,daylight_duration,sunshine_duration,uv_index_max,precipitation_sum,rain_sum,showers_sum,snowfall_sum,precipitation_hours,precipitation_probability_max,wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant&temperature_unit={temperatureUnit}&wind_speed_unit=kmh&precipitation_unit=mm&forecast_days={days}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weather forecast from Open-Meteo");
            return $"Error getting weather forecast: {ex.Message}";
        }
    }

    [KernelFunction("GetHourlyWeather")]
    [Description("Get hourly weather forecast for the next 24-48 hours")]
    [return: Description("Hourly weather data including temperature, precipitation, wind, etc.")]
    public async Task<string> GetHourlyWeatherAsync(
        [Description("Latitude of the location")] double latitude,
        [Description("Longitude of the location")] double longitude,
        [Description("Number of forecast days for hourly data (1-16), default: 2")] int days = 2,
        [Description("Temperature unit (celsius or fahrenheit), default: celsius")] string temperatureUnit = "celsius")
    {
        try
        {
            days = Math.Max(1, Math.Min(days, 16)); // Limit to 1-16 days
            var url = $"{BaseUrl}/forecast?latitude={latitude}&longitude={longitude}&hourly=temperature_2m,relative_humidity_2m,dew_point_2m,apparent_temperature,precipitation_probability,precipitation,rain,showers,snowfall,snow_depth,weather_code,pressure_msl,surface_pressure,cloud_cover,cloud_cover_low,cloud_cover_mid,cloud_cover_high,visibility,evapotranspiration,et0_fao_evapotranspiration,vapour_pressure_deficit,wind_speed_10m,wind_speed_80m,wind_speed_120m,wind_speed_180m,wind_direction_10m,wind_direction_80m,wind_direction_120m,wind_direction_180m,wind_gusts_10m,temperature_80m,temperature_120m,temperature_180m,soil_temperature_0cm,soil_temperature_6cm,soil_temperature_18cm,soil_temperature_54cm,soil_moisture_0_1cm,soil_moisture_1_3cm,soil_moisture_3_9cm,soil_moisture_9_27cm,soil_moisture_27_81cm&temperature_unit={temperatureUnit}&wind_speed_unit=kmh&precipitation_unit=mm&forecast_days={days}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hourly weather from Open-Meteo");
            return $"Error getting hourly weather: {ex.Message}";
        }
    }

    [KernelFunction("GetHistoricalWeather")]
    [Description("Get historical weather data for a specific date range")]
    [return: Description("Historical weather data for the specified period")]
    public async Task<string> GetHistoricalWeatherAsync(
        [Description("Latitude of the location")] double latitude,
        [Description("Longitude of the location")] double longitude,
        [Description("Start date in YYYY-MM-DD format")] string startDate,
        [Description("End date in YYYY-MM-DD format")] string endDate,
        [Description("Temperature unit (celsius or fahrenheit), default: celsius")] string temperatureUnit = "celsius")
    {
        try
        {
            var url = $"https://archive-api.open-meteo.com/v1/archive?latitude={latitude}&longitude={longitude}&start_date={startDate}&end_date={endDate}&daily=weather_code,temperature_2m_max,temperature_2m_min,temperature_2m_mean,apparent_temperature_max,apparent_temperature_min,apparent_temperature_mean,sunrise,sunset,daylight_duration,sunshine_duration,precipitation_sum,rain_sum,snowfall_sum,precipitation_hours,wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant,shortwave_radiation_sum&temperature_unit={temperatureUnit}&wind_speed_unit=kmh&precipitation_unit=mm";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical weather from Open-Meteo");
            return $"Error getting historical weather: {ex.Message}";
        }
    }

    [KernelFunction("GetWeatherForCity")]
    [Description("Get current weather and forecast for a city using geocoding")]
    [return: Description("Weather information for the specified city")]
    public async Task<string> GetWeatherForCityAsync(
        [Description("City name (e.g., 'London', 'New York', 'Tokyo')")] string cityName,
        [Description("Country code (optional, e.g., 'US', 'GB', 'JP')")] string? countryCode = null,
        [Description("Temperature unit (celsius or fahrenheit), default: celsius")] string temperatureUnit = "celsius")
    {
        try
        {
            // First, get coordinates for the city using Open-Meteo's geocoding API
            var geocodeUrl = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(cityName)}";
            if (!string.IsNullOrEmpty(countryCode))
            {
                geocodeUrl += $"&country_code={countryCode}";
            }
            geocodeUrl += "&count=1&language=en&format=json";

            var geocodeResponse = await _httpClient.GetAsync(geocodeUrl);
            if (!geocodeResponse.IsSuccessStatusCode)
            {
                return $"Error finding location for {cityName}: {geocodeResponse.StatusCode}";
            }

            var geocodeContent = await geocodeResponse.Content.ReadAsStringAsync();
            using var geocodeDoc = JsonDocument.Parse(geocodeContent);
            
            if (!geocodeDoc.RootElement.TryGetProperty("results", out var results) || 
                results.GetArrayLength() == 0)
            {
                return $"City '{cityName}' not found";
            }

            var firstResult = results[0];
            var latitude = firstResult.GetProperty("latitude").GetDouble();
            var longitude = firstResult.GetProperty("longitude").GetDouble();
            var foundCityName = firstResult.GetProperty("name").GetString();
            var foundCountry = firstResult.GetProperty("country").GetString();

            // Now get the weather for these coordinates
            var weatherUrl = $"{BaseUrl}/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,cloud_cover,pressure_msl,wind_speed_10m,wind_direction_10m&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_speed_10m_max&temperature_unit={temperatureUnit}&wind_speed_unit=kmh&precipitation_unit=mm&forecast_days=3";

            var weatherResponse = await _httpClient.GetAsync(weatherUrl);
            if (weatherResponse.IsSuccessStatusCode)
            {
                var weatherContent = await weatherResponse.Content.ReadAsStringAsync();
                
                // Parse and enhance the response with city information
                using var weatherDoc = JsonDocument.Parse(weatherContent);
                var enhancedResponse = new
                {
                    city = new
                    {
                        name = foundCityName,
                        country = foundCountry,
                        latitude = latitude,
                        longitude = longitude
                    },
                    weather = weatherDoc.RootElement
                };

                return JsonSerializer.Serialize(enhancedResponse, new JsonSerializerOptions { WriteIndented = true });
            }
            return $"Error getting weather data: {weatherResponse.StatusCode}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weather for city {CityName}", cityName);
            return $"Error getting weather for {cityName}: {ex.Message}";
        }
    }

    [KernelFunction("GetMarineWeather")]
    [Description("Get marine weather forecast for coastal and ocean locations")]
    [return: Description("Marine weather data including wave height, swell, and ocean conditions")]
    public async Task<string> GetMarineWeatherAsync(
        [Description("Latitude of the marine location")] double latitude,
        [Description("Longitude of the marine location")] double longitude,
        [Description("Number of forecast days (1-7), default: 3")] int days = 3,
        [Description("Temperature unit (celsius or fahrenheit), default: celsius")] string temperatureUnit = "celsius")
    {
        try
        {
            days = Math.Max(1, Math.Min(days, 7)); // Limit to 1-7 days for marine forecast
            var url = $"https://marine-api.open-meteo.com/v1/marine?latitude={latitude}&longitude={longitude}&daily=wave_height_max,wave_direction_dominant,wave_period_max,wind_wave_height_max,wind_wave_direction_dominant,wind_wave_period_max,swell_wave_height_max,swell_wave_direction_dominant,swell_wave_period_max&temperature_unit={temperatureUnit}&wind_speed_unit=kmh&forecast_days={days}";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting marine weather from Open-Meteo");
            return $"Error getting marine weather: {ex.Message}";
        }
    }
}
