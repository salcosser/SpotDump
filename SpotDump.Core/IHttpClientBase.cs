
namespace SpotDump.Core {
    public interface IHttpClientBase {
        HttpClient Client { get; }
        bool IsInitialized { get; }

        void Dispose();
    }
}