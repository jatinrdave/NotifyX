using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NotifyX.Core.Services;

/// <summary>
/// Default implementation of the authentication service.
/// Provides authentication and authorization functionality.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IAuditService _auditService;
    private readonly AuthenticationOptions _options;
    private readonly ConcurrentDictionary<string, ApiKeyInfo> _apiKeys = new();
    private readonly ConcurrentDictionary<string, AuthenticatedUser> _users = new();
    private readonly ConcurrentDictionary<string, UserRole> _roles = new();

    /// <summary>
    /// Initializes a new instance of the AuthenticationService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="options">The authentication options.</param>
    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IAuditService auditService,
        IOptions<AuthenticationOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        InitializeDefaultRoles();
        InitializeDefaultUsers();
    }

    /// <inheritdoc />
    public async Task<AuthenticationResult> AuthenticateWithApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Authenticating with API key");

            if (string.IsNullOrEmpty(apiKey))
            {
                await LogAuthenticationFailureAsync("API key is null or empty", "MISSING_API_KEY", AuthenticationMethod.ApiKey, cancellationToken);
                return AuthenticationResult.Failure("API key is required", "MISSING_API_KEY", AuthenticationMethod.ApiKey);
            }

            // Find the API key
            var apiKeyInfo = _apiKeys.Values.FirstOrDefault(k => k.Id == apiKey || k.Name == apiKey);
            if (apiKeyInfo == null)
            {
                await LogAuthenticationFailureAsync("API key not found", "INVALID_API_KEY", AuthenticationMethod.ApiKey, cancellationToken);
                return AuthenticationResult.Failure("Invalid API key", "INVALID_API_KEY", AuthenticationMethod.ApiKey);
            }

            // Check if API key is active
            if (!apiKeyInfo.IsActive)
            {
                await LogAuthenticationFailureAsync("API key is inactive", "INACTIVE_API_KEY", AuthenticationMethod.ApiKey, cancellationToken);
                return AuthenticationResult.Failure("API key is inactive", "INACTIVE_API_KEY", AuthenticationMethod.ApiKey);
            }

            // Check if API key is expired
            if (apiKeyInfo.IsExpired())
            {
                await LogAuthenticationFailureAsync("API key is expired", "EXPIRED_API_KEY", AuthenticationMethod.ApiKey, cancellationToken);
                return AuthenticationResult.Failure("API key is expired", "EXPIRED_API_KEY", AuthenticationMethod.ApiKey);
            }

            // Get user information
            var user = await GetUserByIdAsync(apiKeyInfo.UserId, cancellationToken);
            if (user == null)
            {
                await LogAuthenticationFailureAsync("User not found for API key", "USER_NOT_FOUND", AuthenticationMethod.ApiKey, cancellationToken);
                return AuthenticationResult.Failure("User not found", "USER_NOT_FOUND", AuthenticationMethod.ApiKey);
            }

            // Update API key last used information
            var updatedApiKey = apiKeyInfo with
            {
                LastUsedAt = DateTime.UtcNow,
                LastUsedFromIp = GetCurrentIpAddress()
            };
            _apiKeys[apiKeyInfo.Id] = updatedApiKey;

            // Log successful authentication
            await LogAuthenticationSuccessAsync(user, AuthenticationMethod.ApiKey, cancellationToken);

            _logger.LogInformation("Successfully authenticated user {UserId} with API key", user.UserId);

            return AuthenticationResult.Success(user, AuthenticationMethod.ApiKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with API key");
            await LogAuthenticationFailureAsync($"Authentication error: {ex.Message}", "AUTHENTICATION_ERROR", AuthenticationMethod.ApiKey, cancellationToken);
            return AuthenticationResult.Failure("Authentication failed", "AUTHENTICATION_ERROR", AuthenticationMethod.ApiKey);
        }
    }

    /// <inheritdoc />
    public async Task<AuthenticationResult> AuthenticateWithJwtAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Authenticating with JWT token");

            if (string.IsNullOrEmpty(token))
            {
                await LogAuthenticationFailureAsync("JWT token is null or empty", "MISSING_JWT_TOKEN", AuthenticationMethod.Jwt, cancellationToken);
                return AuthenticationResult.Failure("JWT token is required", "MISSING_JWT_TOKEN", AuthenticationMethod.Jwt);
            }

            // Validate the JWT token
            var validationResult = await ValidateTokenAsync(token, cancellationToken);
            if (!validationResult.IsValid)
            {
                await LogAuthenticationFailureAsync($"JWT token validation failed: {validationResult.ErrorMessage}", 
                    validationResult.ErrorCode ?? "INVALID_JWT_TOKEN", AuthenticationMethod.Jwt, cancellationToken);
                return AuthenticationResult.Failure(validationResult.ErrorMessage ?? "Invalid JWT token", 
                    validationResult.ErrorCode ?? "INVALID_JWT_TOKEN", AuthenticationMethod.Jwt);
            }

            // Log successful authentication
            await LogAuthenticationSuccessAsync(validationResult.User!, AuthenticationMethod.Jwt, cancellationToken);

            _logger.LogInformation("Successfully authenticated user {UserId} with JWT token", validationResult.User!.UserId);

            return AuthenticationResult.Success(validationResult.User, AuthenticationMethod.Jwt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with JWT token");
            await LogAuthenticationFailureAsync($"JWT authentication error: {ex.Message}", "JWT_AUTHENTICATION_ERROR", AuthenticationMethod.Jwt, cancellationToken);
            return AuthenticationResult.Failure("JWT authentication failed", "JWT_AUTHENTICATION_ERROR", AuthenticationMethod.Jwt);
        }
    }

    /// <inheritdoc />
    public async Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Validating JWT token");

            if (string.IsNullOrEmpty(token))
            {
                return TokenValidationResult.Failure("Token is required", "MISSING_TOKEN");
            }

            // For demo purposes, we'll implement a simple token validation
            // In a real implementation, you would use a proper JWT library like System.IdentityModel.Tokens.Jwt
            if (!IsValidJwtFormat(token))
            {
                return TokenValidationResult.Failure("Invalid token format", "INVALID_TOKEN_FORMAT");
            }

            // Extract user information from token (simplified for demo)
            var userInfo = ExtractUserInfoFromToken(token);
            if (userInfo == null)
            {
                return TokenValidationResult.Failure("Unable to extract user information from token", "TOKEN_PARSE_ERROR");
            }

            // Get user from storage
            var user = await GetUserByIdAsync(userInfo.Value.UserId, cancellationToken);
            if (user == null)
            {
                return TokenValidationResult.Failure("User not found", "USER_NOT_FOUND");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                return TokenValidationResult.Failure("User is inactive", "USER_INACTIVE");
            }

            return TokenValidationResult.Success(user, userInfo.ExpiresAt, userInfo.IssuedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            return TokenValidationResult.Failure($"Token validation error: {ex.Message}", "TOKEN_VALIDATION_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<ApiKeyResult> CreateApiKeyAsync(string userId, string tenantId, List<string> permissions, DateTime? expiresAt = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating API key for user {UserId} in tenant {TenantId}", userId, tenantId);

            // Validate user exists
            var user = await GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return ApiKeyResult.Failure("User not found", "USER_NOT_FOUND");
            }

            // Validate tenant
            if (user.TenantId != tenantId)
            {
                return ApiKeyResult.Failure("User does not belong to the specified tenant", "TENANT_MISMATCH");
            }

            // Generate API key
            var apiKey = GenerateApiKey();
            var apiKeyInfo = new ApiKeyInfo
            {
                Id = apiKey,
                Name = $"API Key for {user.Name}",
                Description = $"API key created for user {user.Name}",
                UserId = userId,
                TenantId = tenantId,
                Permissions = permissions,
                ExpiresAt = expiresAt,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            // Store API key
            _apiKeys[apiKey] = apiKeyInfo;

            // Log audit event
            await _auditService.LogAuditEventAsync(new AuditEvent
            {
                EventType = "api_key_created",
                Category = AuditEventCategory.Authentication,
                UserId = userId,
                UserName = user.Name,
                TenantId = tenantId,
                Action = "CreateApiKey",
                Resource = "ApiKey",
                ResourceId = apiKey,
                Result = AuditEventResult.Success,
                Details = new Dictionary<string, object>
                {
                    ["Permissions"] = permissions,
                    ["ExpiresAt"] = expiresAt?.ToString("O") ?? "Never"
                }
            }, cancellationToken);

            _logger.LogInformation("Successfully created API key {ApiKeyId} for user {UserId}", apiKey, userId);

            return ApiKeyResult.Success(apiKey, apiKeyInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key for user {UserId}", userId);
            return ApiKeyResult.Failure($"API key creation failed: {ex.Message}", "API_KEY_CREATION_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<bool> RevokeApiKeyAsync(string apiKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Revoking API key {ApiKeyId}", apiKey);

            if (!_apiKeys.TryGetValue(apiKey, out var apiKeyInfo))
            {
                _logger.LogWarning("API key {ApiKeyId} not found for revocation", apiKey);
                return false;
            }

            // Get user information
            var user = await GetUserByIdAsync(apiKeyInfo.UserId, cancellationToken);

            // Mark API key as inactive
            var updatedApiKey = apiKeyInfo with
            {
                IsActive = false,
                UpdatedAt = DateTime.UtcNow
            };
            _apiKeys[apiKey] = updatedApiKey;

            // Log audit event
            await _auditService.LogAuditEventAsync(new AuditEvent
            {
                EventType = "api_key_revoked",
                Category = AuditEventCategory.Authentication,
                UserId = apiKeyInfo.UserId,
                UserName = user?.Name ?? "Unknown",
                TenantId = apiKeyInfo.TenantId,
                Action = "RevokeApiKey",
                Resource = "ApiKey",
                ResourceId = apiKey,
                Result = AuditEventResult.Success
            }, cancellationToken);

            _logger.LogInformation("Successfully revoked API key {ApiKeyId}", apiKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking API key {ApiKeyId}", apiKey);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ApiKeyInfo>> GetApiKeysAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting API keys for user {UserId}", userId);

            var userApiKeys = _apiKeys.Values
                .Where(k => k.UserId == userId)
                .ToList();

            return userApiKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API keys for user {UserId}", userId);
            return Enumerable.Empty<ApiKeyInfo>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasPermissionAsync(string userId, string tenantId, string permission, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking permission {Permission} for user {UserId} in tenant {TenantId}", permission, userId, tenantId);

            var user = await GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return false;
            }

            // Check if user belongs to the tenant
            if (user.TenantId != tenantId)
            {
                return false;
            }

            // Check user permissions
            if (user.HasPermission(permission))
            {
                return true;
            }

            // Check role permissions
            foreach (var role in user.Roles)
            {
                if (role.Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting roles for user {UserId} in tenant {TenantId}", userId, tenantId);

            var user = await GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                return Enumerable.Empty<UserRole>();
            }

            // Check if user belongs to the tenant
            if (user.TenantId != tenantId)
            {
                return Enumerable.Empty<UserRole>();
            }

            return user.Roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return Enumerable.Empty<UserRole>();
        }
    }

    #region Private Helper Methods

    private void InitializeDefaultRoles()
    {
        // System Admin Role
        var systemAdminRole = new UserRole
        {
            Id = "system-admin-role",
            Name = SystemRoles.SystemAdmin,
            Description = "System administrator with full access",
            TenantId = "system",
            Permissions = new List<string>
            {
                Permissions.SystemAdmin,
                Permissions.SystemRead,
                Permissions.SystemWrite,
                Permissions.TenantRead,
                Permissions.TenantWrite,
                Permissions.TenantDelete,
                Permissions.UsersRead,
                Permissions.UsersWrite,
                Permissions.UsersDelete,
                Permissions.RolesRead,
                Permissions.RolesWrite,
                Permissions.RolesDelete,
                Permissions.ApiKeysRead,
                Permissions.ApiKeysWrite,
                Permissions.ApiKeysDelete,
                Permissions.AuditRead,
                Permissions.NotificationsRead,
                Permissions.NotificationsWrite,
                Permissions.NotificationsDelete,
                Permissions.NotificationsSend,
                Permissions.NotificationsBulk,
                Permissions.RulesRead,
                Permissions.RulesWrite,
                Permissions.RulesDelete,
                Permissions.RulesBulk,
                Permissions.TemplatesRead,
                Permissions.TemplatesWrite,
                Permissions.TemplatesDelete,
                Permissions.SubscriptionsRead,
                Permissions.SubscriptionsWrite,
                Permissions.SubscriptionsDelete,
                Permissions.SubscriptionsBulk
            },
            IsSystemRole = true,
            CreatedBy = "system",
            UpdatedBy = "system"
        };
        _roles[systemAdminRole.Id] = systemAdminRole;

        // Tenant Admin Role
        var tenantAdminRole = new UserRole
        {
            Id = "tenant-admin-role",
            Name = SystemRoles.TenantAdmin,
            Description = "Tenant administrator with full tenant access",
            TenantId = "default",
            Permissions = new List<string>
            {
                Permissions.TenantRead,
                Permissions.TenantWrite,
                Permissions.UsersRead,
                Permissions.UsersWrite,
                Permissions.UsersDelete,
                Permissions.RolesRead,
                Permissions.RolesWrite,
                Permissions.RolesDelete,
                Permissions.ApiKeysRead,
                Permissions.ApiKeysWrite,
                Permissions.ApiKeysDelete,
                Permissions.AuditRead,
                Permissions.NotificationsRead,
                Permissions.NotificationsWrite,
                Permissions.NotificationsDelete,
                Permissions.NotificationsSend,
                Permissions.NotificationsBulk,
                Permissions.RulesRead,
                Permissions.RulesWrite,
                Permissions.RulesDelete,
                Permissions.RulesBulk,
                Permissions.TemplatesRead,
                Permissions.TemplatesWrite,
                Permissions.TemplatesDelete,
                Permissions.SubscriptionsRead,
                Permissions.SubscriptionsWrite,
                Permissions.SubscriptionsDelete,
                Permissions.SubscriptionsBulk
            },
            IsSystemRole = true,
            CreatedBy = "system",
            UpdatedBy = "system"
        };
        _roles[tenantAdminRole.Id] = tenantAdminRole;

        // Developer Role
        var developerRole = new UserRole
        {
            Id = "developer-role",
            Name = SystemRoles.Developer,
            Description = "Developer with notification and rule management access",
            TenantId = "default",
            Permissions = new List<string>
            {
                Permissions.NotificationsRead,
                Permissions.NotificationsWrite,
                Permissions.NotificationsSend,
                Permissions.NotificationsBulk,
                Permissions.RulesRead,
                Permissions.RulesWrite,
                Permissions.RulesBulk,
                Permissions.TemplatesRead,
                Permissions.TemplatesWrite,
                Permissions.SubscriptionsRead,
                Permissions.SubscriptionsWrite,
                Permissions.SubscriptionsBulk,
                Permissions.ApiKeysRead,
                Permissions.ApiKeysWrite
            },
            IsSystemRole = true,
            CreatedBy = "system",
            UpdatedBy = "system"
        };
        _roles[developerRole.Id] = developerRole;

        // Auditor Role
        var auditorRole = new UserRole
        {
            Id = "auditor-role",
            Name = SystemRoles.Auditor,
            Description = "Auditor with read-only access to audit logs and system information",
            TenantId = "default",
            Permissions = new List<string>
            {
                Permissions.AuditRead,
                Permissions.NotificationsRead,
                Permissions.RulesRead,
                Permissions.TemplatesRead,
                Permissions.SubscriptionsRead,
                Permissions.UsersRead,
                Permissions.RolesRead,
                Permissions.ApiKeysRead,
                Permissions.TenantRead,
                Permissions.SystemRead
            },
            IsSystemRole = true,
            CreatedBy = "system",
            UpdatedBy = "system"
        };
        _roles[auditorRole.Id] = auditorRole;

        // Service Account Role
        var serviceAccountRole = new UserRole
        {
            Id = "service-account-role",
            Name = SystemRoles.ServiceAccount,
            Description = "Service account with limited access for automated operations",
            TenantId = "default",
            Permissions = new List<string>
            {
                Permissions.NotificationsSend,
                Permissions.NotificationsBulk,
                Permissions.RulesRead,
                Permissions.TemplatesRead,
                Permissions.SubscriptionsRead
            },
            IsSystemRole = true,
            CreatedBy = "system",
            UpdatedBy = "system"
        };
        _roles[serviceAccountRole.Id] = serviceAccountRole;
    }

    private void InitializeDefaultUsers()
    {
        // System Admin User
        var systemAdminUser = new AuthenticatedUser
        {
            UserId = "system-admin",
            Name = "System Administrator",
            Email = "admin@notifyx.com",
            TenantId = "system",
            Roles = new List<UserRole> { _roles["system-admin-role"] },
            Permissions = new List<string>(),
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _users[systemAdminUser.UserId] = systemAdminUser;

        // Tenant Admin User
        var tenantAdminUser = new AuthenticatedUser
        {
            UserId = "tenant-admin",
            Name = "Tenant Administrator",
            Email = "tenant-admin@example.com",
            TenantId = "default",
            Roles = new List<UserRole> { _roles["tenant-admin-role"] },
            Permissions = new List<string>(),
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _users[tenantAdminUser.UserId] = tenantAdminUser;

        // Developer User
        var developerUser = new AuthenticatedUser
        {
            UserId = "developer",
            Name = "Developer",
            Email = "developer@example.com",
            TenantId = "default",
            Roles = new List<UserRole> { _roles["developer-role"] },
            Permissions = new List<string>(),
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _users[developerUser.UserId] = developerUser;
    }

    private async Task<AuthenticatedUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return _users.TryGetValue(userId, out var user) ? user : null;
    }

    private string GenerateApiKey()
    {
        // Generate a secure random API key
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private bool IsValidJwtFormat(string token)
    {
        // Simple JWT format validation (3 parts separated by dots)
        var parts = token.Split('.');
        return parts.Length == 3;
    }

    private (string UserId, DateTime? ExpiresAt, DateTime? IssuedAt)? ExtractUserInfoFromToken(string token)
    {
        try
        {
            // This is a simplified implementation for demo purposes
            // In a real implementation, you would properly decode and validate the JWT
            
            // For demo, we'll extract user info from a simple format
            // In reality, you'd decode the JWT payload
            if (token.StartsWith("demo_"))
            {
                var userId = token.Substring(5); // Remove "demo_" prefix
                return (userId, DateTime.UtcNow.AddHours(1), DateTime.UtcNow);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private string? GetCurrentIpAddress()
    {
        // In a real implementation, you would get this from the HTTP context
        return "127.0.0.1";
    }

    private async Task LogAuthenticationSuccessAsync(AuthenticatedUser user, AuthenticationMethod method, CancellationToken cancellationToken)
    {
        await _auditService.LogAuditEventAsync(new AuditEvent
        {
            EventType = "authentication_success",
            Category = AuditEventCategory.Authentication,
            UserId = user.UserId,
            UserName = user.Name,
            TenantId = user.TenantId,
            Action = "Authenticate",
            Resource = "Authentication",
            ResourceId = user.UserId,
            Result = AuditEventResult.Success,
            Details = new Dictionary<string, object>
            {
                ["Method"] = method.ToString(),
                ["IpAddress"] = GetCurrentIpAddress() ?? "Unknown"
            }
        }, cancellationToken);
    }

    private async Task LogAuthenticationFailureAsync(string reason, string errorCode, AuthenticationMethod method, CancellationToken cancellationToken)
    {
        await _auditService.LogAuditEventAsync(new AuditEvent
        {
            EventType = "authentication_failure",
            Category = AuditEventCategory.Authentication,
            UserId = "anonymous",
            UserName = "Anonymous",
            TenantId = "unknown",
            Action = "Authenticate",
            Resource = "Authentication",
            ResourceId = "unknown",
            Result = AuditEventResult.Failure,
            Severity = AuditEventSeverity.Medium,
            Details = new Dictionary<string, object>
            {
                ["Method"] = method.ToString(),
                ["Reason"] = reason,
                ["ErrorCode"] = errorCode,
                ["IpAddress"] = GetCurrentIpAddress() ?? "Unknown"
            }
        }, cancellationToken);
    }

    #endregion
}

/// <summary>
/// Configuration options for authentication.
/// </summary>
public sealed class AuthenticationOptions
{
    /// <summary>
    /// Whether authentication is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// JWT secret key for token validation.
    /// </summary>
    public string JwtSecretKey { get; set; } = "your-secret-key-here";

    /// <summary>
    /// JWT issuer.
    /// </summary>
    public string JwtIssuer { get; set; } = "NotifyX";

    /// <summary>
    /// JWT audience.
    /// </summary>
    public string JwtAudience { get; set; } = "NotifyX-Users";

    /// <summary>
    /// JWT token expiration time.
    /// </summary>
    public TimeSpan JwtExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// API key expiration time.
    /// </summary>
    public TimeSpan ApiKeyExpiration { get; set; } = TimeSpan.FromDays(365);

    /// <summary>
    /// Whether to require HTTPS for authentication.
    /// </summary>
    public bool RequireHttps { get; set; } = true;

    /// <summary>
    /// Maximum number of failed authentication attempts before lockout.
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Lockout duration after max failed attempts.
    /// </summary>
    public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Whether to enable audit logging for authentication events.
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;
}