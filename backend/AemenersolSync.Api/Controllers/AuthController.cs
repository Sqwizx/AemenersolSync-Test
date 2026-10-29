using AemenersolSync.Api.Dtos;
using AemenersolSync.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AemenersolSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAemenersolClient _client;
    private readonly ISyncTokenStore _tokenStore;

    public AuthController(IAemenersolClient client, ISyncTokenStore tokenStore)
    {
        _client = client;
        _tokenStore = tokenStore;
    }

    [HttpPost("login")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var token = await _client.LoginAsync();
            _tokenStore.SetToken(token);
            return Ok(new { authenticated = true });
        }
        catch (AemenersolUnauthorizedException)
        {
            return Unauthorized(new { authenticated = false });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _tokenStore.ClearToken();
        return Ok();
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new { authenticated = _tokenStore.GetToken() is not null });
    }
}
