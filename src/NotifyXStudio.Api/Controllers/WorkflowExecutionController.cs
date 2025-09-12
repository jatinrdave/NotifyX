using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionController> _logger;
        private readonly IWorkflowExecutionService _workflowExecutionService;

        public WorkflowExecutionController(ILogger<WorkflowExecutionController> logger, IWorkflowExecutionService workflowExecutionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionService = workflowExecutionService ?? throw new ArgumentNullException(nameof(workflowExecutionService));
        }

        /// <summary>
        /// Creates a workflow execution.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecution([FromBody] CreateWorkflowExecutionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution request is required");
                }

                var workflowExecutionId = await _workflowExecutionService.CreateWorkflowExecutionAsync(
                    request.WorkflowId,
                    request.ExecutionType,
                    request.ExecutionConfig,
                    request.ExecutionName,
                    request.ExecutionDescription,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionId,
                    message = "Workflow execution created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution information.
        /// </summary>
        [HttpGet("{workflowExecutionId}")]
        public async Task<IActionResult> GetWorkflowExecution(string workflowExecutionId)
        {
            try
            {
                var workflowExecution = await _workflowExecutionService.GetWorkflowExecutionAsync(workflowExecutionId);

                if (workflowExecution == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution not found",
                        workflowExecutionId
                    });
                }

                return Ok(workflowExecution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution {WorkflowExecutionId}: {Message}", workflowExecutionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow executions.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutions(
            [FromQuery] string? workflowId,
            [FromQuery] string? executionType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutions = await _workflowExecutionService.ListWorkflowExecutionsAsync(workflowId, executionType, status, page, pageSize);
                var totalCount = await _workflowExecutionService.GetWorkflowExecutionCountAsync(workflowId, executionType, status);

                return Ok(new
                {
                    workflowExecutions,
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
                _logger.LogError(ex, "Failed to list workflow executions: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow executions",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution.
        /// </summary>
        [HttpPut("{workflowExecutionId}")]
        public async Task<IActionResult> UpdateWorkflowExecution(
            string workflowExecutionId,
            [FromBody] UpdateWorkflowExecutionRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionService.UpdateWorkflowExecutionAsync(
                    workflowExecutionId,
                    request.ExecutionType,
                    request.ExecutionConfig,
                    request.ExecutionName,
                    request.ExecutionDescription,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution updated successfully",
                    workflowExecutionId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution {WorkflowExecutionId}: {Message}", workflowExecutionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution.
        /// </summary>
        [HttpDelete("{workflowExecutionId}")]
        public async Task<IActionResult> DeleteWorkflowExecution(string workflowExecutionId)
        {
            try
            {
                await _workflowExecutionService.DeleteWorkflowExecutionAsync(workflowExecutionId);

                return Ok(new
                {
                    message = "Workflow execution deleted successfully",
                    workflowExecutionId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution {WorkflowExecutionId}: {Message}", workflowExecutionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution status.
        /// </summary>
        [HttpGet("{workflowExecutionId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionStatus(string workflowExecutionId)
        {
            try
            {
                var status = await _workflowExecutionService.GetWorkflowExecutionStatusAsync(workflowExecutionId);

                return Ok(new
                {
                    workflowExecutionId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution status for {WorkflowExecutionId}: {Message}", workflowExecutionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution issues.
        /// </summary>
        [HttpGet("{workflowExecutionId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionIssues(string workflowExecutionId)
        {
            try
            {
                var issues = await _workflowExecutionService.GetWorkflowExecutionIssuesAsync(workflowExecutionId);

                return Ok(new
                {
                    workflowExecutionId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution issues for {WorkflowExecutionId}: {Message}", workflowExecutionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution statistics.
        /// </summary>
        [HttpGet("{workflowExecutionId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionStats(string workflowExecutionId)
        {
            try
            {
                var stats = await _workflowExecutionService.GetWorkflowExecutionStatsAsync(workflowExecutionId);

                return Ok(new
                {
                    workflowExecutionId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution stats for {WorkflowExecutionId}: {Message}", workflowExecutionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowExecutionTypes()
        {
            try
            {
                var workflowExecutionTypes = await _workflowExecutionService.GetWorkflowExecutionTypesAsync();

                return Ok(new
                {
                    workflowExecutionTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution request model.
    /// </summary>
    public class CreateWorkflowExecutionRequest
    {
        /// <summary>
        /// Workflow ID.
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// Execution type.
        /// </summary>
        public string ExecutionType { get; set; } = "manual";

        /// <summary>
        /// Execution configuration.
        /// </summary>
        public Dictionary<string, object>? ExecutionConfig { get; set; }

        /// <summary>
        /// Execution name.
        /// </summary>
        public string ExecutionName { get; set; } = string.Empty;

        /// <summary>
        /// Execution description.
        /// </summary>
        public string ExecutionDescription { get; set; } = string.Empty;

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow execution request model.
    /// </summary>
    public class UpdateWorkflowExecutionRequest
    {
        /// <summary>
        /// Execution type.
        /// </summary>
        public string? ExecutionType { get; set; }

        /// <summary>
        /// Execution configuration.
        /// </summary>
        public Dictionary<string, object>? ExecutionConfig { get; set; }

        /// <summary>
        /// Execution name.
        /// </summary>
        public string? ExecutionName { get; set; }

        /// <summary>
        /// Execution description.
        /// </summary>
        public string? ExecutionDescription { get; set; }

        /// <summary>
        /// Execution status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}