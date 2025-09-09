using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Interface for the notification template service.
/// Handles template rendering, variable interpolation, and multi-language support.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Renders a notification using the specified template.
    /// </summary>
    /// <param name="notification">The notification to render.</param>
    /// <param name="template">The template to use for rendering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous render operation.</returns>
    Task<TemplateRenderResult> RenderAsync(
        NotificationEvent notification, 
        NotificationTemplate template, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a notification using a template ID.
    /// </summary>
    /// <param name="notification">The notification to render.</param>
    /// <param name="templateId">The ID of the template to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous render operation.</returns>
    Task<TemplateRenderResult> RenderAsync(
        NotificationEvent notification, 
        string templateId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates template variables against the notification data.
    /// </summary>
    /// <param name="template">The template to validate.</param>
    /// <param name="notification">The notification data to validate against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous validation operation.</returns>
    Task<TemplateValidationResult> ValidateAsync(
        NotificationTemplate template, 
        NotificationEvent notification, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a template by ID.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template retrieval operation.</returns>
    Task<NotificationTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets templates for a specific tenant and channel.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="channel">The notification channel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template retrieval operation.</returns>
    Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(
        string tenantId, 
        NotificationChannel channel, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets templates for a specific tenant, channel, and event type.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="channel">The notification channel.</param>
    /// <param name="eventType">The event type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template retrieval operation.</returns>
    Task<IEnumerable<NotificationTemplate>> GetTemplatesAsync(
        string tenantId, 
        NotificationChannel channel, 
        string eventType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new template.
    /// </summary>
    /// <param name="template">The template to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template addition operation.</returns>
    Task<bool> AddTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing template.
    /// </summary>
    /// <param name="template">The updated template.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template update operation.</returns>
    Task<bool> UpdateTemplateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a template.
    /// </summary>
    /// <param name="templateId">The template ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous template removal operation.</returns>
    Task<bool> RemoveTemplateAsync(string templateId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of template rendering.
/// </summary>
public sealed class TemplateRenderResult
{
    /// <summary>
    /// Whether the rendering was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The rendered subject.
    /// </summary>
    public string? RenderedSubject { get; init; }

    /// <summary>
    /// The rendered content.
    /// </summary>
    public string RenderedContent { get; init; } = string.Empty;

    /// <summary>
    /// The rendered attachments.
    /// </summary>
    public List<RenderedAttachment> RenderedAttachments { get; init; } = new();

    /// <summary>
    /// Error message if rendering failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Additional metadata about the rendering process.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Timestamp when the rendering was performed.
    /// </summary>
    public DateTime RenderedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful render result.
    /// </summary>
    public static TemplateRenderResult Success(
        string renderedContent, 
        string? renderedSubject = null, 
        List<RenderedAttachment>? renderedAttachments = null,
        Dictionary<string, object>? metadata = null)
    {
        return new TemplateRenderResult
        {
            IsSuccess = true,
            RenderedContent = renderedContent,
            RenderedSubject = renderedSubject,
            RenderedAttachments = renderedAttachments ?? new List<RenderedAttachment>(),
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a failed render result.
    /// </summary>
    public static TemplateRenderResult Failure(string errorMessage, Dictionary<string, object>? metadata = null)
    {
        return new TemplateRenderResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }
}

/// <summary>
/// Result of template validation.
/// </summary>
public sealed class TemplateValidationResult
{
    /// <summary>
    /// Whether the validation passed.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Validation errors.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Validation warnings.
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Missing required variables.
    /// </summary>
    public List<string> MissingVariables { get; init; } = new();

    /// <summary>
    /// Unused variables in the template.
    /// </summary>
    public List<string> UnusedVariables { get; init; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static TemplateValidationResult Success(List<string>? warnings = null)
    {
        return new TemplateValidationResult
        {
            IsValid = true,
            Warnings = warnings ?? new List<string>()
        };
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static TemplateValidationResult Failure(
        List<string> errors, 
        List<string>? warnings = null,
        List<string>? missingVariables = null,
        List<string>? unusedVariables = null)
    {
        return new TemplateValidationResult
        {
            IsValid = false,
            Errors = errors,
            Warnings = warnings ?? new List<string>(),
            MissingVariables = missingVariables ?? new List<string>(),
            UnusedVariables = unusedVariables ?? new List<string>()
        };
    }
}

/// <summary>
/// Represents a rendered attachment.
/// </summary>
public sealed class RenderedAttachment
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