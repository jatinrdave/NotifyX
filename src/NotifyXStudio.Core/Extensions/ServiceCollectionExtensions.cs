using Microsoft.Extensions.DependencyInjection;
using NotifyXStudio.Core.Services;
using NotifyXStudio.Core.Interfaces;

namespace NotifyXStudio.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotifyXStudioCore(this IServiceCollection services)
        {
            // Intentionally do not register stub services. Real core services are registered from NotifyX.Core in API.
            return services;
        }
    }
}
