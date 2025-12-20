using SpotDump.Models;

namespace SpotDump.WebApi.Repositories;

public interface IUserProfileRepository
{
    Task<SpotifyUserProfile> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
