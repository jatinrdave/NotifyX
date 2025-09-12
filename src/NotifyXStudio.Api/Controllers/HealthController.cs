using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for health check endpoints.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly HealthCheckService _healthCheckService;

        public HealthController(ILogger<HealthController> logger, HealthCheckService healthCheckService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        }

        /// <summary>
        /// Gets the overall health status of the application.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                var healthReport = await _healthCheckService.CheckHealthAsync();
                var statusCode = healthReport.Status switch
                {
                    HealthStatus.Healthy => 200,
                    HealthStatus.Degraded => 200,
                    HealthStatus.Unhealthy => 503,
                    _ => 503
                };

                return StatusCode(statusCode, new
                {
                    status = healthReport.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    duration = healthReport.TotalDuration,
                    checks = healthReport.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        duration = entry.Value.Duration,
                        description = entry.Value.Description,
                        data = entry.Value.Data,
                        exception = entry.Value.Exception?.Message
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed: {Message}", ex.Message);
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets the readiness status of the application.
        /// </summary>
        [HttpGet("ready")]
        public async Task<IActionResult> GetReadiness()
        {
            try
            {
                var healthReport = await _healthCheckService.CheckHealthAsync();
                var isReady = healthReport.Status != HealthStatus.Unhealthy;

                return StatusCode(isReady ? 200 : 503, new
                {
                    ready = isReady,
                    timestamp = DateTime.UtcNow,
                    status = healthReport.Status.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed: {Message}", ex.Message);
                return StatusCode(503, new
                {
                    ready = false,
                    timestamp = DateTime.UtcNow,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets the liveness status of the application.
        /// </summary>
        [HttpGet("live")]
        public IActionResult GetLiveness()
        {
            return Ok(new
            {
                alive = true,
                timestamp = DateTime.UtcNow
            });
        }
    }
}