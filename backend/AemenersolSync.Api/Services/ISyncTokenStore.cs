namespace AemenersolSync.Api.Services;

public interface ISyncTokenStore
{
    string? GetToken();
    void SetToken(string token);
    void ClearToken();
}
