using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SpotDump.HTTP;
using SpotDump.Models;

namespace SpotDump.WebApi.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PlaylistRepository> _logger;
    private readonly IUserProfileRepository _userProfileRepository;
    private string? _currentUserId;

    public PlaylistRepository(
        IHttpClientFactory httpClientFactory,
        ILogger<PlaylistRepository> logger,
        IUserProfileRepository userProfileRepository)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _userProfileRepository = userProfileRepository;
    }

    public async Task<IReadOnlyCollection<PlaylistSummary>> GetPlaylistsAsync(
        PlaylistQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        var playlists = await FetchAllPlaylistsAsync(cancellationToken);
        var currentUserId = options.CreatedByCurrentUser ? await GetCurrentUserIdAsync(cancellationToken) : null;

        var filtered = playlists
            .Where(p => string.IsNullOrWhiteSpace(options.NameContains) ||
                        p.Name.Contains(options.NameContains, StringComparison.OrdinalIgnoreCase))
            .Where(p => string.IsNullOrWhiteSpace(currentUserId) ||
                        string.Equals(p.Owner?.Id, currentUserId, StringComparison.OrdinalIgnoreCase))
            .Select(MapToSummary)
            .ToList();

        return filtered;
    }

    public async Task<PlaylistSummary?> GetPlaylistAsync(string playlistId, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.Client;
        var response = await client.GetAsync($"playlists/{playlistId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to fetch playlist {PlaylistId}. StatusCode: {StatusCode}. Message: {Message}", playlistId, response.StatusCode, message);
            throw new HttpRequestException($"Unable to retrieve playlist {playlistId}", null, response.StatusCode);
        }

        var playlist = await response.Content.ReadFromJsonAsync<PlaylistItem>(cancellationToken: cancellationToken);
        if (playlist == null)
        {
            throw new HttpRequestException($"Spotify returned an empty playlist for {playlistId}", null, HttpStatusCode.InternalServerError);
        }

        return MapToSummary(playlist);
    }

    private async Task<IReadOnlyCollection<PlaylistItem>> FetchAllPlaylistsAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.Client;
        var nextPath = "me/playlists?limit=50";
        var results = new List<PlaylistItem>();

        while (!string.IsNullOrWhiteSpace(nextPath))
        {
            var response = await client.GetAsync(nextPath, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to fetch playlists page. StatusCode: {StatusCode}. Message: {Message}", response.StatusCode, message);
                throw new HttpRequestException("Unable to retrieve playlists from Spotify", null, response.StatusCode);
            }

            var page = await response.Content.ReadFromJsonAsync<PlaylistPageResponse>(cancellationToken: cancellationToken);
            if (page == null)
            {
                _logger.LogError("Spotify returned an empty playlist page.");
                throw new HttpRequestException("Spotify returned an empty playlist page.", null, HttpStatusCode.InternalServerError);
            }

            results.AddRange(page.Items);
            nextPath = page.Next;
        }

        return results;
    }

    private async Task<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_currentUserId))
        {
            return _currentUserId;
        }

        var profile = await _userProfileRepository.GetCurrentUserAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(profile.Id))
        {
            throw new HttpRequestException("Spotify user profile is missing an id.", null, HttpStatusCode.InternalServerError);
        }

        _currentUserId = profile.Id;
        return _currentUserId;
    }

    private static PlaylistSummary MapToSummary(PlaylistItem playlist)
    {
        return new PlaylistSummary
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            OwnerId = playlist.Owner?.Id,
            OwnerName = playlist.Owner?.DisplayName,
            TrackCount = playlist.Tracks?.Total ?? 0,
            Href = playlist.Href,
            Uri = playlist.Uri
        };
    }
}
