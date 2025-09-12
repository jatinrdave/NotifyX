using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Samples;

/// <summary>
/// Sample demonstrating AI and MCP service functionality.
/// </summary>
public class AIMCPSample
{
    private readonly ILogger<AIMCPSample> _logger;
    private readonly IAIService _aiService;
    private readonly IMCPService _mcpService;
    private readonly IAINotificationOptimizer _aiOptimizer;

    public AIMCPSample(
        ILogger<AIMCPSample> logger,
        IAIService aiService,
        IMCPService mcpService,
        IAINotificationOptimizer aiOptimizer)
    {
        _logger = logger;
        _aiService = aiService;
        _mcpService = mcpService;
        _aiOptimizer = aiOptimizer;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting AI & MCP Sample");

        try
        {
            // Test AI service health
            await TestAIServiceHealthAsync();

            // Demonstrate natural language rule translation
            await DemonstrateNaturalLanguageRuleTranslationAsync();

            // Demonstrate AI routing recommendations
            await DemonstrateAIRoutingRecommendationsAsync();

            // Demonstrate content optimization
            await DemonstrateContentOptimizationAsync();

            // Demonstrate notification summaries
            await DemonstrateNotificationSummariesAsync();

            // Demonstrate sentiment analysis
            await DemonstrateSentimentAnalysisAsync();

            // Demonstrate template suggestions
            await DemonstrateTemplateSuggestionsAsync();

            // Test MCP service health
            await TestMCPServiceHealthAsync();

            // Demonstrate MCP tools
            await DemonstrateMCPToolsAsync();

            // Demonstrate AI notification optimization
            await DemonstrateAINotificationOptimizationAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI & MCP Sample");
        }
    }

    private async Task TestAIServiceHealthAsync()
    {
        _logger.LogInformation("=== AI Service Health Check ===");

        var healthStatus = await _aiService.GetHealthStatusAsync();
        _logger.LogInformation("AI Service Health: {Status}", healthStatus);
    }

    private async Task DemonstrateNaturalLanguageRuleTranslationAsync()
    {
        _logger.LogInformation("=== Natural Language Rule Translation ===");

        var naturalLanguageInputs = new[]
        {
            "Send an email notification when a user logs in",
            "Alert me via SMS when there's a system error",
            "Send a push notification for high priority alerts",
            "Create a rule to notify admins when users sign up"
        };

        foreach (var input in naturalLanguageInputs)
        {
            _logger.LogInformation("Translating: {Input}", input);

            var result = await _aiService.TranslateNaturalLanguageToRuleAsync(input, "test-tenant");
            
            if (result.IsSuccess && result.Rule != null)
            {
                _logger.LogInformation("Generated Rule: {RuleName} - {Description}", result.Rule.Name, result.Rule.Description);
                _logger.LogInformation("Confidence: {Confidence}", result.Confidence);
                _logger.LogInformation("Explanation: {Explanation}", result.Explanation);
                
                foreach (var suggestion in result.Suggestions)
                {
                    _logger.LogInformation("Suggestion: {Suggestion}", suggestion);
                }
            }
            else
            {
                _logger.LogWarning("Translation failed: {Explanation}", result.Explanation);
            }

            _logger.LogInformation("---");
        }
    }

    private async Task DemonstrateAIRoutingRecommendationsAsync()
    {
        _logger.LogInformation("=== AI Routing Recommendations ===");

        var notifications = new[]
        {
            CreateTestNotification("routing-1", "user.login", NotificationPriority.Normal),
            CreateTestNotification("routing-2", "system.error", NotificationPriority.Critical),
            CreateTestNotification("routing-3", "marketing.promo", NotificationPriority.Low),
            CreateTestNotification("routing-4", "security.alert", NotificationPriority.High)
        };

        foreach (var notification in notifications)
        {
            _logger.LogInformation("Getting routing recommendation for: {EventType} (Priority: {Priority})", 
                notification.EventType, notification.Priority);

            var recommendation = await _aiService.GetRoutingRecommendationAsync(notification);
            
            _logger.LogInformation("Recommended Channels: {Channels}", 
                string.Join(", ", recommendation.RecommendedChannels));
            _logger.LogInformation("Reasoning: {Reasoning}", recommendation.Reasoning);
            _logger.LogInformation("Confidence: {Confidence:P2}", recommendation.Confidence);

            foreach (var channelScore in recommendation.ChannelScores)
            {
                _logger.LogInformation("Channel {Channel}: {Score:P2}", channelScore.Key, channelScore.Value);
            }

            _logger.LogInformation("---");
        }
    }

    private async Task DemonstrateContentOptimizationAsync()
    {
        _logger.LogInformation("=== Content Optimization ===");

        var notifications = new[]
        {
            CreateTestNotification("content-1", "user.welcome", NotificationPriority.Normal, 
                "Welcome", "Thanks for signing up! Click here to get started."),
            CreateTestNotification("content-2", "system.maintenance", NotificationPriority.High,
                "Maintenance", "We will be performing maintenance on our systems tomorrow from 2-4 AM EST. Some services may be unavailable during this time."),
            CreateTestNotification("content-3", "marketing.sale", NotificationPriority.Low,
                "Sale", "Don't miss our biggest sale of the year! Save up to 50% on all items. Limited time offer.")
        };

        foreach (var notification in notifications)
        {
            _logger.LogInformation("Optimizing content for: {EventType}", notification.EventType);
            _logger.LogInformation("Original Subject: {Subject}", notification.Subject);
            _logger.LogInformation("Original Content: {Content}", notification.Content);

            var optimization = await _aiService.OptimizeContentAsync(notification);
            
            if (optimization.IsSuccess)
            {
                _logger.LogInformation("Optimized Subject: {Subject}", optimization.OptimizedSubject);
                _logger.LogInformation("Optimized Content: {Content}", optimization.OptimizedContent);
                _logger.LogInformation("Optimization Score: {Score:P2}", optimization.OptimizationScore);
                
                foreach (var improvement in optimization.Improvements)
                {
                    _logger.LogInformation("Improvement: {Improvement}", improvement);
                }
            }
            else
            {
                _logger.LogWarning("Content optimization failed");
            }

            _logger.LogInformation("---");
        }
    }

    private async Task DemonstrateNotificationSummariesAsync()
    {
        _logger.LogInformation("=== Notification Summaries ===");

        var notifications = new[]
        {
            CreateTestNotification("summary-1", "user.login", NotificationPriority.Normal),
            CreateTestNotification("summary-2", "user.logout", NotificationPriority.Normal),
            CreateTestNotification("summary-3", "system.error", NotificationPriority.Critical),
            CreateTestNotification("summary-4", "system.alert", NotificationPriority.High),
            CreateTestNotification("summary-5", "user.login", NotificationPriority.Normal)
        };

        _logger.LogInformation("Generating summary for {Count} notifications", notifications.Length);

        var summary = await _aiService.GenerateSummaryAsync(notifications);
        
        if (summary.IsSuccess)
        {
            _logger.LogInformation("Summary: {Summary}", summary.Summary);
            
            _logger.LogInformation("Statistics:");
            foreach (var stat in summary.Statistics)
            {
                _logger.LogInformation("  {Key}: {Value}", stat.Key, stat.Value);
            }

            _logger.LogInformation("Key Insights:");
            foreach (var insight in summary.KeyInsights)
            {
                _logger.LogInformation("  - {Insight}", insight);
            }

            _logger.LogInformation("Recommendations:");
            foreach (var recommendation in summary.Recommendations)
            {
                _logger.LogInformation("  - {Recommendation}", recommendation);
            }
        }
        else
        {
            _logger.LogWarning("Summary generation failed");
        }
    }

    private async Task DemonstrateSentimentAnalysisAsync()
    {
        _logger.LogInformation("=== Sentiment Analysis ===");

        var contents = new[]
        {
            "Welcome to our platform! We're excited to have you on board.",
            "We apologize for the inconvenience. Our system is currently experiencing issues.",
            "Congratulations! You've successfully completed your first transaction.",
            "URGENT: Security breach detected. Please change your password immediately.",
            "Thank you for your feedback. We appreciate your input and will review it carefully."
        };

        foreach (var content in contents)
        {
            _logger.LogInformation("Analyzing sentiment for: {Content}", content.Substring(0, Math.Min(50, content.Length)) + "...");

            var analysis = await _aiService.AnalyzeSentimentAsync(content);
            
            if (analysis.IsSuccess)
            {
                _logger.LogInformation("Sentiment: {Sentiment} (Confidence: {Confidence:P2})", 
                    analysis.Sentiment, analysis.Confidence);
                _logger.LogInformation("Explanation: {Explanation}", analysis.Explanation);
                
                _logger.LogInformation("Emotion Scores:");
                foreach (var emotion in analysis.EmotionScores)
                {
                    _logger.LogInformation("  {Emotion}: {Score:P2}", emotion.Key, emotion.Value);
                }

                _logger.LogInformation("Key Phrases: {Phrases}", string.Join(", ", analysis.KeyPhrases));
            }
            else
            {
                _logger.LogWarning("Sentiment analysis failed");
            }

            _logger.LogInformation("---");
        }
    }

    private async Task DemonstrateTemplateSuggestionsAsync()
    {
        _logger.LogInformation("=== Template Suggestions ===");

        var contexts = new[]
        {
            ("user onboarding", "user.welcome"),
            ("system maintenance", "system.maintenance"),
            ("security alert", "security.breach"),
            ("marketing campaign", "marketing.promo"),
            ("payment confirmation", "payment.success")
        };

        foreach (var (context, eventType) in contexts)
        {
            _logger.LogInformation("Suggesting templates for context: {Context}, event type: {EventType}", context, eventType);

            var suggestions = await _aiService.SuggestTemplatesAsync(context, eventType);
            
            if (suggestions.IsSuccess)
            {
                _logger.LogInformation("Suggested {Count} template(s):", suggestions.SuggestedTemplates.Count);
                
                foreach (var template in suggestions.SuggestedTemplates)
                {
                    _logger.LogInformation("  Template: {Name} - {Description}", template.Name, template.Description);
                    _logger.LogInformation("    Subject: {Subject}", template.Subject);
                    _logger.LogInformation("    Content: {Content}", template.Content.Substring(0, Math.Min(100, template.Content.Length)) + "...");
                }

                _logger.LogInformation("Reasoning:");
                foreach (var reason in suggestions.Reasoning)
                {
                    _logger.LogInformation("  - {Reason}", reason);
                }
            }
            else
            {
                _logger.LogWarning("Template suggestion failed");
            }

            _logger.LogInformation("---");
        }
    }

    private async Task TestMCPServiceHealthAsync()
    {
        _logger.LogInformation("=== MCP Service Health Check ===");

        var healthStatus = await _mcpService.GetHealthStatusAsync();
        _logger.LogInformation("MCP Service Health: {Status}", healthStatus);
    }

    private async Task DemonstrateMCPToolsAsync()
    {
        _logger.LogInformation("=== MCP Tools Demonstration ===");

        // Get available tools
        var tools = await _mcpService.GetAvailableToolsAsync();
        _logger.LogInformation("Available MCP Tools: {Count}", tools.Count());

        foreach (var tool in tools)
        {
            _logger.LogInformation("Tool: {Name} - {Description}", tool.Name, tool.Description);
            _logger.LogInformation("  Version: {Version}, Enabled: {Enabled}", tool.Version, tool.IsEnabled);
            _logger.LogInformation("  Parameters: {Count}", tool.Parameters.Count);
        }

        // Execute some tools
        var toolExecutions = new[]
        {
            ("send_notification", new Dictionary<string, object>
            {
                ["tenant_id"] = "test-tenant",
                ["event_type"] = "user.login",
                ["subject"] = "Welcome!",
                ["content"] = "Welcome to our platform!",
                ["recipients"] = new[] { "user@example.com" }
            }),
            ("get_notification_status", new Dictionary<string, object>
            {
                ["notification_id"] = "test-notification-123"
            }),
            ("analyze_notifications", new Dictionary<string, object>
            {
                ["tenant_id"] = "test-tenant",
                ["time_range"] = "7d"
            })
        };

        foreach (var (toolName, parameters) in toolExecutions)
        {
            _logger.LogInformation("Executing tool: {ToolName}", toolName);

            var result = await _mcpService.ExecuteToolAsync(toolName, parameters);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Tool execution successful in {ExecutionTime}ms", result.ExecutionTime.TotalMilliseconds);
                _logger.LogInformation("Result: {Result}", result.Result);
            }
            else
            {
                _logger.LogWarning("Tool execution failed: {ErrorMessage}", result.ErrorMessage);
            }

            _logger.LogInformation("---");
        }
    }

    private async Task DemonstrateAINotificationOptimizationAsync()
    {
        _logger.LogInformation("=== AI Notification Optimization ===");

        var notifications = new[]
        {
            CreateTestNotification("optimize-1", "user.login", NotificationPriority.Normal),
            CreateTestNotification("optimize-2", "system.error", NotificationPriority.Critical),
            CreateTestNotification("optimize-3", "marketing.promo", NotificationPriority.Low)
        };

        foreach (var notification in notifications)
        {
            _logger.LogInformation("Optimizing notification: {EventType} (Priority: {Priority})", 
                notification.EventType, notification.Priority);

            // Optimize delivery timing
            var timingResult = await _aiOptimizer.OptimizeDeliveryTimingAsync(notification);
            if (timingResult.IsSuccess)
            {
                _logger.LogInformation("Timing Optimization:");
                _logger.LogInformation("  Recommended Time: {Time}", timingResult.RecommendedTime);
                _logger.LogInformation("  Recommended Delay: {Delay}", timingResult.RecommendedDelay);
                _logger.LogInformation("  Reasoning: {Reasoning}", timingResult.Reasoning);
                _logger.LogInformation("  Confidence: {Confidence:P2}", timingResult.Confidence);
            }

            // Optimize channel selection
            var channelResult = await _aiOptimizer.OptimizeChannelSelectionAsync(notification);
            if (channelResult.IsSuccess)
            {
                _logger.LogInformation("Channel Optimization:");
                _logger.LogInformation("  Recommended Channels: {Channels}", 
                    string.Join(", ", channelResult.RecommendedChannels));
                _logger.LogInformation("  Reasoning: {Reasoning}", channelResult.Reasoning);
                _logger.LogInformation("  Confidence: {Confidence:P2}", channelResult.Confidence);
            }

            _logger.LogInformation("---");
        }

        // Demonstrate frequency optimization
        _logger.LogInformation("Optimizing frequency for recipient: test-recipient-123, event type: user.login");
        var frequencyResult = await _aiOptimizer.OptimizeFrequencyAsync("test-recipient-123", "user.login");
        if (frequencyResult.IsSuccess)
        {
            _logger.LogInformation("Frequency Optimization:");
            _logger.LogInformation("  Recommended Interval: {Interval}", frequencyResult.RecommendedInterval);
            _logger.LogInformation("  Recommended Max Per Day: {MaxPerDay}", frequencyResult.RecommendedMaxPerDay);
            _logger.LogInformation("  Reasoning: {Reasoning}", frequencyResult.Reasoning);
            _logger.LogInformation("  Confidence: {Confidence:P2}", frequencyResult.Confidence);
        }

        // Demonstrate performance analysis
        _logger.LogInformation("Analyzing performance patterns for tenant: test-tenant");
        var performanceResult = await _aiOptimizer.AnalyzePerformancePatternsAsync("test-tenant", TimeSpan.FromDays(7));
        if (performanceResult.IsSuccess)
        {
            _logger.LogInformation("Performance Analysis:");
            _logger.LogInformation("  Metrics:");
            foreach (var metric in performanceResult.PerformanceMetrics)
            {
                _logger.LogInformation("    {Key}: {Value}", metric.Key, metric.Value);
            }

            _logger.LogInformation("  Insights:");
            foreach (var insight in performanceResult.Insights)
            {
                _logger.LogInformation("    - {Insight}", insight);
            }

            _logger.LogInformation("  Recommendations:");
            foreach (var recommendation in performanceResult.Recommendations)
            {
                _logger.LogInformation("    - {Recommendation}", recommendation);
            }
        }
    }

    private static NotificationEvent CreateTestNotification(string id, string eventType, NotificationPriority priority, string? subject = null, string? content = null)
    {
        return new NotificationEvent
        {
            Id = id,
            TenantId = "test-tenant",
            EventType = eventType,
            Priority = priority,
            Subject = subject ?? $"Test Notification - {eventType}",
            Content = content ?? $"This is a test notification for {eventType}",
            Recipients = new List<NotificationRecipient>
            {
                new NotificationRecipient
                {
                    Id = "test-recipient",
                    Name = "Test User",
                    Email = "test@example.com",
                    TimeZone = "America/New_York",
                    ChannelPreferences = new List<ChannelPreference>
                    {
                        ChannelPreference.Preferred
                    }
                }
            },
            PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email },
            Metadata = new Dictionary<string, object>
            {
                ["source"] = "ai-mcp-sample",
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }
}