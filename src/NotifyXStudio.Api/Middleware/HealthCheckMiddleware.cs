using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for health checks and system status monitoring.
    /// </summary>
    public class HealthCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HealthCheckMiddleware> _logger;
        private readonly HealthCheckOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public HealthCheckMiddleware(RequestDelegate next, ILogger<HealthCheckMiddleware> logger, HealthCheckOptions options, IServiceProvider serviceProvider)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value?.ToLowerInvariant();
            
            if (path == "/health" || path == "/health/ready" || path == "/health/live")
            {
                await HandleHealthCheckAsync(context, path);
                return;
            }

            await _next(context);
        }

        private async Task HandleHealthCheckAsync(HttpContext context, string path)
        {
            var healthStatus = await GetHealthStatusAsync(path);
            var statusCode = GetStatusCode(healthStatus.Status);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var healthResponse = new HealthResponse
            {
                Status = healthStatus.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Duration = healthStatus.TotalDuration,
                Checks = healthStatus.Entries.Select(entry => new HealthCheck
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Duration = entry.Value.Duration,
                    Description = entry.Value.Description,
                    Data = entry.Value.Data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    Exception = entry.Value.Exception?.Message
                }).ToList()
            };

            var jsonResponse = JsonSerializer.Serialize(healthResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private async Task<HealthReport> GetHealthStatusAsync(string path)
        {
            var healthCheckService = _serviceProvider.GetService<HealthCheckService>();
            if (healthCheckService == null)
            {
                // Fallback to basic health check
                return new HealthReport(new Dictionary<string, HealthReportEntry>
                {
                    ["basic"] = new HealthReportEntry(HealthStatus.Healthy, "Basic health check", TimeSpan.Zero, null, null)
                }, TimeSpan.Zero);
            }

            try
            {
                var healthReport = await healthCheckService.CheckHealthAsync();
                return healthReport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed: {Message}", ex.Message);
                return new HealthReport(new Dictionary<string, HealthReportEntry>
                {
                    ["error"] = new HealthReportEntry(HealthStatus.Unhealthy, "Health check failed", TimeSpan.Zero, ex, null)
                }, TimeSpan.Zero);
            }
        }

        private int GetStatusCode(HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Healthy => 200,
                HealthStatus.Degraded => 200,
                HealthStatus.Unhealthy => 503,
                _ => 503
            };
        }
    }

    /// <summary>
    /// Health check response model.
    /// </summary>
    public class HealthResponse
    {
        /// <summary>
        /// Overall health status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Health check timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Total duration of health checks.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Individual health check results.
        /// </summary>
        public List<HealthCheck> Checks { get; set; } = new();
    }

    /// <summary>
    /// Individual health check result.
    /// </summary>
    public class HealthCheck
    {
        /// <summary>
        /// Health check name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Health check status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Health check duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Health check description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Additional health check data.
        /// </summary>
        public Dictionary<string, object>? Data { get; set; }

        /// <summary>
        /// Exception message if health check failed.
        /// </summary>
        public string? Exception { get; set; }
    }

    /// <summary>
    /// Health check configuration options.
    /// </summary>
    public class HealthCheckOptions
    {
        /// <summary>
        /// Whether health checks are enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Health check endpoint path.
        /// </summary>
        public string EndpointPath { get; set; } = "/health";

        /// <summary>
        /// Readiness check endpoint path.
        /// </summary>
        public string ReadinessPath { get; set; } = "/health/ready";

        /// <summary>
        /// Liveness check endpoint path.
        /// </summary>
        public string LivenessPath { get; set; } = "/health/live";

        /// <summary>
        /// Health check timeout.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Whether to include detailed health check information.
        /// </summary>
        public bool IncludeDetails { get; set; } = true;

        /// <summary>
        /// Whether to include exception details in health check responses.
        /// </summary>
        public bool IncludeExceptions { get; set; } = false;
    }
}