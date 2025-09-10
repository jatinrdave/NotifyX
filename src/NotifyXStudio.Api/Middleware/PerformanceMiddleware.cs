using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for performance monitoring and request/response logging.
    /// </summary>
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMiddleware> _logger;
        private readonly PerformanceMiddlewareOptions _options;

        public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger, PerformanceMiddlewareOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();
            
            // Add request ID to context
            context.Items["RequestId"] = requestId;
            context.Response.Headers.Add("X-Request-ID", requestId);

            // Log request
            if (_options.LogRequests)
            {
                await LogRequestAsync(context, requestId);
            }

            // Add performance headers
            if (_options.AddPerformanceHeaders)
            {
                context.Response.Headers.Add("X-Response-Time", "0ms");
                context.Response.Headers.Add("X-Start-Time", DateTime.UtcNow.ToString("O"));
            }

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Log error
                _logger.LogError(ex, "Request {RequestId} failed after {Duration}ms: {Method} {Path}", 
                    requestId, stopwatch.ElapsedMilliseconds, context.Request.Method, context.Request.Path);

                // Add error headers
                if (_options.AddPerformanceHeaders)
                {
                    context.Response.Headers.Add("X-Error", "true");
                    context.Response.Headers.Add("X-Error-Type", ex.GetType().Name);
                }

                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Update performance headers
                if (_options.AddPerformanceHeaders)
                {
                    context.Response.Headers["X-Response-Time"] = $"{stopwatch.ElapsedMilliseconds}ms";
                }

                // Log response
                if (_options.LogResponses)
                {
                    await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds);
                }

                // Log performance metrics
                if (_options.LogPerformanceMetrics)
                {
                    LogPerformanceMetrics(context, requestId, stopwatch.ElapsedMilliseconds);
                }
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId)
        {
            var request = new
            {
                RequestId = requestId,
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.Value,
                Headers = GetSafeHeaders(context.Request.Headers),
                ContentType = context.Request.ContentType,
                ContentLength = context.Request.ContentLength,
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Request {RequestId}: {Request}", requestId, JsonSerializer.Serialize(request));
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, long durationMs)
        {
            var response = new
            {
                RequestId = requestId,
                StatusCode = context.Response.StatusCode,
                ContentType = context.Response.ContentType,
                ContentLength = context.Response.ContentLength,
                Headers = GetSafeHeaders(context.Response.Headers),
                DurationMs = durationMs,
                Timestamp = DateTime.UtcNow
            };

            var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(logLevel, "Response {RequestId}: {Response}", requestId, JsonSerializer.Serialize(response));
        }

        private void LogPerformanceMetrics(HttpContext context, string requestId, long durationMs)
        {
            var metrics = new
            {
                RequestId = requestId,
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                StatusCode = context.Response.StatusCode,
                DurationMs = durationMs,
                MemoryUsageMB = GC.GetTotalMemory(false) / 1024 / 1024,
                Timestamp = DateTime.UtcNow
            };

            // Log as structured data for metrics collection
            _logger.LogInformation("PerformanceMetrics: {Metrics}", JsonSerializer.Serialize(metrics));
        }

        private Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
        {
            var safeHeaders = new Dictionary<string, string>();
            var sensitiveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Authorization",
                "Cookie",
                "X-API-Key",
                "X-Auth-Token",
                "X-Access-Token"
            };

            foreach (var header in headers)
            {
                if (sensitiveHeaders.Contains(header.Key))
                {
                    safeHeaders[header.Key] = "[REDACTED]";
                }
                else
                {
                    safeHeaders[header.Key] = string.Join(", ", header.Value);
                }
            }

            return safeHeaders;
        }
    }

    /// <summary>
    /// Options for the performance middleware.
    /// </summary>
    public class PerformanceMiddlewareOptions
    {
        /// <summary>
        /// Whether to log incoming requests.
        /// </summary>
        public bool LogRequests { get; set; } = true;

        /// <summary>
        /// Whether to log outgoing responses.
        /// </summary>
        public bool LogResponses { get; set; } = true;

        /// <summary>
        /// Whether to log performance metrics.
        /// </summary>
        public bool LogPerformanceMetrics { get; set; } = true;

        /// <summary>
        /// Whether to add performance headers to responses.
        /// </summary>
        public bool AddPerformanceHeaders { get; set; } = true;

        /// <summary>
        /// Minimum duration in milliseconds to log as slow request.
        /// </summary>
        public long SlowRequestThresholdMs { get; set; } = 1000;

        /// <summary>
        /// Whether to log request/response bodies.
        /// </summary>
        public bool LogBodies { get; set; } = false;

        /// <summary>
        /// Maximum body size to log in bytes.
        /// </summary>
        public int MaxBodySizeBytes { get; set; } = 1024 * 10; // 10KB
    }
}