using AemenersolSync.Api.Data;
using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Hubs;
using AemenersolSync.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AemenersolSync.Api.Services;

public class SyncService : ISyncService
{
    private readonly IAemenersolClient _client;
    private readonly AppDbContext _context;
    private readonly ISyncTokenStore _tokenStore;
    private readonly IHubContext<SyncHub> _hubContext;
    private readonly ILogger<SyncService>? _logger;

    public SyncService(
        IAemenersolClient client,
        AppDbContext context,
        ISyncTokenStore tokenStore,
        IHubContext<SyncHub> hubContext,
        ILogger<SyncService>? logger = null)
    {
        _client = client;
        _context = context;
        _tokenStore = tokenStore;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<SyncResultDto> RunSyncAsync(bool useDummyEndpoint = false)
    {
        try
        {
            var token = _tokenStore.GetToken();
            if (token is null)
            {
                token = await _client.LoginAsync();
                _tokenStore.SetToken(token);
            }

            var platforms = useDummyEndpoint
                ? await _client.GetPlatformWellDummyAsync(token)
                : await _client.GetPlatformWellActualAsync(token);

            var upsertResult = await UpsertPlatformsAsync(platforms);

            var result = new SyncResultDto
            {
                PlatformsInserted = upsertResult.PlatformsInserted,
                PlatformsUpdated = upsertResult.PlatformsUpdated,
                WellsInserted = upsertResult.WellsInserted,
                WellsUpdated = upsertResult.WellsUpdated,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("syncCompleted", result);
            return result;
        }
        catch (AemenersolUnauthorizedException)
        {
            _tokenStore.ClearToken();
            await _hubContext.Clients.All.SendAsync("unauthorized");
            throw;
        }
    }

    // Upserts Platforms (and their nested Wells) directly against AppDbContext.
    // Previously lived in the now-removed PlatformWellRepository; moved here so
    // SyncService uses AppDbContext directly, consistent with SyncController's
    // direct-DbContext reads. Semantics are unchanged: find-or-insert by external
    // id, apply only non-null DTO fields (null preserves the existing value), and
    // skip malformed records rather than throwing.
    private async Task<UpsertResult> UpsertPlatformsAsync(List<PlatformDto> platforms)
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

    // Internal count-tracking shape for a single upsert pass; not part of any
    // public API surface (unlike the removed IPlatformWellRepository.UpsertResult).
    private record UpsertResult(int PlatformsInserted, int PlatformsUpdated, int WellsInserted, int WellsUpdated);
}
