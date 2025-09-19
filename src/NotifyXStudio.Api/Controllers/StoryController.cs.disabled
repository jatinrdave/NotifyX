using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for story operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StoryController : ControllerBase
    {
        private readonly ILogger<StoryController> _logger;
        private readonly IStoryService _storyService;

        public StoryController(ILogger<StoryController> logger, IStoryService storyService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storyService = storyService ?? throw new ArgumentNullException(nameof(storyService));
        }

        /// <summary>
        /// Creates a story.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateStory([FromBody] CreateStoryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Story request is required");
                }

                var storyId = await _storyService.CreateStoryAsync(
                    request.Title,
                    request.Description,
                    request.StoryType,
                    request.Priority,
                    null,
                    request.ProjectId);

                return Ok(new
                {
                    storyId,
                    message = "Story created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create story: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create story",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets story information.
        /// </summary>
        [HttpGet("{storyId}")]
        public async Task<IActionResult> GetStory(string storyId)
        {
            try
            {
                var story = await _storyService.GetStoryAsync(storyId);

                if (story == null)
                {
                    return NotFound(new
                    {
                        error = "Story not found",
                        storyId
                    });
                }

                return Ok(story);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get story {StoryId}: {Message}", storyId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve story",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists stories.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListStories(
            [FromQuery] string? projectId,
            [FromQuery] string? storyType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var stories = await _storyService.ListStoriesAsync(projectId, storyType, status, page, pageSize);
                var totalCount = await _storyService.GetStoryCountAsync(projectId, storyType, status);

                return Ok(new
                {
                    stories,
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
                _logger.LogError(ex, "Failed to list stories: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list stories",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a story.
        /// </summary>
        [HttpPut("{storyId}")]
        public async Task<IActionResult> UpdateStory(
            string storyId,
            [FromBody] UpdateStoryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _storyService.UpdateStoryAsync(
                    storyId,
                    request.Title,
                    request.Description,
                    request.StoryType,
                    request.Priority,
                    request.Estimate,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Story updated successfully",
                    storyId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update story {StoryId}: {Message}", storyId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update story",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a story.
        /// </summary>
        [HttpDelete("{storyId}")]
        public async Task<IActionResult> DeleteStory(string storyId)
        {
            try
            {
                await _storyService.DeleteStoryAsync(storyId);

                return Ok(new
                {
                    message = "Story deleted successfully",
                    storyId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete story {StoryId}: {Message}", storyId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete story",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets story status.
        /// </summary>
        [HttpGet("{storyId}/status")]
        public async Task<IActionResult> GetStoryStatus(string storyId)
        {
            try
            {
                var status = await _storyService.GetStoryStatusAsync(storyId);

                return Ok(new
                {
                    storyId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get story status for {StoryId}: {Message}", storyId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve story status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets story issues.
        /// </summary>
        [HttpGet("{storyId}/issues")]
        public async Task<IActionResult> GetStoryIssues(string storyId)
        {
            try
            {
                var issues = await _storyService.GetStoryIssuesAsync(storyId);

                return Ok(new
                {
                    storyId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get story issues for {StoryId}: {Message}", storyId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve story issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets story statistics.
        /// </summary>
        [HttpGet("{storyId}/stats")]
        public async Task<IActionResult> GetStoryStats(string storyId)
        {
            try
            {
                var stats = await _storyService.GetStoryStatsAsync(storyId);

                return Ok(new
                {
                    storyId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get story stats for {StoryId}: {Message}", storyId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve story statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available story types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetStoryTypes()
        {
            try
            {
                var storyTypes = await _storyService.GetStoryTypesAsync();

                return Ok(new
                {
                    storyTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get story types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve story types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create story request model.
    /// </summary>
    public class CreateStoryRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Story title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Story description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Story type.
        /// </summary>
        public string StoryType { get; set; } = "user_story";

        /// <summary>
        /// Story priority.
        /// </summary>
        public string Priority { get; set; } = "medium";

        /// <summary>
        /// Story estimate.
        /// </summary>
        public int? Estimate { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update story request model.
    /// </summary>
    public class UpdateStoryRequest
    {
        /// <summary>
        /// Story title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Story description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Story type.
        /// </summary>
        public string? StoryType { get; set; }

        /// <summary>
        /// Story priority.
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Story estimate.
        /// </summary>
        public int? Estimate { get; set; }

        /// <summary>
        /// Story status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}