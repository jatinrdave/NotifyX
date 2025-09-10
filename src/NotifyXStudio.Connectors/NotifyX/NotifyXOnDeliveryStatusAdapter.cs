using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Connectors;
using System.Threading;
using System.Threading.Tasks;

namespace NotifyXStudio.Connectors.NotifyX
{
    /// <summary>
    /// Adapter for NotifyX On Delivery Status trigger.
    /// </summary>
    public class NotifyXOnDeliveryStatusAdapter : IConnectorAdapter
    {
        public string Type => "notifyx.onDeliveryStatus";

        private readonly INotifyXEventService _eventService;
        private readonly ILogger<NotifyXOnDeliveryStatusAdapter> _logger;

        public NotifyXOnDeliveryStatusAdapter(INotifyXEventService eventService, ILogger<NotifyXOnDeliveryStatusAdapter> logger)
        {
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectorExecutionResult> ExecuteAsync(ConnectorExecutionContext context, CancellationToken ct = default)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                _logger.LogInformation("Executing NotifyX On Delivery Status for run {RunId}, node {NodeId}", 
                    context.RunMetadata.RunId, context.RunMetadata.NodeId);

                // Parse node configuration
                var config = ParseConfig(context.NodeConfig);
                
                // Check for delivery status events
                var events = await _eventService.GetDeliveryStatusEventsAsync(config, context.RunMetadata.RunId, ct);
                
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                _logger.LogInformation("NotifyX On Delivery Status completed successfully in {Duration}ms, found {EventCount} events", 
                    duration, events.Count);

                return new ConnectorExecutionResult
                {
                    Success = true,
                    Output = JsonSerializer.SerializeToElement(new
                    {
                        status = "success",
                        eventCount = events.Count,
                        events = events,
                        processedAt = DateTime.UtcNow
                    }),
                    DurationMs = (long)duration,
                    Metadata = new Dictionary<string, object>
                    {
                        ["eventCount"] = events.Count,
                        ["processedAt"] = DateTime.UtcNow
                    }
                };
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                
                _logger.LogError(ex, "NotifyX On Delivery Status failed after {Duration}ms", duration);

                return new ConnectorExecutionResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    DurationMs = (long)duration,
                    Output = JsonSerializer.SerializeToElement(new { status = "failed", error = ex.Message })
                };
            }
        }

        private NotifyXDeliveryStatusConfig ParseConfig(JsonElement config)
        {
            return new NotifyXDeliveryStatusConfig
            {
                Statuses = config.TryGetProperty("statuses", out var statuses) 
                    ? statuses.EnumerateArray().Select(s => s.GetString() ?? "").ToList() 
                    : new List<string> { "success", "failed", "queued" },
                NotificationIds = config.TryGetProperty("notificationIds", out var ids) 
                    ? ids.EnumerateArray().Select(s => s.GetString() ?? "").ToList() 
                    : new List<string>(),
                Channels = config.TryGetProperty("channels", out var channels) 
                    ? channels.EnumerateArray().Select(s => s.GetString() ?? "").ToList() 
                    : new List<string>(),
                TimeRange = config.TryGetProperty("timeRange", out var timeRange) 
                    ? timeRange.GetInt32() 
                    : 3600, // 1 hour default
                MaxEvents = config.TryGetProperty("maxEvents", out var maxEvents) 
                    ? maxEvents.GetInt32() 
                    : 100
            };
        }
    }

    /// <summary>
    /// Configuration for NotifyX On Delivery Status.
    /// </summary>
    public class NotifyXDeliveryStatusConfig
    {
        public List<string> Statuses { get; init; } = new();
        public List<string> NotificationIds { get; init; } = new();
        public List<string> Channels { get; init; } = new();
        public int TimeRange { get; init; } = 3600; // seconds
        public int MaxEvents { get; init; } = 100;
    }

    /// <summary>
    /// Delivery status event.
    /// </summary>
    public class DeliveryStatusEvent
    {
        public string NotificationId { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Channel { get; init; } = string.Empty;
        public string Recipient { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string? ErrorMessage { get; init; }
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// NotifyX event service interface.
    /// </summary>
    public interface INotifyXEventService
    {
        Task<List<DeliveryStatusEvent>> GetDeliveryStatusEventsAsync(
            NotifyXDeliveryStatusConfig config, 
            string runId, 
            CancellationToken cancellationToken = default);
    }
}