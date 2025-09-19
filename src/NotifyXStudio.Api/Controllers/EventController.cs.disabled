using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for event operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventService _eventService;

        public EventController(ILogger<EventController> logger, IEventService eventService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
        }

        /// <summary>
        /// Publishes an event.
        /// </summary>
        [HttpPost("publish")]
        public async Task<IActionResult> PublishEvent([FromBody] PublishEventRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Event request is required");
                }

                var eventId = await _eventService.CreateEventAsync(
                    request.TenantId,
                    new List<string> { request.EventType },
                    request.EventType,
                    request.Metadata ?? new Dictionary<string, object>());

                return Ok(new
                {
                    eventId,
                    message = "Event published successfully",
                    publishedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to publish event",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets event information.
        /// </summary>
        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetEvent(string eventId)
        {
            try
            {
                var eventData = await _eventService.GetEventAsync(eventId);

                if (eventData == null)
                {
                    return NotFound(new
                    {
                        error = "Event not found",
                        eventId
                    });
                }

                return Ok(eventData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get event {EventId}: {Message}", eventId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve event",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists events for a tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListEvents(
            [FromQuery] Guid? tenantId,
            [FromQuery] string? eventType,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-1);
                var end = endDate ?? DateTime.UtcNow;

                var events = await _eventService.ListEventsAsync(tenantId?.ToString(), eventType, "active", page, pageSize);
                var totalCount = await _eventService.GetEventCountAsync(tenantId?.ToString(), eventType, "active");

                return Ok(new
                {
                    events,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list events: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list events",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets event statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetEventStats(
            [FromQuery] Guid? tenantId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-1);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _eventService.GetEventStatsAsync(tenantId?.ToString(), null);

                return Ok(new
                {
                    tenantId,
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get event stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve event statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets event types.
        /// </summary>
        [HttpGet("types")]
        public async Task<IActionResult> GetEventTypes([FromQuery] Guid? tenantId)
        {
            try
            {
                var eventTypes = await _eventService.GetEventTypesAsync();

                return Ok(new
                {
                    tenantId,
                    eventTypes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get event types: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve event types",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToEvents([FromBody] SubscribeToEventsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Subscription request is required");
                }

                var subscriptionId = await _eventService.CreateEventAsync(
                    request.TenantId,
                    request.EventTypes,
                    request.CallbackUrl,
                    request.Filters ?? new Dictionary<string, object>());

                return Ok(new
                {
                    subscriptionId,
                    message = "Event subscription created successfully",
                    subscribedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to events: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to subscribe to events",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Unsubscribes from events.
        /// </summary>
        [HttpDelete("subscribe/{subscriptionId}")]
        public async Task<IActionResult> UnsubscribeFromEvents(string subscriptionId)
        {
            try
            {
                await _eventService.UnsubscribeFromEventsAsync(subscriptionId);

                return Ok(new
                {
                    message = "Event subscription cancelled successfully",
                    subscriptionId,
                    unsubscribedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unsubscribe from events {SubscriptionId}: {Message}", subscriptionId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to unsubscribe from events",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets event subscriptions.
        /// </summary>
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetEventSubscriptions([FromQuery] Guid? tenantId)
        {
            try
            {
                var subscriptions = await _eventService.GetEventSubscriptionsAsync(tenantId?.ToString() ?? "default");

                return Ok(new
                {
                    tenantId,
                    subscriptions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get event subscriptions: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve event subscriptions",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Publish event request model.
    /// </summary>
    public class PublishEventRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Event type.
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Event data.
        /// </summary>
        public Dictionary<string, object> EventData { get; set; } = new();

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Subscribe to events request model.
    /// </summary>
    public class SubscribeToEventsRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Event types to subscribe to.
        /// </summary>
        public List<string> EventTypes { get; set; } = new();

        /// <summary>
        /// Callback URL for event notifications.
        /// </summary>
        public string CallbackUrl { get; set; } = string.Empty;

        /// <summary>
        /// Event filters.
        /// </summary>
        public Dictionary<string, object>? Filters { get; set; }
    }
}