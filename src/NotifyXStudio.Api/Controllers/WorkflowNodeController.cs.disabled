using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow node operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowNodeController : ControllerBase
    {
        private readonly ILogger<WorkflowNodeController> _logger;
        private readonly IWorkflowNodeService _workflowNodeService;

        public WorkflowNodeController(ILogger<WorkflowNodeController> logger, IWorkflowNodeService workflowNodeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowNodeService = workflowNodeService ?? throw new ArgumentNullException(nameof(workflowNodeService));
        }

        /// <summary>
        /// Creates a workflow node.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowNode([FromBody] CreateWorkflowNodeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow node request is required");
                }

                var workflowNodeId = await _workflowNodeService.CreateWorkflowNodeAsync(
                    request.Title,
                    request.Description,
                    request.NodeType,
                    request.WorkflowId,
                    null);

                return Ok(new
                {
                    workflowNodeId,
                    message = "Workflow node created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow node: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow node",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow node information.
        /// </summary>
        [HttpGet("{workflowNodeId}")]
        public async Task<IActionResult> GetWorkflowNode(string workflowNodeId)
        {
            try
            {
                var workflowNode = await _workflowNodeService.GetWorkflowNodeAsync(workflowNodeId);

                if (workflowNode == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow node not found",
                        workflowNodeId
                    });
                }

                return Ok(workflowNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow node {WorkflowNodeId}: {Message}", workflowNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow node",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow nodes.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowNodes(
            [FromQuery] string? workflowId,
            [FromQuery] string? nodeType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowNodes = await _workflowNodeService.ListWorkflowNodesAsync(workflowId, nodeType, page, pageSize);
                var totalCount = await _workflowNodeService.GetWorkflowNodeCountAsync(workflowId);

                return Ok(new
                {
                    workflowNodes,
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
                _logger.LogError(ex, "Failed to list workflow nodes: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow nodes",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow node.
        /// </summary>
        [HttpPut("{workflowNodeId}")]
        public async Task<IActionResult> UpdateWorkflowNode(
            string workflowNodeId,
            [FromBody] UpdateWorkflowNodeRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowNodeService.UpdateWorkflowNodeAsync(
                    workflowNodeId,
                    request.Title,
                    request.Description,
                    request.NodeType,
                    null,
                    request.Status);

                return Ok(new
                {
                    message = "Workflow node updated successfully",
                    workflowNodeId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow node {WorkflowNodeId}: {Message}", workflowNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow node",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow node.
        /// </summary>
        [HttpDelete("{workflowNodeId}")]
        public async Task<IActionResult> DeleteWorkflowNode(string workflowNodeId)
        {
            try
            {
                await _workflowNodeService.DeleteWorkflowNodeAsync(workflowNodeId);

                return Ok(new
                {
                    message = "Workflow node deleted successfully",
                    workflowNodeId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow node {WorkflowNodeId}: {Message}", workflowNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow node",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow node status.
        /// </summary>
        [HttpGet("{workflowNodeId}/status")]
        public async Task<IActionResult> GetWorkflowNodeStatus(string workflowNodeId)
        {
            try
            {
                var status = await _workflowNodeService.GetWorkflowNodeStatusAsync(workflowNodeId);

                return Ok(new
                {
                    workflowNodeId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow node status for {WorkflowNodeId}: {Message}", workflowNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow node status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow node issues.
        /// </summary>
        [HttpGet("{workflowNodeId}/issues")]
        public async Task<IActionResult> GetWorkflowNodeIssues(string workflowNodeId)
        {
            try
            {
                var issues = await _workflowNodeService.GetWorkflowNodeIssuesAsync(workflowNodeId);

                return Ok(new
                {
                    workflowNodeId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow node issues for {WorkflowNodeId}: {Message}", workflowNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow node issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow node statistics.
        /// </summary>
        [HttpGet("{workflowNodeId}/stats")]
        public async Task<IActionResult> GetWorkflowNodeStats(string workflowNodeId)
        {
            try
            {
                var stats = await _workflowNodeService.GetWorkflowNodeStatsAsync(workflowNodeId);

                return Ok(new
                {
                    workflowNodeId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow node stats for {WorkflowNodeId}: {Message}", workflowNodeId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow node statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow node types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowNodeTypes()
        {
            try
            {
                var workflowNodeTypes = await _workflowNodeService.GetWorkflowNodeTypesAsync();

                return Ok(new
                {
                    workflowNodeTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow node types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow node types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow node request model.
    /// </summary>
    public class CreateWorkflowNodeRequest
    {
        /// <summary>
        /// Workflow ID.
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// Node type.
        /// </summary>
        public string NodeType { get; set; } = "http.request";

        /// <summary>
        /// Node title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Node description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Node configuration.
        /// </summary>
        public Dictionary<string, object>? NodeConfig { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow node request model.
    /// </summary>
    public class UpdateWorkflowNodeRequest
    {
        /// <summary>
        /// Node type.
        /// </summary>
        public string? NodeType { get; set; }

        /// <summary>
        /// Node title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Node description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Node configuration.
        /// </summary>
        public Dictionary<string, object>? NodeConfig { get; set; }

        /// <summary>
        /// Node status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}