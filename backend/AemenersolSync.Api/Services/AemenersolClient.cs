using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using AemenersolSync.Api.Dtos;

namespace AemenersolSync.Api.Services;

public class AemenersolClient : IAemenersolClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public AemenersolClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> LoginAsync()
    {
        var request = new LoginRequestDto
        {
            Username = _configuration["Aemenersol:Username"]!,
            Password = _configuration["Aemenersol:Password"]!
        };

        var response = await _httpClient.PostAsJsonAsync("/api/Account/Login", request, JsonOptions);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AemenersolUnauthorizedException();

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<string>(JsonOptions);
        return token ?? throw new InvalidOperationException("Login succeeded but returned no token.");
    }

    public Task<List<PlatformDto>> GetPlatformWellActualAsync(string token) =>
        GetPlatformWellAsync("/api/PlatformWell/GetPlatformWellActual", token);

    public Task<List<PlatformDto>> GetPlatformWellDummyAsync(string token) =>
        GetPlatformWellAsync("/api/PlatformWell/GetPlatformWellDummy", token);

    private async Task<List<PlatformDto>> GetPlatformWellAsync(string path, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new AemenersolUnauthorizedException();

        response.EnsureSuccessStatusCode();

        var platforms = await response.Content.ReadFromJsonAsync<List<PlatformDto>>(JsonOptions);
        return platforms ?? new List<PlatformDto>();
    }
}
