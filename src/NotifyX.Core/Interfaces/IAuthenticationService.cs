using NotifyX.Core.Models;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Service interface for authentication and authorization operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user using API key.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous authentication operation.</returns>
    Task<AuthenticationResult> AuthenticateWithApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user using OAuth2/JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous authentication operation.</returns>
    Task<AuthenticationResult> AuthenticateWithJwtAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous token validation operation.</returns>
    Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new API key for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="permissions">The permissions for the API key.</param>
    /// <param name="expiresAt">Optional expiration date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous API key creation operation.</returns>
    Task<ApiKeyResult> CreateApiKeyAsync(string userId, string tenantId, List<string> permissions, DateTime? expiresAt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an API key.
    /// </summary>
    /// <param name="apiKey">The API key to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous API key revocation operation.</returns>
    Task<bool> RevokeApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets API keys for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous API key retrieval operation.</returns>
    Task<IEnumerable<ApiKeyInfo>> GetApiKeysAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has permission for a specific action.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="permission">The permission to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous permission check operation.</returns>
    Task<bool> HasPermissionAsync(string userId, string tenantId, string permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user roles for a tenant.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous role retrieval operation.</returns>
    Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for audit logging operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit event.
    /// </summary>
    /// <param name="auditEvent">The audit event to log.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous audit logging operation.</returns>
    Task LogAuditEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit events for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="startDate">Start date for filtering.</param>
    /// <param name="endDate">End date for filtering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous audit event retrieval operation.</returns>
    Task<IEnumerable<AuditEvent>> GetAuditEventsAsync(string userId, string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit events for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="startDate">Start date for filtering.</param>
    /// <param name="endDate">End date for filtering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous audit event retrieval operation.</returns>
    Task<IEnumerable<AuditEvent>> GetAuditEventsForTenantAsync(string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit events by event type.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="startDate">Start date for filtering.</param>
    /// <param name="endDate">End date for filtering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous audit event retrieval operation.</returns>
    Task<IEnumerable<AuditEvent>> GetAuditEventsByTypeAsync(string eventType, string tenantId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an authentication operation.
/// </summary>
public sealed class AuthenticationResult
{
    /// <summary>
    /// Whether the authentication was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The authenticated user information.
    /// </summary>
    public AuthenticatedUser? User { get; init; }

    /// <summary>
    /// Error message if authentication failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if authentication failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The authentication method used.
    /// </summary>
    public AuthenticationMethod Method { get; init; }

    /// <summary>
    /// Timestamp when the authentication occurred.
    /// </summary>
    public DateTime AuthenticatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <param name="method">The authentication method.</param>
    /// <returns>A successful authentication result.</returns>
    public static AuthenticationResult Success(AuthenticatedUser user, AuthenticationMethod method)
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            User = user,
            Method = method
        };
    }

    /// <summary>
    /// Creates a failed authentication result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <param name="method">The authentication method.</param>
    /// <returns>A failed authentication result.</returns>
    public static AuthenticationResult Failure(string errorMessage, string errorCode, AuthenticationMethod method)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode,
            Method = method
        };
    }
}

/// <summary>
/// Result of a token validation operation.
/// </summary>
public sealed class TokenValidationResult
{
    /// <summary>
    /// Whether the token is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// The user information from the token.
    /// </summary>
    public AuthenticatedUser? User { get; init; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if validation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Token expiration date.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Token issued date.
    /// </summary>
    public DateTime? IssuedAt { get; init; }

    /// <summary>
    /// Creates a successful token validation result.
    /// </summary>
    /// <param name="user">The user information.</param>
    /// <param name="expiresAt">Token expiration date.</param>
    /// <param name="issuedAt">Token issued date.</param>
    /// <returns>A successful token validation result.</returns>
    public static TokenValidationResult Success(AuthenticatedUser user, DateTime? expiresAt = null, DateTime? issuedAt = null)
    {
        return new TokenValidationResult
        {
            IsValid = true,
            User = user,
            ExpiresAt = expiresAt,
            IssuedAt = issuedAt
        };
    }

    /// <summary>
    /// Creates a failed token validation result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A failed token validation result.</returns>
    public static TokenValidationResult Failure(string errorMessage, string errorCode)
    {
        return new TokenValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Result of an API key creation operation.
/// </summary>
public sealed class ApiKeyResult
{
    /// <summary>
    /// Whether the API key creation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The created API key.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// The API key information.
    /// </summary>
    public ApiKeyInfo? ApiKeyInfo { get; init; }

    /// <summary>
    /// Error message if creation failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if creation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Creates a successful API key result.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="apiKeyInfo">The API key information.</param>
    /// <returns>A successful API key result.</returns>
    public static ApiKeyResult Success(string apiKey, ApiKeyInfo apiKeyInfo)
    {
        return new ApiKeyResult
        {
            IsSuccess = true,
            ApiKey = apiKey,
            ApiKeyInfo = apiKeyInfo
        };
    }

    /// <summary>
    /// Creates a failed API key result.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A failed API key result.</returns>
    public static ApiKeyResult Failure(string errorMessage, string errorCode)
    {
        return new ApiKeyResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}

/// <summary>
/// Authentication methods.
/// </summary>
public enum AuthenticationMethod
{
    /// <summary>
    /// API key authentication.
    /// </summary>
    ApiKey = 0,

    /// <summary>
    /// JWT token authentication.
    /// </summary>
    Jwt = 1,

    /// <summary>
    /// OAuth2 authentication.
    /// </summary>
    OAuth2 = 2,

    /// <summary>
    /// Basic authentication.
    /// </summary>
    Basic = 3
}