using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for system operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly ILogger<SystemController> _logger;
        private readonly ISystemService _systemService;

        public SystemController(ILogger<SystemController> logger, ISystemService systemService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
        }

        /// <summary>
        /// Gets system information.
        /// </summary>
        [HttpGet("info")]
        public async Task<IActionResult> GetSystemInfo()
        {
            try
            {
                var systemInfo = await _systemService.GetSystemInfoAsync();

                return Ok(new
                {
                    systemInfo,
                    retrievedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system info: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve system information",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets system status.
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetSystemStatus()
        {
            try
            {
                var systemStatus = await _systemService.GetSystemStatusAsync();

                return Ok(new
                {
                    systemStatus,
                    checkedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve system status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets system metrics.
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetSystemMetrics()
        {
            try
            {
                var systemMetrics = await _systemService.GetSystemMetricsAsync();

                return Ok(new
                {
                    systemMetrics,
                    collectedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system metrics: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve system metrics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets system logs.
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetSystemLogs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? level,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-1);
                var end = endDate ?? DateTime.UtcNow;

                var logs = await _systemService.GetSystemLogsAsync(start, end, level, page, pageSize);
                var totalCount = await _systemService.GetSystemLogCountAsync(start, end, level);

                return Ok(new
                {
                    logs,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve system logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets system alerts.
        /// </summary>
        [HttpGet("alerts")]
        public async Task<IActionResult> GetSystemAlerts()
        {
            try
            {
                var alerts = await _systemService.GetSystemAlertsAsync();

                return Ok(new
                {
                    alerts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system alerts: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve system alerts",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Dismisses a system alert.
        /// </summary>
        [HttpPost("alerts/{alertId}/dismiss")]
        public async Task<IActionResult> DismissSystemAlert(string alertId)
        {
            try
            {
                await _systemService.DismissSystemAlertAsync(alertId);

                return Ok(new
                {
                    message = "System alert dismissed successfully",
                    alertId,
                    dismissedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dismiss system alert {AlertId}: {Message}", alertId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to dismiss system alert",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets system configuration.
        /// </summary>
        [HttpGet("config")]
        public async Task<IActionResult> GetSystemConfig()
        {
            try
            {
                var config = await _systemService.GetSystemConfigAsync();

                return Ok(new
                {
                    config
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system config: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve system configuration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates system configuration.
        /// </summary>
        [HttpPut("config")]
        public async Task<IActionResult> UpdateSystemConfig([FromBody] UpdateSystemConfigRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Configuration request is required");
                }

                await _systemService.UpdateSystemConfigAsync(request.Config);

                return Ok(new
                {
                    message = "System configuration updated successfully",
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update system config: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update system configuration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Restarts the system.
        /// </summary>
        [HttpPost("restart")]
        public async Task<IActionResult> RestartSystem()
        {
            try
            {
                await _systemService.RestartSystemAsync();

                return Ok(new
                {
                    message = "System restart initiated successfully",
                    restartedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart system: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to restart system",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Shuts down the system.
        /// </summary>
        [HttpPost("shutdown")]
        public async Task<IActionResult> ShutdownSystem()
        {
            try
            {
                await _systemService.ShutdownSystemAsync();

                return Ok(new
                {
                    message = "System shutdown initiated successfully",
                    shutdownAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to shutdown system: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to shutdown system",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Update system config request model.
    /// </summary>
    public class UpdateSystemConfigRequest
    {
        /// <summary>
        /// System configuration.
        /// </summary>
        public Dictionary<string, object> Config { get; set; } = new();
    }
}