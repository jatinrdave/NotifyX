using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Security.Claims;

namespace NotifyX.Core.Middleware;

/// <summary>
/// Middleware for handling authentication in NotifyX API.
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly AuthenticationOptions _options;

    /// <summary>
    /// Initializes a new instance of the AuthenticationMiddleware class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The authentication options.</param>
    public AuthenticationMiddleware(
        RequestDelegate next,
        ILogger<AuthenticationMiddleware> logger,
        IOptions<AuthenticationOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, IAuthenticationService authenticationService)
    {
        try
        {
            if (!_options.IsEnabled)
            {
                await _next(context);
                return;
            }

            // Skip authentication for certain paths
            if (ShouldSkipAuthentication(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Extract authentication information
            var authResult = await ExtractAuthenticationAsync(context, authenticationService);
            
            if (authResult.IsSuccess && authResult.User != null)
            {
                // Set user context
                context.Items["AuthenticatedUser"] = authResult.User;
                context.Items["AuthenticationMethod"] = authResult.Method;

                // Create claims principal
                var claimsPrincipal = CreateClaimsPrincipal(authResult.User);
                context.User = claimsPrincipal;

                _logger.LogDebug("User {UserId} authenticated successfully using {Method}", 
                    authResult.User.UserId, authResult.Method);
            }
            else
            {
                // Authentication failed
                _logger.LogWarning("Authentication failed: {Error}", authResult.ErrorMessage);
                
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in authentication middleware");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal Server Error");
        }
    }

    /// <summary>
    /// Extracts authentication information from the HTTP context.
    /// </summary>
    private async Task<AuthenticationResult> ExtractAuthenticationAsync(HttpContext context, IAuthenticationService authenticationService)
    {
        // Try API key authentication first
        var apiKey = ExtractApiKey(context);
        if (!string.IsNullOrEmpty(apiKey))
        {
            return await authenticationService.AuthenticateWithApiKeyAsync(apiKey);
        }

        // Try JWT token authentication
        var jwtToken = ExtractJwtToken(context);
        if (!string.IsNullOrEmpty(jwtToken))
        {
            return await authenticationService.AuthenticateWithJwtAsync(jwtToken);
        }

        // Try OAuth2 Bearer token
        var bearerToken = ExtractBearerToken(context);
        if (!string.IsNullOrEmpty(bearerToken))
        {
            return await authenticationService.AuthenticateWithJwtAsync(bearerToken);
        }

        return AuthenticationResult.Failure("No authentication credentials provided", "NO_CREDENTIALS", AuthenticationMethod.ApiKey);
    }

    /// <summary>
    /// Extracts API key from the request.
    /// </summary>
    private string? ExtractApiKey(HttpContext context)
    {
        // Try header first
        if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            return apiKeyHeader.FirstOrDefault();
        }

        // Try query parameter
        if (context.Request.Query.TryGetValue("api_key", out var apiKeyQuery))
        {
            return apiKeyQuery.FirstOrDefault();
        }

        // Try authorization header with ApiKey scheme
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("ApiKey ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring(7);
        }

        return null;
    }

    /// <summary>
    /// Extracts JWT token from the request.
    /// </summary>
    private string? ExtractJwtToken(HttpContext context)
    {
        // Try authorization header with Bearer scheme
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring(7);
        }

        // Try query parameter
        if (context.Request.Query.TryGetValue("access_token", out var tokenQuery))
        {
            return tokenQuery.FirstOrDefault();
        }

        return null;
    }

    /// <summary>
    /// Extracts Bearer token from the request.
    /// </summary>
    private string? ExtractBearerToken(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring(7);
        }

        return null;
    }

    /// <summary>
    /// Creates a claims principal from the authenticated user.
    /// </summary>
    private ClaimsPrincipal CreateClaimsPrincipal(AuthenticatedUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("tenant_id", user.TenantId),
            new("is_active", user.IsActive.ToString()),
            new("is_verified", user.IsVerified.ToString())
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        // Add permission claims
        foreach (var permission in user.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var identity = new ClaimsIdentity(claims, "NotifyX");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Determines if authentication should be skipped for the given path.
    /// </summary>
    private bool ShouldSkipAuthentication(PathString path)
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
/// Extension methods for authentication middleware.
/// </summary>
public static class AuthenticationMiddlewareExtensions
{
    /// <summary>
    /// Adds the NotifyX authentication middleware to the pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseNotifyXAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}