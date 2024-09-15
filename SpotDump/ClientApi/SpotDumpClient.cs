using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SpotDump.ClientApi {
    public class SpotDumpClient {
        private readonly HttpClientBase _httpClient;
        public SpotDumpClient() { 
            _httpClient = new HttpClientBase();
        }
        ~SpotDumpClient() {
            if (_httpClient != null) {
                _httpClient.Dispose();
            }    
        }

        public async Task<List<string>?> GetGeneresAsync() {
            var resp = await _httpClient.Client.GetAsync("recommendations/available-genre-seeds");

            if (resp.IsSuccessStatusCode) {
                var result =  await resp.Content.ReadFromJsonAsync<GenreSeeds>();
                return result?.genres;
            }
            throw new Exception(resp.Content.ReadAsStringAsync().Result);
        }

    }
}
