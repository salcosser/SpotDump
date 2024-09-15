using System.Net.Http.Json;
using System.Text;

namespace SpotDump.ClientApi {
    public class HttpClientBase : IDisposable {
        private const string _authPath = "C:\\SANDBOX\\__Auth\\SpotDumpAuth.txt";
        private static string? _sessionToken = string.Empty;
        public bool IsInitialized => _sessionToken != null;

        private HttpClient? _client;
        public HttpClient Client {
            get {
                if (_client == null) {
                    _client = new HttpClient { BaseAddress = new Uri("https://api.spotify.com/v1/") };
                    var tk = string.IsNullOrEmpty(_sessionToken) ? GetAndRefreshToken() : _sessionToken;
                    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tk);
                }
                return _client;
            }
        }
        

        /// <summary>
        /// Regenerates a session token, sets it on the private member, and returns it
        /// </summary>
        /// <returns></returns>
        protected string GetAndRefreshToken() {
            if (string.IsNullOrEmpty(_sessionToken)) {
                var tkInitTask = Task.Run(() => GenerateSessionToken(_authPath, true));
                tkInitTask.Wait();
                _sessionToken = tkInitTask.Result;
                return _sessionToken;
            }
            var tkTask = Task.Run(() => GenerateSessionToken(_authPath, false));
            tkTask.Wait();
            _sessionToken = tkTask.Result;
            return _sessionToken;
        }


        private async Task<string> GenerateSessionToken(string authKeyPath, bool isBootstrap) {
            string? clientId, clientSecret;
            
            // read in auth keys
            using (var fs = new StreamReader(authKeyPath, Encoding.UTF8)) {
                if (fs.EndOfStream) { throw new Exception("keys missing."); }
                clientId = fs.ReadLine();
                if (fs.EndOfStream) { throw new Exception("client secret missing."); }
                clientSecret = fs.ReadLine();
            }
            if (string.IsNullOrEmpty(clientId)) { throw new Exception("client Id empty or null"); }
            if (string.IsNullOrEmpty(clientSecret)) { throw new Exception("client secret empty or null"); }

            using (var bootstrapClient = new HttpClient { BaseAddress = new Uri("https://accounts.spotify.com/api/") }) {

               

                var req = new HttpRequestMessage(HttpMethod.Post, "token");
                req.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>{
                    new KeyValuePair<string, string>("grant_type", isBootstrap ? "client_credentials" : "refresh_token"),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });
                req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
                var credsResp = await bootstrapClient.SendAsync(req);



                if (credsResp.IsSuccessStatusCode) {
                    var creds = await credsResp.Content.ReadFromJsonAsync<ClientCredentialsResponse>();
                    if (creds?.access_token == null) { throw new Exception("Null Access Token returned!"); }
                    return creds.access_token;
                }

                throw new Exception($"An error occurred while making the request for credentials\n{credsResp.Content.ReadAsStringAsync()}");
            }
        }

        public void Dispose() {
            _sessionToken = null;
            if( _client != null ) {
                _client.Dispose();
            }
        }
        ~HttpClientBase() { this.Dispose(); }
    }
}
