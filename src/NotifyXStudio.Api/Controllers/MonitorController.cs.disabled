using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for monitoring operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MonitorController : ControllerBase
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly IMonitorService _monitorService;

        public MonitorController(ILogger<MonitorController> logger, IMonitorService monitorService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
        }

        /// <summary>
        /// Gets monitoring dashboard data.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetMonitoringDashboard()
        {
            try
            {
                var dashboard = await _monitorService.GetMonitoringDashboardAsync();

                return Ok(new
                {
                    dashboard,
                    generatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get monitoring dashboard: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve monitoring dashboard",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets monitoring metrics.
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMonitoringMetrics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? metricType)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddHours(-1);
                var end = endDate ?? DateTime.UtcNow;

                var metrics = await _monitorService.GetMonitoringMetricsAsync(null, metricType);

                return Ok(new
                {
                    metrics,
                    dateRange = new { start, end }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get monitoring metrics: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve monitoring metrics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets monitoring alerts.
        /// </summary>
        [HttpGet("alerts")]
        public async Task<IActionResult> GetMonitoringAlerts()
        {
            try
            {
                var alerts = await _monitorService.GetMonitoringAlertsAsync();

                return Ok(new
                {
                    alerts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get monitoring alerts: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve monitoring alerts",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets monitoring thresholds.
        /// </summary>
        [HttpGet("thresholds")]
        public async Task<IActionResult> GetMonitoringThresholds()
        {
            try
            {
                var thresholds = await _monitorService.GetMonitoringThresholdsAsync();

                return Ok(new
                {
                    thresholds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get monitoring thresholds: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve monitoring thresholds",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates monitoring thresholds.
        /// </summary>
        [HttpPut("thresholds")]
        public async Task<IActionResult> UpdateMonitoringThresholds([FromBody] UpdateMonitoringThresholdsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Thresholds request is required");
                }

                await _monitorService.UpdateMonitoringThresholdsAsync(request.Thresholds);

                return Ok(new
                {
                    message = "Monitoring thresholds updated successfully",
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update monitoring thresholds: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update monitoring thresholds",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets monitoring reports.
        /// </summary>
        [HttpGet("reports")]
        public async Task<IActionResult> GetMonitoringReports(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? reportType)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                var reports = await _monitorService.GetMonitoringReportsAsync(null, reportType);

                return Ok(new
                {
                    reports,
                    dateRange = new { start, end }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get monitoring reports: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve monitoring reports",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Generates monitoring report.
        /// </summary>
        [HttpPost("reports/generate")]
        public async Task<IActionResult> GenerateMonitoringReport([FromBody] GenerateMonitoringReportRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Report request is required");
                }

                var reportId = await _monitorService.GenerateMonitoringReportAsync(
                    null,
                    request.StartDate,
                    request.EndDate);

                return Ok(new
                {
                    reportId,
                    message = "Monitoring report generation initiated successfully",
                    generatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate monitoring report: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to generate monitoring report",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets monitoring configuration.
        /// </summary>
        [HttpGet("config")]
        public async Task<IActionResult> GetMonitoringConfig()
        {
            try
            {
                var config = await _monitorService.GetMonitoringConfigAsync();

                return Ok(new
                {
                    config
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get monitoring config: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve monitoring configuration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates monitoring configuration.
        /// </summary>
        [HttpPut("config")]
        public async Task<IActionResult> UpdateMonitoringConfig([FromBody] UpdateMonitoringConfigRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Configuration request is required");
                }

                await _monitorService.UpdateMonitoringConfigAsync(request.Config);

                return Ok(new
                {
                    message = "Monitoring configuration updated successfully",
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update monitoring config: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update monitoring configuration",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Update monitoring thresholds request model.
    /// </summary>
    public class UpdateMonitoringThresholdsRequest
    {
        /// <summary>
        /// Monitoring thresholds.
        /// </summary>
        public Dictionary<string, object> Thresholds { get; set; } = new();
    }

    /// <summary>
    /// Generate monitoring report request model.
    /// </summary>
    public class GenerateMonitoringReportRequest
    {
        /// <summary>
        /// Start date for the report.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the report.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Report type.
        /// </summary>
        public string ReportType { get; set; } = "summary";

        /// <summary>
        /// Report parameters.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }
    }

    /// <summary>
    /// Update monitoring config request model.
    /// </summary>
    public class UpdateMonitoringConfigRequest
    {
        /// <summary>
        /// Monitoring configuration.
        /// </summary>
        public Dictionary<string, object> Config { get; set; } = new();
    }
}