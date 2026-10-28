namespace AemenersolSync.Api.Services;

public class SessionSyncTokenStore : ISyncTokenStore
{
    private readonly InMemorySyncTokenCache _cache;

    public SessionSyncTokenStore(InMemorySyncTokenCache cache)
    {
        _cache = cache;
    }

    public string? GetToken() => _cache.GetToken();

    public void SetToken(string token) => _cache.SetToken(token);

    public void ClearToken() => _cache.ClearToken();
}
