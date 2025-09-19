using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NotifyXStudio.Api.Middleware;

/// <summary>
/// Global exception handling middleware for production-ready error handling
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var problemDetails = new ProblemDetails();

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = exception.Message;
                break;

            case UnauthorizedAccessException:
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = "Access denied";
                break;

            case KeyNotFoundException:
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                problemDetails.Title = "Not Found";
                problemDetails.Detail = "The requested resource was not found";
                break;

            case InvalidOperationException:
                problemDetails.Status = (int)HttpStatusCode.Conflict;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
                problemDetails.Title = "Conflict";
                problemDetails.Detail = exception.Message;
                break;

            case TimeoutException:
                problemDetails.Status = (int)HttpStatusCode.RequestTimeout;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.7";
                problemDetails.Title = "Request Timeout";
                problemDetails.Detail = "The request timed out";
                break;

            default:
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred";
                break;
        }

        problemDetails.Instance = context.Request.Path;

        response.StatusCode = problemDetails.Status.Value;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, options);
        await response.WriteAsync(json);
    }
}

/// <summary>
/// Extension method to register the global exception handling middleware
/// </summary>
public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}

