using Microsoft.Extensions.DependencyInjection;
using SpotDump.HTTP;
using SpotDump.WebApi.Repositories;

namespace SpotDump.WebApi.Repositories.DI {
    public static class Pack {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services) {
            return services.AddSingleton<IHttpClientFactory, HttpClientFactory>()
                           .AddSingleton<IGenreRepository, GenreRepository>()
                           .AddSingleton<IUserProfileRepository, UserProfileRepository>()
                           .AddSingleton<IPlaylistRepository, PlaylistRepository>()
                           .AddSingleton<IPlaylistExportService, PlaylistExportService>();
        }
    }
}
