using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger log entry log entry log entry log entry operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryController> _logger;
        private readonly IWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService;

        public WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryController(ILogger<WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryController> logger, IWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService = workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService));
        }

        /// <summary>
        /// Creates a workflow execution trigger log entry log entry log entry log entry.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntry([FromBody] CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger log entry log entry log entry log entry request is required");
                }

                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryAsync(
                    request.WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
                    request.LogEntryLevel,
                    request.LogEntryMessage,
                    request.LogEntryData,
                    request.LogEntrySource,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
                    message = "Workflow execution trigger log entry log entry log entry log entry created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger log entry log entry log entry log entry: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger log entry log entry log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log entry information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntry(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntry = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId);

                if (workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntry == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger log entry log entry log entry log entry not found",
                        workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId
                    });
                }

                return Ok(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution trigger log entry log entry log entry log entries.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntries(
            [FromQuery] string? workflowExecutionTriggerLogEntryLogEntryLogEntryLogId,
            [FromQuery] string? logEntryLevel,
            [FromQuery] string? logEntrySource,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntries = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.ListWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntriesAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogId, logEntryLevel, logEntrySource, page, pageSize);
                var totalCount = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryCountAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogId, logEntryLevel, logEntrySource);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntries,
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
                _logger.LogError(ex, "Failed to list workflow execution trigger log entry log entry log entry log entries: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution trigger log entry log entry log entry log entries",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger log entry log entry log entry log entry.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntry(
            string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
            [FromBody] UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryAsync(
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
                    request.LogEntryLevel,
                    request.LogEntryMessage,
                    request.LogEntryData,
                    request.LogEntrySource,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log entry log entry updated successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger log entry log entry log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger log entry log entry log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger log entry log entry log entry log entry.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntry(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId)
        {
            try
            {
                await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.DeleteWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId);

                return Ok(new
                {
                    message = "Workflow execution trigger log entry log entry log entry log entry deleted successfully",
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger log entry log entry log entry log entry {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger log entry log entry log entry log entry",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log entry status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryStatus(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId)
        {
            try
            {
                var status = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryStatusAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry status for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log entry issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryIssues(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryIssuesAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry issues for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger log entry log entry log entry log entry statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryStats(string workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryStatsAsync(workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId);

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry stats for {WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId}: {Message}", workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger log entry log entry log entry log entry levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLevels()
        {
            try
            {
                var workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLevels = await _workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryService.GetWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLevelsAsync();

                return Ok(new
                {
                    workflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryLevels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger log entry log entry log entry log entry levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger log entry log entry log entry log entry levels",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger log entry log entry log entry log entry request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryRequest
    {
        /// <summary>
        /// Workflow execution trigger log entry log entry log entry log ID.
        /// </summary>
        public string WorkflowExecutionTriggerLogEntryLogEntryLogEntryLogId { get; set; } = string.Empty;

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
    /// Update workflow execution trigger log entry log entry log entry log entry request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerLogEntryLogEntryLogEntryLogEntryRequest
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