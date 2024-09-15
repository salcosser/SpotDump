using Microsoft.Extensions.Logging;
using SpotDump.HTTP;
using SpotDump.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SpotDump.WebApi.Repositories {
    public class GenreRepository : IGenreRepository {
        private readonly ILogger<GenreRepository> _log;
        private readonly HttpClient _client;
        public GenreRepository(ILogger<GenreRepository> log, IHttpClientFactory factory) {
            _log = log;
            _client = factory.Client;
        }

        public async Task<List<string>> GetGeneresAsync() {
            var resp = await _client.GetAsync("recommendations/available-genre-seeds");

            if (resp.IsSuccessStatusCode) {
                var result = await resp.Content.ReadFromJsonAsync<GenreSeeds>();
                return result?.genres ?? throw new Exception("response null");
            }
            throw new Exception(resp.Content.ReadAsStringAsync().Result);
        }
    }
}
