using Microsoft.AspNetCore.SignalR;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Api.Services
{
    /// <summary>
    /// Service for sending real-time notifications about workflow execution.
    /// </summary>
    public interface IWorkflowNotificationService
    {
        /// <summary>
        /// Notifies clients about a workflow run status change.
        /// </summary>
        Task NotifyRunStatusChangeAsync(string runId, RunStatus status, string? errorMessage = null);

        /// <summary>
        /// Notifies clients about a node execution result.
        /// </summary>
        Task NotifyNodeExecutionAsync(string runId, NodeExecutionResult nodeResult);

        /// <summary>
        /// Notifies clients about workflow execution progress.
        /// </summary>
        Task NotifyExecutionProgressAsync(string runId, ExecutionProgress progress);

        /// <summary>
        /// Notifies clients about workflow execution completion.
        /// </summary>
        Task NotifyExecutionCompletedAsync(string runId, WorkflowRunResult result);

        /// <summary>
        /// Notifies clients about workflow execution errors.
        /// </summary>
        Task NotifyExecutionErrorAsync(string runId, string errorMessage, Exception? exception = null);

        /// <summary>
        /// Notifies clients about workflow changes.
        /// </summary>
        Task NotifyWorkflowChangedAsync(string workflowId, string tenantId, WorkflowChangeType changeType);

        /// <summary>
        /// Notifies clients about system-wide events.
        /// </summary>
        Task NotifySystemEventAsync(string tenantId, SystemEvent systemEvent);
    }

    /// <summary>
    /// Progress information for workflow execution.
    /// </summary>
    public class ExecutionProgress
    {
        public string RunId { get; init; } = string.Empty;
        public int CompletedNodes { get; init; }
        public int TotalNodes { get; init; }
        public double ProgressPercentage { get; init; }
        public string? CurrentNodeId { get; init; }
        public string? CurrentNodeType { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Types of workflow changes.
    /// </summary>
    public enum WorkflowChangeType
    {
        Created,
        Updated,
        Deleted,
        Activated,
        Deactivated,
        Executed
    }

    /// <summary>
    /// System-wide events.
    /// </summary>
    public class SystemEvent
    {
        public string Id { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Severity { get; init; } = "info";
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Data { get; init; } = new();
    }
}