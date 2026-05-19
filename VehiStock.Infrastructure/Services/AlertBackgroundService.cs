using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Settings;

namespace VehiStock.Infrastructure.Services;

public class AlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly AlertProcessingSettings _settings;
    private readonly ILogger<AlertBackgroundService> _logger;

    public AlertBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<AlertProcessingSettings> settings,
        ILogger<AlertBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1, _settings.ScanIntervalMinutes)));
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessOnceAsync(stoppingToken);
        }
    }

    private async Task ProcessOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
            await alertService.ProcessAlertsAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed while processing background alerts.");
        }
    }
}
