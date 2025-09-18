using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Concurrent;

namespace NotifyX.Core.Services;

/// <summary>
/// In-memory implementation of the queue service for development and testing.
/// </summary>
public class InMemoryQueueService : IPriorityQueueService
{
    private readonly ILogger<InMemoryQueueService> _logger;
    private readonly ConcurrentQueue<QueueMessage> _criticalQueue = new();
    private readonly ConcurrentQueue<QueueMessage> _highQueue = new();
    private readonly ConcurrentQueue<QueueMessage> _normalQueue = new();
    private readonly ConcurrentQueue<QueueMessage> _lowQueue = new();
    private readonly ConcurrentDictionary<string, QueueMessage> _processingMessages = new();
    private readonly QueueStatistics _statistics = new()
    {
        TotalMessages = 0,
        ProcessedMessages = 0,
        FailedMessages = 0,
        PendingMessages = 0,
        AverageProcessingTime = 0,
        LastProcessedAt = DateTime.UtcNow
    };

    public InMemoryQueueService(ILogger<InMemoryQueueService> logger)
    {
        _logger = logger;
    }

    public Task<bool> EnqueueAsync(NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        return EnqueueWithPriorityAsync(notification, notification.Priority, cancellationToken);
    }

    public Task<bool> EnqueueWithPriorityAsync(NotificationEvent notification, NotificationPriority priority, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new QueueMessage
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = notification.TenantId,
                Notification = notification,
                Priority = priority,
                EnqueuedAt = DateTime.UtcNow,
                ScheduledFor = notification.ScheduledFor
            };

            var queue = GetQueueByPriority(priority);
            queue.Enqueue(message);

            Interlocked.Increment(ref _statistics.TotalMessages);
            Interlocked.Increment(ref _statistics.PendingMessages);

            _logger.LogDebug("Enqueued notification {NotificationId} with priority {Priority}", notification.Id, priority);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue notification {NotificationId}", notification.Id);
            return Task.FromResult(false);
        }
    }

    public async Task<bool> EnqueueBatchAsync(IEnumerable<NotificationEvent> notifications, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = notifications.Select(n => EnqueueAsync(n, cancellationToken));
            var results = await Task.WhenAll(tasks);
            return results.All(r => r);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue batch of notifications");
            return false;
        }
    }

    public async Task<NotificationEvent?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        var message = await DequeueHighestPriorityAsync(cancellationToken);
        return message?.Notification;
    }

    public Task<NotificationEvent?> DequeueHighestPriorityAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to dequeue from highest priority queue first
            var queues = new[]
            {
                (NotificationPriority.Critical, _criticalQueue),
                (NotificationPriority.High, _highQueue),
                (NotificationPriority.Normal, _normalQueue),
                (NotificationPriority.Low, _lowQueue)
            };

            foreach (var (priority, queue) in queues)
            {
                if (queue.TryDequeue(out var message))
                {
                    _processingMessages.TryAdd(message.Id, message);
                    Interlocked.Decrement(ref _statistics.PendingMessages);
                    
                    _logger.LogDebug("Dequeued notification {NotificationId} with priority {Priority}", 
                        message.Notification.Id, priority);
                    
                    return Task.FromResult(message.Notification);
                }
            }

            return Task.FromResult<NotificationEvent?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dequeue notification");
            return Task.FromResult<NotificationEvent?>(null);
        }
    }

    public async Task<IEnumerable<NotificationEvent>> DequeueBatchAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var notifications = new List<NotificationEvent>();
        
        for (int i = 0; i < batchSize; i++)
        {
            var notification = await DequeueAsync(cancellationToken);
            if (notification == null) break;
            
            notifications.Add(notification);
        }

        return notifications;
    }

    public Task<long> GetQueueLengthAsync(CancellationToken cancellationToken = default)
    {
        var totalLength = _criticalQueue.Count + _highQueue.Count + _normalQueue.Count + _lowQueue.Count;
        return Task.FromResult((long)totalLength);
    }

    public Task<long> GetQueueLengthAsync(NotificationPriority priority, CancellationToken cancellationToken = default)
    {
        var queue = GetQueueByPriority(priority);
        return Task.FromResult((long)queue.Count);
    }

    public Task<QueueStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var stats = _statistics with
        {
            PendingMessages = _criticalQueue.Count + _highQueue.Count + _normalQueue.Count + _lowQueue.Count,
            LastProcessedAt = DateTime.UtcNow
        };

        return Task.FromResult(stats);
    }

    public Task<bool> PurgeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            while (_criticalQueue.TryDequeue(out _)) { }
            while (_highQueue.TryDequeue(out _)) { }
            while (_normalQueue.TryDequeue(out _)) { }
            while (_lowQueue.TryDequeue(out _)) { }

            _processingMessages.Clear();
            
            _logger.LogInformation("Purged all messages from queues");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge queues");
            return Task.FromResult(false);
        }
    }

    public Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var totalMessages = _criticalQueue.Count + _highQueue.Count + _normalQueue.Count + _lowQueue.Count;
            var isHealthy = totalMessages < 10000; // Threshold for health check

            return Task.FromResult(isHealthy ? HealthStatus.Healthy : HealthStatus.Degraded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue health status");
            return Task.FromResult(HealthStatus.Unhealthy);
        }
    }

    private ConcurrentQueue<QueueMessage> GetQueueByPriority(NotificationPriority priority)
    {
        return priority switch
        {
            NotificationPriority.Critical => _criticalQueue,
            NotificationPriority.High => _highQueue,
            NotificationPriority.Normal => _normalQueue,
            NotificationPriority.Low => _lowQueue,
            _ => _normalQueue
        };
    }
}