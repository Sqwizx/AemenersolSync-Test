using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Models;

namespace AemenersolSync.Api.Services;

public record UpsertResult(int PlatformsInserted, int PlatformsUpdated, int WellsInserted, int WellsUpdated);

public interface IPlatformWellRepository
{
    Task<UpsertResult> UpsertPlatformsAsync(List<PlatformDto> platforms);

    // Returns every Platform with its Wells included, for display purposes
    // (e.g. GET /api/sync/data). No filtering/pagination: this project's
    // dataset is small enough to always return it in full.
    Task<List<Platform>> GetAllPlatformsWithWellsAsync();
}
