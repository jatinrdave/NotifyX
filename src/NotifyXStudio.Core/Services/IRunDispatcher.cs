using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Core.Services
{
    /// <summary>
    /// Service for dispatching workflow runs to the execution engine.
    /// </summary>
    public interface IRunDispatcher
    {
        /// <summary>
        /// Enqueues a workflow run for execution.
        /// </summary>
        Task<string> EnqueueRunAsync(Workflow workflow, System.Text.Json.JsonElement payload, RunMode mode = RunMode.Manual);

        /// <summary>
        /// Enqueues a scheduled workflow run.
        /// </summary>
        Task<string> EnqueueScheduledRunAsync(string workflowId, string tenantId, System.Text.Json.JsonElement payload);

        /// <summary>
        /// Enqueues a triggered workflow run.
        /// </summary>
        Task<string> EnqueueTriggeredRunAsync(string workflowId, string tenantId, System.Text.Json.JsonElement payload, string triggerType);

        /// <summary>
        /// Gets the status of a queued run.
        /// </summary>
        Task<RunQueueStatus> GetRunStatusAsync(string runId);

        /// <summary>
        /// Cancels a queued run.
        /// </summary>
        Task<bool> CancelQueuedRunAsync(string runId);

        /// <summary>
        /// Gets queue statistics.
        /// </summary>
        Task<QueueStatistics> GetQueueStatisticsAsync(string? tenantId = null);
    }

    /// <summary>
    /// Status of a run in the queue.
    /// </summary>
    public class RunQueueStatus
    {
        public string RunId { get; init; } = string.Empty;
        public string WorkflowId { get; init; } = string.Empty;
        public string TenantId { get; init; } = string.Empty;
        public RunStatus Status { get; init; }
        public DateTime QueuedAt { get; init; }
        public DateTime? StartedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public int QueuePosition { get; init; }
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// Queue statistics.
    /// </summary>
    public class QueueStatistics
    {
        public int PendingRuns { get; init; }
        public int RunningRuns { get; init; }
        public int CompletedRuns { get; init; }
        public int FailedRuns { get; init; }
        public double AverageQueueTimeMs { get; init; }
        public double AverageExecutionTimeMs { get; init; }
        public Dictionary<string, int> RunsByTenant { get; init; } = new();
        public Dictionary<RunStatus, int> RunsByStatus { get; init; } = new();
    }
}