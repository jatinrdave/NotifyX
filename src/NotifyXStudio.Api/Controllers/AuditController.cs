using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for audit and compliance operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly ILogger<AuditController> _logger;
        private readonly IAuditService _auditService;

        public AuditController(ILogger<AuditController> logger, IAuditService auditService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Gets audit logs for a specific time range.
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? userId,
            [FromQuery] string? action,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var logs = await _auditService.GetAuditLogsAsync(start, end, userId, action, page, pageSize);
                var totalCount = await _auditService.GetAuditLogCountAsync(start, end, userId, action);

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
                _logger.LogError(ex, "Failed to get audit logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve audit logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets audit statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetAuditStats([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _auditService.GetAuditStatsAsync(start, end);

                return Ok(new
                {
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get audit stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve audit statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Exports audit logs.
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportAuditLogs([FromBody] ExportAuditLogsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Export request is required");
                }

                var logs = await _auditService.GetAuditLogsAsync(
                    request.StartDate,
                    request.EndDate,
                    request.UserId,
                    request.Action,
                    1,
                    int.MaxValue);

                var exportData = new
                {
                    exportDate = DateTime.UtcNow,
                    dateRange = new { request.StartDate, request.EndDate },
                    filters = new
                    {
                        request.UserId,
                        request.Action
                    },
                    logs
                };

                return Ok(exportData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export audit logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to export audit logs",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Export audit logs request model.
    /// </summary>
    public class ExportAuditLogsRequest
    {
        /// <summary>
        /// Start date for the export.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the export.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// User ID filter.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Action filter.
        /// </summary>
        public string? Action { get; set; }
    }
}