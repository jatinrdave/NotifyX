using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for user operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        /// <summary>
        /// Creates a user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("User request is required");
                }

                var user = await _userService.CreateUserAsync(
                    request.Email,
                    request.Name, // firstName
                    "", // lastName - not provided in request
                    request.TenantId.ToString(),
                    request.Metadata,
                    cancellationToken);

                return Ok(new
                {
                    userId = user.Id,
                    message = "User created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create user",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets user information.
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            try
            {
                var user = await _userService.GetUserAsync(userId);

                if (user == null)
                {
                    return NotFound(new
                    {
                        error = "User not found",
                        userId
                    });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user {UserId}: {Message}", userId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve user",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists users for a tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListUsers(
            [FromQuery] Guid? tenantId,
            [FromQuery] string? role,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _userService.ListUsersAsync(tenantId?.ToString(), role, page, pageSize, cancellationToken);
                var totalCount = await _userService.GetUserCountAsync(tenantId?.ToString(), role, cancellationToken);

                return Ok(new
                {
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
                _logger.LogError(ex, "Failed to list users: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list users",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(
            string userId,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _userService.UpdateUserAsync(
                    userId,
                    request.Name, // firstName
                    "", // lastName - not provided in request
                    null, // email - not provided in request
                    request.Metadata,
                    cancellationToken);

                return Ok(new
                {
                    message = "User updated successfully",
                    userId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}: {Message}", userId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update user",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                await _userService.DeleteUserAsync(userId);

                return Ok(new
                {
                    message = "User deleted successfully",
                    userId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}: {Message}", userId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete user",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets user permissions.
        /// </summary>
        [HttpGet("{userId}/permissions")]
        public async Task<IActionResult> GetUserPermissions(string userId)
        {
            try
            {
                var permissions = await _userService.GetUserPermissionsAsync(userId);

                return Ok(new
                {
                    userId,
                    permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user permissions for {UserId}: {Message}", userId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve user permissions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates user permissions.
        /// </summary>
        [HttpPut("{userId}/permissions")]
        public async Task<IActionResult> UpdateUserPermissions(
            string userId,
            [FromBody] UpdateUserPermissionsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Permissions request is required");
                }

                await _userService.UpdateUserPermissionsAsync(userId, request.Permissions);

                return Ok(new
                {
                    message = "User permissions updated successfully",
                    userId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user permissions for {UserId}: {Message}", userId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update user permissions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets user activity.
        /// </summary>
        [HttpGet("{userId}/activity")]
        public async Task<IActionResult> GetUserActivity(
            string userId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var activity = await _userService.GetUserActivityAsync(userId, start, end, page, pageSize);
                var totalCount = await _userService.GetUserActivityCountAsync(userId, start, end);

                return Ok(new
                {
                    userId,
                    activity,
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
                _logger.LogError(ex, "Failed to get user activity for {UserId}: {Message}", userId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve user activity",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create user request model.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// User email.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User role.
        /// </summary>
        public string Role { get; set; } = "user";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update user request model.
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// User role.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update user permissions request model.
    /// </summary>
    public class UpdateUserPermissionsRequest
    {
        /// <summary>
        /// User permissions.
        /// </summary>
        public List<string> Permissions { get; set; } = new();
    }
}