using System.Text.Json.Serialization;

namespace AemenersolSync.Api.Models;

public class Well
{
    public int Id { get; set; }
    public int PlatformId { get; set; }
    public string? UniqueName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Ignored for JSON serialization to avoid a Platform -> Wells -> Platform
    // cycle when GET /api/sync/data serializes Platforms with their nested Wells.
    [JsonIgnore]
    public Platform? Platform { get; set; }
}
