using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoTestWithDocker;
using Tests.Builders;

namespace Tests;

[Parallelizable(ParallelScope.Fixtures)]
public class TestWeatherApiGetForecasts4 : TestWeatherApiBase
{
    [Test]
    public async Task Test_HealthCheck_2()
    {
        // Arrange
        var cancellationToken = CtsTests.Token;

        // Act
        var response = await HttpClient.GetAsync(ImageBuilderWeatherApi.ApiUrls.Health, cancellationToken);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Test_WeatherForecast_2()
    {
        // Arrange
        var requestUri = $"{ImageBuilderWeatherApi.ApiUrls.Weather}";
        var cancellationToken = CtsTests.Token;
        var jsonSerializerOptions = new JsonSerializerOptions();

        // Act
        var response = await HttpClient.GetFromJsonAsync<WeatherForecasts[]>(requestUri, jsonSerializerOptions,
            cancellationToken: cancellationToken);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response?.Length, Is.GreaterThan(0));
    }
}