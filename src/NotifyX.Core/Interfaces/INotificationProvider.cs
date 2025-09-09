using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Interface for notification delivery providers.
/// Each provider handles delivery through a specific channel (email, SMS, push, etc.).
/// </summary>
public interface INotificationProvider
{
    /// <summary>
    /// The notification channel this provider handles.
    /// </summary>
    NotificationChannel Channel { get; }

    /// <summary>
    /// Whether this provider is currently available and healthy.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Sends a notification through this provider.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="recipient">The recipient of the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task<DeliveryResult> SendAsync(
        NotificationEvent notification,
        NotificationRecipient recipient,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the notification can be sent through this provider.
    /// </summary>
    /// <param name="notification">The notification to validate.</param>
    /// <param name="recipient">The recipient to validate.</param>
    /// <returns>Validation result indicating if the notification can be sent.</returns>
    ValidationResult Validate(NotificationEvent notification, NotificationRecipient recipient);

    /// <summary>
    /// Gets the health status of this provider.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous health check operation.</returns>
    Task<ProviderHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures the provider with the given configuration.
    /// </summary>
    /// <param name="configuration">The channel configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous configuration operation.</returns>
    Task ConfigureAsync(ChannelConfiguration configuration, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a notification delivery attempt.
/// </summary>
public sealed class DeliveryResult
{
    /// <summary>
    /// Whether the delivery was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The delivery attempt ID for tracking.
    /// </summary>
    public string DeliveryId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Error message if delivery failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if delivery failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// HTTP status code if applicable.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Whether this error is retryable.
    /// </summary>
    public bool IsRetryable { get; init; } = true;

    /// <summary>
    /// Suggested retry delay if the error is retryable.
    /// </summary>
    public TimeSpan? SuggestedRetryDelay { get; init; }

    /// <summary>
    /// Additional metadata about the delivery.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when the delivery was attempted.
    /// </summary>
    public DateTime AttemptedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the delivery was completed.
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Creates a successful delivery result.
    /// </summary>
    /// <param name="deliveryId">The delivery ID.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A successful delivery result.</returns>
    public static DeliveryResult Success(string deliveryId, Dictionary<string, object>? metadata = null)
    {
        return new DeliveryResult
        {
            IsSuccess = true,
            DeliveryId = deliveryId,
            CompletedAt = DateTime.UtcNow,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a failed delivery result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="isRetryable">Whether the error is retryable.</param>
    /// <param name="suggestedRetryDelay">Suggested retry delay.</param>
    /// <param name="statusCode">HTTP status code if applicable.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A failed delivery result.</returns>
    public static DeliveryResult Failure(
        string errorMessage,
        string? errorCode = null,
        bool isRetryable = true,
        TimeSpan? suggestedRetryDelay = null,
        int? statusCode = null,
        Dictionary<string, object>? metadata = null)
    {
        return new DeliveryResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            IsRetryable = isRetryable,
            SuggestedRetryDelay = suggestedRetryDelay,
            StatusCode = statusCode,
            CompletedAt = DateTime.UtcNow,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}

/// <summary>
/// Result of notification validation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Validation error messages.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Validation warnings.
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="warnings">Optional warnings.</param>
    /// <returns>A successful validation result.</returns>
    public static ValidationResult Success(List<string>? warnings = null)
    {
        return new ValidationResult
        {
            IsValid = true,
            Warnings = warnings ?? new List<string>()
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errors">Validation errors.</param>
    /// <param name="warnings">Optional warnings.</param>
    /// <returns>A failed validation result.</returns>
    public static ValidationResult Failure(List<string> errors, List<string>? warnings = null)
    {
        return new ValidationResult
        {
            IsValid = false,
            Errors = errors,
            Warnings = warnings ?? new List<string>()
        };
    }
}

/// <summary>
/// Health status of a notification provider.
/// </summary>
public sealed class ProviderHealthStatus
{
    /// <summary>
    /// Whether the provider is healthy.
    /// </summary>
    public bool IsHealthy { get; init; }

    /// <summary>
    /// Health status message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Additional health metrics.
    /// </summary>
    public Dictionary<string, object> Metrics { get; init; } = new();

    /// <summary>
    /// Timestamp when the health check was performed.
    /// </summary>
    public DateTime CheckedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a healthy status.
    /// </summary>
    /// <param name="message">Health message.</param>
    /// <param name="metrics">Additional metrics.</param>
    /// <returns>A healthy status.</returns>
    public static ProviderHealthStatus Healthy(string message = "Provider is healthy", Dictionary<string, object>? metrics = null)
    {
        return new ProviderHealthStatus
        {
            IsHealthy = true,
            Message = message,
            Metrics = metrics ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates an unhealthy status.
    /// </summary>
    /// <param name="message">Health message.</param>
    /// <param name="metrics">Additional metrics.</param>
    /// <returns>An unhealthy status.</returns>
    public static ProviderHealthStatus Unhealthy(string message, Dictionary<string, object>? metrics = null)
    {
        return new ProviderHealthStatus
        {
            IsHealthy = false,
            Message = message,
            Metrics = metrics ?? new Dictionary<string, object>()
        };
    }
}