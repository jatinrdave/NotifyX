using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log entry log entry log entry log operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogController> _logger;
        private readonly IWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogService _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService;

        public WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogController(ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogController> logger, IWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogService workflowExecutionTriggerLogEntryLogEntryLogEntryLogService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService = workflowExecutionTriggerLogEntryLogEntryLogEntryLogService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogEntryLogEntryLogEntryLogService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log entry log entry log entry log.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLog([FromBody] CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log entry log entry log entry log request is required");
                }

                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogId = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogAsync(
                    request.WorkflowExecutionTriggerLogEntryLogEntryLogEntryId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
                    message = "Workflow execution trigger log entry log entry log entry log created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log entry log entry log entry log: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log entry log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLog(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLog = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogId);

                if (workflowExecutionTriggerLogEntryLogEntryLogEntryLog == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log entry log entry log entry log not found",
                        workflowExecutionTriggerLogEntryLogEntryLogEntryLogId
                    });
                }

                return Ok(workflowExecutionTriggerLogEntryLogEntryLogEntryLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger log entry log entry log entry logs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogs(
            [FromQuery] string? workflowExecutionTriggerLogEntryLogEntryLogEntryId,
            [FromQuery] string? logLevel,
            [FromQuery] string? logSource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogs = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.ListWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogsAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryId, logLevel, logSource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogCountAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryId, logLevel, logSource);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogs,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger log entry log entry log entry logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger log entry log entry log entry logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log entry log entry log entry log.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLog(
            string workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
            [FromBody] UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogAsync(
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log entry log updated successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log entry log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log entry log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log entry log entry log entry log.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLogEntryLogEntryLogEntryLog(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.DeleteWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogId);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log entry log deleted successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log entry log entry log entry log {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log entry log entry log entry log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogStatus(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogStatusAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log status for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogIssues(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogIssuesAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log issues for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogStats(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogStatsAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log stats for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log entry log entry log entry log levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogLevels()
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogLevels = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log entry log entry log entry log request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogRequest
    {
        /// <summary>
        /// Workflow execution trigger log entry log entry log entry ID.
        /// </summary>
        public string WorkflowExecutionTriggerLogEntryLogEntryLogEntryId { get; set; } = string.Empty;

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
    /// Update workflow execution trigger log entry log entry log entry log request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogRequest
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