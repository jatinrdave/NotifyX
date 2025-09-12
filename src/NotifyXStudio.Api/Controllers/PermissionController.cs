using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for permission operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly ILogger<PermissionController> _logger;
        private readonly IPermissionService _permissionService;

        public PermissionController(ILogger<PermissionController> logger, IPermissionService permissionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }

        /// <summary>
        /// Creates a permission.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Permission request is required");
                }

                var permissionId = await _permissionService.CreatePermissionAsync(
                    request.TenantId,
                    request.Name,
                    request.Description,
                    request.Resource,
                    request.Action,
                    request.Metadata);

                return Ok(new
                {
                    permissionId,
                    message = "Permission created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create permission: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create permission",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets permission information.
        /// </summary>
        [HttpGet("{permissionId}")]
        public async Task<IActionResult> GetPermission(string permissionId)
        {
            try
            {
                var permission = await _permissionService.GetPermissionAsync(permissionId);

                if (permission == null)
                {
                    return NotFound(new
                    {
                        error = "Permission not found",
                        permissionId
                    });
                }

                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get permission {PermissionId}: {Message}", permissionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve permission",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists permissions for a tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListPermissions(
            [FromQuery] Guid? tenantId,
            [FromQuery] string? resource,
            [FromQuery] string? action,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var permissions = await _permissionService.ListPermissionsAsync(tenantId, resource, action, page, pageSize);
                var totalCount = await _permissionService.GetPermissionCountAsync(tenantId, resource, action);

                return Ok(new
                {
                    permissions,
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
                _logger.LogError(ex, "Failed to list permissions: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list permissions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a permission.
        /// </summary>
        [HttpPut("{permissionId}")]
        public async Task<IActionResult> UpdatePermission(
            string permissionId,
            [FromBody] UpdatePermissionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _permissionService.UpdatePermissionAsync(
                    permissionId,
                    request.Name,
                    request.Description,
                    request.Resource,
                    request.Action,
                    request.Metadata);

                return Ok(new
                {
                    message = "Permission updated successfully",
                    permissionId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update permission {PermissionId}: {Message}", permissionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update permission",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a permission.
        /// </summary>
        [HttpDelete("{permissionId}")]
        public async Task<IActionResult> DeletePermission(string permissionId)
        {
            try
            {
                await _permissionService.DeletePermissionAsync(permissionId);

                return Ok(new
                {
                    message = "Permission deleted successfully",
                    permissionId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete permission {PermissionId}: {Message}", permissionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete permission",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available resources.
        /// </summary>
        [HttpGet("resources")]
        public async Task<IActionResult> GetAvailableResources()
        {
            try
            {
                var resources = await _permissionService.GetAvailableResourcesAsync();

                return Ok(new
                {
                    resources
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available resources: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve available resources",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available actions.
        /// </summary>
        [HttpGet("actions")]
        public async Task<IActionResult> GetAvailableActions()
        {
            try
            {
                var actions = await _permissionService.GetAvailableActionsAsync();

                return Ok(new
                {
                    actions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available actions: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve available actions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets permission matrix.
        /// </summary>
        [HttpGet("matrix")]
        public async Task<IActionResult> GetPermissionMatrix([FromQuery] Guid? tenantId)
        {
            try
            {
                var matrix = await _permissionService.GetPermissionMatrixAsync(tenantId);

                return Ok(new
                {
                    tenantId,
                    matrix
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get permission matrix: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve permission matrix",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create permission request model.
    /// </summary>
    public class CreatePermissionRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Permission name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Permission description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Resource name.
        /// </summary>
        public string Resource { get; set; } = string.Empty;

        /// <summary>
        /// Action name.
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update permission request model.
    /// </summary>
    public class UpdatePermissionRequest
    {
        /// <summary>
        /// Permission name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Permission description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Resource name.
        /// </summary>
        public string? Resource { get; set; }

        /// <summary>
        /// Action name.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}