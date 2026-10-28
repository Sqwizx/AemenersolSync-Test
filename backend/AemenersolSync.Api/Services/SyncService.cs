using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AemenersolSync.Api.Services;

public class SyncService : ISyncService
{
    private readonly IAemenersolClient _client;
    private readonly IPlatformWellRepository _repository;
    private readonly ISyncTokenStore _tokenStore;
    private readonly IHubContext<SyncHub> _hubContext;

    public SyncService(
        IAemenersolClient client,
        IPlatformWellRepository repository,
        ISyncTokenStore tokenStore,
        IHubContext<SyncHub> hubContext)
    {
        _client = client;
        _repository = repository;
        _tokenStore = tokenStore;
        _hubContext = hubContext;
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

            var upsertResult = await _repository.UpsertPlatformsAsync(platforms);

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
}
