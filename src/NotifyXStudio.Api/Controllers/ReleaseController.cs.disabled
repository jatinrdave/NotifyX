using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for release operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReleaseController : ControllerBase
    {
        private readonly ILogger<ReleaseController> _logger;
        private readonly IReleaseService _releaseService;

        public ReleaseController(ILogger<ReleaseController> logger, IReleaseService releaseService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _releaseService = releaseService ?? throw new ArgumentNullException(nameof(releaseService));
        }

        /// <summary>
        /// Creates a release.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRelease([FromBody] CreateReleaseRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Release request is required");
                }

                var releaseId = await _releaseService.CreateReleaseAsync(
                    request.Version,
                    request.Version,
                    request.Description,
                    null);

                return Ok(new
                {
                    releaseId,
                    message = "Release created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create release: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create release",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets release information.
        /// </summary>
        [HttpGet("{releaseId}")]
        public async Task<IActionResult> GetRelease(string releaseId)
        {
            try
            {
                var release = await _releaseService.GetReleaseAsync(releaseId);

                if (release == null)
                {
                    return NotFound(new
                    {
                        error = "Release not found",
                        releaseId
                    });
                }

                return Ok(release);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get release {ReleaseId}: {Message}", releaseId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve release",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists releases.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListReleases(
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

                var releases = await _releaseService.ListReleasesAsync(null, status, page, pageSize);
                var totalCount = await _releaseService.GetReleaseCountAsync(null, status);

                return Ok(new
                {
                    releases,
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
                _logger.LogError(ex, "Failed to list releases: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list releases",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a release.
        /// </summary>
        [HttpPut("{releaseId}")]
        public async Task<IActionResult> UpdateRelease(
            string releaseId,
            [FromBody] UpdateReleaseRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _releaseService.UpdateReleaseAsync(
                    releaseId,
                    request.Description,
                    request.ReleaseNotes,
                    null);

                return Ok(new
                {
                    message = "Release updated successfully",
                    releaseId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update release {ReleaseId}: {Message}", releaseId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update release",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a release.
        /// </summary>
        [HttpDelete("{releaseId}")]
        public async Task<IActionResult> DeleteRelease(string releaseId)
        {
            try
            {
                await _releaseService.DeleteReleaseAsync(releaseId);

                return Ok(new
                {
                    message = "Release deleted successfully",
                    releaseId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete release {ReleaseId}: {Message}", releaseId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete release",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Publishes a release.
        /// </summary>
        [HttpPost("{releaseId}/publish")]
        public async Task<IActionResult> PublishRelease(string releaseId)
        {
            try
            {
                await _releaseService.PublishReleaseAsync(releaseId);

                return Ok(new
                {
                    message = "Release published successfully",
                    releaseId,
                    publishedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish release {ReleaseId}: {Message}", releaseId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to publish release",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Unpublishes a release.
        /// </summary>
        [HttpPost("{releaseId}/unpublish")]
        public async Task<IActionResult> UnpublishRelease(string releaseId)
        {
            try
            {
                await _releaseService.UnpublishReleaseAsync(releaseId);

                return Ok(new
                {
                    message = "Release unpublished successfully",
                    releaseId,
                    unpublishedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unpublish release {ReleaseId}: {Message}", releaseId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to unpublish release",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets release statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetReleaseStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _releaseService.GetReleaseStatsAsync("default");

                return Ok(new
                {
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get release stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve release statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets release components.
        /// </summary>
        [HttpGet("{releaseId}/components")]
        public async Task<IActionResult> GetReleaseComponents(string releaseId)
        {
            try
            {
                var components = await _releaseService.GetReleaseComponentsAsync(releaseId);

                return Ok(new
                {
                    releaseId,
                    components
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get release components for {ReleaseId}: {Message}", releaseId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve release components",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets release deployments.
        /// </summary>
        [HttpGet("{releaseId}/deployments")]
        public async Task<IActionResult> GetReleaseDeployments(string releaseId)
        {
            try
            {
                var deployments = await _releaseService.GetReleaseDeploymentsAsync(releaseId);

                return Ok(new
                {
                    releaseId,
                    deployments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get release deployments for {ReleaseId}: {Message}", releaseId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve release deployments",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create release request model.
    /// </summary>
    public class CreateReleaseRequest
    {
        /// <summary>
        /// Release version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Release description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Release notes.
        /// </summary>
        public string ReleaseNotes { get; set; } = string.Empty;

        /// <summary>
        /// Release components.
        /// </summary>
        public List<string> Components { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update release request model.
    /// </summary>
    public class UpdateReleaseRequest
    {
        /// <summary>
        /// Release description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Release notes.
        /// </summary>
        public string? ReleaseNotes { get; set; }

        /// <summary>
        /// Release components.
        /// </summary>
        public List<string>? Components { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}