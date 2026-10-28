namespace AemenersolSync.Api.Dtos;

public class SyncResultDto
{
    public int PlatformsInserted { get; set; }
    public int PlatformsUpdated { get; set; }
    public int WellsInserted { get; set; }
    public int WellsUpdated { get; set; }
    public DateTime Timestamp { get; set; }
}
