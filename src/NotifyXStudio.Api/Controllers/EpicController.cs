using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for epic operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EpicController : ControllerBase
    {
        private readonly ILogger<EpicController> _logger;
        private readonly IEpicService _epicService;

        public EpicController(ILogger<EpicController> logger, IEpicService epicService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _epicService = epicService ?? throw new ArgumentNullException(nameof(epicService));
        }

        /// <summary>
        /// Creates an epic.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEpic([FromBody] CreateEpicRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Epic request is required");
                }

                var epicId = await _epicService.CreateEpicAsync(
                    request.ProjectId,
                    request.Title,
                    request.Description,
                    request.EpicType,
                    request.Priority,
                    request.Metadata);

                return Ok(new
                {
                    epicId,
                    message = "Epic created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create epic: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create epic",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets epic information.
        /// </summary>
        [HttpGet("{epicId}")]
        public async Task<IActionResult> GetEpic(string epicId)
        {
            try
            {
                var epic = await _epicService.GetEpicAsync(epicId);

                if (epic == null)
                {
                    return NotFound(new
                    {
                        error = "Epic not found",
                        epicId
                    });
                }

                return Ok(epic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get epic {EpicId}: {Message}", epicId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve epic",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists epics.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListEpics(
            [FromQuery] string? projectId,
            [FromQuery] string? epicType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var epics = await _epicService.ListEpicsAsync(projectId, epicType, status, page, pageSize);
                var totalCount = await _epicService.GetEpicCountAsync(projectId, epicType, status);

                return Ok(new
                {
                    epics,
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
                _logger.LogError(ex, "Failed to list epics: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list epics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an epic.
        /// </summary>
        [HttpPut("{epicId}")]
        public async Task<IActionResult> UpdateEpic(
            string epicId,
            [FromBody] UpdateEpicRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _epicService.UpdateEpicAsync(
                    epicId,
                    request.Title,
                    request.Description,
                    request.EpicType,
                    request.Priority,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Epic updated successfully",
                    epicId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update epic {EpicId}: {Message}", epicId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update epic",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an epic.
        /// </summary>
        [HttpDelete("{epicId}")]
        public async Task<IActionResult> DeleteEpic(string epicId)
        {
            try
            {
                await _epicService.DeleteEpicAsync(epicId);

                return Ok(new
                {
                    message = "Epic deleted successfully",
                    epicId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete epic {EpicId}: {Message}", epicId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete epic",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets epic status.
        /// </summary>
        [HttpGet("{epicId}/status")]
        public async Task<IActionResult> GetEpicStatus(string epicId)
        {
            try
            {
                var status = await _epicService.GetEpicStatusAsync(epicId);

                return Ok(new
                {
                    epicId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get epic status for {EpicId}: {Message}", epicId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve epic status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets epic issues.
        /// </summary>
        [HttpGet("{epicId}/issues")]
        public async Task<IActionResult> GetEpicIssues(string epicId)
        {
            try
            {
                var issues = await _epicService.GetEpicIssuesAsync(epicId);

                return Ok(new
                {
                    epicId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get epic issues for {EpicId}: {Message}", epicId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve epic issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets epic statistics.
        /// </summary>
        [HttpGet("{epicId}/stats")]
        public async Task<IActionResult> GetEpicStats(string epicId)
        {
            try
            {
                var stats = await _epicService.GetEpicStatsAsync(epicId);

                return Ok(new
                {
                    epicId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get epic stats for {EpicId}: {Message}", epicId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve epic statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available epic types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetEpicTypes()
        {
            try
            {
                var epicTypes = await _epicService.GetEpicTypesAsync();

                return Ok(new
                {
                    epicTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get epic types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve epic types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create epic request model.
    /// </summary>
    public class CreateEpicRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Epic title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Epic description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Epic type.
        /// </summary>
        public string EpicType { get; set; } = "feature";

        /// <summary>
        /// Epic priority.
        /// </summary>
        public string Priority { get; set; } = "medium";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update epic request model.
    /// </summary>
    public class UpdateEpicRequest
    {
        /// <summary>
        /// Epic title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Epic description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Epic type.
        /// </summary>
        public string? EpicType { get; set; }

        /// <summary>
        /// Epic priority.
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Epic status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}