using System.Text.Json;

namespace NotifyXStudio.Core.Models
{
    /// <summary>
    /// Advanced workflow node with support for loops, conditions, and sub-workflows.
    /// </summary>
    public class AdvancedWorkflowNode : WorkflowNode
    {
        /// <summary>
        /// Loop configuration for iterative execution.
        /// </summary>
        public LoopConfig? LoopConfig { get; init; }

        /// <summary>
        /// Condition configuration for conditional execution.
        /// </summary>
        public ConditionConfig? ConditionConfig { get; init; }

        /// <summary>
        /// Sub-workflow configuration for nested workflows.
        /// </summary>
        public SubWorkflowConfig? SubWorkflowConfig { get; init; }

        /// <summary>
        /// Error handling configuration.
        /// </summary>
        public ErrorHandlingConfig? ErrorHandling { get; init; }

        /// <summary>
        /// Execution mode for the node.
        /// </summary>
        public ExecutionMode ExecutionMode { get; init; } = ExecutionMode.Sequential;

        /// <summary>
        /// Maximum execution time in milliseconds.
        /// </summary>
        public int MaxExecutionTimeMs { get; init; } = 300000; // 5 minutes

        /// <summary>
        /// Whether to continue execution on error.
        /// </summary>
        public bool ContinueOnError { get; init; } = false;

        /// <summary>
        /// Custom metadata for the node.
        /// </summary>
        public Dictionary<string, object> CustomMetadata { get; init; } = new();
    }

    /// <summary>
    /// Configuration for loop execution.
    /// </summary>
    public class LoopConfig
    {
        /// <summary>
        /// Type of loop to execute.
        /// </summary>
        public LoopType Type { get; init; } = LoopType.ForEach;

        /// <summary>
        /// Maximum number of iterations.
        /// </summary>
        public int MaxIterations { get; init; } = 1000;

        /// <summary>
        /// Delay between iterations in milliseconds.
        /// </summary>
        public int DelayBetweenIterationsMs { get; init; } = 0;

        /// <summary>
        /// Condition to break the loop.
        /// </summary>
        public string? BreakCondition { get; init; }

        /// <summary>
        /// Variable name to store current iteration value.
        /// </summary>
        public string IterationVariable { get; init; } = "i";

        /// <summary>
        /// Array or collection to iterate over.
        /// </summary>
        public string? IterationSource { get; init; }

        /// <summary>
        /// Start value for numeric loops.
        /// </summary>
        public int StartValue { get; init; } = 0;

        /// <summary>
        /// End value for numeric loops.
        /// </summary>
        public int EndValue { get; init; } = 10;

        /// <summary>
        /// Step value for numeric loops.
        /// </summary>
        public int StepValue { get; init; } = 1;
    }

    /// <summary>
    /// Configuration for conditional execution.
    /// </summary>
    public class ConditionConfig
    {
        /// <summary>
        /// Type of condition to evaluate.
        /// </summary>
        public ConditionType Type { get; init; } = ConditionType.Expression;

        /// <summary>
        /// Expression to evaluate.
        /// </summary>
        public string Expression { get; init; } = "";

        /// <summary>
        /// Left operand for comparison.
        /// </summary>
        public string LeftOperand { get; init; } = "";

        /// <summary>
        /// Right operand for comparison.
        /// </summary>
        public string RightOperand { get; init; } = "";

        /// <summary>
        /// Comparison operator.
        /// </summary>
        public ComparisonOperator Operator { get; init; } = ComparisonOperator.Equal;

        /// <summary>
        /// Whether the comparison is case sensitive.
        /// </summary>
        public bool CaseSensitive { get; init; } = true;

        /// <summary>
        /// Multiple conditions for complex logic.
        /// </summary>
        public List<ConditionRule> Rules { get; init; } = new();

        /// <summary>
        /// Logical operator to combine multiple rules.
        /// </summary>
        public LogicalOperator LogicalOperator { get; init; } = LogicalOperator.And;
    }

    /// <summary>
    /// Configuration for sub-workflow execution.
    /// </summary>
    public class SubWorkflowConfig
    {
        /// <summary>
        /// ID of the sub-workflow to execute.
        /// </summary>
        public string SubWorkflowId { get; init; } = "";

        /// <summary>
        /// Input mapping for the sub-workflow.
        /// </summary>
        public Dictionary<string, string> InputMapping { get; init; } = new();

        /// <summary>
        /// Output mapping from the sub-workflow.
        /// </summary>
        public Dictionary<string, string> OutputMapping { get; init; } = new();

        /// <summary>
        /// Whether to wait for sub-workflow completion.
        /// </summary>
        public bool WaitForCompletion { get; init; } = true;

        /// <summary>
        /// Timeout for sub-workflow execution.
        /// </summary>
        public int TimeoutMs { get; init; } = 300000; // 5 minutes

        /// <summary>
        /// Whether to pass through all inputs to sub-workflow.
        /// </summary>
        public bool PassThroughInputs { get; init; } = false;

        /// <summary>
        /// Whether to merge sub-workflow outputs with current context.
        /// </summary>
        public bool MergeOutputs { get; init; } = true;
    }

    /// <summary>
    /// Configuration for error handling.
    /// </summary>
    public class ErrorHandlingConfig
    {
        /// <summary>
        /// Strategy for handling errors.
        /// </summary>
        public ErrorHandlingStrategy Strategy { get; init; } = ErrorHandlingStrategy.Stop;

        /// <summary>
        /// Maximum number of retry attempts.
        /// </summary>
        public int MaxRetries { get; init; } = 3;

        /// <summary>
        /// Delay between retry attempts in milliseconds.
        /// </summary>
        public int RetryDelayMs { get; init; } = 1000;

        /// <summary>
        /// Whether to use exponential backoff for retries.
        /// </summary>
        public bool UseExponentialBackoff { get; init; } = true;

        /// <summary>
        /// Fallback action when all retries fail.
        /// </summary>
        public string? FallbackAction { get; init; }

        /// <summary>
        /// Whether to log errors.
        /// </summary>
        public bool LogErrors { get; init; } = true;

        /// <summary>
        /// Custom error message template.
        /// </summary>
        public string? ErrorMessageTemplate { get; init; }
    }

    /// <summary>
    /// Individual condition rule.
    /// </summary>
    public class ConditionRule
    {
        /// <summary>
        /// Left operand for the rule.
        /// </summary>
        public string LeftOperand { get; init; } = "";

        /// <summary>
        /// Right operand for the rule.
        /// </summary>
        public string RightOperand { get; init; } = "";

        /// <summary>
        /// Comparison operator for the rule.
        /// </summary>
        public ComparisonOperator Operator { get; init; } = ComparisonOperator.Equal;

        /// <summary>
        /// Whether the comparison is case sensitive.
        /// </summary>
        public bool CaseSensitive { get; init; } = true;
    }

    /// <summary>
    /// Advanced workflow with support for complex execution patterns.
    /// </summary>
    public class AdvancedWorkflow : Workflow
    {
        /// <summary>
        /// Global variables for the workflow.
        /// </summary>
        public Dictionary<string, object> GlobalVariables { get; init; } = new();

        /// <summary>
        /// Workflow execution settings.
        /// </summary>
        public WorkflowExecutionSettings ExecutionSettings { get; init; } = new();

        /// <summary>
        /// Workflow templates and reusable components.
        /// </summary>
        public List<WorkflowTemplate> Templates { get; init; } = new();

        /// <summary>
        /// Workflow versioning information.
        /// </summary>
        public WorkflowVersionInfo VersionInfo { get; init; } = new();

        /// <summary>
        /// Workflow permissions and access control.
        /// </summary>
        public WorkflowPermissions Permissions { get; init; } = new();
    }

    /// <summary>
    /// Workflow execution settings.
    /// </summary>
    public class WorkflowExecutionSettings
    {
        /// <summary>
        /// Maximum concurrent executions.
        /// </summary>
        public int MaxConcurrentExecutions { get; init; } = 10;

        /// <summary>
        /// Default timeout for workflow execution.
        /// </summary>
        public int DefaultTimeoutMs { get; init; } = 1800000; // 30 minutes

        /// <summary>
        /// Whether to enable parallel execution where possible.
        /// </summary>
        public bool EnableParallelExecution { get; init; } = true;

        /// <summary>
        /// Whether to enable workflow caching.
        /// </summary>
        public bool EnableCaching { get; init; } = false;

        /// <summary>
        /// Cache TTL in milliseconds.
        /// </summary>
        public int CacheTtlMs { get; init; } = 3600000; // 1 hour

        /// <summary>
        /// Whether to enable workflow debugging.
        /// </summary>
        public bool EnableDebugging { get; init; } = false;

        /// <summary>
        /// Whether to enable performance monitoring.
        /// </summary>
        public bool EnablePerformanceMonitoring { get; init; } = true;
    }

    /// <summary>
    /// Workflow template for reusable components.
    /// </summary>
    public class WorkflowTemplate
    {
        /// <summary>
        /// Template ID.
        /// </summary>
        public string Id { get; init; } = "";

        /// <summary>
        /// Template name.
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// Template description.
        /// </summary>
        public string Description { get; init; } = "";

        /// <summary>
        /// Template category.
        /// </summary>
        public string Category { get; init; } = "";

        /// <summary>
        /// Template parameters.
        /// </summary>
        public List<TemplateParameter> Parameters { get; init; } = new();

        /// <summary>
        /// Template nodes.
        /// </summary>
        public List<AdvancedWorkflowNode> Nodes { get; init; } = new();

        /// <summary>
        /// Template edges.
        /// </summary>
        public List<WorkflowEdge> Edges { get; init; } = new();

        /// <summary>
        /// Template version.
        /// </summary>
        public string Version { get; init; } = "1.0.0";

        /// <summary>
        /// Template author.
        /// </summary>
        public string Author { get; init; } = "";

        /// <summary>
        /// Template tags.
        /// </summary>
        public List<string> Tags { get; init; } = new();
    }

    /// <summary>
    /// Template parameter definition.
    /// </summary>
    public class TemplateParameter
    {
        /// <summary>
        /// Parameter name.
        /// </summary>
        public string Name { get; init; } = "";

        /// <summary>
        /// Parameter type.
        /// </summary>
        public string Type { get; init; } = "string";

        /// <summary>
        /// Parameter description.
        /// </summary>
        public string Description { get; init; } = "";

        /// <summary>
        /// Whether the parameter is required.
        /// </summary>
        public bool Required { get; init; } = false;

        /// <summary>
        /// Default value for the parameter.
        /// </summary>
        public object? DefaultValue { get; init; }

        /// <summary>
        /// Validation rules for the parameter.
        /// </summary>
        public List<ValidationRule> ValidationRules { get; init; } = new();
    }

    /// <summary>
    /// Validation rule for template parameters.
    /// </summary>
    public class ValidationRule
    {
        /// <summary>
        /// Rule type.
        /// </summary>
        public string Type { get; init; } = "";

        /// <summary>
        /// Rule value.
        /// </summary>
        public object? Value { get; init; }

        /// <summary>
        /// Error message for validation failure.
        /// </summary>
        public string ErrorMessage { get; init; } = "";
    }

    /// <summary>
    /// Workflow version information.
    /// </summary>
    public class WorkflowVersionInfo
    {
        /// <summary>
        /// Current version.
        /// </summary>
        public string CurrentVersion { get; init; } = "1.0.0";

        /// <summary>
        /// Version history.
        /// </summary>
        public List<WorkflowVersion> History { get; init; } = new();

        /// <summary>
        /// Whether versioning is enabled.
        /// </summary>
        public bool VersioningEnabled { get; init; } = true;

        /// <summary>
        /// Maximum number of versions to keep.
        /// </summary>
        public int MaxVersions { get; init; } = 10;
    }

    /// <summary>
    /// Individual workflow version.
    /// </summary>
    public class WorkflowVersion
    {
        /// <summary>
        /// Version number.
        /// </summary>
        public string Version { get; init; } = "";

        /// <summary>
        /// Version description.
        /// </summary>
        public string Description { get; init; } = "";

        /// <summary>
        /// Version created date.
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Version created by.
        /// </summary>
        public string CreatedBy { get; init; } = "";

        /// <summary>
        /// Whether this is the current version.
        /// </summary>
        public bool IsCurrent { get; init; } = false;

        /// <summary>
        /// Version tags.
        /// </summary>
        public List<string> Tags { get; init; } = new();
    }

    /// <summary>
    /// Workflow permissions and access control.
    /// </summary>
    public class WorkflowPermissions
    {
        /// <summary>
        /// Whether the workflow is public.
        /// </summary>
        public bool IsPublic { get; init; } = false;

        /// <summary>
        /// Allowed users.
        /// </summary>
        public List<string> AllowedUsers { get; init; } = new();

        /// <summary>
        /// Allowed roles.
        /// </summary>
        public List<string> AllowedRoles { get; init; } = new();

        /// <summary>
        /// Allowed groups.
        /// </summary>
        public List<string> AllowedGroups { get; init; } = new();

        /// <summary>
        /// Permission levels.
        /// </summary>
        public Dictionary<string, PermissionLevel> PermissionLevels { get; init; } = new();
    }

    /// <summary>
    /// Permission level for workflow access.
    /// </summary>
    public enum PermissionLevel
    {
        None = 0,
        Read = 1,
        Execute = 2,
        Edit = 3,
        Admin = 4
    }

    /// <summary>
    /// Loop execution types.
    /// </summary>
    public enum LoopType
    {
        ForEach,
        For,
        While,
        DoWhile
    }

    /// <summary>
    /// Condition evaluation types.
    /// </summary>
    public enum ConditionType
    {
        Expression,
        Comparison,
        Multiple
    }

    /// <summary>
    /// Comparison operators.
    /// </summary>
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        NotContains,
        StartsWith,
        EndsWith,
        Regex,
        IsEmpty,
        IsNotEmpty,
        IsNull,
        IsNotNull
    }

    /// <summary>
    /// Logical operators for combining conditions.
    /// </summary>
    public enum LogicalOperator
    {
        And,
        Or,
        Not
    }

    /// <summary>
    /// Error handling strategies.
    /// </summary>
    public enum ErrorHandlingStrategy
    {
        Stop,
        Retry,
        Skip,
        Fallback,
        Continue
    }

    /// <summary>
    /// Node execution modes.
    /// </summary>
    public enum ExecutionMode
    {
        Sequential,
        Parallel,
        Conditional,
        Loop
    }
}