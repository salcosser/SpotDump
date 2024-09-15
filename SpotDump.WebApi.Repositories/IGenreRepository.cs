
namespace SpotDump.WebApi.Repositories {
    public interface IGenreRepository {
        Task<List<string>> GetGeneresAsync();
    }
}