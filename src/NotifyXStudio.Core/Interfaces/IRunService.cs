using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Core.Interfaces
{
    /// <summary>
    /// Service for managing workflow runs.
    /// </summary>
    public interface IRunService
    {
        /// <summary>
        /// Creates a new workflow run.
        /// </summary>
        Task<WorkflowRun> CreateRunAsync(WorkflowRun run, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a workflow run by ID.
        /// </summary>
        Task<WorkflowRun?> GetRunAsync(string runId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a workflow run.
        /// </summary>
        Task<WorkflowRun> UpdateRunAsync(WorkflowRun run, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets runs for a specific workflow.
        /// </summary>
        Task<List<WorkflowRun>> GetRunsForWorkflowAsync(string workflowId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets runs by status.
        /// </summary>
        Task<List<WorkflowRun>> GetRunsByStatusAsync(RunStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a workflow run.
        /// </summary>
        System.Threading.Tasks.Task DeleteRunAsync(string runId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the status of a workflow run.
        /// </summary>
        System.Threading.Tasks.Task UpdateRunStatusAsync(string runId, RunStatus status, string? errorMessage = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a workflow run by ID (alias for GetRunAsync).
        /// </summary>
        Task<WorkflowRun?> GetByIdAsync(string runId, string? tenantId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a node execution result.
        /// </summary>
        System.Threading.Tasks.Task UpdateNodeResultAsync(string runId, NodeExecutionResult result, CancellationToken cancellationToken = default);
        
        // Additional methods required by controllers
        Task<int> GetRunCountAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        Task<int> GetActiveRunCountAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        Task<int> GetCompletedRunCountAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        Task<int> GetFailedRunCountAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<WorkflowRun>> GetRunsAsync(string? workflowId = null, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task DeleteOldRunsAsync(int daysOld, CancellationToken cancellationToken = default);
        System.Threading.Tasks.Task DeleteOldLogsAsync(int daysOld, CancellationToken cancellationToken = default);
    }
}
