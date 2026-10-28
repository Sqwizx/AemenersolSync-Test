namespace AemenersolSync.Api.Models;

public class Platform
{
    public int Id { get; set; }
    public string? UniqueName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<Well> Wells { get; set; } = new();
}
