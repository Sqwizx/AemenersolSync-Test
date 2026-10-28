using AemenersolSync.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AemenersolSync.Api.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly IPlatformWellRepository _repository;

    public SyncController(ISyncService syncService, IPlatformWellRepository repository)
    {
        _syncService = syncService;
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Sync([FromQuery] bool dummy = false)
    {
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
    public async Task<IActionResult> GetData()
    {
        var platforms = await _repository.GetAllPlatformsWithWellsAsync();
        return Ok(platforms);
    }
}
