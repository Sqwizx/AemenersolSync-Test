using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AemenersolSync.Api.Tests.Services;

public class SyncBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteAsync_CallsRunSyncAsync_OnEachTick()
    {
        var syncService = new Mock<ISyncService>();
        syncService.Setup(s => s.RunSyncAsync(false)).ReturnsAsync(new SyncResultDto());

        var tokenStore = new Mock<ISyncTokenStore>();
        tokenStore.Setup(t => t.GetToken()).Returns("some-token");

        var services = new ServiceCollection();
        services.AddScoped(_ => syncService.Object);
        services.AddScoped(_ => tokenStore.Object);
        var provider = services.BuildServiceProvider();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SyncIntervalSeconds"] = "1" })
            .Build();

        var backgroundService = new SyncBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            config,
            NullLogger<SyncBackgroundService>.Instance);

        using var cts = new CancellationTokenSource();
        await backgroundService.StartAsync(cts.Token);
        await Task.Delay(1500);
        cts.Cancel();
        await backgroundService.StopAsync(CancellationToken.None);

        syncService.Verify(s => s.RunSyncAsync(false), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_UnauthorizedExceptionFromRunSync_DoesNotCrashLoop()
    {
        var syncService = new Mock<ISyncService>();
        syncService.SetupSequence(s => s.RunSyncAsync(false))
            .ThrowsAsync(new AemenersolUnauthorizedException())
            .ReturnsAsync(new SyncResultDto());

        var tokenStore = new Mock<ISyncTokenStore>();
        tokenStore.Setup(t => t.GetToken()).Returns("some-token");

        var services = new ServiceCollection();
        services.AddScoped(_ => syncService.Object);
        services.AddScoped(_ => tokenStore.Object);
        var provider = services.BuildServiceProvider();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SyncIntervalSeconds"] = "1" })
            .Build();

        var backgroundService = new SyncBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            config,
            NullLogger<SyncBackgroundService>.Instance);

        using var cts = new CancellationTokenSource();
        await backgroundService.StartAsync(cts.Token);
        await Task.Delay(2500);
        cts.Cancel();
        await backgroundService.StopAsync(CancellationToken.None);

        syncService.Verify(s => s.RunSyncAsync(false), Times.AtLeast(2));
    }
}
