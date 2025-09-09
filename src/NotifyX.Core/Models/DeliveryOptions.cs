using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Delivery options and constraints for notifications.
/// </summary>
public sealed class DeliveryOptions
{
    /// <summary>
    /// Delivery guarantee level.
    /// </summary>
    public DeliveryGuarantee Guarantee { get; init; } = DeliveryGuarantee.AtLeastOnce;

    /// <summary>
    /// Maximum number of delivery attempts.
    /// </summary>
    public int MaxAttempts { get; init; } = 3;

    /// <summary>
    /// Timeout for individual delivery attempts.
    /// </summary>
    public TimeSpan AttemptTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether to retry on failure.
    /// </summary>
    public bool RetryOnFailure { get; init; } = true;

    /// <summary>
    /// Retry strategy to use.
    /// </summary>
    public RetryStrategy RetryStrategy { get; init; } = RetryStrategy.ExponentialBackoff;

    /// <summary>
    /// Initial delay before first retry.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum delay between retries.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Backoff multiplier for exponential backoff.
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Whether to use jitter in retry delays to avoid thundering herd.
    /// </summary>
    public bool UseJitter { get; init; } = true;

    /// <summary>
    /// Whether to fail fast on certain error types.
    /// </summary>
    public bool FailFast { get; init; } = false;

    /// <summary>
    /// Error types that should trigger immediate failure.
    /// </summary>
    public HashSet<string> FailFastErrorTypes { get; init; } = new();

    /// <summary>
    /// Whether to aggregate similar notifications.
    /// </summary>
    public bool EnableAggregation { get; init; } = false;

    /// <summary>
    /// Time window for aggregating notifications.
    /// </summary>
    public TimeSpan AggregationWindow { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum number of notifications to aggregate.
    /// </summary>
    public int MaxAggregationCount { get; init; } = 10;

    /// <summary>
    /// Whether to enable escalation on failure.
    /// </summary>
    public bool EnableEscalation { get; init; } = false;

    /// <summary>
    /// Time to wait before escalating.
    /// </summary>
    public TimeSpan EscalationDelay { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Escalation channels to use.
    /// </summary>
    public List<NotificationChannel> EscalationChannels { get; init; } = new();

    /// <summary>
    /// Whether to enable deduplication.
    /// </summary>
    public bool EnableDeduplication { get; init; } = true;

    /// <summary>
    /// Time window for deduplication.
    /// </summary>
    public TimeSpan DeduplicationWindow { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Whether to enable rate limiting.
    /// </summary>
    public bool EnableRateLimiting { get; init; } = true;

    /// <summary>
    /// Rate limit per recipient per minute.
    /// </summary>
    public int RateLimitPerMinute { get; init; } = 10;

    /// <summary>
    /// Rate limit per recipient per hour.
    /// </summary>
    public int RateLimitPerHour { get; init; } = 100;

    /// <summary>
    /// Whether to enable circuit breaker pattern.
    /// </summary>
    public bool EnableCircuitBreaker { get; init; } = false;

    /// <summary>
    /// Circuit breaker failure threshold.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; init; } = 5;

    /// <summary>
    /// Circuit breaker timeout duration.
    /// </summary>
    public TimeSpan CircuitBreakerTimeout { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Custom delivery options specific to the implementation.
    /// </summary>
    public Dictionary<string, object> CustomOptions { get; init; } = new();

    /// <summary>
    /// Creates a copy of this delivery options with updated properties.
    /// </summary>
    /// <param name="updater">Action to update the delivery options properties.</param>
    /// <returns>A new DeliveryOptions with updated properties.</returns>
    public DeliveryOptions With(Action<DeliveryOptionsBuilder> updater)
    {
        var builder = new DeliveryOptionsBuilder(this);
        updater(builder);
        return builder.Build();
    }
}

/// <summary>
/// Delivery guarantee levels.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeliveryGuarantee
{
    /// <summary>
    /// Fire and forget - no delivery guarantees.
    /// </summary>
    FireAndForget = 0,

    /// <summary>
    /// At least once delivery - may deliver multiple times.
    /// </summary>
    AtLeastOnce = 1,

    /// <summary>
    /// Exactly once delivery - guaranteed single delivery.
    /// </summary>
    ExactlyOnce = 2
}

/// <summary>
/// Retry strategies for failed deliveries.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RetryStrategy
{
    /// <summary>
    /// No retries.
    /// </summary>
    None = 0,

    /// <summary>
    /// Fixed delay between retries.
    /// </summary>
    FixedDelay = 1,

    /// <summary>
    /// Linear backoff - delay increases linearly.
    /// </summary>
    LinearBackoff = 2,

    /// <summary>
    /// Exponential backoff - delay increases exponentially.
    /// </summary>
    ExponentialBackoff = 3,

    /// <summary>
    /// Custom retry strategy.
    /// </summary>
    Custom = 4
}

/// <summary>
/// Builder class for creating DeliveryOptions instances with fluent API.
/// </summary>
public sealed class DeliveryOptionsBuilder
{
    private readonly DeliveryOptions _options;

    internal DeliveryOptionsBuilder(DeliveryOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Sets the delivery guarantee.
    /// </summary>
    public DeliveryOptionsBuilder WithGuarantee(DeliveryGuarantee guarantee)
    {
        return new DeliveryOptionsBuilder(_options with { Guarantee = guarantee });
    }

    /// <summary>
    /// Sets retry configuration.
    /// </summary>
    public DeliveryOptionsBuilder WithRetryConfiguration(
        int maxAttempts,
        TimeSpan attemptTimeout,
        RetryStrategy strategy,
        TimeSpan initialDelay,
        TimeSpan maxDelay,
        double backoffMultiplier = 2.0,
        bool useJitter = true)
    {
        return new DeliveryOptionsBuilder(_options with
        {
            MaxAttempts = maxAttempts,
            AttemptTimeout = attemptTimeout,
            RetryOnFailure = maxAttempts > 1,
            RetryStrategy = strategy,
            InitialRetryDelay = initialDelay,
            MaxRetryDelay = maxDelay,
            BackoffMultiplier = backoffMultiplier,
            UseJitter = useJitter
        });
    }

    /// <summary>
    /// Sets fail-fast configuration.
    /// </summary>
    public DeliveryOptionsBuilder WithFailFast(bool failFast, HashSet<string>? errorTypes = null)
    {
        return new DeliveryOptionsBuilder(_options with
        {
            FailFast = failFast,
            FailFastErrorTypes = errorTypes ?? new HashSet<string>()
        });
    }

    /// <summary>
    /// Sets aggregation configuration.
    /// </summary>
    public DeliveryOptionsBuilder WithAggregation(
        bool enableAggregation,
        TimeSpan aggregationWindow,
        int maxAggregationCount)
    {
        return new DeliveryOptionsBuilder(_options with
        {
            EnableAggregation = enableAggregation,
            AggregationWindow = aggregationWindow,
            MaxAggregationCount = maxAggregationCount
        });
    }

    /// <summary>
    /// Sets escalation configuration.
    /// </summary>
    public DeliveryOptionsBuilder WithEscalation(
        bool enableEscalation,
        TimeSpan escalationDelay,
        List<NotificationChannel> escalationChannels)
    {
        return new DeliveryOptionsBuilder(_options with
        {
            EnableEscalation = enableEscalation,
            EscalationDelay = escalationDelay,
            EscalationChannels = escalationChannels
        });
    }

    /// <summary>
    /// Sets deduplication configuration.
    /// </summary>
    public DeliveryOptionsBuilder WithDeduplication(bool enableDeduplication, TimeSpan deduplicationWindow)
    {
        return new DeliveryOptionsBuilder(_options with
        {
            EnableDeduplication = enableDeduplication,
            DeduplicationWindow = deduplicationWindow
        });
    }

    /// <summary>
    /// Sets rate limiting configuration.
    /// </summary>
    public DeliveryOptionsBuilder WithRateLimiting(
        bool enableRateLimiting,
        int rateLimitPerMinute,
        int rateLimitPerHour)
    {
        return new DeliveryOptionsBuilder(_options with
        {
            EnableRateLimiting = enableRateLimiting,
            RateLimitPerMinute = rateLimitPerMinute,
            RateLimitPerHour = rateLimitPerHour
        });
    }

    /// <summary>
    /// Sets circuit breaker configuration.
    /// </summary>
    public DeliveryOptionsBuilder WithCircuitBreaker(
        bool enableCircuitBreaker,
        int failureThreshold,
        TimeSpan timeout)
    {
        return new DeliveryOptionsBuilder(_options with
        {
            EnableCircuitBreaker = enableCircuitBreaker,
            CircuitBreakerFailureThreshold = failureThreshold,
            CircuitBreakerTimeout = timeout
        });
    }

    /// <summary>
    /// Adds a custom option.
    /// </summary>
    public DeliveryOptionsBuilder WithCustomOption(string key, object value)
    {
        var customOptions = new Dictionary<string, object>(_options.CustomOptions) { [key] = value };
        return new DeliveryOptionsBuilder(_options with { CustomOptions = customOptions });
    }

    /// <summary>
    /// Builds the final DeliveryOptions.
    /// </summary>
    public DeliveryOptions Build() => _options;
}