using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoTestWithDocker;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Tests.Builders;

namespace Tests;

[Parallelizable(ParallelScope.Fixtures)]
[TestFixture]
public class TestWeatherApiGetForecasts1
{
    private readonly HttpClient _httpClient = new();
    private readonly Guid _imageConsumerId = Guid.NewGuid();
    private IContainer _containerBuilder;
    private IFutureDockerImage _futureDockerImage;
    private CancellationTokenSource _ctsTests;

    [OneTimeSetUp]
    public async Task Setup()
    {
        var ctsSetup = new CancellationTokenSource(360_000);
        _futureDockerImage = await ImageBuilderWeatherApi.GetOrCreate(_imageConsumerId, ctsSetup.Token);
        _containerBuilder = ContainerBuilders.WeatherApi(_futureDockerImage.FullName);
        await _containerBuilder.StartAsync(ctsSetup.Token);
        ctsSetup.Dispose();

        _httpClient.BaseAddress = new Uri($"https://{_containerBuilder.Hostname}:{_containerBuilder.GetMappedPublicPort(8080)}");
        _ctsTests = new CancellationTokenSource(360_000);
    }

    [OneTimeTearDown]
    public async Task Clean()
    {
        _httpClient.Dispose();
        await _containerBuilder.DisposeAsync();
        await ImageBuilderWeatherApi.DisposeImage(_imageConsumerId);
        _ctsTests.Dispose();
    }
    
    [Test]
    public async Task Test_HealthCheck()
    {
        // Arrange
        var requestUri = $"http://{_containerBuilder.Hostname}:{_containerBuilder.GetMappedPublicPort(8080)}/{ImageBuilderWeatherApi.ApiUrls.Health}";
        var cancellationToken = _ctsTests.Token;

        // Act
        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task Test_WeatherForecast()
    {
        // Arrange
        var requestUri = $"http://{_containerBuilder.Hostname}:{_containerBuilder.GetMappedPublicPort(8080)}/{ImageBuilderWeatherApi.ApiUrls.Weather}";
        var cancellationToken = _ctsTests.Token;
        var jsonSerializerOptions = new JsonSerializerOptions();

        // Act
        var response = await _httpClient.GetFromJsonAsync<WeatherForecasts[]>(requestUri, jsonSerializerOptions, cancellationToken: cancellationToken);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response?.Length, Is.GreaterThan(0));
    }
}