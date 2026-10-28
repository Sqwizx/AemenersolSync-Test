using AemenersolSync.Api.Dtos;

namespace AemenersolSync.Api.Services;

public interface ISyncService
{
    Task<SyncResultDto> RunSyncAsync(bool useDummyEndpoint = false);
}
