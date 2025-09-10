using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Core.Services
{
    /// <summary>
    /// Service for managing workflows.
    /// </summary>
    public interface IWorkflowService
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

    /// <summary>
    /// Validation result for workflows.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; init; }
        public List<string> Errors { get; init; } = new();
        public List<string> Warnings { get; init; } = new();
    }
}