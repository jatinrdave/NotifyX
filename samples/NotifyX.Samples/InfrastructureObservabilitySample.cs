using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Samples;

/// <summary>
/// Sample demonstrating infrastructure and observability functionality.
/// </summary>
public class InfrastructureObservabilitySample
{
    private readonly ILogger<InfrastructureObservabilitySample> _logger;
    private readonly IObservabilityService _observabilityService;
    private readonly IMonitoringService _monitoringService;
    private readonly IDeploymentService _deploymentService;

    public InfrastructureObservabilitySample(
        ILogger<InfrastructureObservabilitySample> logger,
        IObservabilityService observabilityService,
        IMonitoringService monitoringService,
        IDeploymentService deploymentService)
    {
        _logger = logger;
        _observabilityService = observabilityService;
        _monitoringService = monitoringService;
        _deploymentService = deploymentService;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Infrastructure & Observability Sample");

        try
        {
            // Demonstrate observability service
            await DemonstrateObservabilityServiceAsync();

            // Demonstrate monitoring service
            await DemonstrateMonitoringServiceAsync();

            // Demonstrate deployment service
            await DemonstrateDeploymentServiceAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Infrastructure & Observability Sample");
        }
    }

    private async Task DemonstrateObservabilityServiceAsync()
    {
        _logger.LogInformation("=== Observability Service Demonstration ===");

        // Record various metrics
        await _observabilityService.IncrementCounterAsync("notifications.sent", 1.0, new Dictionary<string, string>
        {
            ["tenant"] = "test-tenant",
            ["channel"] = "email"
        });

        await _observabilityService.IncrementCounterAsync("notifications.sent", 1.0, new Dictionary<string, string>
        {
            ["tenant"] = "test-tenant",
            ["channel"] = "sms"
        });

        await _observabilityService.SetGaugeAsync("active.connections", 150.0, new Dictionary<string, string>
        {
            ["region"] = "us-east-1"
        });

        await _observabilityService.RecordHistogramAsync("notification.delivery.time", 2.5, new Dictionary<string, string>
        {
            ["channel"] = "email",
            ["priority"] = "normal"
        });

        await _observabilityService.RecordHistogramAsync("notification.delivery.time", 1.8, new Dictionary<string, string>
        {
            ["channel"] = "sms",
            ["priority"] = "high"
        });

        // Record events
        await _observabilityService.RecordEventAsync("user.login", new Dictionary<string, object>
        {
            ["user_id"] = "user123",
            ["tenant_id"] = "tenant456",
            ["ip_address"] = "192.168.1.100"
        });

        await _observabilityService.RecordEventAsync("notification.failed", new Dictionary<string, object>
        {
            ["notification_id"] = "notif789",
            ["error_code"] = "PROVIDER_UNAVAILABLE",
            ["retry_count"] = 2
        });

        // Create spans for tracing
        using var span = await _observabilityService.StartSpanAsync("notification.processing", new Dictionary<string, object>
        {
            ["notification_id"] = "notif123",
            ["tenant_id"] = "tenant456",
            ["event_type"] = "user.welcome"
        });

        span.SetAttributes(new Dictionary<string, object>
        {
            ["priority"] = "normal",
            ["channel"] = "email"
        });

        span.AddEvent("template.rendered", new Dictionary<string, object>
        {
            ["template_id"] = "template789",
            ["render_time_ms"] = 45
        });

        // Simulate some work
        await Task.Delay(100);

        span.AddEvent("notification.sent", new Dictionary<string, object>
        {
            ["provider"] = "sendgrid",
            ["delivery_time_ms"] = 1200
        });

        span.SetStatus(SpanStatus.Ok, "Notification sent successfully");

        // Get service health
        var healthStatus = await _observabilityService.GetHealthStatusAsync();
        _logger.LogInformation("Observability service health: {Status}", healthStatus);

        // Get metrics
        var metrics = await _observabilityService.GetMetricsAsync();
        _logger.LogInformation("Service metrics - Counters: {CounterCount}, Gauges: {GaugeCount}, Histograms: {HistogramCount}",
            metrics.Counters.Count, metrics.Gauges.Count, metrics.Histograms.Count);

        foreach (var counter in metrics.Counters.Take(3))
        {
            _logger.LogInformation("  Counter {Name}: {Value}", counter.Key, counter.Value);
        }

        foreach (var gauge in metrics.Gauges.Take(3))
        {
            _logger.LogInformation("  Gauge {Name}: {Value}", gauge.Key, gauge.Value);
        }

        foreach (var histogram in metrics.Histograms.Take(3))
        {
            _logger.LogInformation("  Histogram {Name}: Count={Count}, Mean={Mean}, P95={P95}",
                histogram.Key, histogram.Value.Count, histogram.Value.Mean, 
                histogram.Value.Percentiles.GetValueOrDefault(95.0, 0));
        }
    }

    private async Task DemonstrateMonitoringServiceAsync()
    {
        _logger.LogInformation("=== Monitoring Service Demonstration ===");

        // Create a dashboard
        var dashboardConfig = new DashboardConfiguration
        {
            Name = "NotifyX Operations Dashboard",
            Description = "Real-time monitoring dashboard for NotifyX operations",
            TenantId = "test-tenant",
            Panels = new List<DashboardPanel>
            {
                new DashboardPanel
                {
                    Title = "Notifications Sent",
                    Type = "line-chart",
                    Configuration = new Dictionary<string, object>
                    {
                        ["metric"] = "notifications.sent",
                        ["timeRange"] = "1h"
                    },
                    Position = new PanelPosition { X = 0, Y = 0 },
                    Size = new PanelSize { Width = 6, Height = 4 }
                },
                new DashboardPanel
                {
                    Title = "Delivery Success Rate",
                    Type = "gauge",
                    Configuration = new Dictionary<string, object>
                    {
                        ["metric"] = "delivery.success.rate",
                        ["threshold"] = 0.95
                    },
                    Position = new PanelPosition { X = 6, Y = 0 },
                    Size = new PanelSize { Width = 6, Height = 4 }
                },
                new DashboardPanel
                {
                    Title = "Active Alerts",
                    Type = "table",
                    Configuration = new Dictionary<string, object>
                    {
                        ["query"] = "alerts.status=triggered"
                    },
                    Position = new PanelPosition { X = 0, Y = 4 },
                    Size = new PanelSize { Width = 12, Height = 4 }
                }
            },
            Layout = new DashboardLayout
            {
                Columns = 12,
                RowHeight = 60
            }
        };

        var dashboard = await _monitoringService.CreateDashboardAsync(dashboardConfig);
        _logger.LogInformation("Created dashboard: {DashboardId} - {DashboardName}", dashboard.Id, dashboard.Name);

        // Get dashboards
        var dashboards = await _monitoringService.GetDashboardsAsync("test-tenant");
        _logger.LogInformation("Retrieved {Count} dashboards for tenant", dashboards.Count());

        // Create alerts
        var alertConfigs = new[]
        {
            new AlertConfiguration
            {
                Name = "High Error Rate",
                Description = "Alert when error rate exceeds 5%",
                TenantId = "test-tenant",
                Condition = new AlertCondition
                {
                    Metric = "error.rate",
                    Operator = ">",
                    Threshold = 0.05,
                    EvaluationWindow = TimeSpan.FromMinutes(5),
                    CooldownPeriod = TimeSpan.FromMinutes(10)
                },
                Actions = new List<AlertAction>
                {
                    new AlertAction
                    {
                        Type = "email",
                        Configuration = new Dictionary<string, object>
                        {
                            ["recipients"] = new[] { "admin@example.com" },
                            ["subject"] = "High Error Rate Alert"
                        }
                    },
                    new AlertAction
                    {
                        Type = "slack",
                        Configuration = new Dictionary<string, object>
                        {
                            ["channel"] = "#alerts",
                            ["webhook_url"] = "https://hooks.slack.com/services/..."
                        }
                    }
                },
                Severity = AlertSeverity.Critical
            },
            new AlertConfiguration
            {
                Name = "Low Delivery Rate",
                Description = "Alert when delivery rate drops below 90%",
                TenantId = "test-tenant",
                Condition = new AlertCondition
                {
                    Metric = "delivery.success.rate",
                    Operator = "<",
                    Threshold = 0.90,
                    EvaluationWindow = TimeSpan.FromMinutes(10),
                    CooldownPeriod = TimeSpan.FromMinutes(15)
                },
                Actions = new List<AlertAction>
                {
                    new AlertAction
                    {
                        Type = "webhook",
                        Configuration = new Dictionary<string, object>
                        {
                            ["url"] = "https://api.example.com/alerts",
                            ["method"] = "POST"
                        }
                    }
                },
                Severity = AlertSeverity.Warning
            }
        };

        var alerts = new List<Alert>();
        foreach (var config in alertConfigs)
        {
            var alert = await _monitoringService.CreateAlertAsync(config);
            alerts.Add(alert);
            _logger.LogInformation("Created alert: {AlertId} - {AlertName}", alert.Id, alert.Name);
        }

        // Get alerts
        var tenantAlerts = await _monitoringService.GetAlertsAsync("test-tenant");
        _logger.LogInformation("Retrieved {Count} alerts for tenant", tenantAlerts.Count());

        // Trigger alerts
        var triggerResults = new[]
        {
            await _monitoringService.TriggerAlertAsync(alerts[0].Id, AlertSeverity.Critical, 
                "Error rate has exceeded 5% threshold", new Dictionary<string, object>
                {
                    ["current_rate"] = 0.07,
                    ["threshold"] = 0.05,
                    ["time_window"] = "5 minutes"
                }),
            await _monitoringService.TriggerAlertAsync(alerts[1].Id, AlertSeverity.Warning, 
                "Delivery rate has dropped below 90%", new Dictionary<string, object>
                {
                    ["current_rate"] = 0.85,
                    ["threshold"] = 0.90,
                    ["time_window"] = "10 minutes"
                })
        };

        foreach (var result in triggerResults)
        {
            _logger.LogInformation("Alert trigger result: {Success}", result);
        }

        // Resolve alerts
        var resolveResults = new[]
        {
            await _monitoringService.ResolveAlertAsync(alerts[0].Id, "Error rate has returned to normal levels"),
            await _monitoringService.ResolveAlertAsync(alerts[1].Id, "Delivery rate has improved")
        };

        foreach (var result in resolveResults)
        {
            _logger.LogInformation("Alert resolve result: {Success}", result);
        }

        // Get alert statistics
        var alertStats = await _monitoringService.GetAlertStatisticsAsync("test-tenant", TimeSpan.FromDays(7));
        _logger.LogInformation("Alert statistics: Total={Total}, Active={Active}, Triggered={Triggered}, Resolved={Resolved}",
            alertStats.TotalAlerts, alertStats.ActiveAlerts, alertStats.TriggeredAlerts, alertStats.ResolvedAlerts);
        _logger.LogInformation("Average resolution time: {ResolutionTime}", alertStats.AverageResolutionTime);
    }

    private async Task DemonstrateDeploymentServiceAsync()
    {
        _logger.LogInformation("=== Deployment Service Demonstration ===");

        // Get available regions
        var regions = await _deploymentService.GetAvailableRegionsAsync();
        _logger.LogInformation("Available regions: {Count}", regions.Count());
        
        foreach (var region in regions.Take(3))
        {
            _logger.LogInformation("  Region: {DisplayName} ({Id}) - {Location}", 
                region.DisplayName, region.Id, region.Location);
        }

        // Deploy to multiple regions
        var deploymentConfig = new DeploymentConfiguration
        {
            Name = "notifyx-api",
            Version = "1.2.3",
            Image = "notifyx/api:1.2.3",
            InstanceCount = 3,
            EnvironmentVariables = new Dictionary<string, string>
            {
                ["ENVIRONMENT"] = "production",
                ["LOG_LEVEL"] = "info",
                ["DATABASE_URL"] = "postgresql://..."
            },
            Resources = new Dictionary<string, object>
            {
                ["cpu"] = "1000m",
                ["memory"] = "2Gi",
                ["storage"] = "10Gi"
            }
        };

        var deploymentTasks = new[]
        {
            _deploymentService.DeployToRegionAsync("us-east-1", deploymentConfig),
            _deploymentService.DeployToRegionAsync("eu-west-1", deploymentConfig),
            _deploymentService.DeployToRegionAsync("ap-southeast-1", deploymentConfig)
        };

        var deploymentResults = await Task.WhenAll(deploymentTasks);
        
        foreach (var result in deploymentResults)
        {
            _logger.LogInformation("Deployment to {Region}: {Success} - {DeploymentId}", 
                result.Region, result.IsSuccess, result.DeploymentId);
        }

        // Check deployment status
        foreach (var result in deploymentResults.Where(r => r.IsSuccess))
        {
            var status = await _deploymentService.GetDeploymentStatusAsync(result.DeploymentId);
            _logger.LogInformation("Deployment {DeploymentId} status: {State} - {RunningInstances}/{InstanceCount} instances running",
                status.DeploymentId, status.State, status.RunningInstances, status.InstanceCount);
        }

        // Scale deployments
        var scaleTasks = deploymentResults
            .Where(r => r.IsSuccess)
            .Select(r => _deploymentService.ScaleDeploymentAsync(r.DeploymentId, 5));

        var scaleResults = await Task.WhenAll(scaleTasks);
        
        foreach (var result in scaleResults)
        {
            _logger.LogInformation("Scale deployment result: {Success}", result);
        }

        // Rollback a deployment
        if (deploymentResults.Any(r => r.IsSuccess))
        {
            var firstDeployment = deploymentResults.First(r => r.IsSuccess);
            var rollbackResult = await _deploymentService.RollbackDeploymentAsync(firstDeployment.DeploymentId, "1.2.2");
            _logger.LogInformation("Rollback deployment {DeploymentId} result: {Success}", 
                firstDeployment.DeploymentId, rollbackResult);
        }

        // Check final status
        foreach (var result in deploymentResults.Where(r => r.IsSuccess))
        {
            var finalStatus = await _deploymentService.GetDeploymentStatusAsync(result.DeploymentId);
            _logger.LogInformation("Final deployment {DeploymentId} status: {State} - Version: {Version}",
                finalStatus.DeploymentId, finalStatus.State, finalStatus.Version);
        }
    }
}