using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Extension methods for registering middleware.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Registers all middleware services.
        /// </summary>
        public static IServiceCollection AddNotifyXStudioMiddleware(this IServiceCollection services, IConfiguration configuration)
        {
            // Register middleware options
            services.Configure<RateLimitingOptions>(configuration.GetSection("RateLimiting"));
            services.Configure<RequestLoggingOptions>(configuration.GetSection("RequestLogging"));
            services.Configure<SecurityHeadersOptions>(configuration.GetSection("SecurityHeaders"));
            services.Configure<ErrorHandlingOptions>(configuration.GetSection("ErrorHandling"));
            services.Configure<HealthCheckOptions>(configuration.GetSection("HealthCheck"));
            services.Configure<CompressionOptions>(configuration.GetSection("Compression"));
            services.Configure<CachingOptions>(configuration.GetSection("Caching"));
            services.Configure<MetricsOptions>(configuration.GetSection("Metrics"));

            // Register services
            services.AddMemoryCache();
            services.AddSingleton<IMetricsCollector, DefaultMetricsCollector>();

            return services;
        }

        /// <summary>
        /// Configures the middleware pipeline.
        /// </summary>
        public static IApplicationBuilder UseNotifyXStudioMiddleware(this IApplicationBuilder app)
        {
            // Order matters - middleware is executed in the order it's registered
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseMiddleware<CompressionMiddleware>();
            app.UseMiddleware<CachingMiddleware>();
            app.UseMiddleware<RateLimitingMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<HealthCheckMiddleware>();
            app.UseMiddleware<MetricsMiddleware>();

            return app;
        }
    }
}