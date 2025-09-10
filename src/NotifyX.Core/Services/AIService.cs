using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Text;
using System.Text.Json;

namespace NotifyX.Core.Services;

/// <summary>
/// AI service implementation for notification optimization and analysis.
/// </summary>
public class AIService : IAIService
{
    private readonly ILogger<AIService> _logger;
    private readonly AIServiceOptions _options;

    public AIService(ILogger<AIService> logger, AIServiceOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<AIRuleTranslationResult> TranslateNaturalLanguageToRuleAsync(string naturalLanguage, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Translating natural language to rule: {NaturalLanguage}", naturalLanguage);

            // Simulate AI processing
            await Task.Delay(1000, cancellationToken);

            var rule = new NotificationRule
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId,
                Name = $"AI Generated Rule - {DateTime.UtcNow:yyyyMMddHHmmss}",
                Description = $"Rule generated from: {naturalLanguage}",
                IsActive = true,
                Priority = NotificationPriority.Normal,
                Condition = new RuleCondition
                {
                    ConditionType = ConditionType.EventType,
                    ConditionOperator = ConditionOperator.Contains,
                    Value = ExtractEventType(naturalLanguage)
                },
                Actions = new List<RuleAction>
                {
                    new RuleAction
                    {
                        ActionType = ActionType.SendNotification,
                        Parameters = new Dictionary<string, object>
                        {
                            ["channel"] = ExtractChannel(naturalLanguage),
                            ["template"] = "default"
                        }
                    }
                }
            };

            return new AIRuleTranslationResult
            {
                IsSuccess = true,
                Rule = rule,
                Confidence = "0.85",
                Explanation = $"Generated rule based on natural language input. Detected event type: {ExtractEventType(naturalLanguage)}, recommended channel: {ExtractChannel(naturalLanguage)}",
                Suggestions = new List<string>
                {
                    "Consider adding time-based conditions",
                    "Review recipient targeting",
                    "Add escalation rules for critical events"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to translate natural language to rule");
            return new AIRuleTranslationResult
            {
                IsSuccess = false,
                Explanation = $"Translation failed: {ex.Message}"
            };
        }
    }

    public async Task<AIRoutingRecommendation> GetRoutingRecommendationAsync(NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting routing recommendation for notification {NotificationId}", notification.Id);

            // Simulate AI processing
            await Task.Delay(500, cancellationToken);

            var recommendations = AnalyzeNotificationForRouting(notification);
            var channelScores = CalculateChannelScores(notification);

            return new AIRoutingRecommendation
            {
                RecommendedChannels = recommendations,
                ChannelScores = channelScores,
                Reasoning = GenerateRoutingReasoning(notification, recommendations),
                Confidence = 0.82
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get routing recommendation");
            return new AIRoutingRecommendation
            {
                RecommendedChannels = new List<NotificationChannel> { NotificationChannel.Email },
                ChannelScores = new Dictionary<NotificationChannel, double> { { NotificationChannel.Email, 0.5 } },
                Reasoning = "Fallback to email due to analysis error",
                Confidence = 0.1
            };
        }
    }

    public async Task<AIContentOptimizationResult> OptimizeContentAsync(NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Optimizing content for notification {NotificationId}", notification.Id);

            // Simulate AI processing
            await Task.Delay(800, cancellationToken);

            var optimizedContent = OptimizeNotificationContent(notification.Content);
            var optimizedSubject = OptimizeNotificationSubject(notification.Subject);
            var improvements = AnalyzeImprovements(notification.Content, optimizedContent);

            return new AIContentOptimizationResult
            {
                IsSuccess = true,
                OriginalContent = notification.Content,
                OptimizedContent = optimizedContent,
                OriginalSubject = notification.Subject,
                OptimizedSubject = optimizedSubject,
                Improvements = improvements,
                OptimizationScore = CalculateOptimizationScore(notification.Content, optimizedContent)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to optimize content");
            return new AIContentOptimizationResult
            {
                IsSuccess = false,
                OriginalContent = notification.Content,
                OptimizedContent = notification.Content,
                OriginalSubject = notification.Subject,
                OptimizedSubject = notification.Subject,
                OptimizationScore = 0.0
            };
        }
    }

    public async Task<AISummaryResult> GenerateSummaryAsync(IEnumerable<NotificationEvent> notifications, CancellationToken cancellationToken = default)
    {
        try
        {
            var notificationList = notifications.ToList();
            _logger.LogDebug("Generating summary for {Count} notifications", notificationList.Count);

            // Simulate AI processing
            await Task.Delay(1200, cancellationToken);

            var statistics = CalculateNotificationStatistics(notificationList);
            var insights = GenerateInsights(notificationList);
            var recommendations = GenerateRecommendations(notificationList);

            return new AISummaryResult
            {
                IsSuccess = true,
                Summary = GenerateSummaryText(notificationList, statistics),
                Statistics = statistics,
                KeyInsights = insights,
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate summary");
            return new AISummaryResult
            {
                IsSuccess = false,
                Summary = "Failed to generate summary due to processing error"
            };
        }
    }

    public async Task<AISentimentAnalysisResult> AnalyzeSentimentAsync(string content, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Analyzing sentiment for content: {Content}", content.Substring(0, Math.Min(100, content.Length)));

            // Simulate AI processing
            await Task.Delay(600, cancellationToken);

            var sentiment = AnalyzeSentiment(content);
            var emotionScores = CalculateEmotionScores(content);
            var keyPhrases = ExtractKeyPhrases(content);

            return new AISentimentAnalysisResult
            {
                IsSuccess = true,
                Sentiment = sentiment,
                Confidence = CalculateSentimentConfidence(content),
                EmotionScores = emotionScores,
                KeyPhrases = keyPhrases,
                Explanation = GenerateSentimentExplanation(sentiment, emotionScores)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze sentiment");
            return new AISentimentAnalysisResult
            {
                IsSuccess = false,
                Sentiment = SentimentType.Neutral,
                Confidence = 0.0,
                Explanation = "Sentiment analysis failed due to processing error"
            };
        }
    }

    public async Task<AITemplateSuggestionResult> SuggestTemplatesAsync(string context, string eventType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Suggesting templates for context: {Context}, event type: {EventType}", context, eventType);

            // Simulate AI processing
            await Task.Delay(700, cancellationToken);

            var templates = GenerateTemplateSuggestions(context, eventType);
            var reasoning = GenerateTemplateReasoning(context, eventType, templates);

            return new AITemplateSuggestionResult
            {
                IsSuccess = true,
                SuggestedTemplates = templates,
                Context = context,
                Reasoning = reasoning
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suggest templates");
            return new AITemplateSuggestionResult
            {
                IsSuccess = false,
                Context = context,
                Reasoning = new List<string> { "Template suggestion failed due to processing error" }
            };
        }
    }

    public async Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate health check
            await Task.Delay(100, cancellationToken);

            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                return HealthStatus.Unhealthy;
            }

            return HealthStatus.Healthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get AI service health status");
            return HealthStatus.Unhealthy;
        }
    }

    private static string ExtractEventType(string naturalLanguage)
    {
        var lowerText = naturalLanguage.ToLower();
        if (lowerText.Contains("login") || lowerText.Contains("sign in"))
            return "user.login";
        if (lowerText.Contains("logout") || lowerText.Contains("sign out"))
            return "user.logout";
        if (lowerText.Contains("error") || lowerText.Contains("failed"))
            return "system.error";
        if (lowerText.Contains("alert") || lowerText.Contains("warning"))
            return "system.alert";
        
        return "general.notification";
    }

    private static NotificationChannel ExtractChannel(string naturalLanguage)
    {
        var lowerText = naturalLanguage.ToLower();
        if (lowerText.Contains("email") || lowerText.Contains("mail"))
            return NotificationChannel.Email;
        if (lowerText.Contains("sms") || lowerText.Contains("text"))
            return NotificationChannel.SMS;
        if (lowerText.Contains("push") || lowerText.Contains("mobile"))
            return NotificationChannel.Push;
        if (lowerText.Contains("slack"))
            return NotificationChannel.Slack;
        
        return NotificationChannel.Email;
    }

    private static List<NotificationChannel> AnalyzeNotificationForRouting(NotificationEvent notification)
    {
        var channels = new List<NotificationChannel>();

        // Priority-based routing
        switch (notification.Priority)
        {
            case NotificationPriority.Critical:
                channels.AddRange(new[] { NotificationChannel.Push, NotificationChannel.SMS, NotificationChannel.Email });
                break;
            case NotificationPriority.High:
                channels.AddRange(new[] { NotificationChannel.Email, NotificationChannel.Push });
                break;
            case NotificationPriority.Normal:
                channels.Add(NotificationChannel.Email);
                break;
            case NotificationPriority.Low:
                channels.Add(NotificationChannel.Email);
                break;
        }

        return channels;
    }

    private static Dictionary<NotificationChannel, double> CalculateChannelScores(NotificationEvent notification)
    {
        var scores = new Dictionary<NotificationChannel, double>();

        // Base scores
        scores[NotificationChannel.Email] = 0.8;
        scores[NotificationChannel.SMS] = 0.6;
        scores[NotificationChannel.Push] = 0.7;
        scores[NotificationChannel.Webhook] = 0.5;

        // Adjust based on priority
        switch (notification.Priority)
        {
            case NotificationPriority.Critical:
                scores[NotificationChannel.SMS] += 0.3;
                scores[NotificationChannel.Push] += 0.2;
                break;
            case NotificationPriority.High:
                scores[NotificationChannel.Push] += 0.1;
                break;
        }

        return scores;
    }

    private static string GenerateRoutingReasoning(NotificationEvent notification, List<NotificationChannel> recommendations)
    {
        var reasoning = new StringBuilder();
        reasoning.Append($"Based on priority {notification.Priority}, ");
        
        if (notification.Priority == NotificationPriority.Critical)
        {
            reasoning.Append("critical notifications require immediate delivery via multiple channels.");
        }
        else if (notification.Priority == NotificationPriority.High)
        {
            reasoning.Append("high priority notifications benefit from push notifications for faster delivery.");
        }
        else
        {
            reasoning.Append("standard notifications are efficiently delivered via email.");
        }

        return reasoning.ToString();
    }

    private static string OptimizeNotificationContent(string content)
    {
        // Simple content optimization
        var optimized = content.Trim();
        
        // Add call-to-action if missing
        if (!optimized.ToLower().Contains("click") && !optimized.ToLower().Contains("visit"))
        {
            optimized += "\n\nFor more information, please visit our website.";
        }

        return optimized;
    }

    private static string OptimizeNotificationSubject(string subject)
    {
        // Simple subject optimization
        var optimized = subject.Trim();
        
        // Ensure subject is not too long
        if (optimized.Length > 50)
        {
            optimized = optimized.Substring(0, 47) + "...";
        }

        return optimized;
    }

    private static List<string> AnalyzeImprovements(string original, string optimized)
    {
        var improvements = new List<string>();

        if (optimized.Length > original.Length)
        {
            improvements.Add("Added call-to-action");
        }

        if (optimized.Trim() != original.Trim())
        {
            improvements.Add("Improved formatting");
        }

        return improvements;
    }

    private static double CalculateOptimizationScore(string original, string optimized)
    {
        // Simple scoring based on length and content changes
        var lengthScore = Math.Min(1.0, (double)optimized.Length / Math.Max(1, original.Length));
        var contentScore = original.Equals(optimized) ? 0.5 : 0.8;
        
        return (lengthScore + contentScore) / 2;
    }

    private static Dictionary<string, int> CalculateNotificationStatistics(List<NotificationEvent> notifications)
    {
        return new Dictionary<string, int>
        {
            ["Total"] = notifications.Count,
            ["Critical"] = notifications.Count(n => n.Priority == NotificationPriority.Critical),
            ["High"] = notifications.Count(n => n.Priority == NotificationPriority.High),
            ["Normal"] = notifications.Count(n => n.Priority == NotificationPriority.Normal),
            ["Low"] = notifications.Count(n => n.Priority == NotificationPriority.Low)
        };
    }

    private static List<string> GenerateInsights(List<NotificationEvent> notifications)
    {
        var insights = new List<string>();

        var criticalCount = notifications.Count(n => n.Priority == NotificationPriority.Critical);
        if (criticalCount > notifications.Count * 0.1)
        {
            insights.Add("High volume of critical notifications detected");
        }

        var uniqueEventTypes = notifications.Select(n => n.EventType).Distinct().Count();
        insights.Add($"Notifications span {uniqueEventTypes} different event types");

        return insights;
    }

    private static List<string> GenerateRecommendations(List<NotificationEvent> notifications)
    {
        var recommendations = new List<string>();

        var criticalCount = notifications.Count(n => n.Priority == NotificationPriority.Critical);
        if (criticalCount > notifications.Count * 0.1)
        {
            recommendations.Add("Consider reviewing critical notification thresholds");
        }

        recommendations.Add("Monitor notification delivery rates");
        recommendations.Add("Review recipient engagement patterns");

        return recommendations;
    }

    private static string GenerateSummaryText(List<NotificationEvent> notifications, Dictionary<string, int> statistics)
    {
        return $"Summary of {statistics["Total"]} notifications: {statistics["Critical"]} critical, {statistics["High"]} high priority, {statistics["Normal"]} normal, {statistics["Low"]} low priority.";
    }

    private static SentimentType AnalyzeSentiment(string content)
    {
        var lowerContent = content.ToLower();
        
        var positiveWords = new[] { "good", "great", "excellent", "success", "welcome", "thank" };
        var negativeWords = new[] { "bad", "error", "failed", "problem", "issue", "sorry" };

        var positiveCount = positiveWords.Count(word => lowerContent.Contains(word));
        var negativeCount = negativeWords.Count(word => lowerContent.Contains(word));

        if (positiveCount > negativeCount)
            return SentimentType.Positive;
        if (negativeCount > positiveCount)
            return SentimentType.Negative;
        
        return SentimentType.Neutral;
    }

    private static Dictionary<string, double> CalculateEmotionScores(string content)
    {
        return new Dictionary<string, double>
        {
            ["joy"] = 0.3,
            ["sadness"] = 0.1,
            ["anger"] = 0.1,
            ["fear"] = 0.1,
            ["surprise"] = 0.2,
            ["neutral"] = 0.2
        };
    }

    private static List<string> ExtractKeyPhrases(string content)
    {
        // Simple key phrase extraction
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 4)
            .GroupBy(w => w.ToLower())
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        return words;
    }

    private static double CalculateSentimentConfidence(string content)
    {
        // Simple confidence calculation based on content length and word variety
        var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var uniqueWords = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().Count();
        
        return Math.Min(1.0, (double)uniqueWords / Math.Max(1, wordCount));
    }

    private static string GenerateSentimentExplanation(SentimentType sentiment, Dictionary<string, double> emotionScores)
    {
        var dominantEmotion = emotionScores.OrderByDescending(e => e.Value).First();
        return $"Content shows {sentiment.ToString().ToLower()} sentiment with {dominantEmotion.Key} being the dominant emotion ({dominantEmotion.Value:P0}).";
    }

    private static List<NotificationTemplate> GenerateTemplateSuggestions(string context, string eventType)
    {
        return new List<NotificationTemplate>
        {
            new NotificationTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Template for {eventType}",
                Description = $"Generated template for {context}",
                Subject = $"Notification: {eventType}",
                Content = $"This is a notification for {eventType} in the context of {context}.",
                Channel = NotificationChannel.Email
            }
        };
    }

    private static List<string> GenerateTemplateReasoning(string context, string eventType, List<NotificationTemplate> templates)
    {
        return new List<string>
        {
            $"Generated {templates.Count} template(s) based on event type '{eventType}'",
            $"Context '{context}' influenced template selection",
            "Templates are optimized for clarity and engagement"
        };
    }
}