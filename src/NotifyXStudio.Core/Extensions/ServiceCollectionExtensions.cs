using Microsoft.Extensions.DependencyInjection;
using NotifyXStudio.Core.Services;
using NotifyXStudio.Core.Interfaces;

namespace NotifyXStudio.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotifyXStudioCore(this IServiceCollection services)
        {
            // Register stub services for API compatibility - these will be replaced progressively
            services.AddScoped<IUserService, StubUserService>();
            services.AddScoped<IProjectService, StubProjectService>();
            services.AddScoped<ITaskService, StubTaskService>();
            services.AddScoped<IIssueService, StubIssueService>();
            services.AddScoped<IStoryService, StubStoryService>();
            services.AddScoped<IEpicService, StubEpicService>();
            services.AddScoped<ISubtaskService, StubSubtaskService>();
            services.AddScoped<IMilestoneService, StubMilestoneService>();
            services.AddScoped<IReleaseService, StubReleaseService>();
            services.AddScoped<IIterationService, StubIterationService>();
            services.AddScoped<ITagService, StubTagService>();
            services.AddScoped<IRoleService, StubRoleService>();
            services.AddScoped<IPermissionService, StubPermissionService>();
            services.AddScoped<ITenantService, StubTenantService>();
            services.AddScoped<INotificationService, StubNotificationService>();
            services.AddScoped<IEventService, StubEventService>();
            services.AddScoped<IFileService, StubFileService>();
            services.AddScoped<ILogService, StubLogService>();
            services.AddScoped<IAuditService, StubAuditService>();
            services.AddScoped<IConfigService, StubConfigService>();
            services.AddScoped<ISystemService, StubSystemService>();
            services.AddScoped<IStatusService, StubStatusService>();
            services.AddScoped<IMonitorService, StubMonitorService>();
            services.AddScoped<IAlertService, StubAlertService>();
            services.AddScoped<IReportService, StubReportService>();
            services.AddScoped<IDashboardService, StubDashboardService>();
            services.AddScoped<IIntegrationService, StubIntegrationService>();
            services.AddScoped<IWebhookService, StubWebhookService>();
            services.AddScoped<IQueueService, StubQueueService>();
            services.AddScoped<IRepositoryService, StubRepositoryService>();
            services.AddScoped<IBranchService, StubBranchService>();
            services.AddScoped<ICommitService, StubCommitService>();
            services.AddScoped<IBuildService, StubBuildService>();
            services.AddScoped<IDeployService, StubDeployService>();
            services.AddScoped<IEnvironmentService, StubEnvironmentService>();
            services.AddScoped<ITestService, StubTestService>();
            services.AddScoped<IVersionService, StubVersionService>();
            services.AddScoped<IBackupService, StubBackupService>();
            services.AddScoped<IComplianceService, StubComplianceService>();
            services.AddScoped<ICredentialService, StubCredentialService>();
            services.AddScoped<IWorkflowExecutionService, StubWorkflowExecutionService>();
            services.AddScoped<IWorkflowExecutionNodeService, StubWorkflowExecutionNodeService>();
            services.AddScoped<IWorkflowExecutionEdgeService, StubWorkflowExecutionEdgeService>();
            services.AddScoped<IWorkflowExecutionLogService, StubWorkflowExecutionLogService>();
            services.AddScoped<IWorkflowExecutionTriggerService, StubWorkflowExecutionTriggerService>();
            services.AddScoped<IWorkflowExecutionTriggerLogService, StubWorkflowExecutionTriggerLogService>();
            services.AddScoped<IWorkflowExecutionTriggerLogEntryService, StubWorkflowExecutionTriggerLogEntryService>();
            services.AddScoped<IWorkflowNodeService, StubWorkflowNodeService>();
            services.AddScoped<IWorkflowEdgeService, StubWorkflowEdgeService>();
            services.AddScoped<IWorkflowTriggerService, StubWorkflowTriggerService>();
            services.AddScoped<IWorkflowRunService, StubWorkflowRunService>();
            services.AddScoped<IWorkflowService, StubWorkflowService>();

            return services;
        }
    }
}
