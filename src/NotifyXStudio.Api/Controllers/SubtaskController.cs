using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for subtask operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SubtaskController : ControllerBase
    {
        private readonly ILogger<SubtaskController> _logger;
        private readonly ISubtaskService _subtaskService;

        public SubtaskController(ILogger<SubtaskController> logger, ISubtaskService subtaskService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subtaskService = subtaskService ?? throw new ArgumentNullException(nameof(subtaskService));
        }

        /// <summary>
        /// Creates a subtask.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSubtask([FromBody] CreateSubtaskRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Subtask request is required");
                }

                var subtaskId = await _subtaskService.CreateSubtaskAsync(
                    request.TaskId,
                    request.Title,
                    request.Description,
                    request.SubtaskType,
                    request.Priority,
                    request.Estimate,
                    request.AssigneeId,
                    request.Metadata);

                return Ok(new
                {
                    subtaskId,
                    message = "Subtask created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create subtask: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create subtask",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets subtask information.
        /// </summary>
        [HttpGet("{subtaskId}")]
        public async Task<IActionResult> GetSubtask(string subtaskId)
        {
            try
            {
                var subtask = await _subtaskService.GetSubtaskAsync(subtaskId);

                if (subtask == null)
                {
                    return NotFound(new
                    {
                        error = "Subtask not found",
                        subtaskId
                    });
                }

                return Ok(subtask);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get subtask {SubtaskId}: {Message}", subtaskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve subtask",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists subtasks.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListSubtasks(
            [FromQuery] string? taskId,
            [FromQuery] string? subtaskType,
            [FromQuery] string? status,
            [FromQuery] string? assigneeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var subtasks = await _subtaskService.ListSubtasksAsync(taskId, subtaskType, status, assigneeId, page, pageSize);
                var totalCount = await _subtaskService.GetSubtaskCountAsync(taskId, subtaskType, status, assigneeId);

                return Ok(new
                {
                    subtasks,
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
                _logger.LogError(ex, "Failed to list subtasks: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list subtasks",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a subtask.
        /// </summary>
        [HttpPut("{subtaskId}")]
        public async Task<IActionResult> UpdateSubtask(
            string subtaskId,
            [FromBody] UpdateSubtaskRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _subtaskService.UpdateSubtaskAsync(
                    subtaskId,
                    request.Title,
                    request.Description,
                    request.SubtaskType,
                    request.Priority,
                    request.Estimate,
                    request.AssigneeId,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Subtask updated successfully",
                    subtaskId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update subtask {SubtaskId}: {Message}", subtaskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update subtask",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a subtask.
        /// </summary>
        [HttpDelete("{subtaskId}")]
        public async Task<IActionResult> DeleteSubtask(string subtaskId)
        {
            try
            {
                await _subtaskService.DeleteSubtaskAsync(subtaskId);

                return Ok(new
                {
                    message = "Subtask deleted successfully",
                    subtaskId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete subtask {SubtaskId}: {Message}", subtaskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete subtask",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets subtask status.
        /// </summary>
        [HttpGet("{subtaskId}/status")]
        public async Task<IActionResult> GetSubtaskStatus(string subtaskId)
        {
            try
            {
                var status = await _subtaskService.GetSubtaskStatusAsync(subtaskId);

                return Ok(new
                {
                    subtaskId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get subtask status for {SubtaskId}: {Message}", subtaskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve subtask status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets subtask issues.
        /// </summary>
        [HttpGet("{subtaskId}/issues")]
        public async Task<IActionResult> GetSubtaskIssues(string subtaskId)
        {
            try
            {
                var issues = await _subtaskService.GetSubtaskIssuesAsync(subtaskId);

                return Ok(new
                {
                    subtaskId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get subtask issues for {SubtaskId}: {Message}", subtaskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve subtask issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets subtask statistics.
        /// </summary>
        [HttpGet("{subtaskId}/stats")]
        public async Task<IActionResult> GetSubtaskStats(string subtaskId)
        {
            try
            {
                var stats = await _subtaskService.GetSubtaskStatsAsync(subtaskId);

                return Ok(new
                {
                    subtaskId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get subtask stats for {SubtaskId}: {Message}", subtaskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve subtask statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available subtask types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetSubtaskTypes()
        {
            try
            {
                var subtaskTypes = await _subtaskService.GetSubtaskTypesAsync();

                return Ok(new
                {
                    subtaskTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get subtask types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve subtask types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create subtask request model.
    /// </summary>
    public class CreateSubtaskRequest
    {
        /// <summary>
        /// Task ID.
        /// </summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// Subtask title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Subtask description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Subtask type.
        /// </summary>
        public string SubtaskType { get; set; } = "development";

        /// <summary>
        /// Subtask priority.
        /// </summary>
        public string Priority { get; set; } = "medium";

        /// <summary>
        /// Subtask estimate.
        /// </summary>
        public int? Estimate { get; set; }

        /// <summary>
        /// Subtask assignee ID.
        /// </summary>
        public string? AssigneeId { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update subtask request model.
    /// </summary>
    public class UpdateSubtaskRequest
    {
        /// <summary>
        /// Subtask title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Subtask description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Subtask type.
        /// </summary>
        public string? SubtaskType { get; set; }

        /// <summary>
        /// Subtask priority.
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Subtask estimate.
        /// </summary>
        public int? Estimate { get; set; }

        /// <summary>
        /// Subtask assignee ID.
        /// </summary>
        public string? AssigneeId { get; set; }

        /// <summary>
        /// Subtask status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}