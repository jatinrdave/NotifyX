using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents a notification subscription that defines how a recipient should receive notifications.
/// </summary>
public sealed record NotificationSubscription
{
    /// <summary>
    /// Unique identifier for this subscription.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The tenant/organization this subscription belongs to.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// The recipient this subscription is for.
    /// </summary>
    public NotificationRecipient Recipient { get; init; } = new();

    /// <summary>
    /// Event types this subscription covers.
    /// If empty, covers all event types.
    /// </summary>
    public List<string> EventTypes { get; init; } = new();

    /// <summary>
    /// Channels this subscription is active for.
    /// </summary>
    public List<NotificationChannel> Channels { get; init; } = new();

    /// <summary>
    /// Priority levels this subscription covers.
    /// If empty, covers all priority levels.
    /// </summary>
    public List<NotificationPriority> PriorityLevels { get; init; } = new();

    /// <summary>
    /// Whether this subscription is currently active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Whether this subscription is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Delivery preferences for this subscription.
    /// </summary>
    public SubscriptionDeliveryPreferences DeliveryPreferences { get; init; } = new();

    /// <summary>
    /// Filtering rules for this subscription.
    /// </summary>
    public List<SubscriptionFilter> Filters { get; init; } = new();

    /// <summary>
    /// Tags for categorization and filtering.
    /// </summary>
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Custom metadata associated with this subscription.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when this subscription was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this subscription was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this subscription.
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Who last updated this subscription.
    /// </summary>
    public string UpdatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Version number for this subscription.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Whether this subscription should be evaluated in parallel with other subscriptions.
    /// </summary>
    public bool EvaluateInParallel { get; init; } = false;

    /// <summary>
    /// Timeout for subscription evaluation.
    /// </summary>
    public TimeSpan? EvaluationTimeout { get; init; }

    /// <summary>
    /// Creates a copy of this subscription with updated properties.
    /// </summary>
    /// <param name="updater">Action to update the subscription properties.</param>
    /// <returns>A new NotificationSubscription with updated properties.</returns>
    public NotificationSubscription With(Action<NotificationSubscriptionBuilder> updater)
    {
        var builder = new NotificationSubscriptionBuilder(this);
        updater(builder);
        return builder.Build();
    }
}

/// <summary>
/// Delivery preferences for a notification subscription.
/// </summary>
public sealed record SubscriptionDeliveryPreferences
{
    /// <summary>
    /// Preferred delivery time window.
    /// </summary>
    public DeliveryTimeWindow PreferredTimeWindow { get; init; } = new();

    /// <summary>
    /// Quiet hours when notifications should not be sent.
    /// </summary>
    public List<QuietHour> QuietHours { get; init; } = new();

    /// <summary>
    /// Maximum number of notifications per day.
    /// </summary>
    public int MaxNotificationsPerDay { get; init; } = 100;

    /// <summary>
    /// Maximum number of notifications per hour.
    /// </summary>
    public int MaxNotificationsPerHour { get; init; } = 10;

    /// <summary>
    /// Whether to enable digest mode (batch notifications).
    /// </summary>
    public bool EnableDigestMode { get; init; } = false;

    /// <summary>
    /// Digest frequency when digest mode is enabled.
    /// </summary>
    public DigestFrequency DigestFrequency { get; init; } = DigestFrequency.Daily;

    /// <summary>
    /// Whether to enable escalation for failed deliveries.
    /// </summary>
    public bool EnableEscalation { get; init; } = false;

    /// <summary>
    /// Escalation delay before trying alternative channels.
    /// </summary>
    public TimeSpan EscalationDelay { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Escalation channels to use.
    /// </summary>
    public List<NotificationChannel> EscalationChannels { get; init; } = new();

    /// <summary>
    /// Whether to enable rate limiting.
    /// </summary>
    public bool EnableRateLimiting { get; init; } = true;

    /// <summary>
    /// Rate limit per minute.
    /// </summary>
    public int RateLimitPerMinute { get; init; } = 5;

    /// <summary>
    /// Rate limit per hour.
    /// </summary>
    public int RateLimitPerHour { get; init; } = 50;
}

/// <summary>
/// Delivery time window for notifications.
/// </summary>
public sealed record DeliveryTimeWindow
{
    /// <summary>
    /// Start time of the delivery window (24-hour format).
    /// </summary>
    public TimeSpan StartTime { get; init; } = TimeSpan.FromHours(9); // 9:00 AM

    /// <summary>
    /// End time of the delivery window (24-hour format).
    /// </summary>
    public TimeSpan EndTime { get; init; } = TimeSpan.FromHours(17); // 5:00 PM

    /// <summary>
    /// Days of the week when delivery is allowed.
    /// </summary>
    public List<DayOfWeek> AllowedDays { get; init; } = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    /// <summary>
    /// Time zone for the delivery window.
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Whether to respect the delivery time window.
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}

/// <summary>
/// Quiet hour configuration for notifications.
/// </summary>
public sealed record QuietHour
{
    /// <summary>
    /// Start time of the quiet hour (24-hour format).
    /// </summary>
    public TimeSpan StartTime { get; init; }

    /// <summary>
    /// End time of the quiet hour (24-hour format).
    /// </summary>
    public TimeSpan EndTime { get; init; }

    /// <summary>
    /// Days of the week when quiet hours apply.
    /// </summary>
    public List<DayOfWeek> Days { get; init; } = new();

    /// <summary>
    /// Time zone for the quiet hour.
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Whether this quiet hour is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Priority levels that are exempt from quiet hours.
    /// </summary>
    public List<NotificationPriority> ExemptPriorities { get; init; } = new()
    {
        NotificationPriority.Critical
    };
}

/// <summary>
/// Digest frequency options.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DigestFrequency
{
    /// <summary>
    /// No digest - send notifications immediately.
    /// </summary>
    None = 0,

    /// <summary>
    /// Hourly digest.
    /// </summary>
    Hourly = 1,

    /// <summary>
    /// Daily digest.
    /// </summary>
    Daily = 2,

    /// <summary>
    /// Weekly digest.
    /// </summary>
    Weekly = 3,

    /// <summary>
    /// Monthly digest.
    /// </summary>
    Monthly = 4
}

/// <summary>
/// Filter for notification subscriptions.
/// </summary>
public sealed record SubscriptionFilter
{
    /// <summary>
    /// The type of filter.
    /// </summary>
    public SubscriptionFilterType Type { get; init; } = SubscriptionFilterType.EventType;

    /// <summary>
    /// The field path to filter on.
    /// </summary>
    public string FieldPath { get; init; } = string.Empty;

    /// <summary>
    /// The operator to use for filtering.
    /// </summary>
    public ConditionOperator Operator { get; init; } = ConditionOperator.Equals;

    /// <summary>
    /// The values to filter by.
    /// </summary>
    public List<object> Values { get; init; } = new();

    /// <summary>
    /// Whether this filter should be case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; init; } = true;

    /// <summary>
    /// Whether this filter is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;
}

/// <summary>
/// Types of subscription filters.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionFilterType
{
    /// <summary>
    /// Event type filter.
    /// </summary>
    EventType = 0,

    /// <summary>
    /// Priority filter.
    /// </summary>
    Priority = 1,

    /// <summary>
    /// Source filter.
    /// </summary>
    Source = 2,

    /// <summary>
    /// Tag filter.
    /// </summary>
    Tag = 3,

    /// <summary>
    /// Metadata filter.
    /// </summary>
    Metadata = 4,

    /// <summary>
    /// Custom filter.
    /// </summary>
    Custom = 5
}

/// <summary>
/// Builder class for creating NotificationSubscription instances with fluent API.
/// </summary>
public sealed class NotificationSubscriptionBuilder
{
    private readonly NotificationSubscription _subscription;

    internal NotificationSubscriptionBuilder(NotificationSubscription subscription)
    {
        _subscription = subscription;
    }

    /// <summary>
    /// Sets the tenant ID.
    /// </summary>
    public NotificationSubscriptionBuilder WithTenantId(string tenantId)
    {
        return new NotificationSubscriptionBuilder(_subscription with { TenantId = tenantId });
    }

    /// <summary>
    /// Sets the recipient.
    /// </summary>
    public NotificationSubscriptionBuilder WithRecipient(NotificationRecipient recipient)
    {
        return new NotificationSubscriptionBuilder(_subscription with { Recipient = recipient });
    }

    /// <summary>
    /// Adds an event type.
    /// </summary>
    public NotificationSubscriptionBuilder WithEventType(string eventType)
    {
        var eventTypes = new List<string>(_subscription.EventTypes) { eventType };
        return new NotificationSubscriptionBuilder(_subscription with { EventTypes = eventTypes });
    }

    /// <summary>
    /// Adds a channel.
    /// </summary>
    public NotificationSubscriptionBuilder WithChannel(NotificationChannel channel)
    {
        var channels = new List<NotificationChannel>(_subscription.Channels) { channel };
        return new NotificationSubscriptionBuilder(_subscription with { Channels = channels });
    }

    /// <summary>
    /// Adds a priority level.
    /// </summary>
    public NotificationSubscriptionBuilder WithPriorityLevel(NotificationPriority priority)
    {
        var priorities = new List<NotificationPriority>(_subscription.PriorityLevels) { priority };
        return new NotificationSubscriptionBuilder(_subscription with { PriorityLevels = priorities });
    }

    /// <summary>
    /// Sets the active status.
    /// </summary>
    public NotificationSubscriptionBuilder WithActiveStatus(bool isActive)
    {
        return new NotificationSubscriptionBuilder(_subscription with { IsActive = isActive });
    }

    /// <summary>
    /// Sets the enabled status.
    /// </summary>
    public NotificationSubscriptionBuilder WithEnabledStatus(bool isEnabled)
    {
        return new NotificationSubscriptionBuilder(_subscription with { IsEnabled = isEnabled });
    }

    /// <summary>
    /// Sets the delivery preferences.
    /// </summary>
    public NotificationSubscriptionBuilder WithDeliveryPreferences(SubscriptionDeliveryPreferences preferences)
    {
        return new NotificationSubscriptionBuilder(_subscription with { DeliveryPreferences = preferences });
    }

    /// <summary>
    /// Adds a filter.
    /// </summary>
    public NotificationSubscriptionBuilder WithFilter(SubscriptionFilter filter)
    {
        var filters = new List<SubscriptionFilter>(_subscription.Filters) { filter };
        return new NotificationSubscriptionBuilder(_subscription with { Filters = filters });
    }

    /// <summary>
    /// Adds a tag.
    /// </summary>
    public NotificationSubscriptionBuilder WithTag(string tag)
    {
        var tags = new List<string>(_subscription.Tags) { tag };
        return new NotificationSubscriptionBuilder(_subscription with { Tags = tags });
    }

    /// <summary>
    /// Adds metadata.
    /// </summary>
    public NotificationSubscriptionBuilder WithMetadata(string key, object value)
    {
        var metadata = new Dictionary<string, object>(_subscription.Metadata) { [key] = value };
        return new NotificationSubscriptionBuilder(_subscription with { Metadata = metadata });
    }

    /// <summary>
    /// Sets the created by user.
    /// </summary>
    public NotificationSubscriptionBuilder WithCreatedBy(string createdBy)
    {
        return new NotificationSubscriptionBuilder(_subscription with { CreatedBy = createdBy });
    }

    /// <summary>
    /// Sets the updated by user.
    /// </summary>
    public NotificationSubscriptionBuilder WithUpdatedBy(string updatedBy)
    {
        return new NotificationSubscriptionBuilder(_subscription with { UpdatedBy = updatedBy });
    }

    /// <summary>
    /// Sets the version.
    /// </summary>
    public NotificationSubscriptionBuilder WithVersion(int version)
    {
        return new NotificationSubscriptionBuilder(_subscription with { Version = version });
    }

    /// <summary>
    /// Sets the parallel evaluation flag.
    /// </summary>
    public NotificationSubscriptionBuilder WithParallelEvaluation(bool evaluateInParallel)
    {
        return new NotificationSubscriptionBuilder(_subscription with { EvaluateInParallel = evaluateInParallel });
    }

    /// <summary>
    /// Sets the evaluation timeout.
    /// </summary>
    public NotificationSubscriptionBuilder WithEvaluationTimeout(TimeSpan timeout)
    {
        return new NotificationSubscriptionBuilder(_subscription with { EvaluationTimeout = timeout });
    }

    /// <summary>
    /// Builds the final NotificationSubscription.
    /// </summary>
    public NotificationSubscription Build() => _subscription;
}