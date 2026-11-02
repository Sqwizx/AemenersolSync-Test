using AemenersolSync.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AemenersolSync.Api.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly IPlatformWellRepository _repository;
    private readonly ISyncTokenStore _tokenStore;

    public SyncController(ISyncService syncService, IPlatformWellRepository repository, ISyncTokenStore tokenStore)
    {
        _syncService = syncService;
        _repository = repository;
        _tokenStore = tokenStore;
    }

    [HttpPost]
    public async Task<IActionResult> Sync([FromQuery] bool dummy = false)
    {
        if (_tokenStore.GetToken() is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _syncService.RunSyncAsync(useDummyEndpoint: dummy);
            return Ok(result);
        }
        catch (AemenersolUnauthorizedException)
        {
            return Unauthorized();
        }
    }

    [HttpGet("data")]
    public async Task<IActionResult> GetData([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (_tokenStore.GetToken() is null)
        {
            return Unauthorized();
        }

        var result = await _repository.GetPlatformsWithWellsAsync(page, pageSize);
        return Ok(result);
    }
}
