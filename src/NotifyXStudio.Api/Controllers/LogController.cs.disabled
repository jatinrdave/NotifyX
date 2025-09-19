using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Api.Controllers
{
    /// <summary>
    /// Controller for log operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly ILogger<LogController> _logger;
        private readonly ILogService _logService;

        public LogController(ILogger<LogController> logger, ILogService logService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
        }

        /// <summary>
        /// Gets logs for a specific time range.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? level,
            [FromQuery] string? source,
            [FromQuery] string? message,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-1);
                var end = endDate ?? DateTime.UtcNow;

                var logs = await _logService.GetLogsAsync(null, level, source, start, end, page, pageSize);
                var totalCount = await _logService.GetLogCountAsync(null, level, source, start, end);

                return Ok(new
                {
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
                _logger.LogError(ex, "Failed to get logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets log statistics.
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetLogStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-1);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _logService.GetLogStatsAsync("default");

                return Ok(new
                {
                    dateRange = new { start, end },
                    stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get log stats: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve log statistics",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets log levels.
        /// </summary>
        [HttpGet("levels")]
        public async Task<IActionResult> GetLogLevels()
        {
            try
            {
                var levels = await _logService.GetLogLevelsAsync();

                return Ok(new
                {
                    levels
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get log levels: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve log levels",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets log sources.
        /// </summary>
        [HttpGet("sources")]
        public async Task<IActionResult> GetLogSources()
        {
            try
            {
                var sources = await _logService.GetLogSourcesAsync();

                return Ok(new
                {
                    sources
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get log sources: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to retrieve log sources",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Exports logs.
        /// </summary>
        [HttpPost("export")]
        public async Task<IActionResult> ExportLogs([FromBody] ExportLogsRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Export request is required");
                }

                var exportStream = await _logService.ExportLogsAsync(
                    null,
                    request.StartDate,
                    request.EndDate,
                    request.Format,
                    request.Level);

                return File(new byte[0], "application/zip", $"logs_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}.zip");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to export logs",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes old logs.
        /// </summary>
        [HttpDelete("cleanup")]
        public async Task<IActionResult> CleanupLogs([FromQuery] int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var deletedCount = await _logService.DeleteOldLogsAsync(cutoffDate);

                return Ok(new
                {
                    message = "Log cleanup completed successfully",
                    deletedCount,
                    cutoffDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup logs: {Message}", ex.Message);
                return StatusCode(500, new
                {
                    error = "Failed to cleanup logs",
                    message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Export logs request model.
    /// </summary>
    public class ExportLogsRequest
    {
        /// <summary>
        /// Start date for the export.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for the export.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Log level filter.
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// Log source filter.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Log message filter.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Export format.
        /// </summary>
        public string Format { get; set; } = "json";
    }
}