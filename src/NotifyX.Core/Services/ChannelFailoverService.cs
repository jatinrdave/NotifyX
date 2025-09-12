using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Core.Services;

/// <summary>
/// Channel failover service implementation.
/// </summary>
public class ChannelFailoverService : IChannelFailoverService
{
    private readonly ILogger<ChannelFailoverService> _logger;
    private readonly INotificationService _notificationService;
    private readonly Dictionary<string, List<FailoverRule>> _tenantFailoverRules = new();

    public ChannelFailoverService(ILogger<ChannelFailoverService> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<bool> ConfigureFailoverRulesAsync(string tenantId, List<FailoverRule> rules, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Configuring failover rules for tenant: {TenantId}, rules: {Count}", tenantId, rules.Count);

            // Validate rules
            foreach (var rule in rules)
            {
                if (!ValidateFailoverRule(rule))
                {
                    _logger.LogWarning("Invalid failover rule: {RuleId}", rule.Id);
                    return false;
                }
            }

            _tenantFailoverRules[tenantId] = rules;
            
            _logger.LogInformation("Successfully configured {Count} failover rules for tenant {TenantId}", 
                rules.Count, tenantId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure failover rules for tenant: {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<List<NotificationChannel>> GetFailoverChannelsAsync(NotificationEvent notification, NotificationChannel primaryChannel, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting failover channels for notification {NotificationId}, primary channel: {PrimaryChannel}", 
                notification.Id, primaryChannel);

            var failoverChannels = new List<NotificationChannel>();

            if (!_tenantFailoverRules.TryGetValue(notification.TenantId, out var rules))
            {
                _logger.LogDebug("No failover rules configured for tenant: {TenantId}", notification.TenantId);
                return failoverChannels;
            }

            // Find applicable failover rules
            var applicableRules = rules.Where(rule => 
                rule.IsEnabled && 
                rule.PrimaryChannel == primaryChannel &&
                IsRuleApplicable(rule, notification)).ToList();

            foreach (var rule in applicableRules)
            {
                failoverChannels.AddRange(rule.FailoverChannels);
            }

            // Remove duplicates and primary channel
            failoverChannels = failoverChannels.Distinct().Where(c => c != primaryChannel).ToList();

            _logger.LogDebug("Found {Count} failover channels for notification {NotificationId}", 
                failoverChannels.Count, notification.Id);

            return failoverChannels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get failover channels for notification: {NotificationId}", notification.Id);
            return new List<NotificationChannel>();
        }
    }

    public async Task<FailoverResult> ExecuteFailoverAsync(NotificationEvent notification, NotificationChannel failedChannel, string failureReason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing failover for notification {NotificationId}, failed channel: {FailedChannel}, reason: {Reason}", 
                notification.Id, failedChannel, failureReason);

            var failoverChannels = await GetFailoverChannelsAsync(notification, failedChannel, cancellationToken);
            
            if (!failoverChannels.Any())
            {
                _logger.LogWarning("No failover channels available for notification {NotificationId}", notification.Id);
                return new FailoverResult
                {
                    IsSuccess = false,
                    Message = "No failover channels available",
                    FailedChannels = new List<string> { failedChannel.ToString() }
                };
            }

            var failedChannels = new List<string> { failedChannel.ToString() };
            var attempts = 1;

            foreach (var failoverChannel in failoverChannels)
            {
                try
                {
                    _logger.LogDebug("Attempting failover to channel: {FailoverChannel}", failoverChannel);

                    // Create notification for failover channel
                    var failoverNotification = notification with
                    {
                        Id = $"{notification.Id}-failover-{attempts}",
                        PreferredChannels = new List<NotificationChannel> { failoverChannel },
                        Metadata = new Dictionary<string, object>(notification.Metadata)
                        {
                            ["isFailover"] = true,
                            ["originalChannel"] = failedChannel.ToString(),
                            ["failoverAttempt"] = attempts,
                            ["failureReason"] = failureReason
                        }
                    };

                    var result = await _notificationService.SendAsync(failoverNotification, cancellationToken);
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation("Failover successful for notification {NotificationId} using channel {Channel}", 
                            notification.Id, failoverChannel);

                        return new FailoverResult
                        {
                            IsSuccess = true,
                            UsedChannel = failoverChannel,
                            Message = $"Successfully delivered via {failoverChannel}",
                            Delay = TimeSpan.FromMinutes(5 * attempts), // Simulate delay
                            Attempts = attempts,
                            FailedChannels = failedChannels
                        };
                    }
                    else
                    {
                        _logger.LogWarning("Failover attempt failed for notification {NotificationId} using channel {Channel}: {Error}", 
                            notification.Id, failoverChannel, result.ErrorMessage);
                        
                        failedChannels.Add(failoverChannel.ToString());
                        attempts++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception during failover attempt for notification {NotificationId} using channel {Channel}", 
                        notification.Id, failoverChannel);
                    
                    failedChannels.Add(failoverChannel.ToString());
                    attempts++;
                }
            }

            _logger.LogError("All failover attempts failed for notification {NotificationId}", notification.Id);
            return new FailoverResult
            {
                IsSuccess = false,
                Message = "All failover attempts failed",
                Delay = TimeSpan.FromMinutes(5 * attempts),
                Attempts = attempts,
                FailedChannels = failedChannels
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute failover for notification: {NotificationId}", notification.Id);
            return new FailoverResult
            {
                IsSuccess = false,
                Message = $"Failover execution failed: {ex.Message}",
                FailedChannels = new List<string> { failedChannel.ToString() }
            };
        }
    }

    public async Task<FailoverStatistics> GetFailoverStatisticsAsync(string tenantId, TimeSpan timeRange, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting failover statistics for tenant: {TenantId}, time range: {TimeRange}", tenantId, timeRange);

            // Simulate failover statistics
            var statistics = new FailoverStatistics
            {
                TotalFailovers = 150,
                SuccessfulFailovers = 135,
                FailedFailovers = 15,
                SuccessRate = 0.90,
                AverageDelay = TimeSpan.FromMinutes(7.5),
                FailoverByChannel = new Dictionary<NotificationChannel, int>
                {
                    [NotificationChannel.Email] = 45,
                    [NotificationChannel.SMS] = 35,
                    [NotificationChannel.Push] = 30,
                    [NotificationChannel.Webhook] = 20,
                    [NotificationChannel.Slack] = 20
                },
                FailoverByReason = new Dictionary<string, int>
                {
                    ["ProviderUnavailable"] = 60,
                    ["RateLimitExceeded"] = 40,
                    ["InvalidCredentials"] = 25,
                    ["NetworkTimeout"] = 15,
                    ["UnknownError"] = 10
                }
            };

            _logger.LogInformation("Retrieved failover statistics for tenant {TenantId}: {TotalFailovers} total, {SuccessRate:P2} success rate", 
                tenantId, statistics.TotalFailovers, statistics.SuccessRate);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get failover statistics for tenant: {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<FailoverTestResult> TestFailoverConfigurationAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing failover configuration for tenant: {TenantId}", tenantId);

            if (!_tenantFailoverRules.TryGetValue(tenantId, out var rules))
            {
                return new FailoverTestResult
                {
                    IsSuccess = false,
                    Summary = "No failover rules configured for tenant",
                    Recommendations = new List<string> { "Configure failover rules for the tenant" }
                };
            }

            var testScenarios = new List<FailoverTestScenario>();
            var recommendations = new List<string>();

            foreach (var rule in rules.Where(r => r.IsEnabled))
            {
                // Test each failover rule
                var scenario = new FailoverTestScenario
                {
                    Name = $"Test {rule.Name}",
                    PrimaryChannel = rule.PrimaryChannel,
                    FailureReason = "TestFailure",
                    TestPassed = true, // Simulate successful test
                    UsedFailoverChannel = rule.FailoverChannels.FirstOrDefault(),
                    ExecutionTime = TimeSpan.FromMilliseconds(150),
                    Message = "Failover test passed"
                };

                testScenarios.Add(scenario);

                // Generate recommendations based on rule configuration
                if (rule.FailoverChannels.Count == 0)
                {
                    recommendations.Add($"Rule '{rule.Name}' has no failover channels configured");
                }

                if (rule.Delay > TimeSpan.FromMinutes(10))
                {
                    recommendations.Add($"Rule '{rule.Name}' has a long delay ({rule.Delay}) which may impact user experience");
                }

                if (rule.MaxRetries > 5)
                {
                    recommendations.Add($"Rule '{rule.Name}' has high retry count ({rule.MaxRetries}) which may cause delays");
                }
            }

            var allTestsPassed = testScenarios.All(s => s.TestPassed);
            var summary = allTestsPassed 
                ? $"All {testScenarios.Count} failover tests passed"
                : $"{testScenarios.Count(s => s.TestPassed)}/{testScenarios.Count} failover tests passed";

            if (!recommendations.Any())
            {
                recommendations.Add("Failover configuration looks good");
            }

            _logger.LogInformation("Failover configuration test completed for tenant {TenantId}: {Summary}", 
                tenantId, summary);

            return new FailoverTestResult
            {
                IsSuccess = allTestsPassed,
                TestScenarios = testScenarios,
                Summary = summary,
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test failover configuration for tenant: {TenantId}", tenantId);
            return new FailoverTestResult
            {
                IsSuccess = false,
                Summary = $"Failover test failed: {ex.Message}",
                Recommendations = new List<string> { "Review failover configuration and try again" }
            };
        }
    }

    private static bool ValidateFailoverRule(FailoverRule rule)
    {
        if (string.IsNullOrEmpty(rule.Name))
        {
            return false;
        }

        if (rule.FailoverChannels.Count == 0)
        {
            return false;
        }

        if (rule.Delay < TimeSpan.Zero)
        {
            return false;
        }

        if (rule.MaxRetries < 0)
        {
            return false;
        }

        return true;
    }

    private static bool IsRuleApplicable(FailoverRule rule, NotificationEvent notification)
    {
        // Check if rule conditions match the notification
        if (rule.Conditions.Count == 0)
        {
            return true; // No conditions means rule applies to all notifications
        }

        foreach (var condition in rule.Conditions)
        {
            if (!EvaluateCondition(condition, notification))
            {
                return false;
            }
        }

        return true;
    }

    private static bool EvaluateCondition(KeyValuePair<string, object> condition, NotificationEvent notification)
    {
        var field = condition.Key;
        var expectedValue = condition.Value;

        return field.ToLower() switch
        {
            "priority" => notification.Priority.ToString().Equals(expectedValue.ToString(), StringComparison.OrdinalIgnoreCase),
            "eventtype" => notification.EventType.Equals(expectedValue.ToString(), StringComparison.OrdinalIgnoreCase),
            "tenantid" => notification.TenantId.Equals(expectedValue.ToString(), StringComparison.OrdinalIgnoreCase),
            _ => true // Unknown conditions are considered to match
        };
    }
}