using System.Security.Cryptography.X509Certificates;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Tests.Builders;

public static class ContainerBuilders
{
    private const string CertificatePath = "certificate.crt";
    private const string Password = "password";
    internal static readonly X509Certificate Certificate = new X509Certificate2(CertificatePath, Password);

    public static IContainer WeatherApi(string imageName)
    {
        var healthCheck = Wait
            .ForUnixContainer()
            .UntilHttpRequestIsSucceeded(r => r
                .ForPath(ImageBuilderWeatherApi.ApiUrls.Health)
                .ForPort(8080)
                .UsingTls(false)
                .WithMethod(HttpMethod.Get)
            );

        return new ContainerBuilder()
            .WithImage(imageName)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "ContainerTests")
            .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Path", CertificatePath)
            .WithEnvironment("ASPNETCORE_Kestrel__Certificates__Default__Password", Password)
            .WithEnvironment("EnvSettings:EnvMessage", "A test message from an environment variable.")
            .WithPortBinding(8080, true)
            .WithPortBinding(8081, true)
            .WithWaitStrategy(healthCheck)
            .WithCleanUp(true)
            .Build();
    }
}