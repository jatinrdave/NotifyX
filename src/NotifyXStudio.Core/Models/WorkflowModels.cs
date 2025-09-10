using System.Text.Json;

namespace NotifyXStudio.Core.Models
{
    /// <summary>
    /// Represents a workflow definition.
    /// </summary>
    public class Workflow
    {
        /// <summary>Unique workflow identifier.</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Workflow name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Tenant identifier for multi-tenant isolation.</summary>
        public string TenantId { get; init; } = string.Empty;

        /// <summary>Workflow description.</summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>List of nodes in the workflow.</summary>
        public List<WorkflowNode> Nodes { get; init; } = new();

        /// <summary>List of edges connecting nodes.</summary>
        public List<WorkflowEdge> Edges { get; init; } = new();

        /// <summary>Workflow triggers.</summary>
        public List<WorkflowTrigger> Triggers { get; init; } = new();

        /// <summary>Workflow version number.</summary>
        public int Version { get; init; } = 1;

        /// <summary>Whether the workflow is active.</summary>
        public bool IsActive { get; init; }

        /// <summary>Creation timestamp.</summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>Last update timestamp.</summary>
        public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>Created by user identifier.</summary>
        public string CreatedBy { get; init; } = string.Empty;

        /// <summary>Last updated by user identifier.</summary>
        public string UpdatedBy { get; init; } = string.Empty;

        /// <summary>Workflow tags for categorization.</summary>
        public List<string> Tags { get; init; } = new();

        /// <summary>Global variables for the workflow.</summary>
        public Dictionary<string, object> GlobalVariables { get; init; } = new();
    }

    /// <summary>
    /// Represents a node in a workflow.
    /// </summary>
    public class WorkflowNode
    {
        /// <summary>Unique node identifier.</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Node type (connector type).</summary>
        public string Type { get; init; } = string.Empty;

        /// <summary>Node category.</summary>
        public string Category { get; init; } = string.Empty;

        /// <summary>Node label for display.</summary>
        public string Label { get; init; } = string.Empty;

        /// <summary>Node position on canvas.</summary>
        public NodePosition Position { get; init; } = new();

        /// <summary>Node configuration.</summary>
        public JsonElement Config { get; init; }

        /// <summary>Credential identifier for this node.</summary>
        public string? CredentialId { get; init; }

        /// <summary>Retry configuration.</summary>
        public RetryConfig RetryConfig { get; init; } = new();

        /// <summary>Timeout in milliseconds.</summary>
        public int TimeoutMs { get; init; } = 30000;

        /// <summary>Whether the node is enabled.</summary>
        public bool IsEnabled { get; init; } = true;

        /// <summary>Node metadata.</summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Represents an edge connecting two nodes in a workflow.
    /// </summary>
    public class WorkflowEdge
    {
        /// <summary>Source node identifier.</summary>
        public string From { get; init; } = string.Empty;

        /// <summary>Target node identifier.</summary>
        public string To { get; init; } = string.Empty;

        /// <summary>Condition expression for the edge.</summary>
        public string Condition { get; init; } = string.Empty;

        /// <summary>Edge label for display.</summary>
        public string Label { get; init; } = string.Empty;

        /// <summary>Edge metadata.</summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Represents a workflow trigger.
    /// </summary>
    public class WorkflowTrigger
    {
        /// <summary>Trigger type.</summary>
        public TriggerType Type { get; init; }

        /// <summary>Trigger configuration.</summary>
        public JsonElement Config { get; init; }

        /// <summary>Whether the trigger is active.</summary>
        public bool IsActive { get; init; } = true;

        /// <summary>Trigger metadata.</summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Represents a workflow execution run.
    /// </summary>
    public class WorkflowRun
    {
        /// <summary>Unique run identifier.</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Workflow identifier.</summary>
        public string WorkflowId { get; init; } = string.Empty;

        /// <summary>Tenant identifier.</summary>
        public string TenantId { get; init; } = string.Empty;

        /// <summary>Run status.</summary>
        public RunStatus Status { get; init; }

        /// <summary>Run mode.</summary>
        public RunMode Mode { get; init; }

        /// <summary>Input payload for the run.</summary>
        public JsonElement Input { get; init; }

        /// <summary>Output result of the run.</summary>
        public JsonElement? Output { get; init; }

        /// <summary>Error message if run failed.</summary>
        public string? ErrorMessage { get; init; }

        /// <summary>Run start time.</summary>
        public DateTime StartTime { get; init; } = DateTime.UtcNow;

        /// <summary>Run end time.</summary>
        public DateTime? EndTime { get; init; }

        /// <summary>Run duration in milliseconds.</summary>
        public long? DurationMs { get; init; }

        /// <summary>Triggered by user identifier.</summary>
        public string? TriggeredBy { get; init; }

        /// <summary>Node execution results.</summary>
        public List<NodeExecutionResult> NodeResults { get; init; } = new();

        /// <summary>Run metadata.</summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Represents the result of a node execution.
    /// </summary>
    public class NodeExecutionResult
    {
        /// <summary>Run identifier.</summary>
        public string RunId { get; init; } = string.Empty;

        /// <summary>Node identifier.</summary>
        public string NodeId { get; init; } = string.Empty;

        /// <summary>Execution status.</summary>
        public ExecutionStatus Status { get; init; }

        /// <summary>Input data for the node.</summary>
        public JsonElement Input { get; init; }

        /// <summary>Output data from the node.</summary>
        public JsonElement? Output { get; init; }

        /// <summary>Error message if execution failed.</summary>
        public string? ErrorMessage { get; init; }

        /// <summary>Execution start time.</summary>
        public DateTime StartTime { get; init; } = DateTime.UtcNow;

        /// <summary>Execution end time.</summary>
        public DateTime? EndTime { get; init; }

        /// <summary>Execution duration in milliseconds.</summary>
        public long? DurationMs { get; init; }

        /// <summary>Execution attempt number.</summary>
        public int Attempt { get; init; } = 1;

        /// <summary>Execution metadata.</summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Represents a credential for connector authentication.
    /// </summary>
    public class ConnectorCredential
    {
        /// <summary>Unique credential identifier.</summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>Tenant identifier.</summary>
        public string TenantId { get; init; } = string.Empty;

        /// <summary>Connector type this credential is for.</summary>
        public string ConnectorType { get; init; } = string.Empty;

        /// <summary>Credential name.</summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>Encrypted credential secret.</summary>
        public string EncryptedSecret { get; init; } = string.Empty;

        /// <summary>Credential scopes.</summary>
        public List<string> Scopes { get; init; } = new();

        /// <summary>Created by user identifier.</summary>
        public string CreatedBy { get; init; } = string.Empty;

        /// <summary>Creation timestamp.</summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>Last update timestamp.</summary>
        public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>Whether the credential is active.</summary>
        public bool IsActive { get; init; } = true;

        /// <summary>Credential metadata.</summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Node position on the canvas.
    /// </summary>
    public class NodePosition
    {
        /// <summary>X coordinate.</summary>
        public double X { get; init; }

        /// <summary>Y coordinate.</summary>
        public double Y { get; init; }
    }

    /// <summary>
    /// Retry configuration for a node.
    /// </summary>
    public class RetryConfig
    {
        /// <summary>Maximum number of retry attempts.</summary>
        public int MaxRetries { get; init; } = 3;

        /// <summary>Initial retry delay in milliseconds.</summary>
        public int InitialDelayMs { get; init; } = 1000;

        /// <summary>Maximum retry delay in milliseconds.</summary>
        public int MaxDelayMs { get; init; } = 30000;

        /// <summary>Retry delay multiplier for exponential backoff.</summary>
        public double Multiplier { get; init; } = 2.0;

        /// <summary>Whether to use exponential backoff.</summary>
        public bool UseExponentialBackoff { get; init; } = true;
    }

    /// <summary>
    /// Trigger type enumeration.
    /// </summary>
    public enum TriggerType
    {
        Webhook,
        Schedule,
        NotifyXEvent,
        Kafka,
        Manual
    }

    /// <summary>
    /// Workflow run status enumeration.
    /// </summary>
    public enum RunStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled,
        Timeout
    }

    /// <summary>
    /// Workflow run mode enumeration.
    /// </summary>
    public enum RunMode
    {
        Test,
        Scheduled,
        Triggered,
        Manual
    }

    /// <summary>
    /// Node execution status enumeration.
    /// </summary>
    public enum ExecutionStatus
    {
        Pending,
        Running,
        Success,
        Failed,
        Skipped,
        Timeout
    }
}