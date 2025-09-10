using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for adding security headers to HTTP responses.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;
        private readonly SecurityHeadersOptions _options;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger, SecurityHeadersOptions options)
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

            var response = context.Response;

            // Add security headers
            AddSecurityHeaders(response);

            // Add CORS headers if enabled
            if (_options.EnableCors)
            {
                AddCorsHeaders(response);
            }

            // Add HSTS header for HTTPS requests
            if (context.Request.IsHttps && _options.EnableHsts)
            {
                AddHstsHeader(response);
            }

            // Add CSP header if enabled
            if (_options.EnableCsp)
            {
                AddCspHeader(response);
            }

            // Add X-Frame-Options header
            if (_options.EnableXFrameOptions)
            {
                AddXFrameOptionsHeader(response);
            }

            // Add X-Content-Type-Options header
            if (_options.EnableXContentTypeOptions)
            {
                AddXContentTypeOptionsHeader(response);
            }

            // Add X-XSS-Protection header
            if (_options.EnableXXssProtection)
            {
                AddXXssProtectionHeader(response);
            }

            // Add Referrer-Policy header
            if (_options.EnableReferrerPolicy)
            {
                AddReferrerPolicyHeader(response);
            }

            // Add Permissions-Policy header
            if (_options.EnablePermissionsPolicy)
            {
                AddPermissionsPolicyHeader(response);
            }

            await _next(context);
        }

        private void AddSecurityHeaders(HttpResponse response)
        {
            // Add custom security headers
            foreach (var header in _options.CustomHeaders)
            {
                if (!response.Headers.ContainsKey(header.Key))
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }
        }

        private void AddCorsHeaders(HttpResponse response)
        {
            if (!response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                response.Headers.Add("Access-Control-Allow-Origin", _options.CorsOrigin);
            }

            if (!response.Headers.ContainsKey("Access-Control-Allow-Methods"))
            {
                response.Headers.Add("Access-Control-Allow-Methods", _options.CorsMethods);
            }

            if (!response.Headers.ContainsKey("Access-Control-Allow-Headers"))
            {
                response.Headers.Add("Access-Control-Allow-Headers", _options.CorsHeaders);
            }

            if (!response.Headers.ContainsKey("Access-Control-Allow-Credentials"))
            {
                response.Headers.Add("Access-Control-Allow-Credentials", _options.CorsCredentials.ToString().ToLowerInvariant());
            }

            if (!response.Headers.ContainsKey("Access-Control-Max-Age"))
            {
                response.Headers.Add("Access-Control-Max-Age", _options.CorsMaxAge.ToString());
            }
        }

        private void AddHstsHeader(HttpResponse response)
        {
            if (!response.Headers.ContainsKey("Strict-Transport-Security"))
            {
                var hstsValue = $"max-age={_options.HstsMaxAge}";
                if (_options.HstsIncludeSubDomains)
                {
                    hstsValue += "; includeSubDomains";
                }
                if (_options.HstsPreload)
                {
                    hstsValue += "; preload";
                }
                response.Headers.Add("Strict-Transport-Security", hstsValue);
            }
        }

        private void AddCspHeader(HttpResponse response)
        {
            if (!response.Headers.ContainsKey("Content-Security-Policy"))
            {
                var cspValue = string.Join("; ", _options.CspDirectives.Select(d => $"{d.Key} {d.Value}"));
                response.Headers.Add("Content-Security-Policy", cspValue);
            }
        }

        private void AddXFrameOptionsHeader(HttpResponse response)
        {
            if (!response.Headers.ContainsKey("X-Frame-Options"))
            {
                response.Headers.Add("X-Frame-Options", _options.XFrameOptions);
            }
        }

        private void AddXContentTypeOptionsHeader(HttpResponse response)
        {
            if (!response.Headers.ContainsKey("X-Content-Type-Options"))
            {
                response.Headers.Add("X-Content-Type-Options", "nosniff");
            }
        }

        private void AddXXssProtectionHeader(HttpResponse response)
        {
            if (!response.Headers.ContainsKey("X-XSS-Protection"))
            {
                response.Headers.Add("X-XSS-Protection", "1; mode=block");
            }
        }

        private void AddReferrerPolicyHeader(HttpResponse response)
        {
            if (!response.Headers.ContainsKey("Referrer-Policy"))
            {
                response.Headers.Add("Referrer-Policy", _options.ReferrerPolicy);
            }
        }

        private void AddPermissionsPolicyHeader(HttpResponse response)
        {
            if (!response.Headers.ContainsKey("Permissions-Policy"))
            {
                var permissionsValue = string.Join(", ", _options.PermissionsPolicy.Select(p => $"{p.Key}=({p.Value})"));
                response.Headers.Add("Permissions-Policy", permissionsValue);
            }
        }
    }

    /// <summary>
    /// Security headers configuration options.
    /// </summary>
    public class SecurityHeadersOptions
    {
        /// <summary>
        /// Whether security headers are enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether CORS headers are enabled.
        /// </summary>
        public bool EnableCors { get; set; } = true;

        /// <summary>
        /// CORS origin value.
        /// </summary>
        public string CorsOrigin { get; set; } = "*";

        /// <summary>
        /// CORS methods value.
        /// </summary>
        public string CorsMethods { get; set; } = "GET, POST, PUT, DELETE, OPTIONS";

        /// <summary>
        /// CORS headers value.
        /// </summary>
        public string CorsHeaders { get; set; } = "Content-Type, Authorization, X-Requested-With";

        /// <summary>
        /// CORS credentials value.
        /// </summary>
        public bool CorsCredentials { get; set; } = true;

        /// <summary>
        /// CORS max age in seconds.
        /// </summary>
        public int CorsMaxAge { get; set; } = 86400; // 24 hours

        /// <summary>
        /// Whether HSTS is enabled.
        /// </summary>
        public bool EnableHsts { get; set; } = true;

        /// <summary>
        /// HSTS max age in seconds.
        /// </summary>
        public int HstsMaxAge { get; set; } = 31536000; // 1 year

        /// <summary>
        /// Whether HSTS includes subdomains.
        /// </summary>
        public bool HstsIncludeSubDomains { get; set; } = true;

        /// <summary>
        /// Whether HSTS preload is enabled.
        /// </summary>
        public bool HstsPreload { get; set; } = false;

        /// <summary>
        /// Whether Content Security Policy is enabled.
        /// </summary>
        public bool EnableCsp { get; set; } = true;

        /// <summary>
        /// CSP directives.
        /// </summary>
        public Dictionary<string, string> CspDirectives { get; set; } = new()
        {
            { "default-src", "'self'" },
            { "script-src", "'self' 'unsafe-inline' 'unsafe-eval'" },
            { "style-src", "'self' 'unsafe-inline'" },
            { "img-src", "'self' data: https:" },
            { "font-src", "'self' data:" },
            { "connect-src", "'self'" },
            { "frame-ancestors", "'none'" }
        };

        /// <summary>
        /// Whether X-Frame-Options is enabled.
        /// </summary>
        public bool EnableXFrameOptions { get; set; } = true;

        /// <summary>
        /// X-Frame-Options value.
        /// </summary>
        public string XFrameOptions { get; set; } = "DENY";

        /// <summary>
        /// Whether X-Content-Type-Options is enabled.
        /// </summary>
        public bool EnableXContentTypeOptions { get; set; } = true;

        /// <summary>
        /// Whether X-XSS-Protection is enabled.
        /// </summary>
        public bool EnableXXssProtection { get; set; } = true;

        /// <summary>
        /// Whether Referrer-Policy is enabled.
        /// </summary>
        public bool EnableReferrerPolicy { get; set; } = true;

        /// <summary>
        /// Referrer-Policy value.
        /// </summary>
        public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

        /// <summary>
        /// Whether Permissions-Policy is enabled.
        /// </summary>
        public bool EnablePermissionsPolicy { get; set; } = true;

        /// <summary>
        /// Permissions-Policy directives.
        /// </summary>
        public Dictionary<string, string> PermissionsPolicy { get; set; } = new()
        {
            { "camera", "()" },
            { "microphone", "()" },
            { "geolocation", "()" },
            { "payment", "()" },
            { "usb", "()" },
            { "magnetometer", "()" },
            { "accelerometer", "()" },
            { "gyroscope", "()" }
        };

        /// <summary>
        /// Custom security headers.
        /// </summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
    }
}