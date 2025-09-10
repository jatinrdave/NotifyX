using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Connectors;

namespace NotifyXStudio.Runtime.Services
{
    /// <summary>
    /// Service for executing workflow runs.
    /// </summary>
    public interface IWorkflowExecutionEngine
    {
        /// <summary>
        /// Executes a workflow run.
        /// </summary>
        Task<WorkflowRunResult> ExecuteAsync(WorkflowRun run, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a single node within a workflow run.
        /// </summary>
        Task<NodeExecutionResult> ExecuteNodeAsync(
            WorkflowNode node,
            WorkflowRun run,
            Dictionary<string, object> nodeInputs,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates a workflow before execution.
        /// </summary>
        Task<ValidationResult> ValidateWorkflowAsync(Workflow workflow);

        /// <summary>
        /// Gets the execution plan for a workflow.
        /// </summary>
        Task<ExecutionPlan> GetExecutionPlanAsync(Workflow workflow);

        /// <summary>
        /// Cancels a running workflow execution.
        /// </summary>
        Task<bool> CancelExecutionAsync(string runId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of a workflow run execution.
    /// </summary>
    public class WorkflowRunResult
    {
        public string RunId { get; init; } = string.Empty;
        public RunStatus Status { get; init; }
        public string? ErrorMessage { get; init; }
        public Dictionary<string, object> Output { get; init; } = new();
        public List<NodeExecutionResult> NodeResults { get; init; } = new();
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public long DurationMs { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Execution plan for a workflow.
    /// </summary>
    public class ExecutionPlan
    {
        public List<ExecutionStep> Steps { get; init; } = new();
        public Dictionary<string, List<string>> Dependencies { get; init; } = new();
        public List<string> ParallelGroups { get; init; } = new();
        public int EstimatedDurationMs { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// A single step in the execution plan.
    /// </summary>
    public class ExecutionStep
    {
        public string NodeId { get; init; } = string.Empty;
        public string NodeType { get; init; } = string.Empty;
        public int Order { get; init; }
        public List<string> Dependencies { get; init; } = new();
        public bool CanRunInParallel { get; init; }
        public int EstimatedDurationMs { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
}