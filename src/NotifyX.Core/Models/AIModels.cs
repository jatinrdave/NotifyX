using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents AI rule translation result.
/// </summary>
public sealed record AIRuleTranslationResult
{
    public bool IsSuccess { get; init; }
    public NotificationRule? Rule { get; init; }
    public string Confidence { get; init; } = string.Empty;
    public string Explanation { get; init; } = string.Empty;
    public List<string> Suggestions { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI routing recommendation.
/// </summary>
public sealed record AIRoutingRecommendation
{
    public List<NotificationChannel> RecommendedChannels { get; init; } = new();
    public Dictionary<NotificationChannel, double> ChannelScores { get; init; } = new();
    public string Reasoning { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI content optimization result.
/// </summary>
public sealed record AIContentOptimizationResult
{
    public bool IsSuccess { get; init; }
    public string OriginalContent { get; init; } = string.Empty;
    public string OptimizedContent { get; init; } = string.Empty;
    public string OriginalSubject { get; init; } = string.Empty;
    public string OptimizedSubject { get; init; } = string.Empty;
    public List<string> Improvements { get; init; } = new();
    public double OptimizationScore { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI summary result.
/// </summary>
public sealed record AISummaryResult
{
    public bool IsSuccess { get; init; }
    public string Summary { get; init; } = string.Empty;
    public Dictionary<string, int> Statistics { get; init; } = new();
    public List<string> KeyInsights { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI sentiment analysis result.
/// </summary>
public sealed record AISentimentAnalysisResult
{
    public bool IsSuccess { get; init; }
    public SentimentType Sentiment { get; init; }
    public double Confidence { get; init; }
    public Dictionary<string, double> EmotionScores { get; init; } = new();
    public List<string> KeyPhrases { get; init; } = new();
    public string Explanation { get; init; } = string.Empty;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI template suggestion result.
/// </summary>
public sealed record AITemplateSuggestionResult
{
    public bool IsSuccess { get; init; }
    public List<NotificationTemplate> SuggestedTemplates { get; init; } = new();
    public string Context { get; init; } = string.Empty;
    public List<string> Reasoning { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI timing optimization result.
/// </summary>
public sealed record AITimingOptimizationResult
{
    public bool IsSuccess { get; init; }
    public DateTime? RecommendedTime { get; init; }
    public TimeSpan? RecommendedDelay { get; init; }
    public string Reasoning { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI channel optimization result.
/// </summary>
public sealed record AIChannelOptimizationResult
{
    public bool IsSuccess { get; init; }
    public List<NotificationChannel> RecommendedChannels { get; init; } = new();
    public Dictionary<NotificationChannel, double> ChannelScores { get; init; } = new();
    public string Reasoning { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI frequency optimization result.
/// </summary>
public sealed record AIFrequencyOptimizationResult
{
    public bool IsSuccess { get; init; }
    public TimeSpan? RecommendedInterval { get; init; }
    public int? RecommendedMaxPerDay { get; init; }
    public string Reasoning { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents AI performance analysis result.
/// </summary>
public sealed record AIPerformanceAnalysisResult
{
    public bool IsSuccess { get; init; }
    public Dictionary<string, double> PerformanceMetrics { get; init; } = new();
    public List<string> Insights { get; init; } = new();
    public List<string> Recommendations { get; init; } = new();
    public Dictionary<string, object> Trends { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents an MCP tool.
/// </summary>
public sealed record MCPTool
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0.0";
    public Dictionary<string, MCPToolParameter> Parameters { get; init; } = new();
    public string ReturnType { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents an MCP tool parameter.
/// </summary>
public sealed record MCPToolParameter
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsRequired { get; init; } = false;
    public object? DefaultValue { get; init; }
    public List<object>? AllowedValues { get; init; }
    public Dictionary<string, object> Validation { get; init; } = new();
}

/// <summary>
/// Represents MCP tool execution result.
/// </summary>
public sealed record MCPToolResult
{
    public bool IsSuccess { get; init; }
    public string ToolName { get; init; } = string.Empty;
    public object? Result { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public TimeSpan ExecutionTime { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents sentiment types.
/// </summary>
public enum SentimentType
{
    Positive,
    Negative,
    Neutral,
    Mixed
}

/// <summary>
/// Represents AI service configuration.
/// </summary>
public sealed record AIServiceOptions
{
    public string Provider { get; init; } = "OpenAI";
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string Model { get; init; } = "gpt-3.5-turbo";
    public int MaxTokens { get; init; } = 1000;
    public double Temperature { get; init; } = 0.7;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public bool EnableCaching { get; init; } = true;
    public TimeSpan CacheExpiration { get; init; } = TimeSpan.FromHours(1);
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents MCP service configuration.
/// </summary>
public sealed record MCPServiceOptions
{
    public string ServerUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public int MaxConcurrentTools { get; init; } = 10;
    public bool EnableToolDiscovery { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}