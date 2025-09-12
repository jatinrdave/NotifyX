using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log entry log entry log operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogEntryLogEntryLogController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogController> _logger;
        private readonly IWorkflowExecutionTriggerLogEntryLogEntryLogService _workflowExecutionTriggerLogEntryLogEntryLogService;

        public WorkflowExecutionTriggerLogEntryLogEntryLogController(ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogController> logger, IWorkflowExecutionTriggerLogEntryLogEntryLogService workflowExecutionTriggerLogEntryLogEntryLogService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogEntryLogEntryLogService = workflowExecutionTriggerLogEntryLogEntryLogService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogEntryLogEntryLogService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log entry log entry log.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLogEntryLogEntryLog([FromBody] CreateWorkflowExecutionTriggerLogEntryLogEntryLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log entry log entry log request is required");
                }

                var workflowExecutionTriggerLogEntryLogEntryLogId = await _workflowExecutionTriggerLogEntryLogEntryLogService.CreateWorkflowExecutionTriggerLogEntryLogEntryLogAsync(
                    request.WorkflowExecutionTriggerLogEntryLogEntryId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogId,
                    message = "Workflow execution trigger log entry log entry log created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log entry log entry log: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLog(string workflowExecutionTriggerLogEntryLogEntryLogId)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLog = await _workflowExecutionTriggerLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogAsync(workflowExecutionTriggerLogEntryLogEntryLogId);

                if (workflowExecutionTriggerLogEntryLogEntryLog == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log entry log entry log not found",
                        workflowExecutionTriggerLogEntryLogEntryLogId
                    });
                }

                return Ok(workflowExecutionTriggerLogEntryLogEntryLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger log entry log entry logs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogEntryLogEntryLogs(
            [FromQuery] string? workflowExecutionTriggerLogEntryLogEntryId,
            [FromQuery] string? logLevel,
            [FromQuery] string? logSource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogs = await _workflowExecutionTriggerLogEntryLogEntryLogService.ListWorkflowExecutionTriggerLogEntryLogEntryLogsAsync(workflowExecutionTriggerLogEntryLogEntryId, logLevel, logSource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogCountAsync(workflowExecutionTriggerLogEntryLogEntryId, logLevel, logSource);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogs,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger log entry log entry logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger log entry log entry logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log entry log entry log.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogEntryLogEntryLogId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLogEntryLogEntryLog(
            string workflowExecutionTriggerLogEntryLogEntryLogId,
            [FromBody] UpdateWorkflowExecutionTriggerLogEntryLogEntryLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogEntryLogEntryLogService.UpdateWorkflowExecutionTriggerLogEntryLogEntryLogAsync(
                    workflowExecutionTriggerLogEntryLogEntryLogId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log updated successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log entry log entry log.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogEntryLogEntryLogId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLogEntryLogEntryLog(string workflowExecutionTriggerLogEntryLogEntryLogId)
        {
            try
            {
                await _workflowExecutionTriggerLogEntryLogEntryLogService.DeleteWorkflowExecutionTriggerLogEntryLogEntryLogAsync(workflowExecutionTriggerLogEntryLogEntryLogId);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log deleted successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogStatus(string workflowExecutionTriggerLogEntryLogEntryLogId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogStatusAsync(workflowExecutionTriggerLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log status for {WorkflowExecutionTriggerLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogIssues(string workflowExecutionTriggerLogEntryLogEntryLogId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogIssuesAsync(workflowExecutionTriggerLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log issues for {WorkflowExecutionTriggerLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogStats(string workflowExecutionTriggerLogEntryLogEntryLogId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogStatsAsync(workflowExecutionTriggerLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log stats for {WorkflowExecutionTriggerLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log entry log entry log levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogLevels()
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogLevels = await _workflowExecutionTriggerLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log entry log entry log request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogEntryLogEntryLogRequest
    {
        /// <summary>
        /// Workflow execution trigger log entry log entry ID.
        /// </summary>
        public string WorkflowExecutionTriggerLogEntryLogEntryId { get; set; } = string.Empty;

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
    /// Update workflow execution trigger log entry log entry log request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogEntryLogEntryLogRequest
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