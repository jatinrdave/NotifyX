using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for compliance and regulatory operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ComplianceController : ControllerBase
    {
        private readonly ILogger<ComplianceController> _logger;
        private readonly IComplianceService _complianceService;

        public ComplianceController(ILogger<ComplianceController> logger, IComplianceService complianceService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _complianceService = complianceService ?? throw new ArgumentNullException(nameof(complianceService));
        }

        /// <summary>
        /// Gets compliance status for a tenant.
        /// </summary>
        [HttpGet("status/{tenantId}")]
        public async Task<IActionResult> GetComplianceStatus(Guid tenantId)
        {
            try
            {
                var status = await _complianceService.GetComplianceStatusAsync(tenantId);

                return Ok(new
                {
                    tenantId,
                    status,
                    checkedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get compliance status for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve compliance status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets compliance violations for a tenant.
        /// </summary>
        [HttpGet("violations/{tenantId}")]
        public async Task<IActionResult> GetComplianceViolations(
            Guid tenantId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? severity,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var violations = await _complianceService.GetComplianceViolationsAsync(
                    tenantId, start, end, severity, page, pageSize);

                var totalCount = await _complianceService.GetComplianceViolationCountAsync(
                    tenantId, start, end, severity);

                return Ok(new
                {
                    tenantId,
                    violations,
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
                _logger.LogError(ex, "Failed to get compliance violations for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve compliance violations",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets compliance metrics for a tenant.
        /// </summary>
        [HttpGet("metrics/{tenantId}")]
        public async Task<IActionResult> GetComplianceMetrics(
            Guid tenantId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var metrics = await _complianceService.GetComplianceMetricsAsync(tenantId, start, end);

                return Ok(new
                {
                    tenantId,
                    dateRange = new { start, end },
                    metrics
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get compliance metrics for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve compliance metrics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Generates a compliance report for a tenant.
        /// </summary>
        [HttpPost("report/{tenantId}")]
        public async Task<IActionResult> GenerateComplianceReport(
            Guid tenantId,
            [FromBody] ComplianceReportRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Report request is required");
                }

                var report = await _complianceService.GenerateComplianceReportAsync(
                    tenantId, request.StartDate, request.EndDate, request.ReportType);

                return Ok(new
                {
                    tenantId,
                    report,
                    generatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate compliance report for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to generate compliance report",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets compliance policies for a tenant.
        /// </summary>
        [HttpGet("policies/{tenantId}")]
        public async Task<IActionResult> GetCompliancePolicies(Guid tenantId)
        {
            try
            {
                var policies = await _complianceService.GetCompliancePoliciesAsync(tenantId);

                return Ok(new
                {
                    tenantId,
                    policies
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get compliance policies for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve compliance policies",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates compliance policies for a tenant.
        /// </summary>
        [HttpPut("policies/{tenantId}")]
        public async Task<IActionResult> UpdateCompliancePolicies(
            Guid tenantId,
            [FromBody] UpdateCompliancePoliciesRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Policies request is required");
                }

                await _complianceService.UpdateCompliancePoliciesAsync(tenantId, request.Policies);

                return Ok(new
                {
                    message = "Compliance policies updated successfully",
                    tenantId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update compliance policies for tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update compliance policies",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Compliance report request model.
    /// </summary>
    public class ComplianceReportRequest
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
        /// Type of report to generate.
        /// </summary>
        public string ReportType { get; set; } = "summary";
    }

    /// <summary>
    /// Update compliance policies request model.
    /// </summary>
    public class UpdateCompliancePoliciesRequest
    {
        /// <summary>
        /// Compliance policies to update.
        /// </summary>
        public List<CompliancePolicy> Policies { get; set; } = new();
    }

    /// <summary>
    /// Compliance policy model.
    /// </summary>
    public class CompliancePolicy
    {
        /// <summary>
        /// Policy ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Policy name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Policy description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Policy severity.
        /// </summary>
        public string Severity { get; set; } = "medium";

        /// <summary>
        /// Whether the policy is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Policy configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new();
    }
}