using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents a recipient of a notification with their contact information and preferences.
/// </summary>
public sealed record NotificationRecipient
{
    /// <summary>
    /// Unique identifier for this recipient.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Display name of the recipient.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Email address of the recipient.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Phone number of the recipient (for SMS).
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Push notification token (for mobile/web push).
    /// </summary>
    public string? PushToken { get; init; }

    /// <summary>
    /// Device ID for push notifications.
    /// </summary>
    public string? DeviceId { get; init; }

    /// <summary>
    /// Webhook URL for webhook notifications.
    /// </summary>
    public string? WebhookUrl { get; init; }

    /// <summary>
    /// Additional metadata for the recipient.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// User ID in the source system.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Preferred language for notifications.
    /// </summary>
    public string Language { get; init; } = "en";

    /// <summary>
    /// Time zone of the recipient.
    /// </summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>
    /// Channel preferences for this recipient.
    /// Maps channel types to preference levels.
    /// </summary>
    public Dictionary<NotificationChannel, ChannelPreference> ChannelPreferences { get; init; } = new();

    /// <summary>
    /// Custom properties specific to this recipient.
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();

    /// <summary>
    /// Whether this recipient is active and should receive notifications.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Whether this recipient has opted out of notifications.
    /// </summary>
    public bool IsOptedOut { get; init; } = false;

    /// <summary>
    /// Opt-out preferences by channel.
    /// </summary>
    public HashSet<NotificationChannel> OptedOutChannels { get; init; } = new();

    /// <summary>
    /// Creates a copy of this recipient with updated properties.
    /// </summary>
    /// <param name="updater">Action to update the recipient properties.</param>
    /// <returns>A new NotificationRecipient with updated properties.</returns>
    public NotificationRecipient With(Action<NotificationRecipientBuilder> updater)
    {
        var builder = new NotificationRecipientBuilder(this);
        updater(builder);
        return builder.Build();
    }

    /// <summary>
    /// Gets the best available contact method for the specified channel.
    /// </summary>
    /// <param name="channel">The notification channel.</param>
    /// <returns>The contact information for the channel, or null if not available.</returns>
    public string? GetContactForChannel(NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Email => Email,
            NotificationChannel.SMS => PhoneNumber,
            NotificationChannel.Push => DeviceId ?? PushToken,
            NotificationChannel.Webhook => WebhookUrl,
            _ => null
        };
    }

    /// <summary>
    /// Checks if this recipient can receive notifications through the specified channel.
    /// </summary>
    /// <param name="channel">The notification channel.</param>
    /// <returns>True if the recipient can receive notifications through this channel.</returns>
    public bool CanReceiveOnChannel(NotificationChannel channel)
    {
        if (!IsActive || IsOptedOut)
            return false;

        if (OptedOutChannels.Contains(channel))
            return false;

        // Check if we have contact information for this channel
        var contact = GetContactForChannel(channel);
        if (string.IsNullOrEmpty(contact))
            return false;

        // Check channel preferences
        if (ChannelPreferences.TryGetValue(channel, out var preference))
        {
            return preference != ChannelPreference.Disabled;
        }

        return true; // Default to enabled if no preference is set
    }
}

/// <summary>
/// Channel preference levels for recipients.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChannelPreference
{
    /// <summary>
    /// Channel is disabled for this recipient.
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// Channel is enabled but not preferred.
    /// </summary>
    Enabled = 1,

    /// <summary>
    /// Channel is preferred for this recipient.
    /// </summary>
    Preferred = 2,

    /// <summary>
    /// Channel is the primary/preferred method for this recipient.
    /// </summary>
    Primary = 3
}

/// <summary>
/// Builder class for creating NotificationRecipient instances with fluent API.
/// </summary>
public sealed class NotificationRecipientBuilder
{
    private readonly NotificationRecipient _recipient;

    internal NotificationRecipientBuilder(NotificationRecipient recipient)
    {
        _recipient = recipient;
    }

    /// <summary>
    /// Sets the recipient ID.
    /// </summary>
    public NotificationRecipientBuilder WithId(string id)
    {
        return new NotificationRecipientBuilder(_recipient with { Id = id });
    }

    /// <summary>
    /// Sets the recipient name.
    /// </summary>
    public NotificationRecipientBuilder WithName(string name)
    {
        return new NotificationRecipientBuilder(_recipient with { Name = name });
    }

    /// <summary>
    /// Sets the email address.
    /// </summary>
    public NotificationRecipientBuilder WithEmail(string email)
    {
        return new NotificationRecipientBuilder(_recipient with { Email = email });
    }

    /// <summary>
    /// Sets the phone number.
    /// </summary>
    public NotificationRecipientBuilder WithPhoneNumber(string phoneNumber)
    {
        return new NotificationRecipientBuilder(_recipient with { PhoneNumber = phoneNumber });
    }

    /// <summary>
    /// Sets the push token.
    /// </summary>
    public NotificationRecipientBuilder WithPushToken(string pushToken)
    {
        return new NotificationRecipientBuilder(_recipient with { PushToken = pushToken });
    }

    /// <summary>
    /// Sets the user ID.
    /// </summary>
    public NotificationRecipientBuilder WithUserId(string userId)
    {
        return new NotificationRecipientBuilder(_recipient with { UserId = userId });
    }

    /// <summary>
    /// Sets the language.
    /// </summary>
    public NotificationRecipientBuilder WithLanguage(string language)
    {
        return new NotificationRecipientBuilder(_recipient with { Language = language });
    }

    /// <summary>
    /// Sets the time zone.
    /// </summary>
    public NotificationRecipientBuilder WithTimeZone(string timeZone)
    {
        return new NotificationRecipientBuilder(_recipient with { TimeZone = timeZone });
    }

    /// <summary>
    /// Sets a channel preference.
    /// </summary>
    public NotificationRecipientBuilder WithChannelPreference(NotificationChannel channel, ChannelPreference preference)
    {
        var preferences = new Dictionary<NotificationChannel, ChannelPreference>(_recipient.ChannelPreferences)
        {
            [channel] = preference
        };
        return new NotificationRecipientBuilder(_recipient with { ChannelPreferences = preferences });
    }

    /// <summary>
    /// Adds a custom property.
    /// </summary>
    public NotificationRecipientBuilder WithProperty(string key, object value)
    {
        var properties = new Dictionary<string, object>(_recipient.Properties) { [key] = value };
        return new NotificationRecipientBuilder(_recipient with { Properties = properties });
    }

    /// <summary>
    /// Sets the active status.
    /// </summary>
    public NotificationRecipientBuilder WithActiveStatus(bool isActive)
    {
        return new NotificationRecipientBuilder(_recipient with { IsActive = isActive });
    }

    /// <summary>
    /// Sets the opt-out status.
    /// </summary>
    public NotificationRecipientBuilder WithOptOutStatus(bool isOptedOut)
    {
        return new NotificationRecipientBuilder(_recipient with { IsOptedOut = isOptedOut });
    }

    /// <summary>
    /// Adds an opted-out channel.
    /// </summary>
    public NotificationRecipientBuilder WithOptedOutChannel(NotificationChannel channel)
    {
        var channels = new HashSet<NotificationChannel>(_recipient.OptedOutChannels) { channel };
        return new NotificationRecipientBuilder(_recipient with { OptedOutChannels = channels });
    }

    /// <summary>
    /// Builds the final NotificationRecipient.
    /// </summary>
    public NotificationRecipient Build() => _recipient;
}