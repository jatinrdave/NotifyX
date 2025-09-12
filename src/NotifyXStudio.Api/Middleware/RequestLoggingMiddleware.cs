using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for logging HTTP requests and responses.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly RequestLoggingOptions _options;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, RequestLoggingOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];
            var correlationId = GetCorrelationId(context);

            // Log request
            await LogRequestAsync(context, requestId, correlationId);

            // Capture response
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request {RequestId} failed with exception: {Message}", requestId, ex.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Log response
                await LogResponseAsync(context, requestId, correlationId, stopwatch.ElapsedMilliseconds);

                // Restore response body
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
                context.Response.Body = originalResponseBodyStream;
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId, string correlationId)
        {
            var request = context.Request;
            var user = context.User?.Identity?.Name ?? "anonymous";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var logMessage = new StringBuilder();
            logMessage.AppendLine($"Request {requestId} started");
            logMessage.AppendLine($"  Method: {request.Method}");
            logMessage.AppendLine($"  Path: {request.Path}");
            logMessage.AppendLine($"  QueryString: {request.QueryString}");
            logMessage.AppendLine($"  User: {user}");
            logMessage.AppendLine($"  IP: {ipAddress}");
            logMessage.AppendLine($"  UserAgent: {request.Headers.UserAgent}");
            logMessage.AppendLine($"  CorrelationId: {correlationId}");

            if (_options.LogHeaders)
            {
                logMessage.AppendLine("  Headers:");
                foreach (var header in request.Headers)
                {
                    if (!_options.SensitiveHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        logMessage.AppendLine($"    {header.Key}: {header.Value}");
                    }
                    else
                    {
                        logMessage.AppendLine($"    {header.Key}: [REDACTED]");
                    }
                }
            }

            if (_options.LogBody && request.ContentLength > 0 && request.ContentLength <= _options.MaxBodySize)
            {
                var body = await ReadRequestBodyAsync(request);
                if (!string.IsNullOrEmpty(body))
                {
                    logMessage.AppendLine($"  Body: {body}");
                }
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, string correlationId, long elapsedMs)
        {
            var response = context.Response;
            var statusCode = response.StatusCode;
            var logLevel = GetLogLevel(statusCode);

            var logMessage = new StringBuilder();
            logMessage.AppendLine($"Request {requestId} completed");
            logMessage.AppendLine($"  StatusCode: {statusCode}");
            logMessage.AppendLine($"  ElapsedMs: {elapsedMs}");
            logMessage.AppendLine($"  CorrelationId: {correlationId}");

            if (_options.LogHeaders)
            {
                logMessage.AppendLine("  Response Headers:");
                foreach (var header in response.Headers)
                {
                    if (!_options.SensitiveHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        logMessage.AppendLine($"    {header.Key}: {header.Value}");
                    }
                    else
                    {
                        logMessage.AppendLine($"    {header.Key}: [REDACTED]");
                    }
                }
            }

            if (_options.LogBody && response.Body.Length > 0 && response.Body.Length <= _options.MaxBodySize)
            {
                var body = await ReadResponseBodyAsync(response.Body);
                if (!string.IsNullOrEmpty(body))
                {
                    logMessage.AppendLine($"  Response Body: {body}");
                }
            }

            _logger.Log(logLevel, logMessage.ToString());
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            try
            {
                request.EnableBuffering();
                request.Body.Position = 0;

                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read request body");
                return string.Empty;
            }
        }

        private async Task<string> ReadResponseBodyAsync(Stream responseBody)
        {
            try
            {
                responseBody.Position = 0;
                using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                responseBody.Position = 0;

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read response body");
                return string.Empty;
            }
        }

        private string GetCorrelationId(HttpContext context)
        {
            // Try to get correlation ID from headers
            if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                return correlationId.ToString();
            }

            // Try to get correlation ID from claims
            var claimCorrelationId = context.User?.FindFirst("correlation_id")?.Value;
            if (!string.IsNullOrEmpty(claimCorrelationId))
            {
                return claimCorrelationId;
            }

            // Generate new correlation ID
            return Guid.NewGuid().ToString("N")[..8];
        }

        private LogLevel GetLogLevel(int statusCode)
        {
            return statusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                >= 300 => LogLevel.Information,
                _ => LogLevel.Information
            };
        }
    }

    /// <summary>
    /// Request logging configuration options.
    /// </summary>
    public class RequestLoggingOptions
    {
        /// <summary>
        /// Whether request logging is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether to log request/response headers.
        /// </summary>
        public bool LogHeaders { get; set; } = true;

        /// <summary>
        /// Whether to log request/response bodies.
        /// </summary>
        public bool LogBody { get; set; } = false;

        /// <summary>
        /// Maximum body size to log in bytes.
        /// </summary>
        public int MaxBodySize { get; set; } = 1024 * 4; // 4KB

        /// <summary>
        /// Headers that contain sensitive information and should be redacted.
        /// </summary>
        public HashSet<string> SensitiveHeaders { get; set; } = new()
        {
            "Authorization",
            "X-API-Key",
            "X-Auth-Token",
            "Cookie",
            "Set-Cookie"
        };

        /// <summary>
        /// Whether to log requests to health check endpoints.
        /// </summary>
        public bool LogHealthChecks { get; set; } = false;

        /// <summary>
        /// Whether to log requests to static files.
        /// </summary>
        public bool LogStaticFiles { get; set; } = false;
    }
}