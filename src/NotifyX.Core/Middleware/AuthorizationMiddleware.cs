using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Models;
using System.Security.Claims;

namespace NotifyX.Core.Middleware;

/// <summary>
/// Middleware for handling authorization in NotifyX API.
/// </summary>
public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthorizationMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Skip authorization for certain paths
            if (ShouldSkipAuthorization(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("Unauthenticated request to {Path}", context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            // Check authorization based on the endpoint
            var isAuthorized = await CheckAuthorizationAsync(context);
            
            if (!isAuthorized)
            {
                _logger.LogWarning("Unauthorized request to {Path} by user {UserId}", 
                    context.Request.Path, context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden");
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in authorization middleware");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal Server Error");
        }
    }

    /// <summary>
    /// Checks authorization for the current request.
    /// </summary>
    private async Task<bool> CheckAuthorizationAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();
        var user = context.User;

        // Extract tenant ID from route or query
        var tenantId = ExtractTenantId(context);
        var userTenantId = user.FindFirst("tenant_id")?.Value;

        // Check tenant access
        if (!string.IsNullOrEmpty(tenantId) && userTenantId != tenantId)
        {
            // Check if user has system admin role
            if (!user.IsInRole(SystemRoles.SystemAdmin))
            {
                _logger.LogWarning("User {UserId} attempted to access tenant {TenantId} but belongs to {UserTenantId}", 
                    user.FindFirst(ClaimTypes.NameIdentifier)?.Value, tenantId, userTenantId);
                return false;
            }
        }

        // Check permissions based on endpoint
        return path switch
        {
            var p when p.StartsWith("/api/notifications") => CheckNotificationPermissions(user, method),
            var p when p.StartsWith("/api/rules") => CheckRulePermissions(user, method),
            var p when p.StartsWith("/api/templates") => CheckTemplatePermissions(user, method),
            var p when p.StartsWith("/api/subscriptions") => CheckSubscriptionPermissions(user, method),
            var p when p.StartsWith("/api/users") => CheckUserPermissions(user, method),
            var p when p.StartsWith("/api/roles") => CheckRolePermissions(user, method),
            var p when p.StartsWith("/api/apikeys") => CheckApiKeyPermissions(user, method),
            var p when p.StartsWith("/api/audit") => CheckAuditPermissions(user, method),
            var p when p.StartsWith("/api/tenant") => CheckTenantPermissions(user, method),
            var p when p.StartsWith("/api/system") => CheckSystemPermissions(user, method),
            _ => true // Allow access to other endpoints
        };
    }

    /// <summary>
    /// Checks notification-related permissions.
    /// </summary>
    private bool CheckNotificationPermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.NotificationsRead),
            "POST" => user.HasClaim("permission", Permissions.NotificationsWrite) || 
                      user.HasClaim("permission", Permissions.NotificationsSend),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.NotificationsWrite),
            "DELETE" => user.HasClaim("permission", Permissions.NotificationsDelete),
            _ => false
        };
    }

    /// <summary>
    /// Checks rule-related permissions.
    /// </summary>
    private bool CheckRulePermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.RulesRead),
            "POST" => user.HasClaim("permission", Permissions.RulesWrite),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.RulesWrite),
            "DELETE" => user.HasClaim("permission", Permissions.RulesDelete),
            _ => false
        };
    }

    /// <summary>
    /// Checks template-related permissions.
    /// </summary>
    private bool CheckTemplatePermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.TemplatesRead),
            "POST" => user.HasClaim("permission", Permissions.TemplatesWrite),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.TemplatesWrite),
            "DELETE" => user.HasClaim("permission", Permissions.TemplatesDelete),
            _ => false
        };
    }

    /// <summary>
    /// Checks subscription-related permissions.
    /// </summary>
    private bool CheckSubscriptionPermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.SubscriptionsRead),
            "POST" => user.HasClaim("permission", Permissions.SubscriptionsWrite),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.SubscriptionsWrite),
            "DELETE" => user.HasClaim("permission", Permissions.SubscriptionsDelete),
            _ => false
        };
    }

    /// <summary>
    /// Checks user management permissions.
    /// </summary>
    private bool CheckUserPermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.UsersRead),
            "POST" => user.HasClaim("permission", Permissions.UsersWrite),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.UsersWrite),
            "DELETE" => user.HasClaim("permission", Permissions.UsersDelete),
            _ => false
        };
    }

    /// <summary>
    /// Checks role management permissions.
    /// </summary>
    private bool CheckRolePermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.RolesRead),
            "POST" => user.HasClaim("permission", Permissions.RolesWrite),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.RolesWrite),
            "DELETE" => user.HasClaim("permission", Permissions.RolesDelete),
            _ => false
        };
    }

    /// <summary>
    /// Checks API key management permissions.
    /// </summary>
    private bool CheckApiKeyPermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.ApiKeysRead),
            "POST" => user.HasClaim("permission", Permissions.ApiKeysWrite),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.ApiKeysWrite),
            "DELETE" => user.HasClaim("permission", Permissions.ApiKeysDelete),
            _ => false
        };
    }

    /// <summary>
    /// Checks audit permissions.
    /// </summary>
    private bool CheckAuditPermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.AuditRead),
            _ => false
        };
    }

    /// <summary>
    /// Checks tenant management permissions.
    /// </summary>
    private bool CheckTenantPermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.TenantRead),
            "POST" => user.HasClaim("permission", Permissions.TenantWrite),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.TenantWrite),
            "DELETE" => user.HasClaim("permission", Permissions.TenantDelete),
            _ => false
        };
    }

    /// <summary>
    /// Checks system permissions.
    /// </summary>
    private bool CheckSystemPermissions(ClaimsPrincipal user, string method)
    {
        return method switch
        {
            "GET" => user.HasClaim("permission", Permissions.SystemRead),
            "POST" => user.HasClaim("permission", Permissions.SystemWrite),
            "PUT" or "PATCH" => user.HasClaim("permission", Permissions.SystemWrite),
            "DELETE" => user.HasClaim("permission", Permissions.SystemAdmin),
            _ => false
        };
    }

    /// <summary>
    /// Extracts tenant ID from the request context.
    /// </summary>
    private string? ExtractTenantId(HttpContext context)
    {
        // Try route parameter first
        if (context.Request.RouteValues.TryGetValue("tenantId", out var tenantIdRoute))
        {
            return tenantIdRoute?.ToString();
        }

        // Try query parameter
        if (context.Request.Query.TryGetValue("tenantId", out var tenantIdQuery))
        {
            return tenantIdQuery.FirstOrDefault();
        }

        // Try header
        if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader))
        {
            return tenantIdHeader.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Determines if authorization should be skipped for the given path.
    /// </summary>
    private bool ShouldSkipAuthorization(PathString path)
    {
        var skipPaths = new[]
        {
            "/health",
            "/metrics",
            "/swagger",
            "/favicon.ico",
            "/.well-known"
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Extension methods for authorization middleware.
/// </summary>
public static class AuthorizationMiddlewareExtensions
{
    /// <summary>
    /// Adds the NotifyX authorization middleware to the pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseNotifyXAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthorizationMiddleware>();
    }
}