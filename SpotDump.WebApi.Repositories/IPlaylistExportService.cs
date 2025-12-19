using SpotDump.Models;

namespace SpotDump.WebApi.Repositories;

public interface IPlaylistExportService
{
    Task<PlaylistExportResult> WriteCsvAsync(
        IEnumerable<PlaylistSummary> playlists,
        string filePrefix,
        Func<string, string?>? downloadLinkFactory = null,
        CancellationToken cancellationToken = default);

    string ResolvePath(string fileName);
}
