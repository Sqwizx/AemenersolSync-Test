using AemenersolSync.Api.Data;
using AemenersolSync.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AemenersolSync.Api.Controllers;

[ApiController]
[Route("api/sync")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly AppDbContext _context;

    public SyncController(ISyncService syncService, AppDbContext context)
    {
        _syncService = syncService;
        _context = context;
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
        var platforms = await _context.Platforms
            .Include(p => p.Wells)
            .AsNoTracking()
            .ToListAsync();

        return Ok(platforms);
    }
}
