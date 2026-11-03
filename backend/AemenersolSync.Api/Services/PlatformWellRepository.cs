using AemenersolSync.Api.Data;
using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Models;
using Microsoft.EntityFrameworkCore;
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
        var result = await UpsertRecordsAsync(platforms);
        await _context.SaveChangesAsync();

        return result;
    }

    private async Task<UpsertResult> UpsertRecordsAsync(IReadOnlyCollection<PlatformDto> platforms)
    {
        int platformsInserted = 0, platformsUpdated = 0, wellsInserted = 0, wellsUpdated = 0;

        var platformIds = platforms
            .Where(platform => platform.Id > 0)
            .Select(platform => platform.Id)
            .Distinct()
            .ToArray();
        var wellIds = platforms
            .Where(platform => platform.Id > 0)
            .SelectMany(platform => platform.Well)
            .Where(well => well.Id > 0 && well.PlatformId > 0)
            .Select(well => well.Id)
            .Distinct()
            .ToArray();

        var platformsById = platformIds.Length == 0
            ? new Dictionary<int, Platform>()
            : await _context.Platforms
                .Where(platform => platformIds.Contains(platform.Id))
                .ToDictionaryAsync(platform => platform.Id);
        var wellsById = wellIds.Length == 0
            ? new Dictionary<int, Well>()
            : await _context.Wells
                .Where(well => wellIds.Contains(well.Id))
                .ToDictionaryAsync(well => well.Id);

        foreach (var platformDto in platforms)
        {
            if (platformDto.Id <= 0)
            {
                _logger?.LogWarning("Skipping platform record with missing/invalid id.");
                continue;
            }

            if (!platformsById.TryGetValue(platformDto.Id, out var platform))
            {
                platform = new Platform { Id = platformDto.Id };
                _context.Platforms.Add(platform);
                platformsById.Add(platform.Id, platform);
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

                if (!wellsById.TryGetValue(wellDto.Id, out var well))
                {
                    well = new Well { Id = wellDto.Id, PlatformId = wellDto.PlatformId };
                    _context.Wells.Add(well);
                    wellsById.Add(well.Id, well);
                    wellsInserted++;
                }
                else
                {
                    wellsUpdated++;
                }

                ApplyWellFields(well, wellDto);
            }
        }

        return new UpsertResult(platformsInserted, platformsUpdated, wellsInserted, wellsUpdated);
    }

    private const int MaxPageSize = 100;

    public async Task<PagedResult<Platform>> GetPlatformsWithWellsAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var totalCount = await _context.Platforms.CountAsync();

        var items = await _context.Platforms
            .Include(p => p.Wells)
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Platform>(items, page, pageSize, totalCount);
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
