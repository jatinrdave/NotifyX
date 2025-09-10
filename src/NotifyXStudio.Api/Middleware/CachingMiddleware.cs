using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for HTTP response caching.
    /// </summary>
    public class CachingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CachingMiddleware> _logger;
        private readonly IMemoryCache _cache;
        private readonly CachingOptions _options;

        public CachingMiddleware(RequestDelegate next, ILogger<CachingMiddleware> logger, IMemoryCache cache, CachingOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            var request = context.Request;
            var response = context.Response;

            // Check if request should be cached
            if (!ShouldCacheRequest(request))
            {
                await _next(context);
                return;
            }

            // Generate cache key
            var cacheKey = GenerateCacheKey(request);
            
            // Try to get cached response
            if (_cache.TryGetValue(cacheKey, out CachedResponse? cachedResponse))
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                await WriteCachedResponseAsync(response, cachedResponse);
                return;
            }

            // Cache miss - execute request and cache response
            _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
            await ExecuteAndCacheResponseAsync(context, cacheKey);
        }

        private bool ShouldCacheRequest(HttpRequest request)
        {
            // Only cache GET requests
            if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Check if path is cacheable
            var path = request.Path.Value?.ToLowerInvariant() ?? "";
            if (!_options.CacheablePaths.Any(cp => path.StartsWith(cp, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // Check if path is excluded
            if (_options.ExcludedPaths.Any(ep => path.StartsWith(ep, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // Check if user is authenticated (optional)
            if (_options.RequireAuthentication && !request.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                return false;
            }

            return true;
        }

        private string GenerateCacheKey(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            
            // Add method and path
            keyBuilder.Append(request.Method.ToUpperInvariant());
            keyBuilder.Append(':');
            keyBuilder.Append(request.Path.Value);

            // Add query string
            if (request.QueryString.HasValue)
            {
                keyBuilder.Append(':');
                keyBuilder.Append(request.QueryString.Value);
            }

            // Add user ID if caching per user
            if (_options.CachePerUser && request.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = request.HttpContext.User.FindFirst("sub")?.Value ?? "anonymous";
                keyBuilder.Append(':');
                keyBuilder.Append(userId);
            }

            // Add tenant ID if caching per tenant
            if (_options.CachePerTenant)
            {
                var tenantId = request.HttpContext.User.FindFirst("tenant_id")?.Value ?? "default";
                keyBuilder.Append(':');
                keyBuilder.Append(tenantId);
            }

            return keyBuilder.ToString();
        }

        private async Task WriteCachedResponseAsync(HttpResponse response, CachedResponse cachedResponse)
        {
            response.StatusCode = cachedResponse.StatusCode;
            response.ContentType = cachedResponse.ContentType;

            // Copy headers
            foreach (var header in cachedResponse.Headers)
            {
                response.Headers[header.Key] = header.Value;
            }

            // Write body
            await response.Body.WriteAsync(cachedResponse.Body, 0, cachedResponse.Body.Length);
        }

        private async Task ExecuteAndCacheResponseAsync(HttpContext context, string cacheKey)
        {
            var response = context.Response;
            var originalBodyStream = response.Body;

            try
            {
                using var responseBodyStream = new MemoryStream();
                response.Body = responseBodyStream;

                await _next(context);

                // Check if response should be cached
                if (!ShouldCacheResponse(response))
                {
                    // Copy to original stream and return
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalBodyStream);
                    return;
                }

                // Create cached response
                var cachedResponse = new CachedResponse
                {
                    StatusCode = response.StatusCode,
                    ContentType = response.ContentType ?? "application/json",
                    Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                    Body = responseBodyStream.ToArray(),
                    CachedAt = DateTime.UtcNow
                };

                // Cache the response
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.DefaultCacheDurationSeconds),
                    SlidingExpiration = TimeSpan.FromSeconds(_options.SlidingExpirationSeconds),
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(cacheKey, cachedResponse, cacheOptions);

                // Copy to original stream
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            finally
            {
                response.Body = originalBodyStream;
            }
        }

        private bool ShouldCacheResponse(HttpResponse response)
        {
            // Only cache successful responses
            if (response.StatusCode < 200 || response.StatusCode >= 300)
            {
                return false;
            }

            // Check content type
            var contentType = response.ContentType?.ToLowerInvariant();
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }

            // Check if content type is cacheable
            if (!_options.CacheableContentTypes.Any(ct => contentType.Contains(ct)))
            {
                return false;
            }

            // Check if response has cache control headers
            if (response.Headers.ContainsKey("Cache-Control"))
            {
                var cacheControl = response.Headers["Cache-Control"].ToString().ToLowerInvariant();
                if (cacheControl.Contains("no-cache") || cacheControl.Contains("no-store"))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Cached response model.
    /// </summary>
    public class CachedResponse
    {
        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Content type.
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Response headers.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// Response body.
        /// </summary>
        public byte[] Body { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// When the response was cached.
        /// </summary>
        public DateTime CachedAt { get; set; }
    }

    /// <summary>
    /// Caching configuration options.
    /// </summary>
    public class CachingOptions
    {
        /// <summary>
        /// Whether caching is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Default cache duration in seconds.
        /// </summary>
        public int DefaultCacheDurationSeconds { get; set; } = 300; // 5 minutes

        /// <summary>
        /// Sliding expiration in seconds.
        /// </summary>
        public int SlidingExpirationSeconds { get; set; } = 60; // 1 minute

        /// <summary>
        /// Whether to cache per user.
        /// </summary>
        public bool CachePerUser { get; set; } = false;

        /// <summary>
        /// Whether to cache per tenant.
        /// </summary>
        public bool CachePerTenant { get; set; } = true;

        /// <summary>
        /// Whether authentication is required for caching.
        /// </summary>
        public bool RequireAuthentication { get; set; } = false;

        /// <summary>
        /// Paths that should be cached.
        /// </summary>
        public HashSet<string> CacheablePaths { get; set; } = new()
        {
            "/api/connectors",
            "/api/workflows",
            "/api/credentials"
        };

        /// <summary>
        /// Paths that should be excluded from caching.
        /// </summary>
        public HashSet<string> ExcludedPaths { get; set; } = new()
        {
            "/api/auth",
            "/api/health",
            "/api/metrics"
        };

        /// <summary>
        /// Content types that should be cached.
        /// </summary>
        public HashSet<string> CacheableContentTypes { get; set; } = new()
        {
            "application/json",
            "application/xml",
            "text/plain",
            "text/html"
        };
    }
}