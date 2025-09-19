using Microsoft.Extensions.DependencyInjection;
using NotifyXStudio.Application.Services;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotifyXStudioApplication(this IServiceCollection services)
        {
            // Register real service implementations
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ITaskService, TaskService>();

            return services;
        }
    }
}