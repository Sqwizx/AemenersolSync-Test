using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AemenersolSync.Api.Services;

public class SyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _interval;
    private readonly ILogger<SyncBackgroundService> _logger;

    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<SyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        var seconds = configuration.GetValue<int?>("SyncIntervalSeconds") ?? 60;
        _interval = TimeSpan.FromSeconds(seconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
                var tokenStore = scope.ServiceProvider.GetRequiredService<ISyncTokenStore>();

                if (tokenStore.GetToken() is null)
                {
                    _logger.LogInformation("Background sync tick skipped: no active session token.");
                    continue;
                }

                await syncService.RunSyncAsync();
            }
            catch (AemenersolUnauthorizedException)
            {
                _logger.LogWarning("Background sync tick: upstream session expired.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background sync tick failed unexpectedly.");
            }
        }
    }
}
