using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotDump.Models;

namespace SpotDump.HTTP;

public class HttpClientFactory : IDisposable, IHttpClientFactory
{
    private readonly ILogger<HttpClientFactory> _logger;
    private readonly SpotifyAuthOptions _options;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);
    private HttpClient? _client;
    private string? _sessionToken;
    private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;

    public HttpClientFactory(ILogger<HttpClientFactory> logger, IOptions<SpotifyAuthOptions> options)
    {
        _logger = logger;
        _options = PopulateOptionsFromFile(options.Value);
    }

    public bool IsInitialized => !string.IsNullOrWhiteSpace(_options.ClientId) &&
                                 !string.IsNullOrWhiteSpace(_options.ClientSecret);

    public HttpClient Client
    {
        get
        {
            if (_client == null)
            {
                _client = new HttpClient { BaseAddress = new Uri(_options.ApiBaseUrl) };
            }

            EnsureAuthorization().GetAwaiter().GetResult();
            return _client;
        }
    }

    private async Task EnsureAuthorization()
    {
        if (_client == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(_sessionToken) && DateTimeOffset.UtcNow < _tokenExpiresAt)
        {
            return;
        }

        await _tokenSemaphore.WaitAsync();
        try
        {
            if (string.IsNullOrEmpty(_sessionToken) || DateTimeOffset.UtcNow >= _tokenExpiresAt)
            {
                var token = await GenerateSessionTokenAsync();
                _sessionToken = token.AccessToken;
                _tokenExpiresAt = token.ExpiresAt;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _sessionToken);
            }
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    private async Task<TokenResult> GenerateSessionTokenAsync()
    {
        ValidateOptions(_options);
        using var bootstrapClient = new HttpClient { BaseAddress = new Uri(_options.AccountsBaseUrl) };

        var formValues = BuildAuthRequest(_options);
        var request = new HttpRequestMessage(HttpMethod.Post, "token")
        {
            Content = new FormUrlEncodedContent(formValues)
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await bootstrapClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"An error occurred while requesting Spotify credentials: {error}");
        }

        var credentials = await response.Content.ReadFromJsonAsync<ClientCredentialsResponse>();
        if (credentials?.access_token == null || credentials.expires_in == 0)
        {
            throw new Exception("Spotify returned an empty access token.");
        }

        var expirationBufferSeconds = Math.Max(credentials.expires_in - 60, 30);
        return new TokenResult(credentials.access_token, DateTimeOffset.UtcNow.AddSeconds(expirationBufferSeconds));
    }

    private static IEnumerable<KeyValuePair<string, string>> BuildAuthRequest(SpotifyAuthOptions options)
    {
        var form = new List<KeyValuePair<string, string>>
        {
            new("client_id", options.ClientId!),
            new("client_secret", options.ClientSecret!)
        };

        if (!string.IsNullOrWhiteSpace(options.RefreshToken))
        {
            form.Insert(0, new KeyValuePair<string, string>("grant_type", "refresh_token"));
            form.Add(new KeyValuePair<string, string>("refresh_token", options.RefreshToken));
            return form;
        }

        form.Insert(0, new KeyValuePair<string, string>("grant_type", "client_credentials"));
        return form;
    }

    private static void ValidateOptions(SpotifyAuthOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            throw new InvalidOperationException("Spotify ClientId is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            throw new InvalidOperationException("Spotify ClientSecret is not configured.");
        }
    }

    private SpotifyAuthOptions PopulateOptionsFromFile(SpotifyAuthOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.CredentialFilePath) || !File.Exists(options.CredentialFilePath))
        {
            return options;
        }

        _logger.LogInformation("Loading Spotify credentials from {Path}", options.CredentialFilePath);
        var lines = File.ReadAllLines(options.CredentialFilePath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var key = parts[0].ToLowerInvariant();
                var value = parts[1];
                switch (key)
                {
                    case "clientid":
                    case "client_id":
                        options.ClientId ??= value;
                        break;
                    case "clientsecret":
                    case "client_secret":
                        options.ClientSecret ??= value;
                        break;
                    case "refreshtoken":
                    case "refresh_token":
                        options.RefreshToken ??= value;
                        break;
                }
                continue;
            }

            // Backwards compatibility: first line client id, second client secret, third optional refresh token.
            if (string.IsNullOrWhiteSpace(options.ClientId))
            {
                options.ClientId = line.Trim();
            }
            else if (string.IsNullOrWhiteSpace(options.ClientSecret))
            {
                options.ClientSecret = line.Trim();
            }
            else if (string.IsNullOrWhiteSpace(options.RefreshToken))
            {
                options.RefreshToken = line.Trim();
            }
        }

        return options;
    }

    public void Dispose()
    {
        _sessionToken = null;
        _client?.Dispose();
        _tokenSemaphore.Dispose();
    }

    ~HttpClientFactory()
    {
        Dispose();
    }

    private record TokenResult(string AccessToken, DateTimeOffset ExpiresAt);
}
