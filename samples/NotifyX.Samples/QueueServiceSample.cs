using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Samples;

/// <summary>
/// Sample demonstrating queue service functionality.
/// </summary>
public class QueueServiceSample
{
    private readonly ILogger<QueueServiceSample> _logger;
    private readonly IQueueService _queueService;
    private readonly IPriorityQueueService _priorityQueueService;
    private readonly IDeadLetterQueueService _deadLetterQueueService;
    private readonly IWorkerService _workerService;

    public QueueServiceSample(
        ILogger<QueueServiceSample> logger,
        IQueueService queueService,
        IPriorityQueueService priorityQueueService,
        IDeadLetterQueueService deadLetterQueueService,
        IWorkerService workerService)
    {
        _logger = logger;
        _queueService = queueService;
        _priorityQueueService = priorityQueueService;
        _deadLetterQueueService = deadLetterQueueService;
        _workerService = workerService;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Queue Service Sample");

        try
        {
            // Start the worker service
            await _workerService.StartAsync();
            _logger.LogInformation("Worker service started");

            // Demonstrate basic queue operations
            await DemonstrateBasicQueueOperationsAsync();

            // Demonstrate priority queue operations
            await DemonstratePriorityQueueOperationsAsync();

            // Demonstrate batch operations
            await DemonstrateBatchOperationsAsync();

            // Demonstrate dead letter queue operations
            await DemonstrateDeadLetterQueueOperationsAsync();

            // Demonstrate worker service statistics
            await DemonstrateWorkerStatisticsAsync();

            // Stop the worker service
            await _workerService.StopAsync();
            _logger.LogInformation("Worker service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Queue Service Sample");
        }
    }

    private async Task DemonstrateBasicQueueOperationsAsync()
    {
        _logger.LogInformation("=== Basic Queue Operations ===");

        // Create test notifications
        var notifications = new[]
        {
            CreateTestNotification("basic-1", "user.login", NotificationPriority.Normal),
            CreateTestNotification("basic-2", "user.logout", NotificationPriority.Normal),
            CreateTestNotification("basic-3", "system.alert", NotificationPriority.Normal)
        };

        // Enqueue notifications
        foreach (var notification in notifications)
        {
            var success = await _queueService.EnqueueAsync(notification);
            _logger.LogInformation("Enqueued notification {NotificationId}: {Success}", 
                notification.Id, success);
        }

        // Get queue statistics
        var stats = await _queueService.GetStatisticsAsync();
        _logger.LogInformation("Queue Statistics - Total: {Total}, Pending: {Pending}, Processed: {Processed}", 
            stats.TotalMessages, stats.PendingMessages, stats.ProcessedMessages);

        // Get queue length
        var length = await _queueService.GetQueueLengthAsync();
        _logger.LogInformation("Current queue length: {Length}", length);

        // Wait for processing
        await Task.Delay(2000);

        // Get updated statistics
        var updatedStats = await _queueService.GetStatisticsAsync();
        _logger.LogInformation("Updated Queue Statistics - Total: {Total}, Pending: {Pending}, Processed: {Processed}", 
            updatedStats.TotalMessages, updatedStats.PendingMessages, updatedStats.ProcessedMessages);
    }

    private async Task DemonstratePriorityQueueOperationsAsync()
    {
        _logger.LogInformation("=== Priority Queue Operations ===");

        // Create notifications with different priorities
        var notifications = new[]
        {
            (CreateTestNotification("priority-low", "low.priority.event", NotificationPriority.Low), NotificationPriority.Low),
            (CreateTestNotification("priority-normal", "normal.priority.event", NotificationPriority.Normal), NotificationPriority.Normal),
            (CreateTestNotification("priority-high", "high.priority.event", NotificationPriority.High), NotificationPriority.High),
            (CreateTestNotification("priority-critical", "critical.priority.event", NotificationPriority.Critical), NotificationPriority.Critical)
        };

        // Enqueue with specific priorities
        foreach (var (notification, priority) in notifications)
        {
            var success = await _priorityQueueService.EnqueueWithPriorityAsync(notification, priority);
            _logger.LogInformation("Enqueued notification {NotificationId} with priority {Priority}: {Success}", 
                notification.Id, priority, success);
        }

        // Get queue lengths by priority
        foreach (NotificationPriority priority in Enum.GetValues<NotificationPriority>())
        {
            var length = await _priorityQueueService.GetQueueLengthAsync(priority);
            _logger.LogInformation("Queue length for priority {Priority}: {Length}", priority, length);
        }

        // Wait for processing
        await Task.Delay(3000);
    }

    private async Task DemonstrateBatchOperationsAsync()
    {
        _logger.LogInformation("=== Batch Operations ===");

        // Create a batch of notifications
        var notifications = Enumerable.Range(1, 10)
            .Select(i => CreateTestNotification($"batch-{i}", "batch.event", NotificationPriority.Normal))
            .ToArray();

        // Enqueue batch
        var success = await _queueService.EnqueueBatchAsync(notifications);
        _logger.LogInformation("Enqueued batch of {Count} notifications: {Success}", notifications.Length, success);

        // Dequeue batch
        var dequeuedNotifications = await _queueService.DequeueBatchAsync(5);
        _logger.LogInformation("Dequeued {Count} notifications from batch", dequeuedNotifications.Count());

        // Wait for processing
        await Task.Delay(2000);
    }

    private async Task DemonstrateDeadLetterQueueOperationsAsync()
    {
        _logger.LogInformation("=== Dead Letter Queue Operations ===");

        // Create a failed notification
        var failedNotification = new FailedNotification
        {
            OriginalNotificationId = "failed-notification-1",
            TenantId = "test-tenant",
            OriginalNotification = CreateTestNotification("failed-1", "failed.event", NotificationPriority.Normal),
            FailureReason = "ProcessingError",
            ErrorMessage = "Simulated processing error",
            StackTrace = "Stack trace would go here",
            FailedAt = DateTime.UtcNow,
            RetryCount = 0,
            MaxRetries = 3
        };

        // Add to dead letter queue
        var success = await _deadLetterQueueService.AddFailedNotificationAsync(failedNotification);
        _logger.LogInformation("Added failed notification to DLQ: {Success}", success);

        // Get failed notifications
        var failedNotifications = await _deadLetterQueueService.GetFailedNotificationsAsync(10);
        _logger.LogInformation("Retrieved {Count} failed notifications from DLQ", failedNotifications.Count());

        // Get DLQ statistics
        var dlqStats = await _deadLetterQueueService.GetStatisticsAsync();
        _logger.LogInformation("DLQ Statistics - Total Failed: {TotalFailed}, Retried: {Retried}, Permanently Failed: {PermanentlyFailed}", 
            dlqStats.TotalFailedMessages, dlqStats.RetryMessages, dlqStats.PermanentlyFailedMessages);

        // Retry failed notification
        var retrySuccess = await _deadLetterQueueService.RetryFailedNotificationAsync(failedNotification.Id);
        _logger.LogInformation("Retried failed notification: {Success}", retrySuccess);

        // Remove failed notification
        var removeSuccess = await _deadLetterQueueService.RemoveFailedNotificationAsync(failedNotification.Id);
        _logger.LogInformation("Removed failed notification from DLQ: {Success}", removeSuccess);
    }

    private async Task DemonstrateWorkerStatisticsAsync()
    {
        _logger.LogInformation("=== Worker Service Statistics ===");

        // Get worker status
        var status = await _workerService.GetStatusAsync();
        _logger.LogInformation("Worker Status - Running: {IsRunning}, Active Workers: {ActiveWorkers}, Total Workers: {TotalWorkers}", 
            status.IsRunning, status.ActiveWorkers, status.TotalWorkers);

        // Get worker statistics
        var stats = await _workerService.GetStatisticsAsync();
        _logger.LogInformation("Worker Statistics - Total Processed: {TotalProcessed}, Total Failed: {TotalFailed}, Average Processing Time: {AvgProcessingTime}ms", 
            stats.TotalProcessed, stats.TotalFailed, stats.AverageProcessingTime);

        // Scale workers
        var scaleSuccess = await _workerService.ScaleAsync(4);
        _logger.LogInformation("Scaled workers to 4: {Success}", scaleSuccess);

        // Get updated status
        var updatedStatus = await _workerService.GetStatusAsync();
        _logger.LogInformation("Updated Worker Status - Total Workers: {TotalWorkers}", updatedStatus.TotalWorkers);
    }

    private static NotificationEvent CreateTestNotification(string id, string eventType, NotificationPriority priority)
    {
        return new NotificationEvent
        {
            Id = id,
            TenantId = "test-tenant",
            EventType = eventType,
            Priority = priority,
            Subject = $"Test Notification - {eventType}",
            Content = $"This is a test notification for {eventType}",
            Recipients = new List<NotificationRecipient>
            {
                new NotificationRecipient
                {
                    Id = "test-recipient",
                    Name = "Test User",
                    Email = "test@example.com"
                }
            },
            PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email }
        };
    }
}