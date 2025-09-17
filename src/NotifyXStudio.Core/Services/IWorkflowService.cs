using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Core.Services
{
    /// <summary>
    /// Service for managing workflows.
    /// </summary>
    public partial interface IWorkflowService
    {
        /// <summary>
        /// Creates a new workflow.
        /// </summary>
        Task<Workflow> CreateAsync(Workflow workflow);

        /// <summary>
        /// Gets a workflow by ID and tenant.
        /// </summary>
        Task<Workflow?> GetByIdAsync(string id, string tenantId);

        /// <summary>
        /// Updates an existing workflow.
        /// </summary>
        Task<Workflow> UpdateAsync(Workflow workflow);

        /// <summary>
        /// Deletes a workflow.
        /// </summary>
        Task<bool> DeleteAsync(string id, string tenantId);

        /// <summary>
        /// Lists workflows for a tenant with pagination and filtering.
        /// </summary>
        Task<IEnumerable<Workflow>> ListAsync(
            string tenantId,
            int page = 1,
            int pageSize = 20,
            string? search = null,
            List<string>? tags = null);

        /// <summary>
        /// Gets workflow statistics for a tenant.
        /// </summary>
        Task<WorkflowStatistics> GetStatisticsAsync(string tenantId);

        /// <summary>
        /// Validates a workflow definition.
        /// </summary>
        Task<ValidationResult> ValidateAsync(Workflow workflow);

        /// <summary>
        /// Duplicates a workflow.
        /// </summary>
        Task<Workflow> DuplicateAsync(string id, string tenantId, string newName);

        // Additional methods required by controllers
        Task<Workflow> UpdateWorkflowAsync(Workflow workflow, CancellationToken cancellationToken = default);
        Task<Workflow> DeleteWorkflowAsync(string id, CancellationToken cancellationToken = default);
        Task<Workflow> GetWorkflowStatusAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Workflow>> GetWorkflowIssuesAsync(string id, CancellationToken cancellationToken = default);
        Task<Workflow> GetWorkflowStatsAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Workflow>> GetWorkflowTypesAsync(CancellationToken cancellationToken = default);
        
        // Methods called by WorkflowController
        Task<Workflow> CreateWorkflowAsync(Workflow workflow, CancellationToken cancellationToken = default);
        Task<Workflow> GetWorkflowAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Workflow>> ListWorkflowsAsync(string? tenantId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<int> GetWorkflowCountAsync(string? tenantId = null, CancellationToken cancellationToken = default);
        Task<Workflow> UpdateWorkflowAsync(Workflow workflow, string? title = null, string? description = null, List<WorkflowNode>? nodes = null, List<WorkflowEdge>? edges = null, List<WorkflowTrigger>? triggers = null, Dictionary<string, object>? globalVariables = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Workflow statistics.
    /// </summary>
    public class WorkflowStatistics
    {
        public int TotalWorkflows { get; init; }
        public int ActiveWorkflows { get; init; }
        public int InactiveWorkflows { get; init; }
        public int TotalRuns { get; init; }
        public int SuccessfulRuns { get; init; }
        public int FailedRuns { get; init; }
        public double AverageRunDurationMs { get; init; }
        public double SuccessRate { get; init; }
    }

}