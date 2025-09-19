using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for integration operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class IntegrationController : ControllerBase
    {
        private readonly ILogger<IntegrationController> _logger;
        private readonly IIntegrationService _integrationService;

        public IntegrationController(ILogger<IntegrationController> logger, IIntegrationService integrationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
        }

        /// <summary>
        /// Creates an integration.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateIntegration([FromBody] CreateIntegrationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Integration request is required");
                }

                var integrationId = await _integrationService.CreateIntegrationAsync(
                    request.Name,
                    null,
                    request.Type,
                    null,
                    request.TenantId.ToString());

                return Ok(new
                {
                    integrationId,
                    message = "Integration created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create integration: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create integration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets integration information.
        /// </summary>
        [HttpGet("{integrationId}")]
        public async Task<IActionResult> GetIntegration(string integrationId)
        {
            try
            {
                var integration = await _integrationService.GetIntegrationAsync(integrationId);

                if (integration == null)
                {
                    return NotFound(new
                    {
                        error = "Integration not found",
                        integrationId
                    });
                }

                return Ok(integration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get integration {IntegrationId}: {Message}", integrationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve integration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists integrations for a tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListIntegrations(
            [FromQuery] Guid? tenantId,
            [FromQuery] string? type,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var integrations = await _integrationService.ListIntegrationsAsync(tenantId, type, page, pageSize);
                var totalCount = await _integrationService.GetIntegrationCountAsync(tenantId, type);

                return Ok(new
                {
                    integrations,
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
                _logger.LogError(ex, "Failed to list integrations: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list integrations",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an integration.
        /// </summary>
        [HttpPut("{integrationId}")]
        public async Task<IActionResult> UpdateIntegration(
            string integrationId,
            [FromBody] UpdateIntegrationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _integrationService.UpdateIntegrationAsync(
                    integrationId,
                    request.Name,
                    null,
                    null,
                    null);

                return Ok(new
                {
                    message = "Integration updated successfully",
                    integrationId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update integration {IntegrationId}: {Message}", integrationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update integration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an integration.
        /// </summary>
        [HttpDelete("{integrationId}")]
        public async Task<IActionResult> DeleteIntegration(string integrationId)
        {
            try
            {
                await _integrationService.DeleteIntegrationAsync(integrationId);

                return Ok(new
                {
                    message = "Integration deleted successfully",
                    integrationId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete integration {IntegrationId}: {Message}", integrationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete integration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Tests an integration.
        /// </summary>
        [HttpPost("{integrationId}/test")]
        public async Task<IActionResult> TestIntegration(string integrationId)
        {
            try
            {
                var result = await _integrationService.TestIntegrationAsync(integrationId);

                return Ok(new
                {
                    message = "Integration test completed",
                    integrationId,
                    result,
                    testedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test integration {IntegrationId}: {Message}", integrationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to test integration",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets integration logs.
        /// </summary>
        [HttpGet("{integrationId}/logs")]
        public async Task<IActionResult> GetIntegrationLogs(
            string integrationId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var logs = await _integrationService.GetIntegrationLogsAsync(integrationId, "info", page, pageSize);
                var totalCount = await _integrationService.GetIntegrationLogCountAsync(integrationId, "info");

                return Ok(new
                {
                    integrationId,
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
                _logger.LogError(ex, "Failed to get integration logs for {IntegrationId}: {Message}", integrationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve integration logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets integration statistics.
        /// </summary>
        [HttpGet("{integrationId}/stats")]
        public async Task<IActionResult> GetIntegrationStats(
            string integrationId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _integrationService.GetIntegrationStatsAsync(integrationId, "default");

                return Ok(new
                {
                    integrationId,
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get integration stats for {IntegrationId}: {Message}", integrationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve integration statistics",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create integration request model.
    /// </summary>
    public class CreateIntegrationRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Integration name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Integration type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Integration configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update integration request model.
    /// </summary>
    public class UpdateIntegrationRequest
    {
        /// <summary>
        /// Integration name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Integration configuration.
        /// </summary>
        public Dictionary<string, object>? Configuration { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}