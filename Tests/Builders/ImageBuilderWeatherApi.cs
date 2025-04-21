using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Images;

namespace Tests.Builders;

internal static class ImageBuilderWeatherApi
{
    private static IFutureDockerImage? _futureDockerImage;
    private static readonly HashSet<Guid> Consumers = [];
    private static readonly SemaphoreSlim Semaphore = new(1,1);
    
    internal static async Task<IFutureDockerImage> GetOrCreate(Guid consumerId, CancellationToken cancellationToken = default)
    {
        try
        {
            await Semaphore.WaitAsync(cancellationToken);
            
            Console.WriteLine($"Thread.CurrentThread.Name: {Thread.CurrentThread.Name}");
            
            Consumers.Add(consumerId);
            
            if (_futureDockerImage is not null)
                return _futureDockerImage;
            
            _futureDockerImage = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
                .WithDockerfile("AutoTestWithDocker/Dockerfile")
                .WithLabel("name", "WebApi")
                .WithName("web-api")
                .WithDeleteIfExists(true)
                .Build();
            
            await _futureDockerImage.CreateAsync(cancellationToken);

            return _futureDockerImage;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    internal static async Task DisposeImage(Guid consumerId)
    {
        Consumers.Remove(consumerId);
        
        if (Consumers.Count == 0 && _futureDockerImage is not null)
        {
            Semaphore.Dispose();
            await _futureDockerImage.DeleteAsync();
            await _futureDockerImage.DisposeAsync();
        }
    }
    
    public static class ApiUrls
    {
        public const string Health = "healthz";
        public const string Weather = "weatherforecast";
    }
}