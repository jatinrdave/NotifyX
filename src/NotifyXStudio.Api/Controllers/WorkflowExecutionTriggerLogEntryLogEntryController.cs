using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log entry log entry operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogEntryLogEntryController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogEntryLogEntryController> _logger;
        private readonly IWorkflowExecutionTriggerLogEntryLogEntryService _workflowExecutionTriggerLogEntryLogEntryService;

        public WorkflowExecutionTriggerLogEntryLogEntryController(ILogger<WorkflowExecutionTriggerLogEntryLogEntryController> logger, IWorkflowExecutionTriggerLogEntryLogEntryService workflowExecutionTriggerLogEntryLogEntryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogEntryLogEntryService = workflowExecutionTriggerLogEntryLogEntryService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogEntryLogEntryService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log entry log entry.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLogEntryLogEntry([FromBody] CreateWorkflowExecutionTriggerLogEntryLogEntryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log entry log entry request is required");
                }

                var workflowExecutionTriggerLogEntryLogEntryId = await _workflowExecutionTriggerLogEntryLogEntryService.CreateWorkflowExecutionTriggerLogEntryLogEntryAsync(
                    request.WorkflowExecutionTriggerLogEntryLogId,
                    request.LogEntryLevel,
                    request.LogEntryMessage,
                    request.LogEntryData,
                    request.LogEntrySource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryId,
                    message = "Workflow execution trigger log entry log entry created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log entry log entry: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntry(string workflowExecutionTriggerLogEntryLogEntryId)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntry = await _workflowExecutionTriggerLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryAsync(workflowExecutionTriggerLogEntryLogEntryId);

                if (workflowExecutionTriggerLogEntryLogEntry == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log entry log entry not found",
                        workflowExecutionTriggerLogEntryLogEntryId
                    });
                }

                return Ok(workflowExecutionTriggerLogEntryLogEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger log entry log entries.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogEntryLogEntries(
            [FromQuery] string? workflowExecutionTriggerLogEntryLogId,
            [FromQuery] string? logEntryLevel,
            [FromQuery] string? logEntrySource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntries = await _workflowExecutionTriggerLogEntryLogEntryService.ListWorkflowExecutionTriggerLogEntryLogEntriesAsync(workflowExecutionTriggerLogEntryLogId, logEntryLevel, logEntrySource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryCountAsync(workflowExecutionTriggerLogEntryLogId, logEntryLevel, logEntrySource);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntries,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger log entry log entries: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger log entry log entries",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log entry log entry.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogEntryLogEntryId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLogEntryLogEntry(
            string workflowExecutionTriggerLogEntryLogEntryId,
            [FromBody] UpdateWorkflowExecutionTriggerLogEntryLogEntryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogEntryLogEntryService.UpdateWorkflowExecutionTriggerLogEntryLogEntryAsync(
                    workflowExecutionTriggerLogEntryLogEntryId,
                    request.LogEntryLevel,
                    request.LogEntryMessage,
                    request.LogEntryData,
                    request.LogEntrySource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry updated successfully",
                    workflowExecutionTriggerLogEntryLogEntryId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log entry log entry.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogEntryLogEntryId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLogEntryLogEntry(string workflowExecutionTriggerLogEntryLogEntryId)
        {
            try
            {
                await _workflowExecutionTriggerLogEntryLogEntryService.DeleteWorkflowExecutionTriggerLogEntryLogEntryAsync(workflowExecutionTriggerLogEntryLogEntryId);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry deleted successfully",
                    workflowExecutionTriggerLogEntryLogEntryId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryStatus(string workflowExecutionTriggerLogEntryLogEntryId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryStatusAsync(workflowExecutionTriggerLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry status for {WorkflowExecutionTriggerLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryIssues(string workflowExecutionTriggerLogEntryLogEntryId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryIssuesAsync(workflowExecutionTriggerLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry issues for {WorkflowExecutionTriggerLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryStats(string workflowExecutionTriggerLogEntryLogEntryId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryStatsAsync(workflowExecutionTriggerLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry stats for {WorkflowExecutionTriggerLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log entry log entry levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLevels()
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLevels = await _workflowExecutionTriggerLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log entry log entry request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogEntryLogEntryRequest
    {
        /// <summary>
        /// Workflow execution trigger log entry log ID.
        /// </summary>
        public string WorkflowExecutionTriggerLogEntryLogId { get; set; } = string.Empty;

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
    /// Update workflow execution trigger log entry log entry request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogEntryLogEntryRequest
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