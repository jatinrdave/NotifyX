using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow trigger operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowTriggerController : ControllerBase
    {
        private readonly ILogger<WorkflowTriggerController> _logger;
        private readonly IWorkflowTriggerService _workflowTriggerService;

        public WorkflowTriggerController(ILogger<WorkflowTriggerController> logger, IWorkflowTriggerService workflowTriggerService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowTriggerService = workflowTriggerService ?? throw new ArgumentNullException(nameof(workflowTriggerService));
        }

        /// <summary>
        /// Creates a workflow trigger.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowTrigger([FromBody] CreateWorkflowTriggerRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow trigger request is required");
                }

                var workflowTriggerId = await _workflowTriggerService.CreateWorkflowTriggerAsync(
                    request.WorkflowId,
                    request.TriggerType,
                    request.TriggerConfig,
                    request.TriggerName,
                    request.TriggerDescription,
                    request.Metadata);

                return Ok(new
                {
                    workflowTriggerId,
                    message = "Workflow trigger created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow trigger: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow trigger",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow trigger information.
        /// </summary>
        [HttpGet("{workflowTriggerId}")]
        public async Task<IActionResult> GetWorkflowTrigger(string workflowTriggerId)
        {
            try
            {
                var workflowTrigger = await _workflowTriggerService.GetWorkflowTriggerAsync(workflowTriggerId);

                if (workflowTrigger == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow trigger not found",
                        workflowTriggerId
                    });
                }

                return Ok(workflowTrigger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow trigger {WorkflowTriggerId}: {Message}", workflowTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow trigger",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow triggers.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowTriggers(
            [FromQuery] string? workflowId,
            [FromQuery] string? triggerType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowTriggers = await _workflowTriggerService.ListWorkflowTriggersAsync(workflowId, triggerType, status, page, pageSize);
                var totalCount = await _workflowTriggerService.GetWorkflowTriggerCountAsync(workflowId, triggerType, status);

                return Ok(new
                {
                    workflowTriggers,
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
                _logger.LogError(ex, "Failed to list workflow triggers: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow triggers",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow trigger.
        /// </summary>
        [HttpPut("{workflowTriggerId}")]
        public async Task<IActionResult> UpdateWorkflowTrigger(
            string workflowTriggerId,
            [FromBody] UpdateWorkflowTriggerRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowTriggerService.UpdateWorkflowTriggerAsync(
                    workflowTriggerId,
                    request.TriggerType,
                    request.TriggerConfig,
                    request.TriggerName,
                    request.TriggerDescription,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow trigger updated successfully",
                    workflowTriggerId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow trigger {WorkflowTriggerId}: {Message}", workflowTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow trigger",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow trigger.
        /// </summary>
        [HttpDelete("{workflowTriggerId}")]
        public async Task<IActionResult> DeleteWorkflowTrigger(string workflowTriggerId)
        {
            try
            {
                await _workflowTriggerService.DeleteWorkflowTriggerAsync(workflowTriggerId);

                return Ok(new
                {
                    message = "Workflow trigger deleted successfully",
                    workflowTriggerId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow trigger {WorkflowTriggerId}: {Message}", workflowTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow trigger",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow trigger status.
        /// </summary>
        [HttpGet("{workflowTriggerId}/status")]
        public async Task<IActionResult> GetWorkflowTriggerStatus(string workflowTriggerId)
        {
            try
            {
                var status = await _workflowTriggerService.GetWorkflowTriggerStatusAsync(workflowTriggerId);

                return Ok(new
                {
                    workflowTriggerId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow trigger status for {WorkflowTriggerId}: {Message}", workflowTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow trigger status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow trigger issues.
        /// </summary>
        [HttpGet("{workflowTriggerId}/issues")]
        public async Task<IActionResult> GetWorkflowTriggerIssues(string workflowTriggerId)
        {
            try
            {
                var issues = await _workflowTriggerService.GetWorkflowTriggerIssuesAsync(workflowTriggerId);

                return Ok(new
                {
                    workflowTriggerId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow trigger issues for {WorkflowTriggerId}: {Message}", workflowTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow trigger issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow trigger statistics.
        /// </summary>
        [HttpGet("{workflowTriggerId}/stats")]
        public async Task<IActionResult> GetWorkflowTriggerStats(string workflowTriggerId)
        {
            try
            {
                var stats = await _workflowTriggerService.GetWorkflowTriggerStatsAsync(workflowTriggerId);

                return Ok(new
                {
                    workflowTriggerId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow trigger stats for {WorkflowTriggerId}: {Message}", workflowTriggerId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow trigger statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow trigger types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowTriggerTypes()
        {
            try
            {
                var workflowTriggerTypes = await _workflowTriggerService.GetWorkflowTriggerTypesAsync();

                return Ok(new
                {
                    workflowTriggerTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow trigger types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow trigger types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow trigger request model.
    /// </summary>
    public class CreateWorkflowTriggerRequest
    {
        /// <summary>
        /// Workflow ID.
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// Trigger type.
        /// </summary>
        public string TriggerType { get; set; } = "webhook";

        /// <summary>
        /// Trigger configuration.
        /// </summary>
        public Dictionary<string, object>? TriggerConfig { get; set; }

        /// <summary>
        /// Trigger name.
        /// </summary>
        public string TriggerName { get; set; } = string.Empty;

        /// <summary>
        /// Trigger description.
        /// </summary>
        public string TriggerDescription { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow trigger request model.
    /// </summary>
    public class UpdateWorkflowTriggerRequest
    {
        /// <summary>
        /// Trigger type.
        /// </summary>
        public string? TriggerType { get; set; }

        /// <summary>
        /// Trigger configuration.
        /// </summary>
        public Dictionary<string, object>? TriggerConfig { get; set; }

        /// <summary>
        /// Trigger name.
        /// </summary>
        public string? TriggerName { get; set; }

        /// <summary>
        /// Trigger description.
        /// </summary>
        public string? TriggerDescription { get; set; }

        /// <summary>
        /// Trigger status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}