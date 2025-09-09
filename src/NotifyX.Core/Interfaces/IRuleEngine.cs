using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Interface for the notification rule engine.
/// Handles rule evaluation, workflow processing, and escalation logic.
/// </summary>
public interface IRuleEngine
{
    /// <summary>
    /// Evaluates rules against a notification event and returns applicable rules.
    /// </summary>
    /// <param name="notification">The notification event to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule evaluation operation.</returns>
    Task<RuleEvaluationResult> EvaluateRulesAsync(NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes workflow rules for a notification.
    /// </summary>
    /// <param name="notification">The notification event.</param>
    /// <param name="rules">The rules to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous workflow processing operation.</returns>
    Task<WorkflowResult> ProcessWorkflowAsync(NotificationEvent notification, IEnumerable<NotificationRule> rules, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if escalation is needed for a notification.
    /// </summary>
    /// <param name="notification">The notification event.</param>
    /// <param name="deliveryResults">Previous delivery results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous escalation check operation.</returns>
    Task<EscalationResult> CheckEscalationAsync(NotificationEvent notification, IEnumerable<DeliveryAttempt> deliveryResults, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new rule to the rule engine.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule addition operation.</returns>
    Task<bool> AddRuleAsync(NotificationRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing rule.
    /// </summary>
    /// <param name="rule">The updated rule.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule update operation.</returns>
    Task<bool> UpdateRuleAsync(NotificationRule rule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a rule from the rule engine.
    /// </summary>
    /// <param name="ruleId">The ID of the rule to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule removal operation.</returns>
    Task<bool> RemoveRuleAsync(string ruleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all rules for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule retrieval operation.</returns>
    Task<IEnumerable<NotificationRule>> GetRulesAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific rule by ID.
    /// </summary>
    /// <param name="ruleId">The rule ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule retrieval operation.</returns>
    Task<NotificationRule?> GetRuleAsync(string ruleId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of rule evaluation.
/// </summary>
public sealed class RuleEvaluationResult
{
    /// <summary>
    /// Whether the evaluation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Rules that matched the notification.
    /// </summary>
    public List<NotificationRule> MatchedRules { get; init; } = new();

    /// <summary>
    /// Rules that were evaluated but didn't match.
    /// </summary>
    public List<NotificationRule> UnmatchedRules { get; init; } = new();

    /// <summary>
    /// Error message if evaluation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional metadata about the evaluation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when the evaluation was performed.
    /// </summary>
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful evaluation result.
    /// </summary>
    /// <param name="matchedRules">Rules that matched.</param>
    /// <param name="unmatchedRules">Rules that didn't match.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A successful evaluation result.</returns>
    public static RuleEvaluationResult Success(
        List<NotificationRule> matchedRules,
        List<NotificationRule>? unmatchedRules = null,
        Dictionary<string, object>? metadata = null)
    {
        return new RuleEvaluationResult
        {
            IsSuccess = true,
            MatchedRules = matchedRules,
            UnmatchedRules = unmatchedRules ?? new List<NotificationRule>(),
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a failed evaluation result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A failed evaluation result.</returns>
    public static RuleEvaluationResult Failure(string errorMessage, Dictionary<string, object>? metadata = null)
    {
        return new RuleEvaluationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}

/// <summary>
/// Result of workflow processing.
/// </summary>
public sealed class WorkflowResult
{
    /// <summary>
    /// Whether the workflow processing was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Actions that were executed as part of the workflow.
    /// </summary>
    public List<WorkflowAction> ExecutedActions { get; init; } = new();

    /// <summary>
    /// Actions that failed during execution.
    /// </summary>
    public List<WorkflowAction> FailedActions { get; init; } = new();

    /// <summary>
    /// Modified notification after workflow processing.
    /// </summary>
    public NotificationEvent? ModifiedNotification { get; init; }

    /// <summary>
    /// Error message if workflow processing failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional metadata about the workflow processing.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when the workflow was processed.
    /// </summary>
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful workflow result.
    /// </summary>
    /// <param name="executedActions">Actions that were executed.</param>
    /// <param name="modifiedNotification">Modified notification.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A successful workflow result.</returns>
    public static WorkflowResult Success(
        List<WorkflowAction> executedActions,
        NotificationEvent? modifiedNotification = null,
        Dictionary<string, object>? metadata = null)
    {
        return new WorkflowResult
        {
            IsSuccess = true,
            ExecutedActions = executedActions,
            ModifiedNotification = modifiedNotification,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a failed workflow result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="failedActions">Actions that failed.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A failed workflow result.</returns>
    public static WorkflowResult Failure(
        string errorMessage,
        List<WorkflowAction>? failedActions = null,
        Dictionary<string, object>? metadata = null)
    {
        return new WorkflowResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            FailedActions = failedActions ?? new List<WorkflowAction>(),
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}

/// <summary>
/// Result of escalation check.
/// </summary>
public sealed class EscalationResult
{
    /// <summary>
    /// Whether escalation is needed.
    /// </summary>
    public bool IsEscalationNeeded { get; init; }

    /// <summary>
    /// Escalation actions to perform.
    /// </summary>
    public List<EscalationAction> EscalationActions { get; init; } = new();

    /// <summary>
    /// Reason for escalation.
    /// </summary>
    public string? EscalationReason { get; init; }

    /// <summary>
    /// Additional metadata about the escalation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when the escalation check was performed.
    /// </summary>
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates an escalation needed result.
    /// </summary>
    /// <param name="escalationActions">Actions to perform.</param>
    /// <param name="reason">Reason for escalation.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>An escalation needed result.</returns>
    public static EscalationResult EscalationNeeded(
        List<EscalationAction> escalationActions,
        string reason,
        Dictionary<string, object>? metadata = null)
    {
        return new EscalationResult
        {
            IsEscalationNeeded = true,
            EscalationActions = escalationActions,
            EscalationReason = reason,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a no escalation needed result.
    /// </summary>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A no escalation needed result.</returns>
    public static EscalationResult NoEscalationNeeded(Dictionary<string, object>? metadata = null)
    {
        return new EscalationResult
        {
            IsEscalationNeeded = false,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}