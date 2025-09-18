using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Collections.Concurrent;

namespace NotifyX.Core.Services;

/// <summary>
/// MCP (Model Context Protocol) service implementation.
/// </summary>
public class MCPService : IMCPService
{
    private readonly ILogger<MCPService> _logger;
    private readonly MCPServiceOptions _options;
    private readonly ConcurrentDictionary<string, MCPTool> _registeredTools = new();

    public MCPService(ILogger<MCPService> logger, MCPServiceOptions options)
    {
        _logger = logger;
        _options = options;
        InitializeDefaultTools();
    }

    public async Task<IEnumerable<MCPTool>> GetAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting available MCP tools");

            // Simulate network call to MCP server
            await Task.Delay(100, cancellationToken);

            return _registeredTools.Values.Where(t => t.IsEnabled).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available MCP tools");
            return new List<MCPTool>();
        }
    }

    public async Task<MCPToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("Executing MCP tool: {ToolName}", toolName);

            if (!_registeredTools.TryGetValue(toolName, out var tool))
            {
                return new MCPToolResult
                {
                    IsSuccess = false,
                    ToolName = toolName,
                    ErrorMessage = $"Tool '{toolName}' not found",
                    ExecutionTime = DateTime.UtcNow - startTime
                };
            }

            if (!tool.IsEnabled)
            {
                return new MCPToolResult
                {
                    IsSuccess = false,
                    ToolName = toolName,
                    ErrorMessage = $"Tool '{toolName}' is disabled",
                    ExecutionTime = DateTime.UtcNow - startTime
                };
            }

            // Validate parameters
            var validationResult = ValidateParameters(tool, parameters);
            if (!validationResult.IsValid)
            {
                return new MCPToolResult
                {
                    IsSuccess = false,
                    ToolName = toolName,
                    ErrorMessage = $"Parameter validation failed: {validationResult.ErrorMessage}",
                    ExecutionTime = DateTime.UtcNow - startTime
                };
            }

            // Execute the tool
            var result = await ExecuteToolInternalAsync(tool, parameters, cancellationToken);
            var executionTime = DateTime.UtcNow - startTime;

            return new MCPToolResult
            {
                IsSuccess = true,
                ToolName = toolName,
                Result = result,
                ExecutionTime = executionTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute MCP tool: {ToolName}", toolName);
            return new MCPToolResult
            {
                IsSuccess = false,
                ToolName = toolName,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }
    }

    public async Task<bool> RegisterToolAsync(MCPTool tool, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registering MCP tool: {ToolName}", tool.Name);

            if (string.IsNullOrEmpty(tool.Name))
            {
                _logger.LogWarning("Cannot register tool with empty name");
                return false;
            }

            _registeredTools.AddOrUpdate(tool.Name, tool, (key, existing) => tool);

            // Simulate registration with MCP server
            await Task.Delay(50, cancellationToken);

            _logger.LogInformation("Successfully registered MCP tool: {ToolName}", tool.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register MCP tool: {ToolName}", tool.Name);
            return false;
        }
    }

    public async Task<bool> UnregisterToolAsync(string toolName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Unregistering MCP tool: {ToolName}", toolName);

            if (!_registeredTools.TryRemove(toolName, out var removedTool))
            {
                _logger.LogWarning("Tool {ToolName} was not registered", toolName);
                return false;
            }

            // Simulate unregistration with MCP server
            await Task.Delay(50, cancellationToken);

            _logger.LogInformation("Successfully unregistered MCP tool: {ToolName}", toolName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister MCP tool: {ToolName}", toolName);
            return false;
        }
    }

    public async Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate health check with MCP server
            await Task.Delay(100, cancellationToken);

            if (string.IsNullOrEmpty(_options.ServerUrl))
            {
                return HealthStatus.Unhealthy;
            }

            return HealthStatus.Healthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MCP service health status");
            return HealthStatus.Unhealthy;
        }
    }

    private void InitializeDefaultTools()
    {
        // Register default NotifyX tools
        var tools = new[]
        {
            new MCPTool
            {
                Name = "send_notification",
                Description = "Sends a notification through the NotifyX platform",
                Version = "1.0.0",
                Parameters = new Dictionary<string, MCPToolParameter>
                {
                    ["tenant_id"] = new MCPToolParameter
                    {
                        Name = "tenant_id",
                        Type = "string",
                        Description = "The tenant ID",
                        IsRequired = true
                    },
                    ["event_type"] = new MCPToolParameter
                    {
                        Name = "event_type",
                        Type = "string",
                        Description = "The type of event",
                        IsRequired = true
                    },
                    ["subject"] = new MCPToolParameter
                    {
                        Name = "subject",
                        Type = "string",
                        Description = "The notification subject",
                        IsRequired = true
                    },
                    ["content"] = new MCPToolParameter
                    {
                        Name = "content",
                        Type = "string",
                        Description = "The notification content",
                        IsRequired = true
                    },
                    ["recipients"] = new MCPToolParameter
                    {
                        Name = "recipients",
                        Type = "array",
                        Description = "List of recipient email addresses",
                        IsRequired = true
                    }
                },
                ReturnType = "object"
            },
            new MCPTool
            {
                Name = "get_notification_status",
                Description = "Gets the status of a notification",
                Version = "1.0.0",
                Parameters = new Dictionary<string, MCPToolParameter>
                {
                    ["notification_id"] = new MCPToolParameter
                    {
                        Name = "notification_id",
                        Type = "string",
                        Description = "The notification ID",
                        IsRequired = true
                    }
                },
                ReturnType = "object"
            },
            new MCPTool
            {
                Name = "create_rule",
                Description = "Creates a new notification rule",
                Version = "1.0.0",
                Parameters = new Dictionary<string, MCPToolParameter>
                {
                    ["tenant_id"] = new MCPToolParameter
                    {
                        Name = "tenant_id",
                        Type = "string",
                        Description = "The tenant ID",
                        IsRequired = true
                    },
                    ["name"] = new MCPToolParameter
                    {
                        Name = "name",
                        Type = "string",
                        Description = "The rule name",
                        IsRequired = true
                    },
                    ["condition"] = new MCPToolParameter
                    {
                        Name = "condition",
                        Type = "object",
                        Description = "The rule condition",
                        IsRequired = true
                    },
                    ["actions"] = new MCPToolParameter
                    {
                        Name = "actions",
                        Type = "array",
                        Description = "The rule actions",
                        IsRequired = true
                    }
                },
                ReturnType = "object"
            },
            new MCPTool
            {
                Name = "analyze_notifications",
                Description = "Analyzes notification patterns and provides insights",
                Version = "1.0.0",
                Parameters = new Dictionary<string, MCPToolParameter>
                {
                    ["tenant_id"] = new MCPToolParameter
                    {
                        Name = "tenant_id",
                        Type = "string",
                        Description = "The tenant ID",
                        IsRequired = true
                    },
                    ["time_range"] = new MCPToolParameter
                    {
                        Name = "time_range",
                        Type = "string",
                        Description = "Time range for analysis (e.g., '7d', '30d')",
                        IsRequired = false,
                        DefaultValue = "7d"
                    }
                },
                ReturnType = "object"
            }
        };

        foreach (var tool in tools)
        {
            _registeredTools.TryAdd(tool.Name, tool);
        }

        _logger.LogInformation("Initialized {Count} default MCP tools", tools.Length);
    }

    private static (bool IsValid, string ErrorMessage) ValidateParameters(MCPTool tool, Dictionary<string, object> parameters)
    {
        foreach (var paramDef in tool.Parameters.Values)
        {
            if (paramDef.IsRequired && !parameters.ContainsKey(paramDef.Name))
            {
                return (false, $"Required parameter '{paramDef.Name}' is missing");
            }

            if (parameters.ContainsKey(paramDef.Name))
            {
                var value = parameters[paramDef.Name];
                if (!IsValidParameterType(value, paramDef.Type))
                {
                    return (false, $"Parameter '{paramDef.Name}' has invalid type. Expected: {paramDef.Type}");
                }
            }
        }

        return (true, string.Empty);
    }

    private static bool IsValidParameterType(object value, string expectedType)
    {
        return expectedType.ToLower() switch
        {
            "string" => value is string,
            "number" => value is int or long or double or decimal,
            "boolean" => value is bool,
            "array" => value is System.Collections.IEnumerable,
            "object" => value is Dictionary<string, object> or object,
            _ => true // Unknown types are considered valid
        };
    }

    private async Task<object> ExecuteToolInternalAsync(MCPTool tool, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        // Simulate tool execution
        await Task.Delay(200, cancellationToken);

        return tool.Name switch
        {
            "send_notification" => ExecuteSendNotificationTool(parameters),
            "get_notification_status" => ExecuteGetNotificationStatusTool(parameters),
            "create_rule" => ExecuteCreateRuleTool(parameters),
            "analyze_notifications" => ExecuteAnalyzeNotificationsTool(parameters),
            _ => new { error = $"Unknown tool: {tool.Name}" }
        };
    }

    private static object ExecuteSendNotificationTool(Dictionary<string, object> parameters)
    {
        return new
        {
            success = true,
            notification_id = Guid.NewGuid().ToString(),
            message = "Notification sent successfully",
            timestamp = DateTime.UtcNow
        };
    }

    private static object ExecuteGetNotificationStatusTool(Dictionary<string, object> parameters)
    {
        return new
        {
            notification_id = parameters.GetValueOrDefault("notification_id")?.ToString(),
            status = "delivered",
            delivered_at = DateTime.UtcNow.AddMinutes(-5),
            delivery_attempts = 1
        };
    }

    private static object ExecuteCreateRuleTool(Dictionary<string, object> parameters)
    {
        return new
        {
            success = true,
            rule_id = Guid.NewGuid().ToString(),
            message = "Rule created successfully",
            timestamp = DateTime.UtcNow
        };
    }

    private static object ExecuteAnalyzeNotificationsTool(Dictionary<string, object> parameters)
    {
        return new
        {
            analysis_id = Guid.NewGuid().ToString(),
            total_notifications = 1250,
            delivery_rate = 0.95,
            average_delivery_time = "2.3 seconds",
            top_event_types = new[] { "user.login", "system.alert", "notification.sent" },
            insights = new[]
            {
                "Email delivery rate is 98%",
                "SMS delivery rate is 92%",
                "Peak notification time is 9-10 AM"
            },
            recommendations = new[]
            {
                "Consider optimizing SMS delivery for better rates",
                "Implement rate limiting during peak hours"
            }
        };
    }
}