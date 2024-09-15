using Microsoft.AspNetCore.Mvc;
using SpotDump.WebApi.Repositories;
using System.Net;

namespace SpotDump.WebApi.Controllers {
    [ApiController]
    [Route("[controller]/[action]")]
    public class GenreController : ControllerBase{
        private readonly IGenreRepository _repo;
        private readonly ILogger<GenreController> _log;
        public GenreController(ILogger<GenreController> log, IGenreRepository genreRepository) {
            _repo = genreRepository;
            _log = log;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync() {
            try {
                var res = await _repo.GetGeneresAsync();
                return Ok(res);
            } catch (Exception ex) {
                _log.LogError(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred");
            }
        }


    }
}
