using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents connector configuration.
/// </summary>
public sealed record ConnectorConfiguration
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0.0";
    public bool IsEnabled { get; init; } = true;
    public string BaseUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
    public Dictionary<string, string> Headers { get; init; } = new();
    public Dictionary<string, object> Settings { get; init; } = new();
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public int MaxRetries { get; init; } = 3;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents webhook configuration.
/// </summary>
public sealed record WebhookConfiguration
{
    public string Url { get; init; } = string.Empty;
    public string Method { get; init; } = "POST";
    public Dictionary<string, string> Headers { get; init; } = new();
    public string ContentType { get; init; } = "application/json";
    public string BodyTemplate { get; init; } = string.Empty;
    public Dictionary<string, object> QueryParameters { get; init; } = new();
    public bool VerifySsl { get; init; } = true;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents webhook result.
/// </summary>
public sealed record WebhookResult
{
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public string ResponseBody { get; init; } = string.Empty;
    public Dictionary<string, string> ResponseHeaders { get; init; } = new();
    public TimeSpan Duration { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents connector test result.
/// </summary>
public sealed record ConnectorTestResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public Dictionary<string, object> Details { get; init; } = new();
}

/// <summary>
/// Represents Zapier Zap configuration.
/// </summary>
public sealed record ZapierZapConfiguration
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string TriggerUrl { get; init; } = string.Empty;
    public string WebhookUrl { get; init; } = string.Empty;
    public Dictionary<string, object> TriggerData { get; init; } = new();
    public Dictionary<string, object> ActionData { get; init; } = new();
    public bool IsEnabled { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a Zapier Zap.
/// </summary>
public sealed record ZapierZap
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastTriggeredAt { get; init; }
    public int TriggerCount { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents Zapier Zap result.
/// </summary>
public sealed record ZapierZapResult
{
    public bool IsSuccess { get; init; }
    public string ZapId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object> Details { get; init; } = new();
}

/// <summary>
/// Represents Zapier trigger result.
/// </summary>
public sealed record ZapierTriggerResult
{
    public bool IsSuccess { get; init; }
    public string ExecutionId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object> Response { get; init; } = new();
}

/// <summary>
/// Represents n8n workflow configuration.
/// </summary>
public sealed record N8nWorkflowConfiguration
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string WebhookUrl { get; init; } = string.Empty;
    public Dictionary<string, object> WorkflowData { get; init; } = new();
    public bool IsActive { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents an n8n workflow.
/// </summary>
public sealed record N8nWorkflow
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastExecutedAt { get; init; }
    public int ExecutionCount { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents n8n workflow result.
/// </summary>
public sealed record N8nWorkflowResult
{
    public bool IsSuccess { get; init; }
    public string ExecutionId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object> Output { get; init; } = new();
}

/// <summary>
/// Represents n8n webhook configuration.
/// </summary>
public sealed record N8nWebhookConfiguration
{
    public string WorkflowId { get; init; } = string.Empty;
    public string NodeName { get; init; } = string.Empty;
    public string WebhookPath { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = "POST";
    public Dictionary<string, object> NodeData { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents n8n webhook result.
/// </summary>
public sealed record N8nWebhookResult
{
    public bool IsSuccess { get; init; }
    public string WebhookUrl { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object> Details { get; init; } = new();
}

/// <summary>
/// Represents Make.com scenario configuration.
/// </summary>
public sealed record MakeScenarioConfiguration
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string WebhookUrl { get; init; } = string.Empty;
    public Dictionary<string, object> ScenarioData { get; init; } = new();
    public bool IsActive { get; init; } = true;
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a Make.com scenario.
/// </summary>
public sealed record MakeScenario
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastExecutedAt { get; init; }
    public int ExecutionCount { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents Make.com scenario result.
/// </summary>
public sealed record MakeScenarioResult
{
    public bool IsSuccess { get; init; }
    public string ExecutionId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object> Output { get; init; } = new();
}

/// <summary>
/// Represents Make.com webhook configuration.
/// </summary>
public sealed record MakeWebhookConfiguration
{
    public string ScenarioId { get; init; } = string.Empty;
    public string ModuleName { get; init; } = string.Empty;
    public string WebhookPath { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = "POST";
    public Dictionary<string, object> ModuleData { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents Make.com webhook result.
/// </summary>
public sealed record MakeWebhookResult
{
    public bool IsSuccess { get; init; }
    public string WebhookUrl { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object> Details { get; init; } = new();
}

/// <summary>
/// Represents MuleSoft configuration.
/// </summary>
public sealed record MuleSoftConfiguration
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public Dictionary<string, string> Headers { get; init; } = new();
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a MuleSoft application.
/// </summary>
public sealed record MuleSoftApplication
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastDeployedAt { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Represents MuleSoft result.
/// </summary>
public sealed record MuleSoftResult
{
    public bool IsSuccess { get; init; }
    public string MessageId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object> Response { get; init; } = new();
}