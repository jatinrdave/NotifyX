using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Main service interface for the notification system.
/// Provides high-level operations for sending notifications and managing the notification pipeline.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification through the notification pipeline.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task<NotificationResult> SendAsync(NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends multiple notifications in batch.
    /// </summary>
    /// <param name="notifications">The notifications to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous batch send operation.</returns>
    Task<BatchNotificationResult> SendBatchAsync(IEnumerable<NotificationEvent> notifications, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a notification for future delivery.
    /// </summary>
    /// <param name="notification">The notification to schedule.</param>
    /// <param name="scheduledFor">When to deliver the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous schedule operation.</returns>
    Task<NotificationResult> ScheduleAsync(NotificationEvent notification, DateTime scheduledFor, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a scheduled notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous cancel operation.</returns>
    Task<bool> CancelAsync(string notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the status of a notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous status check operation.</returns>
    Task<NotificationStatus?> GetStatusAsync(string notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the delivery history for a notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous history retrieval operation.</returns>
    Task<IEnumerable<DeliveryAttempt>> GetDeliveryHistoryAsync(string notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to retry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous retry operation.</returns>
    Task<NotificationResult> RetryAsync(string notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges receipt of a notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification.</param>
    /// <param name="acknowledgedBy">Who acknowledged the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous acknowledgment operation.</returns>
    Task<bool> AcknowledgeAsync(string notificationId, string acknowledgedBy, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a notification operation.
/// </summary>
public sealed class NotificationResult
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The notification ID.
    /// </summary>
    public string NotificationId { get; init; } = string.Empty;

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if the operation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Delivery results for each recipient.
    /// </summary>
    public List<RecipientDeliveryResult> DeliveryResults { get; init; } = new();

    /// <summary>
    /// Additional metadata about the operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when the operation was completed.
    /// </summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful notification result.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="deliveryResults">Delivery results.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A successful notification result.</returns>
    public static NotificationResult Success(
        string notificationId,
        List<RecipientDeliveryResult>? deliveryResults = null,
        Dictionary<string, object>? metadata = null)
    {
        return new NotificationResult
        {
            IsSuccess = true,
            NotificationId = notificationId,
            DeliveryResults = deliveryResults ?? new List<RecipientDeliveryResult>(),
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a failed notification result.
    /// </summary>
    /// <param name="notificationId">The notification ID.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="metadata">Additional metadata.</param>
    /// <returns>A failed notification result.</returns>
    public static NotificationResult Failure(
        string notificationId,
        string errorMessage,
        string? errorCode = null,
        Dictionary<string, object>? metadata = null)
    {
        return new NotificationResult
        {
            IsSuccess = false,
            NotificationId = notificationId,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}

/// <summary>
/// Result of a batch notification operation.
/// </summary>
public sealed class BatchNotificationResult
{
    /// <summary>
    /// Total number of notifications in the batch.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Number of successful notifications.
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Number of failed notifications.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// Individual notification results.
    /// </summary>
    public List<NotificationResult> Results { get; init; } = new();

    /// <summary>
    /// Overall batch status.
    /// </summary>
    public BatchStatus Status { get; init; }

    /// <summary>
    /// Timestamp when the batch operation was completed.
    /// </summary>
    public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Status of a batch operation.
/// </summary>
public enum BatchStatus
{
    /// <summary>
    /// All notifications were successful.
    /// </summary>
    AllSuccessful = 0,

    /// <summary>
    /// Some notifications failed.
    /// </summary>
    PartialFailure = 1,

    /// <summary>
    /// All notifications failed.
    /// </summary>
    AllFailed = 2
}

/// <summary>
/// Delivery result for a specific recipient.
/// </summary>
public sealed class RecipientDeliveryResult
{
    /// <summary>
    /// The recipient ID.
    /// </summary>
    public string RecipientId { get; init; } = string.Empty;

    /// <summary>
    /// The channel used for delivery.
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Whether the delivery was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The delivery ID.
    /// </summary>
    public string DeliveryId { get; init; } = string.Empty;

    /// <summary>
    /// Error message if delivery failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Timestamp when the delivery was attempted.
    /// </summary>
    public DateTime AttemptedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the delivery was completed.
    /// </summary>
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// Status of a notification.
/// </summary>
public sealed class NotificationStatus
{
    /// <summary>
    /// The notification ID.
    /// </summary>
    public string NotificationId { get; init; } = string.Empty;

    /// <summary>
    /// Current status of the notification.
    /// </summary>
    public NotificationState State { get; init; }

    /// <summary>
    /// Progress percentage (0-100).
    /// </summary>
    public int Progress { get; init; }

    /// <summary>
    /// Number of delivery attempts made.
    /// </summary>
    public int AttemptCount { get; init; }

    /// <summary>
    /// Number of successful deliveries.
    /// </summary>
    public int SuccessCount { get; init; }

    /// <summary>
    /// Number of failed deliveries.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// Timestamp when the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when the notification was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; init; }

    /// <summary>
    /// Timestamp when the notification was completed.
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Current error message if any.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// States of a notification.
/// </summary>
public enum NotificationState
{
    /// <summary>
    /// Notification is pending processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Notification is being processed.
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Notification is scheduled for future delivery.
    /// </summary>
    Scheduled = 2,

    /// <summary>
    /// Notification is being delivered.
    /// </summary>
    Delivering = 3,

    /// <summary>
    /// Notification was delivered successfully.
    /// </summary>
    Delivered = 4,

    /// <summary>
    /// Notification delivery failed.
    /// </summary>
    Failed = 5,

    /// <summary>
    /// Notification was cancelled.
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Notification expired.
    /// </summary>
    Expired = 7,

    /// <summary>
    /// Notification was acknowledged.
    /// </summary>
    Acknowledged = 8
}

/// <summary>
/// Represents a delivery attempt for a notification.
/// </summary>
public sealed class DeliveryAttempt
{
    /// <summary>
    /// The attempt ID.
    /// </summary>
    public string AttemptId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The notification ID.
    /// </summary>
    public string NotificationId { get; init; } = string.Empty;

    /// <summary>
    /// The recipient ID.
    /// </summary>
    public string RecipientId { get; init; } = string.Empty;

    /// <summary>
    /// The channel used for this attempt.
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// Whether the attempt was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The delivery ID from the provider.
    /// </summary>
    public string DeliveryId { get; init; } = string.Empty;

    /// <summary>
    /// Error message if the attempt failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if the attempt failed.
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
    /// Timestamp when the attempt was made.
    /// </summary>
    public DateTime AttemptedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the attempt was completed.
    /// </summary>
    public DateTime? CompletedAt { get; init; }

    /// <summary>
    /// Duration of the attempt.
    /// </summary>
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - AttemptedAt : null;

    /// <summary>
    /// Additional metadata about the attempt.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}