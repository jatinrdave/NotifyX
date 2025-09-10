using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents service metrics.
/// </summary>
public sealed record ServiceMetrics
{
    public Dictionary<string, double> Counters { get; init; } = new();
    public Dictionary<string, double> Gauges { get; init; } = new();
    public Dictionary<string, HistogramData> Histograms { get; init; } = new();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents histogram data.
/// </summary>
public sealed record HistogramData
{
    public double Count { get; init; }
    public double Sum { get; init; }
    public double Min { get; init; }
    public double Max { get; init; }
    public double Mean { get; init; }
    public Dictionary<double, double> Percentiles { get; init; } = new();
}

/// <summary>
/// Represents dashboard configuration.
/// </summary>
public sealed record DashboardConfiguration
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public List<DashboardPanel> Panels { get; init; } = new();
    public DashboardLayout Layout { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a dashboard.
/// </summary>
public sealed record Dashboard
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public List<DashboardPanel> Panels { get; init; } = new();
    public DashboardLayout Layout { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public bool IsPublic { get; init; } = false;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a dashboard panel.
/// </summary>
public sealed record DashboardPanel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty; // chart, table, text, etc.
    public Dictionary<string, object> Configuration { get; init; } = new();
    public PanelPosition Position { get; init; } = new();
    public PanelSize Size { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents panel position.
/// </summary>
public sealed record PanelPosition
{
    public int X { get; init; } = 0;
    public int Y { get; init; } = 0;
}

/// <summary>
/// Represents panel size.
/// </summary>
public sealed record PanelSize
{
    public int Width { get; init; } = 6;
    public int Height { get; init; } = 4;
}

/// <summary>
/// Represents dashboard layout.
/// </summary>
public sealed record DashboardLayout
{
    public int Columns { get; init; } = 12;
    public int RowHeight { get; init; } = 60;
    public bool IsDraggable { get; init; } = true;
    public bool IsResizable { get; init; } = true;
}

/// <summary>
/// Represents alert configuration.
/// </summary>
public sealed record AlertConfiguration
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public AlertCondition Condition { get; init; } = new();
    public List<AlertAction> Actions { get; init; } = new();
    public AlertSeverity Severity { get; init; } = AlertSeverity.Warning;
    public bool IsEnabled { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents an alert.
/// </summary>
public sealed record Alert
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public AlertCondition Condition { get; init; } = new();
    public List<AlertAction> Actions { get; init; } = new();
    public AlertSeverity Severity { get; init; } = AlertSeverity.Warning;
    public AlertStatus Status { get; init; } = AlertStatus.Active;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? LastTriggeredAt { get; init; }
    public DateTime? LastResolvedAt { get; init; }
    public int TriggerCount { get; init; } = 0;
    public bool IsEnabled { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents alert condition.
/// </summary>
public sealed record AlertCondition
{
    public string Metric { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty; // >, <, >=, <=, ==, !=
    public double Threshold { get; init; }
    public TimeSpan EvaluationWindow { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan CooldownPeriod { get; init; } = TimeSpan.FromMinutes(10);
    public Dictionary<string, string> Tags { get; init; } = new();
}

/// <summary>
/// Represents alert action.
/// </summary>
public sealed record AlertAction
{
    public string Type { get; init; } = string.Empty; // email, webhook, slack, etc.
    public Dictionary<string, object> Configuration { get; init; } = new();
    public int Delay { get; init; } = 0; // Delay in seconds
}

/// <summary>
/// Represents alert severity.
/// </summary>
public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Represents alert status.
/// </summary>
public enum AlertStatus
{
    Active,
    Triggered,
    Resolved,
    Disabled
}

/// <summary>
/// Represents alert statistics.
/// </summary>
public sealed record AlertStatistics
{
    public int TotalAlerts { get; init; }
    public int ActiveAlerts { get; init; }
    public int TriggeredAlerts { get; init; }
    public int ResolvedAlerts { get; init; }
    public Dictionary<AlertSeverity, int> AlertsBySeverity { get; init; } = new();
    public TimeSpan AverageResolutionTime { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents trace information.
/// </summary>
public sealed record TraceInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public TimeSpan Duration { get; init; }
    public TraceStatus Status { get; init; } = TraceStatus.Success;
    public List<SpanInfo> Spans { get; init; } = new();
    public Dictionary<string, object> Attributes { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents span information.
/// </summary>
public sealed record SpanInfo
{
    public string Id { get; init; } = string.Empty;
    public string TraceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public TimeSpan Duration { get; init; }
    public SpanStatus Status { get; init; } = SpanStatus.Ok;
    public List<SpanEvent> Events { get; init; } = new();
    public Dictionary<string, object> Attributes { get; init; } = new();
    public Exception? Exception { get; init; }
}

/// <summary>
/// Represents span event.
/// </summary>
public sealed record SpanEvent
{
    public string Name { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Attributes { get; init; } = new();
}

/// <summary>
/// Represents trace status.
/// </summary>
public enum TraceStatus
{
    Success,
    Error,
    Timeout,
    Cancelled
}

/// <summary>
/// Represents span status.
/// </summary>
public enum SpanStatus
{
    Ok,
    Error,
    Unset
}

/// <summary>
/// Represents trace statistics.
/// </summary>
public sealed record TraceStatistics
{
    public int TotalTraces { get; init; }
    public int SuccessfulTraces { get; init; }
    public int FailedTraces { get; init; }
    public double SuccessRate { get; init; }
    public TimeSpan AverageDuration { get; init; }
    public TimeSpan P95Duration { get; init; }
    public TimeSpan P99Duration { get; init; }
    public Dictionary<string, int> TracesByStatus { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents deployment configuration.
/// </summary>
public sealed record DeploymentConfiguration
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
    public int InstanceCount { get; init; } = 1;
    public Dictionary<string, string> EnvironmentVariables { get; init; } = new();
    public Dictionary<string, object> Resources { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents deployment result.
/// </summary>
public sealed record DeploymentResult
{
    public bool IsSuccess { get; init; }
    public string DeploymentId { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime DeployedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents deployment status.
/// </summary>
public sealed record DeploymentStatus
{
    public string DeploymentId { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public DeploymentState State { get; init; } = DeploymentState.Pending;
    public int InstanceCount { get; init; }
    public int RunningInstances { get; init; }
    public int HealthyInstances { get; init; }
    public DateTime DeployedAt { get; init; }
    public DateTime? LastUpdatedAt { get; init; }
    public string Version { get; init; } = string.Empty;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents deployment state.
/// </summary>
public enum DeploymentState
{
    Pending,
    Deploying,
    Running,
    Failed,
    Stopped,
    RollingBack
}

/// <summary>
/// Represents a region.
/// </summary>
public sealed record Region
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public bool IsAvailable { get; init; } = true;
    public List<string> SupportedServices { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}