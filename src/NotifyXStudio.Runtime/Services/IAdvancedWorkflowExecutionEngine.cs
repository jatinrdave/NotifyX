using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Runtime.Services
{
    /// <summary>
    /// Advanced workflow execution engine with support for loops, conditions, and sub-workflows.
    /// </summary>
    public interface IAdvancedWorkflowExecutionEngine : IWorkflowExecutionEngine
    {
        /// <summary>
        /// Executes a workflow with advanced features like loops and conditions.
        /// </summary>
        Task<AdvancedWorkflowRunResult> ExecuteAdvancedAsync(AdvancedWorkflow workflow, WorkflowRun run, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a loop node with the specified configuration.
        /// </summary>
        Task<LoopExecutionResult> ExecuteLoopAsync(AdvancedWorkflowNode node, WorkflowRun run, Dictionary<string, object> inputs, CancellationToken cancellationToken = default);

        /// <summary>
        /// Evaluates a condition and returns the result.
        /// </summary>
        Task<ConditionEvaluationResult> EvaluateConditionAsync(ConditionConfig condition, Dictionary<string, object> context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a sub-workflow.
        /// </summary>
        Task<SubWorkflowExecutionResult> ExecuteSubWorkflowAsync(SubWorkflowConfig config, WorkflowRun parentRun, Dictionary<string, object> inputs, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a workflow template.
        /// </summary>
        Task<WorkflowTemplateExecutionResult> ExecuteTemplateAsync(WorkflowTemplate template, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates an advanced workflow configuration.
        /// </summary>
        Task<AdvancedValidationResult> ValidateAdvancedWorkflowAsync(AdvancedWorkflow workflow);

        /// <summary>
        /// Gets execution statistics for a workflow.
        /// </summary>
        Task<WorkflowExecutionStatistics> GetExecutionStatisticsAsync(string workflowId, DateTime? fromDate = null, DateTime? toDate = null);
    }

    /// <summary>
    /// Result of advanced workflow execution.
    /// </summary>
    public class AdvancedWorkflowRunResult : WorkflowRunResult
    {
        /// <summary>
        /// Loop execution results.
        /// </summary>
        public List<LoopExecutionResult> LoopResults { get; init; } = new();

        /// <summary>
        /// Sub-workflow execution results.
        /// </summary>
        public List<SubWorkflowExecutionResult> SubWorkflowResults { get; init; } = new();

        /// <summary>
        /// Condition evaluation results.
        /// </summary>
        public List<ConditionEvaluationResult> ConditionResults { get; init; } = new();

        /// <summary>
        /// Global variables used during execution.
        /// </summary>
        public Dictionary<string, object> GlobalVariables { get; init; } = new();

        /// <summary>
        /// Performance metrics.
        /// </summary>
        public WorkflowPerformanceMetrics PerformanceMetrics { get; init; } = new();
    }

    /// <summary>
    /// Result of loop execution.
    /// </summary>
    public class LoopExecutionResult
    {
        /// <summary>
        /// Node ID that executed the loop.
        /// </summary>
        public string NodeId { get; init; } = "";

        /// <summary>
        /// Loop type that was executed.
        /// </summary>
        public LoopType LoopType { get; init; }

        /// <summary>
        /// Number of iterations completed.
        /// </summary>
        public int IterationsCompleted { get; init; }

        /// <summary>
        /// Results from each iteration.
        /// </summary>
        public List<Dictionary<string, object>> IterationResults { get; init; } = new();

        /// <summary>
        /// Whether the loop completed successfully.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Error message if the loop failed.
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Total execution time for the loop.
        /// </summary>
        public long TotalDurationMs { get; init; }

        /// <summary>
        /// Average execution time per iteration.
        /// </summary>
        public long AverageIterationDurationMs { get; init; }
    }

    /// <summary>
    /// Result of condition evaluation.
    /// </summary>
    public class ConditionEvaluationResult
    {
        /// <summary>
        /// Node ID that evaluated the condition.
        /// </summary>
        public string NodeId { get; init; } = "";

        /// <summary>
        /// Whether the condition evaluated to true.
        /// </summary>
        public bool Result { get; init; }

        /// <summary>
        /// The expression or condition that was evaluated.
        /// </summary>
        public string Expression { get; init; } = "";

        /// <summary>
        /// Context variables used in evaluation.
        /// </summary>
        public Dictionary<string, object> Context { get; init; } = new();

        /// <summary>
        /// Evaluation duration in milliseconds.
        /// </summary>
        public long DurationMs { get; init; }

        /// <summary>
        /// Error message if evaluation failed.
        /// </summary>
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// Result of sub-workflow execution.
    /// </summary>
    public class SubWorkflowExecutionResult
    {
        /// <summary>
        /// Parent node ID that triggered the sub-workflow.
        /// </summary>
        public string ParentNodeId { get; init; } = "";

        /// <summary>
        /// Sub-workflow ID that was executed.
        /// </summary>
        public string SubWorkflowId { get; init; } = "";

        /// <summary>
        /// Sub-workflow run ID.
        /// </summary>
        public string SubWorkflowRunId { get; init; } = "";

        /// <summary>
        /// Whether the sub-workflow executed successfully.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Output from the sub-workflow.
        /// </summary>
        public Dictionary<string, object> Output { get; init; } = new();

        /// <summary>
        /// Error message if the sub-workflow failed.
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Sub-workflow execution duration.
        /// </summary>
        public long DurationMs { get; init; }

        /// <summary>
        /// Node execution results from the sub-workflow.
        /// </summary>
        public List<NodeExecutionResult> NodeResults { get; init; } = new();
    }

    /// <summary>
    /// Result of workflow template execution.
    /// </summary>
    public class WorkflowTemplateExecutionResult
    {
        /// <summary>
        /// Template ID that was executed.
        /// </summary>
        public string TemplateId { get; init; } = "";

        /// <summary>
        /// Whether the template executed successfully.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Output from the template execution.
        /// </summary>
        public Dictionary<string, object> Output { get; init; } = new();

        /// <summary>
        /// Error message if the template failed.
        /// </summary>
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Template execution duration.
        /// </summary>
        public long DurationMs { get; init; }

        /// <summary>
        /// Parameters that were passed to the template.
        /// </summary>
        public Dictionary<string, object> Parameters { get; init; } = new();
    }

    /// <summary>
    /// Advanced validation result for workflows.
    /// </summary>
    public class AdvancedValidationResult : ValidationResult
    {
        /// <summary>
        /// Loop validation results.
        /// </summary>
        public List<LoopValidationResult> LoopValidations { get; init; } = new();

        /// <summary>
        /// Condition validation results.
        /// </summary>
        public List<ConditionValidationResult> ConditionValidations { get; init; } = new();

        /// <summary>
        /// Sub-workflow validation results.
        /// </summary>
        public List<SubWorkflowValidationResult> SubWorkflowValidations { get; init; } = new();

        /// <summary>
        /// Template validation results.
        /// </summary>
        public List<TemplateValidationResult> TemplateValidations { get; init; } = new();

        /// <summary>
        /// Performance validation results.
        /// </summary>
        public PerformanceValidationResult? PerformanceValidation { get; init; }
    }

    /// <summary>
    /// Loop validation result.
    /// </summary>
    public class LoopValidationResult
    {
        /// <summary>
        /// Node ID of the loop.
        /// </summary>
        public string NodeId { get; init; } = "";

        /// <summary>
        /// Whether the loop configuration is valid.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Validation errors.
        /// </summary>
        public List<string> Errors { get; init; } = new();

        /// <summary>
        /// Validation warnings.
        /// </summary>
        public List<string> Warnings { get; init; } = new();
    }

    /// <summary>
    /// Condition validation result.
    /// </summary>
    public class ConditionValidationResult
    {
        /// <summary>
        /// Node ID of the condition.
        /// </summary>
        public string NodeId { get; init; } = "";

        /// <summary>
        /// Whether the condition configuration is valid.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Validation errors.
        /// </summary>
        public List<string> Errors { get; init; } = new();

        /// <summary>
        /// Validation warnings.
        /// </summary>
        public List<string> Warnings { get; init; } = new();
    }

    /// <summary>
    /// Sub-workflow validation result.
    /// </summary>
    public class SubWorkflowValidationResult
    {
        /// <summary>
        /// Node ID of the sub-workflow.
        /// </summary>
        public string NodeId { get; init; } = "";

        /// <summary>
        /// Sub-workflow ID.
        /// </summary>
        public string SubWorkflowId { get; init; } = "";

        /// <summary>
        /// Whether the sub-workflow configuration is valid.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Validation errors.
        /// </summary>
        public List<string> Errors { get; init; } = new();

        /// <summary>
        /// Validation warnings.
        /// </summary>
        public List<string> Warnings { get; init; } = new();
    }

    /// <summary>
    /// Template validation result.
    /// </summary>
    public class TemplateValidationResult
    {
        /// <summary>
        /// Template ID.
        /// </summary>
        public string TemplateId { get; init; } = "";

        /// <summary>
        /// Whether the template is valid.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Validation errors.
        /// </summary>
        public List<string> Errors { get; init; } = new();

        /// <summary>
        /// Validation warnings.
        /// </summary>
        public List<string> Warnings { get; init; } = new();
    }

    /// <summary>
    /// Performance validation result.
    /// </summary>
    public class PerformanceValidationResult
    {
        /// <summary>
        /// Whether the workflow meets performance requirements.
        /// </summary>
        public bool MeetsRequirements { get; init; }

        /// <summary>
        /// Estimated execution time in milliseconds.
        /// </summary>
        public long EstimatedExecutionTimeMs { get; init; }

        /// <summary>
        /// Estimated memory usage in MB.
        /// </summary>
        public long EstimatedMemoryUsageMB { get; init; }

        /// <summary>
        /// Performance warnings.
        /// </summary>
        public List<string> Warnings { get; init; } = new();

        /// <summary>
        /// Performance recommendations.
        /// </summary>
        public List<string> Recommendations { get; init; } = new();
    }

    /// <summary>
    /// Workflow performance metrics.
    /// </summary>
    public class WorkflowPerformanceMetrics
    {
        /// <summary>
        /// Total execution time in milliseconds.
        /// </summary>
        public long TotalExecutionTimeMs { get; init; }

        /// <summary>
        /// Peak memory usage in MB.
        /// </summary>
        public long PeakMemoryUsageMB { get; init; }

        /// <summary>
        /// Number of nodes executed.
        /// </summary>
        public int NodesExecuted { get; init; }

        /// <summary>
        /// Number of loops executed.
        /// </summary>
        public int LoopsExecuted { get; init; }

        /// <summary>
        /// Number of conditions evaluated.
        /// </summary>
        public int ConditionsEvaluated { get; init; }

        /// <summary>
        /// Number of sub-workflows executed.
        /// </summary>
        public int SubWorkflowsExecuted { get; init; }

        /// <summary>
        /// Average node execution time in milliseconds.
        /// </summary>
        public long AverageNodeExecutionTimeMs { get; init; }

        /// <summary>
        /// CPU usage percentage.
        /// </summary>
        public double CpuUsagePercentage { get; init; }

        /// <summary>
        /// Network requests made.
        /// </summary>
        public int NetworkRequests { get; init; }

        /// <summary>
        /// Database queries executed.
        /// </summary>
        public int DatabaseQueries { get; init; }
    }

    /// <summary>
    /// Workflow execution statistics.
    /// </summary>
    public class WorkflowExecutionStatistics
    {
        /// <summary>
        /// Total number of executions.
        /// </summary>
        public int TotalExecutions { get; init; }

        /// <summary>
        /// Number of successful executions.
        /// </summary>
        public int SuccessfulExecutions { get; init; }

        /// <summary>
        /// Number of failed executions.
        /// </summary>
        public int FailedExecutions { get; init; }

        /// <summary>
        /// Success rate percentage.
        /// </summary>
        public double SuccessRate { get; init; }

        /// <summary>
        /// Average execution time in milliseconds.
        /// </summary>
        public long AverageExecutionTimeMs { get; init; }

        /// <summary>
        /// Minimum execution time in milliseconds.
        /// </summary>
        public long MinExecutionTimeMs { get; init; }

        /// <summary>
        /// Maximum execution time in milliseconds.
        /// </summary>
        public long MaxExecutionTimeMs { get; init; }

        /// <summary>
        /// Most common error types.
        /// </summary>
        public Dictionary<string, int> CommonErrors { get; init; } = new();

        /// <summary>
        /// Execution frequency by hour of day.
        /// </summary>
        public Dictionary<int, int> ExecutionsByHour { get; init; } = new();

        /// <summary>
        /// Execution frequency by day of week.
        /// </summary>
        public Dictionary<int, int> ExecutionsByDayOfWeek { get; init; } = new();
    }
}