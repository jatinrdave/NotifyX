using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NotifyXStudio.Core.Connectors
{
    /// <summary>
    /// Contract for connector adapters executed by the runtime.
    /// </summary>
    public interface IConnectorAdapter
    {
        /// <summary>Unique connector type (e.g. "notifyx.sendNotification").</summary>
        string Type { get; }

        /// <summary>
        /// Execute node logic and return the result payload.
        /// </summary>
        Task<ConnectorExecutionResult> ExecuteAsync(ConnectorExecutionContext context, CancellationToken ct = default);
    }

    /// <summary>
    /// Context provided to connector adapters during execution.
    /// </summary>
    public class ConnectorExecutionContext
    {
        /// <summary>Tenant identifier for multi-tenant isolation.</summary>
        public string TenantId { get; init; } = string.Empty;

        /// <summary>Node configuration as JSON element.</summary>
        public JsonElement NodeConfig { get; init; }

        /// <summary>Input data from previous nodes or trigger.</summary>
        public JsonElement Inputs { get; init; }

        /// <summary>Decrypted credential secret for this connector.</summary>
        public string? CredentialSecret { get; init; }

        /// <summary>Run metadata including run ID, workflow ID, etc.</summary>
        public RunMetadata RunMetadata { get; init; } = new();
    }

    /// <summary>
    /// Result returned by connector adapters after execution.
    /// </summary>
    public class ConnectorExecutionResult
    {
        /// <summary>Whether the execution was successful.</summary>
        public bool Success { get; init; }

        /// <summary>Output data to pass to next nodes.</summary>
        public JsonElement Output { get; init; }

        /// <summary>Error message if execution failed.</summary>
        public string? ErrorMessage { get; init; }

        /// <summary>Execution duration in milliseconds.</summary>
        public long DurationMs { get; init; }

        /// <summary>Additional metadata about the execution.</summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Metadata about the current workflow run.
    /// </summary>
    public class RunMetadata
    {
        /// <summary>Unique run identifier.</summary>
        public string RunId { get; init; } = string.Empty;

        /// <summary>Workflow identifier.</summary>
        public string WorkflowId { get; init; } = string.Empty;

        /// <summary>Node identifier being executed.</summary>
        public string NodeId { get; init; } = string.Empty;

        /// <summary>Execution attempt number (for retries).</summary>
        public int Attempt { get; init; } = 1;

        /// <summary>Maximum retry attempts allowed.</summary>
        public int MaxRetries { get; init; } = 3;

        /// <summary>Retry delay in milliseconds.</summary>
        public int RetryDelayMs { get; init; } = 1000;
    }
}