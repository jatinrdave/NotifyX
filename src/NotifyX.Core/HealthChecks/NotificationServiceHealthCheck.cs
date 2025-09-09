using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Core.HealthChecks;

/// <summary>
/// Health check for the notification service.
/// </summary>
public sealed class NotificationServiceHealthCheck : IHealthCheck
{
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the NotificationServiceHealthCheck class.
    /// </summary>
    /// <param name="notificationService">The notification service.</param>
    public NotificationServiceHealthCheck(INotificationService notificationService)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform a simple health check by getting the status of a non-existent notification
            // This tests that the service is responsive
            var status = await _notificationService.GetStatusAsync("health-check-test", cancellationToken);
            
            // If we get here without an exception, the service is healthy
            return HealthCheckResult.Healthy("Notification service is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Notification service is unhealthy", ex);
        }
    }
}

/// <summary>
/// Health check for the rule engine.
/// </summary>
public sealed class RuleEngineHealthCheck : IHealthCheck
{
    private readonly IRuleEngine _ruleEngine;

    /// <summary>
    /// Initializes a new instance of the RuleEngineHealthCheck class.
    /// </summary>
    /// <param name="ruleEngine">The rule engine.</param>
    public RuleEngineHealthCheck(IRuleEngine ruleEngine)
    {
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform a simple health check by getting rules for a test tenant
            var rules = await _ruleEngine.GetRulesAsync("health-check-test", cancellationToken);
            
            // If we get here without an exception, the rule engine is healthy
            return HealthCheckResult.Healthy("Rule engine is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Rule engine is unhealthy", ex);
        }
    }
}

/// <summary>
/// Health check for the template service.
/// </summary>
public sealed class TemplateServiceHealthCheck : IHealthCheck
{
    private readonly ITemplateService _templateService;

    /// <summary>
    /// Initializes a new instance of the TemplateServiceHealthCheck class.
    /// </summary>
    /// <param name="templateService">The template service.</param>
    public TemplateServiceHealthCheck(ITemplateService templateService)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform a simple health check by getting templates for a test tenant
            var templates = await _templateService.GetTemplatesAsync("health-check-test", NotificationChannel.Email, cancellationToken);
            
            // If we get here without an exception, the template service is healthy
            return HealthCheckResult.Healthy("Template service is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Template service is unhealthy", ex);
        }
    }
}