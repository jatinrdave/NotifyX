using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NotifyX.Core.Services;

/// <summary>
/// Default implementation of the notification template service.
/// Handles template rendering, variable interpolation, and multi-language support.
/// </summary>
public sealed class TemplateService : ITemplateService
{
    private readonly ILogger<TemplateService> _logger;
    private readonly Dictionary<string, NotificationTemplate> _templates = new();
    private readonly object _templatesLock = new();

    /// <summary>
    /// Initializes a new instance of the TemplateService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TemplateService(ILogger<TemplateService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TemplateRenderResult> RenderAsync(
        NotificationEvent notification, 
        NotificationTemplate template, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Rendering template {TemplateId} for notification {NotificationId}", 
                template.Id, notification.Id);

            // Validate template variables
            var validationResult = await ValidateAsync(template, notification, cancellationToken);
            if (!validationResult.IsValid)
            {
                return TemplateRenderResult.Failure(
                    $"Template validation failed: {string.Join(", ", validationResult.Errors)}");
            }

            // Prepare template variables
            var templateVariables = PrepareTemplateVariables(template, notification);

            // Render content based on template engine
            var renderedContent = await RenderContentAsync(template, templateVariables, cancellationToken);
            var renderedSubject = await RenderSubjectAsync(template, templateVariables, cancellationToken);
            var renderedAttachments = await RenderAttachmentsAsync(template, templateVariables, cancellationToken);

            _logger.LogDebug("Successfully rendered template {TemplateId} for notification {NotificationId}", 
                template.Id, notification.Id);

            return TemplateRenderResult.Success(renderedContent, renderedSubject, renderedAttachments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateId} for notification {NotificationId}", 
                template.Id, notification.Id);
            return TemplateRenderResult.Failure($"Template rendering failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<TemplateRenderResult> RenderAsync(
        NotificationEvent notification, 
        string templateId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await GetTemplateAsync(templateId, cancellationToken);
            if (template == null)
            {
                return TemplateRenderResult.Failure($"Template {templateId} not found");
            }

            return await RenderAsync(notification, template, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateId} for notification {NotificationId}", 
                templateId, notification.Id);
            return TemplateRenderResult.Failure($"Template rendering failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<TemplateValidationResult> ValidateAsync(
        NotificationTemplate template, 
        NotificationEvent notification, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Validating template {TemplateId} against notification {NotificationId}", 
                template.Id, notification.Id);

            var errors = new List<string>();
            var warnings = new List<string>();
            var missingVariables = new List<string>();
            var unusedVariables = new List<string>();

            // Check required variables
            foreach (var variable in template.Variables.Values.Where(v => v.IsRequired))
            {
                if (!HasVariableValue(notification, variable.Name))
                {
                    missingVariables.Add(variable.Name);
                    errors.Add($"Required variable '{variable.Name}' is missing");
                }
            }

            // Check variable validations
            foreach (var variable in template.Variables.Values)
            {
                if (HasVariableValue(notification, variable.Name))
                {
                    var value = GetVariableValue(notification, variable.Name);
                    var validationErrors = ValidateVariableValue(variable, value);
                    errors.AddRange(validationErrors);
                }
            }

            // Check for unused variables in template content
            var usedVariables = ExtractUsedVariables(template);
            foreach (var variableName in template.Variables.Keys)
            {
                if (!usedVariables.Contains(variableName))
                {
                    unusedVariables.Add(variableName);
                    warnings.Add($"Variable '{variableName}' is defined but not used in template");
                }
            }

            // Check for undefined variables in template content
            foreach (var usedVariable in usedVariables)
            {
                if (!template.Variables.ContainsKey(usedVariable))
                {
                    warnings.Add($"Variable '{usedVariable}' is used in template but not defined");
                }
            }

            var isValid = !errors.Any();
            _logger.LogDebug("Template validation completed for template {TemplateId}. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}", 
                template.Id, isValid, errors.Count, warnings.Count);

            return isValid 
                ? TemplateValidationResult.Success(warnings)
                : TemplateValidationResult.Failure(errors, warnings, missingVariables, unusedVariables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template {TemplateId}", template.Id);
            return TemplateValidationResult.Failure(new List<string> { $"Validation failed: {ex.Message}" });
        }
    }

    /// <inheritdoc />
    public async Task<NotificationTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting template {TemplateId}", templateId);

            lock (_templatesLock)
            {
                _templates.TryGetValue(templateId, out var template);
                return template;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {TemplateId}", templateId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(
        string tenantId, 
        NotificationChannel channel, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting templates for tenant {TenantId} and channel {Channel}", tenantId, channel);

            lock (_templatesLock)
            {
                var templates = _templates.Values
                    .Where(t => t.TenantId == tenantId && t.Channel == channel && t.IsActive)
                    .ToList();
                
                _logger.LogDebug("Found {TemplateCount} templates for tenant {TenantId} and channel {Channel}", 
                    templates.Count, tenantId, channel);
                
                return templates;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates for tenant {TenantId} and channel {Channel}", tenantId, channel);
            return Enumerable.Empty<NotificationTemplate>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(
        string tenantId, 
        NotificationChannel channel, 
        string eventType, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting templates for tenant {TenantId}, channel {Channel}, and event type {EventType}", 
                tenantId, channel, eventType);

            lock (_templatesLock)
            {
                var templates = _templates.Values
                    .Where(t => t.TenantId == tenantId && 
                               t.Channel == channel && 
                               t.EventType == eventType && 
                               t.IsActive)
                    .ToList();
                
                _logger.LogDebug("Found {TemplateCount} templates for tenant {TenantId}, channel {Channel}, and event type {EventType}", 
                    templates.Count, tenantId, channel, eventType);
                
                return templates;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates for tenant {TenantId}, channel {Channel}, and event type {EventType}", 
                tenantId, channel, eventType);
            return Enumerable.Empty<NotificationTemplate>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> AddTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Adding template {TemplateId} for tenant {TenantId}", template.Id, template.TenantId);

            lock (_templatesLock)
            {
                _templates[template.Id] = template;
            }

            _logger.LogInformation("Successfully added template {TemplateId} for tenant {TenantId}", 
                template.Id, template.TenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding template {TemplateId} for tenant {TenantId}", 
                template.Id, template.TenantId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating template {TemplateId} for tenant {TenantId}", template.Id, template.TenantId);

            lock (_templatesLock)
            {
                if (_templates.ContainsKey(template.Id))
                {
                    _templates[template.Id] = template;
                    _logger.LogInformation("Successfully updated template {TemplateId} for tenant {TenantId}", 
                        template.Id, template.TenantId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Template {TemplateId} not found for update", template.Id);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId} for tenant {TenantId}", 
                template.Id, template.TenantId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Removing template {TemplateId}", templateId);

            lock (_templatesLock)
            {
                var removed = _templates.Remove(templateId);
                if (removed)
                {
                    _logger.LogInformation("Successfully removed template {TemplateId}", templateId);
                }
                else
                {
                    _logger.LogWarning("Template {TemplateId} not found for removal", templateId);
                }
                return removed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing template {TemplateId}", templateId);
            return false;
        }
    }

    /// <summary>
    /// Prepares template variables by combining template defaults with notification data.
    /// </summary>
    private Dictionary<string, object> PrepareTemplateVariables(NotificationTemplate template, NotificationEvent notification)
    {
        var variables = new Dictionary<string, object>();

        // Add template variable defaults
        foreach (var variable in template.Variables)
        {
            if (variable.Value.DefaultValue != null)
            {
                variables[variable.Key] = variable.Value.DefaultValue;
            }
        }

        // Add notification template variables
        foreach (var variable in notification.TemplateVariables)
        {
            variables[variable.Key] = variable.Value;
        }

        // Add standard notification properties
        variables["NotificationId"] = notification.Id;
        variables["EventType"] = notification.EventType;
        variables["Priority"] = notification.Priority.ToString();
        variables["Subject"] = notification.Subject;
        variables["Content"] = notification.Content;
        variables["CreatedAt"] = notification.CreatedAt;
        variables["TenantId"] = notification.TenantId;
        variables["Source"] = notification.Source ?? "";
        variables["CorrelationId"] = notification.CorrelationId ?? "";

        // Add recipient information
        if (notification.Recipients.Any())
        {
            variables["Recipients"] = notification.Recipients.Select(r => new
            {
                r.Id,
                r.Name,
                r.Email,
                r.PhoneNumber,
                r.Language,
                r.TimeZone
            }).ToArray();

            // Add first recipient details for convenience
            var firstRecipient = notification.Recipients.First();
            variables["RecipientName"] = firstRecipient.Name;
            variables["RecipientEmail"] = firstRecipient.Email ?? "";
            variables["RecipientPhone"] = firstRecipient.PhoneNumber ?? "";
        }

        // Add metadata
        foreach (var metadata in notification.Metadata)
        {
            variables[$"Metadata_{metadata.Key}"] = metadata.Value;
        }

        return variables;
    }

    /// <summary>
    /// Renders template content using the specified template engine.
    /// </summary>
    private async Task<string> RenderContentAsync(
        NotificationTemplate template, 
        Dictionary<string, object> variables, 
        CancellationToken cancellationToken)
    {
        return template.Engine switch
        {
            TemplateEngine.Mustache => RenderMustacheTemplate(template.Content, variables),
            TemplateEngine.Handlebars => RenderHandlebarsTemplate(template.Content, variables),
            TemplateEngine.Razor => await RenderRazorTemplateAsync(template.Content, variables, cancellationToken),
            TemplateEngine.Simple => RenderSimpleTemplate(template.Content, variables),
            TemplateEngine.Custom => await RenderCustomTemplateAsync(template, variables, cancellationToken),
            _ => throw new NotSupportedException($"Template engine {template.Engine} is not supported")
        };
    }

    /// <summary>
    /// Renders template subject using the specified template engine.
    /// </summary>
    private async Task<string?> RenderSubjectAsync(
        NotificationTemplate template, 
        Dictionary<string, object> variables, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(template.Subject))
            return null;

        return template.Engine switch
        {
            TemplateEngine.Mustache => RenderMustacheTemplate(template.Subject, variables),
            TemplateEngine.Handlebars => RenderHandlebarsTemplate(template.Subject, variables),
            TemplateEngine.Razor => await RenderRazorTemplateAsync(template.Subject, variables, cancellationToken),
            TemplateEngine.Simple => RenderSimpleTemplate(template.Subject, variables),
            TemplateEngine.Custom => await RenderCustomTemplateAsync(template, variables, cancellationToken),
            _ => throw new NotSupportedException($"Template engine {template.Engine} is not supported")
        };
    }

    /// <summary>
    /// Renders template attachments.
    /// </summary>
    private async Task<List<RenderedAttachment>> RenderAttachmentsAsync(
        NotificationTemplate template, 
        Dictionary<string, object> variables, 
        CancellationToken cancellationToken)
    {
        var renderedAttachments = new List<RenderedAttachment>();

        foreach (var attachment in template.Attachments)
        {
            try
            {
                var renderedAttachment = new RenderedAttachment
                {
                    Name = RenderSimpleTemplate(attachment.Name, variables),
                    ContentType = attachment.ContentType,
                    Content = attachment.Content, // Attachments are typically not templated
                    IsInline = attachment.IsInline,
                    ContentId = attachment.ContentId,
                    Metadata = attachment.Metadata
                };

                renderedAttachments.Add(renderedAttachment);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error rendering attachment {AttachmentName} for template {TemplateId}", 
                    attachment.Name, template.Id);
            }
        }

        return renderedAttachments;
    }

    /// <summary>
    /// Renders a Mustache template.
    /// </summary>
    private string RenderMustacheTemplate(string template, Dictionary<string, object> variables)
    {
        // Simple Mustache-like template rendering
        // This is a basic implementation - in production, you'd use a proper Mustache library
        var result = template;

        foreach (var variable in variables)
        {
            var placeholder = $"{{{{{variable.Key}}}}}";
            var value = variable.Value?.ToString() ?? "";
            result = result.Replace(placeholder, value);
        }

        return result;
    }

    /// <summary>
    /// Renders a Handlebars template.
    /// </summary>
    private string RenderHandlebarsTemplate(string template, Dictionary<string, object> variables)
    {
        // Simple Handlebars-like template rendering
        // This is a basic implementation - in production, you'd use a proper Handlebars library
        var result = template;

        foreach (var variable in variables)
        {
            var placeholder = $"{{{{{{{variable.Key}}}}}}}";
            var value = variable.Value?.ToString() ?? "";
            result = result.Replace(placeholder, value);
        }

        return result;
    }

    /// <summary>
    /// Renders a Razor template.
    /// </summary>
    private async Task<string> RenderRazorTemplateAsync(
        string template, 
        Dictionary<string, object> variables, 
        CancellationToken cancellationToken)
    {
        // This is a placeholder implementation
        // In production, you'd use the Razor templating engine
        await Task.Delay(1, cancellationToken); // Simulate async work
        
        // For now, fall back to simple template rendering
        return RenderSimpleTemplate(template, variables);
    }

    /// <summary>
    /// Renders a simple template with basic variable substitution.
    /// </summary>
    private string RenderSimpleTemplate(string template, Dictionary<string, object> variables)
    {
        var result = template;

        foreach (var variable in variables)
        {
            var placeholder = $"{{{{{variable.Key}}}}}";
            var value = variable.Value?.ToString() ?? "";
            result = result.Replace(placeholder, value);
        }

        return result;
    }

    /// <summary>
    /// Renders a custom template.
    /// </summary>
    private async Task<string> RenderCustomTemplateAsync(
        NotificationTemplate template, 
        Dictionary<string, object> variables, 
        CancellationToken cancellationToken)
    {
        // This is a placeholder for custom template engines
        // In production, you'd implement custom template rendering logic
        await Task.Delay(1, cancellationToken); // Simulate async work
        
        // For now, fall back to simple template rendering
        return RenderSimpleTemplate(template.Content, variables);
    }

    /// <summary>
    /// Checks if a variable has a value in the notification.
    /// </summary>
    private bool HasVariableValue(NotificationEvent notification, string variableName)
    {
        return notification.TemplateVariables.ContainsKey(variableName);
    }

    /// <summary>
    /// Gets a variable value from the notification.
    /// </summary>
    private object? GetVariableValue(NotificationEvent notification, string variableName)
    {
        return notification.TemplateVariables.TryGetValue(variableName, out var value) ? value : null;
    }

    /// <summary>
    /// Validates a variable value against its validation rules.
    /// </summary>
    private List<string> ValidateVariableValue(TemplateVariable variable, object? value)
    {
        var errors = new List<string>();

        foreach (var validation in variable.Validations)
        {
            try
            {
                var error = ValidateVariableValue(variable, value, validation);
                if (!string.IsNullOrEmpty(error))
                {
                    errors.Add(error);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error for variable '{variable.Name}': {ex.Message}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates a variable value against a specific validation rule.
    /// </summary>
    private string? ValidateVariableValue(TemplateVariable variable, object? value, TemplateVariableValidation validation)
    {
        if (value == null)
        {
            if (validation.Type == ValidationType.Required)
            {
                return validation.Message ?? $"Variable '{variable.Name}' is required";
            }
            return null; // Null values are valid for non-required fields
        }

        return validation.Type switch
        {
            ValidationType.Required => null, // Already handled above
            ValidationType.MinLength => ValidateMinLength(value, validation),
            ValidationType.MaxLength => ValidateMaxLength(value, validation),
            ValidationType.MinValue => ValidateMinValue(value, validation),
            ValidationType.MaxValue => ValidateMaxValue(value, validation),
            ValidationType.Regex => ValidateRegex(value, validation),
            ValidationType.Email => ValidateEmail(value, validation),
            ValidationType.Url => ValidateUrl(value, validation),
            ValidationType.Custom => ValidateCustom(value, validation),
            _ => null
        };
    }

    /// <summary>
    /// Validates minimum length.
    /// </summary>
    private string? ValidateMinLength(object value, TemplateVariableValidation validation)
    {
        if (validation.Parameters.TryGetValue("minLength", out var minLengthObj) && 
            int.TryParse(minLengthObj.ToString(), out var minLength))
        {
            var str = value.ToString();
            if (str != null && str.Length < minLength)
            {
                return validation.Message ?? $"Value must be at least {minLength} characters long";
            }
        }
        return null;
    }

    /// <summary>
    /// Validates maximum length.
    /// </summary>
    private string? ValidateMaxLength(object value, TemplateVariableValidation validation)
    {
        if (validation.Parameters.TryGetValue("maxLength", out var maxLengthObj) && 
            int.TryParse(maxLengthObj.ToString(), out var maxLength))
        {
            var str = value.ToString();
            if (str != null && str.Length > maxLength)
            {
                return validation.Message ?? $"Value must be no more than {maxLength} characters long";
            }
        }
        return null;
    }

    /// <summary>
    /// Validates minimum value.
    /// </summary>
    private string? ValidateMinValue(object value, TemplateVariableValidation validation)
    {
        if (validation.Parameters.TryGetValue("minValue", out var minValueObj))
        {
            if (value is IComparable comparable && minValueObj is IComparable minValue)
            {
                if (comparable.CompareTo(minValue) < 0)
                {
                    return validation.Message ?? $"Value must be at least {minValue}";
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Validates maximum value.
    /// </summary>
    private string? ValidateMaxValue(object value, TemplateVariableValidation validation)
    {
        if (validation.Parameters.TryGetValue("maxValue", out var maxValueObj))
        {
            if (value is IComparable comparable && maxValueObj is IComparable maxValue)
            {
                if (comparable.CompareTo(maxValue) > 0)
                {
                    return validation.Message ?? $"Value must be no more than {maxValue}";
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Validates regex pattern.
    /// </summary>
    private string? ValidateRegex(object value, TemplateVariableValidation validation)
    {
        if (validation.Parameters.TryGetValue("pattern", out var patternObj))
        {
            var pattern = patternObj.ToString();
            var str = value.ToString();
            
            if (!string.IsNullOrEmpty(pattern) && !string.IsNullOrEmpty(str))
            {
                try
                {
                    if (!Regex.IsMatch(str, pattern))
                    {
                        return validation.Message ?? $"Value does not match the required pattern";
                    }
                }
                catch (ArgumentException)
                {
                    return "Invalid regex pattern in validation";
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Validates email format.
    /// </summary>
    private string? ValidateEmail(object value, TemplateVariableValidation validation)
    {
        var str = value.ToString();
        if (!string.IsNullOrEmpty(str))
        {
            try
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(str))
                {
                    return validation.Message ?? "Value must be a valid email address";
                }
            }
            catch (ArgumentException)
            {
                return "Invalid email validation pattern";
            }
        }
        return null;
    }

    /// <summary>
    /// Validates URL format.
    /// </summary>
    private string? ValidateUrl(object value, TemplateVariableValidation validation)
    {
        var str = value.ToString();
        if (!string.IsNullOrEmpty(str))
        {
            try
            {
                var urlRegex = new Regex(@"^https?://[^\s/$.?#].[^\s]*$");
                if (!urlRegex.IsMatch(str))
                {
                    return validation.Message ?? "Value must be a valid URL";
                }
            }
            catch (ArgumentException)
            {
                return "Invalid URL validation pattern";
            }
        }
        return null;
    }

    /// <summary>
    /// Validates custom validation.
    /// </summary>
    private string? ValidateCustom(object value, TemplateVariableValidation validation)
    {
        // This is a placeholder for custom validation logic
        // In production, you'd implement custom validation based on the validation parameters
        return null;
    }

    /// <summary>
    /// Extracts used variables from template content.
    /// </summary>
    private HashSet<string> ExtractUsedVariables(NotificationTemplate template)
    {
        var usedVariables = new HashSet<string>();

        // Extract from content
        ExtractVariablesFromText(template.Content, usedVariables);

        // Extract from subject
        if (!string.IsNullOrEmpty(template.Subject))
        {
            ExtractVariablesFromText(template.Subject, usedVariables);
        }

        // Extract from attachment names
        foreach (var attachment in template.Attachments)
        {
            ExtractVariablesFromText(attachment.Name, usedVariables);
        }

        return usedVariables;
    }

    /// <summary>
    /// Extracts variables from text using common template patterns.
    /// </summary>
    private void ExtractVariablesFromText(string text, HashSet<string> variables)
    {
        if (string.IsNullOrEmpty(text))
            return;

        // Extract {{variable}} patterns
        var mustachePattern = @"\{\{([^}]+)\}\}";
        var matches = Regex.Matches(text, mustachePattern);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var variableName = match.Groups[1].Value.Trim();
                variables.Add(variableName);
            }
        }

        // Extract {{{variable}}} patterns (Handlebars)
        var handlebarsPattern = @"\{\{\{([^}]+)\}\}\}";
        matches = Regex.Matches(text, handlebarsPattern);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var variableName = match.Groups[1].Value.Trim();
                variables.Add(variableName);
            }
        }
    }

    /// <inheritdoc />
    public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating template {TemplateId}", template.Id);

            lock (_templatesLock)
            {
                _templates[template.Id] = template;
            }

            _logger.LogInformation("Successfully created template {TemplateId}", template.Id);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create template {TemplateId}", template.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting template {TemplateId}", templateId);

            lock (_templatesLock)
            {
                var removed = _templates.Remove(templateId);
                if (removed)
                {
                    _logger.LogInformation("Successfully deleted template {TemplateId}", templateId);
                }
                else
                {
                    _logger.LogWarning("Template {TemplateId} not found for deletion", templateId);
                }
                return removed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete template {TemplateId}", templateId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<TemplateRenderResult> RenderTemplateAsync(string templateId, Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Rendering template {TemplateId}", templateId);

            var template = await GetTemplateAsync(templateId, cancellationToken);
            if (template == null)
            {
                return new TemplateRenderResult
                {
                    IsSuccess = false,
                    RenderedContent = string.Empty,
                    ErrorMessage = $"Template {templateId} not found"
                };
            }

            // Create a mock notification event for rendering
            var notification = new NotificationEvent
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "default",
                EventType = "template_render",
                Subject = template.Subject,
                Content = template.Content,
                TemplateVariables = data,
                Recipients = new List<NotificationRecipient>(),
                PreferredChannels = new List<NotificationChannel>(),
                Priority = NotificationPriority.Normal,
                ScheduledFor = null,
                Metadata = new Dictionary<string, object>()
            };

            return await RenderAsync(notification, template, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template {TemplateId}", templateId);
            return new TemplateRenderResult
            {
                IsSuccess = false,
                RenderedContent = string.Empty,
                ErrorMessage = ex.Message
            };
        }
    }
}