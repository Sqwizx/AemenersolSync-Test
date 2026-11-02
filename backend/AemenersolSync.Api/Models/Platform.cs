namespace AemenersolSync.Api.Models;

public class Platform : AuditableEntity
{
    public int Id { get; set; }
    public string? UniqueName { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public List<Well> Wells { get; set; } = new();
}
