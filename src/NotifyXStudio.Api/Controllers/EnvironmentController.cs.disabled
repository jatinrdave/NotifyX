using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for environment operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EnvironmentController : ControllerBase
    {
        private readonly ILogger<EnvironmentController> _logger;
        private readonly IEnvironmentService _environmentService;

        public EnvironmentController(ILogger<EnvironmentController> logger, IEnvironmentService environmentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environmentService = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        }

        /// <summary>
        /// Creates an environment.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEnvironment([FromBody] CreateEnvironmentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Environment request is required");
                }

                var environmentId = await _environmentService.CreateEnvironmentAsync(
                    request.Name,
                    request.Description,
                    request.EnvironmentType,
                    request.Configuration,
                    request.Metadata);

                return Ok(new
                {
                    environmentId,
                    message = "Environment created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create environment: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create environment",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets environment information.
        /// </summary>
        [HttpGet("{environmentId}")]
        public async Task<IActionResult> GetEnvironment(string environmentId)
        {
            try
            {
                var environment = await _environmentService.GetEnvironmentAsync(environmentId);

                if (environment == null)
                {
                    return NotFound(new
                    {
                        error = "Environment not found",
                        environmentId
                    });
                }

                return Ok(environment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get environment {EnvironmentId}: {Message}", environmentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve environment",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists environments.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListEnvironments(
            [FromQuery] string? environmentType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var environments = await _environmentService.ListEnvironmentsAsync(environmentType, page, pageSize);
                var totalCount = await _environmentService.GetEnvironmentCountAsync(environmentType);

                return Ok(new
                {
                    environments,
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
                _logger.LogError(ex, "Failed to list environments: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list environments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an environment.
        /// </summary>
        [HttpPut("{environmentId}")]
        public async Task<IActionResult> UpdateEnvironment(
            string environmentId,
            [FromBody] UpdateEnvironmentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _environmentService.UpdateEnvironmentAsync(
                    environmentId,
                    request.Name,
                    request.Description,
                    request.EnvironmentType,
                    request.Configuration,
                    request.Metadata);

                return Ok(new
                {
                    message = "Environment updated successfully",
                    environmentId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update environment {EnvironmentId}: {Message}", environmentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update environment",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an environment.
        /// </summary>
        [HttpDelete("{environmentId}")]
        public async Task<IActionResult> DeleteEnvironment(string environmentId)
        {
            try
            {
                await _environmentService.DeleteEnvironmentAsync(environmentId);

                return Ok(new
                {
                    message = "Environment deleted successfully",
                    environmentId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete environment {EnvironmentId}: {Message}", environmentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete environment",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets environment status.
        /// </summary>
        [HttpGet("{environmentId}/status")]
        public async Task<IActionResult> GetEnvironmentStatus(string environmentId)
        {
            try
            {
                var status = await _environmentService.GetEnvironmentStatusAsync(environmentId);

                return Ok(new
                {
                    environmentId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get environment status for {EnvironmentId}: {Message}", environmentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve environment status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets environment resources.
        /// </summary>
        [HttpGet("{environmentId}/resources")]
        public async Task<IActionResult> GetEnvironmentResources(string environmentId)
        {
            try
            {
                var resources = await _environmentService.GetEnvironmentResourcesAsync(environmentId);

                return Ok(new
                {
                    environmentId,
                    resources
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get environment resources for {EnvironmentId}: {Message}", environmentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve environment resources",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets environment deployments.
        /// </summary>
        [HttpGet("{environmentId}/deployments")]
        public async Task<IActionResult> GetEnvironmentDeployments(string environmentId)
        {
            try
            {
                var deployments = await _environmentService.GetEnvironmentDeploymentsAsync(environmentId);

                return Ok(new
                {
                    environmentId,
                    deployments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get environment deployments for {EnvironmentId}: {Message}", environmentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve environment deployments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets environment statistics.
        /// </summary>
        [HttpGet("{environmentId}/stats")]
        public async Task<IActionResult> GetEnvironmentStats(string environmentId)
        {
            try
            {
                var stats = await _environmentService.GetEnvironmentStatsAsync(environmentId);

                return Ok(new
                {
                    environmentId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get environment stats for {EnvironmentId}: {Message}", environmentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve environment statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available environment types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetEnvironmentTypes()
        {
            try
            {
                var environmentTypes = await _environmentService.GetEnvironmentTypesAsync();

                return Ok(new
                {
                    environmentTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get environment types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve environment types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create environment request model.
    /// </summary>
    public class CreateEnvironmentRequest
    {
        /// <summary>
        /// Environment name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Environment description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Environment type.
        /// </summary>
        public string EnvironmentType { get; set; } = "development";

        /// <summary>
        /// Environment configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update environment request model.
    /// </summary>
    public class UpdateEnvironmentRequest
    {
        /// <summary>
        /// Environment name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Environment description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Environment type.
        /// </summary>
        public string? EnvironmentType { get; set; }

        /// <summary>
        /// Environment configuration.
        /// </summary>
        public Dictionary<string, object>? Configuration { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}