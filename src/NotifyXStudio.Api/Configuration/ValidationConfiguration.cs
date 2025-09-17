using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace NotifyXStudio.Api.Configuration;

/// <summary>
/// Validation configuration for request validation
/// </summary>
public static class ValidationConfiguration
{
    public static void ConfigureValidation(this IServiceCollection services)
    {
        // Add FluentValidation
        services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

        // Register all validators from the current assembly
        services.AddValidatorsFromAssemblyContaining<Program>();

        // Configure model validation behavior
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => new ValidationError
                    {
                        Field = x.Key,
                        Message = e.ErrorMessage
                    }))
                    .ToList();

                var problemDetails = new ValidationProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    Title = "Validation Error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Instance = context.HttpContext.Request.Path
                };

                foreach (var error in errors)
                {
                    if (problemDetails.Errors.ContainsKey(error.Field))
                    {
                        problemDetails.Errors[error.Field] = problemDetails.Errors[error.Field].Concat(new[] { error.Message }).ToArray();
                    }
                    else
                    {
                        problemDetails.Errors.Add(error.Field, new[] { error.Message });
                    }
                }

                return new BadRequestObjectResult(problemDetails);
            };
        });
    }
}

/// <summary>
/// Base validator class with common validation rules
/// </summary>
public abstract class BaseValidator<T> : AbstractValidator<T>
{
    protected BaseValidator()
    {
        ValidatorOptions.Global.LanguageManager.Culture = new System.Globalization.CultureInfo("en-US");
    }

    protected static bool BeValidGuid(string guid)
    {
        return Guid.TryParse(guid, out _);
    }

    protected static bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected static bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    protected static bool BeValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Simple phone number validation - can be enhanced based on requirements
        return phoneNumber.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == '(' || c == ')' || c == ' ');
    }
}

/// <summary>
/// Validation error model
/// </summary>
public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Validation problem details
/// </summary>
public class ValidationProblemDetails : Microsoft.AspNetCore.Mvc.ValidationProblemDetails
{
    public ValidationProblemDetails()
    {
        Title = "Validation Error";
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        Status = StatusCodes.Status400BadRequest;
    }
}

/// <summary>
/// Example validators for common models
/// </summary>
public class LoginRequestValidator : BaseValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .Length(3, 50)
            .WithMessage("Username must be between 3 and 50 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long");

        RuleFor(x => x.TenantId)
            .Must(BeValidGuid!)
            .When(x => !string.IsNullOrEmpty(x.TenantId))
            .WithMessage("TenantId must be a valid GUID");
    }
}

public class RefreshTokenRequestValidator : BaseValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Token is required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required");
    }
}

/// <summary>
/// Project creation request validator
/// </summary>
public class CreateProjectRequestValidator : BaseValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Project name is required")
            .Length(3, 100)
            .WithMessage("Project name must be between 3 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required")
            .Must(BeValidGuid)
            .WithMessage("Tenant ID must be a valid GUID");
    }
}

/// <summary>
/// Example request models
/// </summary>
public class CreateProjectRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string TenantId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public List<string>? Tags { get; set; }
}

