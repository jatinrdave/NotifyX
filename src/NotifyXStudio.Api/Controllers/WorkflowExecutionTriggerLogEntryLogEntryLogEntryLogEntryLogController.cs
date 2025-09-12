using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log entry log entry log entry log entry log operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogController> _logger;
        private readonly IWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService;

        public WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogController(ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogController> logger, IWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService = workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log entry log entry log entry log entry log.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLog([FromBody] CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log entry log entry log entry log entry log request is required");
                }

                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogAsync(
                    request.WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId,
                    message = "Workflow execution trigger log entry log entry log entry log entry log created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log entry log entry log entry log entry log: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log entry log entry log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log entry log information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLog(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLog = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId);

                if (workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLog == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log entry log entry log entry log entry log not found",
                        workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId
                    });
                }

                return Ok(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger log entry log entry log entry log entry logs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogs(
            [FromQuery] string? workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
            [FromQuery] string? logLevel,
            [FromQuery] string? logSource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogs = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.ListWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogsAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId, logLevel, logSource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogCountAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId, logLevel, logSource);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogs,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger log entry log entry log entry log entry logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger log entry log entry log entry log entry logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log entry log entry log entry log entry log.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLog(
            string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId,
            [FromBody] UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogAsync(
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log entry log entry log updated successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log entry log entry log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log entry log entry log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log entry log entry log entry log entry log.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLog(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.DeleteWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log entry log entry log deleted successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log entry log entry log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log entry log entry log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log entry log status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogStatus(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogStatusAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry log status for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry log status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log entry log issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogIssues(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogIssuesAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry log issues for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry log issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log entry log statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogStats(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogStatsAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry log stats for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry log statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log entry log entry log entry log entry log levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogLevels()
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogLevels = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry log levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry log levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log entry log entry log entry log entry log request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogRequest
    {
        /// <summary>
        /// Workflow execution trigger log entry log entry log entry log entry ID.
        /// </summary>
        public string WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId { get; set; } = string.Empty;

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
    /// Update workflow execution trigger log entry log entry log entry log entry log request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLogRequest
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