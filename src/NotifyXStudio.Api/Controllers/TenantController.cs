using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for tenant operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly ILogger<TenantController> _logger;
        private readonly ITenantService _tenantService;

        public TenantController(ILogger<TenantController> logger, ITenantService tenantService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        }

        /// <summary>
        /// Creates a tenant.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTenant([FromBody] CreateTenantRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Tenant request is required");
                }

                var tenantId = await _tenantService.CreateTenantAsync(
                    request.Name,
                    request.Description,
                    request.Settings,
                    request.Metadata);

                return Ok(new
                {
                    tenantId,
                    message = "Tenant created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tenant: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create tenant",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tenant information.
        /// </summary>
        [HttpGet("{tenantId}")]
        public async Task<IActionResult> GetTenant(Guid tenantId)
        {
            try
            {
                var tenant = await _tenantService.GetTenantAsync(tenantId);

                if (tenant == null)
                {
                    return NotFound(new
                    {
                        error = "Tenant not found",
                        tenantId
                    });
                }

                return Ok(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tenant",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists tenants.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListTenants(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var tenants = await _tenantService.ListTenantsAsync(page, pageSize);
                var totalCount = await _tenantService.GetTenantCountAsync();

                return Ok(new
                {
                    tenants,
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
                _logger.LogError(ex, "Failed to list tenants: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list tenants",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a tenant.
        /// </summary>
        [HttpPut("{tenantId}")]
        public async Task<IActionResult> UpdateTenant(
            Guid tenantId,
            [FromBody] UpdateTenantRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _tenantService.UpdateTenantAsync(
                    tenantId,
                    request.Name,
                    request.Description,
                    request.Settings,
                    request.Metadata);

                return Ok(new
                {
                    message = "Tenant updated successfully",
                    tenantId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update tenant",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a tenant.
        /// </summary>
        [HttpDelete("{tenantId}")]
        public async Task<IActionResult> DeleteTenant(Guid tenantId)
        {
            try
            {
                await _tenantService.DeleteTenantAsync(tenantId);

                return Ok(new
                {
                    message = "Tenant deleted successfully",
                    tenantId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete tenant {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete tenant",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tenant settings.
        /// </summary>
        [HttpGet("{tenantId}/settings")]
        public async Task<IActionResult> GetTenantSettings(Guid tenantId)
        {
            try
            {
                var settings = await _tenantService.GetTenantSettingsAsync(tenantId);

                return Ok(new
                {
                    tenantId,
                    settings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tenant settings for {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tenant settings",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates tenant settings.
        /// </summary>
        [HttpPut("{tenantId}/settings")]
        public async Task<IActionResult> UpdateTenantSettings(
            Guid tenantId,
            [FromBody] UpdateTenantSettingsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Settings request is required");
                }

                await _tenantService.UpdateTenantSettingsAsync(tenantId, request.Settings);

                return Ok(new
                {
                    message = "Tenant settings updated successfully",
                    tenantId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update tenant settings for {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update tenant settings",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tenant usage statistics.
        /// </summary>
        [HttpGet("{tenantId}/usage")]
        public async Task<IActionResult> GetTenantUsage(
            Guid tenantId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var usage = await _tenantService.GetTenantUsageAsync(tenantId?.ToString() ?? "default");

                return Ok(new
                {
                    tenantId,
                    dateRange = new { start, end },
                    usage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tenant usage for {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tenant usage",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tenant limits.
        /// </summary>
        [HttpGet("{tenantId}/limits")]
        public async Task<IActionResult> GetTenantLimits(Guid tenantId)
        {
            try
            {
                var limits = await _tenantService.GetTenantLimitsAsync(tenantId?.ToString() ?? "default");

                return Ok(new
                {
                    tenantId,
                    limits
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tenant limits for {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tenant limits",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates tenant limits.
        /// </summary>
        [HttpPut("{tenantId}/limits")]
        public async Task<IActionResult> UpdateTenantLimits(
            Guid tenantId,
            [FromBody] UpdateTenantLimitsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Limits request is required");
                }

                await _tenantService.UpdateTenantLimitsAsync(new Tenant());

                return Ok(new
                {
                    message = "Tenant limits updated successfully",
                    tenantId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update tenant limits for {TenantId}: {Message}", tenantId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update tenant limits",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create tenant request model.
    /// </summary>
    public class CreateTenantRequest
    {
        /// <summary>
        /// Tenant name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tenant description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tenant settings.
        /// </summary>
        public Dictionary<string, object> Settings { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update tenant request model.
    /// </summary>
    public class UpdateTenantRequest
    {
        /// <summary>
        /// Tenant name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Tenant description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Tenant settings.
        /// </summary>
        public Dictionary<string, object>? Settings { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update tenant settings request model.
    /// </summary>
    public class UpdateTenantSettingsRequest
    {
        /// <summary>
        /// Tenant settings.
        /// </summary>
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    /// <summary>
    /// Update tenant limits request model.
    /// </summary>
    public class UpdateTenantLimitsRequest
    {
        /// <summary>
        /// Tenant limits.
        /// </summary>
        public Dictionary<string, object> Limits { get; set; } = new();
    }
}