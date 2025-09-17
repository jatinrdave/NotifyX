using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for deployment operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DeployController : ControllerBase
    {
        private readonly ILogger<DeployController> _logger;
        private readonly IDeployService _deployService;

        public DeployController(ILogger<DeployController> logger, IDeployService deployService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deployService = deployService ?? throw new ArgumentNullException(nameof(deployService));
        }

        /// <summary>
        /// Initiates a deployment.
        /// </summary>
        [HttpPost("deploy")]
        public async Task<IActionResult> Deploy([FromBody] DeployRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Deployment request is required");
                }

                var deploymentId = await _deployService.DeployAsync(
                    request.Environment,
                    request.Version,
                    request.Components,
                    request.Parameters);

                return Ok(new
                {
                    deploymentId,
                    message = "Deployment initiated successfully",
                    startedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deploy: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to initiate deployment",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets deployment status.
        /// </summary>
        [HttpGet("deployments/{deploymentId}")]
        public async Task<IActionResult> GetDeploymentStatus(string deploymentId)
        {
            try
            {
                var deployment = await _deployService.GetDeploymentStatusAsync(deploymentId);

                if (deployment == null)
                {
                    return NotFound(new
                    {
                        error = "Deployment not found",
                        deploymentId
                    });
                }

                return Ok(deployment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get deployment status for {DeploymentId}: {Message}", deploymentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve deployment status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists deployments.
        /// </summary>
        [HttpGet("deployments")]
        public async Task<IActionResult> ListDeployments(
            [FromQuery] string? environment,
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var deployments = await _deployService.ListDeploymentsAsync(environment, status, start, end, page, pageSize);
                var totalCount = await _deployService.GetDeploymentCountAsync(environment, status, start, end);

                return Ok(new
                {
                    deployments,
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
                _logger.LogError(ex, "Failed to list deployments: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list deployments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets deployment logs.
        /// </summary>
        [HttpGet("deployments/{deploymentId}/logs")]
        public async Task<IActionResult> GetDeploymentLogs(string deploymentId)
        {
            try
            {
                var logs = await _deployService.GetDeploymentLogsAsync(deploymentId);

                return Ok(new
                {
                    deploymentId,
                    logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get deployment logs for {DeploymentId}: {Message}", deploymentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve deployment logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets deployment statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetDeploymentStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _deployService.GetDeploymentStatsAsync(start, end);

                return Ok(new
                {
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get deployment stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve deployment statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available environments.
        /// </summary>
        [HttpGet("environments")]
        public async Task<IActionResult> GetEnvironments()
        {
            try
            {
                var environments = await _deployService.GetEnvironmentsAsync();

                return Ok(new
                {
                    environments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get environments: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve environments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available versions.
        /// </summary>
        [HttpGet("versions")]
        public async Task<IActionResult> GetVersions()
        {
            try
            {
                var versions = await _deployService.GetVersionsAsync();

                return Ok(new
                {
                    versions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get versions: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve versions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available components.
        /// </summary>
        [HttpGet("components")]
        public async Task<IActionResult> GetComponents()
        {
            try
            {
                var components = await _deployService.GetComponentsAsync("default");

                return Ok(new
                {
                    components
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get components: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve components",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancels a deployment.
        /// </summary>
        [HttpPost("deployments/{deploymentId}/cancel")]
        public async Task<IActionResult> CancelDeployment(string deploymentId)
        {
            try
            {
                await _deployService.CancelDeploymentAsync(deploymentId);

                return Ok(new
                {
                    message = "Deployment cancelled successfully",
                    deploymentId,
                    cancelledAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel deployment {DeploymentId}: {Message}", deploymentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to cancel deployment",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Rolls back a deployment.
        /// </summary>
        [HttpPost("deployments/{deploymentId}/rollback")]
        public async Task<IActionResult> RollbackDeployment(string deploymentId)
        {
            try
            {
                await _deployService.RollbackDeploymentAsync(deploymentId);

                return Ok(new
                {
                    message = "Deployment rollback initiated successfully",
                    deploymentId,
                    rolledBackAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback deployment {DeploymentId}: {Message}", deploymentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to rollback deployment",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a deployment.
        /// </summary>
        [HttpDelete("deployments/{deploymentId}")]
        public async Task<IActionResult> DeleteDeployment(string deploymentId)
        {
            try
            {
                await _deployService.DeleteDeploymentAsync(deploymentId);

                return Ok(new
                {
                    message = "Deployment deleted successfully",
                    deploymentId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete deployment {DeploymentId}: {Message}", deploymentId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete deployment",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Deploy request model.
    /// </summary>
    public class DeployRequest
    {
        /// <summary>
        /// Target environment.
        /// </summary>
        public string Environment { get; set; } = string.Empty;

        /// <summary>
        /// Version to deploy.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Components to deploy.
        /// </summary>
        public List<string> Components { get; set; } = new();

        /// <summary>
        /// Deployment parameters.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }
    }
}