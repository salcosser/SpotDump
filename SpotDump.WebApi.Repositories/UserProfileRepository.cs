using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SpotDump.HTTP;
using SpotDump.Models;

namespace SpotDump.WebApi.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UserProfileRepository> _logger;

    public UserProfileRepository(IHttpClientFactory httpClientFactory, ILogger<UserProfileRepository> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<SpotifyUserProfile> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.Client;
        var response = await client.GetAsync("me", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to fetch current user. StatusCode: {StatusCode}. Message: {Message}", response.StatusCode, message);
            throw new HttpRequestException("Unable to retrieve current Spotify user profile.", null, response.StatusCode);
        }

        var profile = await response.Content.ReadFromJsonAsync<SpotifyUserProfile>(cancellationToken: cancellationToken);
        if (profile == null)
        {
            _logger.LogError("Spotify returned an empty profile payload.");
            throw new HttpRequestException("Spotify returned an empty profile payload.", null, HttpStatusCode.InternalServerError);
        }

        return profile;
    }
}
