using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for webhook operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IWebhookService _webhookService;

        public WebhookController(ILogger<WebhookController> logger, IWebhookService webhookService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        }

        /// <summary>
        /// Creates a webhook.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWebhook([FromBody] CreateWebhookRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Webhook request is required");
                }

                var webhookId = await _webhookService.CreateWebhookAsync(
                    request.TenantId,
                    request.Name,
                    request.Url,
                    request.Events,
                    request.Secret,
                    request.Headers);

                return Ok(new
                {
                    webhookId,
                    message = "Webhook created successfully",
                    createdAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create webhook: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to create webhook",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets webhook information.
        /// </summary>
        [HttpGet("{webhookId}")]
        public async Task<IActionResult> GetWebhook(string webhookId)
        {
            try
            {
                var webhook = await _webhookService.GetWebhookAsync(webhookId);

                if (webhook == null)
                {
                    return NotFound(new
                    {
                        error = "Webhook not found",
                        webhookId
                    });
                }

                return Ok(webhook);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get webhook {WebhookId}: {Message}", webhookId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve webhook",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists webhooks for a tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListWebhooks(
            [FromQuery] Guid? tenantId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var webhooks = await _webhookService.ListWebhooksAsync(tenantId, page, pageSize);
                var totalCount = await _webhookService.GetWebhookCountAsync(tenantId);

                return Ok(new
                {
                    webhooks,
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
                _logger.LogError(ex, "Failed to list webhooks: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list webhooks",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates a webhook.
        /// </summary>
        [HttpPut("{webhookId}")]
        public async Task<IActionResult> UpdateWebhook(
            string webhookId,
            [FromBody] UpdateWebhookRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Update request is required");
                }

                await _webhookService.UpdateWebhookAsync(
                    webhookId,
                    request.Name,
                    request.Url,
                    request.Events,
                    request.Secret,
                    request.Headers);

                return Ok(new
                {
                    message = "Webhook updated successfully",
                    webhookId,
                    updatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update webhook {WebhookId}: {Message}", webhookId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to update webhook",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a webhook.
        /// </summary>
        [HttpDelete("{webhookId}")]
        public async Task<IActionResult> DeleteWebhook(string webhookId)
        {
            try
            {
                await _webhookService.DeleteWebhookAsync(webhookId);

                return Ok(new
                {
                    message = "Webhook deleted successfully",
                    webhookId,
                    deletedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete webhook {WebhookId}: {Message}", webhookId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to delete webhook",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Tests a webhook.
        /// </summary>
        [HttpPost("{webhookId}/test")]
        public async Task<IActionResult> TestWebhook(string webhookId)
        {
            try
            {
                var result = await _webhookService.TestWebhookAsync(webhookId);

                return Ok(new
                {
                    message = "Webhook test completed",
                    webhookId,
                    result,
                    testedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to test webhook {WebhookId}: {Message}", webhookId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to test webhook",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets webhook delivery logs.
        /// </summary>
        [HttpGet("{webhookId}/logs")]
        public async Task<IActionResult> GetWebhookLogs(
            string webhookId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                var logs = await _webhookService.GetWebhookLogsAsync(webhookId, start, end, page, pageSize);
                var totalCount = await _webhookService.GetWebhookLogCountAsync(webhookId, start, end);

                return Ok(new
                {
                    webhookId,
                    logs,
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
                _logger.LogError(ex, "Failed to get webhook logs for {WebhookId}: {Message}", webhookId, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve webhook logs",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Create webhook request model.
    /// </summary>
    public class CreateWebhookRequest
    {
        /// <summary>
        /// Tenant ID.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Webhook name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Webhook URL.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Events to subscribe to.
        /// </summary>
        public List<string> Events { get; set; } = new();

        /// <summary>
        /// Webhook secret for verification.
        /// </summary>
        public string? Secret { get; set; }

        /// <summary>
        /// Additional headers.
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }
    }

    /// <summary>
    /// Update webhook request model.
    /// </summary>
    public class UpdateWebhookRequest
    {
        /// <summary>
        /// Webhook name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Webhook URL.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Events to subscribe to.
        /// </summary>
        public List<string>? Events { get; set; }

        /// <summary>
        /// Webhook secret for verification.
        /// </summary>
        public string? Secret { get; set; }

        /// <summary>
        /// Additional headers.
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }
    }
}