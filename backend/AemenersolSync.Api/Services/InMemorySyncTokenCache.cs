namespace AemenersolSync.Api.Services;

public class InMemorySyncTokenCache
{
    private string? _token;
    private readonly object _lock = new();

    public string? GetToken()
    {
        lock (_lock) { return _token; }
    }

    public void SetToken(string token)
    {
        lock (_lock) { _token = token; }
    }

    public void ClearToken()
    {
        lock (_lock) { _token = null; }
    }
}
