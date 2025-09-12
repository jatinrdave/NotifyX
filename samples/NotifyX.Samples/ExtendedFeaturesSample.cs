using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Samples;

/// <summary>
/// Sample demonstrating extended features functionality.
/// </summary>
public class ExtendedFeaturesSample
{
    private readonly ILogger<ExtendedFeaturesSample> _logger;
    private readonly IAdvancedTemplateService _advancedTemplateService;
    private readonly ITimezoneSchedulingService _timezoneSchedulingService;
    private readonly IChannelFailoverService _channelFailoverService;
    private readonly ICLIService _cliService;

    public ExtendedFeaturesSample(
        ILogger<ExtendedFeaturesSample> logger,
        IAdvancedTemplateService advancedTemplateService,
        ITimezoneSchedulingService timezoneSchedulingService,
        IChannelFailoverService channelFailoverService,
        ICLIService cliService)
    {
        _logger = logger;
        _advancedTemplateService = advancedTemplateService;
        _timezoneSchedulingService = timezoneSchedulingService;
        _channelFailoverService = channelFailoverService;
        _cliService = cliService;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Extended Features Sample");

        try
        {
            // Demonstrate advanced templates
            await DemonstrateAdvancedTemplatesAsync();

            // Demonstrate timezone scheduling
            await DemonstrateTimezoneSchedulingAsync();

            // Demonstrate channel failover
            await DemonstrateChannelFailoverAsync();

            // Demonstrate CLI service
            await DemonstrateCLIServiceAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Extended Features Sample");
        }
    }

    private async Task DemonstrateAdvancedTemplatesAsync()
    {
        _logger.LogInformation("=== Advanced Templates Demonstration ===");

        // Create a rich template
        var richTemplateRequest = new RichTemplateRequest
        {
            Name = "Welcome Email Template",
            Description = "Rich welcome email with conditional content and styling",
            Channel = NotificationChannel.Email,
            Subject = "Welcome to {{company_name}}, {{user_name}}!",
            Content = @"
                <div style='font-family: Arial, sans-serif; color: #333;'>
                    <h1 style='color: {{primary_color}};'>Welcome {{user_name}}!</h1>
                    <p>Thank you for joining {{company_name}}.</p>
                    
                    {% if user_type == 'premium' %}
                        <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid {{primary_color}};'>
                            <h3>Premium Benefits</h3>
                            <ul>
                                <li>Priority support</li>
                                <li>Advanced features</li>
                                <li>24/7 assistance</li>
                            </ul>
                        </div>
                    {% endif %}
                    
                    <p>Get started by visiting your dashboard: <a href='{{dashboard_url}}'>Dashboard</a></p>
                    
                    {% if show_promotion %}
                        <div style='background-color: #e3f2fd; padding: 10px; margin: 20px 0;'>
                            <strong>Special Offer:</strong> {{promotion_text}}
                        </div>
                    {% endif %}
                    
                    <p>Best regards,<br>The {{company_name}} Team</p>
                </div>
            ",
            Variables = new Dictionary<string, object>
            {
                ["company_name"] = "NotifyX",
                ["primary_color"] = "#007bff",
                ["dashboard_url"] = "https://app.notifyx.com/dashboard"
            },
            Style = new TemplateStyle
            {
                Theme = "modern",
                PrimaryColor = "#007bff",
                FontFamily = "Arial, sans-serif",
                FontSize = 14
            }
        };

        var richTemplate = await _advancedTemplateService.CreateRichTemplateAsync(richTemplateRequest);
        _logger.LogInformation("Created rich template: {TemplateId} - {TemplateName}", richTemplate.Id, richTemplate.Name);

        // Validate template
        var validationResult = await _advancedTemplateService.ValidateTemplateAsync(richTemplate.Content);
        _logger.LogInformation("Template validation: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}", 
            validationResult.IsValid, validationResult.Errors.Count, validationResult.Warnings.Count);

        // Get template preview
        var sampleData = new Dictionary<string, object>
        {
            ["user_name"] = "John Doe",
            ["user_type"] = "premium",
            ["show_promotion"] = true,
            ["promotion_text"] = "Get 20% off your first month!"
        };

        var previewResult = await _advancedTemplateService.GetTemplatePreviewAsync(richTemplate.Id, sampleData);
        if (previewResult.IsSuccess)
        {
            _logger.LogInformation("Template preview generated successfully");
            _logger.LogInformation("Rendered Subject: {Subject}", previewResult.RenderedSubject);
            _logger.LogInformation("Used Variables: {Count}", previewResult.UsedVariables.Count);
            _logger.LogInformation("Missing Variables: {Count}", previewResult.MissingVariables.Count);
        }

        // Render with timezone
        var notification = CreateTestNotification("template-test", "user.welcome");
        var timezoneResult = await _advancedTemplateService.RenderWithTimezoneAsync(notification, richTemplate.Id, "America/New_York");
        if (timezoneResult.IsSuccess)
        {
            _logger.LogInformation("Template rendered with timezone successfully");
        }

        // Render with conditions
        var conditions = new Dictionary<string, object>
        {
            ["user_type"] = "premium",
            ["show_promotion"] = true
        };

        var conditionalResult = await _advancedTemplateService.RenderWithConditionsAsync(notification, richTemplate.Id, conditions);
        if (conditionalResult.IsSuccess)
        {
            _logger.LogInformation("Template rendered with conditions successfully");
        }

        // Clone template
        var modifications = new Dictionary<string, object>
        {
            ["subject"] = "Welcome to Premium, {{user_name}}!",
            ["description"] = "Premium welcome email template"
        };

        var clonedTemplate = await _advancedTemplateService.CloneTemplateAsync(richTemplate.Id, "Premium Welcome Template", modifications);
        _logger.LogInformation("Cloned template: {ClonedTemplateId} - {ClonedTemplateName}", clonedTemplate.Id, clonedTemplate.Name);

        // Get usage statistics
        var usageStats = await _advancedTemplateService.GetTemplateUsageStatisticsAsync(richTemplate.Id, TimeSpan.FromDays(30));
        _logger.LogInformation("Template usage statistics: Total: {Total}, Success Rate: {SuccessRate:P2}", 
            usageStats.TotalUsage, usageStats.SuccessRate);
    }

    private async Task DemonstrateTimezoneSchedulingAsync()
    {
        _logger.LogInformation("=== Timezone Scheduling Demonstration ===");

        var notification = CreateTestNotification("timezone-test", "user.reminder");

        // Schedule for specific timezone
        var localTime = new DateTime(2024, 12, 25, 9, 0, 0); // 9 AM local time
        var scheduled = await _timezoneSchedulingService.ScheduleForTimezoneAsync(notification, "America/New_York", localTime);
        _logger.LogInformation("Scheduled notification for timezone: {Success}", scheduled);

        // Schedule for multiple timezones
        var timezoneSchedules = new Dictionary<string, DateTime>
        {
            ["America/New_York"] = new DateTime(2024, 12, 25, 9, 0, 0),
            ["Europe/London"] = new DateTime(2024, 12, 25, 14, 0, 0),
            ["Asia/Tokyo"] = new DateTime(2024, 12, 25, 23, 0, 0)
        };

        var multiScheduled = await _timezoneSchedulingService.ScheduleForMultipleTimezonesAsync(notification, timezoneSchedules);
        _logger.LogInformation("Scheduled notification for multiple timezones: {Success}", multiScheduled);

        // Get optimal delivery times
        var optimalTimes = await _timezoneSchedulingService.GetOptimalDeliveryTimesAsync("America/New_York", NotificationPriority.Normal);
        _logger.LogInformation("Optimal delivery times for New York: {Count} times", optimalTimes.Count());
        
        foreach (var time in optimalTimes.Take(3))
        {
            _logger.LogInformation("  Optimal time: {Time}", time);
        }

        // Convert times
        var utcTime = DateTime.UtcNow;
        var localTimeConverted = await _timezoneSchedulingService.ConvertToLocalTimeAsync(utcTime, "America/New_York");
        var utcTimeConverted = await _timezoneSchedulingService.ConvertToUtcAsync(localTimeConverted, "America/New_York");
        
        _logger.LogInformation("Time conversion: UTC {UtcTime} -> Local {LocalTime} -> UTC {UtcTimeConverted}", 
            utcTime, localTimeConverted, utcTimeConverted);

        // Get timezone info
        var timezoneInfo = await _timezoneSchedulingService.GetTimezoneInfoAsync("America/New_York");
        _logger.LogInformation("Timezone info: {DisplayName}, Offset: {Offset}, Supports DST: {SupportsDST}", 
            timezoneInfo.DisplayName, timezoneInfo.Offset, timezoneInfo.SupportsDaylightSaving);
    }

    private async Task DemonstrateChannelFailoverAsync()
    {
        _logger.LogInformation("=== Channel Failover Demonstration ===");

        // Configure failover rules
        var failoverRules = new List<FailoverRule>
        {
            new FailoverRule
            {
                Name = "Email to SMS Failover",
                Description = "Failover from email to SMS for critical notifications",
                PrimaryChannel = NotificationChannel.Email,
                FailoverChannels = new List<NotificationChannel> { NotificationChannel.SMS, NotificationChannel.Push },
                FailureReasons = new List<string> { "ProviderUnavailable", "RateLimitExceeded" },
                Delay = TimeSpan.FromMinutes(5),
                MaxRetries = 3,
                Conditions = new Dictionary<string, object>
                {
                    ["priority"] = "Critical"
                }
            },
            new FailoverRule
            {
                Name = "SMS to Push Failover",
                Description = "Failover from SMS to push notifications",
                PrimaryChannel = NotificationChannel.SMS,
                FailoverChannels = new List<NotificationChannel> { NotificationChannel.Push },
                FailureReasons = new List<string> { "InvalidCredentials", "NetworkTimeout" },
                Delay = TimeSpan.FromMinutes(2),
                MaxRetries = 2
            }
        };

        var configured = await _channelFailoverService.ConfigureFailoverRulesAsync("test-tenant", failoverRules);
        _logger.LogInformation("Configured failover rules: {Success}", configured);

        // Test failover configuration
        var testResult = await _channelFailoverService.TestFailoverConfigurationAsync("test-tenant");
        _logger.LogInformation("Failover configuration test: {Success} - {Summary}", testResult.IsSuccess, testResult.Summary);
        
        foreach (var recommendation in testResult.Recommendations)
        {
            _logger.LogInformation("  Recommendation: {Recommendation}", recommendation);
        }

        // Get failover channels
        var notification = CreateTestNotification("failover-test", "system.alert");
        var failoverChannels = await _channelFailoverService.GetFailoverChannelsAsync(notification, NotificationChannel.Email);
        _logger.LogInformation("Failover channels for email: {Channels}", string.Join(", ", failoverChannels));

        // Execute failover
        var failoverResult = await _channelFailoverService.ExecuteFailoverAsync(notification, NotificationChannel.Email, "ProviderUnavailable");
        _logger.LogInformation("Failover execution: {Success}, Used Channel: {Channel}, Message: {Message}", 
            failoverResult.IsSuccess, failoverResult.UsedChannel, failoverResult.Message);

        // Get failover statistics
        var statistics = await _channelFailoverService.GetFailoverStatisticsAsync("test-tenant", TimeSpan.FromDays(7));
        _logger.LogInformation("Failover statistics: Total: {Total}, Success Rate: {SuccessRate:P2}, Average Delay: {AverageDelay}", 
            statistics.TotalFailovers, statistics.SuccessRate, statistics.AverageDelay);
    }

    private async Task DemonstrateCLIServiceAsync()
    {
        _logger.LogInformation("=== CLI Service Demonstration ===");

        // Get available commands
        var commands = await _cliService.GetAvailableCommandsAsync();
        _logger.LogInformation("Available CLI commands: {Count}", commands.Count());
        
        foreach (var command in commands.Take(5))
        {
            _logger.LogInformation("  Command: {Name} - {Description}", command.Name, command.Description);
        }

        // Get command help
        var help = await _cliService.GetCommandHelpAsync("send");
        _logger.LogInformation("Help for 'send' command: {Description}", help.Description);
        _logger.LogInformation("Syntax: {Syntax}", help.Syntax);
        _logger.LogInformation("Examples: {Count}", help.Examples.Count);

        // Validate commands
        var validCommands = new[]
        {
            "send --tenant tenant123 --event user.login --subject \"Welcome\" --content \"Welcome!\" --recipients user@example.com",
            "status --id notification-123",
            "list --tenant tenant123 --limit 10",
            "health --detailed"
        };

        foreach (var command in validCommands)
        {
            var validation = await _cliService.ValidateCommandAsync(command);
            _logger.LogInformation("Command validation '{Command}': {IsValid}", 
                command.Substring(0, Math.Min(30, command.Length)) + "...", validation.IsValid);
        }

        // Execute commands
        var commandExecutions = new[]
        {
            ("send", new Dictionary<string, object>
            {
                ["tenant"] = "test-tenant",
                ["event"] = "user.welcome",
                ["subject"] = "Welcome!",
                ["content"] = "Welcome to our platform!",
                ["recipients"] = "user@example.com",
                ["priority"] = "Normal"
            }),
            ("status", new Dictionary<string, object>
            {
                ["id"] = "test-notification-123"
            }),
            ("health", new Dictionary<string, object>
            {
                ["detailed"] = true
            }),
            ("stats", new Dictionary<string, object>
            {
                ["tenant"] = "test-tenant",
                ["period"] = "24h"
            })
        };

        foreach (var (commandName, parameters) in commandExecutions)
        {
            _logger.LogInformation("Executing CLI command: {Command}", commandName);
            
            var result = await _cliService.ExecuteCommandAsync(commandName, parameters);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("  Command executed successfully in {ExecutionTime}ms", result.ExecutionTime.TotalMilliseconds);
                _logger.LogInformation("  Output: {Output}", result.Output);
            }
            else
            {
                _logger.LogWarning("  Command failed: {ErrorMessage}", result.ErrorMessage);
            }
        }
    }

    private static NotificationEvent CreateTestNotification(string id, string eventType)
    {
        return new NotificationEvent
        {
            Id = id,
            TenantId = "test-tenant",
            EventType = eventType,
            Priority = NotificationPriority.Normal,
            Subject = $"Test Notification - {eventType}",
            Content = $"This is a test notification for {eventType}",
            Recipients = new List<NotificationRecipient>
            {
                new NotificationRecipient
                {
                    Id = "test-recipient",
                    Name = "Test User",
                    Email = "test@example.com",
                    TimeZone = "America/New_York"
                }
            },
            PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email },
            Metadata = new Dictionary<string, object>
            {
                ["source"] = "extended-features-sample",
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }
}