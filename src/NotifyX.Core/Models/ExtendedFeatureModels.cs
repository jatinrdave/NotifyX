using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents a rich template request.
/// </summary>
public sealed record RichTemplateRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public NotificationChannel Channel { get; init; } = NotificationChannel.Email;
    public string Subject { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public List<TemplateBlock> Blocks { get; init; } = new();
    public List<TemplateCondition> Conditions { get; init; } = new();
    public Dictionary<string, object> Variables { get; init; } = new();
    public TemplateStyle Style { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a template block.
/// </summary>
public sealed record TemplateBlock
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Dictionary<string, object> Properties { get; init; } = new();
    public List<TemplateCondition> Conditions { get; init; } = new();
    public int Order { get; init; } = 0;
    public bool IsVisible { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a template condition.
/// </summary>
public sealed record TemplateCondition
{
    public string Field { get; init; } = string.Empty;
    public string Operator { get; init; } = string.Empty;
    public object Value { get; init; } = string.Empty;
    public string LogicalOperator { get; init; } = "AND";
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents template styling.
/// </summary>
public sealed record TemplateStyle
{
    public string Theme { get; init; } = "default";
    public string PrimaryColor { get; init; } = "#007bff";
    public string SecondaryColor { get; init; } = "#6c757d";
    public string FontFamily { get; init; } = "Arial, sans-serif";
    public int FontSize { get; init; } = 14;
    public string BackgroundColor { get; init; } = "#ffffff";
    public string TextColor { get; init; } = "#333333";
    public Dictionary<string, object> CustomStyles { get; init; } = new();
}


/// <summary>
/// Represents a template validation error.
/// </summary>
public sealed record TemplateValidationError
{
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int Line { get; init; } = 0;
    public int Column { get; init; } = 0;
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Represents a template validation warning.
/// </summary>
public sealed record TemplateValidationWarning
{
    public string Type { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public int Line { get; init; } = 0;
    public int Column { get; init; } = 0;
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Represents template preview result.
/// </summary>
public sealed record TemplatePreviewResult
{
    public bool IsSuccess { get; init; }
    public string RenderedSubject { get; init; } = string.Empty;
    public string RenderedContent { get; init; } = string.Empty;
    public Dictionary<string, object> UsedVariables { get; init; } = new();
    public List<string> MissingVariables { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents template usage statistics.
/// </summary>
public sealed record TemplateUsageStatistics
{
    public string TemplateId { get; init; } = string.Empty;
    public int TotalUsage { get; init; }
    public int SuccessfulUsage { get; init; }
    public int FailedUsage { get; init; }
    public double SuccessRate { get; init; }
    public DateTime LastUsed { get; init; }
    public Dictionary<string, int> UsageByChannel { get; init; } = new();
    public Dictionary<string, int> UsageByPriority { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents timezone information.
/// </summary>
public sealed record TimezoneInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public TimeSpan Offset { get; init; }
    public bool SupportsDaylightSaving { get; init; }
    public DateTime? DaylightSavingStart { get; init; }
    public DateTime? DaylightSavingEnd { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a failover rule.
/// </summary>
public sealed record FailoverRule
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public NotificationChannel PrimaryChannel { get; init; } = NotificationChannel.Email;
    public List<NotificationChannel> FailoverChannels { get; init; } = new();
    public List<string> FailureReasons { get; init; } = new();
    public TimeSpan Delay { get; init; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; init; } = 3;
    public bool IsEnabled { get; init; } = true;
    public Dictionary<string, object> Conditions { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents failover result.
/// </summary>
public sealed record FailoverResult
{
    public bool IsSuccess { get; init; }
    public NotificationChannel? UsedChannel { get; init; }
    public string Message { get; init; } = string.Empty;
    public TimeSpan Delay { get; init; }
    public int Attempts { get; init; }
    public List<string> FailedChannels { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents failover statistics.
/// </summary>
public sealed record FailoverStatistics
{
    public int TotalFailovers { get; init; }
    public int SuccessfulFailovers { get; init; }
    public int FailedFailovers { get; init; }
    public double SuccessRate { get; init; }
    public TimeSpan AverageDelay { get; init; }
    public Dictionary<NotificationChannel, int> FailoverByChannel { get; init; } = new();
    public Dictionary<string, int> FailoverByReason { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents failover test result.
/// </summary>
public sealed record FailoverTestResult
{
    public bool IsSuccess { get; init; }
    public List<FailoverTestScenario> TestScenarios { get; init; } = new();
    public string Summary { get; init; } = string.Empty;
    public List<string> Recommendations { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a failover test scenario.
/// </summary>
public sealed record FailoverTestScenario
{
    public string Name { get; init; } = string.Empty;
    public NotificationChannel PrimaryChannel { get; init; } = NotificationChannel.Email;
    public string FailureReason { get; init; } = string.Empty;
    public bool TestPassed { get; init; }
    public NotificationChannel? UsedFailoverChannel { get; init; }
    public TimeSpan ExecutionTime { get; init; }
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Represents a CLI command.
/// </summary>
public sealed record CLICommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Syntax { get; init; } = string.Empty;
    public List<CLICommandParameter> Parameters { get; init; } = new();
    public List<string> Aliases { get; init; } = new();
    public bool RequiresAuthentication { get; init; } = false;
    public List<string> RequiredPermissions { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a CLI command parameter.
/// </summary>
public sealed record CLICommandParameter
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsRequired { get; init; } = false;
    public object? DefaultValue { get; init; }
    public List<object>? AllowedValues { get; init; }
    public string ShortName { get; init; } = string.Empty;
    public Dictionary<string, object> Validation { get; init; } = new();
}

/// <summary>
/// Represents CLI command result.
/// </summary>
public sealed record CLICommandResult
{
    public bool IsSuccess { get; init; }
    public string Command { get; init; } = string.Empty;
    public object? Result { get; init; }
    public string Output { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
    public TimeSpan ExecutionTime { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents CLI command help.
/// </summary>
public sealed record CLICommandHelp
{
    public string Command { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Syntax { get; init; } = string.Empty;
    public List<string> Examples { get; init; } = new();
    public List<CLICommandParameter> Parameters { get; init; } = new();
    public List<string> Notes { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents CLI validation result.
/// </summary>
public sealed record CLIValidationResult
{
    public bool IsValid { get; init; }
    public string Command { get; init; } = string.Empty;
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public List<string> Suggestions { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}