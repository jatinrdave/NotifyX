using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Collections.Concurrent;

namespace NotifyX.Core.Services;

/// <summary>
/// In-memory implementation of the dead letter queue service.
/// </summary>
public class DeadLetterQueueService : IDeadLetterQueueService
{
    private readonly ILogger<DeadLetterQueueService> _logger;
    private readonly ConcurrentDictionary<string, FailedNotification> _failedNotifications = new();
    private DeadLetterQueueStatistics _statistics = new()
    {
        TotalFailedMessages = 0,
        RetriedMessages = 0,
        PermanentlyFailedMessages = 0,
        LastFailedAt = DateTime.UtcNow,
        FailureReasons = new Dictionary<string, long>()
    };

    public DeadLetterQueueService(ILogger<DeadLetterQueueService> logger)
    {
        _logger = logger;
    }

    public Task<bool> AddFailedNotificationAsync(FailedNotification failedNotification, CancellationToken cancellationToken = default)
    {
        try
        {
            _failedNotifications.TryAdd(failedNotification.Id, failedNotification);
            
            _statistics = _statistics with { TotalFailedMessages = _statistics.TotalFailedMessages + 1 };
            
            // Update failure reasons statistics
            lock (_statistics.FailureReasons)
            {
                if (_statistics.FailureReasons.ContainsKey(failedNotification.FailureReason))
                {
                    _statistics.FailureReasons[failedNotification.FailureReason]++;
                }
                else
                {
                    _statistics.FailureReasons[failedNotification.FailureReason] = 1;
                }
            }

            _statistics.LastFailedAt = DateTime.UtcNow;

            _logger.LogWarning("Added failed notification {NotificationId} to DLQ. Reason: {Reason}", 
                failedNotification.OriginalNotificationId, failedNotification.FailureReason);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add notification {NotificationId} to DLQ", 
                failedNotification.OriginalNotificationId);
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<FailedNotification>> GetFailedNotificationsAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            var failedNotifications = _failedNotifications.Values
                .OrderByDescending(fn => fn.FailedAt)
                .Take(limit)
                .ToList();

            return Task.FromResult<IEnumerable<FailedNotification>>(failedNotifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve failed notifications");
            return Task.FromResult<IEnumerable<FailedNotification>>(new List<FailedNotification>());
        }
    }

    public Task<bool> RetryFailedNotificationAsync(string failedNotificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_failedNotifications.TryGetValue(failedNotificationId, out var failedNotification))
            {
                _logger.LogWarning("Failed notification {FailedNotificationId} not found in DLQ", failedNotificationId);
                return Task.FromResult(false);
            }

            // Check if max retries exceeded
            if (failedNotification.RetryCount >= failedNotification.MaxRetries)
            {
                _logger.LogWarning("Max retries exceeded for failed notification {FailedNotificationId}", failedNotificationId);
                _statistics = _statistics with { PermanentlyFailedMessages = _statistics.PermanentlyFailedMessages + 1 };
                return Task.FromResult(false);
            }

            // Update retry count and next retry time
            var updatedFailedNotification = failedNotification with
            {
                RetryCount = failedNotification.RetryCount + 1,
                NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, failedNotification.RetryCount)) // Exponential backoff
            };

            _failedNotifications.TryUpdate(failedNotificationId, updatedFailedNotification, failedNotification);
            _statistics = _statistics with { RetriedMessages = _statistics.RetriedMessages + 1 };

            _logger.LogInformation("Retried failed notification {FailedNotificationId} (attempt {RetryCount}/{MaxRetries})", 
                failedNotificationId, updatedFailedNotification.RetryCount, failedNotification.MaxRetries);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retry notification {FailedNotificationId}", failedNotificationId);
            return Task.FromResult(false);
        }
    }

    public Task<bool> RemoveFailedNotificationAsync(string failedNotificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_failedNotifications.TryRemove(failedNotificationId, out var removedNotification))
            {
                _logger.LogInformation("Removed failed notification {FailedNotificationId} from DLQ", failedNotificationId);
                return Task.FromResult(true);
            }

            _logger.LogWarning("Failed notification {FailedNotificationId} not found in DLQ", failedNotificationId);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove notification {FailedNotificationId} from DLQ", failedNotificationId);
            return Task.FromResult(false);
        }
    }

    public Task<DeadLetterQueueStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var stats = _statistics with
        {
            LastFailedAt = _statistics.LastFailedAt,
            FailureReasons = new Dictionary<string, long>(_statistics.FailureReasons)
        };

        return Task.FromResult(stats);
    }

    public Task<bool> PurgeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = _failedNotifications.Count;
            _failedNotifications.Clear();
            
            // Reset statistics
            _statistics = new DeadLetterQueueStatistics
            {
                TotalFailedMessages = 0,
                RetriedMessages = 0,
                PermanentlyFailedMessages = 0,
                LastFailedAt = DateTime.UtcNow,
                FailureReasons = new Dictionary<string, long>(),
                Metadata = new Dictionary<string, object>()
            };

            _logger.LogInformation("Purged {Count} failed notifications from DLQ", count);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge DLQ");
            return Task.FromResult(false);
        }
    }
}