using System.Text.Json;
using AemenersolSync.Api.Dtos;
using Xunit;

namespace AemenersolSync.Api.Tests.Services;

public class PlatformWellDeserializationTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Deserialize_ActualSample_PopulatesAllKnownFields()
    {
        var json = File.ReadAllText("TestData/actual-sample.json");

        var platforms = JsonSerializer.Deserialize<List<PlatformDto>>(json, Options)!;

        Assert.Single(platforms);
        var platform = platforms[0];
        Assert.Equal(11, platform.Id);
        Assert.Equal("Platform1", platform.UniqueName);
        Assert.Equal(1.012, platform.Latitude);
        Assert.NotNull(platform.CreatedAt);
        Assert.NotNull(platform.UpdatedAt);
        Assert.Single(platform.Well);
        Assert.Equal(1, platform.Well[0].Id);
        Assert.Equal(11, platform.Well[0].PlatformId);
    }

    [Fact]
    public void Deserialize_DummySample_MissingCreatedAtUpdatedAt_DoesNotThrow()
    {
        var json = File.ReadAllText("TestData/dummy-sample.json");

        var platforms = JsonSerializer.Deserialize<List<PlatformDto>>(json, Options)!;

        Assert.Single(platforms);
        var platform = platforms[0];
        Assert.Equal(11, platform.Id);
        Assert.Equal("Platform1", platform.UniqueName);
        Assert.Null(platform.CreatedAt);
        Assert.Null(platform.UpdatedAt);
        Assert.Single(platform.Well);
        Assert.Null(platform.Well[0].CreatedAt);
        Assert.Null(platform.Well[0].UpdatedAt);
    }

    [Fact]
    public void Deserialize_JsonWithUnexpectedNewKey_DoesNotThrow()
    {
        var json = """
        [{"id": 99, "uniqueName": "Edge", "latitude": 1.0, "longitude": 2.0, "brandNewField": "ignored", "well": []}]
        """;

        var platforms = JsonSerializer.Deserialize<List<PlatformDto>>(json, Options)!;

        Assert.Single(platforms);
        Assert.Equal(99, platforms[0].Id);
        Assert.Equal("Edge", platforms[0].UniqueName);
    }
}
