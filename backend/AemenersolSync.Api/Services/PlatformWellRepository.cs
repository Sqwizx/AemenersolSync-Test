using AemenersolSync.Api.Data;
using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Models;
using Microsoft.Extensions.Logging;

namespace AemenersolSync.Api.Services;

public class PlatformWellRepository : IPlatformWellRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PlatformWellRepository>? _logger;

    public PlatformWellRepository(AppDbContext context, ILogger<PlatformWellRepository>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UpsertResult> UpsertPlatformsAsync(List<PlatformDto> platforms)
    {
        int platformsInserted = 0, platformsUpdated = 0, wellsInserted = 0, wellsUpdated = 0;

        foreach (var platformDto in platforms)
        {
            if (platformDto.Id <= 0)
            {
                _logger?.LogWarning("Skipping platform record with missing/invalid id.");
                continue;
            }

            var platform = await _context.Platforms.FindAsync(platformDto.Id);
            if (platform is null)
            {
                platform = new Platform { Id = platformDto.Id };
                _context.Platforms.Add(platform);
                platformsInserted++;
            }
            else
            {
                platformsUpdated++;
            }

            ApplyPlatformFields(platform, platformDto);

            foreach (var wellDto in platformDto.Well)
            {
                if (wellDto.Id <= 0 || wellDto.PlatformId <= 0)
                {
                    _logger?.LogWarning("Skipping well record with missing/invalid id or platformId.");
                    continue;
                }

                var well = await _context.Wells.FindAsync(wellDto.Id);
                if (well is null)
                {
                    well = new Well { Id = wellDto.Id, PlatformId = wellDto.PlatformId };
                    _context.Wells.Add(well);
                    wellsInserted++;
                }
                else
                {
                    wellsUpdated++;
                }

                ApplyWellFields(well, wellDto);
            }
        }

        await _context.SaveChangesAsync();

        return new UpsertResult(platformsInserted, platformsUpdated, wellsInserted, wellsUpdated);
    }

    private static void ApplyPlatformFields(Platform platform, PlatformDto dto)
    {
        if (dto.UniqueName is not null) platform.UniqueName = dto.UniqueName;
        if (dto.Latitude is not null) platform.Latitude = dto.Latitude;
        if (dto.Longitude is not null) platform.Longitude = dto.Longitude;
        if (dto.CreatedAt is not null) platform.CreatedAt = dto.CreatedAt;
        if (dto.UpdatedAt is not null) platform.UpdatedAt = dto.UpdatedAt;
    }

    private static void ApplyWellFields(Well well, WellDto dto)
    {
        well.PlatformId = dto.PlatformId;
        if (dto.UniqueName is not null) well.UniqueName = dto.UniqueName;
        if (dto.Latitude is not null) well.Latitude = dto.Latitude;
        if (dto.Longitude is not null) well.Longitude = dto.Longitude;
        if (dto.CreatedAt is not null) well.CreatedAt = dto.CreatedAt;
        if (dto.UpdatedAt is not null) well.UpdatedAt = dto.UpdatedAt;
    }
}
