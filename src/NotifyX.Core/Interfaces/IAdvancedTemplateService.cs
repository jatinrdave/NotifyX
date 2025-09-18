using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Interface for advanced template service operations.
/// </summary>
public interface IAdvancedTemplateService
{
    /// <summary>
    /// Creates a rich template with advanced formatting.
    /// </summary>
    Task<NotificationTemplate> CreateRichTemplateAsync(RichTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a template with timezone-aware content.
    /// </summary>
    Task<TemplateRenderResult> RenderWithTimezoneAsync(NotificationEvent notification, string templateId, string timezone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a template with conditional content.
    /// </summary>
    Task<TemplateRenderResult> RenderWithConditionsAsync(NotificationEvent notification, string templateId, Dictionary<string, object> conditions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a template with dynamic content blocks.
    /// </summary>
    Task<TemplateRenderResult> RenderWithDynamicBlocksAsync(NotificationEvent notification, string templateId, Dictionary<string, object> dynamicData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates template syntax and structure.
    /// </summary>
    Task<TemplateValidationResult> ValidateTemplateAsync(string templateContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets template preview with sample data.
    /// </summary>
    Task<TemplatePreviewResult> GetTemplatePreviewAsync(string templateId, Dictionary<string, object> sampleData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clones a template with modifications.
    /// </summary>
    Task<NotificationTemplate> CloneTemplateAsync(string templateId, string newName, Dictionary<string, object> modifications, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets template usage statistics.
    /// </summary>
    Task<TemplateUsageStatistics> GetTemplateUsageStatisticsAsync(string templateId, TimeSpan timeRange, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for timezone scheduling service.
/// </summary>
public interface ITimezoneSchedulingService
{
    /// <summary>
    /// Schedules a notification for a specific timezone.
    /// </summary>
    Task<bool> ScheduleForTimezoneAsync(NotificationEvent notification, string timezone, DateTime localTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules a notification for multiple timezones.
    /// </summary>
    Task<bool> ScheduleForMultipleTimezonesAsync(NotificationEvent notification, Dictionary<string, DateTime> timezoneSchedules, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets optimal delivery times for a recipient's timezone.
    /// </summary>
    Task<IEnumerable<DateTime>> GetOptimalDeliveryTimesAsync(string timezone, NotificationPriority priority, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts UTC time to recipient's local time.
    /// </summary>
    Task<DateTime> ConvertToLocalTimeAsync(DateTime utcTime, string timezone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts local time to UTC for scheduling.
    /// </summary>
    Task<DateTime> ConvertToUtcAsync(DateTime localTime, string timezone, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets timezone information for a recipient.
    /// </summary>
    Task<TimezoneInfo> GetTimezoneInfoAsync(string timezone, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for channel failover service.
/// </summary>
public interface IChannelFailoverService
{
    /// <summary>
    /// Configures failover rules for a tenant.
    /// </summary>
    Task<bool> ConfigureFailoverRulesAsync(string tenantId, List<FailoverRule> rules, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failover options for a notification.
    /// </summary>
    Task<List<NotificationChannel>> GetFailoverChannelsAsync(NotificationEvent notification, NotificationChannel primaryChannel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes failover for a failed delivery.
    /// </summary>
    Task<FailoverResult> ExecuteFailoverAsync(NotificationEvent notification, NotificationChannel failedChannel, string failureReason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets failover statistics for a tenant.
    /// </summary>
    Task<FailoverStatistics> GetFailoverStatisticsAsync(string tenantId, TimeSpan timeRange, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests failover configuration.
    /// </summary>
    Task<FailoverTestResult> TestFailoverConfigurationAsync(string tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for CLI service operations.
/// </summary>
public interface ICLIService
{
    /// <summary>
    /// Executes a CLI command.
    /// </summary>
    Task<CLICommandResult> ExecuteCommandAsync(string command, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available CLI commands.
    /// </summary>
    Task<IEnumerable<CLICommand>> GetAvailableCommandsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets command help information.
    /// </summary>
    Task<CLICommandHelp> GetCommandHelpAsync(string command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates CLI command syntax.
    /// </summary>
    Task<CLIValidationResult> ValidateCommandAsync(string command, CancellationToken cancellationToken = default);
}