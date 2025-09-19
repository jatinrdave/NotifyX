using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogController> _logger;
        private readonly IWorkflowExecutionTriggerLogService _workflowExecutionTriggerLogService;

        public WorkflowExecutionTriggerLogController(ILogger<WorkflowExecutionTriggerLogController> logger, IWorkflowExecutionTriggerLogService workflowExecutionTriggerLogService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogService = workflowExecutionTriggerLogService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLog([FromBody] CreateWorkflowExecutionTriggerLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log request is required");
                }

                var workflowExecutionTriggerLogId = await _workflowExecutionTriggerLogService.CreateWorkflowExecutionTriggerLogAsync(
                    request.WorkflowExecutionTriggerId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogId,
                    message = "Workflow execution trigger log created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLog(string workflowExecutionTriggerLogId)
        {
            try
            {
                var workflowExecutionTriggerLog = await _workflowExecutionTriggerLogService.GetWorkflowExecutionTriggerLogAsync(workflowExecutionTriggerLogId);

                if (workflowExecutionTriggerLog == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log not found",
                        workflowExecutionTriggerLogId
                    });
                }

                return Ok(workflowExecutionTriggerLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log {WorkflowExecutionTriggerLogId}: {Message}", workflowExecutionTriggerLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger logs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogs(
            [FromQuery] string? workflowExecutionTriggerId,
            [FromQuery] string? logLevel,
            [FromQuery] string? logSource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogs = await _workflowExecutionTriggerLogService.ListWorkflowExecutionTriggerLogsAsync(workflowExecutionTriggerId, logLevel, logSource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogService.GetWorkflowExecutionTriggerLogCountAsync(workflowExecutionTriggerId, logLevel, logSource);

                return Ok(new
                {
                    workflowExecutionTriggerLogs,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLog(
            string workflowExecutionTriggerLogId,
            [FromBody] UpdateWorkflowExecutionTriggerLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogService.UpdateWorkflowExecutionTriggerLogAsync(
                    workflowExecutionTriggerLogId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log updated successfully",
                    workflowExecutionTriggerLogId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log {WorkflowExecutionTriggerLogId}: {Message}", workflowExecutionTriggerLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLog(string workflowExecutionTriggerLogId)
        {
            try
            {
                await _workflowExecutionTriggerLogService.DeleteWorkflowExecutionTriggerLogAsync(workflowExecutionTriggerLogId);

                return Ok(new
                {
                    message = "Workflow execution trigger log deleted successfully",
                    workflowExecutionTriggerLogId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log {WorkflowExecutionTriggerLogId}: {Message}", workflowExecutionTriggerLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogStatus(string workflowExecutionTriggerLogId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogService.GetWorkflowExecutionTriggerLogStatusAsync(workflowExecutionTriggerLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log status for {WorkflowExecutionTriggerLogId}: {Message}", workflowExecutionTriggerLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogIssues(string workflowExecutionTriggerLogId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogService.GetWorkflowExecutionTriggerLogIssuesAsync(workflowExecutionTriggerLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log issues for {WorkflowExecutionTriggerLogId}: {Message}", workflowExecutionTriggerLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogStats(string workflowExecutionTriggerLogId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogService.GetWorkflowExecutionTriggerLogStatsAsync(workflowExecutionTriggerLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log stats for {WorkflowExecutionTriggerLogId}: {Message}", workflowExecutionTriggerLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogLevels()
        {
            try
            {
                var workflowExecutionTriggerLogLevels = await _workflowExecutionTriggerLogService.GetWorkflowExecutionTriggerLogLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogRequest
    {
        /// <summary>
        /// Workflow execution trigger ID.
        /// </summary>
        public string WorkflowExecutionTriggerId { get; set; } = string.Empty;

        /// <summary>
        /// Log level.
        /// </summary>
        public string LogLevel { get; set; } = "info";

        /// <summary>
        /// Log message.
        /// </summary>
        public string LogMessage { get; set; } = string.Empty;

        /// <summary>
        /// Log data.
        /// </summary>
        public Dictionary<string, object>? LogData { get; set; }

        /// <summary>
        /// Log source.
        /// </summary>
        public string LogSource { get; set; } = "trigger";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow execution trigger log request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogRequest
    {
        /// <summary>
        /// Log level.
        /// </summary>
        public string? LogLevel { get; set; }

        /// <summary>
        /// Log message.
        /// </summary>
        public string? LogMessage { get; set; }

        /// <summary>
        /// Log data.
        /// </summary>
        public Dictionary<string, object>? LogData { get; set; }

        /// <summary>
        /// Log source.
        /// </summary>
        public string? LogSource { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}