using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NotifyXStudio.Api.Filters;

/// <summary>
/// Filter that returns 501 Not Implemented for stub controllers
/// </summary>
public class StubControllerFilter : IActionFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StubControllerFilter> _logger;

    public StubControllerFilter(IConfiguration configuration, ILogger<StubControllerFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var enableStubEndpoints = _configuration.GetValue<bool>("FeatureFlags:EnableStubEndpoints", false);
        
        if (!enableStubEndpoints && IsStubController(context.Controller))
        {
            _logger.LogWarning("Stub controller accessed: {Controller} {Action}", 
                context.Controller.GetType().Name, context.ActionDescriptor.DisplayName);
            
            context.Result = new ObjectResult(new
            {
                error = "Not Implemented",
                message = "This controller is currently not implemented. Stub services are disabled.",
                controller = context.Controller.GetType().Name,
                action = context.ActionDescriptor.DisplayName,
                timestamp = DateTime.UtcNow
            })
            {
                StatusCode = 501
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed
    }

    private static bool IsStubController(object controller)
    {
        var controllerType = controller.GetType();
        var controllerName = controllerType.Name.ToLowerInvariant();
        
        // Define stub controller patterns
        var stubControllers = new[]
        {
            "usercontroller", "projectcontroller", "taskcontroller", "issuecontroller",
            "storycontroller", "epiccontroller", "subtaskcontroller", "milestonecontroller",
            "releasecontroller", "iterationcontroller", "tagcontroller", "rolecontroller",
            "permissioncontroller", "tenantcontroller", "eventcontroller", "filecontroller",
            "logcontroller", "configcontroller", "systemcontroller", "statuscontroller",
            "monitorcontroller", "alertcontroller", "reportcontroller", "dashboardcontroller",
            "integrationcontroller", "webhookcontroller", "queuecontroller", "repositorycontroller",
            "branchcontroller", "commitcontroller", "buildcontroller", "deploycontroller",
            "environmentcontroller", "testcontroller", "versioncontroller", "backupcontroller",
            "compliancecontroller", "credentialcontroller", "workflowcontroller", "workflowexecutioncontroller",
            "workflownodecontroller", "workflowedgecontroller", "workflowtriggercontroller", "workflowruncontroller"
        };

        return stubControllers.Any(stub => controllerName.Contains(stub));
    }
}