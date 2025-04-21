using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Tests.Builders;

namespace Tests;

[TestFixture]
public abstract class TestWeatherApiBase
{
    protected readonly HttpClient HttpClient = new(
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                (_, certificate, _, _) => ContainerBuilders.Certificate.Equals(certificate)
        }
    );

    protected CancellationTokenSource CtsTests;

    private readonly Guid _imageConsumerId = Guid.NewGuid();
    private IContainer _containerBuilder;
    private IFutureDockerImage _futureDockerImage;

    [OneTimeSetUp]
    public async Task Setup()
    {
        // Timeout for the test. 
        var ctsSetup = new CancellationTokenSource(360_000);

        _futureDockerImage = await ImageBuilderWeatherApi.GetOrCreate(_imageConsumerId, ctsSetup.Token);
        _containerBuilder = ContainerBuilders.WeatherApi(_futureDockerImage.FullName);
        await _containerBuilder.StartAsync(ctsSetup.Token);

        ctsSetup.Dispose();

        HttpClient.BaseAddress =
            new Uri($"http://{_containerBuilder.Hostname}:{_containerBuilder.GetMappedPublicPort(8080)}");
        CtsTests = new CancellationTokenSource(360_000);
    }

    [OneTimeTearDown]
    public async Task Clean()
    {
        HttpClient.Dispose();
        await _containerBuilder.DisposeAsync();
        await ImageBuilderWeatherApi.DisposeImage(_imageConsumerId);
        CtsTests.Dispose();
    }
}