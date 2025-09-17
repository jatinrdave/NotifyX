using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for role operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly ILogger<RoleController> _logger;
        private readonly IRoleService _roleService;

        public RoleController(ILogger<RoleController> logger, IRoleService roleService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }

        /// <summary>
        /// Creates a role.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Role request is required");
                }

                var roleId = await _roleService.CreateRoleAsync(
                    request.Name,
                    request.Description,
                    string.Join(",", request.Permissions),
                    request.TenantId.ToString());

                return Ok(new
                {
                    roleId,
                    message = "Role created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create role: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create role",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets role information.
        /// </summary>
        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRole(string roleId)
        {
            try
            {
                var role = await _roleService.GetRoleAsync(roleId);

                if (role == null)
                {
                    return NotFound(new
                    {
                        error = "Role not found",
                        roleId
                    });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get role {RoleId}: {Message}", roleId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve role",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists roles for a tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListRoles(
            [FromQuery] Guid? tenantId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var roles = await _roleService.ListRolesAsync(tenantId?.ToString(), page, pageSize);
                var totalCount = await _roleService.GetRoleCountAsync(tenantId?.ToString());

                return Ok(new
                {
                    roles,
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
                _logger.LogError(ex, "Failed to list roles: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list roles",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a role.
        /// </summary>
        [HttpPut("{roleId}")]
        public async Task<IActionResult> UpdateRole(
            string roleId,
            [FromBody] UpdateRoleRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _roleService.UpdateRoleAsync(
                    roleId,
                    request.Name,
                    request.Description,
                    string.Join(",", request.Permissions));

                return Ok(new
                {
                    message = "Role updated successfully",
                    roleId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role {RoleId}: {Message}", roleId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update role",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a role.
        /// </summary>
        [HttpDelete("{roleId}")]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            try
            {
                await _roleService.DeleteRoleAsync(roleId);

                return Ok(new
                {
                    message = "Role deleted successfully",
                    roleId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete role {RoleId}: {Message}", roleId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete role",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets role permissions.
        /// </summary>
        [HttpGet("{roleId}/permissions")]
        public async Task<IActionResult> GetRolePermissions(string roleId)
        {
            try
            {
                var permissions = await _roleService.GetRolePermissionsAsync(roleId);

                return Ok(new
                {
                    roleId,
                    permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get role permissions for {RoleId}: {Message}", roleId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve role permissions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates role permissions.
        /// </summary>
        [HttpPut("{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(
            string roleId,
            [FromBody] UpdateRolePermissionsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Permissions request is required");
                }

                await _roleService.UpdateRolePermissionsAsync(roleId, request.Permissions);

                return Ok(new
                {
                    message = "Role permissions updated successfully",
                    roleId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role permissions for {RoleId}: {Message}", roleId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update role permissions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available permissions.
        /// </summary>
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAvailablePermissions()
        {
            try
            {
                var permissions = await _roleService.GetAvailablePermissionsAsync();

                return Ok(new
                {
                    permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available permissions: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve available permissions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets role users.
        /// </summary>
        [HttpGet("{roleId}/users")]
        public async Task<IActionResult> GetRoleUsers(
            string roleId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var users = await _roleService.GetRoleUsersAsync(roleId, page, pageSize);
                var totalCount = await _roleService.GetRoleUserCountAsync(roleId);

                return Ok(new
                {
                    roleId,
                    users,
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
                _logger.LogError(ex, "Failed to get role users for {RoleId}: {Message}", roleId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve role users",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create role request model.
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Role name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Role description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Role permissions.
        /// </summary>
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update role request model.
    /// </summary>
    public class UpdateRoleRequest
    {
        /// <summary>
        /// Role name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Role description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Role permissions.
        /// </summary>
        public List<string>? Permissions { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update role permissions request model.
    /// </summary>
    public class UpdateRolePermissionsRequest
    {
        /// <summary>
        /// Role permissions.
        /// </summary>
        public List<string> Permissions { get; set; } = new();
    }
}