using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution edge operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionEdgeController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionEdgeController> _logger;
        private readonly IWorkflowExecutionEdgeService _workflowExecutionEdgeService;

        public WorkflowExecutionEdgeController(ILogger<WorkflowExecutionEdgeController> logger, IWorkflowExecutionEdgeService workflowExecutionEdgeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionEdgeService = workflowExecutionEdgeService ?? throw new ArgumentNullException(nameof(workflowExecutionEdgeService));
        }

        /// <summary>
        /// Creates a workflow execution edge.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionEdge([FromBody] CreateWorkflowExecutionEdgeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution edge request is required");
                }

                var workflowExecutionEdgeId = await _workflowExecutionEdgeService.CreateWorkflowExecutionEdgeAsync(
                    request.WorkflowExecutionId,
                    request.EdgeId,
                    request.FromNodeId,
                    request.ToNodeId,
                    request.EdgeType,
                    request.EdgeStatus,
                    request.EdgeCondition,
                    request.EdgeStartTime,
                    request.EdgeEndTime,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionEdgeId,
                    message = "Workflow execution edge created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution edge: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution edge",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution edge information.
        /// </summary>
        [HttpGet("{workflowExecutionEdgeId}")]
        public async Task<IActionResult> GetWorkflowExecutionEdge(string workflowExecutionEdgeId)
        {
            try
            {
                var workflowExecutionEdge = await _workflowExecutionEdgeService.GetWorkflowExecutionEdgeAsync(workflowExecutionEdgeId);

                if (workflowExecutionEdge == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution edge not found",
                        workflowExecutionEdgeId
                    });
                }

                return Ok(workflowExecutionEdge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution edge {WorkflowExecutionEdgeId}: {Message}", workflowExecutionEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution edge",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution edges.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionEdges(
            [FromQuery] string? workflowExecutionId,
            [FromQuery] string? edgeId,
            [FromQuery] string? fromNodeId,
            [FromQuery] string? toNodeId,
            [FromQuery] string? edgeType,
            [FromQuery] string? edgeStatus,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionEdges = await _workflowExecutionEdgeService.ListWorkflowExecutionEdgesAsync(workflowExecutionId, edgeId, fromNodeId, toNodeId, edgeType, edgeStatus, page, pageSize);
                var totalCount = await _workflowExecutionEdgeService.GetWorkflowExecutionEdgeCountAsync(workflowExecutionId, edgeId, fromNodeId, toNodeId, edgeType, edgeStatus);

                return Ok(new
                {
                    workflowExecutionEdges,
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
                _logger.LogError(ex, "Failed to list workflow execution edges: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution edges",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution edge.
        /// </summary>
        [HttpPut("{workflowExecutionEdgeId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionEdge(
            string workflowExecutionEdgeId,
            [FromBody] UpdateWorkflowExecutionEdgeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionEdgeService.UpdateWorkflowExecutionEdgeAsync(
                    workflowExecutionEdgeId,
                    request.EdgeId,
                    request.FromNodeId,
                    request.ToNodeId,
                    request.EdgeType,
                    request.EdgeStatus,
                    request.EdgeCondition,
                    request.EdgeStartTime,
                    request.EdgeEndTime,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution edge updated successfully",
                    workflowExecutionEdgeId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution edge {WorkflowExecutionEdgeId}: {Message}", workflowExecutionEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution edge",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution edge.
        /// </summary>
        [HttpDelete("{workflowExecutionEdgeId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionEdge(string workflowExecutionEdgeId)
        {
            try
            {
                await _workflowExecutionEdgeService.DeleteWorkflowExecutionEdgeAsync(workflowExecutionEdgeId);

                return Ok(new
                {
                    message = "Workflow execution edge deleted successfully",
                    workflowExecutionEdgeId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution edge {WorkflowExecutionEdgeId}: {Message}", workflowExecutionEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution edge",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution edge status.
        /// </summary>
        [HttpGet("{workflowExecutionEdgeId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionEdgeStatus(string workflowExecutionEdgeId)
        {
            try
            {
                var status = await _workflowExecutionEdgeService.GetWorkflowExecutionEdgeStatusAsync(workflowExecutionEdgeId);

                return Ok(new
                {
                    workflowExecutionEdgeId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution edge status for {WorkflowExecutionEdgeId}: {Message}", workflowExecutionEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution edge status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution edge issues.
        /// </summary>
        [HttpGet("{workflowExecutionEdgeId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionEdgeIssues(string workflowExecutionEdgeId)
        {
            try
            {
                var issues = await _workflowExecutionEdgeService.GetWorkflowExecutionEdgeIssuesAsync(workflowExecutionEdgeId);

                return Ok(new
                {
                    workflowExecutionEdgeId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution edge issues for {WorkflowExecutionEdgeId}: {Message}", workflowExecutionEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution edge issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution edge statistics.
        /// </summary>
        [HttpGet("{workflowExecutionEdgeId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionEdgeStats(string workflowExecutionEdgeId)
        {
            try
            {
                var stats = await _workflowExecutionEdgeService.GetWorkflowExecutionEdgeStatsAsync(workflowExecutionEdgeId);

                return Ok(new
                {
                    workflowExecutionEdgeId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution edge stats for {WorkflowExecutionEdgeId}: {Message}", workflowExecutionEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution edge statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution edge types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowExecutionEdgeTypes()
        {
            try
            {
                var workflowExecutionEdgeTypes = await _workflowExecutionEdgeService.GetWorkflowExecutionEdgeTypesAsync();

                return Ok(new
                {
                    workflowExecutionEdgeTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution edge types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution edge types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution edge request model.
    /// </summary>
    public class CreateWorkflowExecutionEdgeRequest
    {
        /// <summary>
        /// Workflow execution ID.
        /// </summary>
        public string WorkflowExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Edge ID.
        /// </summary>
        public string EdgeId { get; set; } = string.Empty;

        /// <summary>
        /// From node ID.
        /// </summary>
        public string FromNodeId { get; set; } = string.Empty;

        /// <summary>
        /// To node ID.
        /// </summary>
        public string ToNodeId { get; set; } = string.Empty;

        /// <summary>
        /// Edge type.
        /// </summary>
        public string EdgeType { get; set; } = "default";

        /// <summary>
        /// Edge status.
        /// </summary>
        public string EdgeStatus { get; set; } = "pending";

        /// <summary>
        /// Edge condition.
        /// </summary>
        public string? EdgeCondition { get; set; }

        /// <summary>
        /// Edge start time.
        /// </summary>
        public DateTime? EdgeStartTime { get; set; }

        /// <summary>
        /// Edge end time.
        /// </summary>
        public DateTime? EdgeEndTime { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow execution edge request model.
    /// </summary>
    public class UpdateWorkflowExecutionEdgeRequest
    {
        /// <summary>
        /// Edge ID.
        /// </summary>
        public string? EdgeId { get; set; }

        /// <summary>
        /// From node ID.
        /// </summary>
        public string? FromNodeId { get; set; }

        /// <summary>
        /// To node ID.
        /// </summary>
        public string? ToNodeId { get; set; }

        /// <summary>
        /// Edge type.
        /// </summary>
        public string? EdgeType { get; set; }

        /// <summary>
        /// Edge status.
        /// </summary>
        public string? EdgeStatus { get; set; }

        /// <summary>
        /// Edge condition.
        /// </summary>
        public string? EdgeCondition { get; set; }

        /// <summary>
        /// Edge start time.
        /// </summary>
        public DateTime? EdgeStartTime { get; set; }

        /// <summary>
        /// Edge end time.
        /// </summary>
        public DateTime? EdgeEndTime { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}