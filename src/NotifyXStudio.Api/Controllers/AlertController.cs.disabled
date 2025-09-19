using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for alert operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly ILogger<AlertController> _logger;
        private readonly IAlertService _alertService;

        public AlertController(ILogger<AlertController> logger, IAlertService alertService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
        }

        /// <summary>
        /// Creates an alert.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAlert([FromBody] CreateAlertRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Alert request is required");
                }

                var alertId = await _alertService.CreateAlertAsync(
                    request.TenantId,
                    request.Name,
                    request.Description,
                    request.Severity,
                    request.Conditions,
                    request.Actions,
                    request.Enabled);

                return Ok(new
                {
                    alertId,
                    message = "Alert created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create alert: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create alert",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets alert information.
        /// </summary>
        [HttpGet("{alertId}")]
        public async Task<IActionResult> GetAlert(string alertId)
        {
            try
            {
                var alert = await _alertService.GetAlertAsync(alertId);

                if (alert == null)
                {
                    return NotFound(new
                    {
                        error = "Alert not found",
                        alertId
                    });
                }

                return Ok(alert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get alert {AlertId}: {Message}", alertId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve alert",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists alerts for a tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListAlerts(
            [FromQuery] Guid? tenantId,
            [FromQuery] string? severity,
            [FromQuery] bool? enabled,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var alerts = await _alertService.ListAlertsAsync(tenantId, severity, enabled, page, pageSize);
                var totalCount = await _alertService.GetAlertCountAsync(tenantId, severity, enabled);

                return Ok(new
                {
                    alerts,
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
                _logger.LogError(ex, "Failed to list alerts: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list alerts",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an alert.
        /// </summary>
        [HttpPut("{alertId}")]
        public async Task<IActionResult> UpdateAlert(
            string alertId,
            [FromBody] UpdateAlertRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _alertService.UpdateAlertAsync(
                    alertId,
                    request.Name,
                    request.Description,
                    request.Severity,
                    request.Conditions,
                    request.Actions,
                    request.Enabled);

                return Ok(new
                {
                    message = "Alert updated successfully",
                    alertId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update alert {AlertId}: {Message}", alertId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update alert",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an alert.
        /// </summary>
        [HttpDelete("{alertId}")]
        public async Task<IActionResult> DeleteAlert(string alertId)
        {
            try
            {
                await _alertService.DeleteAlertAsync(alertId);

                return Ok(new
                {
                    message = "Alert deleted successfully",
                    alertId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete alert {AlertId}: {Message}", alertId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete alert",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Tests an alert.
        /// </summary>
        [HttpPost("{alertId}/test")]
        public async Task<IActionResult> TestAlert(string alertId)
        {
            try
            {
                var result = await _alertService.TestAlertAsync(alertId);

                return Ok(new
                {
                    message = "Alert test completed",
                    alertId,
                    result,
                    testedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test alert {AlertId}: {Message}", alertId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to test alert",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets alert history.
        /// </summary>
        [HttpGet("{alertId}/history")]
        public async Task<IActionResult> GetAlertHistory(
            string alertId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var history = await _alertService.GetAlertHistoryAsync(alertId, start, end, page, pageSize);
                var totalCount = await _alertService.GetAlertHistoryCountAsync(alertId, start, end);

                return Ok(new
                {
                    alertId,
                    history,
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
                _logger.LogError(ex, "Failed to get alert history for {AlertId}: {Message}", alertId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve alert history",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets alert statistics.
        /// </summary>
        [HttpGet("{alertId}/stats")]
        public async Task<IActionResult> GetAlertStats(
            string alertId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _alertService.GetAlertStatsAsync(alertId, start, end);

                return Ok(new
                {
                    alertId,
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get alert stats for {AlertId}: {Message}", alertId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve alert statistics",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create alert request model.
    /// </summary>
    public class CreateAlertRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Alert name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Alert description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Alert severity.
        /// </summary>
        public string Severity { get; set; } = "medium";

        /// <summary>
        /// Alert conditions.
        /// </summary>
        public List<AlertCondition> Conditions { get; set; } = new();

        /// <summary>
        /// Alert actions.
        /// </summary>
        public List<AlertAction> Actions { get; set; } = new();

        /// <summary>
        /// Whether the alert is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Update alert request model.
    /// </summary>
    public class UpdateAlertRequest
    {
        /// <summary>
        /// Alert name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Alert description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Alert severity.
        /// </summary>
        public string? Severity { get; set; }

        /// <summary>
        /// Alert conditions.
        /// </summary>
        public List<AlertCondition>? Conditions { get; set; }

        /// <summary>
        /// Alert actions.
        /// </summary>
        public List<AlertAction>? Actions { get; set; }

        /// <summary>
        /// Whether the alert is enabled.
        /// </summary>
        public bool? Enabled { get; set; }
    }

    /// <summary>
    /// Alert condition model.
    /// </summary>
    public class AlertCondition
    {
        /// <summary>
        /// Condition field.
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Condition operator.
        /// </summary>
        public string Operator { get; set; } = string.Empty;

        /// <summary>
        /// Condition value.
        /// </summary>
        public object Value { get; set; } = new();
    }

    /// <summary>
    /// Alert action model.
    /// </summary>
    public class AlertAction
    {
        /// <summary>
        /// Action type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Action configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new();
    }
}