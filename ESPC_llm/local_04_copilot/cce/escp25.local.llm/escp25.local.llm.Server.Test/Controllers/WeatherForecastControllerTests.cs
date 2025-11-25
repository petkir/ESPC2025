using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using escp25.local.llm.Server.Test.Infrastructure;

namespace escp25.local.llm.Server.Test.Controllers;

public class WeatherForecastControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WeatherForecastControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsOkWithWeatherData()
    {
        // Act
        var response = await _client.GetAsync("/WeatherForecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var weatherForecasts = JsonSerializer.Deserialize<WeatherForecast[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        weatherForecasts.Should().NotBeNull();
        weatherForecasts.Should().HaveCount(5);
        
        foreach (var forecast in weatherForecasts!)
        {
            forecast.Date.Should().NotBe(default);
            forecast.TemperatureC.Should().BeInRange(-20, 55);
            forecast.Summary.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsValidTemperatureRange()
    {
        // Act
        var response = await _client.GetAsync("/WeatherForecast");
        var content = await response.Content.ReadAsStringAsync();
        var weatherForecasts = JsonSerializer.Deserialize<WeatherForecast[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        weatherForecasts.Should().NotBeNull();
        weatherForecasts!.All(w => w.TemperatureC >= -20 && w.TemperatureC <= 55).Should().BeTrue();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsValidSummaries()
    {
        // Arrange
        var validSummaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        // Act
        var response = await _client.GetAsync("/WeatherForecast");
        var content = await response.Content.ReadAsStringAsync();
        var weatherForecasts = JsonSerializer.Deserialize<WeatherForecast[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        weatherForecasts.Should().NotBeNull();
        weatherForecasts!.All(w => validSummaries.Contains(w.Summary)).Should().BeTrue();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFutureDates()
    {
        // Act
        var response = await _client.GetAsync("/WeatherForecast");
        var content = await response.Content.ReadAsStringAsync();
        var weatherForecasts = JsonSerializer.Deserialize<WeatherForecast[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Assert
        weatherForecasts.Should().NotBeNull();
        
        var today = DateOnly.FromDateTime(DateTime.Now);
        weatherForecasts!.All(w => w.Date > today).Should().BeTrue();
    }
}
