using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Service interface for bulk operations including rule management, subscription management, and batch event ingestion.
/// </summary>
public interface IBulkOperationsService
{
    /// <summary>
    /// Creates multiple notification rules in bulk.
    /// </summary>
    /// <param name="rules">The rules to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous bulk rule creation operation.</returns>
    Task<BulkRuleResult> CreateRulesBulkAsync(IEnumerable<NotificationRule> rules, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple notification rules in bulk.
    /// </summary>
    /// <param name="rules">The rules to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous bulk rule update operation.</returns>
    Task<BulkRuleResult> UpdateRulesBulkAsync(IEnumerable<NotificationRule> rules, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple notification rules in bulk.
    /// </summary>
    /// <param name="ruleIds">The rule IDs to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous bulk rule deletion operation.</returns>
    Task<BulkOperationResult> DeleteRulesBulkAsync(IEnumerable<string> ruleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports rules from JSON/YAML format.
    /// </summary>
    /// <param name="content">The JSON/YAML content containing rules.</param>
    /// <param name="format">The format of the content (JSON or YAML).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule import operation.</returns>
    Task<BulkRuleResult> ImportRulesAsync(string content, BulkImportFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports rules to JSON/YAML format.
    /// </summary>
    /// <param name="ruleIds">The rule IDs to export. If null, exports all rules.</param>
    /// <param name="format">The format to export to (JSON or YAML).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule export operation.</returns>
    Task<BulkExportResult> ExportRulesAsync(IEnumerable<string>? ruleIds, BulkImportFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple notification subscriptions in bulk.
    /// </summary>
    /// <param name="subscriptions">The subscriptions to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous bulk subscription creation operation.</returns>
    Task<BulkSubscriptionResult> CreateSubscriptionsBulkAsync(IEnumerable<NotificationSubscription> subscriptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple notification subscriptions in bulk.
    /// </summary>
    /// <param name="subscriptions">The subscriptions to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous bulk subscription update operation.</returns>
    Task<BulkSubscriptionResult> UpdateSubscriptionsBulkAsync(IEnumerable<NotificationSubscription> subscriptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple notification subscriptions in bulk.
    /// </summary>
    /// <param name="subscriptionIds">The subscription IDs to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous bulk subscription deletion operation.</returns>
    Task<BulkOperationResult> DeleteSubscriptionsBulkAsync(IEnumerable<string> subscriptionIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports subscriptions from CSV/JSON format.
    /// </summary>
    /// <param name="content">The CSV/JSON content containing subscriptions.</param>
    /// <param name="format">The format of the content (CSV or JSON).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous subscription import operation.</returns>
    Task<BulkSubscriptionResult> ImportSubscriptionsAsync(string content, BulkImportFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports subscriptions to CSV/JSON format.
    /// </summary>
    /// <param name="subscriptionIds">The subscription IDs to export. If null, exports all subscriptions.</param>
    /// <param name="format">The format to export to (CSV or JSON).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous subscription export operation.</returns>
    Task<BulkExportResult> ExportSubscriptionsAsync(IEnumerable<string>? subscriptionIds, BulkImportFormat format, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ingests a large batch of notification events.
    /// </summary>
    /// <param name="events">The events to ingest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous batch event ingestion operation.</returns>
    Task<BulkEventResult> IngestEventsBulkAsync(IEnumerable<NotificationEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a bulk operation.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous status check operation.</returns>
    Task<BulkOperationStatus?> GetBulkOperationStatusAsync(string operationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a bulk operation.
    /// </summary>
    /// <param name="operationId">The operation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous cancel operation.</returns>
    Task<bool> CancelBulkOperationAsync(string operationId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a bulk rule operation.
/// </summary>
public sealed class BulkRuleResult
{
    /// <summary>
    /// The operation ID.
    /// </summary>
    public string OperationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Total number of rules processed.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Number of successful operations.
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Number of failed operations.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// Individual rule results.
    /// </summary>
    public List<RuleOperationResult> Results { get; init; } = new();

    /// <summary>
    /// Overall operation status.
    /// </summary>
    public BulkOperationStatus Status { get; init; } = new();

    /// <summary>
    /// Timestamp when the operation was completed.
    /// </summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a bulk subscription operation.
/// </summary>
public sealed class BulkSubscriptionResult
{
    /// <summary>
    /// The operation ID.
    /// </summary>
    public string OperationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Total number of subscriptions processed.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Number of successful operations.
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Number of failed operations.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// Individual subscription results.
    /// </summary>
    public List<SubscriptionOperationResult> Results { get; init; } = new();

    /// <summary>
    /// Overall operation status.
    /// </summary>
    public BulkOperationStatus Status { get; init; } = new();

    /// <summary>
    /// Timestamp when the operation was completed.
    /// </summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a bulk event operation.
/// </summary>
public sealed class BulkEventResult
{
    /// <summary>
    /// The operation ID.
    /// </summary>
    public string OperationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Total number of events processed.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Number of successful operations.
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Number of failed operations.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// Individual event results.
    /// </summary>
    public List<EventOperationResult> Results { get; init; } = new();

    /// <summary>
    /// Overall operation status.
    /// </summary>
    public BulkOperationStatus Status { get; init; } = new();

    /// <summary>
    /// Timestamp when the operation was completed.
    /// </summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a bulk operation.
/// </summary>
public sealed class BulkOperationResult
{
    /// <summary>
    /// The operation ID.
    /// </summary>
    public string OperationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Total number of items processed.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Number of successful operations.
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Number of failed operations.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// Overall operation status.
    /// </summary>
    public BulkOperationStatus Status { get; init; } = new();

    /// <summary>
    /// Timestamp when the operation was completed.
    /// </summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a bulk export operation.
/// </summary>
public sealed class BulkExportResult
{
    /// <summary>
    /// The operation ID.
    /// </summary>
    public string OperationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The exported content.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// The format of the exported content.
    /// </summary>
    public BulkImportFormat Format { get; init; }

    /// <summary>
    /// Number of items exported.
    /// </summary>
    public int ExportedCount { get; init; }

    /// <summary>
    /// Timestamp when the export was completed.
    /// </summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Result of a rule operation.
/// </summary>
public sealed class RuleOperationResult
{
    /// <summary>
    /// The rule ID.
    /// </summary>
    public string RuleId { get; init; } = string.Empty;

    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if the operation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The created or updated rule.
    /// </summary>
    public NotificationRule? Rule { get; init; }
}

/// <summary>
/// Result of a subscription operation.
/// </summary>
public sealed class SubscriptionOperationResult
{
    /// <summary>
    /// The subscription ID.
    /// </summary>
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if the operation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The created or updated subscription.
    /// </summary>
    public NotificationSubscription? Subscription { get; init; }
}

/// <summary>
/// Result of an event operation.
/// </summary>
public sealed class EventOperationResult
{
    /// <summary>
    /// The event ID.
    /// </summary>
    public string EventId { get; init; } = string.Empty;

    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if the operation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The processed event.
    /// </summary>
    public NotificationEvent? Event { get; init; }
}

/// <summary>
/// Status of a bulk operation.
/// </summary>
public sealed class BulkOperationStatus
{
    /// <summary>
    /// The operation ID.
    /// </summary>
    public string OperationId { get; init; } = string.Empty;

    /// <summary>
    /// Current status of the operation.
    /// </summary>
    public BulkOperationState State { get; init; }

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public int Progress { get; init; }

    /// <summary>
    /// Total number of items to process.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// Number of items processed.
    /// </summary>
    public int ProcessedItems { get; init; }

    /// <summary>
    /// Number of successful items.
    /// </summary>
    public int SuccessfulItems { get; init; }

    /// <summary>
    /// Number of failed items.
    /// </summary>
    public int FailedItems { get; init; }

    /// <summary>
    /// Timestamp when the operation was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the operation was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the operation was completed.
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Current error message if any.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional metadata about the operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// States of a bulk operation.
/// </summary>
public enum BulkOperationState
{
    /// <summary>
    /// Operation is pending.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Operation is in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Operation failed.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Operation was cancelled.
    /// </summary>
    Cancelled = 4,

    /// <summary>
    /// Operation completed with partial failures.
    /// </summary>
    PartialFailure = 5
}

/// <summary>
/// Formats for bulk import/export operations.
/// </summary>
public enum BulkImportFormat
{
    /// <summary>
    /// JSON format.
    /// </summary>
    Json = 0,

    /// <summary>
    /// YAML format.
    /// </summary>
    Yaml = 1,

    /// <summary>
    /// CSV format.
    /// </summary>
    Csv = 2
}