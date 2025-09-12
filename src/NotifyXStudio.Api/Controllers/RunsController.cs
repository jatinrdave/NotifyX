using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Services;
using System.Text.Json;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for managing workflow runs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RunsController : ControllerBase
    {
        private readonly IRunService _runService;
        private readonly ILogger<RunsController> _logger;

        public RunsController(IRunService runService, ILogger<RunsController> logger)
        {
            _runService = runService ?? throw new ArgumentNullException(nameof(runService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a workflow run by ID.
        /// </summary>
        [HttpGet("{runId}")]
        public async Task<ActionResult<WorkflowRun>> GetRun(string runId)
        {
            try
            {
                var tenantId = GetTenantId();
                var run = await _runService.GetByIdAsync(runId, tenantId);
                
                if (run == null)
                {
                    return NotFound();
                }

                return Ok(run);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get run {RunId}", runId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Lists workflow runs for a specific workflow.
        /// </summary>
        [HttpGet("workflow/{workflowId}")]
        public async Task<ActionResult<IEnumerable<WorkflowRun>>> ListRuns(
            string workflowId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] RunStatus? status = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var runs = await _runService.ListByWorkflowAsync(workflowId, tenantId, page, pageSize, status, from, to);
                return Ok(runs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list runs for workflow {WorkflowId}", workflowId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets node execution logs for a specific run.
        /// </summary>
        [HttpGet("{runId}/logs")]
        public async Task<ActionResult<IEnumerable<NodeExecutionResult>>> GetRunLogs(
            string runId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var tenantId = GetTenantId();
                var logs = await _runService.GetNodeLogsAsync(runId, tenantId, page, pageSize);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get logs for run {RunId}", runId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Replays a failed workflow run.
        /// </summary>
        [HttpPost("{runId}/replay")]
        public async Task<ActionResult<StartRunResponse>> ReplayRun(string runId, [FromBody] ReplayRunRequest? request = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var originalRun = await _runService.GetByIdAsync(runId, tenantId);
                
                if (originalRun == null)
                {
                    return NotFound();
                }

                var workflow = await _runService.GetWorkflowForRunAsync(runId, tenantId);
                if (workflow == null)
                {
                    return BadRequest("Workflow not found for this run");
                }

                var payload = request?.Payload ?? originalRun.Input;
                var newRunId = await _runService.ReplayRunAsync(originalRun, payload);
                
                return Accepted(new StartRunResponse { RunId = newRunId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to replay run {RunId}", runId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Cancels a running workflow run.
        /// </summary>
        [HttpPost("{runId}/cancel")]
        public async Task<ActionResult> CancelRun(string runId)
        {
            try
            {
                var tenantId = GetTenantId();
                var success = await _runService.CancelRunAsync(runId, tenantId);
                
                if (!success)
                {
                    return NotFound();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel run {RunId}", runId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Gets run statistics for a workflow.
        /// </summary>
        [HttpGet("workflow/{workflowId}/stats")]
        public async Task<ActionResult<RunStatistics>> GetRunStatistics(
            string workflowId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var tenantId = GetTenantId();
                var stats = await _runService.GetRunStatisticsAsync(workflowId, tenantId, from, to);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get run statistics for workflow {WorkflowId}", workflowId);
                return StatusCode(500, "Internal server error");
            }
        }

        private string GetTenantId()
        {
            return User.FindFirst("tenant_id")?.Value ?? throw new UnauthorizedAccessException("Tenant ID not found in token");
        }
    }

    /// <summary>
    /// Request to replay a failed run.
    /// </summary>
    public class ReplayRunRequest
    {
        public JsonElement? Payload { get; init; }
    }

    /// <summary>
    /// Run statistics for a workflow.
    /// </summary>
    public class RunStatistics
    {
        public int TotalRuns { get; init; }
        public int SuccessfulRuns { get; init; }
        public int FailedRuns { get; init; }
        public int CancelledRuns { get; init; }
        public double AverageDurationMs { get; init; }
        public double SuccessRate { get; init; }
        public Dictionary<RunStatus, int> RunsByStatus { get; init; } = new();
        public List<RunTrend> Trends { get; init; } = new();
    }

    /// <summary>
    /// Run trend data point.
    /// </summary>
    public class RunTrend
    {
        public DateTime Date { get; init; }
        public int TotalRuns { get; init; }
        public int SuccessfulRuns { get; init; }
        public int FailedRuns { get; init; }
        public double AverageDurationMs { get; init; }
    }
}