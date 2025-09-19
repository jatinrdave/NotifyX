using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for collecting HTTP request metrics.
    /// </summary>
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MetricsMiddleware> _logger;
        private readonly MetricsOptions _options;
        private readonly IMetricsCollector _metricsCollector;

        public MetricsMiddleware(RequestDelegate next, ILogger<MetricsMiddleware> logger, IOptions<MetricsOptions> options, IMetricsCollector metricsCollector)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _metricsCollector = metricsCollector ?? throw new ArgumentNullException(nameof(metricsCollector));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var response = context.Response;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                await CollectMetricsAsync(context, stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task CollectMetricsAsync(HttpContext context, long elapsedMs)
        {
            var request = context.Request;
            var response = context.Response;

            var metrics = new RequestMetrics
            {
                Method = request.Method,
                Path = request.Path.Value ?? "",
                StatusCode = response.StatusCode,
                ElapsedMs = elapsedMs,
                ContentLength = response.ContentLength ?? 0,
                UserAgent = request.Headers.UserAgent.ToString(),
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                UserId = context.User?.FindFirst("sub")?.Value ?? "anonymous",
                TenantId = context.User?.FindFirst("tenant_id")?.Value ?? "default",
                Timestamp = DateTime.UtcNow
            };

            await _metricsCollector.CollectAsync(metrics);
        }
    }

    /// <summary>
    /// Request metrics model.
    /// </summary>
    public class RequestMetrics
    {
        /// <summary>
        /// HTTP method.
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Request path.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Request duration in milliseconds.
        /// </summary>
        public long ElapsedMs { get; set; }

        /// <summary>
        /// Response content length.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// User agent.
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// Remote IP address.
        /// </summary>
        public string RemoteIpAddress { get; set; } = string.Empty;

        /// <summary>
        /// User ID.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Tenant ID.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Request timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Metrics collector interface.
    /// </summary>
    public interface IMetricsCollector
    {
        /// <summary>
        /// Collects request metrics.
        /// </summary>
        /// <param name="metrics">The request metrics to collect.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CollectAsync(RequestMetrics metrics);
    }

    /// <summary>
    /// Default metrics collector implementation.
    /// </summary>
    public class DefaultMetricsCollector : IMetricsCollector
    {
        private readonly ILogger<DefaultMetricsCollector> _logger;
        private readonly MetricsOptions _options;

        public DefaultMetricsCollector(ILogger<DefaultMetricsCollector> logger, MetricsOptions options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task CollectAsync(RequestMetrics metrics)
        {
            if (!_options.Enabled)
            {
                return;
            }

            try
            {
                // Log metrics
                if (_options.LogMetrics)
                {
                    _logger.LogInformation("Request metrics: {Method} {Path} {StatusCode} {ElapsedMs}ms {ContentLength}bytes {UserId} {TenantId}", 
                        metrics.Method, metrics.Path, metrics.StatusCode, metrics.ElapsedMs, metrics.ContentLength, metrics.UserId, metrics.TenantId);
                }

                // Store metrics in memory cache or database
                if (_options.StoreMetrics)
                {
                    await StoreMetricsAsync(metrics);
                }

                // Send metrics to external systems
                if (_options.SendToExternalSystems)
                {
                    await SendToExternalSystemsAsync(metrics);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect metrics: {Message}", ex.Message);
            }
        }

        private async Task StoreMetricsAsync(RequestMetrics metrics)
        {
            // Implementation would store metrics in a database or cache
            // For now, just log the action
            _logger.LogDebug("Storing metrics for request: {Path}", metrics.Path);
            await Task.CompletedTask;
        }

        private async Task SendToExternalSystemsAsync(RequestMetrics metrics)
        {
            // Implementation would send metrics to external systems like Prometheus, InfluxDB, etc.
            // For now, just log the action
            _logger.LogDebug("Sending metrics to external systems for request: {Path}", metrics.Path);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Metrics configuration options.
    /// </summary>
    public class MetricsOptions
    {
        /// <summary>
        /// Whether metrics collection is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether to log metrics.
        /// </summary>
        public bool LogMetrics { get; set; } = true;

        /// <summary>
        /// Whether to store metrics.
        /// </summary>
        public bool StoreMetrics { get; set; } = true;

        /// <summary>
        /// Whether to send metrics to external systems.
        /// </summary>
        public bool SendToExternalSystems { get; set; } = false;

        /// <summary>
        /// Metrics retention period in days.
        /// </summary>
        public int RetentionDays { get; set; } = 30;

        /// <summary>
        /// Whether to collect detailed metrics.
        /// </summary>
        public bool CollectDetailedMetrics { get; set; } = true;

        /// <summary>
        /// Whether to collect user-specific metrics.
        /// </summary>
        public bool CollectUserMetrics { get; set; } = true;

        /// <summary>
        /// Whether to collect tenant-specific metrics.
        /// </summary>
        public bool CollectTenantMetrics { get; set; } = true;
    }
}