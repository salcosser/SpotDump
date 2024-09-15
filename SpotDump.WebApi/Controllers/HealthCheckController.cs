using Microsoft.AspNetCore.Mvc;

namespace SpotDump.WebApi.Controllers {
    [ApiController]
    [Route("[controller]/[action]")]
    public class HealthCheckController : ControllerBase {
        private readonly ILogger<HealthCheckController> _logger;

        public HealthCheckController(ILogger<HealthCheckController> logger) {
            _logger = logger;
        }

        [HttpGet(Name = "CheckHealth")]
        public IActionResult Get() {
            return Ok("Healthy");
        }

        [HttpGet(Name = "Error")]
        public IActionResult Error() {
            try {
                throw new Exception("Healthcheck test error.");
            } catch (Exception ex) {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Error");
            }
        }
    }
}
