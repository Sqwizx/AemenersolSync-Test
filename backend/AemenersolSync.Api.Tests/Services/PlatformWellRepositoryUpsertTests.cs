using AemenersolSync.Api.Data;
using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Models;
using AemenersolSync.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AemenersolSync.Api.Tests.Services;

public class PlatformWellRepositoryUpsertTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task UpsertPlatformsAsync_NewPlatform_Inserts()
    {
        using var context = CreateInMemoryContext();
        var repo = new PlatformWellRepository(context);

        var dto = new PlatformDto { Id = 1, UniqueName = "P1", Latitude = 1.1, Longitude = 2.2, Well = new() };

        var result = await repo.UpsertPlatformsAsync(new List<PlatformDto> { dto });

        Assert.Equal(1, result.PlatformsInserted);
        Assert.Equal(0, result.PlatformsUpdated);
        var saved = await context.Platforms.FindAsync(1);
        Assert.NotNull(saved);
        Assert.Equal("P1", saved!.UniqueName);
    }

    [Fact]
    public async Task UpsertPlatformsAsync_ExistingPlatform_Updates()
    {
        using var context = CreateInMemoryContext();
        context.Platforms.Add(new Platform { Id = 1, UniqueName = "OldName", Latitude = 0, Longitude = 0 });
        await context.SaveChangesAsync();
        var repo = new PlatformWellRepository(context);

        var dto = new PlatformDto { Id = 1, UniqueName = "NewName", Latitude = 9.9, Longitude = 8.8, Well = new() };

        var result = await repo.UpsertPlatformsAsync(new List<PlatformDto> { dto });

        Assert.Equal(0, result.PlatformsInserted);
        Assert.Equal(1, result.PlatformsUpdated);
        var saved = await context.Platforms.FindAsync(1);
        Assert.Equal("NewName", saved!.UniqueName);
        Assert.Equal(9.9, saved.Latitude);
    }

    [Fact]
    public async Task UpsertPlatformsAsync_MissingFieldOnUpdate_PreservesExistingValue()
    {
        using var context = CreateInMemoryContext();
        context.Platforms.Add(new Platform { Id = 1, UniqueName = "Keep", Latitude = 5.5, Longitude = 6.6 });
        await context.SaveChangesAsync();
        var repo = new PlatformWellRepository(context);

        // Latitude is null -> simulates a field missing from the API response.
        var dto = new PlatformDto { Id = 1, UniqueName = "Keep", Latitude = null, Longitude = 7.7, Well = new() };

        await repo.UpsertPlatformsAsync(new List<PlatformDto> { dto });

        var saved = await context.Platforms.FindAsync(1);
        Assert.Equal(5.5, saved!.Latitude); // preserved, not overwritten with null
        Assert.Equal(7.7, saved.Longitude); // non-null value does overwrite
    }

    [Fact]
    public async Task UpsertPlatformsAsync_MixedBatch_InsertsAndUpdatesSeparately()
    {
        using var context = CreateInMemoryContext();
        context.Platforms.Add(new Platform { Id = 1, UniqueName = "Existing" });
        await context.SaveChangesAsync();
        var repo = new PlatformWellRepository(context);

        var dtos = new List<PlatformDto>
        {
            new() { Id = 1, UniqueName = "Updated", Well = new() },
            new() { Id = 2, UniqueName = "New", Well = new() }
        };

        var result = await repo.UpsertPlatformsAsync(dtos);

        Assert.Equal(1, result.PlatformsInserted);
        Assert.Equal(1, result.PlatformsUpdated);
    }

    [Fact]
    public async Task UpsertPlatformsAsync_NestedWells_UpsertIndependentlyOfParent()
    {
        using var context = CreateInMemoryContext();
        var repo = new PlatformWellRepository(context);

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

        var result = await repo.UpsertPlatformsAsync(new List<PlatformDto> { dto });

        Assert.Equal(2, result.WellsInserted);
        Assert.Equal(2, await context.Wells.CountAsync());
    }

    [Fact]
    public async Task UpsertPlatformsAsync_ExistingWell_ReSyncsPlatformId()
    {
        using var context = CreateInMemoryContext();
        context.Platforms.Add(new Platform { Id = 1, UniqueName = "P1" });
        context.Platforms.Add(new Platform { Id = 2, UniqueName = "P2" });
        context.Wells.Add(new Well { Id = 10, PlatformId = 1, UniqueName = "W10" });
        await context.SaveChangesAsync();
        var repo = new PlatformWellRepository(context);

        var dto = new PlatformDto
        {
            Id = 2,
            UniqueName = "P2",
            Well = new List<WellDto>
            {
                new() { Id = 10, PlatformId = 2, UniqueName = "W10" }
            }
        };

        await repo.UpsertPlatformsAsync(new List<PlatformDto> { dto });

        var saved = await context.Wells.FindAsync(10);
        Assert.Equal(2, saved!.PlatformId);
    }

    [Fact]
    public async Task UpsertPlatformsAsync_RecordMissingId_IsSkippedWithoutThrowing()
    {
        using var context = CreateInMemoryContext();
        var repo = new PlatformWellRepository(context);

        var dto = new PlatformDto
        {
            Id = 1,
            UniqueName = "P1",
            Well = new List<WellDto>
            {
                new() { Id = 0, PlatformId = 0, UniqueName = "MissingId" } // Id defaults to 0 = treated as missing
            }
        };

        var result = await repo.UpsertPlatformsAsync(new List<PlatformDto> { dto });

        Assert.Equal(1, result.PlatformsInserted);
        Assert.Equal(0, result.WellsInserted); // skipped, not inserted as id=0
        Assert.Empty(await context.Wells.ToListAsync());
    }
}
