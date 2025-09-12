using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log entry operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogEntryController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogEntryController> _logger;
        private readonly IWorkflowExecutionTriggerLogEntryService _workflowExecutionTriggerLogEntryService;

        public WorkflowExecutionTriggerLogEntryController(ILogger<WorkflowExecutionTriggerLogEntryController> logger, IWorkflowExecutionTriggerLogEntryService workflowExecutionTriggerLogEntryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogEntryService = workflowExecutionTriggerLogEntryService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogEntryService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log entry.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLogEntry([FromBody] CreateWorkflowExecutionTriggerLogEntryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log entry request is required");
                }

                var workflowExecutionTriggerLogEntryId = await _workflowExecutionTriggerLogEntryService.CreateWorkflowExecutionTriggerLogEntryAsync(
                    request.WorkflowExecutionTriggerLogId,
                    request.LogEntryLevel,
                    request.LogEntryMessage,
                    request.LogEntryData,
                    request.LogEntrySource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryId,
                    message = "Workflow execution trigger log entry created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log entry: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntry(string workflowExecutionTriggerLogEntryId)
        {
            try
            {
                var workflowExecutionTriggerLogEntry = await _workflowExecutionTriggerLogEntryService.GetWorkflowExecutionTriggerLogEntryAsync(workflowExecutionTriggerLogEntryId);

                if (workflowExecutionTriggerLogEntry == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log entry not found",
                        workflowExecutionTriggerLogEntryId
                    });
                }

                return Ok(workflowExecutionTriggerLogEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry {WorkflowExecutionTriggerLogEntryId}: {Message}", workflowExecutionTriggerLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger log entries.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogEntries(
            [FromQuery] string? workflowExecutionTriggerLogId,
            [FromQuery] string? logEntryLevel,
            [FromQuery] string? logEntrySource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogEntries = await _workflowExecutionTriggerLogEntryService.ListWorkflowExecutionTriggerLogEntriesAsync(workflowExecutionTriggerLogId, logEntryLevel, logEntrySource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogEntryService.GetWorkflowExecutionTriggerLogEntryCountAsync(workflowExecutionTriggerLogId, logEntryLevel, logEntrySource);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntries,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger log entries: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger log entries",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log entry.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogEntryId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLogEntry(
            string workflowExecutionTriggerLogEntryId,
            [FromBody] UpdateWorkflowExecutionTriggerLogEntryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogEntryService.UpdateWorkflowExecutionTriggerLogEntryAsync(
                    workflowExecutionTriggerLogEntryId,
                    request.LogEntryLevel,
                    request.LogEntryMessage,
                    request.LogEntryData,
                    request.LogEntrySource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry updated successfully",
                    workflowExecutionTriggerLogEntryId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log entry {WorkflowExecutionTriggerLogEntryId}: {Message}", workflowExecutionTriggerLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log entry.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogEntryId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLogEntry(string workflowExecutionTriggerLogEntryId)
        {
            try
            {
                await _workflowExecutionTriggerLogEntryService.DeleteWorkflowExecutionTriggerLogEntryAsync(workflowExecutionTriggerLogEntryId);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry deleted successfully",
                    workflowExecutionTriggerLogEntryId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log entry {WorkflowExecutionTriggerLogEntryId}: {Message}", workflowExecutionTriggerLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryStatus(string workflowExecutionTriggerLogEntryId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogEntryService.GetWorkflowExecutionTriggerLogEntryStatusAsync(workflowExecutionTriggerLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry status for {WorkflowExecutionTriggerLogEntryId}: {Message}", workflowExecutionTriggerLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryIssues(string workflowExecutionTriggerLogEntryId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogEntryService.GetWorkflowExecutionTriggerLogEntryIssuesAsync(workflowExecutionTriggerLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry issues for {WorkflowExecutionTriggerLogEntryId}: {Message}", workflowExecutionTriggerLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryStats(string workflowExecutionTriggerLogEntryId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogEntryService.GetWorkflowExecutionTriggerLogEntryStatsAsync(workflowExecutionTriggerLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry stats for {WorkflowExecutionTriggerLogEntryId}: {Message}", workflowExecutionTriggerLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log entry levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLevels()
        {
            try
            {
                var workflowExecutionTriggerLogEntryLevels = await _workflowExecutionTriggerLogEntryService.GetWorkflowExecutionTriggerLogEntryLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log entry request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogEntryRequest
    {
        /// <summary>
        /// Workflow execution trigger log ID.
        /// </summary>
        public string WorkflowExecutionTriggerLogId { get; set; } = string.Empty;

        /// <summary>
        /// Log entry level.
        /// </summary>
        public string LogEntryLevel { get; set; } = "info";

        /// <summary>
        /// Log entry message.
        /// </summary>
        public string LogEntryMessage { get; set; } = string.Empty;

        /// <summary>
        /// Log entry data.
        /// </summary>
        public Dictionary<string, object>? LogEntryData { get; set; }

        /// <summary>
        /// Log entry source.
        /// </summary>
        public string LogEntrySource { get; set; } = "trigger";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow execution trigger log entry request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogEntryRequest
    {
        /// <summary>
        /// Log entry level.
        /// </summary>
        public string? LogEntryLevel { get; set; }

        /// <summary>
        /// Log entry message.
        /// </summary>
        public string? LogEntryMessage { get; set; }

        /// <summary>
        /// Log entry data.
        /// </summary>
        public Dictionary<string, object>? LogEntryData { get; set; }

        /// <summary>
        /// Log entry source.
        /// </summary>
        public string? LogEntrySource { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}