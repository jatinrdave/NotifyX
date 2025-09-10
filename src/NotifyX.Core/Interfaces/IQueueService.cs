using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Interface for queue service operations.
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// Enqueues a notification event for processing.
    /// </summary>
    Task<bool> EnqueueAsync(NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues multiple notification events for batch processing.
    /// </summary>
    Task<bool> EnqueueBatchAsync(IEnumerable<NotificationEvent> notifications, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues a notification event for processing.
    /// </summary>
    Task<NotificationEvent?> DequeueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues multiple notification events for batch processing.
    /// </summary>
    Task<IEnumerable<NotificationEvent>> DequeueBatchAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current queue length.
    /// </summary>
    Task<long> GetQueueLengthAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets queue statistics.
    /// </summary>
    Task<QueueStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges all messages from the queue.
    /// </summary>
    Task<bool> PurgeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status of the queue service.
    /// </summary>
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for priority queue service operations.
/// </summary>
public interface IPriorityQueueService : IQueueService
{
    /// <summary>
    /// Enqueues a notification event with priority.
    /// </summary>
    Task<bool> EnqueueWithPriorityAsync(NotificationEvent notification, NotificationPriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues the highest priority notification event.
    /// </summary>
    Task<NotificationEvent?> DequeueHighestPriorityAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the queue length for a specific priority.
    /// </summary>
    Task<long> GetQueueLengthAsync(NotificationPriority priority, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for dead letter queue service operations.
/// </summary>
public interface IDeadLetterQueueService
{
    /// <summary>
    /// Adds a failed notification to the dead letter queue.
    /// </summary>
    Task<bool> AddFailedNotificationAsync(FailedNotification failedNotification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves failed notifications from the dead letter queue.
    /// </summary>
    Task<IEnumerable<FailedNotification>> GetFailedNotificationsAsync(int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries a failed notification.
    /// </summary>
    Task<bool> RetryFailedNotificationAsync(string failedNotificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a failed notification from the dead letter queue.
    /// </summary>
    Task<bool> RemoveFailedNotificationAsync(string failedNotificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dead letter queue statistics.
    /// </summary>
    Task<DeadLetterQueueStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges all failed notifications from the dead letter queue.
    /// </summary>
    Task<bool> PurgeAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for worker service operations.
/// </summary>
public interface IWorkerService
{
    /// <summary>
    /// Starts the worker service.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the worker service.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the worker service status.
    /// </summary>
    Task<WorkerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets worker service statistics.
    /// </summary>
    Task<WorkerStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Scales the worker service to the specified number of workers.
    /// </summary>
    Task<bool> ScaleAsync(int workerCount, CancellationToken cancellationToken = default);
}