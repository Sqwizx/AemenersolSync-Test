using AemenersolSync.Api.Data;
using AemenersolSync.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace AemenersolSync.Api.Tests.Data;

public class AppDbContextModelTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void Model_PlatformId_IsConfiguredAsValueGeneratedNever()
    {
        using var context = CreateInMemoryContext();

        var entityType = context.Model.FindEntityType(typeof(Platform));
        var idProperty = entityType!.FindProperty(nameof(Platform.Id));

        Assert.Equal(ValueGenerated.Never, idProperty!.ValueGenerated);
    }

    [Fact]
    public void Model_WellId_IsConfiguredAsValueGeneratedNever()
    {
        using var context = CreateInMemoryContext();

        var entityType = context.Model.FindEntityType(typeof(Well));
        var idProperty = entityType!.FindProperty(nameof(Well.Id));

        Assert.Equal(ValueGenerated.Never, idProperty!.ValueGenerated);
    }
}
