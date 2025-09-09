using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NotifyX.SDK;

namespace NotifyX.SDK.Extensions;

/// <summary>
/// Extension methods for configuring NotifyX SDK services in dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds NotifyX SDK services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotifyXSDK(this IServiceCollection services, IConfiguration configuration)
    {
        // Register client options
        services.Configure<NotifyXClientOptions>(configuration.GetSection("NotifyX:Client"));

        // Register the client
        services.AddScoped<NotifyXClient>();

        return services;
    }

    /// <summary>
    /// Adds NotifyX SDK services to the service collection with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure client options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotifyXSDK(this IServiceCollection services, Action<NotifyXClientOptions> configureOptions)
    {
        // Register client options
        services.Configure(configureOptions);

        // Register the client
        services.AddScoped<NotifyXClient>();

        return services;
    }

    /// <summary>
    /// Adds NotifyX SDK services to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNotifyXSDK(this IServiceCollection services)
    {
        // Register client options with defaults
        services.Configure<NotifyXClientOptions>(_ => { });

        // Register the client
        services.AddScoped<NotifyXClient>();

        return services;
    }
}