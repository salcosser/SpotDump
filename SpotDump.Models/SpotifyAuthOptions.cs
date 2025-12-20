namespace SpotDump.Models;

public class SpotifyAuthOptions
{
    public const string ConfigurationSectionName = "Spotify";

    /// <summary>
    /// Optional path to a credentials file. When provided, values in this file
    /// will be used to populate any missing option values at runtime.
    /// </summary>
    public string? CredentialFilePath { get; set; }

    /// <summary>
    /// Client Id obtained from Spotify's developer portal.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Client Secret obtained from Spotify's developer portal.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Refresh token granted to the application for the authenticated user.
    /// Required for user-specific endpoints like playlists.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Base API URL for Spotify. Defaults to the public API URL.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.spotify.com/v1/";

    /// <summary>
    /// Spotify accounts service URL for authentication flows.
    /// </summary>
    public string AccountsBaseUrl { get; set; } = "https://accounts.spotify.com/api/";

    /// <summary>
    /// Directory where exports (CSV files) will be written.
    /// </summary>
    public string ExportDirectory { get; set; } = "Exports";
}
