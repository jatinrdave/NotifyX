using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for rate limiting requests by IP address and user.
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IMemoryCache _cache;
        private readonly RateLimitingOptions _options;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IMemoryCache cache, RateLimitingOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientId(context);
            var endpoint = GetEndpoint(context);
            var rateLimitKey = $"{clientId}:{endpoint}";

            var rateLimitInfo = await GetRateLimitInfoAsync(rateLimitKey);

            if (rateLimitInfo.IsRateLimited)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Requests: {RequestCount}/{Limit} in window {WindowMs}ms", 
                    clientId, endpoint, rateLimitInfo.RequestCount, rateLimitInfo.Limit, rateLimitInfo.WindowMs);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Add("X-RateLimit-Limit", rateLimitInfo.Limit.ToString());
                context.Response.Headers.Add("X-RateLimit-Remaining", rateLimitInfo.Remaining.ToString());
                context.Response.Headers.Add("X-RateLimit-Reset", rateLimitInfo.ResetTime.ToString("O"));
                context.Response.Headers.Add("Retry-After", rateLimitInfo.ResetTime.Subtract(DateTime.UtcNow).TotalSeconds.ToString("F0"));

                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            // Add rate limit headers to response
            context.Response.Headers.Add("X-RateLimit-Limit", rateLimitInfo.Limit.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", rateLimitInfo.Remaining.ToString());
            context.Response.Headers.Add("X-RateLimit-Reset", rateLimitInfo.ResetTime.ToString("O"));

            await _next(context);
        }

        private string GetClientId(HttpContext context)
        {
            // Try to get user ID from claims first
            var userId = context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }

            // Fall back to IP address
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "unknown";
            }

            return $"ip:{ipAddress}";
        }

        private string GetEndpoint(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            var method = context.Request.Method.ToUpperInvariant();

            // Group similar endpoints
            if (path.StartsWith("/api/workflows/") && path.EndsWith("/runs"))
            {
                return $"{method}:workflow-runs";
            }
            else if (path.StartsWith("/api/workflows/"))
            {
                return $"{method}:workflows";
            }
            else if (path.StartsWith("/api/connectors/"))
            {
                return $"{method}:connectors";
            }
            else if (path.StartsWith("/api/credentials/"))
            {
                return $"{method}:credentials";
            }
            else if (path.StartsWith("/api/"))
            {
                return $"{method}:api";
            }
            else
            {
                return $"{method}:{path}";
            }
        }

        private async Task<RateLimitInfo> GetRateLimitInfoAsync(string rateLimitKey)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMilliseconds(-_options.WindowMs);

            // Get or create rate limit entry
            var cacheKey = $"rate_limit:{rateLimitKey}";
            var rateLimitEntry = _cache.Get<RateLimitEntry>(cacheKey);

            if (rateLimitEntry == null || rateLimitEntry.WindowStart < windowStart)
            {
                // Create new window
                rateLimitEntry = new RateLimitEntry
                {
                    WindowStart = now,
                    RequestCount = 1,
                    Limit = _options.RequestsPerWindow
                };
            }
            else
            {
                // Increment existing window
                rateLimitEntry.RequestCount++;
            }

            // Update cache
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_options.WindowMs * 2)
            };
            _cache.Set(cacheKey, rateLimitEntry, cacheOptions);

            var resetTime = rateLimitEntry.WindowStart.AddMilliseconds(_options.WindowMs);
            var remaining = Math.Max(0, rateLimitEntry.Limit - rateLimitEntry.RequestCount);

            return new RateLimitInfo
            {
                RequestCount = rateLimitEntry.RequestCount,
                Limit = rateLimitEntry.Limit,
                Remaining = remaining,
                ResetTime = resetTime,
                WindowMs = _options.WindowMs,
                IsRateLimited = rateLimitEntry.RequestCount > rateLimitEntry.Limit
            };
        }

        private class RateLimitEntry
        {
            public DateTime WindowStart { get; set; }
            public int RequestCount { get; set; }
            public int Limit { get; set; }
        }
    }

    /// <summary>
    /// Rate limiting configuration options.
    /// </summary>
    public class RateLimitingOptions
    {
        /// <summary>
        /// Number of requests allowed per window.
        /// </summary>
        public int RequestsPerWindow { get; set; } = 100;

        /// <summary>
        /// Window size in milliseconds.
        /// </summary>
        public int WindowMs { get; set; } = 60000; // 1 minute

        /// <summary>
        /// Whether to enable rate limiting.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Rate limit rules for specific endpoints.
        /// </summary>
        public Dictionary<string, RateLimitRule> EndpointRules { get; set; } = new();

        /// <summary>
        /// Rate limit rules for specific users.
        /// </summary>
        public Dictionary<string, RateLimitRule> UserRules { get; set; } = new();

        /// <summary>
        /// Rate limit rules for specific IP addresses.
        /// </summary>
        public Dictionary<string, RateLimitRule> IpRules { get; set; } = new();
    }

    /// <summary>
    /// Rate limit rule for specific clients or endpoints.
    /// </summary>
    public class RateLimitRule
    {
        /// <summary>
        /// Number of requests allowed per window.
        /// </summary>
        public int RequestsPerWindow { get; set; }

        /// <summary>
        /// Window size in milliseconds.
        /// </summary>
        public int WindowMs { get; set; }

        /// <summary>
        /// Whether this rule is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// Rate limit information for a client.
    /// </summary>
    public class RateLimitInfo
    {
        /// <summary>
        /// Current request count in the window.
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Maximum requests allowed in the window.
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Remaining requests in the window.
        /// </summary>
        public int Remaining { get; set; }

        /// <summary>
        /// Time when the rate limit window resets.
        /// </summary>
        public DateTime ResetTime { get; set; }

        /// <summary>
        /// Window size in milliseconds.
        /// </summary>
        public int WindowMs { get; set; }

        /// <summary>
        /// Whether the client is currently rate limited.
        /// </summary>
        public bool IsRateLimited { get; set; }
    }
}