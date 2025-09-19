using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow execution node operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExecutionNodeController : ControllerBase
    {
        private readonly ILogger<WorkflowExecutionNodeController> _logger;
        private readonly IWorkflowExecutionNodeService _workflowExecutionNodeService;

        public WorkflowExecutionNodeController(ILogger<WorkflowExecutionNodeController> logger, IWorkflowExecutionNodeService workflowExecutionNodeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowExecutionNodeService = workflowExecutionNodeService ?? throw new ArgumentNullException(nameof(workflowExecutionNodeService));
        }

        /// <summary>
        /// Creates a workflow execution node.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowExecutionNode([FromBody] CreateWorkflowExecutionNodeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow execution node request is required");
                }

                var workflowExecutionNodeId = await _workflowExecutionNodeService.CreateWorkflowExecutionNodeAsync(
                    request.WorkflowExecutionId,
                    request.NodeId,
                    request.NodeType,
                    request.NodeStatus,
                    request.NodeInput,
                    request.NodeOutput,
                    request.NodeError,
                    request.NodeStartTime,
                    request.NodeEndTime,
                    request.Metadata);

                return Ok(new
                {
                    workflowExecutionNodeId,
                    message = "Workflow execution node created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow execution node: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow execution node",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution node information.
        /// </summary>
        [HttpGet("{workflowExecutionNodeId}")]
        public async Task<IActionResult> GetWorkflowExecutionNode(string workflowExecutionNodeId)
        {
            try
            {
                var workflowExecutionNode = await _workflowExecutionNodeService.GetWorkflowExecutionNodeAsync(workflowExecutionNodeId);

                if (workflowExecutionNode == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow execution node not found",
                        workflowExecutionNodeId
                    });
                }

                return Ok(workflowExecutionNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution node {WorkflowExecutionNodeId}: {Message}", workflowExecutionNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution node",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow execution nodes.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowExecutionNodes(
            [FromQuery] string? workflowExecutionId,
            [FromQuery] string? nodeId,
            [FromQuery] string? nodeType,
            [FromQuery] string? nodeStatus,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowExecutionNodes = await _workflowExecutionNodeService.ListWorkflowExecutionNodesAsync(workflowExecutionId, nodeId, nodeType, nodeStatus, page, pageSize);
                var totalCount = await _workflowExecutionNodeService.GetWorkflowExecutionNodeCountAsync(workflowExecutionId, nodeId, nodeType, nodeStatus);

                return Ok(new
                {
                    workflowExecutionNodes,
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
                _logger.LogError(ex, "Failed to list workflow execution nodes: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow execution nodes",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow execution node.
        /// </summary>
        [HttpPut("{workflowExecutionNodeId}")]
        public async Task<IActionResult> UpdateWorkflowExecutionNode(
            string workflowExecutionNodeId,
            [FromBody] UpdateWorkflowExecutionNodeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowExecutionNodeService.UpdateWorkflowExecutionNodeAsync(
                    workflowExecutionNodeId,
                    request.NodeId,
                    request.NodeType,
                    request.NodeStatus,
                    request.NodeInput,
                    request.NodeOutput,
                    request.NodeError,
                    request.NodeStartTime,
                    request.NodeEndTime,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow execution node updated successfully",
                    workflowExecutionNodeId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow execution node {WorkflowExecutionNodeId}: {Message}", workflowExecutionNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow execution node",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow execution node.
        /// </summary>
        [HttpDelete("{workflowExecutionNodeId}")]
        public async Task<IActionResult> DeleteWorkflowExecutionNode(string workflowExecutionNodeId)
        {
            try
            {
                await _workflowExecutionNodeService.DeleteWorkflowExecutionNodeAsync(workflowExecutionNodeId);

                return Ok(new
                {
                    message = "Workflow execution node deleted successfully",
                    workflowExecutionNodeId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow execution node {WorkflowExecutionNodeId}: {Message}", workflowExecutionNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow execution node",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution node status.
        /// </summary>
        [HttpGet("{workflowExecutionNodeId}/status")]
        public async Task<IActionResult> GetWorkflowExecutionNodeStatus(string workflowExecutionNodeId)
        {
            try
            {
                var status = await _workflowExecutionNodeService.GetWorkflowExecutionNodeStatusAsync(workflowExecutionNodeId);

                return Ok(new
                {
                    workflowExecutionNodeId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution node status for {WorkflowExecutionNodeId}: {Message}", workflowExecutionNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution node status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution node issues.
        /// </summary>
        [HttpGet("{workflowExecutionNodeId}/issues")]
        public async Task<IActionResult> GetWorkflowExecutionNodeIssues(string workflowExecutionNodeId)
        {
            try
            {
                var issues = await _workflowExecutionNodeService.GetWorkflowExecutionNodeIssuesAsync(workflowExecutionNodeId);

                return Ok(new
                {
                    workflowExecutionNodeId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution node issues for {WorkflowExecutionNodeId}: {Message}", workflowExecutionNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution node issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow execution node statistics.
        /// </summary>
        [HttpGet("{workflowExecutionNodeId}/stats")]
        public async Task<IActionResult> GetWorkflowExecutionNodeStats(string workflowExecutionNodeId)
        {
            try
            {
                var stats = await _workflowExecutionNodeService.GetWorkflowExecutionNodeStatsAsync(workflowExecutionNodeId);

                return Ok(new
                {
                    workflowExecutionNodeId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution node stats for {WorkflowExecutionNodeId}: {Message}", workflowExecutionNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution node statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow execution node types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowExecutionNodeTypes()
        {
            try
            {
                var workflowExecutionNodeTypes = await _workflowExecutionNodeService.GetWorkflowExecutionNodeTypesAsync();

                return Ok(new
                {
                    workflowExecutionNodeTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow execution node types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow execution node types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow execution node request model.
    /// </summary>
    public class CreateWorkflowExecutionNodeRequest
    {
        /// <summary>
        /// Workflow execution ID.
        /// </summary>
        public string WorkflowExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Node ID.
        /// </summary>
        public string NodeId { get; set; } = string.Empty;

        /// <summary>
        /// Node type.
        /// </summary>
        public string NodeType { get; set; } = "http.request";

        /// <summary>
        /// Node status.
        /// </summary>
        public string NodeStatus { get; set; } = "pending";

        /// <summary>
        /// Node input.
        /// </summary>
        public Dictionary<string, object>? NodeInput { get; set; }

        /// <summary>
        /// Node output.
        /// </summary>
        public Dictionary<string, object>? NodeOutput { get; set; }

        /// <summary>
        /// Node error.
        /// </summary>
        public string? NodeError { get; set; }

        /// <summary>
        /// Node start time.
        /// </summary>
        public DateTime? NodeStartTime { get; set; }

        /// <summary>
        /// Node end time.
        /// </summary>
        public DateTime? NodeEndTime { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow execution node request model.
    /// </summary>
    public class UpdateWorkflowExecutionNodeRequest
    {
        /// <summary>
        /// Node ID.
        /// </summary>
        public string? NodeId { get; set; }

        /// <summary>
        /// Node type.
        /// </summary>
        public string? NodeType { get; set; }

        /// <summary>
        /// Node status.
        /// </summary>
        public string? NodeStatus { get; set; }

        /// <summary>
        /// Node input.
        /// </summary>
        public Dictionary<string, object>? NodeInput { get; set; }

        /// <summary>
        /// Node output.
        /// </summary>
        public Dictionary<string, object>? NodeOutput { get; set; }

        /// <summary>
        /// Node error.
        /// </summary>
        public string? NodeError { get; set; }

        /// <summary>
        /// Node start time.
        /// </summary>
        public DateTime? NodeStartTime { get; set; }

        /// <summary>
        /// Node end time.
        /// </summary>
        public DateTime? NodeEndTime { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}