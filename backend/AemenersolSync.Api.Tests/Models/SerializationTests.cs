using System.Text.Json;
using AemenersolSync.Api.Models;
using Xunit;

namespace AemenersolSync.Api.Tests.Models;

public class SerializationTests
{
    [Fact]
    public void Serialize_PlatformWithWellsCycle_DoesNotThrowAndOmitsPlatformBackReference()
    {
        var platform = new Platform
        {
            Id = 1,
            UniqueName = "P1",
            Latitude = 1.1,
            Longitude = 2.2
        };
        var well = new Well
        {
            Id = 10,
            PlatformId = 1,
            UniqueName = "W10",
            Platform = platform // creates the cycle: platform.Wells[0].Platform == platform
        };
        platform.Wells.Add(well);

        // Must not throw a JsonException about a possible object cycle.
        var json = JsonSerializer.Serialize(platform);

        Assert.NotNull(json);
        // Confirms [JsonIgnore] actually excluded Well.Platform, not merely that
        // serialization happened to succeed for some unrelated reason.
        Assert.DoesNotContain("\"platform\"", json, StringComparison.OrdinalIgnoreCase);
    }
}
