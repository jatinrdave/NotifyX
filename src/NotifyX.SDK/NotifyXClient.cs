using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Text.Json;

namespace NotifyX.SDK;

/// <summary>
/// Client for interacting with the NotifyX notification platform.
/// Provides a high-level API for sending notifications, managing rules, and templates.
/// </summary>
public sealed class NotifyXClient
{
    private readonly ILogger<NotifyXClient> _logger;
    private readonly NotifyXClientOptions _options;
    private readonly INotificationService _notificationService;
    private readonly IRuleEngine _ruleEngine;
    private readonly ITemplateService _templateService;

    /// <summary>
    /// Initializes a new instance of the NotifyXClient class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The client options.</param>
    /// <param name="notificationService">The notification service.</param>
    /// <param name="ruleEngine">The rule engine.</param>
    /// <param name="templateService">The template service.</param>
    public NotifyXClient(
        ILogger<NotifyXClient> logger,
        IOptions<NotifyXClientOptions> options,
        INotificationService notificationService,
        IRuleEngine ruleEngine,
        ITemplateService templateService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
    }

    /// <summary>
    /// Sends a notification using the specified builder.
    /// </summary>
    /// <param name="builder">The notification builder.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task<NotificationResult> SendAsync(Action<NotificationEventBuilder> builder, CancellationToken cancellationToken = default)
    {
        try
        {
            var notificationBuilder = new NotificationEventBuilder(new NotificationEvent
            {
                TenantId = _options.DefaultTenantId
            });

            builder(notificationBuilder);
            var notification = notificationBuilder.Build();

            _logger.LogDebug("Sending notification {NotificationId} via NotifyX client", notification.Id);
            return await _notificationService.SendAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Sends a notification directly.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task<NotificationResult> SendAsync(NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Sending notification {NotificationId} via NotifyX client", notification.Id);
            return await _notificationService.SendAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Sends multiple notifications in batch.
    /// </summary>
    /// <param name="notifications">The notifications to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous batch send operation.</returns>
    public async Task<BatchNotificationResult> SendBatchAsync(IEnumerable<NotificationEvent> notifications, CancellationToken cancellationToken = default)
    {
        try
        {
            var notificationsList = notifications.ToList();
            _logger.LogDebug("Sending batch of {Count} notifications via NotifyX client", notificationsList.Count);
            return await _notificationService.SendBatchAsync(notificationsList, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch notifications via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Schedules a notification for future delivery.
    /// </summary>
    /// <param name="notification">The notification to schedule.</param>
    /// <param name="scheduledFor">When to deliver the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous schedule operation.</returns>
    public async Task<NotificationResult> ScheduleAsync(NotificationEvent notification, DateTime scheduledFor, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Scheduling notification {NotificationId} for {ScheduledFor} via NotifyX client", 
                notification.Id, scheduledFor);
            return await _notificationService.ScheduleAsync(notification, scheduledFor, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling notification via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Cancels a scheduled notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous cancel operation.</returns>
    public async Task<bool> CancelAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Cancelling notification {NotificationId} via NotifyX client", notificationId);
            return await _notificationService.CancelAsync(notificationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling notification via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Gets the status of a notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous status check operation.</returns>
    public async Task<NotificationStatus?> GetStatusAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting status for notification {NotificationId} via NotifyX client", notificationId);
            return await _notificationService.GetStatusAsync(notificationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification status via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Gets the delivery history for a notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous history retrieval operation.</returns>
    public async Task<IEnumerable<DeliveryAttempt>> GetDeliveryHistoryAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting delivery history for notification {NotificationId} via NotifyX client", notificationId);
            return await _notificationService.GetDeliveryHistoryAsync(notificationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery history via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Retries a failed notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to retry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous retry operation.</returns>
    public async Task<NotificationResult> RetryAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrying notification {NotificationId} via NotifyX client", notificationId);
            return await _notificationService.RetryAsync(notificationId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying notification via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Acknowledges receipt of a notification.
    /// </summary>
    /// <param name="notificationId">The ID of the notification.</param>
    /// <param name="acknowledgedBy">Who acknowledged the notification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous acknowledgment operation.</returns>
    public async Task<bool> AcknowledgeAsync(string notificationId, string acknowledgedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Acknowledging notification {NotificationId} by {AcknowledgedBy} via NotifyX client", 
                notificationId, acknowledgedBy);
            return await _notificationService.AcknowledgeAsync(notificationId, acknowledgedBy, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging notification via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Adds a notification rule.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule addition operation.</returns>
    public async Task<bool> AddRuleAsync(NotificationRule rule, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Adding rule {RuleId} via NotifyX client", rule.Id);
            return await _ruleEngine.AddRuleAsync(rule, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding rule via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Updates a notification rule.
    /// </summary>
    /// <param name="rule">The updated rule.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule update operation.</returns>
    public async Task<bool> UpdateRuleAsync(NotificationRule rule, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating rule {RuleId} via NotifyX client", rule.Id);
            return await _ruleEngine.UpdateRuleAsync(rule, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rule via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Removes a notification rule.
    /// </summary>
    /// <param name="ruleId">The ID of the rule to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule removal operation.</returns>
    public async Task<bool> RemoveRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Removing rule {RuleId} via NotifyX client", ruleId);
            return await _ruleEngine.RemoveRuleAsync(ruleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing rule via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Gets all rules for the default tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule retrieval operation.</returns>
    public async Task<IEnumerable<NotificationRule>> GetRulesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting rules for tenant {TenantId} via NotifyX client", _options.DefaultTenantId);
            return await _ruleEngine.GetRulesAsync(_options.DefaultTenantId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rules via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Gets all rules for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule retrieval operation.</returns>
    public async Task<IEnumerable<NotificationRule>> GetRulesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting rules for tenant {TenantId} via NotifyX client", tenantId);
            return await _ruleEngine.GetRulesAsync(tenantId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rules via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Gets a specific rule by ID.
    /// </summary>
    /// <param name="ruleId">The rule ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous rule retrieval operation.</returns>
    public async Task<NotificationRule?> GetRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting rule {RuleId} via NotifyX client", ruleId);
            return await _ruleEngine.GetRuleAsync(ruleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rule via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Adds a notification template.
    /// </summary>
    /// <param name="template">The template to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template addition operation.</returns>
    public async Task<bool> AddTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Adding template {TemplateId} via NotifyX client", template.Id);
            return await _templateService.AddTemplateAsync(template, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding template via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Updates a notification template.
    /// </summary>
    /// <param name="template">The updated template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template update operation.</returns>
    public async Task<bool> UpdateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating template {TemplateId} via NotifyX client", template.Id);
            return await _templateService.UpdateTemplateAsync(template, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Removes a notification template.
    /// </summary>
    /// <param name="templateId">The template ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template removal operation.</returns>
    public async Task<bool> RemoveTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Removing template {TemplateId} via NotifyX client", templateId);
            return await _templateService.RemoveTemplateAsync(templateId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing template via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template retrieval operation.</returns>
    public async Task<NotificationTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting template {TemplateId} via NotifyX client", templateId);
            return await _templateService.GetTemplateAsync(templateId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Gets templates for a specific channel.
    /// </summary>
    /// <param name="channel">The notification channel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template retrieval operation.</returns>
    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(NotificationChannel channel, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting templates for channel {Channel} and tenant {TenantId} via NotifyX client", 
                channel, _options.DefaultTenantId);
            return await _templateService.GetTemplatesAsync(_options.DefaultTenantId, channel, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Gets templates for a specific channel and event type.
    /// </summary>
    /// <param name="channel">The notification channel.</param>
    /// <param name="eventType">The event type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template retrieval operation.</returns>
    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(
        NotificationChannel channel, 
        string eventType, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting templates for channel {Channel}, event type {EventType}, and tenant {TenantId} via NotifyX client", 
                channel, eventType, _options.DefaultTenantId);
            return await _templateService.GetTemplatesAsync(_options.DefaultTenantId, channel, eventType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates via NotifyX client");
            throw;
        }
    }

    /// <summary>
    /// Creates a notification builder for fluent API usage.
    /// </summary>
    /// <returns>A notification builder.</returns>
    public NotificationEventBuilder CreateNotification()
    {
        return new NotificationEventBuilder(new NotificationEvent
        {
            TenantId = _options.DefaultTenantId
        });
    }

    /// <summary>
    /// Creates a notification builder for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>A notification builder.</returns>
    public NotificationEventBuilder CreateNotification(string tenantId)
    {
        return new NotificationEventBuilder(new NotificationEvent
        {
            TenantId = tenantId
        });
    }

    /// <summary>
    /// Creates a rule builder for fluent API usage.
    /// </summary>
    /// <returns>A rule builder.</returns>
    public NotificationRuleBuilder CreateRule()
    {
        return new NotificationRuleBuilder(new NotificationRule
        {
            TenantId = _options.DefaultTenantId
        });
    }

    /// <summary>
    /// Creates a rule builder for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>A rule builder.</returns>
    public NotificationRuleBuilder CreateRule(string tenantId)
    {
        return new NotificationRuleBuilder(new NotificationRule
        {
            TenantId = tenantId
        });
    }

    /// <summary>
    /// Creates a template builder for fluent API usage.
    /// </summary>
    /// <returns>A template builder.</returns>
    public NotificationTemplateBuilder CreateTemplate()
    {
        return new NotificationTemplateBuilder(new NotificationTemplate
        {
            TenantId = _options.DefaultTenantId
        });
    }

    /// <summary>
    /// Creates a template builder for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>A template builder.</returns>
    public NotificationTemplateBuilder CreateTemplate(string tenantId)
    {
        return new NotificationTemplateBuilder(new NotificationTemplate
        {
            TenantId = tenantId
        });
    }
}

/// <summary>
/// Configuration options for the NotifyX client.
/// </summary>
public sealed class NotifyXClientOptions
{
    /// <summary>
    /// Default tenant ID to use for operations.
    /// </summary>
    public string DefaultTenantId { get; set; } = "default";

    /// <summary>
    /// API endpoint URL (for remote client usage).
    /// </summary>
    public string? ApiEndpoint { get; set; }

    /// <summary>
    /// API key for authentication (for remote client usage).
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Timeout for API requests.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether to enable retry logic.
    /// </summary>
    public bool EnableRetry { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry delay between attempts.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}