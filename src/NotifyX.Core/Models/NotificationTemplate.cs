using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents a notification template for rendering notifications across different channels.
/// </summary>
public sealed record NotificationTemplate
{
    /// <summary>
    /// Unique identifier for this template.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The tenant/organization this template belongs to.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable name for this template.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Description of what this template is used for.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The notification channel this template is for.
    /// </summary>
    public NotificationChannel Channel { get; init; }

    /// <summary>
    /// The event type this template is associated with.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// The template content with variable placeholders.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// The subject template (for email, etc.).
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// The language this template is in.
    /// </summary>
    public string Language { get; init; } = "en";

    /// <summary>
    /// Whether this template is currently active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// The template engine to use for rendering.
    /// </summary>
    public TemplateEngine Engine { get; init; } = TemplateEngine.Mustache;

    /// <summary>
    /// Template variables and their default values.
    /// </summary>
    public Dictionary<string, TemplateVariable> Variables { get; init; } = new();

    /// <summary>
    /// Attachments for this template.
    /// </summary>
    public List<TemplateAttachment> Attachments { get; init; } = new();

    /// <summary>
    /// Custom metadata associated with this template.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when this template was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when this template was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this template.
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Who last updated this template.
    /// </summary>
    public string UpdatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Version number for this template.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Whether this template requires authentication.
    /// </summary>
    public bool RequiresAuthentication { get; init; } = false;

    /// <summary>
    /// Authentication configuration for this template.
    /// </summary>
    public Dictionary<string, string> Authentication { get; init; } = new();

    /// <summary>
    /// Creates a copy of this template with updated properties.
    /// </summary>
    /// <param name="updater">Action to update the template properties.</param>
    /// <returns>A new NotificationTemplate with updated properties.</returns>
    public NotificationTemplate With(Action<NotificationTemplateBuilder> updater)
    {
        var builder = new NotificationTemplateBuilder(this);
        updater(builder);
        return builder.Build();
    }
}

/// <summary>
/// Template engines supported by the system.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TemplateEngine
{
    /// <summary>
    /// Mustache template engine.
    /// </summary>
    Mustache = 0,

    /// <summary>
    /// Handlebars template engine.
    /// </summary>
    Handlebars = 1,

    /// <summary>
    /// Razor template engine.
    /// </summary>
    Razor = 2,

    /// <summary>
    /// Simple string replacement.
    /// </summary>
    Simple = 3,

    /// <summary>
    /// Custom template engine.
    /// </summary>
    Custom = 4
}

/// <summary>
/// Represents a template variable with its definition and default value.
/// </summary>
public sealed class TemplateVariable
{
    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The data type of the variable.
    /// </summary>
    public TemplateVariableType Type { get; init; } = TemplateVariableType.String;

    /// <summary>
    /// The default value for this variable.
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Whether this variable is required.
    /// </summary>
    public bool IsRequired { get; init; } = false;

    /// <summary>
    /// Description of what this variable represents.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Validation rules for this variable.
    /// </summary>
    public List<TemplateVariableValidation> Validations { get; init; } = new();

    /// <summary>
    /// Custom metadata for this variable.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Types of template variables.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TemplateVariableType
{
    /// <summary>
    /// String variable.
    /// </summary>
    String = 0,

    /// <summary>
    /// Integer variable.
    /// </summary>
    Integer = 1,

    /// <summary>
    /// Decimal variable.
    /// </summary>
    Decimal = 2,

    /// <summary>
    /// Boolean variable.
    /// </summary>
    Boolean = 3,

    /// <summary>
    /// Date variable.
    /// </summary>
    Date = 4,

    /// <summary>
    /// DateTime variable.
    /// </summary>
    DateTime = 5,

    /// <summary>
    /// Object variable.
    /// </summary>
    Object = 6,

    /// <summary>
    /// Array variable.
    /// </summary>
    Array = 7
}

/// <summary>
/// Validation rule for template variables.
/// </summary>
public sealed class TemplateVariableValidation
{
    /// <summary>
    /// The type of validation.
    /// </summary>
    public ValidationType Type { get; init; } = ValidationType.Required;

    /// <summary>
    /// The validation message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Parameters for the validation.
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();
}

/// <summary>
/// Types of validation that can be applied to template variables.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValidationType
{
    /// <summary>
    /// Required validation.
    /// </summary>
    Required = 0,

    /// <summary>
    /// Minimum length validation.
    /// </summary>
    MinLength = 1,

    /// <summary>
    /// Maximum length validation.
    /// </summary>
    MaxLength = 2,

    /// <summary>
    /// Minimum value validation.
    /// </summary>
    MinValue = 3,

    /// <summary>
    /// Maximum value validation.
    /// </summary>
    MaxValue = 4,

    /// <summary>
    /// Regular expression validation.
    /// </summary>
    Regex = 5,

    /// <summary>
    /// Email validation.
    /// </summary>
    Email = 6,

    /// <summary>
    /// URL validation.
    /// </summary>
    Url = 7,

    /// <summary>
    /// Custom validation.
    /// </summary>
    Custom = 8
}

/// <summary>
/// Represents an attachment for a template.
/// </summary>
public sealed class TemplateAttachment
{
    /// <summary>
    /// The name of the attachment.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The content type of the attachment.
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// The content of the attachment (base64 encoded).
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Whether this attachment is inline.
    /// </summary>
    public bool IsInline { get; init; } = false;

    /// <summary>
    /// The content ID for inline attachments.
    /// </summary>
    public string? ContentId { get; init; }

    /// <summary>
    /// Custom metadata for this attachment.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Builder class for creating NotificationTemplate instances with fluent API.
/// </summary>
public sealed class NotificationTemplateBuilder
{
    private readonly NotificationTemplate _template;

    public NotificationTemplateBuilder(NotificationTemplate template)
    {
        _template = template;
    }

    /// <summary>
    /// Sets the tenant ID.
    /// </summary>
    public NotificationTemplateBuilder WithTenantId(string tenantId)
    {
        return new NotificationTemplateBuilder(_template with { TenantId = tenantId });
    }

    /// <summary>
    /// Sets the template name.
    /// </summary>
    public NotificationTemplateBuilder WithName(string name)
    {
        return new NotificationTemplateBuilder(_template with { Name = name });
    }

    /// <summary>
    /// Sets the template description.
    /// </summary>
    public NotificationTemplateBuilder WithDescription(string description)
    {
        return new NotificationTemplateBuilder(_template with { Description = description });
    }

    /// <summary>
    /// Sets the notification channel.
    /// </summary>
    public NotificationTemplateBuilder WithChannel(NotificationChannel channel)
    {
        return new NotificationTemplateBuilder(_template with { Channel = channel });
    }

    /// <summary>
    /// Sets the event type.
    /// </summary>
    public NotificationTemplateBuilder WithEventType(string eventType)
    {
        return new NotificationTemplateBuilder(_template with { EventType = eventType });
    }

    /// <summary>
    /// Sets the template content.
    /// </summary>
    public NotificationTemplateBuilder WithContent(string content)
    {
        return new NotificationTemplateBuilder(_template with { Content = content });
    }

    /// <summary>
    /// Sets the template subject.
    /// </summary>
    public NotificationTemplateBuilder WithSubject(string subject)
    {
        return new NotificationTemplateBuilder(_template with { Subject = subject });
    }

    /// <summary>
    /// Sets the language.
    /// </summary>
    public NotificationTemplateBuilder WithLanguage(string language)
    {
        return new NotificationTemplateBuilder(_template with { Language = language });
    }

    /// <summary>
    /// Sets the active status.
    /// </summary>
    public NotificationTemplateBuilder WithActiveStatus(bool isActive)
    {
        return new NotificationTemplateBuilder(_template with { IsActive = isActive });
    }

    /// <summary>
    /// Sets the template engine.
    /// </summary>
    public NotificationTemplateBuilder WithEngine(TemplateEngine engine)
    {
        return new NotificationTemplateBuilder(_template with { Engine = engine });
    }

    /// <summary>
    /// Adds a template variable.
    /// </summary>
    public NotificationTemplateBuilder WithVariable(TemplateVariable variable)
    {
        var variables = new Dictionary<string, TemplateVariable>(_template.Variables) { [variable.Name] = variable };
        return new NotificationTemplateBuilder(_template with { Variables = variables });
    }

    /// <summary>
    /// Adds an attachment.
    /// </summary>
    public NotificationTemplateBuilder WithAttachment(TemplateAttachment attachment)
    {
        var attachments = new List<TemplateAttachment>(_template.Attachments) { attachment };
        return new NotificationTemplateBuilder(_template with { Attachments = attachments });
    }

    /// <summary>
    /// Adds metadata.
    /// </summary>
    public NotificationTemplateBuilder WithMetadata(string key, object value)
    {
        var metadata = new Dictionary<string, object>(_template.Metadata) { [key] = value };
        return new NotificationTemplateBuilder(_template with { Metadata = metadata });
    }

    /// <summary>
    /// Sets the created by user.
    /// </summary>
    public NotificationTemplateBuilder WithCreatedBy(string createdBy)
    {
        return new NotificationTemplateBuilder(_template with { CreatedBy = createdBy });
    }

    /// <summary>
    /// Sets the updated by user.
    /// </summary>
    public NotificationTemplateBuilder WithUpdatedBy(string updatedBy)
    {
        return new NotificationTemplateBuilder(_template with { UpdatedBy = updatedBy });
    }

    /// <summary>
    /// Sets the version.
    /// </summary>
    public NotificationTemplateBuilder WithVersion(int version)
    {
        return new NotificationTemplateBuilder(_template with { Version = version });
    }

    /// <summary>
    /// Sets the authentication requirements.
    /// </summary>
    public NotificationTemplateBuilder WithAuthentication(bool requiresAuth, Dictionary<string, string>? authConfig = null)
    {
        return new NotificationTemplateBuilder(_template with 
        { 
            RequiresAuthentication = requiresAuth,
            Authentication = authConfig ?? new Dictionary<string, string>()
        });
    }

    /// <summary>
    /// Builds the final NotificationTemplate.
    /// </summary>
    public NotificationTemplate Build() => _template;
}