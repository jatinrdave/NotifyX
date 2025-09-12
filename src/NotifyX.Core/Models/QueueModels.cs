using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents queue statistics.
/// </summary>
public sealed record QueueStatistics
{
    public long TotalMessages { get; init; }
    public long ProcessedMessages { get; init; }
    public long FailedMessages { get; init; }
    public long PendingMessages { get; init; }
    public double AverageProcessingTime { get; init; }
    public DateTime LastProcessedAt { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents dead letter queue statistics.
/// </summary>
public sealed record DeadLetterQueueStatistics
{
    public long TotalFailedMessages { get; init; }
    public long RetriedMessages { get; init; }
    public long PermanentlyFailedMessages { get; init; }
    public DateTime LastFailedAt { get; init; }
    public Dictionary<string, long> FailureReasons { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a failed notification.
/// </summary>
public sealed record FailedNotification
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string OriginalNotificationId { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public NotificationEvent OriginalNotification { get; init; } = null!;
    public string FailureReason { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public string StackTrace { get; init; } = string.Empty;
    public DateTime FailedAt { get; init; } = DateTime.UtcNow;
    public int RetryCount { get; init; } = 0;
    public int MaxRetries { get; init; } = 3;
    public DateTime? NextRetryAt { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents worker service status.
/// </summary>
public sealed record WorkerStatus
{
    public bool IsRunning { get; init; }
    public int ActiveWorkers { get; init; }
    public int IdleWorkers { get; init; }
    public int TotalWorkers { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? LastProcessedAt { get; init; }
    public string CurrentOperation { get; init; } = string.Empty;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents worker service statistics.
/// </summary>
public sealed record WorkerStatistics
{
    public long TotalProcessed { get; init; }
    public long TotalFailed { get; init; }
    public double AverageProcessingTime { get; init; }
    public double ThroughputPerSecond { get; init; }
    public DateTime LastProcessedAt { get; init; }
    public Dictionary<string, long> ProcessingTimes { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a queue message.
/// </summary>
public sealed record QueueMessage
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string TenantId { get; init; } = string.Empty;
    public NotificationEvent Notification { get; init; } = null!;
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
    public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ScheduledFor { get; init; }
    public int RetryCount { get; init; } = 0;
    public int MaxRetries { get; init; } = 3;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents queue configuration options.
/// </summary>
public sealed record QueueOptions
{
    public string ConnectionString { get; init; } = string.Empty;
    public string QueueName { get; init; } = "notifyx-notifications";
    public string DeadLetterQueueName { get; init; } = "notifyx-dlq";
    public int MaxRetries { get; init; } = 3;
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan MessageVisibilityTimeout { get; init; } = TimeSpan.FromMinutes(30);
    public int BatchSize { get; init; } = 10;
    public int MaxConcurrency { get; init; } = 10;
    public bool EnableDeadLetterQueue { get; init; } = true;
    public bool EnablePriorityQueue { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents worker configuration options.
/// </summary>
public sealed record WorkerOptions
{
    public int WorkerCount { get; init; } = Environment.ProcessorCount;
    public TimeSpan PollingInterval { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan ProcessingTimeout { get; init; } = TimeSpan.FromMinutes(5);
    public int MaxConcurrentOperations { get; init; } = 10;
    public bool EnableAutoScaling { get; init; } = false;
    public int MinWorkers { get; init; } = 1;
    public int MaxWorkers { get; init; } = 20;
    public TimeSpan ScaleUpThreshold { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan ScaleDownThreshold { get; init; } = TimeSpan.FromMinutes(5);
    public Dictionary<string, object> Metadata { get; init; } = new();
}