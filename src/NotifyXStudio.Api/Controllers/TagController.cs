using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for tag operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly ILogger<TagController> _logger;
        private readonly ITagService _tagService;

        public TagController(ILogger<TagController> logger, ITagService tagService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        }

        /// <summary>
        /// Creates a tag.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Tag request is required");
                }

                var tagId = await _tagService.CreateTagAsync(
                    request.ProjectId,
                    request.BranchId,
                    request.Name,
                    request.Description,
                    request.TagType,
                    request.Metadata);

                return Ok(new
                {
                    tagId,
                    message = "Tag created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create tag: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create tag",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tag information.
        /// </summary>
        [HttpGet("{tagId}")]
        public async Task<IActionResult> GetTag(string tagId)
        {
            try
            {
                var tag = await _tagService.GetTagAsync(tagId);

                if (tag == null)
                {
                    return NotFound(new
                    {
                        error = "Tag not found",
                        tagId
                    });
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tag {TagId}: {Message}", tagId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tag",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists tags.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListTags(
            [FromQuery] string? projectId,
            [FromQuery] string? branchId,
            [FromQuery] string? tagType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var tags = await _tagService.ListTagsAsync(projectId, branchId, tagType, page, pageSize);
                var totalCount = await _tagService.GetTagCountAsync(projectId, branchId, tagType);

                return Ok(new
                {
                    tags,
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
                _logger.LogError(ex, "Failed to list tags: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list tags",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a tag.
        /// </summary>
        [HttpPut("{tagId}")]
        public async Task<IActionResult> UpdateTag(
            string tagId,
            [FromBody] UpdateTagRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _tagService.UpdateTagAsync(
                    tagId,
                    request.Name,
                    request.Description,
                    request.TagType,
                    request.Metadata);

                return Ok(new
                {
                    message = "Tag updated successfully",
                    tagId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update tag {TagId}: {Message}", tagId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update tag",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a tag.
        /// </summary>
        [HttpDelete("{tagId}")]
        public async Task<IActionResult> DeleteTag(string tagId)
        {
            try
            {
                await _tagService.DeleteTagAsync(tagId);

                return Ok(new
                {
                    message = "Tag deleted successfully",
                    tagId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete tag {TagId}: {Message}", tagId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete tag",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tag status.
        /// </summary>
        [HttpGet("{tagId}/status")]
        public async Task<IActionResult> GetTagStatus(string tagId)
        {
            try
            {
                var status = await _tagService.GetTagStatusAsync(tagId);

                return Ok(new
                {
                    tagId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tag status for {TagId}: {Message}", tagId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tag status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tag commits.
        /// </summary>
        [HttpGet("{tagId}/commits")]
        public async Task<IActionResult> GetTagCommits(string tagId)
        {
            try
            {
                var commits = await _tagService.GetTagCommitsAsync(tagId);

                return Ok(new
                {
                    tagId,
                    commits
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tag commits for {TagId}: {Message}", tagId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tag commits",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tag builds.
        /// </summary>
        [HttpGet("{tagId}/builds")]
        public async Task<IActionResult> GetTagBuilds(string tagId)
        {
            try
            {
                var builds = await _tagService.GetTagBuildsAsync(tagId);

                return Ok(new
                {
                    tagId,
                    builds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tag builds for {TagId}: {Message}", tagId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tag builds",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tag deployments.
        /// </summary>
        [HttpGet("{tagId}/deployments")]
        public async Task<IActionResult> GetTagDeployments(string tagId)
        {
            try
            {
                var deployments = await _tagService.GetTagDeploymentsAsync(tagId);

                return Ok(new
                {
                    tagId,
                    deployments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tag deployments for {TagId}: {Message}", tagId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tag deployments",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets tag statistics.
        /// </summary>
        [HttpGet("{tagId}/stats")]
        public async Task<IActionResult> GetTagStats(string tagId)
        {
            try
            {
                var stats = await _tagService.GetTagStatsAsync(tagId);

                return Ok(new
                {
                    tagId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tag stats for {TagId}: {Message}", tagId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tag statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available tag types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetTagTypes()
        {
            try
            {
                var tagTypes = await _tagService.GetTagTypesAsync();

                return Ok(new
                {
                    tagTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get tag types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve tag types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create tag request model.
    /// </summary>
    public class CreateTagRequest
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
        /// Tag name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tag description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tag type.
        /// </summary>
        public string TagType { get; set; } = "version";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update tag request model.
    /// </summary>
    public class UpdateTagRequest
    {
        /// <summary>
        /// Tag name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Tag description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Tag type.
        /// </summary>
        public string? TagType { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}