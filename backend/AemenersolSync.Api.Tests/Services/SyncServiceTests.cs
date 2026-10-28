using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Hubs;
using AemenersolSync.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace AemenersolSync.Api.Tests.Services;

public class SyncServiceTests
{
    [Fact]
    public async Task RunSyncAsync_NoStoredToken_LogsInFirst()
    {
        var tokenStore = new Mock<ISyncTokenStore>();
        tokenStore.Setup(t => t.GetToken()).Returns((string?)null);

        var client = new Mock<IAemenersolClient>();
        client.Setup(c => c.LoginAsync()).ReturnsAsync("new-token");
        client.Setup(c => c.GetPlatformWellActualAsync("new-token")).ReturnsAsync(new List<PlatformDto>());

        var repo = new Mock<IPlatformWellRepository>();
        repo.Setup(r => r.UpsertPlatformsAsync(It.IsAny<List<PlatformDto>>()))
            .ReturnsAsync(new UpsertResult(0, 0, 0, 0));

        var hubContext = new Mock<IHubContext<SyncHub>>();
        var clientProxy = new Mock<IClientProxy>();
        hubContext.Setup(h => h.Clients.All).Returns(clientProxy.Object);

        var service = new SyncService(client.Object, repo.Object, tokenStore.Object, hubContext.Object);

        await service.RunSyncAsync();

        client.Verify(c => c.LoginAsync(), Times.Once);
        tokenStore.Verify(t => t.SetToken("new-token"), Times.Once);
    }

    [Fact]
    public async Task RunSyncAsync_StoredToken_SkipsLogin()
    {
        var tokenStore = new Mock<ISyncTokenStore>();
        tokenStore.Setup(t => t.GetToken()).Returns("existing-token");

        var client = new Mock<IAemenersolClient>();
        client.Setup(c => c.GetPlatformWellActualAsync("existing-token")).ReturnsAsync(new List<PlatformDto>());

        var repo = new Mock<IPlatformWellRepository>();
        repo.Setup(r => r.UpsertPlatformsAsync(It.IsAny<List<PlatformDto>>()))
            .ReturnsAsync(new UpsertResult(0, 0, 0, 0));

        var hubContext = new Mock<IHubContext<SyncHub>>();
        hubContext.Setup(h => h.Clients.All).Returns(Mock.Of<IClientProxy>());

        var service = new SyncService(client.Object, repo.Object, tokenStore.Object, hubContext.Object);

        await service.RunSyncAsync();

        client.Verify(c => c.LoginAsync(), Times.Never);
    }

    [Fact]
    public async Task RunSyncAsync_UpstreamUnauthorized_ClearsTokenAndRethrows()
    {
        var tokenStore = new Mock<ISyncTokenStore>();
        tokenStore.Setup(t => t.GetToken()).Returns("expired-token");

        var client = new Mock<IAemenersolClient>();
        client.Setup(c => c.GetPlatformWellActualAsync("expired-token"))
            .ThrowsAsync(new AemenersolUnauthorizedException());

        var repo = new Mock<IPlatformWellRepository>();
        var hubContext = new Mock<IHubContext<SyncHub>>();
        hubContext.Setup(h => h.Clients.All).Returns(Mock.Of<IClientProxy>());

        var service = new SyncService(client.Object, repo.Object, tokenStore.Object, hubContext.Object);

        await Assert.ThrowsAsync<AemenersolUnauthorizedException>(() => service.RunSyncAsync());

        tokenStore.Verify(t => t.ClearToken(), Times.Once);
    }

    [Fact]
    public async Task RunSyncAsync_UseDummyEndpoint_True_CallsDummyEndpoint()
    {
        var tokenStore = new Mock<ISyncTokenStore>();
        tokenStore.Setup(t => t.GetToken()).Returns("token");

        var client = new Mock<IAemenersolClient>();
        client.Setup(c => c.GetPlatformWellDummyAsync("token")).ReturnsAsync(new List<PlatformDto>());

        var repo = new Mock<IPlatformWellRepository>();
        repo.Setup(r => r.UpsertPlatformsAsync(It.IsAny<List<PlatformDto>>()))
            .ReturnsAsync(new UpsertResult(0, 0, 0, 0));

        var hubContext = new Mock<IHubContext<SyncHub>>();
        hubContext.Setup(h => h.Clients.All).Returns(Mock.Of<IClientProxy>());

        var service = new SyncService(client.Object, repo.Object, tokenStore.Object, hubContext.Object);

        await service.RunSyncAsync(useDummyEndpoint: true);

        client.Verify(c => c.GetPlatformWellDummyAsync("token"), Times.Once);
        client.Verify(c => c.GetPlatformWellActualAsync(It.IsAny<string>()), Times.Never);
    }
}
