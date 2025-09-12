using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for workflow run operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowRunController : ControllerBase
    {
        private readonly ILogger<WorkflowRunController> _logger;
        private readonly IWorkflowRunService _workflowRunService;

        public WorkflowRunController(ILogger<WorkflowRunController> logger, IWorkflowRunService workflowRunService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _workflowRunService = workflowRunService ?? throw new ArgumentNullException(nameof(workflowRunService));
        }

        /// <summary>
        /// Creates a workflow run.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWorkflowRun([FromBody] CreateWorkflowRunRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Workflow run request is required");
                }

                var workflowRunId = await _workflowRunService.CreateWorkflowRunAsync(
                    request.WorkflowId,
                    request.RunType,
                    request.Priority,
                    request.Metadata);

                return Ok(new
                {
                    workflowRunId,
                    message = "Workflow run created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create workflow run: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create workflow run",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow run information.
        /// </summary>
        [HttpGet("{workflowRunId}")]
        public async Task<IActionResult> GetWorkflowRun(string workflowRunId)
        {
            try
            {
                var workflowRun = await _workflowRunService.GetWorkflowRunAsync(workflowRunId);

                if (workflowRun == null)
                {
                    return NotFound(new
                    {
                        error = "Workflow run not found",
                        workflowRunId
                    });
                }

                return Ok(workflowRun);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow run {WorkflowRunId}: {Message}", workflowRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow run",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists workflow runs.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWorkflowRuns(
            [FromQuery] string? workflowId,
            [FromQuery] string? runType,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var workflowRuns = await _workflowRunService.ListWorkflowRunsAsync(workflowId, runType, status, page, pageSize);
                var totalCount = await _workflowRunService.GetWorkflowRunCountAsync(workflowId, runType, status);

                return Ok(new
                {
                    workflowRuns,
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
                _logger.LogError(ex, "Failed to list workflow runs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list workflow runs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a workflow run.
        /// </summary>
        [HttpPut("{workflowRunId}")]
        public async Task<IActionResult> UpdateWorkflowRun(
            string workflowRunId,
            [FromBody] UpdateWorkflowRunRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _workflowRunService.UpdateWorkflowRunAsync(
                    workflowRunId,
                    request.RunType,
                    request.Priority,
                    request.Status,
                    request.Metadata);

                return Ok(new
                {
                    message = "Workflow run updated successfully",
                    workflowRunId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update workflow run {WorkflowRunId}: {Message}", workflowRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update workflow run",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a workflow run.
        /// </summary>
        [HttpDelete("{workflowRunId}")]
        public async Task<IActionResult> DeleteWorkflowRun(string workflowRunId)
        {
            try
            {
                await _workflowRunService.DeleteWorkflowRunAsync(workflowRunId);

                return Ok(new
                {
                    message = "Workflow run deleted successfully",
                    workflowRunId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete workflow run {WorkflowRunId}: {Message}", workflowRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete workflow run",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow run status.
        /// </summary>
        [HttpGet("{workflowRunId}/status")]
        public async Task<IActionResult> GetWorkflowRunStatus(string workflowRunId)
        {
            try
            {
                var status = await _workflowRunService.GetWorkflowRunStatusAsync(workflowRunId);

                return Ok(new
                {
                    workflowRunId,
                    status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow run status for {WorkflowRunId}: {Message}", workflowRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow run status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow run issues.
        /// </summary>
        [HttpGet("{workflowRunId}/issues")]
        public async Task<IActionResult> GetWorkflowRunIssues(string workflowRunId)
        {
            try
            {
                var issues = await _workflowRunService.GetWorkflowRunIssuesAsync(workflowRunId);

                return Ok(new
                {
                    workflowRunId,
                    issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow run issues for {WorkflowRunId}: {Message}", workflowRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow run issues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets workflow run statistics.
        /// </summary>
        [HttpGet("{workflowRunId}/stats")]
        public async Task<IActionResult> GetWorkflowRunStats(string workflowRunId)
        {
            try
            {
                var stats = await _workflowRunService.GetWorkflowRunStatsAsync(workflowRunId);

                return Ok(new
                {
                    workflowRunId,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow run stats for {WorkflowRunId}: {Message}", workflowRunId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow run statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets available workflow run types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetWorkflowRunTypes()
        {
            try
            {
                var workflowRunTypes = await _workflowRunService.GetWorkflowRunTypesAsync();

                return Ok(new
                {
                    workflowRunTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get workflow run types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve workflow run types",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create workflow run request model.
    /// </summary>
    public class CreateWorkflowRunRequest
    {
        /// <summary>
        /// Workflow ID.
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// Run type.
        /// </summary>
        public string RunType { get; set; } = "manual";

        /// <summary>
        /// Run priority.
        /// </summary>
        public string Priority { get; set; } = "medium";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Update workflow run request model.
    /// </summary>
    public class UpdateWorkflowRunRequest
    {
        /// <summary>
        /// Run type.
        /// </summary>
        public string? RunType { get; set; }

        /// <summary>
        /// Run priority.
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Run status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}