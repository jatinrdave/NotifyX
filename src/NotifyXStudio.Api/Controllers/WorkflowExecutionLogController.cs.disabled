using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution log operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionLogController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionLogController> _logger;
        private readonly IWorkflowExecutionLogService _workflowExecutionLogService;

        public WorkflowExecutionLogController(ILogger<WorkflowExecutionLogController> logger, IWorkflowExecutionLogService workflowExecutionLogService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionLogService = workflowExecutionLogService ?? throw new ArgumentNullException(nameof(workflowExecutionLogService));
        }

        /// <summary>
        /// Creates a workflow execution log.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionLog([FromBody] CreateWorkflowExecutionLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution log request is required");
                }

                var workflowExecutionLogId = await _workflowExecutionLogService.CreateWorkflowExecutionLogAsync(
                    request.WorkflowExecutionId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionLogId,
                    message = "Workflow execution log created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution log: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution log information.
        /// </summary>
        [HttpGet("{workflowExecutionLogId}")]
        public async Task<IActionResult> GetWorkflowExecutionLog(string workflowExecutionLogId)
        {
            try
            {
                var workflowExecutionLog = await _workflowExecutionLogService.GetWorkflowExecutionLogAsync(workflowExecutionLogId);

                if (workflowExecutionLog == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution log not found",
                        workflowExecutionLogId
                    });
                }

                return Ok(workflowExecutionLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution log {WorkflowExecutionLogId}: {Message}", workflowExecutionLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution logs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionLogs(
            [FromQuery] string? workflowExecutionId,
            [FromQuery] string? logLevel,
            [FromQuery] string? logSource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionLogs = await _workflowExecutionLogService.ListWorkflowExecutionLogsAsync(workflowExecutionId, logLevel, logSource, page, pageSize);
                var totalCount = await _workflowExecutionLogService.GetWorkflowExecutionLogCountAsync(workflowExecutionId, logLevel, logSource);

                return Ok(new
                {
                    workflowExecutionLogs,
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
                _logger.LogError(ex, "Failed to list workflow execution logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution log.
        /// </summary>
        [HttpPut("{workflowExecutionLogId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionLog(
            string workflowExecutionLogId,
            [FromBody] UpdateWorkflowExecutionLogRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionLogService.UpdateWorkflowExecutionLogAsync(
                    workflowExecutionLogId,
                    request.LogLevel,
                    request.LogMessage,
                    request.LogData,
                    request.LogSource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution log updated successfully",
                    workflowExecutionLogId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution log {WorkflowExecutionLogId}: {Message}", workflowExecutionLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution log.
        /// </summary>
        [HttpDelete("{workflowExecutionLogId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionLog(string workflowExecutionLogId)
        {
            try
            {
                await _workflowExecutionLogService.DeleteWorkflowExecutionLogAsync(workflowExecutionLogId);

                return Ok(new
                {
                    message = "Workflow execution log deleted successfully",
                    workflowExecutionLogId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution log {WorkflowExecutionLogId}: {Message}", workflowExecutionLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution log",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution log status.
        /// </summary>
        [HttpGet("{workflowExecutionLogId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionLogStatus(string workflowExecutionLogId)
        {
            try
            {
                var status = await _workflowExecutionLogService.GetWorkflowExecutionLogStatusAsync(workflowExecutionLogId);

                return Ok(new
                {
                    workflowExecutionLogId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution log status for {WorkflowExecutionLogId}: {Message}", workflowExecutionLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution log status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution log issues.
        /// </summary>
        [HttpGet("{workflowExecutionLogId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionLogIssues(string workflowExecutionLogId)
        {
            try
            {
                var issues = await _workflowExecutionLogService.GetWorkflowExecutionLogIssuesAsync(workflowExecutionLogId);

                return Ok(new
                {
                    workflowExecutionLogId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution log issues for {WorkflowExecutionLogId}: {Message}", workflowExecutionLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution log issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution log statistics.
        /// </summary>
        [HttpGet("{workflowExecutionLogId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionLogStats(string workflowExecutionLogId)
        {
            try
            {
                var stats = await _workflowExecutionLogService.GetWorkflowExecutionLogStatsAsync(workflowExecutionLogId);

                return Ok(new
                {
                    workflowExecutionLogId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution log stats for {WorkflowExecutionLogId}: {Message}", workflowExecutionLogId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution log statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution log levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionLogLevels()
        {
            try
            {
                var workflowExecutionLogLevels = await _workflowExecutionLogService.GetWorkflowExecutionLogLevelsAsync();

                return Ok(new
                {
                    workflowExecutionLogLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution log levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution log levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution log request model.
    /// </summary>
    public class CreateWorkflowExecutionLogRequest
    {
        /// <summary>
        /// Workflow execution ID.
        /// </summary>
        public string WorkflowExecutionId { get; set; } = string.Empty;

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
        public string LogSource { get; set; } = "workflow";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow execution log request model.
    /// </summary>
    public class UpdateWorkflowExecutionLogRequest
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