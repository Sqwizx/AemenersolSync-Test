namespace AemenersolSync.Api.Services;

public class SessionSyncTokenStore : ISyncTokenStore
{
    private const string TokenKey = "AemenersolToken";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionSyncTokenStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetToken() =>
        _httpContextAccessor.HttpContext?.Session.GetString(TokenKey);

    public void SetToken(string token) =>
        _httpContextAccessor.HttpContext?.Session.SetString(TokenKey, token);

    public void ClearToken() =>
        _httpContextAccessor.HttpContext?.Session.Remove(TokenKey);
}
