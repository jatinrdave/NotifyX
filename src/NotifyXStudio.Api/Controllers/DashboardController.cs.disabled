using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for dashboard operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _dashboardService;

        public DashboardController(ILogger<DashboardController> logger, IDashboardService dashboardService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        /// <summary>
        /// Gets dashboard data for a tenant.
        /// </summary>
        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetDashboardData(Guid tenantId)
        {
            try
            {
                var dashboardData = await _dashboardService.GetDashboardDataAsync(tenantId);

                return Ok(new
                {
                    tenantId,
                    dashboardData,
                    generatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard data for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve dashboard data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets dashboard widgets for a tenant.
        /// </summary>
        [HttpGet("{tenantId}/widgets")]
        public async Task<IActionResult> GetDashboardWidgets(Guid tenantId)
        {
            try
            {
                var widgets = await _dashboardService.GetDashboardWidgetsAsync(tenantId);

                return Ok(new
                {
                    tenantId,
                    widgets
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard widgets for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve dashboard widgets",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates dashboard layout for a tenant.
        /// </summary>
        [HttpPut("{tenantId}/layout")]
        public async Task<IActionResult> UpdateDashboardLayout(
            Guid tenantId,
            [FromBody] UpdateDashboardLayoutRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Layout request is required");
                }

                await _dashboardService.UpdateDashboardLayoutAsync(tenantId, request.Layout);

                return Ok(new
                {
                    message = "Dashboard layout updated successfully",
                    tenantId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update dashboard layout for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update dashboard layout",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets dashboard metrics for a tenant.
        /// </summary>
        [HttpGet("{tenantId}/metrics")]
        public async Task<IActionResult> GetDashboardMetrics(
            Guid tenantId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var metrics = await _dashboardService.GetDashboardMetricsAsync(tenantId, start, end);

                return Ok(new
                {
                    tenantId,
                    dateRange = new { start, end },
                    metrics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard metrics for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve dashboard metrics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets dashboard alerts for a tenant.
        /// </summary>
        [HttpGet("{tenantId}/alerts")]
        public async Task<IActionResult> GetDashboardAlerts(Guid tenantId)
        {
            try
            {
                var alerts = await _dashboardService.GetDashboardAlertsAsync(tenantId);

                return Ok(new
                {
                    tenantId,
                    alerts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get dashboard alerts for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve dashboard alerts",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Dismisses a dashboard alert.
        /// </summary>
        [HttpPost("{tenantId}/alerts/{alertId}/dismiss")]
        public async Task<IActionResult> DismissDashboardAlert(Guid tenantId, string alertId)
        {
            try
            {
                await _dashboardService.DismissDashboardAlertAsync(tenantId, alertId);

                return Ok(new
                {
                    message = "Alert dismissed successfully",
                    tenantId,
                    alertId,
                    dismissedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dismiss dashboard alert {AlertId} for tenant {TenantId}: {Message}", alertId, tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to dismiss dashboard alert",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Update dashboard layout request model.
    /// </summary>
    public class UpdateDashboardLayoutRequest
    {
        /// <summary>
        /// Dashboard layout configuration.
        /// </summary>
        public Dictionary<string, object> Layout { get; set; } = new();
    }
}