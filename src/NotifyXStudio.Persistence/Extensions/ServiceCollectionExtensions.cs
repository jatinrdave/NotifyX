using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotifyXStudio.Persistence.Repositories;

namespace NotifyXStudio.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotifyXStudioPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Entity Framework DbContext
            services.AddDbContext<NotifyXStudioDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Add existing repository services
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<IConnectorRepository, ConnectorRepository>();
            services.AddScoped<IRunRepository, RunRepository>();

            // Add core repository services
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();

            return services;
        }
    }
}


