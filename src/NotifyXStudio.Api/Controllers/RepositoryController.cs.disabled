using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for repository operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RepositoryController : ControllerBase
    {
        private readonly ILogger<RepositoryController> _logger;
        private readonly IRepositoryService _repositoryService;

        public RepositoryController(ILogger<RepositoryController> logger, IRepositoryService repositoryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
        }

        /// <summary>
        /// Creates a repository.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRepository([FromBody] CreateRepositoryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Repository request is required");
                }

                var repositoryId = await _repositoryService.CreateRepositoryAsync(
                    request.Name,
                    request.Description,
                    null,
                    null);

                return Ok(new
                {
                    repositoryId,
                    message = "Repository created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create repository: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create repository",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets repository information.
        /// </summary>
        [HttpGet("{repositoryId}")]
        public async Task<IActionResult> GetRepository(string repositoryId)
        {
            try
            {
                var repository = await _repositoryService.GetRepositoryAsync(repositoryId);

                if (repository == null)
                {
                    return NotFound(new
                    {
                        error = "Repository not found",
                        repositoryId
                    });
                }

                return Ok(repository);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get repository {RepositoryId}: {Message}", repositoryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve repository",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists repositories.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListRepositories(
            [FromQuery] string? repositoryType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var repositories = await _repositoryService.ListRepositoriesAsync(repositoryType, page, pageSize);
                var totalCount = await _repositoryService.GetRepositoryCountAsync(repositoryType);

                return Ok(new
                {
                    repositories,
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
                _logger.LogError(ex, "Failed to list repositories: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list repositories",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a repository.
        /// </summary>
        [HttpPut("{repositoryId}")]
        public async Task<IActionResult> UpdateRepository(
            string repositoryId,
            [FromBody] UpdateRepositoryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _repositoryService.UpdateRepositoryAsync(
                    repositoryId,
                    request.Name,
                    request.Description,
                    null,
                    null);

                return Ok(new
                {
                    message = "Repository updated successfully",
                    repositoryId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update repository {RepositoryId}: {Message}", repositoryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update repository",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a repository.
        /// </summary>
        [HttpDelete("{repositoryId}")]
        public async Task<IActionResult> DeleteRepository(string repositoryId)
        {
            try
            {
                await _repositoryService.DeleteRepositoryAsync(repositoryId);

                return Ok(new
                {
                    message = "Repository deleted successfully",
                    repositoryId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete repository {RepositoryId}: {Message}", repositoryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete repository",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets repository status.
        /// </summary>
        [HttpGet("{repositoryId}/status")]
        public async Task<IActionResult> GetRepositoryStatus(string repositoryId)
        {
            try
            {
                var status = await _repositoryService.GetRepositoryStatusAsync(repositoryId);

                return Ok(new
                {
                    repositoryId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get repository status for {RepositoryId}: {Message}", repositoryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve repository status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets repository branches.
        /// </summary>
        [HttpGet("{repositoryId}/branches")]
        public async Task<IActionResult> GetRepositoryBranches(string repositoryId)
        {
            try
            {
                var branches = await _repositoryService.GetRepositoryBranchesAsync(repositoryId);

                return Ok(new
                {
                    repositoryId,
                    branches
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get repository branches for {RepositoryId}: {Message}", repositoryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve repository branches",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets repository commits.
        /// </summary>
        [HttpGet("{repositoryId}/commits")]
        public async Task<IActionResult> GetRepositoryCommits(string repositoryId)
        {
            try
            {
                var commits = await _repositoryService.GetRepositoryCommitsAsync(repositoryId);

                return Ok(new
                {
                    repositoryId,
                    commits
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get repository commits for {RepositoryId}: {Message}", repositoryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve repository commits",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets repository files.
        /// </summary>
        [HttpGet("{repositoryId}/files")]
        public async Task<IActionResult> GetRepositoryFiles(string repositoryId)
        {
            try
            {
                var files = await _repositoryService.GetRepositoryFilesAsync(repositoryId);

                return Ok(new
                {
                    repositoryId,
                    files
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get repository files for {RepositoryId}: {Message}", repositoryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve repository files",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets repository statistics.
        /// </summary>
        [HttpGet("{repositoryId}/stats")]
        public async Task<IActionResult> GetRepositoryStats(string repositoryId)
        {
            try
            {
                var stats = await _repositoryService.GetRepositoryStatsAsync(repositoryId);

                return Ok(new
                {
                    repositoryId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get repository stats for {RepositoryId}: {Message}", repositoryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve repository statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available repository types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetRepositoryTypes()
        {
            try
            {
                var repositoryTypes = await _repositoryService.GetRepositoryTypesAsync();

                return Ok(new
                {
                    repositoryTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get repository types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve repository types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create repository request model.
    /// </summary>
    public class CreateRepositoryRequest
    {
        /// <summary>
        /// Repository name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Repository description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Repository type.
        /// </summary>
        public string RepositoryType { get; set; } = "git";

        /// <summary>
        /// Repository configuration.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update repository request model.
    /// </summary>
    public class UpdateRepositoryRequest
    {
        /// <summary>
        /// Repository name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Repository description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Repository type.
        /// </summary>
        public string? RepositoryType { get; set; }

        /// <summary>
        /// Repository configuration.
        /// </summary>
        public Dictionary<string, object>? Configuration { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}