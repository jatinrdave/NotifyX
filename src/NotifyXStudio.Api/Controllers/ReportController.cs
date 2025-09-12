using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for reporting operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IReportService _reportService;

        public ReportController(ILogger<ReportController> logger, IReportService reportService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        }

        /// <summary>
        /// Generates a report.
        /// </summary>
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport([FromBody] GenerateReportRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Report request is required");
                }

                var reportId = await _reportService.GenerateReportAsync(
                    request.TenantId,
                    request.ReportType,
                    request.StartDate,
                    request.EndDate,
                    request.Parameters);

                return Ok(new
                {
                    reportId,
                    message = "Report generation initiated successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate report: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to generate report",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets report information.
        /// </summary>
        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReport(string reportId)
        {
            try
            {
                var report = await _reportService.GetReportAsync(reportId);

                if (report == null)
                {
                    return NotFound(new
                    {
                        error = "Report not found",
                        reportId
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get report {ReportId}: {Message}", reportId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve report",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists reports for a tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListReports(
            [FromQuery] Guid? tenantId,
            [FromQuery] string? reportType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var reports = await _reportService.ListReportsAsync(tenantId, reportType, page, pageSize);
                var totalCount = await _reportService.GetReportCountAsync(tenantId, reportType);

                return Ok(new
                {
                    reports,
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
                _logger.LogError(ex, "Failed to list reports: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list reports",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Downloads a report.
        /// </summary>
        [HttpGet("{reportId}/download")]
        public async Task<IActionResult> DownloadReport(string reportId)
        {
            try
            {
                var reportStream = await _reportService.DownloadReportAsync(reportId);

                if (reportStream == null)
                {
                    return NotFound(new
                    {
                        error = "Report not found",
                        reportId
                    });
                }

                return File(reportStream, "application/pdf", $"report_{reportId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download report {ReportId}: {Message}", reportId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to download report",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a report.
        /// </summary>
        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteReport(string reportId)
        {
            try
            {
                await _reportService.DeleteReportAsync(reportId);

                return Ok(new
                {
                    message = "Report deleted successfully",
                    reportId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete report {ReportId}: {Message}", reportId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete report",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available report types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetReportTypes()
        {
            try
            {
                var reportTypes = await _reportService.GetReportTypesAsync();

                return Ok(new
                {
                    reportTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get report types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve report types",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets report templates.
        /// </summary>
        [HttpGet("templates")]
        public async Task<IActionResult> GetReportTemplates()
        {
            try
            {
                var templates = await _reportService.GetReportTemplatesAsync();

                return Ok(new
                {
                    templates
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get report templates: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve report templates",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Generate report request model.
    /// </summary>
    public class GenerateReportRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Report type.
        /// </summary>
        public string ReportType { get; set; } = string.Empty;

        /// <summary>
        /// Start date for the report.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the report.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Report parameters.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }
    }
}