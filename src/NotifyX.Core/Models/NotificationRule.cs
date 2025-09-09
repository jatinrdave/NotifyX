using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents a notification rule that defines when and how notifications should be processed.
/// Rules can include conditions, actions, and workflow logic.
/// </summary>
public sealed class NotificationRule
{
    /// <summary>
    /// Unique identifier for this rule.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The tenant/organization this rule belongs to.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable name for this rule.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of what this rule does.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Whether this rule is currently active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Priority of this rule (higher numbers = higher priority).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// The conditions that must be met for this rule to apply.
    /// </summary>
    public RuleCondition Condition { get; init; } = new();

    /// <summary>
    /// The actions to perform when this rule matches.
    /// </summary>
    public List<RuleAction> Actions { get; init; } = new();

    /// <summary>
    /// Tags for categorization and filtering.
    /// </summary>
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Custom metadata associated with this rule.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when this rule was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this rule was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this rule.
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Who last updated this rule.
    /// </summary>
    public string UpdatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Version number for this rule.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Whether this rule should be evaluated in parallel with other rules.
    /// </summary>
    public bool EvaluateInParallel { get; init; } = false;

    /// <summary>
    /// Timeout for rule evaluation.
    /// </summary>
    public TimeSpan? EvaluationTimeout { get; init; }

    /// <summary>
    /// Creates a copy of this rule with updated properties.
    /// </summary>
    /// <param name="updater">Action to update the rule properties.</param>
    /// <returns>A new NotificationRule with updated properties.</returns>
    public NotificationRule With(Action<NotificationRuleBuilder> updater)
    {
        var builder = new NotificationRuleBuilder(this);
        updater(builder);
        return builder.Build();
    }
}

/// <summary>
/// Represents a condition that must be met for a rule to apply.
/// </summary>
public sealed class RuleCondition
{
    /// <summary>
    /// The type of condition.
    /// </summary>
    public ConditionType Type { get; init; } = ConditionType.EventType;

    /// <summary>
    /// The operator to use for comparison.
    /// </summary>
    public ConditionOperator Operator { get; init; } = ConditionOperator.Equals;

    /// <summary>
    /// The field path to evaluate (e.g., "EventType", "Priority", "Metadata.Status").
    /// </summary>
    public string FieldPath { get; init; } = string.Empty;

    /// <summary>
    /// The expected value(s) for comparison.
    /// </summary>
    public List<object> ExpectedValues { get; init; } = new();

    /// <summary>
    /// Nested conditions for complex logic (AND/OR operations).
    /// </summary>
    public List<RuleCondition> NestedConditions { get; init; } = new();

    /// <summary>
    /// Logical operator for combining nested conditions.
    /// </summary>
    public LogicalOperator LogicalOperator { get; init; } = LogicalOperator.And;

    /// <summary>
    /// Whether this condition should be case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; init; } = true;

    /// <summary>
    /// Custom condition evaluator for complex scenarios.
    /// </summary>
    public string? CustomEvaluator { get; init; }

    /// <summary>
    /// Parameters for the custom evaluator.
    /// </summary>
    public Dictionary<string, object> CustomEvaluatorParameters { get; init; } = new();
}

/// <summary>
/// Types of conditions that can be evaluated.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConditionType
{
    /// <summary>
    /// Event type condition.
    /// </summary>
    EventType = 0,

    /// <summary>
    /// Priority condition.
    /// </summary>
    Priority = 1,

    /// <summary>
    /// Tenant condition.
    /// </summary>
    Tenant = 2,

    /// <summary>
    /// Metadata condition.
    /// </summary>
    Metadata = 3,

    /// <summary>
    /// Recipient condition.
    /// </summary>
    Recipient = 4,

    /// <summary>
    /// Time-based condition.
    /// </summary>
    TimeBased = 5,

    /// <summary>
    /// Custom condition.
    /// </summary>
    Custom = 6
}

/// <summary>
/// Operators for condition evaluation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConditionOperator
{
    /// <summary>
    /// Equality operator.
    /// </summary>
    Equals = 0,

    /// <summary>
    /// Inequality operator.
    /// </summary>
    NotEquals = 1,

    /// <summary>
    /// Greater than operator.
    /// </summary>
    GreaterThan = 2,

    /// <summary>
    /// Greater than or equal operator.
    /// </summary>
    GreaterThanOrEqual = 3,

    /// <summary>
    /// Less than operator.
    /// </summary>
    LessThan = 4,

    /// <summary>
    /// Less than or equal operator.
    /// </summary>
    LessThanOrEqual = 5,

    /// <summary>
    /// Contains operator.
    /// </summary>
    Contains = 6,

    /// <summary>
    /// Does not contain operator.
    /// </summary>
    DoesNotContain = 7,

    /// <summary>
    /// Starts with operator.
    /// </summary>
    StartsWith = 8,

    /// <summary>
    /// Ends with operator.
    /// </summary>
    EndsWith = 9,

    /// <summary>
    /// Regular expression match operator.
    /// </summary>
    Regex = 10,

    /// <summary>
    /// In operator (value is in list).
    /// </summary>
    In = 11,

    /// <summary>
    /// Not in operator (value is not in list).
    /// </summary>
    NotIn = 12,

    /// <summary>
    /// Is null operator.
    /// </summary>
    IsNull = 13,

    /// <summary>
    /// Is not null operator.
    /// </summary>
    IsNotNull = 14
}

/// <summary>
/// Logical operators for combining conditions.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogicalOperator
{
    /// <summary>
    /// AND operator - all conditions must be true.
    /// </summary>
    And = 0,

    /// <summary>
    /// OR operator - at least one condition must be true.
    /// </summary>
    Or = 1,

    /// <summary>
    /// NOT operator - condition must be false.
    /// </summary>
    Not = 2
}

/// <summary>
/// Represents an action to perform when a rule matches.
/// </summary>
public sealed class RuleAction
{
    /// <summary>
    /// The type of action to perform.
    /// </summary>
    public ActionType Type { get; init; } = ActionType.SendNotification;

    /// <summary>
    /// Parameters for the action.
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();

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
    public ActionRetryConfiguration? RetryConfiguration { get; init; }
}

/// <summary>
/// Types of actions that can be performed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActionType
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
    /// Custom action.
    /// </summary>
    Custom = 10
}

/// <summary>
/// Retry configuration for rule actions.
/// </summary>
public sealed class ActionRetryConfiguration
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
}

/// <summary>
/// Builder class for creating NotificationRule instances with fluent API.
/// </summary>
public sealed class NotificationRuleBuilder
{
    private readonly NotificationRule _rule;

    internal NotificationRuleBuilder(NotificationRule rule)
    {
        _rule = rule;
    }

    /// <summary>
    /// Sets the tenant ID.
    /// </summary>
    public NotificationRuleBuilder WithTenantId(string tenantId)
    {
        return new NotificationRuleBuilder(_rule with { TenantId = tenantId });
    }

    /// <summary>
    /// Sets the rule name.
    /// </summary>
    public NotificationRuleBuilder WithName(string name)
    {
        return new NotificationRuleBuilder(_rule with { Name = name });
    }

    /// <summary>
    /// Sets the rule description.
    /// </summary>
    public NotificationRuleBuilder WithDescription(string description)
    {
        return new NotificationRuleBuilder(_rule with { Description = description });
    }

    /// <summary>
    /// Sets the active status.
    /// </summary>
    public NotificationRuleBuilder WithActiveStatus(bool isActive)
    {
        return new NotificationRuleBuilder(_rule with { IsActive = isActive });
    }

    /// <summary>
    /// Sets the priority.
    /// </summary>
    public NotificationRuleBuilder WithPriority(int priority)
    {
        return new NotificationRuleBuilder(_rule with { Priority = priority });
    }

    /// <summary>
    /// Sets the condition.
    /// </summary>
    public NotificationRuleBuilder WithCondition(RuleCondition condition)
    {
        return new NotificationRuleBuilder(_rule with { Condition = condition });
    }

    /// <summary>
    /// Adds an action.
    /// </summary>
    public NotificationRuleBuilder WithAction(RuleAction action)
    {
        var actions = new List<RuleAction>(_rule.Actions) { action };
        return new NotificationRuleBuilder(_rule with { Actions = actions });
    }

    /// <summary>
    /// Adds a tag.
    /// </summary>
    public NotificationRuleBuilder WithTag(string tag)
    {
        var tags = new List<string>(_rule.Tags) { tag };
        return new NotificationRuleBuilder(_rule with { Tags = tags });
    }

    /// <summary>
    /// Adds metadata.
    /// </summary>
    public NotificationRuleBuilder WithMetadata(string key, object value)
    {
        var metadata = new Dictionary<string, object>(_rule.Metadata) { [key] = value };
        return new NotificationRuleBuilder(_rule with { Metadata = metadata });
    }

    /// <summary>
    /// Sets the created by user.
    /// </summary>
    public NotificationRuleBuilder WithCreatedBy(string createdBy)
    {
        return new NotificationRuleBuilder(_rule with { CreatedBy = createdBy });
    }

    /// <summary>
    /// Sets the updated by user.
    /// </summary>
    public NotificationRuleBuilder WithUpdatedBy(string updatedBy)
    {
        return new NotificationRuleBuilder(_rule with { UpdatedBy = updatedBy });
    }

    /// <summary>
    /// Sets the version.
    /// </summary>
    public NotificationRuleBuilder WithVersion(int version)
    {
        return new NotificationRuleBuilder(_rule with { Version = version });
    }

    /// <summary>
    /// Sets the parallel evaluation flag.
    /// </summary>
    public NotificationRuleBuilder WithParallelEvaluation(bool evaluateInParallel)
    {
        return new NotificationRuleBuilder(_rule with { EvaluateInParallel = evaluateInParallel });
    }

    /// <summary>
    /// Sets the evaluation timeout.
    /// </summary>
    public NotificationRuleBuilder WithEvaluationTimeout(TimeSpan timeout)
    {
        return new NotificationRuleBuilder(_rule with { EvaluationTimeout = timeout });
    }

    /// <summary>
    /// Builds the final NotificationRule.
    /// </summary>
    public NotificationRule Build() => _rule;
}