using Microsoft.AspNetCore.Mvc;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using NotifyXStudio.Api.Filters;

namespace NotifyXStudio.Api.Controllers;

/// <summary>
/// Controller for notification operations using real services
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(INotificationService notificationService, ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Send a notification
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendNotification([FromBody] NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending notification {NotificationId} of type {EventType}", 
                notification.Id, notification.EventType);

            var result = await _notificationService.SendAsync(notification, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {NotificationId}", notification.Id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Send a batch of notifications
    /// </summary>
    [HttpPost("send-batch")]
    [ProducesResponseType(typeof(BatchNotificationResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SendBatchNotifications([FromBody] IEnumerable<NotificationEvent> notifications, CancellationToken cancellationToken = default)
    {
        try
        {
            var notificationsList = notifications.ToList();
            _logger.LogInformation("Sending batch of {Count} notifications", notificationsList.Count);

            var result = await _notificationService.SendBatchAsync(notificationsList, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch notifications");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Schedule a notification
    /// </summary>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(NotificationResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ScheduleNotification([FromBody] ScheduleNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Scheduling notification {NotificationId} for {ScheduledFor}", 
                request.Notification.Id, request.ScheduledFor);

            var result = await _notificationService.ScheduleAsync(request.Notification, request.ScheduledFor, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling notification {NotificationId}", request.Notification.Id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get notification status
    /// </summary>
    [HttpGet("{notificationId}/status")]
    [ProducesResponseType(typeof(NotificationStatus), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetNotificationStatus(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await _notificationService.GetStatusAsync(notificationId, cancellationToken);
            
            if (status == null)
            {
                return NotFound(new { error = "Notification not found" });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification status {NotificationId}", notificationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel a notification
    /// </summary>
    [HttpPost("{notificationId}/cancel")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CancelNotification(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _notificationService.CancelAsync(notificationId, cancellationToken);
            
            if (result)
            {
                return Ok(new { message = "Notification cancelled successfully" });
            }
            else
            {
                return BadRequest(new { error = "Failed to cancel notification" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling notification {NotificationId}", notificationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Retry a failed notification
    /// </summary>
    [HttpPost("{notificationId}/retry")]
    [ProducesResponseType(typeof(NotificationResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> RetryNotification(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _notificationService.RetryAsync(notificationId, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying notification {NotificationId}", notificationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get delivery history for a notification
    /// </summary>
    [HttpGet("{notificationId}/delivery-history")]
    [ProducesResponseType(typeof(IEnumerable<DeliveryAttempt>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetDeliveryHistory(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _notificationService.GetDeliveryHistoryAsync(notificationId, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery history for notification {NotificationId}", notificationId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

/// <summary>
/// Request model for scheduling notifications
/// </summary>
public class ScheduleNotificationRequest
{
    public NotificationEvent Notification { get; set; } = null!;
    public DateTime ScheduledFor { get; set; }
}