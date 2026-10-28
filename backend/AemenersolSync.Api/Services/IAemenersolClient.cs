using AemenersolSync.Api.Dtos;

namespace AemenersolSync.Api.Services;

public interface IAemenersolClient
{
    Task<string> LoginAsync();
    Task<List<PlatformDto>> GetPlatformWellActualAsync(string token);
    Task<List<PlatformDto>> GetPlatformWellDummyAsync(string token);
}

public class AemenersolUnauthorizedException : Exception { }
