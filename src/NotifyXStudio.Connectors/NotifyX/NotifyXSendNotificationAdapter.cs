using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Connectors;
using System.Threading;
using System.Threading.Tasks;

namespace NotifyXStudio.Connectors.NotifyX
{
    /// <summary>
    /// Adapter for NotifyX Send Notification connector.
    /// </summary>
    public class NotifyXSendNotificationAdapter : IConnectorAdapter
    {
        public string Type => "notifyx.sendNotification";

        private readonly INotifyXSdk _notifyx;
        private readonly ILogger<NotifyXSendNotificationAdapter> _logger;

        public NotifyXSendNotificationAdapter(INotifyXSdk notifyx, ILogger<NotifyXSendNotificationAdapter> logger)
        {
            _notifyx = notifyx ?? throw new ArgumentNullException(nameof(notifyx));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectorExecutionResult> ExecuteAsync(ConnectorExecutionContext context, CancellationToken ct = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation("Executing NotifyX Send Notification for run {RunId}, node {NodeId}", 
                    context.RunMetadata.RunId, context.RunMetadata.NodeId);

                // Parse node configuration
                var config = ParseConfig(context.NodeConfig);
                
                // Build notification request
                var request = BuildNotificationRequest(config, context.Inputs);
                
                // Send notification via NotifyX SDK
                var response = await _notifyx.SendNotificationAsync(request, ct);
                
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                _logger.LogInformation("NotifyX Send Notification completed successfully in {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = true,
                    Output = JsonSerializer.SerializeToElement(new
                    {
                        status = "success",
                        notificationId = response.NotificationId,
                        deliveryStatus = response.DeliveryStatus,
                        channels = response.Channels,
                        sentAt = response.SentAt
                    }),
                    DurationMs = (long)duration,
                    Metadata = new Dictionary<string, object>
                    {
                        ["notificationId"] = response.NotificationId,
                        ["deliveryStatus"] = response.DeliveryStatus,
                        ["channels"] = response.Channels
                    }
                };
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                _logger.LogError(ex, "NotifyX Send Notification failed after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    DurationMs = (long)duration,
                    Output = JsonSerializer.SerializeToElement(new { status = "failed", error = ex.Message })
                };
            }
        }

        private NotifyXSendConfig ParseConfig(JsonElement config)
        {
            return new NotifyXSendConfig
            {
                Channel = config.GetProperty("channel").GetString() ?? "email",
                Recipient = config.GetProperty("recipient").GetString() ?? throw new InvalidOperationException("Recipient is required"),
                Message = config.GetProperty("message").GetString() ?? throw new InvalidOperationException("Message is required"),
                Priority = config.TryGetProperty("priority", out var priority) ? priority.GetString() ?? "normal" : "normal",
                Template = config.TryGetProperty("template", out var template) ? template.GetString() : null,
                Subject = config.TryGetProperty("subject", out var subject) ? subject.GetString() : null,
                Metadata = config.TryGetProperty("metadata", out var metadata) ? metadata : JsonSerializer.SerializeToElement(new { })
            };
        }

        private NotifyXNotificationRequest BuildNotificationRequest(NotifyXSendConfig config, JsonElement inputs)
        {
            // Resolve template variables from inputs
            var resolvedMessage = ResolveTemplate(config.Message, inputs);
            var resolvedSubject = config.Subject != null ? ResolveTemplate(config.Subject, inputs) : null;

            return new NotifyXNotificationRequest
            {
                Channel = config.Channel,
                Recipient = config.Recipient,
                Message = resolvedMessage,
                Subject = resolvedSubject,
                Priority = config.Priority,
                Template = config.Template,
                Metadata = config.Metadata
            };
        }

        private string ResolveTemplate(string template, JsonElement inputs)
        {
            // Simple template resolution - replace {{variable}} with values from inputs
            var result = template;
            
            foreach (var property in inputs.EnumerateObject())
            {
                var placeholder = $"{{{{{property.Name}}}}}";
                var value = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString() ?? "",
                    JsonValueKind.Number => property.Value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => property.Value.GetRawText()
                };
                
                result = result.Replace(placeholder, value);
            }
            
            return result;
        }
    }

    /// <summary>
    /// Configuration for NotifyX Send Notification.
    /// </summary>
    public class NotifyXSendConfig
    {
        public string Channel { get; init; } = "email";
        public string Recipient { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Priority { get; init; } = "normal";
        public string? Template { get; init; }
        public string? Subject { get; init; }
        public JsonElement Metadata { get; init; }
    }

    /// <summary>
    /// NotifyX notification request.
    /// </summary>
    public class NotifyXNotificationRequest
    {
        public string Channel { get; init; } = string.Empty;
        public string Recipient { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? Subject { get; init; }
        public string Priority { get; init; } = "normal";
        public string? Template { get; init; }
        public JsonElement Metadata { get; init; }
    }

    /// <summary>
    /// NotifyX notification response.
    /// </summary>
    public class NotifyXNotificationResponse
    {
        public string NotificationId { get; init; } = string.Empty;
        public string DeliveryStatus { get; init; } = string.Empty;
        public List<string> Channels { get; init; } = new();
        public DateTime SentAt { get; init; } = DateTime.UtcNow;
    }

    /// <summary>
    /// NotifyX SDK interface.
    /// </summary>
    public interface INotifyXSdk
    {
        Task<NotifyXNotificationResponse> SendNotificationAsync(NotifyXNotificationRequest request, CancellationToken cancellationToken = default);
    }
}