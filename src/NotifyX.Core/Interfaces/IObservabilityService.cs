using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Interface for observability service operations.
/// </summary>
public interface IObservabilityService
{
    /// <summary>
    /// Records a metric.
    /// </summary>
    Task RecordMetricAsync(string name, double value, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a counter metric.
    /// </summary>
    Task IncrementCounterAsync(string name, double increment = 1.0, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a histogram metric.
    /// </summary>
    Task RecordHistogramAsync(string name, double value, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a gauge metric.
    /// </summary>
    Task SetGaugeAsync(string name, double value, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a span for tracing.
    /// </summary>
    Task<ISpan> StartSpanAsync(string name, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records an event.
    /// </summary>
    Task RecordEventAsync(string name, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets service health status.
    /// </summary>
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets service metrics.
    /// </summary>
    Task<ServiceMetrics> GetMetricsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for monitoring service operations.
/// </summary>
public interface IMonitoringService
{
    /// <summary>
    /// Creates a monitoring dashboard.
    /// </summary>
    Task<Dashboard> CreateDashboardAsync(DashboardConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets monitoring dashboards.
    /// </summary>
    Task<IEnumerable<Dashboard>> GetDashboardsAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a monitoring alert.
    /// </summary>
    Task<Alert> CreateAlertAsync(AlertConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets monitoring alerts.
    /// </summary>
    Task<IEnumerable<Alert>> GetAlertsAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers an alert.
    /// </summary>
    Task<bool> TriggerAlertAsync(string alertId, AlertSeverity severity, string message, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an alert.
    /// </summary>
    Task<bool> ResolveAlertAsync(string alertId, string resolution, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets alert statistics.
    /// </summary>
    Task<AlertStatistics> GetAlertStatisticsAsync(string tenantId, TimeSpan timeRange, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for tracing service operations.
/// </summary>
public interface ITracingService
{
    /// <summary>
    /// Creates a new trace.
    /// </summary>
    Task<ITrace> StartTraceAsync(string name, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trace information.
    /// </summary>
    Task<TraceInfo?> GetTraceAsync(string traceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets traces for a time range.
    /// </summary>
    Task<IEnumerable<TraceInfo>> GetTracesAsync(string tenantId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trace statistics.
    /// </summary>
    Task<TraceStatistics> GetTraceStatisticsAsync(string tenantId, TimeSpan timeRange, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for deployment service operations.
/// </summary>
public interface IDeploymentService
{
    /// <summary>
    /// Deploys to a region.
    /// </summary>
    Task<DeploymentResult> DeployToRegionAsync(string region, DeploymentConfiguration config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets deployment status.
    /// </summary>
    Task<DeploymentStatus> GetDeploymentStatusAsync(string deploymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available regions.
    /// </summary>
    Task<IEnumerable<Region>> GetAvailableRegionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Scales deployment.
    /// </summary>
    Task<bool> ScaleDeploymentAsync(string deploymentId, int instanceCount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back deployment.
    /// </summary>
    Task<bool> RollbackDeploymentAsync(string deploymentId, string targetVersion, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for span operations.
/// </summary>
public interface ISpan : IDisposable
{
    /// <summary>
    /// Gets the span ID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the trace ID.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Sets span attributes.
    /// </summary>
    void SetAttributes(Dictionary<string, object> attributes);

    /// <summary>
    /// Adds an event to the span.
    /// </summary>
    void AddEvent(string name, Dictionary<string, object>? attributes = null);

    /// <summary>
    /// Sets span status.
    /// </summary>
    void SetStatus(SpanStatus status, string? description = null);

    /// <summary>
    /// Records an exception.
    /// </summary>
    void RecordException(Exception exception);

    /// <summary>
    /// Ends the span.
    /// </summary>
    void End();
}

/// <summary>
/// Interface for trace operations.
/// </summary>
public interface ITrace : IDisposable
{
    /// <summary>
    /// Gets the trace ID.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Creates a span within the trace.
    /// </summary>
    Task<ISpan> CreateSpanAsync(string name, Dictionary<string, object>? attributes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets trace attributes.
    /// </summary>
    void SetAttributes(Dictionary<string, object> attributes);

    /// <summary>
    /// Ends the trace.
    /// </summary>
    void End();
}