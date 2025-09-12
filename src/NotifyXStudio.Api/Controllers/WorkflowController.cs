using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowController : ControllerBase
    {
        private readonly ILogger<WorkflowController> _logger;
        private readonly IWorkflowService _workflowService;

        public WorkflowController(ILogger<WorkflowController> logger, IWorkflowService workflowService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
        }

        /// <summary>
        /// Creates a workflow.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflow([FromBody] CreateWorkflowRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow request is required");
                }

                var workflowId = await _workflowService.CreateWorkflowAsync(
                    request.ProjectId,
                    request.Title,
                    request.Description,
                    request.WorkflowType,
                    request.Priority,
                    request.Metadata);

                return Ok(new
                {
                    workflowId,
                    message = "Workflow created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow information.
        /// </summary>
        [HttpGet("{workflowId}")]
        public async Task<IActionResult> GetWorkflow(string workflowId)
        {
            try
            {
                var workflow = await _workflowService.GetWorkflowAsync(workflowId);

                if (workflow == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow not found",
                        workflowId
                    });
                }

                return Ok(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow {WorkflowId}: {Message}", workflowId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflows.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflows(
            [FromQuery] string? projectId,
            [FromQuery] string? workflowType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflows = await _workflowService.ListWorkflowsAsync(projectId, workflowType, status, page, pageSize);
                var totalCount = await _workflowService.GetWorkflowCountAsync(projectId, workflowType, status);

                return Ok(new
                {
                    workflows,
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
                _logger.LogError(ex, "Failed to list workflows: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflows",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow.
        /// </summary>
        [HttpPut("{workflowId}")]
        public async Task<IActionResult> UpdateWorkflow(
            string workflowId,
            [FromBody] UpdateWorkflowRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowService.UpdateWorkflowAsync(
                    workflowId,
                    request.Title,
                    request.Description,
                    request.WorkflowType,
                    request.Priority,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow updated successfully",
                    workflowId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow {WorkflowId}: {Message}", workflowId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow.
        /// </summary>
        [HttpDelete("{workflowId}")]
        public async Task<IActionResult> DeleteWorkflow(string workflowId)
        {
            try
            {
                await _workflowService.DeleteWorkflowAsync(workflowId);

                return Ok(new
                {
                    message = "Workflow deleted successfully",
                    workflowId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow {WorkflowId}: {Message}", workflowId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow status.
        /// </summary>
        [HttpGet("{workflowId}/status")]
        public async Task<IActionResult> GetWorkflowStatus(string workflowId)
        {
            try
            {
                var status = await _workflowService.GetWorkflowStatusAsync(workflowId);

                return Ok(new
                {
                    workflowId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow status for {WorkflowId}: {Message}", workflowId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow issues.
        /// </summary>
        [HttpGet("{workflowId}/issues")]
        public async Task<IActionResult> GetWorkflowIssues(string workflowId)
        {
            try
            {
                var issues = await _workflowService.GetWorkflowIssuesAsync(workflowId);

                return Ok(new
                {
                    workflowId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow issues for {WorkflowId}: {Message}", workflowId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow statistics.
        /// </summary>
        [HttpGet("{workflowId}/stats")]
        public async Task<IActionResult> GetWorkflowStats(string workflowId)
        {
            try
            {
                var stats = await _workflowService.GetWorkflowStatsAsync(workflowId);

                return Ok(new
                {
                    workflowId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow stats for {WorkflowId}: {Message}", workflowId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowTypes()
        {
            try
            {
                var workflowTypes = await _workflowService.GetWorkflowTypesAsync();

                return Ok(new
                {
                    workflowTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow request model.
    /// </summary>
    public class CreateWorkflowRequest
    {
        /// <summary>
        /// Project ID.
        /// </summary>
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Workflow title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Workflow description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Workflow type.
        /// </summary>
        public string WorkflowType { get; set; } = "automation";

        /// <summary>
        /// Workflow priority.
        /// </summary>
        public string Priority { get; set; } = "medium";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow request model.
    /// </summary>
    public class UpdateWorkflowRequest
    {
        /// <summary>
        /// Workflow title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Workflow description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Workflow type.
        /// </summary>
        public string? WorkflowType { get; set; }

        /// <summary>
        /// Workflow priority.
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Workflow status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}