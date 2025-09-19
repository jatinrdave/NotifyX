using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution trigger operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionTriggerController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionTriggerController> _logger;
        private readonly IWorkflowExecutionTriggerService _workflowExecutionTriggerService;

        public WorkflowExecutionTriggerController(ILogger<WorkflowExecutionTriggerController> logger, IWorkflowExecutionTriggerService workflowExecutionTriggerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionTriggerService = workflowExecutionTriggerService ?? throw new ArgumentNullException(nameof(workflowExecutionTriggerService));
        }

        /// <summary>
        /// Creates a workflow execution trigger.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionTrigger([FromBody] CreateWorkflowExecutionTriggerRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution trigger request is required");
                }

                var workflowExecutionTriggerId = await _workflowExecutionTriggerService.CreateWorkflowExecutionTriggerAsync(
                    request.WorkflowExecutionId,
                    request.TriggerId,
                    request.TriggerType,
                    request.TriggerStatus,
                    request.TriggerConfig,
                    request.TriggerStartTime,
                    request.TriggerEndTime,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionTriggerId,
                    message = "Workflow execution trigger created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution trigger: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution trigger",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger information.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerId}")]
        public async Task<IActionResult> GetWorkflowExecutionTrigger(string workflowExecutionTriggerId)
        {
            try
            {
                var workflowExecutionTrigger = await _workflowExecutionTriggerService.GetWorkflowExecutionTriggerAsync(workflowExecutionTriggerId);

                if (workflowExecutionTrigger == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution trigger not found",
                        workflowExecutionTriggerId
                    });
                }

                return Ok(workflowExecutionTrigger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger {WorkflowExecutionTriggerId}: {Message}", workflowExecutionTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution triggers.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionTriggers(
            [FromQuery] string? workflowExecutionId,
            [FromQuery] string? triggerId,
            [FromQuery] string? triggerType,
            [FromQuery] string? triggerStatus,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionTriggers = await _workflowExecutionTriggerService.ListWorkflowExecutionTriggersAsync(workflowExecutionId, triggerId, triggerType, triggerStatus, page, pageSize);
                var totalCount = await _workflowExecutionTriggerService.GetWorkflowExecutionTriggerCountAsync(workflowExecutionId, triggerId, triggerType, triggerStatus);

                return Ok(new
                {
                    workflowExecutionTriggers,
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
                _logger.LogError(ex, "Failed to list workflow execution triggers: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution triggers",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution trigger.
        /// </summary>
        [HttpPut("{workflowExecutionTriggerId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionTrigger(
            string workflowExecutionTriggerId,
            [FromBody] UpdateWorkflowExecutionTriggerRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionTriggerService.UpdateWorkflowExecutionTriggerAsync(
                    workflowExecutionTriggerId,
                    request.TriggerId,
                    request.TriggerType,
                    request.TriggerStatus,
                    request.TriggerConfig,
                    request.TriggerStartTime,
                    request.TriggerEndTime,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution trigger updated successfully",
                    workflowExecutionTriggerId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution trigger {WorkflowExecutionTriggerId}: {Message}", workflowExecutionTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution trigger",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution trigger.
        /// </summary>
        [HttpDelete("{workflowExecutionTriggerId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionTrigger(string workflowExecutionTriggerId)
        {
            try
            {
                await _workflowExecutionTriggerService.DeleteWorkflowExecutionTriggerAsync(workflowExecutionTriggerId);

                return Ok(new
                {
                    message = "Workflow execution trigger deleted successfully",
                    workflowExecutionTriggerId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution trigger {WorkflowExecutionTriggerId}: {Message}", workflowExecutionTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution trigger",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger status.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerStatus(string workflowExecutionTriggerId)
        {
            try
            {
                var status = await _workflowExecutionTriggerService.GetWorkflowExecutionTriggerStatusAsync(workflowExecutionTriggerId);

                return Ok(new
                {
                    workflowExecutionTriggerId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger status for {WorkflowExecutionTriggerId}: {Message}", workflowExecutionTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger issues.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerIssues(string workflowExecutionTriggerId)
        {
            try
            {
                var issues = await _workflowExecutionTriggerService.GetWorkflowExecutionTriggerIssuesAsync(workflowExecutionTriggerId);

                return Ok(new
                {
                    workflowExecutionTriggerId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger issues for {WorkflowExecutionTriggerId}: {Message}", workflowExecutionTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution trigger statistics.
        /// </summary>
        [HttpGet("{workflowExecutionTriggerId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerStats(string workflowExecutionTriggerId)
        {
            try
            {
                var stats = await _workflowExecutionTriggerService.GetWorkflowExecutionTriggerStatsAsync(workflowExecutionTriggerId);

                return Ok(new
                {
                    workflowExecutionTriggerId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger stats for {WorkflowExecutionTriggerId}: {Message}", workflowExecutionTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution trigger types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowExecutionTriggerTypes()
        {
            try
            {
                var workflowExecutionTriggerTypes = await _workflowExecutionTriggerService.GetWorkflowExecutionTriggerTypesAsync();

                return Ok(new
                {
                    workflowExecutionTriggerTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution trigger types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution trigger types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution trigger request model.
    /// </summary>
    public class CreateWorkflowExecutionTriggerRequest
    {
        /// <summary>
        /// Workflow execution ID.
        /// </summary>
        public string WorkflowExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Trigger ID.
        /// </summary>
        public string TriggerId { get; set; } = string.Empty;

        /// <summary>
        /// Trigger type.
        /// </summary>
        public string TriggerType { get; set; } = "webhook";

        /// <summary>
        /// Trigger status.
        /// </summary>
        public string TriggerStatus { get; set; } = "pending";

        /// <summary>
        /// Trigger configuration.
        /// </summary>
        public Dictionary<string, object>? TriggerConfig { get; set; }

        /// <summary>
        /// Trigger start time.
        /// </summary>
        public DateTime? TriggerStartTime { get; set; }

        /// <summary>
        /// Trigger end time.
        /// </summary>
        public DateTime? TriggerEndTime { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow execution trigger request model.
    /// </summary>
    public class UpdateWorkflowExecutionTriggerRequest
    {
        /// <summary>
        /// Trigger ID.
        /// </summary>
        public string? TriggerId { get; set; }

        /// <summary>
        /// Trigger type.
        /// </summary>
        public string? TriggerType { get; set; }

        /// <summary>
        /// Trigger status.
        /// </summary>
        public string? TriggerStatus { get; set; }

        /// <summary>
        /// Trigger configuration.
        /// </summary>
        public Dictionary<string, object>? TriggerConfig { get; set; }

        /// <summary>
        /// Trigger start time.
        /// </summary>
        public DateTime? TriggerStartTime { get; set; }

        /// <summary>
        /// Trigger end time.
        /// </summary>
        public DateTime? TriggerEndTime { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}