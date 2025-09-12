using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for notification operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly INotificationService _notificationService;

        public NotificationController(ILogger<NotificationController> logger, INotificationService notificationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// Sends a notification.
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Notification request is required");
                }

                var notificationId = await _notificationService.SendNotificationAsync(
                    request.TenantId,
                    request.Channel,
                    request.Recipient,
                    request.Message,
                    request.Priority,
                    request.Metadata);

                return Ok(new
                {
                    notificationId,
                    message = "Notification sent successfully",
                    sentAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to send notification",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets notification status.
        /// </summary>
        [HttpGet("{notificationId}/status")]
        public async Task<IActionResult> GetNotificationStatus(string notificationId)
        {
            try
            {
                var status = await _notificationService.GetNotificationStatusAsync(notificationId);

                if (status == null)
                {
                    return NotFound(new
                    {
                        error = "Notification not found",
                        notificationId
                    });
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notification status for {NotificationId}: {Message}", notificationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve notification status",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets notification history.
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetNotificationHistory(
            [FromQuery] Guid? tenantId,
            [FromQuery] string? channel,
            [FromQuery] string? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var notifications = await _notificationService.GetNotificationHistoryAsync(
                    tenantId, channel, status, start, end, page, pageSize);

                var totalCount = await _notificationService.GetNotificationCountAsync(
                    tenantId, channel, status, start, end);

                return Ok(new
                {
                    notifications,
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
                _logger.LogError(ex, "Failed to get notification history: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve notification history",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets notification statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetNotificationStats(
            [FromQuery] Guid? tenantId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _notificationService.GetNotificationStatsAsync(tenantId, start, end);

                return Ok(new
                {
                    tenantId,
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notification stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve notification statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Cancels a notification.
        /// </summary>
        [HttpPost("{notificationId}/cancel")]
        public async Task<IActionResult> CancelNotification(string notificationId)
        {
            try
            {
                await _notificationService.CancelNotificationAsync(notificationId);

                return Ok(new
                {
                    message = "Notification cancelled successfully",
                    notificationId,
                    cancelledAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel notification {NotificationId}: {Message}", notificationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to cancel notification",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retries a failed notification.
        /// </summary>
        [HttpPost("{notificationId}/retry")]
        public async Task<IActionResult> RetryNotification(string notificationId)
        {
            try
            {
                await _notificationService.RetryNotificationAsync(notificationId);

                return Ok(new
                {
                    message = "Notification retry initiated successfully",
                    notificationId,
                    retriedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retry notification {NotificationId}: {Message}", notificationId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retry notification",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Send notification request model.
    /// </summary>
    public class SendNotificationRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Notification channel.
        /// </summary>
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// Recipient information.
        /// </summary>
        public string Recipient { get; set; } = string.Empty;

        /// <summary>
        /// Notification message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Notification priority.
        /// </summary>
        public string Priority { get; set; } = "normal";

        /// <summary>
        /// Additional metadata.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}