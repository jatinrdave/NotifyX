using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Text.RegularExpressions;

namespace NotifyX.Core.Services;

/// <summary>
/// Advanced template service implementation with rich formatting and conditional content.
/// </summary>
public class AdvancedTemplateService : IAdvancedTemplateService
{
    private readonly ILogger<AdvancedTemplateService> _logger;
    private readonly ITemplateService _baseTemplateService;
    private readonly Dictionary<string, NotificationTemplate> _templates = new();

    public AdvancedTemplateService(ILogger<AdvancedTemplateService> logger, ITemplateService baseTemplateService)
    {
        _logger = logger;
        _baseTemplateService = baseTemplateService;
    }

    public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        return await _baseTemplateService.CreateTemplateAsync(template, cancellationToken);
    }

    public async Task<NotificationTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        return await _baseTemplateService.GetTemplateAsync(templateId, cancellationToken);
    }

    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        return await _baseTemplateService.GetTemplatesAsync(tenantId, cancellationToken);
    }

    public async Task<NotificationTemplate> UpdateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        return await _baseTemplateService.UpdateTemplateAsync(template, cancellationToken);
    }

    public async Task<bool> DeleteTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        return await _baseTemplateService.DeleteTemplateAsync(templateId, cancellationToken);
    }

    public async Task<TemplateRenderResult> RenderTemplateAsync(NotificationEvent notification, string templateId, CancellationToken cancellationToken = default)
    {
        return await _baseTemplateService.RenderTemplateAsync(notification, templateId, cancellationToken);
    }

    public async Task<NotificationTemplate> CreateRichTemplateAsync(RichTemplateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating rich template: {TemplateName}", request.Name);

            var template = new NotificationTemplate
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "default", // This should come from context
                Name = request.Name,
                Description = request.Description,
                Channel = request.Channel,
                Subject = request.Subject,
                Content = request.Content,
                Variables = request.Variables,
                Metadata = new Dictionary<string, object>(request.Metadata)
                {
                    ["isRichTemplate"] = true,
                    ["blocks"] = request.Blocks,
                    ["conditions"] = request.Conditions,
                    ["style"] = request.Style
                }
            };

            _templates[template.Id] = template;
            _logger.LogInformation("Created rich template: {TemplateId}", template.Id);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create rich template: {TemplateName}", request.Name);
            throw;
        }
    }

    public async Task<TemplateRenderResult> RenderWithTimezoneAsync(NotificationEvent notification, string templateId, string timezone, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Rendering template with timezone: {TemplateId}, {Timezone}", templateId, timezone);

            var template = await GetTemplateAsync(templateId, cancellationToken);
            if (template == null)
            {
                return new TemplateRenderResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Template {templateId} not found"
                };
            }

            // Add timezone information to notification metadata
            var timezoneAwareNotification = notification with
            {
                Metadata = new Dictionary<string, object>(notification.Metadata)
                {
                    ["timezone"] = timezone,
                    ["localTime"] = GetLocalTime(DateTime.UtcNow, timezone)
                }
            };

            var result = await RenderTemplateAsync(timezoneAwareNotification, templateId, cancellationToken);
            
            if (result.IsSuccess)
            {
                // Apply timezone-specific formatting
                result.RenderedSubject = FormatWithTimezone(result.RenderedSubject, timezone);
                result.RenderedContent = FormatWithTimezone(result.RenderedContent, timezone);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template with timezone: {TemplateId}", templateId);
            return new TemplateRenderResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TemplateRenderResult> RenderWithConditionsAsync(NotificationEvent notification, string templateId, Dictionary<string, object> conditions, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Rendering template with conditions: {TemplateId}", templateId);

            var template = await GetTemplateAsync(templateId, cancellationToken);
            if (template == null)
            {
                return new TemplateRenderResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Template {templateId} not found"
                };
            }

            // Add conditions to notification metadata
            var conditionalNotification = notification with
            {
                Metadata = new Dictionary<string, object>(notification.Metadata)
                {
                    ["conditions"] = conditions
                }
            };

            var result = await RenderTemplateAsync(conditionalNotification, templateId, cancellationToken);
            
            if (result.IsSuccess)
            {
                // Apply conditional content
                result.RenderedSubject = ApplyConditionalContent(result.RenderedSubject, conditions);
                result.RenderedContent = ApplyConditionalContent(result.RenderedContent, conditions);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template with conditions: {TemplateId}", templateId);
            return new TemplateRenderResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TemplateRenderResult> RenderWithDynamicBlocksAsync(NotificationEvent notification, string templateId, Dictionary<string, object> dynamicData, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Rendering template with dynamic blocks: {TemplateId}", templateId);

            var template = await GetTemplateAsync(templateId, cancellationToken);
            if (template == null)
            {
                return new TemplateRenderResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Template {templateId} not found"
                };
            }

            // Add dynamic data to notification metadata
            var dynamicNotification = notification with
            {
                Metadata = new Dictionary<string, object>(notification.Metadata)
                {
                    ["dynamicData"] = dynamicData
                }
            };

            var result = await RenderTemplateAsync(dynamicNotification, templateId, cancellationToken);
            
            if (result.IsSuccess)
            {
                // Apply dynamic blocks
                result.RenderedSubject = ApplyDynamicBlocks(result.RenderedSubject, dynamicData);
                result.RenderedContent = ApplyDynamicBlocks(result.RenderedContent, dynamicData);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template with dynamic blocks: {TemplateId}", templateId);
            return new TemplateRenderResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<TemplateValidationResult> ValidateTemplateAsync(string templateContent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Validating template content");

            var errors = new List<TemplateValidationError>();
            var warnings = new List<TemplateValidationWarning>();

            // Check for basic syntax
            if (string.IsNullOrWhiteSpace(templateContent))
            {
                errors.Add(new TemplateValidationError
                {
                    Type = "EmptyContent",
                    Message = "Template content cannot be empty",
                    Line = 1,
                    Column = 1,
                    Code = "TEMPLATE_001"
                });
            }

            // Check for unclosed variables
            var variablePattern = @"\{\{([^}]*)\}\}";
            var matches = Regex.Matches(templateContent, variablePattern);
            foreach (Match match in matches)
            {
                if (match.Groups[1].Value.Contains("{{") || match.Groups[1].Value.Contains("}}"))
                {
                    errors.Add(new TemplateValidationError
                    {
                        Type = "InvalidVariable",
                        Message = $"Invalid variable syntax: {match.Value}",
                        Line = GetLineNumber(templateContent, match.Index),
                        Column = GetColumnNumber(templateContent, match.Index),
                        Code = "TEMPLATE_002"
                    });
                }
            }

            // Check for conditional syntax
            var conditionalPattern = @"\{\%\s*if\s+([^%]*)\s*\%\}";
            var conditionalMatches = Regex.Matches(templateContent, conditionalPattern);
            foreach (Match match in conditionalMatches)
            {
                var endPattern = @"\{\%\s*endif\s*\%\}";
                if (!Regex.IsMatch(templateContent, endPattern))
                {
                    warnings.Add(new TemplateValidationWarning
                    {
                        Type = "MissingEndIf",
                        Message = "Conditional block may be missing endif",
                        Line = GetLineNumber(templateContent, match.Index),
                        Column = GetColumnNumber(templateContent, match.Index),
                        Code = "TEMPLATE_003"
                    });
                }
            }

            return new TemplateValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate template");
            return new TemplateValidationResult
            {
                IsValid = false,
                Errors = new List<TemplateValidationError>
                {
                    new TemplateValidationError
                    {
                        Type = "ValidationError",
                        Message = $"Template validation failed: {ex.Message}",
                        Code = "TEMPLATE_000"
                    }
                }
            };
        }
    }

    public async Task<TemplatePreviewResult> GetTemplatePreviewAsync(string templateId, Dictionary<string, object> sampleData, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting template preview: {TemplateId}", templateId);

            var template = await GetTemplateAsync(templateId, cancellationToken);
            if (template == null)
            {
                return new TemplatePreviewResult
                {
                    IsSuccess = false
                };
            }

            // Create sample notification
            var sampleNotification = new NotificationEvent
            {
                Id = "preview",
                TenantId = "preview",
                EventType = "preview",
                Priority = NotificationPriority.Normal,
                Subject = "Preview Subject",
                Content = "Preview Content",
                Recipients = new List<NotificationRecipient>
                {
                    new NotificationRecipient
                    {
                        Id = "preview-recipient",
                        Name = "Preview User",
                        Email = "preview@example.com"
                    }
                },
                Metadata = sampleData
            };

            var result = await RenderTemplateAsync(sampleNotification, templateId, cancellationToken);
            
            if (result.IsSuccess)
            {
                var usedVariables = ExtractUsedVariables(template.Content);
                var missingVariables = usedVariables.Where(v => !sampleData.ContainsKey(v)).ToList();

                return new TemplatePreviewResult
                {
                    IsSuccess = true,
                    RenderedSubject = result.RenderedSubject,
                    RenderedContent = result.RenderedContent,
                    UsedVariables = usedVariables.ToDictionary(v => v, v => sampleData.GetValueOrDefault(v, "N/A")),
                    MissingVariables = missingVariables
                };
            }

            return new TemplatePreviewResult
            {
                IsSuccess = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template preview: {TemplateId}", templateId);
            return new TemplatePreviewResult
            {
                IsSuccess = false
            };
        }
    }

    public async Task<NotificationTemplate> CloneTemplateAsync(string templateId, string newName, Dictionary<string, object> modifications, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Cloning template: {TemplateId} to {NewName}", templateId, newName);

            var originalTemplate = await GetTemplateAsync(templateId, cancellationToken);
            if (originalTemplate == null)
            {
                throw new ArgumentException($"Template {templateId} not found");
            }

            var clonedTemplate = originalTemplate with
            {
                Id = Guid.NewGuid().ToString(),
                Name = newName,
                Description = $"Cloned from {originalTemplate.Name}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Apply modifications
            foreach (var modification in modifications)
            {
                switch (modification.Key.ToLower())
                {
                    case "subject":
                        clonedTemplate = clonedTemplate with { Subject = modification.Value.ToString() ?? clonedTemplate.Subject };
                        break;
                    case "content":
                        clonedTemplate = clonedTemplate with { Content = modification.Value.ToString() ?? clonedTemplate.Content };
                        break;
                    case "description":
                        clonedTemplate = clonedTemplate with { Description = modification.Value.ToString() ?? clonedTemplate.Description };
                        break;
                    case "channel":
                        if (modification.Value is NotificationChannel channel)
                        {
                            clonedTemplate = clonedTemplate with { Channel = channel };
                        }
                        break;
                }
            }

            _templates[clonedTemplate.Id] = clonedTemplate;
            _logger.LogInformation("Cloned template: {OriginalId} to {NewId}", templateId, clonedTemplate.Id);

            return clonedTemplate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clone template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<TemplateUsageStatistics> GetTemplateUsageStatisticsAsync(string templateId, TimeSpan timeRange, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting template usage statistics: {TemplateId}", templateId);

            // Simulate usage statistics
            var statistics = new TemplateUsageStatistics
            {
                TemplateId = templateId,
                TotalUsage = 1250,
                SuccessfulUsage = 1180,
                FailedUsage = 70,
                SuccessRate = 0.944,
                LastUsed = DateTime.UtcNow.AddHours(-2),
                UsageByChannel = new Dictionary<string, int>
                {
                    ["Email"] = 1000,
                    ["SMS"] = 150,
                    ["Push"] = 100
                },
                UsageByPriority = new Dictionary<string, int>
                {
                    ["Normal"] = 800,
                    ["High"] = 300,
                    ["Critical"] = 100,
                    ["Low"] = 50
                }
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template usage statistics: {TemplateId}", templateId);
            throw;
        }
    }

    private static DateTime GetLocalTime(DateTime utcTime, string timezone)
    {
        try
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeZoneInfo);
        }
        catch
        {
            return utcTime; // Fallback to UTC
        }
    }

    private static string FormatWithTimezone(string content, string timezone)
    {
        // Replace timezone placeholders
        content = content.Replace("{{timezone}}", timezone);
        content = content.Replace("{{localTime}}", GetLocalTime(DateTime.UtcNow, timezone).ToString("yyyy-MM-dd HH:mm:ss"));
        
        return content;
    }

    private static string ApplyConditionalContent(string content, Dictionary<string, object> conditions)
    {
        // Simple conditional content replacement
        foreach (var condition in conditions)
        {
            var pattern = $@"\{\%\s*if\s+{condition.Key}\s*\%\}(.*?)\{\%\s*endif\s*\%\}";
            var match = Regex.Match(content, pattern, RegexOptions.Singleline);
            
            if (match.Success)
            {
                var shouldShow = EvaluateCondition(condition.Key, condition.Value);
                if (shouldShow)
                {
                    content = content.Replace(match.Value, match.Groups[1].Value);
                }
                else
                {
                    content = content.Replace(match.Value, string.Empty);
                }
            }
        }

        return content;
    }

    private static string ApplyDynamicBlocks(string content, Dictionary<string, object> dynamicData)
    {
        // Replace dynamic block placeholders
        foreach (var data in dynamicData)
        {
            var pattern = $@"\{\{{\s*{data.Key}\s*\}\}}";
            content = Regex.Replace(content, pattern, data.Value?.ToString() ?? string.Empty);
        }

        return content;
    }

    private static bool EvaluateCondition(string field, object value)
    {
        // Simple condition evaluation
        return value switch
        {
            bool boolValue => boolValue,
            string stringValue => !string.IsNullOrEmpty(stringValue),
            int intValue => intValue > 0,
            _ => value != null
        };
    }

    private static int GetLineNumber(string content, int index)
    {
        return content.Substring(0, index).Split('\n').Length;
    }

    private static int GetColumnNumber(string content, int index)
    {
        var lines = content.Substring(0, index).Split('\n');
        return lines.Last().Length + 1;
    }

    private static List<string> ExtractUsedVariables(string content)
    {
        var variables = new List<string>();
        var pattern = @"\{\{\s*([^}]+)\s*\}\}";
        var matches = Regex.Matches(content, pattern);
        
        foreach (Match match in matches)
        {
            var variable = match.Groups[1].Value.Trim();
            if (!variables.Contains(variable))
            {
                variables.Add(variable);
            }
        }

        return variables;
    }
}