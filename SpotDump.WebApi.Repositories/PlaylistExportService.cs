using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotDump.Models;

namespace SpotDump.WebApi.Repositories;

public class PlaylistExportService : IPlaylistExportService
{
    private readonly ILogger<PlaylistExportService> _logger;
    private readonly string _exportRoot;

    public PlaylistExportService(IOptions<SpotifyAuthOptions> options, ILogger<PlaylistExportService> logger)
    {
        _logger = logger;
        var exportDirectory = options.Value.ExportDirectory;
        _exportRoot = Path.IsPathRooted(exportDirectory)
            ? exportDirectory
            : Path.Combine(AppContext.BaseDirectory, exportDirectory);

        Directory.CreateDirectory(_exportRoot);
    }

    public async Task<PlaylistExportResult> WriteCsvAsync(
        IEnumerable<PlaylistSummary> playlists,
        string filePrefix,
        Func<string, string?>? downloadLinkFactory = null,
        CancellationToken cancellationToken = default)
    {
        var playlistList = playlists.ToList();
        var fileName = $"{Sanitize(filePrefix)}_{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.csv";
        var exportPath = Path.Combine(_exportRoot, fileName);

        await using (var writer = new StreamWriter(exportPath, false, Encoding.UTF8))
        {
            await writer.WriteLineAsync("Id,Name,Description,OwnerName,OwnerId,TrackCount,Href,Uri");
            foreach (var playlist in playlistList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = string.Join(',', new[]
                {
                    Escape(playlist.Id),
                    Escape(playlist.Name),
                    Escape(playlist.Description),
                    Escape(playlist.OwnerName),
                    Escape(playlist.OwnerId),
                    playlist.TrackCount.ToString(CultureInfo.InvariantCulture),
                    Escape(playlist.Href),
                    Escape(playlist.Uri)
                });
                await writer.WriteLineAsync(row);
            }
        }

        var downloadUrl = downloadLinkFactory?.Invoke(fileName);
        _logger.LogInformation("Saved playlist export to {Path}", exportPath);

        return new PlaylistExportResult
        {
            FileName = fileName,
            ExportPath = exportPath,
            DownloadUrl = downloadUrl,
            PlaylistCount = playlistList.Count
        };
    }

    public string ResolvePath(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName);
        return Path.Combine(_exportRoot, safeFileName);
    }

    private static string Sanitize(string input)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            builder.Append(invalidCharacters.Contains(ch) ? '_' : ch);
        }
        return builder.ToString().Trim('_');
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "\"\"";
        }

        var sanitized = value.Replace("\"", "\"\"");
        return $"\"{sanitized}\"";
    }
}
