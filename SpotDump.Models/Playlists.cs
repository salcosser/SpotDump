using System.Text.Json.Serialization;

namespace SpotDump.Models;

public class PlaylistOwner
{
    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class PlaylistTracksInfo
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }
}

public class PlaylistItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("owner")]
    public PlaylistOwner? Owner { get; set; }

    [JsonPropertyName("tracks")]
    public PlaylistTracksInfo? Tracks { get; set; }

    [JsonPropertyName("href")]
    public string? Href { get; set; }

    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}

public class PlaylistPageResponse
{
    [JsonPropertyName("href")]
    public string? Href { get; set; }

    [JsonPropertyName("items")]
    public List<PlaylistItem> Items { get; set; } = new();

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class PlaylistSummary
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? OwnerName { get; set; }

    public string? OwnerId { get; set; }

    public int TrackCount { get; set; }

    public string? Href { get; set; }

    public string? Uri { get; set; }
}

public class PlaylistCollectionResult
{
    public IReadOnlyCollection<PlaylistSummary> Playlists { get; init; } = Array.Empty<PlaylistSummary>();

    public int Total => Playlists.Count;
}

public class PlaylistQueryOptions
{
    /// <summary>
    /// Filter playlists down to those created by the authenticated user.
    /// </summary>
    public bool CreatedByCurrentUser { get; set; }

    /// <summary>
    /// Optional text to match against playlist names.
    /// </summary>
    public string? NameContains { get; set; }
}

public class PlaylistExportResult
{
    public string FileName { get; init; } = string.Empty;

    public string ExportPath { get; init; } = string.Empty;

    public string? DownloadUrl { get; init; }

    public int PlaylistCount { get; init; }
}

public class SpotifyUserProfile
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }
}
