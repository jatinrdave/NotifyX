using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace NotifyXStudio.Api.Middleware;

/// <summary>
/// Middleware that returns 501 Not Implemented for stub service endpoints
/// </summary>
public class StubServiceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StubServiceMiddleware> _logger;

    public StubServiceMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<StubServiceMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if stub endpoints are disabled
        var enableStubEndpoints = _configuration.GetValue<bool>("FeatureFlags:EnableStubEndpoints", false);
        
        if (!enableStubEndpoints && IsStubEndpoint(context.Request.Path))
        {
            _logger.LogWarning("Stub endpoint accessed: {Path} {Method}", context.Request.Path, context.Request.Method);
            
            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "Not Implemented",
                message = "This endpoint is currently not implemented. Stub services are disabled.",
                path = context.Request.Path.Value,
                method = context.Request.Method,
                timestamp = DateTime.UtcNow
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        await _next(context);
    }

    private static bool IsStubEndpoint(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? "";
        
        // Define stub endpoint patterns
        var stubPatterns = new[]
        {
            "/api/v1/users",
            "/api/v1/projects",
            "/api/v1/tasks",
            "/api/v1/issues",
            "/api/v1/stories",
            "/api/v1/epics",
            "/api/v1/subtasks",
            "/api/v1/milestones",
            "/api/v1/releases",
            "/api/v1/iterations",
            "/api/v1/tags",
            "/api/v1/roles",
            "/api/v1/permissions",
            "/api/v1/tenants",
            "/api/v1/events",
            "/api/v1/files",
            "/api/v1/logs",
            "/api/v1/config",
            "/api/v1/system",
            "/api/v1/status",
            "/api/v1/monitor",
            "/api/v1/alerts",
            "/api/v1/reports",
            "/api/v1/dashboard",
            "/api/v1/integrations",
            "/api/v1/webhooks",
            "/api/v1/queue",
            "/api/v1/repositories",
            "/api/v1/branches",
            "/api/v1/commits",
            "/api/v1/builds",
            "/api/v1/deployments",
            "/api/v1/environments",
            "/api/v1/tests",
            "/api/v1/versions",
            "/api/v1/backups",
            "/api/v1/compliance",
            "/api/v1/credentials",
            "/api/v1/workflows",
            "/api/v1/workflow-executions",
            "/api/v1/workflow-nodes",
            "/api/v1/workflow-edges",
            "/api/v1/workflow-triggers",
            "/api/v1/workflow-runs"
        };

        return stubPatterns.Any(pattern => pathValue.StartsWith(pattern));
    }
}