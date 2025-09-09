using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Collections.Concurrent;

namespace NotifyX.Core.Services;

/// <summary>
/// Default implementation of the audit service.
/// Provides audit logging functionality for security and compliance.
/// </summary>
public sealed class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly AuditOptions _options;
    private readonly ConcurrentDictionary<string, AuditEvent> _auditEvents = new();
    private readonly ConcurrentQueue<AuditEvent> _auditQueue = new();

    /// <summary>
    /// Initializes a new instance of the AuditService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The audit options.</param>
    public AuditService(
        ILogger<AuditService> logger,
        IOptions<AuditOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task LogAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.IsEnabled)
            {
                return;
            }

            // Validate audit event
            if (string.IsNullOrEmpty(auditEvent.EventType))
            {
                _logger.LogWarning("Audit event missing event type, skipping");
                return;
            }

            // Add timestamp if not set
            var eventToLog = auditEvent with
            {
                Timestamp = auditEvent.Timestamp == default ? DateTime.UtcNow : auditEvent.Timestamp
            };

            // Store audit event
            _auditEvents[eventToLog.Id] = eventToLog;

            // Add to queue for processing
            _auditQueue.Enqueue(eventToLog);

            // Log to application logger
            LogAuditEventToLogger(eventToLog);

            // Process queue if it's getting large
            if (_auditQueue.Count > _options.BatchSize)
            {
                await ProcessAuditQueueAsync(cancellationToken);
            }

            _logger.LogDebug("Audit event logged: {EventType} for user {UserId}", eventToLog.EventType, eventToLog.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging audit event: {EventType}", auditEvent.EventType);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditEvent>> GetAuditEventsAsync(string userId, string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting audit events for user {UserId} in tenant {TenantId}", userId, tenantId);

            var events = _auditEvents.Values
                .Where(e => e.UserId == userId && e.TenantId == tenantId)
                .AsQueryable();

            if (startDate.HasValue)
            {
                events = events.Where(e => e.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                events = events.Where(e => e.Timestamp <= endDate.Value);
            }

            return events.OrderByDescending(e => e.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit events for user {UserId}", userId);
            return Enumerable.Empty<AuditEvent>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditEvent>> GetAuditEventsForTenantAsync(string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting audit events for tenant {TenantId}", tenantId);

            var events = _auditEvents.Values
                .Where(e => e.TenantId == tenantId)
                .AsQueryable();

            if (startDate.HasValue)
            {
                events = events.Where(e => e.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                events = events.Where(e => e.Timestamp <= endDate.Value);
            }

            return events.OrderByDescending(e => e.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit events for tenant {TenantId}", tenantId);
            return Enumerable.Empty<AuditEvent>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditEvent>> GetAuditEventsByTypeAsync(string eventType, string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting audit events of type {EventType} for tenant {TenantId}", eventType, tenantId);

            var events = _auditEvents.Values
                .Where(e => e.EventType == eventType && e.TenantId == tenantId)
                .AsQueryable();

            if (startDate.HasValue)
            {
                events = events.Where(e => e.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                events = events.Where(e => e.Timestamp <= endDate.Value);
            }

            return events.OrderByDescending(e => e.Timestamp).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit events of type {EventType}", eventType);
            return Enumerable.Empty<AuditEvent>();
        }
    }

    #region Private Helper Methods

    private void LogAuditEventToLogger(AuditEvent auditEvent)
    {
        var logLevel = GetLogLevel(auditEvent.Severity);
        var message = "Audit Event: {EventType} - User: {UserId} ({UserName}) - Action: {Action} - Resource: {Resource} - Result: {Result}";

        _logger.Log(logLevel, message,
            auditEvent.EventType,
            auditEvent.UserId,
            auditEvent.UserName,
            auditEvent.Action,
            auditEvent.Resource,
            auditEvent.Result);

        // Log additional details if present
        if (auditEvent.Details.Any())
        {
            _logger.Log(logLevel, "Audit Event Details: {@Details}", auditEvent.Details);
        }

        // Log IP address and user agent if present
        if (!string.IsNullOrEmpty(auditEvent.IpAddress) || !string.IsNullOrEmpty(auditEvent.UserAgent))
        {
            _logger.Log(logLevel, "Audit Event Context - IP: {IpAddress}, UserAgent: {UserAgent}",
                auditEvent.IpAddress ?? "Unknown",
                auditEvent.UserAgent ?? "Unknown");
        }
    }

    private static LogLevel GetLogLevel(AuditEventSeverity severity)
    {
        return severity switch
        {
            AuditEventSeverity.Low => LogLevel.Information,
            AuditEventSeverity.Medium => LogLevel.Warning,
            AuditEventSeverity.High => LogLevel.Error,
            AuditEventSeverity.Critical => LogLevel.Critical,
            AuditEventSeverity.Info => LogLevel.Information,
            _ => LogLevel.Information
        };
    }

    private async Task ProcessAuditQueueAsync(CancellationToken cancellationToken)
    {
        try
        {
            var batch = new List<AuditEvent>();
            
            // Dequeue events in batches
            while (_auditQueue.TryDequeue(out var auditEvent) && batch.Count < _options.BatchSize)
            {
                batch.Add(auditEvent);
            }

            if (batch.Any())
            {
                await ProcessAuditBatchAsync(batch, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit queue");
        }
    }

    private async Task ProcessAuditBatchAsync(List<AuditEvent> batch, CancellationToken cancellationToken)
    {
        try
        {
            // In a real implementation, you would:
            // 1. Store to database
            // 2. Send to external audit system
            // 3. Archive to long-term storage
            // 4. Send alerts for critical events

            foreach (var auditEvent in batch)
            {
                await ProcessIndividualAuditEventAsync(auditEvent, cancellationToken);
            }

            _logger.LogDebug("Processed batch of {Count} audit events", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit batch of {Count} events", batch.Count);
        }
    }

    private async Task ProcessIndividualAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Check for critical events that need immediate attention
            if (auditEvent.Severity == AuditEventSeverity.Critical)
            {
                await HandleCriticalAuditEventAsync(auditEvent, cancellationToken);
            }

            // Check for security-related events
            if (auditEvent.Category == AuditEventCategory.Security)
            {
                await HandleSecurityAuditEventAsync(auditEvent, cancellationToken);
            }

            // Check for authentication failures
            if (auditEvent.EventType == "authentication_failure")
            {
                await HandleAuthenticationFailureAsync(auditEvent, cancellationToken);
            }

            // Store to persistent storage (in a real implementation)
            await StoreAuditEventAsync(auditEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing individual audit event {EventId}", auditEvent.Id);
        }
    }

    private async Task HandleCriticalAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        _logger.LogCritical("CRITICAL AUDIT EVENT: {EventType} - User: {UserId} - Action: {Action} - Resource: {Resource}",
            auditEvent.EventType, auditEvent.UserId, auditEvent.Action, auditEvent.Resource);

        // In a real implementation, you would:
        // 1. Send immediate alerts to administrators
        // 2. Trigger incident response procedures
        // 3. Log to external monitoring systems
        // 4. Create security tickets

        await Task.CompletedTask;
    }

    private async Task HandleSecurityAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        _logger.LogWarning("SECURITY AUDIT EVENT: {EventType} - User: {UserId} - Action: {Action} - Resource: {Resource}",
            auditEvent.EventType, auditEvent.UserId, auditEvent.Action, auditEvent.Resource);

        // In a real implementation, you would:
        // 1. Check for suspicious patterns
        // 2. Update security metrics
        // 3. Trigger security workflows
        // 4. Update user risk scores

        await Task.CompletedTask;
    }

    private async Task HandleAuthenticationFailureAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        _logger.LogWarning("AUTHENTICATION FAILURE: User: {UserId} - IP: {IpAddress} - Reason: {Reason}",
            auditEvent.UserId, auditEvent.IpAddress, auditEvent.Details.GetValueOrDefault("Reason", "Unknown"));

        // In a real implementation, you would:
        // 1. Track failed login attempts
        // 2. Implement account lockout policies
        // 3. Update user risk scores
        // 4. Send security notifications

        await Task.CompletedTask;
    }

    private async Task StoreAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        // In a real implementation, you would store to:
        // 1. Database (PostgreSQL, SQL Server, etc.)
        // 2. Search engine (Elasticsearch, OpenSearch)
        // 3. Time-series database (InfluxDB, TimescaleDB)
        // 4. Object storage (S3, Azure Blob Storage)

        // For demo purposes, we're just keeping it in memory
        await Task.CompletedTask;
    }

    #endregion
}

/// <summary>
/// Configuration options for audit logging.
/// </summary>
public sealed class AuditOptions
{
    /// <summary>
    /// Whether audit logging is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Batch size for processing audit events.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum number of audit events to keep in memory.
    /// </summary>
    public int MaxInMemoryEvents { get; set; } = 10000;

    /// <summary>
    /// Retention period for audit events.
    /// </summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(90);

    /// <summary>
    /// Whether to enable detailed logging for audit events.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable real-time alerting for critical events.
    /// </summary>
    public bool EnableRealTimeAlerting { get; set; } = true;

    /// <summary>
    /// Whether to encrypt sensitive audit events.
    /// </summary>
    public bool EnableEncryption { get; set; } = true;

    /// <summary>
    /// Encryption key for sensitive audit events.
    /// </summary>
    public string EncryptionKey { get; set; } = "your-encryption-key-here";

    /// <summary>
    /// Whether to enable audit event archiving.
    /// </summary>
    public bool EnableArchiving { get; set; } = true;

    /// <summary>
    /// Archive retention period.
    /// </summary>
    public TimeSpan ArchiveRetentionPeriod { get; set; } = TimeSpan.FromDays(365);

    /// <summary>
    /// Whether to enable audit event compression.
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Whether to enable audit event deduplication.
    /// </summary>
    public bool EnableDeduplication { get; set; } = true;

    /// <summary>
    /// Deduplication window for similar events.
    /// </summary>
    public TimeSpan DeduplicationWindow { get; set; } = TimeSpan.FromMinutes(5);
}