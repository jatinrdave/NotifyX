using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for status operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;
        private readonly IStatusService _statusService;

        public StatusController(ILogger<StatusController> logger, IStatusService statusService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        }

        /// <summary>
        /// Gets overall system status.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var status = await _statusService.GetStatusAsync();

                return Ok(new
                {
                    status,
                    checkedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets component status.
        /// </summary>
        [HttpGet("components")]
        public async Task<IActionResult> GetComponentStatus()
        {
            try
            {
                var componentStatus = await _statusService.GetComponentStatusAsync();

                return Ok(new
                {
                    componentStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get component status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve component status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets service status.
        /// </summary>
        [HttpGet("services")]
        public async Task<IActionResult> GetServiceStatus()
        {
            try
            {
                var serviceStatus = await _statusService.GetServiceStatusAsync();

                return Ok(new
                {
                    serviceStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get service status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve service status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets database status.
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                var databaseStatus = await _statusService.GetDatabaseStatusAsync();

                return Ok(new
                {
                    databaseStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve database status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets queue status.
        /// </summary>
        [HttpGet("queues")]
        public async Task<IActionResult> GetQueueStatus()
        {
            try
            {
                var queueStatus = await _statusService.GetQueueStatusAsync();

                return Ok(new
                {
                    queueStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get queue status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve queue status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets external service status.
        /// </summary>
        [HttpGet("external")]
        public async Task<IActionResult> GetExternalServiceStatus()
        {
            try
            {
                var externalStatus = await _statusService.GetExternalServiceStatusAsync();

                return Ok(new
                {
                    externalStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get external service status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve external service status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets performance status.
        /// </summary>
        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceStatus()
        {
            try
            {
                var performanceStatus = await _statusService.GetPerformanceStatusAsync();

                return Ok(new
                {
                    performanceStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get performance status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve performance status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets security status.
        /// </summary>
        [HttpGet("security")]
        public async Task<IActionResult> GetSecurityStatus()
        {
            try
            {
                var securityStatus = await _statusService.GetSecurityStatusAsync();

                return Ok(new
                {
                    securityStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get security status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve security status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets compliance status.
        /// </summary>
        [HttpGet("compliance")]
        public async Task<IActionResult> GetComplianceStatus()
        {
            try
            {
                var complianceStatus = await _statusService.GetComplianceStatusAsync();

                return Ok(new
                {
                    complianceStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get compliance status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve compliance status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets maintenance status.
        /// </summary>
        [HttpGet("maintenance")]
        public async Task<IActionResult> GetMaintenanceStatus()
        {
            try
            {
                var maintenanceStatus = await _statusService.GetMaintenanceStatusAsync();

                return Ok(new
                {
                    maintenanceStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get maintenance status: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve maintenance status",
                    message = ex.Message
                });
            }
        }
    }
}