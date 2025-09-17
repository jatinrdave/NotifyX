using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NotifyXStudio.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotifyXStudioPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Entity Framework DbContext
            services.AddDbContext<NotifyXStudioDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Add repository services
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<IConnectorRepository, ConnectorRepository>();
            services.AddScoped<IRunRepository, RunRepository>();

            return services;
        }
    }
}


