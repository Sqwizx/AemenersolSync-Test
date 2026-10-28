using AemenersolSync.Api.Dtos;

namespace AemenersolSync.Api.Services;

public record UpsertResult(int PlatformsInserted, int PlatformsUpdated, int WellsInserted, int WellsUpdated);

public interface IPlatformWellRepository
{
    Task<UpsertResult> UpsertPlatformsAsync(List<PlatformDto> platforms);
}
