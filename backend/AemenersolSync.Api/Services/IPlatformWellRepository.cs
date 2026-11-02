using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Models;

namespace AemenersolSync.Api.Services;

public record UpsertResult(int PlatformsInserted, int PlatformsUpdated, int WellsInserted, int WellsUpdated);

public interface IPlatformWellRepository
{
    Task<UpsertResult> UpsertPlatformsAsync(List<PlatformDto> platforms);

    Task<PagedResult<Platform>> GetPlatformsWithWellsAsync(int page, int pageSize);
}
