using AemenersolSync.Api.Data;
using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Hubs;
using AemenersolSync.Api.Models;
using AemenersolSync.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AemenersolSync.Api.Tests.Services;

// Covers the Platform/Well upsert behavior that used to live in the now-removed
// PlatformWellRepository. That logic moved into SyncService as a private method
// (SyncService now uses AppDbContext directly instead of a repository
// abstraction), so these scenarios are exercised end-to-end through
// SyncService.RunSyncAsync against a seeded InMemory AppDbContext, with
// IAemenersolClient mocked to return each test's PlatformDto payload. Assertions
// are made against the resulting AppDbContext state and/or the returned
// SyncResultDto counts, which is equivalent rigor to the original repository
// tests asserting on UpsertResult and AppDbContext state directly.
public class SyncServiceUpsertTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static SyncService CreateService(AppDbContext context, List<PlatformDto> platforms)
    {
        var tokenStore = new Mock<ISyncTokenStore>();
        tokenStore.Setup(t => t.GetToken()).Returns("token");

        var client = new Mock<IAemenersolClient>();
        client.Setup(c => c.GetPlatformWellActualAsync("token")).ReturnsAsync(platforms);

        var hubContext = new Mock<IHubContext<SyncHub>>();
        var clientProxy = new Mock<IClientProxy>();
        hubContext.Setup(h => h.Clients.All).Returns(clientProxy.Object);

        return new SyncService(client.Object, context, tokenStore.Object, hubContext.Object);
    }

    [Fact]
    public async Task RunSyncAsync_NewPlatform_Inserts()
    {
        using var context = CreateInMemoryContext();
        var dto = new PlatformDto { Id = 1, UniqueName = "P1", Latitude = 1.1, Longitude = 2.2, Well = new() };
        var service = CreateService(context, new List<PlatformDto> { dto });

        var result = await service.RunSyncAsync();

        Assert.Equal(1, result.PlatformsInserted);
        Assert.Equal(0, result.PlatformsUpdated);
        var saved = await context.Platforms.FindAsync(1);
        Assert.NotNull(saved);
        Assert.Equal("P1", saved!.UniqueName);
    }

    [Fact]
    public async Task RunSyncAsync_ExistingPlatform_Updates()
    {
        using var context = CreateInMemoryContext();
        context.Platforms.Add(new Platform { Id = 1, UniqueName = "OldName", Latitude = 0, Longitude = 0 });
        await context.SaveChangesAsync();
        var dto = new PlatformDto { Id = 1, UniqueName = "NewName", Latitude = 9.9, Longitude = 8.8, Well = new() };
        var service = CreateService(context, new List<PlatformDto> { dto });

        var result = await service.RunSyncAsync();

        Assert.Equal(0, result.PlatformsInserted);
        Assert.Equal(1, result.PlatformsUpdated);
        var saved = await context.Platforms.FindAsync(1);
        Assert.Equal("NewName", saved!.UniqueName);
        Assert.Equal(9.9, saved.Latitude);
    }

    [Fact]
    public async Task RunSyncAsync_MissingFieldOnUpdate_PreservesExistingValue()
    {
        using var context = CreateInMemoryContext();
        context.Platforms.Add(new Platform { Id = 1, UniqueName = "Keep", Latitude = 5.5, Longitude = 6.6 });
        await context.SaveChangesAsync();

        // Latitude is null -> simulates a field missing from the API response.
        var dto = new PlatformDto { Id = 1, UniqueName = "Keep", Latitude = null, Longitude = 7.7, Well = new() };
        var service = CreateService(context, new List<PlatformDto> { dto });

        await service.RunSyncAsync();

        var saved = await context.Platforms.FindAsync(1);
        Assert.Equal(5.5, saved!.Latitude); // preserved, not overwritten with null
        Assert.Equal(7.7, saved.Longitude); // non-null value does overwrite
    }

    [Fact]
    public async Task RunSyncAsync_MixedBatch_InsertsAndUpdatesSeparately()
    {
        using var context = CreateInMemoryContext();
        context.Platforms.Add(new Platform { Id = 1, UniqueName = "Existing" });
        await context.SaveChangesAsync();

        var dtos = new List<PlatformDto>
        {
            new() { Id = 1, UniqueName = "Updated", Well = new() },
            new() { Id = 2, UniqueName = "New", Well = new() }
        };
        var service = CreateService(context, dtos);

        var result = await service.RunSyncAsync();

        Assert.Equal(1, result.PlatformsInserted);
        Assert.Equal(1, result.PlatformsUpdated);
    }

    [Fact]
    public async Task RunSyncAsync_NestedWells_UpsertIndependentlyOfParent()
    {
        using var context = CreateInMemoryContext();
        var dto = new PlatformDto
        {
            Id = 1,
            UniqueName = "P1",
            Well = new List<WellDto>
            {
                new() { Id = 10, PlatformId = 1, UniqueName = "W10" },
                new() { Id = 11, PlatformId = 1, UniqueName = "W11" }
            }
        };
        var service = CreateService(context, new List<PlatformDto> { dto });

        var result = await service.RunSyncAsync();

        Assert.Equal(2, result.WellsInserted);
        Assert.Equal(2, await context.Wells.CountAsync());
    }

    [Fact]
    public async Task RunSyncAsync_ExistingWell_ReSyncsPlatformId()
    {
        using var context = CreateInMemoryContext();
        context.Platforms.Add(new Platform { Id = 1, UniqueName = "P1" });
        context.Platforms.Add(new Platform { Id = 2, UniqueName = "P2" });
        context.Wells.Add(new Well { Id = 10, PlatformId = 1, UniqueName = "W10" });
        await context.SaveChangesAsync();

        var dto = new PlatformDto
        {
            Id = 2,
            UniqueName = "P2",
            Well = new List<WellDto>
            {
                new() { Id = 10, PlatformId = 2, UniqueName = "W10" }
            }
        };
        var service = CreateService(context, new List<PlatformDto> { dto });

        await service.RunSyncAsync();

        var saved = await context.Wells.FindAsync(10);
        Assert.Equal(2, saved!.PlatformId);
    }

    [Fact]
    public async Task RunSyncAsync_RecordMissingId_IsSkippedWithoutThrowing()
    {
        using var context = CreateInMemoryContext();
        var dto = new PlatformDto
        {
            Id = 1,
            UniqueName = "P1",
            Well = new List<WellDto>
            {
                new() { Id = 0, PlatformId = 0, UniqueName = "MissingId" } // Id defaults to 0 = treated as missing
            }
        };
        var service = CreateService(context, new List<PlatformDto> { dto });

        var result = await service.RunSyncAsync();

        Assert.Equal(1, result.PlatformsInserted);
        Assert.Equal(0, result.WellsInserted); // skipped, not inserted as id=0
        Assert.Empty(await context.Wells.ToListAsync());
    }
}
