using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for build operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BuildController : ControllerBase
    {
        private readonly ILogger<BuildController> _logger;
        private readonly IBuildService _buildService;

        public BuildController(ILogger<BuildController> logger, IBuildService buildService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
        }

        /// <summary>
        /// Initiates a build.
        /// </summary>
        [HttpPost("build")]
        public async Task<IActionResult> Build([FromBody] BuildRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Build request is required");
                }

                var buildId = await _buildService.BuildAsync(
                    request.Project,
                    request.Branch,
                    request.Commit,
                    request.BuildType,
                    request.Parameters);

                return Ok(new
                {
                    buildId,
                    message = "Build initiated successfully",
                    startedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to initiate build",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets build status.
        /// </summary>
        [HttpGet("builds/{buildId}")]
        public async Task<IActionResult> GetBuildStatus(string buildId)
        {
            try
            {
                var build = await _buildService.GetBuildStatusAsync(buildId);

                if (build == null)
                {
                    return NotFound(new
                    {
                        error = "Build not found",
                        buildId
                    });
                }

                return Ok(build);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get build status for {BuildId}: {Message}", buildId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve build status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists builds.
        /// </summary>
        [HttpGet("builds")]
        public async Task<IActionResult> ListBuilds(
            [FromQuery] string? project,
            [FromQuery] string? branch,
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                var builds = await _buildService.ListBuildsAsync(project, branch, status, start, end, page, pageSize);
                var totalCount = await _buildService.GetBuildCountAsync(project, branch, status, start, end);

                return Ok(new
                {
                    builds,
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
                _logger.LogError(ex, "Failed to list builds: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list builds",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets build logs.
        /// </summary>
        [HttpGet("builds/{buildId}/logs")]
        public async Task<IActionResult> GetBuildLogs(string buildId)
        {
            try
            {
                var logs = await _buildService.GetBuildLogsAsync(buildId);

                return Ok(new
                {
                    buildId,
                    logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get build logs for {BuildId}: {Message}", buildId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve build logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets build artifacts.
        /// </summary>
        [HttpGet("builds/{buildId}/artifacts")]
        public async Task<IActionResult> GetBuildArtifacts(string buildId)
        {
            try
            {
                var artifacts = await _buildService.GetBuildArtifactsAsync(buildId);

                return Ok(new
                {
                    buildId,
                    artifacts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get build artifacts for {BuildId}: {Message}", buildId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve build artifacts",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Downloads build artifact.
        /// </summary>
        [HttpGet("builds/{buildId}/artifacts/{artifactName}/download")]
        public async Task<IActionResult> DownloadBuildArtifact(string buildId, string artifactName)
        {
            try
            {
                var artifactStream = await _buildService.DownloadBuildArtifactAsync(buildId, artifactName);

                if (artifactStream == null)
                {
                    return NotFound(new
                    {
                        error = "Build artifact not found",
                        buildId,
                        artifactName
                    });
                }

                return File(artifactStream, "application/octet-stream", artifactName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download build artifact {ArtifactName} for {BuildId}: {Message}", artifactName, buildId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to download build artifact",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets build statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetBuildStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _buildService.GetBuildStatsAsync(start, end);

                return Ok(new
                {
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get build stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve build statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available projects.
        /// </summary>
        [HttpGet("projects")]
        public async Task<IActionResult> GetProjects()
        {
            try
            {
                var projects = await _buildService.GetProjectsAsync();

                return Ok(new
                {
                    projects
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get projects: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve projects",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available branches.
        /// </summary>
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches([FromQuery] string? project)
        {
            try
            {
                var branches = await _buildService.GetBranchesAsync(project);

                return Ok(new
                {
                    branches
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get branches: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve branches",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available build types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetBuildTypes()
        {
            try
            {
                var buildTypes = await _buildService.GetBuildTypesAsync();

                return Ok(new
                {
                    buildTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get build types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve build types",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancels a build.
        /// </summary>
        [HttpPost("builds/{buildId}/cancel")]
        public async Task<IActionResult> CancelBuild(string buildId)
        {
            try
            {
                await _buildService.CancelBuildAsync(buildId);

                return Ok(new
                {
                    message = "Build cancelled successfully",
                    buildId,
                    cancelledAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel build {BuildId}: {Message}", buildId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to cancel build",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a build.
        /// </summary>
        [HttpDelete("builds/{buildId}")]
        public async Task<IActionResult> DeleteBuild(string buildId)
        {
            try
            {
                await _buildService.DeleteBuildAsync(buildId);

                return Ok(new
                {
                    message = "Build deleted successfully",
                    buildId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete build {BuildId}: {Message}", buildId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete build",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Build request model.
    /// </summary>
    public class BuildRequest
    {
        /// <summary>
        /// Project name.
        /// </summary>
        public string Project { get; set; } = string.Empty;

        /// <summary>
        /// Branch name.
        /// </summary>
        public string Branch { get; set; } = string.Empty;

        /// <summary>
        /// Commit hash.
        /// </summary>
        public string Commit { get; set; } = string.Empty;

        /// <summary>
        /// Build type.
        /// </summary>
        public string BuildType { get; set; } = "release";

        /// <summary>
        /// Build parameters.
        /// </summary>
        public Dictionary<string, object>? Parameters { get; set; }
    }
}