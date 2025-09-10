using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log entry log entry log entry operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogEntryLogEntryLogEntryController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogEntryController> _logger;
        private readonly IWorkflowExecutionTriggerLogEntryLogEntryLogEntryService _workflowExecutionTriggerLogEntryLogEntryLogEntryService;

        public WorkflowExecutionTriggerLogEntryLogEntryLogEntryController(ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogEntryController> logger, IWorkflowExecutionTriggerLogEntryLogEntryLogEntryService workflowExecutionTriggerLogEntryLogEntryLogEntryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogEntryLogEntryLogEntryService = workflowExecutionTriggerLogEntryLogEntryLogEntryService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogEntryLogEntryLogEntryService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log entry log entry log entry.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntry([FromBody] CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log entry log entry log entry request is required");
                }

                var workflowExecutionTriggerLogEntryLogEntryLogEntryId = await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryAsync(
                    request.WorkflowExecutionTriggerLogEntryLogEntryLogId,
                    request.LogEntryLevel,
                    request.LogEntryMessage,
                    request.LogEntryData,
                    request.LogEntrySource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryId,
                    message = "Workflow execution trigger log entry log entry log entry created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log entry log entry log entry: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log entry log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntry(string workflowExecutionTriggerLogEntryLogEntryLogEntryId)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntry = await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryId);

                if (workflowExecutionTriggerLogEntryLogEntryLogEntry == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log entry log entry log entry not found",
                        workflowExecutionTriggerLogEntryLogEntryLogEntryId
                    });
                }

                return Ok(workflowExecutionTriggerLogEntryLogEntryLogEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger log entry log entry log entries.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogEntryLogEntryLogEntries(
            [FromQuery] string? workflowExecutionTriggerLogEntryLogEntryLogId,
            [FromQuery] string? logEntryLevel,
            [FromQuery] string? logEntrySource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntries = await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.ListWorkflowExecutionTriggerLogEntryLogEntryLogEntriesAsync(workflowExecutionTriggerLogEntryLogEntryLogId, logEntryLevel, logEntrySource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryCountAsync(workflowExecutionTriggerLogEntryLogEntryLogId, logEntryLevel, logEntrySource);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntries,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger log entry log entry log entries: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger log entry log entry log entries",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log entry log entry log entry.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogEntryLogEntryLogEntryId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntry(
            string workflowExecutionTriggerLogEntryLogEntryLogEntryId,
            [FromBody] UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryAsync(
                    workflowExecutionTriggerLogEntryLogEntryLogEntryId,
                    request.LogEntryLevel,
                    request.LogEntryMessage,
                    request.LogEntryData,
                    request.LogEntrySource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log entry updated successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogEntryId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log entry log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log entry log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log entry log entry log entry.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogEntryLogEntryLogEntryId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLogEntryLogEntryLogEntry(string workflowExecutionTriggerLogEntryLogEntryLogEntryId)
        {
            try
            {
                await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.DeleteWorkflowExecutionTriggerLogEntryLogEntryLogEntryAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryId);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log entry deleted successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogEntryId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log entry log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log entry log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryStatus(string workflowExecutionTriggerLogEntryLogEntryLogEntryId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryStatusAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry status for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryIssues(string workflowExecutionTriggerLogEntryLogEntryLogEntryId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryIssuesAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry issues for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryStats(string workflowExecutionTriggerLogEntryLogEntryLogEntryId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryStatsAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry stats for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log entry log entry log entry levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLevels()
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLevels = await _workflowExecutionTriggerLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log entry log entry log entry request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryRequest
    {
        /// <summary>
        /// Workflow execution trigger log entry log entry log ID.
        /// </summary>
        public string WorkflowExecutionTriggerLogEntryLogEntryLogId { get; set; } = string.Empty;

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
    /// Update workflow execution trigger log entry log entry log entry request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryRequest
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