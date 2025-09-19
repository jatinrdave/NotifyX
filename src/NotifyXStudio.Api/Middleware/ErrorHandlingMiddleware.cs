using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace NotifyXStudio.Api.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions and returning appropriate error responses.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly ErrorHandlingOptions _options;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IOptions<ErrorHandlingOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var errorId = Guid.NewGuid().ToString("N")[..8];
            var correlationId = GetCorrelationId(context);

            _logger.LogError(exception, "Error {ErrorId} occurred for request {CorrelationId}: {Message}", 
                errorId, correlationId, exception.Message);

            var errorResponse = CreateErrorResponse(exception, errorId, correlationId);
            var statusCode = GetStatusCode(exception);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _options.IncludeStackTrace
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private ErrorResponse CreateErrorResponse(Exception exception, string errorId, string correlationId)
        {
            var errorResponse = new ErrorResponse
            {
                ErrorId = errorId,
                CorrelationId = correlationId,
                Message = GetUserFriendlyMessage(exception),
                Timestamp = DateTime.UtcNow
            };

            if (_options.IncludeStackTrace)
            {
                errorResponse.StackTrace = exception.StackTrace;
            }

            if (_options.IncludeInnerException && exception.InnerException != null)
            {
                errorResponse.InnerException = new ErrorResponse
                {
                    ErrorId = errorId,
                    CorrelationId = correlationId,
                    Message = exception.InnerException.Message,
                    Timestamp = DateTime.UtcNow
                };

                if (_options.IncludeStackTrace)
                {
                    errorResponse.InnerException.StackTrace = exception.InnerException.StackTrace;
                }
            }

            if (_options.IncludeDetails)
            {
                errorResponse.Details = GetErrorDetails(exception);
            }

            return errorResponse;
        }

        private string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => "Required parameter is missing.",
                ArgumentException => "Invalid request parameters.",
                UnauthorizedAccessException => "Access denied.",
                NotImplementedException => "Feature not implemented.",
                TimeoutException => "Request timed out.",
                HttpRequestException => "External service error.",
                _ => _options.ShowDetailedErrors ? exception.Message : "An unexpected error occurred."
            };
        }

        private Dictionary<string, object> GetErrorDetails(Exception exception)
        {
            var details = new Dictionary<string, object>();

            if (exception is ArgumentException argEx && !string.IsNullOrEmpty(argEx.ParamName))
            {
                details["ParameterName"] = argEx.ParamName;
            }

            if (exception is HttpRequestException httpEx)
            {
                details["HttpError"] = httpEx.Message;
            }

            if (exception is TimeoutException timeoutEx)
            {
                details["Timeout"] = timeoutEx.Message;
            }

            return details;
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                NotImplementedException => (int)HttpStatusCode.NotImplemented,
                TimeoutException => (int)HttpStatusCode.RequestTimeout,
                HttpRequestException => (int)HttpStatusCode.BadGateway,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private string GetCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                return correlationId.ToString();
            }

            var claimCorrelationId = context.User?.FindFirst("correlation_id")?.Value;
            if (!string.IsNullOrEmpty(claimCorrelationId))
            {
                return claimCorrelationId;
            }

            return Guid.NewGuid().ToString("N")[..8];
        }
    }

    /// <summary>
    /// Error response model.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Unique error identifier.
        /// </summary>
        public string ErrorId { get; set; } = string.Empty;

        /// <summary>
        /// Correlation identifier for tracking.
        /// </summary>
        public string CorrelationId { get; set; } = string.Empty;

        /// <summary>
        /// User-friendly error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Error timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Stack trace (if enabled).
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Inner exception details (if enabled).
        /// </summary>
        public ErrorResponse? InnerException { get; set; }

        /// <summary>
        /// Additional error details (if enabled).
        /// </summary>
        public Dictionary<string, object>? Details { get; set; }
    }

    /// <summary>
    /// Error handling configuration options.
    /// </summary>
    public class ErrorHandlingOptions
    {
        /// <summary>
        /// Whether error handling is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether to include stack traces in error responses.
        /// </summary>
        public bool IncludeStackTrace { get; set; } = false;

        /// <summary>
        /// Whether to include inner exception details.
        /// </summary>
        public bool IncludeInnerException { get; set; } = true;

        /// <summary>
        /// Whether to include additional error details.
        /// </summary>
        public bool IncludeDetails { get; set; } = true;

        /// <summary>
        /// Whether to show detailed error messages to users.
        /// </summary>
        public bool ShowDetailedErrors { get; set; } = false;

        /// <summary>
        /// Whether to log errors.
        /// </summary>
        public bool LogErrors { get; set; } = true;

        /// <summary>
        /// Whether to log error details.
        /// </summary>
        public bool LogErrorDetails { get; set; } = true;
    }
}