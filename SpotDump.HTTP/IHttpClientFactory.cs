
namespace SpotDump.HTTP {
    public interface IHttpClientFactory {
        HttpClient Client { get; }
        bool IsInitialized { get; }

        void Dispose();
    }
}