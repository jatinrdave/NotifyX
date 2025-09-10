using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for queue operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly ILogger<QueueController> _logger;
        private readonly IQueueService _queueService;

        public QueueController(ILogger<QueueController> logger, IQueueService queueService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
        }

        /// <summary>
        /// Gets queue information.
        /// </summary>
        [HttpGet("{queueName}")]
        public async Task<IActionResult> GetQueueInfo(string queueName)
        {
            try
            {
                var queueInfo = await _queueService.GetQueueInfoAsync(queueName);

                if (queueInfo == null)
                {
                    return NotFound(new
                    {
                        error = "Queue not found",
                        queueName
                    });
                }

                return Ok(queueInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get queue info for {QueueName}: {Message}", queueName, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve queue information",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Lists all queues.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListQueues()
        {
            try
            {
                var queues = await _queueService.ListQueuesAsync();

                return Ok(new
                {
                    queues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list queues: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to list queues",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets queue statistics.
        /// </summary>
        [HttpGet("{queueName}/stats")]
        public async Task<IActionResult> GetQueueStats(string queueName)
        {
            try
            {
                var stats = await _queueService.GetQueueStatsAsync(queueName);

                return Ok(new
                {
                    queueName,
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get queue stats for {QueueName}: {Message}", queueName, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve queue statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets queue messages.
        /// </summary>
        [HttpGet("{queueName}/messages")]
        public async Task<IActionResult> GetQueueMessages(
            string queueName,
            [FromQuery] int count = 10)
        {
            try
            {
                var messages = await _queueService.GetQueueMessagesAsync(queueName, count);

                return Ok(new
                {
                    queueName,
                    messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get queue messages for {QueueName}: {Message}", queueName, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve queue messages",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Purges a queue.
        /// </summary>
        [HttpDelete("{queueName}/purge")]
        public async Task<IActionResult> PurgeQueue(string queueName)
        {
            try
            {
                await _queueService.PurgeQueueAsync(queueName);

                return Ok(new
                {
                    message = "Queue purged successfully",
                    queueName,
                    purgedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to purge queue {QueueName}: {Message}", queueName, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to purge queue",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Pauses a queue.
        /// </summary>
        [HttpPost("{queueName}/pause")]
        public async Task<IActionResult> PauseQueue(string queueName)
        {
            try
            {
                await _queueService.PauseQueueAsync(queueName);

                return Ok(new
                {
                    message = "Queue paused successfully",
                    queueName,
                    pausedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to pause queue {QueueName}: {Message}", queueName, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to pause queue",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Resumes a queue.
        /// </summary>
        [HttpPost("{queueName}/resume")]
        public async Task<IActionResult> ResumeQueue(string queueName)
        {
            try
            {
                await _queueService.ResumeQueueAsync(queueName);

                return Ok(new
                {
                    message = "Queue resumed successfully",
                    queueName,
                    resumedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resume queue {QueueName}: {Message}", queueName, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to resume queue",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets dead letter queue messages.
        /// </summary>
        [HttpGet("{queueName}/dlq")]
        public async Task<IActionResult> GetDeadLetterQueueMessages(
            string queueName,
            [FromQuery] int count = 10)
        {
            try
            {
                var messages = await _queueService.GetDeadLetterQueueMessagesAsync(queueName, count);

                return Ok(new
                {
                    queueName,
                    messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get DLQ messages for {QueueName}: {Message}", queueName, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve dead letter queue messages",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Replays dead letter queue messages.
        /// </summary>
        [HttpPost("{queueName}/dlq/replay")]
        public async Task<IActionResult> ReplayDeadLetterQueueMessages(
            string queueName,
            [FromQuery] int count = 10)
        {
            try
            {
                var replayedCount = await _queueService.ReplayDeadLetterQueueMessagesAsync(queueName, count);

                return Ok(new
                {
                    message = "Dead letter queue messages replayed successfully",
                    queueName,
                    replayedCount,
                    replayedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to replay DLQ messages for {QueueName}: {Message}", queueName, ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to replay dead letter queue messages",
                    message = ex.Message
                });
            }
        }
    }
}