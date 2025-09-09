using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents a workflow action that can be executed as part of a notification workflow.
/// </summary>
public sealed class WorkflowAction
{
    /// <summary>
    /// Unique identifier for this action.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The type of action to perform.
    /// </summary>
    public WorkflowActionType Type { get; init; } = WorkflowActionType.SendNotification;

    /// <summary>
    /// The name of this action.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of what this action does.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Parameters for the action.
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();

    /// <summary>
    /// Whether this action was executed successfully.
    /// </summary>
    public bool IsExecuted { get; init; } = false;

    /// <summary>
    /// Whether this action was successful.
    /// </summary>
    public bool IsSuccess { get; init; } = false;

    /// <summary>
    /// Error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Timestamp when the action was executed.
    /// </summary>
    public DateTime? ExecutedAt { get; init; }

    /// <summary>
    /// Duration of the action execution.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Result data from the action execution.
    /// </summary>
    public Dictionary<string, object> Result { get; init; } = new();

    /// <summary>
    /// Whether this action should be executed asynchronously.
    /// </summary>
    public bool IsAsync { get; init; } = false;

    /// <summary>
    /// Timeout for action execution.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Whether to continue processing other actions if this one fails.
    /// </summary>
    public bool ContinueOnFailure { get; init; } = true;

    /// <summary>
    /// Retry configuration for this action.
    /// </summary>
    public WorkflowActionRetryConfiguration? RetryConfiguration { get; init; }

    /// <summary>
    /// Custom metadata associated with this action.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Types of workflow actions that can be performed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowActionType
{
    /// <summary>
    /// Send a notification.
    /// </summary>
    SendNotification = 0,

    /// <summary>
    /// Modify notification properties.
    /// </summary>
    ModifyNotification = 1,

    /// <summary>
    /// Add recipients to the notification.
    /// </summary>
    AddRecipients = 2,

    /// <summary>
    /// Remove recipients from the notification.
    /// </summary>
    RemoveRecipients = 3,

    /// <summary>
    /// Set notification priority.
    /// </summary>
    SetPriority = 4,

    /// <summary>
    /// Set notification channels.
    /// </summary>
    SetChannels = 5,

    /// <summary>
    /// Delay notification delivery.
    /// </summary>
    DelayDelivery = 6,

    /// <summary>
    /// Cancel notification delivery.
    /// </summary>
    CancelDelivery = 7,

    /// <summary>
    /// Execute a webhook.
    /// </summary>
    ExecuteWebhook = 8,

    /// <summary>
    /// Log an event.
    /// </summary>
    LogEvent = 9,

    /// <summary>
    /// Send to escalation channel.
    /// </summary>
    Escalate = 10,

    /// <summary>
    /// Aggregate notifications.
    /// </summary>
    Aggregate = 11,

    /// <summary>
    /// Custom action.
    /// </summary>
    Custom = 12
}

/// <summary>
/// Retry configuration for workflow actions.
/// </summary>
public sealed class WorkflowActionRetryConfiguration
{
    /// <summary>
    /// Maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// Initial delay between retry attempts.
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum delay between retry attempts.
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Backoff multiplier for exponential backoff.
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Whether to use exponential backoff for retries.
    /// </summary>
    public bool UseExponentialBackoff { get; init; } = true;

    /// <summary>
    /// Whether to use jitter in retry delays.
    /// </summary>
    public bool UseJitter { get; init; } = true;
}

/// <summary>
/// Represents an escalation action for failed notifications.
/// </summary>
public sealed class EscalationAction
{
    /// <summary>
    /// Unique identifier for this escalation action.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The type of escalation action.
    /// </summary>
    public EscalationActionType Type { get; init; } = EscalationActionType.SendToEscalationChannel;

    /// <summary>
    /// The name of this escalation action.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of what this escalation action does.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Parameters for the escalation action.
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();

    /// <summary>
    /// The escalation level (1 = first escalation, 2 = second escalation, etc.).
    /// </summary>
    public int EscalationLevel { get; init; } = 1;

    /// <summary>
    /// Delay before this escalation action is executed.
    /// </summary>
    public TimeSpan Delay { get; init; } = TimeSpan.Zero;

    /// <summary>
    /// Whether this escalation action was executed.
    /// </summary>
    public bool IsExecuted { get; init; } = false;

    /// <summary>
    /// Whether this escalation action was successful.
    /// </summary>
    public bool IsSuccess { get; init; } = false;

    /// <summary>
    /// Error message if the escalation action failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Timestamp when the escalation action was executed.
    /// </summary>
    public DateTime? ExecutedAt { get; init; }

    /// <summary>
    /// Duration of the escalation action execution.
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// Result data from the escalation action execution.
    /// </summary>
    public Dictionary<string, object> Result { get; init; } = new();

    /// <summary>
    /// Custom metadata associated with this escalation action.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Types of escalation actions that can be performed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EscalationActionType
{
    /// <summary>
    /// Send to escalation channel.
    /// </summary>
    SendToEscalationChannel = 0,

    /// <summary>
    /// Send to on-call team.
    /// </summary>
    SendToOnCallTeam = 1,

    /// <summary>
    /// Send to manager.
    /// </summary>
    SendToManager = 2,

    /// <summary>
    /// Send to admin.
    /// </summary>
    SendToAdmin = 3,

    /// <summary>
    /// Execute webhook.
    /// </summary>
    ExecuteWebhook = 4,

    /// <summary>
    /// Log critical event.
    /// </summary>
    LogCriticalEvent = 5,

    /// <summary>
    /// Create incident.
    /// </summary>
    CreateIncident = 6,

    /// <summary>
    /// Custom escalation action.
    /// </summary>
    Custom = 7
}