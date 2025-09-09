using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents a notification event that can be processed by the notification system.
/// This is the core entity that flows through the notification pipeline.
/// </summary>
public sealed record NotificationEvent
{
    /// <summary>
    /// Unique identifier for this notification event.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The tenant/organization this notification belongs to.
    /// Used for multi-tenant isolation and rate limiting.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// The type/category of the event (e.g., "order.shipped", "payment.failed").
    /// Used for rule matching and routing.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Priority level of the notification.
    /// Affects delivery order and escalation behavior.
    /// </summary>
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;

    /// <summary>
    /// The subject or title of the notification.
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// The main content/body of the notification.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Template ID to use for rendering this notification.
    /// If provided, Content will be used as fallback or for template variables.
    /// </summary>
    public string? TemplateId { get; init; }

    /// <summary>
    /// Template variables for interpolation.
    /// Key-value pairs that can be substituted in templates.
    /// </summary>
    public Dictionary<string, object> TemplateVariables { get; init; } = new();

    /// <summary>
    /// Target recipients for this notification.
    /// </summary>
    public List<NotificationRecipient> Recipients { get; init; } = new();

    /// <summary>
    /// Preferred delivery channels in order of preference.
    /// The system will attempt delivery through these channels.
    /// </summary>
    public List<NotificationChannel> PreferredChannels { get; init; } = new();

    /// <summary>
    /// Delivery options and constraints.
    /// </summary>
    public DeliveryOptions DeliveryOptions { get; init; } = new();

    /// <summary>
    /// Custom metadata associated with this notification.
    /// Can be used for filtering, routing, or audit purposes.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when this event was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this event should be delivered (for scheduling).
    /// If null, deliver immediately.
    /// </summary>
    public DateTime? ScheduledFor { get; init; }

    /// <summary>
    /// Time-to-live for this notification.
    /// After this time, the notification will be considered expired.
    /// </summary>
    public TimeSpan? TimeToLive { get; init; }

    /// <summary>
    /// Correlation ID for tracking related notifications.
    /// Useful for grouping notifications from the same business process.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Source system or application that generated this event.
    /// </summary>
    public string? Source { get; init; }

    /// <summary>
    /// Tags for categorization and filtering.
    /// </summary>
    public List<string> Tags { get; init; } = new();

    /// <summary>
    /// Whether this notification should be aggregated with similar ones.
    /// </summary>
    public bool ShouldAggregate { get; init; } = false;

    /// <summary>
    /// Aggregation key for grouping similar notifications.
    /// </summary>
    public string? AggregationKey { get; init; }

    /// <summary>
    /// Whether this notification requires acknowledgment.
    /// </summary>
    public bool RequiresAcknowledgment { get; init; } = false;

    /// <summary>
    /// Timeout for acknowledgment before escalation.
    /// </summary>
    public TimeSpan? AcknowledgmentTimeout { get; init; }

    /// <summary>
    /// Creates a copy of this notification event with updated properties.
    /// </summary>
    /// <param name="updater">Action to update the notification properties.</param>
    /// <returns>A new NotificationEvent with updated properties.</returns>
    public NotificationEvent With(Action<NotificationEventBuilder> updater)
    {
        var builder = new NotificationEventBuilder(this);
        updater(builder);
        return builder.Build();
    }
}

/// <summary>
/// Priority levels for notifications.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationPriority
{
    /// <summary>
    /// Low priority - can be delayed or batched.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority - standard delivery.
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority - expedited delivery.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority - immediate delivery with escalation.
    /// </summary>
    Critical = 3
}

/// <summary>
/// Builder class for creating NotificationEvent instances with fluent API.
/// </summary>
public sealed class NotificationEventBuilder
{
    private readonly NotificationEvent _event;

    public NotificationEventBuilder(NotificationEvent notificationEvent)
    {
        _event = notificationEvent;
    }

    /// <summary>
    /// Sets the tenant ID.
    /// </summary>
    public NotificationEventBuilder WithTenantId(string tenantId)
    {
        return new NotificationEventBuilder(_event with { TenantId = tenantId });
    }

    /// <summary>
    /// Sets the event type.
    /// </summary>
    public NotificationEventBuilder WithEventType(string eventType)
    {
        return new NotificationEventBuilder(_event with { EventType = eventType });
    }

    /// <summary>
    /// Sets the priority.
    /// </summary>
    public NotificationEventBuilder WithPriority(NotificationPriority priority)
    {
        return new NotificationEventBuilder(_event with { Priority = priority });
    }

    /// <summary>
    /// Sets the subject.
    /// </summary>
    public NotificationEventBuilder WithSubject(string subject)
    {
        return new NotificationEventBuilder(_event with { Subject = subject });
    }

    /// <summary>
    /// Sets the content.
    /// </summary>
    public NotificationEventBuilder WithContent(string content)
    {
        return new NotificationEventBuilder(_event with { Content = content });
    }

    /// <summary>
    /// Sets the template ID.
    /// </summary>
    public NotificationEventBuilder WithTemplateId(string templateId)
    {
        return new NotificationEventBuilder(_event with { TemplateId = templateId });
    }

    /// <summary>
    /// Adds a template variable.
    /// </summary>
    public NotificationEventBuilder WithTemplateVariable(string key, object value)
    {
        var variables = new Dictionary<string, object>(_event.TemplateVariables) { [key] = value };
        return new NotificationEventBuilder(_event with { TemplateVariables = variables });
    }

    /// <summary>
    /// Adds a recipient.
    /// </summary>
    public NotificationEventBuilder WithRecipient(NotificationRecipient recipient)
    {
        var recipients = new List<NotificationRecipient>(_event.Recipients) { recipient };
        return new NotificationEventBuilder(_event with { Recipients = recipients });
    }

    /// <summary>
    /// Adds a preferred channel.
    /// </summary>
    public NotificationEventBuilder WithPreferredChannel(NotificationChannel channel)
    {
        var channels = new List<NotificationChannel>(_event.PreferredChannels) { channel };
        return new NotificationEventBuilder(_event with { PreferredChannels = channels });
    }

    /// <summary>
    /// Sets delivery options.
    /// </summary>
    public NotificationEventBuilder WithDeliveryOptions(DeliveryOptions options)
    {
        return new NotificationEventBuilder(_event with { DeliveryOptions = options });
    }

    /// <summary>
    /// Adds metadata.
    /// </summary>
    public NotificationEventBuilder WithMetadata(string key, object value)
    {
        var metadata = new Dictionary<string, object>(_event.Metadata) { [key] = value };
        return new NotificationEventBuilder(_event with { Metadata = metadata });
    }

    /// <summary>
    /// Sets the scheduled delivery time.
    /// </summary>
    public NotificationEventBuilder WithScheduledFor(DateTime scheduledFor)
    {
        return new NotificationEventBuilder(_event with { ScheduledFor = scheduledFor });
    }

    /// <summary>
    /// Sets the time-to-live.
    /// </summary>
    public NotificationEventBuilder WithTimeToLive(TimeSpan timeToLive)
    {
        return new NotificationEventBuilder(_event with { TimeToLive = timeToLive });
    }

    /// <summary>
    /// Sets the correlation ID.
    /// </summary>
    public NotificationEventBuilder WithCorrelationId(string correlationId)
    {
        return new NotificationEventBuilder(_event with { CorrelationId = correlationId });
    }

    /// <summary>
    /// Sets the source.
    /// </summary>
    public NotificationEventBuilder WithSource(string source)
    {
        return new NotificationEventBuilder(_event with { Source = source });
    }

    /// <summary>
    /// Adds a tag.
    /// </summary>
    public NotificationEventBuilder WithTag(string tag)
    {
        var tags = new List<string>(_event.Tags) { tag };
        return new NotificationEventBuilder(_event with { Tags = tags });
    }

    /// <summary>
    /// Sets aggregation options.
    /// </summary>
    public NotificationEventBuilder WithAggregation(bool shouldAggregate, string? aggregationKey = null)
    {
        return new NotificationEventBuilder(_event with 
        { 
            ShouldAggregate = shouldAggregate, 
            AggregationKey = aggregationKey 
        });
    }

    /// <summary>
    /// Sets acknowledgment requirements.
    /// </summary>
    public NotificationEventBuilder WithAcknowledgment(bool requiresAcknowledgment, TimeSpan? timeout = null)
    {
        return new NotificationEventBuilder(_event with 
        { 
            RequiresAcknowledgment = requiresAcknowledgment, 
            AcknowledgmentTimeout = timeout 
        });
    }

    /// <summary>
    /// Builds the final NotificationEvent.
    /// </summary>
    public NotificationEvent Build() => _event;
}