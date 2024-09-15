using Microsoft.Extensions.DependencyInjection;
using SpotDump.HTTP;

namespace SpotDump.WebApi.Repositories.DI {
    public static class Pack {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services) {
            return services.AddSingleton<IHttpClientFactory, HttpClientFactory>()
                           .AddSingleton<IGenreRepository, GenreRepository>();
        }
    }
}
