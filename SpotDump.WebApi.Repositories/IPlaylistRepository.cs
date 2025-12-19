using SpotDump.Models;

namespace SpotDump.WebApi.Repositories;

public interface IPlaylistRepository
{
    Task<IReadOnlyCollection<PlaylistSummary>> GetPlaylistsAsync(PlaylistQueryOptions options, CancellationToken cancellationToken = default);

    Task<PlaylistSummary?> GetPlaylistAsync(string playlistId, CancellationToken cancellationToken = default);
}
