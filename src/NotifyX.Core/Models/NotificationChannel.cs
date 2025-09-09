using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Supported notification channels for delivery.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationChannel
{
    /// <summary>
    /// Email delivery via SMTP or email service providers.
    /// </summary>
    Email = 0,

    /// <summary>
    /// SMS delivery via SMS service providers.
    /// </summary>
    SMS = 1,

    /// <summary>
    /// Push notifications to mobile devices and web browsers.
    /// </summary>
    Push = 2,

    /// <summary>
    /// Webhook delivery to custom endpoints.
    /// </summary>
    Webhook = 3,

    /// <summary>
    /// Slack notifications.
    /// </summary>
    Slack = 4,

    /// <summary>
    /// Microsoft Teams notifications.
    /// </summary>
    Teams = 5,

    /// <summary>
    /// Discord notifications.
    /// </summary>
    Discord = 6,

    /// <summary>
    /// WhatsApp notifications.
    /// </summary>
    WhatsApp = 7,

    /// <summary>
    /// Telegram notifications.
    /// </summary>
    Telegram = 8,

    /// <summary>
    /// In-app notifications within the application.
    /// </summary>
    InApp = 9
}

/// <summary>
/// Channel-specific configuration and metadata.
/// </summary>
public sealed record ChannelConfiguration
{
    /// <summary>
    /// The notification channel this configuration applies to.
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Whether this channel is enabled for the tenant.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Priority order for this channel (lower numbers = higher priority).
    /// </summary>
    public int Priority { get; init; } = 0;

    /// <summary>
    /// Maximum number of notifications per minute for this channel.
    /// </summary>
    public int RateLimitPerMinute { get; init; } = 100;

    /// <summary>
    /// Maximum number of notifications per hour for this channel.
    /// </summary>
    public int RateLimitPerHour { get; init; } = 1000;

    /// <summary>
    /// Maximum number of notifications per day for this channel.
    /// </summary>
    public int RateLimitPerDay { get; init; } = 10000;

    /// <summary>
    /// Channel-specific settings and configuration.
    /// </summary>
    public Dictionary<string, object> Settings { get; init; } = new();

    /// <summary>
    /// Default template ID for this channel.
    /// </summary>
    public string? DefaultTemplateId { get; init; }

    /// <summary>
    /// Whether this channel supports rich content (HTML, attachments, etc.).
    /// </summary>
    public bool SupportsRichContent { get; init; } = false;

    /// <summary>
    /// Whether this channel supports attachments.
    /// </summary>
    public bool SupportsAttachments { get; init; } = false;

    /// <summary>
    /// Maximum content length for this channel.
    /// </summary>
    public int? MaxContentLength { get; init; }

    /// <summary>
    /// Whether this channel requires authentication.
    /// </summary>
    public bool RequiresAuthentication { get; init; } = false;

    /// <summary>
    /// Authentication configuration for this channel.
    /// </summary>
    public Dictionary<string, string> Authentication { get; init; } = new();

    /// <summary>
    /// Retry configuration for this channel.
    /// </summary>
    public ChannelRetryConfiguration RetryConfiguration { get; init; } = new();

    /// <summary>
    /// Creates a copy of this configuration with updated properties.
    /// </summary>
    /// <param name="updater">Action to update the configuration properties.</param>
    /// <returns>A new ChannelConfiguration with updated properties.</returns>
    public ChannelConfiguration With(Action<ChannelConfigurationBuilder> updater)
    {
        var builder = new ChannelConfigurationBuilder(this);
        updater(builder);
        return builder.Build();
    }
}

/// <summary>
/// Retry configuration for a specific channel.
/// </summary>
public sealed class ChannelRetryConfiguration
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
    /// HTTP status codes that should trigger a retry.
    /// </summary>
    public HashSet<int> RetryableStatusCodes { get; init; } = new() { 408, 429, 500, 502, 503, 504 };

    /// <summary>
    /// Whether to retry on network errors.
    /// </summary>
    public bool RetryOnNetworkErrors { get; init; } = true;
}

/// <summary>
/// Builder class for creating ChannelConfiguration instances with fluent API.
/// </summary>
public sealed class ChannelConfigurationBuilder
{
    private readonly ChannelConfiguration _configuration;

    internal ChannelConfigurationBuilder(ChannelConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Sets the enabled status.
    /// </summary>
    public ChannelConfigurationBuilder WithEnabled(bool isEnabled)
    {
        return new ChannelConfigurationBuilder(_configuration with { IsEnabled = isEnabled });
    }

    /// <summary>
    /// Sets the priority.
    /// </summary>
    public ChannelConfigurationBuilder WithPriority(int priority)
    {
        return new ChannelConfigurationBuilder(_configuration with { Priority = priority });
    }

    /// <summary>
    /// Sets rate limits.
    /// </summary>
    public ChannelConfigurationBuilder WithRateLimits(int perMinute, int perHour, int perDay)
    {
        return new ChannelConfigurationBuilder(_configuration with 
        { 
            RateLimitPerMinute = perMinute,
            RateLimitPerHour = perHour,
            RateLimitPerDay = perDay
        });
    }

    /// <summary>
    /// Sets a channel-specific setting.
    /// </summary>
    public ChannelConfigurationBuilder WithSetting(string key, object value)
    {
        var settings = new Dictionary<string, object>(_configuration.Settings) { [key] = value };
        return new ChannelConfigurationBuilder(_configuration with { Settings = settings });
    }

    /// <summary>
    /// Sets the default template ID.
    /// </summary>
    public ChannelConfigurationBuilder WithDefaultTemplateId(string templateId)
    {
        return new ChannelConfigurationBuilder(_configuration with { DefaultTemplateId = templateId });
    }

    /// <summary>
    /// Sets content support flags.
    /// </summary>
    public ChannelConfigurationBuilder WithContentSupport(bool supportsRichContent, bool supportsAttachments, int? maxContentLength = null)
    {
        return new ChannelConfigurationBuilder(_configuration with 
        { 
            SupportsRichContent = supportsRichContent,
            SupportsAttachments = supportsAttachments,
            MaxContentLength = maxContentLength
        });
    }

    /// <summary>
    /// Sets authentication configuration.
    /// </summary>
    public ChannelConfigurationBuilder WithAuthentication(bool requiresAuth, Dictionary<string, string>? authConfig = null)
    {
        return new ChannelConfigurationBuilder(_configuration with 
        { 
            RequiresAuthentication = requiresAuth,
            Authentication = authConfig ?? new Dictionary<string, string>()
        });
    }

    /// <summary>
    /// Sets retry configuration.
    /// </summary>
    public ChannelConfigurationBuilder WithRetryConfiguration(ChannelRetryConfiguration retryConfig)
    {
        return new ChannelConfigurationBuilder(_configuration with { RetryConfiguration = retryConfig });
    }

    /// <summary>
    /// Builds the final ChannelConfiguration.
    /// </summary>
    public ChannelConfiguration Build() => _configuration;
}