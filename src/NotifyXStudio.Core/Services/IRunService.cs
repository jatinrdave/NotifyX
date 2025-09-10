using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Core.Services
{
    /// <summary>
    /// Service for managing workflow runs.
    /// </summary>
    public interface IRunService
    {
        /// <summary>
        /// Gets a workflow run by ID and tenant.
        /// </summary>
        Task<WorkflowRun?> GetByIdAsync(string runId, string tenantId);

        /// <summary>
        /// Lists workflow runs for a specific workflow.
        /// </summary>
        Task<IEnumerable<WorkflowRun>> ListByWorkflowAsync(
            string workflowId,
            string tenantId,
            int page = 1,
            int pageSize = 20,
            RunStatus? status = null,
            DateTime? from = null,
            DateTime? to = null);

        /// <summary>
        /// Gets node execution logs for a specific run.
        /// </summary>
        Task<IEnumerable<NodeExecutionResult>> GetNodeLogsAsync(
            string runId,
            string tenantId,
            int page = 1,
            int pageSize = 50);

        /// <summary>
        /// Replays a failed workflow run.
        /// </summary>
        Task<string> ReplayRunAsync(WorkflowRun originalRun, System.Text.Json.JsonElement payload);

        /// <summary>
        /// Cancels a running workflow run.
        /// </summary>
        Task<bool> CancelRunAsync(string runId, string tenantId);

        /// <summary>
        /// Gets run statistics for a workflow.
        /// </summary>
        Task<RunStatistics> GetRunStatisticsAsync(
            string workflowId,
            string tenantId,
            DateTime? from = null,
            DateTime? to = null);

        /// <summary>
        /// Gets the workflow for a specific run.
        /// </summary>
        Task<Workflow?> GetWorkflowForRunAsync(string runId, string tenantId);

        /// <summary>
        /// Updates a node execution result.
        /// </summary>
        Task UpdateNodeResultAsync(NodeExecutionResult result);

        /// <summary>
        /// Updates a workflow run status.
        /// </summary>
        Task UpdateRunStatusAsync(string runId, RunStatus status, string? errorMessage = null);
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