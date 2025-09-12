using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Services;
using NotifyX.Core.HealthChecks;
using NotifyX.Core.Models;

namespace NotifyX.Core.Extensions;

/// <summary>
/// Extension methods for configuring NotifyX services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds NotifyX core services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotifyX(this IServiceCollection services, IConfiguration configuration)
    {
        // Register core services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRuleEngine, RuleEngine>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IBulkOperationsService, BulkOperationsService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IQueueService, InMemoryQueueService>();
        services.AddScoped<IPriorityQueueService, InMemoryQueueService>();
        services.AddScoped<IDeadLetterQueueService, DeadLetterQueueService>();
        services.AddScoped<IWorkerService, NotificationWorkerService>();

        // Register configuration
        services.Configure<NotifyXOptions>(configuration.GetSection("NotifyX"));
        services.Configure<AuthenticationOptions>(configuration.GetSection("NotifyX:Authentication"));
        services.Configure<AuditOptions>(configuration.GetSection("NotifyX:Audit"));
        services.Configure<QueueOptions>(configuration.GetSection("NotifyX:Queue"));
        services.Configure<WorkerOptions>(configuration.GetSection("NotifyX:Worker"));

        // Register health checks
        services.AddHealthChecks()
            .AddCheck<NotificationServiceHealthCheck>("notification-service")
            .AddCheck<RuleEngineHealthCheck>("rule-engine")
            .AddCheck<TemplateServiceHealthCheck>("template-service");

        return services;
    }

    /// <summary>
    /// Adds NotifyX core services to the service collection with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotifyX(this IServiceCollection services, Action<NotifyXOptions> configureOptions)
    {
        // Register core services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRuleEngine, RuleEngine>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IBulkOperationsService, BulkOperationsService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IQueueService, InMemoryQueueService>();
        services.AddScoped<IPriorityQueueService, InMemoryQueueService>();
        services.AddScoped<IDeadLetterQueueService, DeadLetterQueueService>();
        services.AddScoped<IWorkerService, NotificationWorkerService>();

        // Register configuration
        services.Configure(configureOptions);

        // Register health checks
        services.AddHealthChecks()
            .AddCheck<NotificationServiceHealthCheck>("notification-service")
            .AddCheck<RuleEngineHealthCheck>("rule-engine")
            .AddCheck<TemplateServiceHealthCheck>("template-service");

        return services;
    }

    /// <summary>
    /// Adds a notification provider to the service collection.
    /// </summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, INotificationProvider
    {
        services.AddScoped<INotificationProvider, TProvider>();
        return services;
    }

    /// <summary>
    /// Adds a notification provider to the service collection with configuration.
    /// </summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationProvider<TProvider, TOptions>(
        this IServiceCollection services, 
        IConfiguration configuration)
        where TProvider : class, INotificationProvider
        where TOptions : class
    {
        services.AddScoped<INotificationProvider, TProvider>();
        services.Configure<TOptions>(configuration);
        return services;
    }

    /// <summary>
    /// Adds a notification provider to the service collection with custom options.
    /// </summary>
    /// <typeparam name="TProvider">The provider type.</typeparam>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotificationProvider<TProvider, TOptions>(
        this IServiceCollection services, 
        Action<TOptions> configureOptions)
        where TProvider : class, INotificationProvider
        where TOptions : class
    {
        services.AddScoped<INotificationProvider, TProvider>();
        services.Configure(configureOptions);
        return services;
    }
}

/// <summary>
/// Configuration options for NotifyX.
/// </summary>
public sealed class NotifyXOptions
{
    /// <summary>
    /// Whether NotifyX is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Default tenant ID.
    /// </summary>
    public string DefaultTenantId { get; set; } = "default";

    /// <summary>
    /// Maximum number of notifications to process in parallel.
    /// </summary>
    public int MaxConcurrentNotifications { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Default delivery options.
    /// </summary>
    public DeliveryOptions DefaultDeliveryOptions { get; set; } = new();

    /// <summary>
    /// Channel configurations.
    /// </summary>
    public Dictionary<NotificationChannel, ChannelConfiguration> ChannelConfigurations { get; set; } = new();

    /// <summary>
    /// Whether to enable rule engine.
    /// </summary>
    public bool EnableRuleEngine { get; set; } = true;

    /// <summary>
    /// Whether to enable template service.
    /// </summary>
    public bool EnableTemplateService { get; set; } = true;

    /// <summary>
    /// Whether to enable escalation.
    /// </summary>
    public bool EnableEscalation { get; set; } = true;

    /// <summary>
    /// Whether to enable aggregation.
    /// </summary>
    public bool EnableAggregation { get; set; } = true;

    /// <summary>
    /// Whether to enable rate limiting.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Rate limiting configuration.
    /// </summary>
    public RateLimitingOptions RateLimiting { get; set; } = new();

    /// <summary>
    /// Retry configuration.
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Observability configuration.
    /// </summary>
    public ObservabilityOptions Observability { get; set; } = new();
}

/// <summary>
/// Rate limiting configuration options.
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// Rate limit per tenant per minute.
    /// </summary>
    public int PerTenantPerMinute { get; set; } = 1000;

    /// <summary>
    /// Rate limit per tenant per hour.
    /// </summary>
    public int PerTenantPerHour { get; set; } = 10000;

    /// <summary>
    /// Rate limit per tenant per day.
    /// </summary>
    public int PerTenantPerDay { get; set; } = 100000;

    /// <summary>
    /// Rate limit per recipient per minute.
    /// </summary>
    public int PerRecipientPerMinute { get; set; } = 10;

    /// <summary>
    /// Rate limit per recipient per hour.
    /// </summary>
    public int PerRecipientPerHour { get; set; } = 100;

    /// <summary>
    /// Rate limit per recipient per day.
    /// </summary>
    public int PerRecipientPerDay { get; set; } = 1000;
}

/// <summary>
/// Retry configuration options.
/// </summary>
public sealed class RetryOptions
{
    /// <summary>
    /// Maximum number of retry attempts.
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Initial delay between retry attempts.
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum delay between retry attempts.
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Backoff multiplier for exponential backoff.
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Whether to use exponential backoff.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Whether to use jitter in retry delays.
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Observability configuration options.
/// </summary>
public sealed class ObservabilityOptions
{
    /// <summary>
    /// Whether to enable detailed logging.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable performance metrics.
    /// </summary>
    public bool EnablePerformanceMetrics { get; set; } = true;

    /// <summary>
    /// Whether to enable health checks.
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Whether to enable tracing.
    /// </summary>
    public bool EnableTracing { get; set; } = false;

    /// <summary>
    /// Whether to enable audit logging.
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;
}