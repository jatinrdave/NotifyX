using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for branch operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly ILogger<BranchController> _logger;
        private readonly IBranchService _branchService;

        public BranchController(ILogger<BranchController> logger, IBranchService branchService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _branchService = branchService ?? throw new ArgumentNullException(nameof(branchService));
        }

        /// <summary>
        /// Creates a branch.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Branch request is required");
                }

                var branchId = await _branchService.CreateBranchAsync(
                    request.ProjectId,
                    request.Name,
                    request.Description,
                    request.BranchType,
                    request.SourceBranch,
                    request.Metadata);

                return Ok(new
                {
                    branchId,
                    message = "Branch created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create branch: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create branch",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets branch information.
        /// </summary>
        [HttpGet("{branchId}")]
        public async Task<IActionResult> GetBranch(string branchId)
        {
            try
            {
                var branch = await _branchService.GetBranchAsync(branchId);

                if (branch == null)
                {
                    return NotFound(new
                    {
                        error = "Branch not found",
                        branchId
                    });
                }

                return Ok(branch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get branch {BranchId}: {Message}", branchId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve branch",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists branches.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListBranches(
            [FromQuery] string? projectId,
            [FromQuery] string? branchType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var branches = await _branchService.ListBranchesAsync(projectId, branchType, page, pageSize);
                var totalCount = await _branchService.GetBranchCountAsync(projectId, branchType);

                return Ok(new
                {
                    branches,
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
                _logger.LogError(ex, "Failed to list branches: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list branches",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a branch.
        /// </summary>
        [HttpPut("{branchId}")]
        public async Task<IActionResult> UpdateBranch(
            string branchId,
            [FromBody] UpdateBranchRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _branchService.UpdateBranchAsync(
                    branchId,
                    request.Name,
                    request.Description,
                    request.BranchType,
                    request.Metadata);

                return Ok(new
                {
                    message = "Branch updated successfully",
                    branchId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update branch {BranchId}: {Message}", branchId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update branch",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a branch.
        /// </summary>
        [HttpDelete("{branchId}")]
        public async Task<IActionResult> DeleteBranch(string branchId)
        {
            try
            {
                await _branchService.DeleteBranchAsync(branchId);

                return Ok(new
                {
                    message = "Branch deleted successfully",
                    branchId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete branch {BranchId}: {Message}", branchId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete branch",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets branch status.
        /// </summary>
        [HttpGet("{branchId}/status")]
        public async Task<IActionResult> GetBranchStatus(string branchId)
        {
            try
            {
                var status = await _branchService.GetBranchStatusAsync(branchId);

                return Ok(new
                {
                    branchId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get branch status for {BranchId}: {Message}", branchId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve branch status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets branch commits.
        /// </summary>
        [HttpGet("{branchId}/commits")]
        public async Task<IActionResult> GetBranchCommits(string branchId)
        {
            try
            {
                var commits = await _branchService.GetBranchCommitsAsync(branchId);

                return Ok(new
                {
                    branchId,
                    commits
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get branch commits for {BranchId}: {Message}", branchId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve branch commits",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets branch builds.
        /// </summary>
        [HttpGet("{branchId}/builds")]
        public async Task<IActionResult> GetBranchBuilds(string branchId)
        {
            try
            {
                var builds = await _branchService.GetBranchBuildsAsync(branchId);

                return Ok(new
                {
                    branchId,
                    builds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get branch builds for {BranchId}: {Message}", branchId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve branch builds",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets branch deployments.
        /// </summary>
        [HttpGet("{branchId}/deployments")]
        public async Task<IActionResult> GetBranchDeployments(string branchId)
        {
            try
            {
                var deployments = await _branchService.GetBranchDeploymentsAsync(branchId);

                return Ok(new
                {
                    branchId,
                    deployments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get branch deployments for {BranchId}: {Message}", branchId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve branch deployments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets branch statistics.
        /// </summary>
        [HttpGet("{branchId}/stats")]
        public async Task<IActionResult> GetBranchStats(string branchId)
        {
            try
            {
                var stats = await _branchService.GetBranchStatsAsync(branchId);

                return Ok(new
                {
                    branchId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get branch stats for {BranchId}: {Message}", branchId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve branch statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available branch types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetBranchTypes()
        {
            try
            {
                var branchTypes = await _branchService.GetBranchTypesAsync();

                return Ok(new
                {
                    branchTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get branch types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve branch types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create branch request model.
    /// </summary>
    public class CreateBranchRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Branch name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Branch description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Branch type.
        /// </summary>
        public string BranchType { get; set; } = "feature";

        /// <summary>
        /// Source branch.
        /// </summary>
        public string? SourceBranch { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update branch request model.
    /// </summary>
    public class UpdateBranchRequest
    {
        /// <summary>
        /// Branch name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Branch description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Branch type.
        /// </summary>
        public string? BranchType { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}