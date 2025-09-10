using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow edge operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowEdgeController : ControllerBase
    {
        private readonly ILogger<WorkflowEdgeController> _logger;
        private readonly IWorkflowEdgeService _workflowEdgeService;

        public WorkflowEdgeController(ILogger<WorkflowEdgeController> logger, IWorkflowEdgeService workflowEdgeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowEdgeService = workflowEdgeService ?? throw new ArgumentNullException(nameof(workflowEdgeService));
        }

        /// <summary>
        /// Creates a workflow edge.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowEdge([FromBody] CreateWorkflowEdgeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow edge request is required");
                }

                var workflowEdgeId = await _workflowEdgeService.CreateWorkflowEdgeAsync(
                    request.WorkflowId,
                    request.FromNodeId,
                    request.ToNodeId,
                    request.EdgeType,
                    request.Condition,
                    request.Metadata);

                return Ok(new
                {
                    workflowEdgeId,
                    message = "Workflow edge created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow edge: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow edge",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow edge information.
        /// </summary>
        [HttpGet("{workflowEdgeId}")]
        public async Task<IActionResult> GetWorkflowEdge(string workflowEdgeId)
        {
            try
            {
                var workflowEdge = await _workflowEdgeService.GetWorkflowEdgeAsync(workflowEdgeId);

                if (workflowEdge == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow edge not found",
                        workflowEdgeId
                    });
                }

                return Ok(workflowEdge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow edge {WorkflowEdgeId}: {Message}", workflowEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow edge",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow edges.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowEdges(
            [FromQuery] string? workflowId,
            [FromQuery] string? fromNodeId,
            [FromQuery] string? toNodeId,
            [FromQuery] string? edgeType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowEdges = await _workflowEdgeService.ListWorkflowEdgesAsync(workflowId, fromNodeId, toNodeId, edgeType, page, pageSize);
                var totalCount = await _workflowEdgeService.GetWorkflowEdgeCountAsync(workflowId, fromNodeId, toNodeId, edgeType);

                return Ok(new
                {
                    workflowEdges,
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
                _logger.LogError(ex, "Failed to list workflow edges: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow edges",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow edge.
        /// </summary>
        [HttpPut("{workflowEdgeId}")]
        public async Task<IActionResult> UpdateWorkflowEdge(
            string workflowEdgeId,
            [FromBody] UpdateWorkflowEdgeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowEdgeService.UpdateWorkflowEdgeAsync(
                    workflowEdgeId,
                    request.FromNodeId,
                    request.ToNodeId,
                    request.EdgeType,
                    request.Condition,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow edge updated successfully",
                    workflowEdgeId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow edge {WorkflowEdgeId}: {Message}", workflowEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow edge",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow edge.
        /// </summary>
        [HttpDelete("{workflowEdgeId}")]
        public async Task<IActionResult> DeleteWorkflowEdge(string workflowEdgeId)
        {
            try
            {
                await _workflowEdgeService.DeleteWorkflowEdgeAsync(workflowEdgeId);

                return Ok(new
                {
                    message = "Workflow edge deleted successfully",
                    workflowEdgeId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow edge {WorkflowEdgeId}: {Message}", workflowEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow edge",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow edge status.
        /// </summary>
        [HttpGet("{workflowEdgeId}/status")]
        public async Task<IActionResult> GetWorkflowEdgeStatus(string workflowEdgeId)
        {
            try
            {
                var status = await _workflowEdgeService.GetWorkflowEdgeStatusAsync(workflowEdgeId);

                return Ok(new
                {
                    workflowEdgeId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow edge status for {WorkflowEdgeId}: {Message}", workflowEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow edge status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow edge issues.
        /// </summary>
        [HttpGet("{workflowEdgeId}/issues")]
        public async Task<IActionResult> GetWorkflowEdgeIssues(string workflowEdgeId)
        {
            try
            {
                var issues = await _workflowEdgeService.GetWorkflowEdgeIssuesAsync(workflowEdgeId);

                return Ok(new
                {
                    workflowEdgeId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow edge issues for {WorkflowEdgeId}: {Message}", workflowEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow edge issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow edge statistics.
        /// </summary>
        [HttpGet("{workflowEdgeId}/stats")]
        public async Task<IActionResult> GetWorkflowEdgeStats(string workflowEdgeId)
        {
            try
            {
                var stats = await _workflowEdgeService.GetWorkflowEdgeStatsAsync(workflowEdgeId);

                return Ok(new
                {
                    workflowEdgeId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow edge stats for {WorkflowEdgeId}: {Message}", workflowEdgeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow edge statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow edge types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowEdgeTypes()
        {
            try
            {
                var workflowEdgeTypes = await _workflowEdgeService.GetWorkflowEdgeTypesAsync();

                return Ok(new
                {
                    workflowEdgeTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow edge types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow edge types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow edge request model.
    /// </summary>
    public class CreateWorkflowEdgeRequest
    {
        /// <summary>
        /// Workflow ID.
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

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
        /// Edge condition.
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow edge request model.
    /// </summary>
    public class UpdateWorkflowEdgeRequest
    {
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
        /// Edge condition.
        /// </summary>
        public string? Condition { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}