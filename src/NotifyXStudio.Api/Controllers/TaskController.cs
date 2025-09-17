using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for task operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly ITaskService _taskService;

        public TaskController(ILogger<TaskController> logger, ITaskService taskService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        }

        /// <summary>
        /// Creates a task.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Task request is required");
                }

                var taskId = await _taskService.CreateTaskAsync(
                    request.Title,
                    request.Description,
                    request.TaskType,
                    request.Priority,
                    request.AssigneeId,
                    request.ProjectId,
                    null);

                return Ok(new
                {
                    taskId,
                    message = "Task created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create task: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create task",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets task information.
        /// </summary>
        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTask(string taskId)
        {
            try
            {
                var task = await _taskService.GetTaskAsync(taskId);

                if (task == null)
                {
                    return NotFound(new
                    {
                        error = "Task not found",
                        taskId
                    });
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get task {TaskId}: {Message}", taskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve task",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists tasks.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListTasks(
            [FromQuery] string? projectId,
            [FromQuery] string? taskType,
            [FromQuery] string? status,
            [FromQuery] string? assigneeId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var tasks = await _taskService.ListTasksAsync(projectId, status, taskType, assigneeId, page, pageSize);
                var totalCount = await _taskService.GetTaskCountAsync(projectId, status, taskType);

                return Ok(new
                {
                    tasks,
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
                _logger.LogError(ex, "Failed to list tasks: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list tasks",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a task.
        /// </summary>
        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(
            string taskId,
            [FromBody] UpdateTaskRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _taskService.UpdateTaskAsync(
                    taskId,
                    request.Title,
                    request.Description,
                    request.Status,
                    request.Priority,
                    request.AssigneeId,
                    null,
                    null);

                return Ok(new
                {
                    message = "Task updated successfully",
                    taskId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update task {TaskId}: {Message}", taskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update task",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a task.
        /// </summary>
        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(string taskId)
        {
            try
            {
                await _taskService.DeleteTaskAsync(taskId);

                return Ok(new
                {
                    message = "Task deleted successfully",
                    taskId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete task {TaskId}: {Message}", taskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete task",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets task status.
        /// </summary>
        [HttpGet("{taskId}/status")]
        public async Task<IActionResult> GetTaskStatus(string taskId)
        {
            try
            {
                var status = await _taskService.GetTaskStatusAsync(taskId);

                return Ok(new
                {
                    taskId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get task status for {TaskId}: {Message}", taskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve task status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets task issues.
        /// </summary>
        [HttpGet("{taskId}/issues")]
        public async Task<IActionResult> GetTaskIssues(string taskId)
        {
            try
            {
                var issues = await _taskService.GetTaskIssuesAsync(taskId);

                return Ok(new
                {
                    taskId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get task issues for {TaskId}: {Message}", taskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve task issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets task statistics.
        /// </summary>
        [HttpGet("{taskId}/stats")]
        public async Task<IActionResult> GetTaskStats(string taskId)
        {
            try
            {
                var stats = await _taskService.GetTaskStatsAsync(taskId);

                return Ok(new
                {
                    taskId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get task stats for {TaskId}: {Message}", taskId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve task statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available task types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetTaskTypes()
        {
            try
            {
                var taskTypes = await _taskService.GetTaskTypesAsync();

                return Ok(new
                {
                    taskTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get task types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve task types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create task request model.
    /// </summary>
    public class CreateTaskRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Task title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Task description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Task type.
        /// </summary>
        public string TaskType { get; set; } = "development";

        /// <summary>
        /// Task priority.
        /// </summary>
        public string Priority { get; set; } = "medium";

        /// <summary>
        /// Task estimate.
        /// </summary>
        public int? Estimate { get; set; }

        /// <summary>
        /// Task assignee ID.
        /// </summary>
        public string? AssigneeId { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update task request model.
    /// </summary>
    public class UpdateTaskRequest
    {
        /// <summary>
        /// Task title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Task description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Task type.
        /// </summary>
        public string? TaskType { get; set; }

        /// <summary>
        /// Task priority.
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Task estimate.
        /// </summary>
        public int? Estimate { get; set; }

        /// <summary>
        /// Task assignee ID.
        /// </summary>
        public string? AssigneeId { get; set; }

        /// <summary>
        /// Task status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}