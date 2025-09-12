using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log entry log operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogEntryLogController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogEntryLogController> _logger;
        private readonly IWorkflowExecutionTriggerLogEntryLogService _workflowExecutionTriggerLogEntryLogService;

        public WorkflowExecutionTriggerLogEntryLogController(ILogger<WorkflowExecutionTriggerLogEntryLogController> logger, IWorkflowExecutionTriggerLogEntryLogService workflowExecutionTriggerLogEntryLogService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogEntryLogService = workflowExecutionTriggerLogEntryLogService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogEntryLogService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log entry log.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLogEntryLog([FromBody] CreateWorkflowExecutionTriggerLogEntryLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log entry log request is required");
                }

                var workflowExecutionTriggerLogEntryLogId = await _workflowExecutionTriggerLogEntryLogService.CreateWorkflowExecutionTriggerLogEntryLogAsync(
                    request.WorkflowExecutionTriggerLogEntryId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogId,
                    message = "Workflow execution trigger log entry log created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log entry log: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLog(string workflowExecutionTriggerLogEntryLogId)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLog = await _workflowExecutionTriggerLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogAsync(workflowExecutionTriggerLogEntryLogId);

                if (workflowExecutionTriggerLogEntryLog == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log entry log not found",
                        workflowExecutionTriggerLogEntryLogId
                    });
                }

                return Ok(workflowExecutionTriggerLogEntryLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log {WorkflowExecutionTriggerLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger log entry logs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogEntryLogs(
            [FromQuery] string? workflowExecutionTriggerLogEntryId,
            [FromQuery] string? logLevel,
            [FromQuery] string? logSource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogs = await _workflowExecutionTriggerLogEntryLogService.ListWorkflowExecutionTriggerLogEntryLogsAsync(workflowExecutionTriggerLogEntryId, logLevel, logSource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogCountAsync(workflowExecutionTriggerLogEntryId, logLevel, logSource);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogs,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger log entry logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger log entry logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log entry log.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogEntryLogId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLogEntryLog(
            string workflowExecutionTriggerLogEntryLogId,
            [FromBody] UpdateWorkflowExecutionTriggerLogEntryLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogEntryLogService.UpdateWorkflowExecutionTriggerLogEntryLogAsync(
                    workflowExecutionTriggerLogEntryLogId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log updated successfully",
                    workflowExecutionTriggerLogEntryLogId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log entry log {WorkflowExecutionTriggerLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log entry log.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogEntryLogId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLogEntryLog(string workflowExecutionTriggerLogEntryLogId)
        {
            try
            {
                await _workflowExecutionTriggerLogEntryLogService.DeleteWorkflowExecutionTriggerLogEntryLogAsync(workflowExecutionTriggerLogEntryLogId);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log deleted successfully",
                    workflowExecutionTriggerLogEntryLogId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log entry log {WorkflowExecutionTriggerLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogStatus(string workflowExecutionTriggerLogEntryLogId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogStatusAsync(workflowExecutionTriggerLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log status for {WorkflowExecutionTriggerLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogIssues(string workflowExecutionTriggerLogEntryLogId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogIssuesAsync(workflowExecutionTriggerLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log issues for {WorkflowExecutionTriggerLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogStats(string workflowExecutionTriggerLogEntryLogId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogStatsAsync(workflowExecutionTriggerLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log stats for {WorkflowExecutionTriggerLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log entry log levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogLevels()
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogLevels = await _workflowExecutionTriggerLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log entry log request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogEntryLogRequest
    {
        /// <summary>
        /// Workflow execution trigger log entry ID.
        /// </summary>
        public string WorkflowExecutionTriggerLogEntryId { get; set; } = string.Empty;

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
    /// Update workflow execution trigger log entry log request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogEntryLogRequest
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