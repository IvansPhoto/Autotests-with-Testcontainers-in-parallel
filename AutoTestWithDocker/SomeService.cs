using Microsoft.Extensions.Options;

namespace AutoTestWithDocker;

internal sealed class SomeService
{
    private readonly ILogger<SomeService> _logger;
    private readonly IOptionsMonitor<EnvSettings> _optionsMonitor;

    public SomeService(ILogger<SomeService> logger, IOptionsMonitor<EnvSettings> optionsMonitor)
    {
        _logger = logger;
        _optionsMonitor = optionsMonitor;
    }

    internal ApiResponse GetDataWithAsyncVoid(int start, int end, CancellationToken cancellationToken = default)
    {
        try
        {
            DoAsyncVoid(cancellationToken);

            return new ApiResponse(
                EnvMessage: _optionsMonitor.CurrentValue.EnvMessage.Substring(start, end),
                AppSettingMessage: _optionsMonitor.CurrentValue.AppSettingMessage.Substring(start, end)
            );
        }
        catch (SpecialException e)
        {
            const string message = "SpecialException from async void method has never been caught";
            _logger.LogError(exception: e, message: message);
            return new ApiResponse(message, message);
        }
    }
    
    internal ApiResponse GetDataWithAsyncTask(int start, int end, CancellationToken cancellationToken = default)
    {
        try
        {
            Task.Run(() => DoAsyncTask(cancellationToken), cancellationToken);
            
            return new ApiResponse(
                EnvMessage: _optionsMonitor.CurrentValue.EnvMessage.Substring(start, end),
                AppSettingMessage: _optionsMonitor.CurrentValue.AppSettingMessage.Substring(start, end)
            );
        }
        catch (SpecialException e)
        {
            const string message = "SpecialException from Task.Run has never been caught";
            _logger.LogError(exception: e, message: message);
            return new ApiResponse(message, message);
        }
    }
    
    internal ApiResponse GetDataWithAsyncTaskTryCatchInRun(int start, int end, CancellationToken cancellationToken = default)
    {
        Task.Run(async () =>
        {
            try
            {
                await DoAsyncTask(cancellationToken);
            }
            catch (SpecialException e)
            {
                _logger.LogError(exception: e, message: "SpecialException in Task.Run has been caught!");
            }
        }, cancellationToken);

        return new ApiResponse(
            EnvMessage: _optionsMonitor.CurrentValue.EnvMessage.Substring(start, end),
            AppSettingMessage: _optionsMonitor.CurrentValue.AppSettingMessage.Substring(start, end)
        );
    }
    
    private async void DoAsyncVoid(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);

        if (Random.Shared.Next(10) > 5)
        {
            _logger.LogWarning("Exception has been thrown in DoAsyncVoid");
            throw new SpecialException("Exception in async void");
        }
    }
    
    private async Task DoAsyncTask(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);

        if (Random.Shared.Next(10) > 5)
        {
            _logger.LogWarning("Exception has been thrown in DoAsyncTask");
            throw new SpecialException("Exception in async void");
        }
    }
}