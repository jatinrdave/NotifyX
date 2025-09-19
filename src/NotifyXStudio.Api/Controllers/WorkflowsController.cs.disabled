using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Services;
using System.Text.Json;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for managing workflows.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;
        private readonly IRunDispatcher _runDispatcher;
        private readonly ILogger<WorkflowsController> _logger;

        public WorkflowsController(
            IWorkflowService workflowService,
            IRunDispatcher runDispatcher,
            ILogger<WorkflowsController> logger)
        {
            _workflowService = workflowService ?? throw new ArgumentNullException(nameof(workflowService));
            _runDispatcher = runDispatcher ?? throw new ArgumentNullException(nameof(runDispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new workflow.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Workflow>> CreateWorkflow([FromBody] CreateWorkflowRequest request)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                var workflow = new Workflow
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    TenantId = tenantId,
                    Description = request.Description,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    Tags = request.Tags ?? new List<string>(),
                    GlobalVariables = request.GlobalVariables ?? new Dictionary<string, object>()
                };

                var result = await _workflowService.CreateAsync(workflow);
                return CreatedAtAction(nameof(GetWorkflow), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets a workflow by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Workflow>> GetWorkflow(string id)
        {
            try
            {
                var tenantId = GetTenantId();
                var workflow = await _workflowService.GetByIdAsync(id, tenantId);
                
                if (workflow == null)
                {
                    return NotFound();
                }

                return Ok(workflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow {WorkflowId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates a workflow.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Workflow>> UpdateWorkflow(string id, [FromBody] UpdateWorkflowRequest request)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                var existingWorkflow = await _workflowService.GetByIdAsync(id, tenantId);
                if (existingWorkflow == null)
                {
                    return NotFound();
                }

                var updatedWorkflow = existingWorkflow with
                {
                    Name = request.Name ?? existingWorkflow.Name,
                    Description = request.Description ?? existingWorkflow.Description,
                    Nodes = request.Nodes ?? existingWorkflow.Nodes,
                    Edges = request.Edges ?? existingWorkflow.Edges,
                    Triggers = request.Triggers ?? existingWorkflow.Triggers,
                    GlobalVariables = request.GlobalVariables ?? existingWorkflow.GlobalVariables,
                    Tags = request.Tags ?? existingWorkflow.Tags,
                    Version = existingWorkflow.Version + 1,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = userId
                };

                var result = await _workflowService.UpdateAsync(updatedWorkflow);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow {WorkflowId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lists workflows for the current tenant.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Workflow>>> ListWorkflows(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] List<string>? tags = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var workflows = await _workflowService.ListAsync(tenantId, page, pageSize, search, tags);
                return Ok(workflows);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list workflows");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Deletes a workflow.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkflow(string id)
        {
            try
            {
                var tenantId = GetTenantId();
                var success = await _workflowService.DeleteAsync(id, tenantId);
                
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow {WorkflowId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Starts a workflow run.
        /// </summary>
        [HttpPost("{id}/runs")]
        public async Task<ActionResult<StartRunResponse>> StartRun(string id, [FromBody] StartRunRequest request)
        {
            try
            {
                var tenantId = GetTenantId();
                var workflow = await _workflowService.GetByIdAsync(id, tenantId);
                
                if (workflow == null)
                {
                    return NotFound();
                }

                var runId = await _runDispatcher.EnqueueRunAsync(workflow, request.Payload, request.Mode);
                
                return Accepted(new StartRunResponse { RunId = runId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start run for workflow {WorkflowId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Exports a workflow as JSON.
        /// </summary>
        [HttpPost("{id}/export")]
        public async Task<ActionResult<WorkflowExport>> ExportWorkflow(string id)
        {
            try
            {
                var tenantId = GetTenantId();
                var workflow = await _workflowService.GetByIdAsync(id, tenantId);
                
                if (workflow == null)
                {
                    return NotFound();
                }

                var export = new WorkflowExport
                {
                    Workflow = workflow,
                    ExportedAt = DateTime.UtcNow,
                    ExportedBy = GetUserId(),
                    Version = "1.0"
                };

                return Ok(export);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export workflow {WorkflowId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Imports a workflow from JSON.
        /// </summary>
        [HttpPost("import")]
        public async Task<ActionResult<Workflow>> ImportWorkflow([FromBody] WorkflowImport request)
        {
            try
            {
                var tenantId = GetTenantId();
                var userId = GetUserId();

                var workflow = request.Workflow with
                {
                    Id = Guid.NewGuid().ToString(),
                    TenantId = tenantId,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Version = 1
                };

                var result = await _workflowService.CreateAsync(workflow);
                return CreatedAtAction(nameof(GetWorkflow), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import workflow");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GetTenantId()
        {
            return User.FindFirst("tenant_id")?.Value ?? throw new UnauthorizedAccessException("Tenant ID not found in token");
        }

        private string GetUserId()
        {
            return User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException("User ID not found in token");
        }
    }

    /// <summary>
    /// Request to create a new workflow.
    /// </summary>
    public class CreateWorkflowRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public List<string>? Tags { get; init; }
        public Dictionary<string, object>? GlobalVariables { get; init; }
    }

    /// <summary>
    /// Request to update a workflow.
    /// </summary>
    public class UpdateWorkflowRequest
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public List<WorkflowNode>? Nodes { get; init; }
        public List<WorkflowEdge>? Edges { get; init; }
        public List<WorkflowTrigger>? Triggers { get; init; }
        public Dictionary<string, object>? GlobalVariables { get; init; }
        public List<string>? Tags { get; init; }
    }

    /// <summary>
    /// Request to start a workflow run.
    /// </summary>
    public class StartRunRequest
    {
        public JsonElement Payload { get; init; }
        public RunMode Mode { get; init; } = RunMode.Manual;
    }

    /// <summary>
    /// Response from starting a workflow run.
    /// </summary>
    public class StartRunResponse
    {
        public string RunId { get; init; } = string.Empty;
    }

    /// <summary>
    /// Workflow export data.
    /// </summary>
    public class WorkflowExport
    {
        public Workflow Workflow { get; init; } = null!;
        public DateTime ExportedAt { get; init; }
        public string ExportedBy { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
    }

    /// <summary>
    /// Workflow import request.
    /// </summary>
    public class WorkflowImport
    {
        public Workflow Workflow { get; init; } = null!;
    }
}