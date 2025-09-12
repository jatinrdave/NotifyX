using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Text.RegularExpressions;

namespace NotifyX.Core.Services;

/// <summary>
/// CLI service implementation for command-line interface operations.
/// </summary>
public class CLIService : ICLIService
{
    private readonly ILogger<CLIService> _logger;
    private readonly INotificationService _notificationService;
    private readonly Dictionary<string, CLICommand> _commands = new();

    public CLIService(ILogger<CLIService> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
        InitializeCommands();
    }

    public async Task<CLICommandResult> ExecuteCommandAsync(string command, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("Executing CLI command: {Command}", command);

            if (!_commands.TryGetValue(command.ToLower(), out var commandDef))
            {
                return new CLICommandResult
                {
                    IsSuccess = false,
                    Command = command,
                    ErrorMessage = $"Unknown command: {command}",
                    ExecutionTime = DateTime.UtcNow - startTime
                };
            }

            // Validate parameters
            var validationResult = ValidateParameters(commandDef, parameters);
            if (!validationResult.IsValid)
            {
                return new CLICommandResult
                {
                    IsSuccess = false,
                    Command = command,
                    ErrorMessage = $"Parameter validation failed: {string.Join(", ", validationResult.Errors)}",
                    ExecutionTime = DateTime.UtcNow - startTime
                };
            }

            // Execute the command
            var result = await ExecuteCommandInternalAsync(commandDef, parameters, cancellationToken);
            var executionTime = DateTime.UtcNow - startTime;

            return new CLICommandResult
            {
                IsSuccess = true,
                Command = command,
                Result = result,
                Output = FormatCommandOutput(result),
                ExecutionTime = executionTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute CLI command: {Command}", command);
            return new CLICommandResult
            {
                IsSuccess = false,
                Command = command,
                ErrorMessage = ex.Message,
                ExecutionTime = DateTime.UtcNow - startTime
            };
        }
    }

    public async Task<IEnumerable<CLICommand>> GetAvailableCommandsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting available CLI commands");

            // Simulate async operation
            await Task.Delay(10, cancellationToken);

            return _commands.Values.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available CLI commands");
            return new List<CLICommand>();
        }
    }

    public async Task<CLICommandHelp> GetCommandHelpAsync(string command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting help for CLI command: {Command}", command);

            if (!_commands.TryGetValue(command.ToLower(), out var commandDef))
            {
                return new CLICommandHelp
                {
                    Command = command,
                    Description = "Command not found",
                    Syntax = "N/A",
                    Examples = new List<string> { "Command not found" }
                };
            }

            // Simulate async operation
            await Task.Delay(10, cancellationToken);

            return new CLICommandHelp
            {
                Command = commandDef.Name,
                Description = commandDef.Description,
                Syntax = commandDef.Syntax,
                Examples = GenerateCommandExamples(commandDef),
                Parameters = commandDef.Parameters,
                Notes = GenerateCommandNotes(commandDef)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get command help for: {Command}", command);
            return new CLICommandHelp
            {
                Command = command,
                Description = "Failed to get help",
                Syntax = "N/A",
                Examples = new List<string> { "Error retrieving help" }
            };
        }
    }

    public async Task<CLIValidationResult> ValidateCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Validating CLI command: {Command}", command);

            var errors = new List<string>();
            var warnings = new List<string>();
            var suggestions = new List<string>();

            // Parse command
            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                errors.Add("Empty command");
                return new CLIValidationResult
                {
                    IsValid = false,
                    Command = command,
                    Errors = errors
                };
            }

            var commandName = parts[0].ToLower();
            
            // Check if command exists
            if (!_commands.ContainsKey(commandName))
            {
                errors.Add($"Unknown command: {commandName}");
                
                // Suggest similar commands
                var similarCommands = _commands.Keys.Where(c => c.Contains(commandName) || commandName.Contains(c)).Take(3);
                suggestions.AddRange(similarCommands.Select(c => $"Did you mean: {c}"));
            }
            else
            {
                var commandDef = _commands[commandName];
                
                // Validate syntax
                var syntaxValidation = ValidateCommandSyntax(command, commandDef);
                errors.AddRange(syntaxValidation.Errors);
                warnings.AddRange(syntaxValidation.Warnings);
            }

            // Simulate async operation
            await Task.Delay(10, cancellationToken);

            return new CLIValidationResult
            {
                IsValid = errors.Count == 0,
                Command = command,
                Errors = errors,
                Warnings = warnings,
                Suggestions = suggestions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate CLI command: {Command}", command);
            return new CLIValidationResult
            {
                IsValid = false,
                Command = command,
                Errors = new List<string> { $"Validation failed: {ex.Message}" }
            };
        }
    }

    private void InitializeCommands()
    {
        var commands = new[]
        {
            new CLICommand
            {
                Name = "send",
                Description = "Send a notification",
                Syntax = "send --tenant <tenant-id> --event <event-type> --subject <subject> --content <content> --recipients <email1,email2>",
                Parameters = new List<CLICommandParameter>
                {
                    new CLICommandParameter { Name = "tenant", Type = "string", Description = "Tenant ID", IsRequired = true },
                    new CLICommandParameter { Name = "event", Type = "string", Description = "Event type", IsRequired = true },
                    new CLICommandParameter { Name = "subject", Type = "string", Description = "Notification subject", IsRequired = true },
                    new CLICommandParameter { Name = "content", Type = "string", Description = "Notification content", IsRequired = true },
                    new CLICommandParameter { Name = "recipients", Type = "string", Description = "Comma-separated recipient emails", IsRequired = true },
                    new CLICommandParameter { Name = "priority", Type = "string", Description = "Notification priority", IsRequired = false, DefaultValue = "Normal" },
                    new CLICommandParameter { Name = "channel", Type = "string", Description = "Notification channel", IsRequired = false, DefaultValue = "Email" }
                },
                Aliases = new List<string> { "notify", "push" }
            },
            new CLICommand
            {
                Name = "status",
                Description = "Get notification status",
                Syntax = "status --id <notification-id>",
                Parameters = new List<CLICommandParameter>
                {
                    new CLICommandParameter { Name = "id", Type = "string", Description = "Notification ID", IsRequired = true }
                },
                Aliases = new List<string> { "check", "info" }
            },
            new CLICommand
            {
                Name = "list",
                Description = "List notifications",
                Syntax = "list --tenant <tenant-id> [--limit <number>] [--status <status>]",
                Parameters = new List<CLICommandParameter>
                {
                    new CLICommandParameter { Name = "tenant", Type = "string", Description = "Tenant ID", IsRequired = true },
                    new CLICommandParameter { Name = "limit", Type = "number", Description = "Maximum number of results", IsRequired = false, DefaultValue = 10 },
                    new CLICommandParameter { Name = "status", Type = "string", Description = "Filter by status", IsRequired = false }
                },
                Aliases = new List<string> { "ls", "show" }
            },
            new CLICommand
            {
                Name = "retry",
                Description = "Retry a failed notification",
                Syntax = "retry --id <notification-id>",
                Parameters = new List<CLICommandParameter>
                {
                    new CLICommandParameter { Name = "id", Type = "string", Description = "Notification ID", IsRequired = true }
                }
            },
            new CLICommand
            {
                Name = "cancel",
                Description = "Cancel a scheduled notification",
                Syntax = "cancel --id <notification-id>",
                Parameters = new List<CLICommandParameter>
                {
                    new CLICommandParameter { Name = "id", Type = "string", Description = "Notification ID", IsRequired = true }
                },
                Aliases = new List<string> { "stop", "abort" }
            },
            new CLICommand
            {
                Name = "health",
                Description = "Check system health",
                Syntax = "health [--detailed]",
                Parameters = new List<CLICommandParameter>
                {
                    new CLICommandParameter { Name = "detailed", Type = "boolean", Description = "Show detailed health information", IsRequired = false, DefaultValue = false }
                },
                Aliases = new List<string> { "ping", "check" }
            },
            new CLICommand
            {
                Name = "stats",
                Description = "Show system statistics",
                Syntax = "stats --tenant <tenant-id> [--period <period>]",
                Parameters = new List<CLICommandParameter>
                {
                    new CLICommandParameter { Name = "tenant", Type = "string", Description = "Tenant ID", IsRequired = true },
                    new CLICommandParameter { Name = "period", Type = "string", Description = "Time period (1h, 24h, 7d, 30d)", IsRequired = false, DefaultValue = "24h" }
                },
                Aliases = new List<string> { "statistics", "metrics" }
            }
        };

        foreach (var command in commands)
        {
            _commands[command.Name.ToLower()] = command;
            
            // Add aliases
            foreach (var alias in command.Aliases)
            {
                _commands[alias.ToLower()] = command;
            }
        }

        _logger.LogInformation("Initialized {Count} CLI commands", commands.Length);
    }

    private static (bool IsValid, List<string> Errors) ValidateParameters(CLICommand command, Dictionary<string, object> parameters)
    {
        var errors = new List<string>();

        foreach (var param in command.Parameters.Where(p => p.IsRequired))
        {
            if (!parameters.ContainsKey(param.Name))
            {
                errors.Add($"Required parameter '{param.Name}' is missing");
            }
        }

        return (errors.Count == 0, errors);
    }

    private async Task<object> ExecuteCommandInternalAsync(CLICommand command, Dictionary<string, object> parameters, CancellationToken cancellationToken)
    {
        // Simulate command execution
        await Task.Delay(100, cancellationToken);

        return command.Name.ToLower() switch
        {
            "send" => ExecuteSendCommand(parameters),
            "status" => ExecuteStatusCommand(parameters),
            "list" => ExecuteListCommand(parameters),
            "retry" => ExecuteRetryCommand(parameters),
            "cancel" => ExecuteCancelCommand(parameters),
            "health" => ExecuteHealthCommand(parameters),
            "stats" => ExecuteStatsCommand(parameters),
            _ => new { error = $"Command '{command.Name}' not implemented" }
        };
    }

    private static object ExecuteSendCommand(Dictionary<string, object> parameters)
    {
        return new
        {
            success = true,
            notification_id = Guid.NewGuid().ToString(),
            message = "Notification sent successfully",
            timestamp = DateTime.UtcNow,
            tenant = parameters.GetValueOrDefault("tenant"),
            event_type = parameters.GetValueOrDefault("event"),
            priority = parameters.GetValueOrDefault("priority", "Normal")
        };
    }

    private static object ExecuteStatusCommand(Dictionary<string, object> parameters)
    {
        return new
        {
            notification_id = parameters.GetValueOrDefault("id"),
            status = "delivered",
            delivered_at = DateTime.UtcNow.AddMinutes(-5),
            delivery_attempts = 1,
            channel = "email"
        };
    }

    private static object ExecuteListCommand(Dictionary<string, object> parameters)
    {
        var limit = parameters.GetValueOrDefault("limit", 10);
        var notifications = new List<object>();

        for (int i = 0; i < Math.Min((int)limit, 5); i++)
        {
            notifications.Add(new
            {
                id = Guid.NewGuid().ToString(),
                event_type = "user.login",
                status = "delivered",
                created_at = DateTime.UtcNow.AddMinutes(-i * 10),
                priority = "Normal"
            });
        }

        return new
        {
            notifications,
            total = notifications.Count,
            tenant = parameters.GetValueOrDefault("tenant")
        };
    }

    private static object ExecuteRetryCommand(Dictionary<string, object> parameters)
    {
        return new
        {
            success = true,
            notification_id = parameters.GetValueOrDefault("id"),
            message = "Notification retry initiated",
            timestamp = DateTime.UtcNow
        };
    }

    private static object ExecuteCancelCommand(Dictionary<string, object> parameters)
    {
        return new
        {
            success = true,
            notification_id = parameters.GetValueOrDefault("id"),
            message = "Notification cancelled",
            timestamp = DateTime.UtcNow
        };
    }

    private static object ExecuteHealthCommand(Dictionary<string, object> parameters)
    {
        var detailed = parameters.GetValueOrDefault("detailed", false);
        
        var health = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            services = new
            {
                notification_service = "healthy",
                queue_service = "healthy",
                template_service = "healthy"
            }
        };

        if ((bool)detailed)
        {
            return new
            {
                health,
                metrics = new
                {
                    uptime = "99.9%",
                    response_time = "45ms",
                    throughput = "150 notifications/second"
                }
            };
        }

        return health;
    }

    private static object ExecuteStatsCommand(Dictionary<string, object> parameters)
    {
        return new
        {
            tenant = parameters.GetValueOrDefault("tenant"),
            period = parameters.GetValueOrDefault("period", "24h"),
            statistics = new
            {
                total_notifications = 1250,
                delivered = 1180,
                failed = 70,
                success_rate = 0.944,
                average_delivery_time = "2.3 seconds"
            },
            timestamp = DateTime.UtcNow
        };
    }

    private static string FormatCommandOutput(object result)
    {
        return result switch
        {
            var obj when obj.GetType().GetProperty("success")?.GetValue(obj)?.Equals(true) == true => "✅ Command executed successfully",
            var obj when obj.GetType().GetProperty("error") != null => $"❌ Error: {obj.GetType().GetProperty("error")?.GetValue(obj)}",
            _ => "ℹ️ Command completed"
        };
    }

    private static List<string> GenerateCommandExamples(CLICommand command)
    {
        return command.Name.ToLower() switch
        {
            "send" => new List<string>
            {
                "send --tenant tenant123 --event user.login --subject \"Welcome!\" --content \"Welcome to our platform\" --recipients user@example.com",
                "send --tenant tenant123 --event system.alert --subject \"Alert\" --content \"System alert\" --recipients admin@example.com --priority High"
            },
            "status" => new List<string>
            {
                "status --id notification-123"
            },
            "list" => new List<string>
            {
                "list --tenant tenant123",
                "list --tenant tenant123 --limit 20 --status delivered"
            },
            "retry" => new List<string>
            {
                "retry --id notification-123"
            },
            "cancel" => new List<string>
            {
                "cancel --id notification-123"
            },
            "health" => new List<string>
            {
                "health",
                "health --detailed"
            },
            "stats" => new List<string>
            {
                "stats --tenant tenant123",
                "stats --tenant tenant123 --period 7d"
            },
            _ => new List<string> { $"Usage: {command.Syntax}" }
        };
    }

    private static List<string> GenerateCommandNotes(CLICommand command)
    {
        var notes = new List<string>();

        if (command.RequiresAuthentication)
        {
            notes.Add("This command requires authentication");
        }

        if (command.RequiredPermissions.Any())
        {
            notes.Add($"Required permissions: {string.Join(", ", command.RequiredPermissions)}");
        }

        if (command.Aliases.Any())
        {
            notes.Add($"Aliases: {string.Join(", ", command.Aliases)}");
        }

        return notes;
    }

    private static (List<string> Errors, List<string> Warnings) ValidateCommandSyntax(string command, CLICommand commandDef)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Basic syntax validation
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 1)
        {
            errors.Add("Empty command");
            return (errors, warnings);
        }

        // Check for required parameters
        var hasRequiredParams = commandDef.Parameters.Where(p => p.IsRequired).All(p => 
            command.Contains($"--{p.Name}") || command.Contains($"-{p.ShortName}"));

        if (!hasRequiredParams)
        {
            var missingParams = commandDef.Parameters.Where(p => p.IsRequired && 
                !command.Contains($"--{p.Name}") && !command.Contains($"-{p.ShortName}"));
            errors.Add($"Missing required parameters: {string.Join(", ", missingParams.Select(p => p.Name))}");
        }

        return (errors, warnings);
    }
}