using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for commit operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CommitController : ControllerBase
    {
        private readonly ILogger<CommitController> _logger;
        private readonly ICommitService _commitService;

        public CommitController(ILogger<CommitController> logger, ICommitService commitService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commitService = commitService ?? throw new ArgumentNullException(nameof(commitService));
        }

        /// <summary>
        /// Creates a commit.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCommit([FromBody] CreateCommitRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Commit request is required");
                }

                var commitId = await _commitService.CreateCommitAsync(
                    request.ProjectId,
                    request.BranchId,
                    request.Message,
                    request.Author,
                    request.Files,
                    request.Metadata);

                return Ok(new
                {
                    commitId,
                    message = "Commit created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create commit: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create commit",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets commit information.
        /// </summary>
        [HttpGet("{commitId}")]
        public async Task<IActionResult> GetCommit(string commitId)
        {
            try
            {
                var commit = await _commitService.GetCommitAsync(commitId);

                if (commit == null)
                {
                    return NotFound(new
                    {
                        error = "Commit not found",
                        commitId
                    });
                }

                return Ok(commit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get commit {CommitId}: {Message}", commitId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve commit",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists commits.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListCommits(
            [FromQuery] string? projectId,
            [FromQuery] string? branchId,
            [FromQuery] string? author,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var commits = await _commitService.ListCommitsAsync(projectId, branchId, author, start, end, page, pageSize);
                var totalCount = await _commitService.GetCommitCountAsync(projectId, branchId, author, start, end);

                return Ok(new
                {
                    commits,
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
                _logger.LogError(ex, "Failed to list commits: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list commits",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a commit.
        /// </summary>
        [HttpPut("{commitId}")]
        public async Task<IActionResult> UpdateCommit(
            string commitId,
            [FromBody] UpdateCommitRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _commitService.UpdateCommitAsync(
                    commitId,
                    request.Message,
                    request.Files,
                    request.Metadata);

                return Ok(new
                {
                    message = "Commit updated successfully",
                    commitId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update commit {CommitId}: {Message}", commitId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update commit",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a commit.
        /// </summary>
        [HttpDelete("{commitId}")]
        public async Task<IActionResult> DeleteCommit(string commitId)
        {
            try
            {
                await _commitService.DeleteCommitAsync(commitId);

                return Ok(new
                {
                    message = "Commit deleted successfully",
                    commitId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete commit {CommitId}: {Message}", commitId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete commit",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets commit status.
        /// </summary>
        [HttpGet("{commitId}/status")]
        public async Task<IActionResult> GetCommitStatus(string commitId)
        {
            try
            {
                var status = await _commitService.GetCommitStatusAsync(commitId);

                return Ok(new
                {
                    commitId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get commit status for {CommitId}: {Message}", commitId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve commit status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets commit files.
        /// </summary>
        [HttpGet("{commitId}/files")]
        public async Task<IActionResult> GetCommitFiles(string commitId)
        {
            try
            {
                var files = await _commitService.GetCommitFilesAsync(commitId);

                return Ok(new
                {
                    commitId,
                    files
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get commit files for {CommitId}: {Message}", commitId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve commit files",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets commit builds.
        /// </summary>
        [HttpGet("{commitId}/builds")]
        public async Task<IActionResult> GetCommitBuilds(string commitId)
        {
            try
            {
                var builds = await _commitService.GetCommitBuildsAsync(commitId);

                return Ok(new
                {
                    commitId,
                    builds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get commit builds for {CommitId}: {Message}", commitId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve commit builds",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets commit deployments.
        /// </summary>
        [HttpGet("{commitId}/deployments")]
        public async Task<IActionResult> GetCommitDeployments(string commitId)
        {
            try
            {
                var deployments = await _commitService.GetCommitDeploymentsAsync(commitId);

                return Ok(new
                {
                    commitId,
                    deployments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get commit deployments for {CommitId}: {Message}", commitId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve commit deployments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets commit statistics.
        /// </summary>
        [HttpGet("{commitId}/stats")]
        public async Task<IActionResult> GetCommitStats(string commitId)
        {
            try
            {
                var stats = await _commitService.GetCommitStatsAsync(commitId);

                return Ok(new
                {
                    commitId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get commit stats for {CommitId}: {Message}", commitId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve commit statistics",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create commit request model.
    /// </summary>
    public class CreateCommitRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Branch ID.
        /// </summary>
        public string BranchId { get; set; } = string.Empty;

        /// <summary>
        /// Commit message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Commit author.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Files in the commit.
        /// </summary>
        public List<string> Files { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update commit request model.
    /// </summary>
    public class UpdateCommitRequest
    {
        /// <summary>
        /// Commit message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Files in the commit.
        /// </summary>
        public List<string>? Files { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}