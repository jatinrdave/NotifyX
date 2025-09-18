using NotifyX.Core.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Interface for AI service operations.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Translates natural language to notification rules.
    /// </summary>
    Task<AIRuleTranslationResult> TranslateNaturalLanguageToRuleAsync(string naturalLanguage, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates intelligent routing recommendations.
    /// </summary>
    Task<AIRoutingRecommendation> GetRoutingRecommendationAsync(NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimizes notification content using AI.
    /// </summary>
    Task<AIContentOptimizationResult> OptimizeContentAsync(NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates notification summaries.
    /// </summary>
    Task<AISummaryResult> GenerateSummaryAsync(IEnumerable<NotificationEvent> notifications, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes sentiment of notification content.
    /// </summary>
    Task<AISentimentAnalysisResult> AnalyzeSentimentAsync(string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests notification templates based on context.
    /// </summary>
    Task<AITemplateSuggestionResult> SuggestTemplatesAsync(string context, string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets AI service health status.
    /// </summary>
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for MCP (Model Context Protocol) service operations.
/// </summary>
public interface IMCPService
{
    /// <summary>
    /// Gets available MCP tools.
    /// </summary>
    Task<IEnumerable<MCPTool>> GetAvailableToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an MCP tool.
    /// </summary>
    Task<MCPToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new MCP tool.
    /// </summary>
    Task<bool> RegisterToolAsync(MCPTool tool, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters an MCP tool.
    /// </summary>
    Task<bool> UnregisterToolAsync(string toolName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets MCP service health status.
    /// </summary>
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for AI-powered notification optimization.
/// </summary>
public interface IAINotificationOptimizer
{
    /// <summary>
    /// Optimizes notification delivery timing.
    /// </summary>
    Task<AITimingOptimizationResult> OptimizeDeliveryTimingAsync(NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimizes notification channel selection.
    /// </summary>
    Task<AIChannelOptimizationResult> OptimizeChannelSelectionAsync(NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimizes notification frequency for recipients.
    /// </summary>
    Task<AIFrequencyOptimizationResult> OptimizeFrequencyAsync(string recipientId, string eventType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes notification performance patterns.
    /// </summary>
    Task<AIPerformanceAnalysisResult> AnalyzePerformancePatternsAsync(string tenantId, TimeSpan timeRange, CancellationToken cancellationToken = default);
}