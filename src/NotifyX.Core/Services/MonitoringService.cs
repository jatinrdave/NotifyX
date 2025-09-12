using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Core.Services;

/// <summary>
/// Monitoring service implementation for dashboards, alerts, and monitoring.
/// </summary>
public class MonitoringService : IMonitoringService
{
    private readonly ILogger<MonitoringService> _logger;
    private readonly Dictionary<string, Dashboard> _dashboards = new();
    private readonly Dictionary<string, Alert> _alerts = new();
    private readonly Dictionary<string, List<AlertTrigger>> _alertHistory = new();

    public MonitoringService(ILogger<MonitoringService> logger)
    {
        _logger = logger;
    }

    public async Task<Dashboard> CreateDashboardAsync(DashboardConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating dashboard: {DashboardName}", config.Name);

            var dashboard = new Dashboard
            {
                Id = Guid.NewGuid().ToString(),
                Name = config.Name,
                Description = config.Description,
                TenantId = config.TenantId,
                Panels = config.Panels,
                Layout = config.Layout,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>(config.Metadata)
            };

            _dashboards[dashboard.Id] = dashboard;

            _logger.LogInformation("Created dashboard: {DashboardId} - {DashboardName}", dashboard.Id, dashboard.Name);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create dashboard: {DashboardName}", config.Name);
            throw;
        }
    }

    public async Task<IEnumerable<Dashboard>> GetDashboardsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting dashboards for tenant: {TenantId}", tenantId);

            // Simulate async operation
            await Task.Delay(10, cancellationToken);

            var dashboards = _dashboards.Values.Where(d => d.TenantId == tenantId).ToList();

            _logger.LogInformation("Retrieved {Count} dashboards for tenant {TenantId}", dashboards.Count, tenantId);

            return dashboards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dashboards for tenant: {TenantId}", tenantId);
            return new List<Dashboard>();
        }
    }

    public async Task<Alert> CreateAlertAsync(AlertConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating alert: {AlertName}", config.Name);

            var alert = new Alert
            {
                Id = Guid.NewGuid().ToString(),
                Name = config.Name,
                Description = config.Description,
                TenantId = config.TenantId,
                Condition = config.Condition,
                Actions = config.Actions,
                Severity = config.Severity,
                Status = AlertStatus.Active,
                CreatedAt = DateTime.UtcNow,
                IsEnabled = config.IsEnabled,
                Metadata = new Dictionary<string, object>(config.Metadata)
            };

            _alerts[alert.Id] = alert;
            _alertHistory[alert.Id] = new List<AlertTrigger>();

            _logger.LogInformation("Created alert: {AlertId} - {AlertName}", alert.Id, alert.Name);

            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create alert: {AlertName}", config.Name);
            throw;
        }
    }

    public async Task<IEnumerable<Alert>> GetAlertsAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting alerts for tenant: {TenantId}", tenantId);

            // Simulate async operation
            await Task.Delay(10, cancellationToken);

            var alerts = _alerts.Values.Where(a => a.TenantId == tenantId).ToList();

            _logger.LogInformation("Retrieved {Count} alerts for tenant {TenantId}", alerts.Count, tenantId);

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alerts for tenant: {TenantId}", tenantId);
            return new List<Alert>();
        }
    }

    public async Task<bool> TriggerAlertAsync(string alertId, AlertSeverity severity, string message, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Triggering alert: {AlertId} with severity {Severity}", alertId, severity);

            if (!_alerts.TryGetValue(alertId, out var alert))
            {
                _logger.LogWarning("Alert not found: {AlertId}", alertId);
                return false;
            }

            if (!alert.IsEnabled)
            {
                _logger.LogDebug("Alert is disabled: {AlertId}", alertId);
                return false;
            }

            // Check cooldown period
            var lastTrigger = _alertHistory[alertId].LastOrDefault();
            if (lastTrigger != null && DateTime.UtcNow - lastTrigger.TriggeredAt < alert.Condition.CooldownPeriod)
            {
                _logger.LogDebug("Alert is in cooldown period: {AlertId}", alertId);
                return false;
            }

            // Update alert status
            alert = alert with
            {
                Status = AlertStatus.Triggered,
                LastTriggeredAt = DateTime.UtcNow,
                TriggerCount = alert.TriggerCount + 1
            };
            _alerts[alertId] = alert;

            // Record alert trigger
            var trigger = new AlertTrigger
            {
                Id = Guid.NewGuid().ToString(),
                AlertId = alertId,
                Severity = severity,
                Message = message,
                Context = context ?? new Dictionary<string, object>(),
                TriggeredAt = DateTime.UtcNow
            };
            _alertHistory[alertId].Add(trigger);

            // Execute alert actions
            await ExecuteAlertActionsAsync(alert, trigger, cancellationToken);

            _logger.LogInformation("Alert triggered successfully: {AlertId}", alertId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger alert: {AlertId}", alertId);
            return false;
        }
    }

    public async Task<bool> ResolveAlertAsync(string alertId, string resolution, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Resolving alert: {AlertId}", alertId);

            if (!_alerts.TryGetValue(alertId, out var alert))
            {
                _logger.LogWarning("Alert not found: {AlertId}", alertId);
                return false;
            }

            // Update alert status
            alert = alert with
            {
                Status = AlertStatus.Resolved,
                LastResolvedAt = DateTime.UtcNow
            };
            _alerts[alertId] = alert;

            _logger.LogInformation("Alert resolved: {AlertId} - {Resolution}", alertId, resolution);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve alert: {AlertId}", alertId);
            return false;
        }
    }

    public async Task<AlertStatistics> GetAlertStatisticsAsync(string tenantId, TimeSpan timeRange, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting alert statistics for tenant: {TenantId}, time range: {TimeRange}", tenantId, timeRange);

            // Simulate async operation
            await Task.Delay(10, cancellationToken);

            var tenantAlerts = _alerts.Values.Where(a => a.TenantId == tenantId).ToList();
            var cutoffTime = DateTime.UtcNow - timeRange;

            var statistics = new AlertStatistics
            {
                TotalAlerts = tenantAlerts.Count,
                ActiveAlerts = tenantAlerts.Count(a => a.Status == AlertStatus.Active),
                TriggeredAlerts = tenantAlerts.Count(a => a.Status == AlertStatus.Triggered),
                ResolvedAlerts = tenantAlerts.Count(a => a.Status == AlertStatus.Resolved),
                AlertsBySeverity = tenantAlerts.GroupBy(a => a.Severity)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageResolutionTime = CalculateAverageResolutionTime(tenantAlerts),
                Metadata = new Dictionary<string, object>
                {
                    ["timeRange"] = timeRange,
                    ["cutoffTime"] = cutoffTime
                }
            };

            _logger.LogInformation("Retrieved alert statistics for tenant {TenantId}: {TotalAlerts} total, {ActiveAlerts} active", 
                tenantId, statistics.TotalAlerts, statistics.ActiveAlerts);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get alert statistics for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    private async Task ExecuteAlertActionsAsync(Alert alert, AlertTrigger trigger, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var action in alert.Actions)
            {
                try
                {
                    // Add delay if specified
                    if (action.Delay > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(action.Delay), cancellationToken);
                    }

                    await ExecuteAlertActionAsync(action, alert, trigger, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute alert action: {ActionType} for alert {AlertId}", 
                        action.Type, alert.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute alert actions for alert: {AlertId}", alert.Id);
        }
    }

    private async Task ExecuteAlertActionAsync(AlertAction action, Alert alert, AlertTrigger trigger, CancellationToken cancellationToken)
    {
        // Simulate action execution
        await Task.Delay(50, cancellationToken);

        switch (action.Type.ToLower())
        {
            case "email":
                _logger.LogInformation("Sending email alert for {AlertName}: {Message}", alert.Name, trigger.Message);
                break;
            case "webhook":
                _logger.LogInformation("Sending webhook alert for {AlertName}: {Message}", alert.Name, trigger.Message);
                break;
            case "slack":
                _logger.LogInformation("Sending Slack alert for {AlertName}: {Message}", alert.Name, trigger.Message);
                break;
            default:
                _logger.LogWarning("Unknown alert action type: {ActionType}", action.Type);
                break;
        }
    }

    private static TimeSpan CalculateAverageResolutionTime(List<Alert> alerts)
    {
        var resolvedAlerts = alerts.Where(a => a.LastResolvedAt.HasValue && a.LastTriggeredAt.HasValue).ToList();
        
        if (!resolvedAlerts.Any())
        {
            return TimeSpan.Zero;
        }

        var totalResolutionTime = resolvedAlerts.Sum(a => (a.LastResolvedAt!.Value - a.LastTriggeredAt!.Value).TotalMilliseconds);
        return TimeSpan.FromMilliseconds(totalResolutionTime / resolvedAlerts.Count);
    }

    private class AlertTrigger
    {
        public string Id { get; init; } = string.Empty;
        public string AlertId { get; init; } = string.Empty;
        public AlertSeverity Severity { get; init; }
        public string Message { get; init; } = string.Empty;
        public Dictionary<string, object> Context { get; init; } = new();
        public DateTime TriggeredAt { get; init; }
    }
}