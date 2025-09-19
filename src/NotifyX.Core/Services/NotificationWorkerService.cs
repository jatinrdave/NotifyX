using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Collections.Concurrent;

namespace NotifyX.Core.Services;

/// <summary>
/// Worker service for processing notification events from the queue.
/// </summary>
public class NotificationWorkerService : IWorkerService, IDisposable
{
    private readonly ILogger<NotificationWorkerService> _logger;
    private readonly INotificationService _notificationService;
    private readonly IQueueService _queueService;
    private readonly IDeadLetterQueueService _deadLetterQueueService;
    private readonly WorkerOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ConcurrentDictionary<int, Task> _workers = new();
    private WorkerStatistics _statistics = new()
    {
        TotalProcessed = 0,
        TotalFailed = 0,
        AverageProcessingTime = 0,
        ThroughputPerSecond = 0,
        LastProcessedAt = DateTime.UtcNow,
        ProcessingTimes = new Dictionary<string, long>()
    };

    private volatile bool _isRunning = false;
    private volatile int _activeWorkers = 0;
    private DateTime _startedAt = DateTime.UtcNow;

    public NotificationWorkerService(
        ILogger<NotificationWorkerService> logger,
        INotificationService notificationService,
        IQueueService queueService,
        IDeadLetterQueueService deadLetterQueueService,
        WorkerOptions options)
    {
        _logger = logger;
        _notificationService = notificationService;
        _queueService = queueService;
        _deadLetterQueueService = deadLetterQueueService;
        _options = options;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Worker service is already running");
            return;
        }

        try
        {
            _isRunning = true;
            _startedAt = DateTime.UtcNow;

            _logger.LogInformation("Starting notification worker service with {WorkerCount} workers", _options.WorkerCount);

            // Start worker tasks
            for (int i = 0; i < _options.WorkerCount; i++)
            {
                var workerId = i;
                var workerTask = Task.Run(() => ProcessNotificationsAsync(workerId, _cancellationTokenSource.Token), cancellationToken);
                _workers.TryAdd(workerId, workerTask);
            }

            _logger.LogInformation("Notification worker service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start notification worker service");
            _isRunning = false;
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Worker service is not running");
            return;
        }

        try
        {
            _logger.LogInformation("Stopping notification worker service");

            _isRunning = false;
            _cancellationTokenSource.Cancel();

            // Wait for all workers to complete
            var tasks = _workers.Values.ToArray();
            await Task.WhenAll(tasks);

            _workers.Clear();
            _activeWorkers = 0;

            _logger.LogInformation("Notification worker service stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop notification worker service");
            throw;
        }
    }

    public Task<WorkerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new WorkerStatus
        {
            IsRunning = _isRunning,
            ActiveWorkers = _activeWorkers,
            IdleWorkers = _options.WorkerCount - _activeWorkers,
            TotalWorkers = _options.WorkerCount,
            StartedAt = _startedAt,
            LastProcessedAt = _statistics.LastProcessedAt,
            CurrentOperation = _isRunning ? "Processing notifications" : "Stopped"
        };

        return Task.FromResult(status);
    }

    public Task<WorkerStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var stats = _statistics with
        {
            LastProcessedAt = _statistics.LastProcessedAt,
            ProcessingTimes = new Dictionary<string, long>(_statistics.ProcessingTimes)
        };

        return Task.FromResult(stats);
    }

    public Task<bool> ScaleAsync(int workerCount, CancellationToken cancellationToken = default)
    {
        if (workerCount < _options.MinWorkers || workerCount > _options.MaxWorkers)
        {
            _logger.LogWarning("Worker count {WorkerCount} is outside allowed range [{MinWorkers}, {MaxWorkers}]", 
                workerCount, _options.MinWorkers, _options.MaxWorkers);
            return Task.FromResult(false);
        }

        if (workerCount == _options.WorkerCount)
        {
            _logger.LogInformation("Worker count is already {WorkerCount}", workerCount);
            return Task.FromResult(true);
        }

        try
        {
            if (workerCount > _options.WorkerCount)
            {
                // Scale up
                for (int i = _options.WorkerCount; i < workerCount; i++)
                {
                    var workerId = i;
                    var workerTask = Task.Run(() => ProcessNotificationsAsync(workerId, _cancellationTokenSource.Token), cancellationToken);
                    _workers.TryAdd(workerId, workerTask);
                }
            }
            else
            {
                // Scale down
                var workersToRemove = _options.WorkerCount - workerCount;
                var workersToStop = _workers.Keys.Take(workersToRemove).ToList();
                
                foreach (var workerId in workersToStop)
                {
                    if (_workers.TryRemove(workerId, out var workerTask))
                    {
                        // The worker will stop when it checks _isRunning
                    }
                }
            }

            _options.WorkerCount = workerCount;
            _logger.LogInformation("Scaled worker service to {WorkerCount} workers", workerCount);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scale worker service to {WorkerCount} workers", workerCount);
            return Task.FromResult(false);
        }
    }

    private async Task ProcessNotificationsAsync(int workerId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Worker {WorkerId} started", workerId);

        while (_isRunning && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                Interlocked.Increment(ref _activeWorkers);

                var notification = await _queueService.DequeueAsync(cancellationToken);
                if (notification == null)
                {
                    // No messages available, wait before polling again
                    await Task.Delay(_options.PollingInterval, cancellationToken);
                    continue;
                }

                await ProcessNotificationAsync(notification, workerId, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker {WorkerId} encountered an error", workerId);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken); // Brief delay before retrying
            }
            finally
            {
                Interlocked.Decrement(ref _activeWorkers);
            }
        }

        _logger.LogDebug("Worker {WorkerId} stopped", workerId);
    }

    private async Task ProcessNotificationAsync(NotificationEvent notification, int workerId, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogDebug("Worker {WorkerId} processing notification {NotificationId}", workerId, notification.Id);

            var result = await _notificationService.SendAsync(notification, cancellationToken);
            
            stopwatch.Stop();
            var processingTime = stopwatch.ElapsedMilliseconds;

            if (result.IsSuccess)
            {
                _statistics.TotalProcessed++;
                _statistics.LastProcessedAt = DateTime.UtcNow;
                
                // Update average processing time
                var currentAvg = _statistics.AverageProcessingTime;
                var newAvg = (currentAvg + processingTime) / 2;
                _statistics.AverageProcessingTime = newAvg;

                // Update processing times by priority
                var priorityKey = notification.Priority.ToString();
                lock (_statistics.ProcessingTimes)
                {
                    if (_statistics.ProcessingTimes.ContainsKey(priorityKey))
                    {
                        _statistics.ProcessingTimes[priorityKey]++;
                    }
                    else
                    {
                        _statistics.ProcessingTimes[priorityKey] = 1;
                    }
                }

                _logger.LogDebug("Worker {WorkerId} successfully processed notification {NotificationId} in {ProcessingTime}ms", 
                    workerId, notification.Id, processingTime);
            }
            else
            {
                await HandleFailedNotificationAsync(notification, result.ErrorMessage, workerId);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await HandleFailedNotificationAsync(notification, ex.Message, workerId, ex.StackTrace);
        }
    }

    private async Task HandleFailedNotificationAsync(NotificationEvent notification, string errorMessage, int workerId, string? stackTrace = null)
    {
        try
        {
            _statistics.TotalFailed++;

            var failedNotification = new FailedNotification
            {
                OriginalNotificationId = notification.Id,
                TenantId = notification.TenantId,
                OriginalNotification = notification,
                FailureReason = "ProcessingError",
                ErrorMessage = errorMessage,
                StackTrace = stackTrace ?? string.Empty,
                FailedAt = DateTime.UtcNow,
                RetryCount = 0,
                MaxRetries = 3
            };

            await _deadLetterQueueService.AddFailedNotificationAsync(failedNotification);

            _logger.LogWarning("Worker {WorkerId} failed to process notification {NotificationId}: {ErrorMessage}", 
                workerId, notification.Id, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Worker {WorkerId} failed to handle failed notification {NotificationId}", 
                workerId, notification.Id);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }
}