using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Samples;

/// <summary>
/// Sample application demonstrating authentication and security functionality.
/// </summary>
public class AuthenticationSample
{
    private readonly ILogger<AuthenticationSample> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuditService _auditService;

    /// <summary>
    /// Initializes a new instance of the AuthenticationSample class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="authenticationService">The authentication service.</param>
    /// <param name="auditService">The audit service.</param>
    public AuthenticationSample(
        ILogger<AuthenticationSample> logger,
        IAuthenticationService authenticationService,
        IAuditService auditService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    /// <summary>
    /// Runs the authentication sample.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting authentication and security sample...");

        try
        {
            // Sample 1: API Key Authentication
            await DemonstrateApiKeyAuthenticationAsync(cancellationToken);

            // Sample 2: JWT Token Authentication
            await DemonstrateJwtAuthenticationAsync(cancellationToken);

            // Sample 3: Permission Checking
            await DemonstratePermissionCheckingAsync(cancellationToken);

            // Sample 4: Role Management
            await DemonstrateRoleManagementAsync(cancellationToken);

            // Sample 5: Audit Logging
            await DemonstrateAuditLoggingAsync(cancellationToken);

            _logger.LogInformation("Authentication and security sample completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running authentication and security sample");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates API key authentication.
    /// </summary>
    private async Task DemonstrateApiKeyAuthenticationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating API Key Authentication ===");

        try
        {
            // Create an API key for the developer user
            var apiKeyResult = await _authenticationService.CreateApiKeyAsync(
                "developer",
                "default",
                new List<string>
                {
                    Permissions.NotificationsRead,
                    Permissions.NotificationsWrite,
                    Permissions.NotificationsSend,
                    Permissions.RulesRead,
                    Permissions.RulesWrite,
                    Permissions.TemplatesRead,
                    Permissions.TemplatesWrite
                },
                DateTime.UtcNow.AddDays(30), // Expires in 30 days
                cancellationToken);

            if (apiKeyResult.IsSuccess && !string.IsNullOrEmpty(apiKeyResult.ApiKey))
            {
                _logger.LogInformation("✅ API key created successfully: {ApiKey}", apiKeyResult.ApiKey);
                _logger.LogInformation("   - Name: {Name}", apiKeyResult.ApiKeyInfo?.Name);
                _logger.LogInformation("   - Permissions: {Permissions}", string.Join(", ", apiKeyResult.ApiKeyInfo?.Permissions ?? new List<string>()));
                _logger.LogInformation("   - Expires: {ExpiresAt}", apiKeyResult.ApiKeyInfo?.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never");

                // Test authentication with the API key
                var authResult = await _authenticationService.AuthenticateWithApiKeyAsync(apiKeyResult.ApiKey, cancellationToken);
                
                if (authResult.IsSuccess && authResult.User != null)
                {
                    _logger.LogInformation("✅ API key authentication successful");
                    _logger.LogInformation("   - User: {UserName} ({UserId})", authResult.User.Name, authResult.User.UserId);
                    _logger.LogInformation("   - Tenant: {TenantId}", authResult.User.TenantId);
                    _logger.LogInformation("   - Roles: {Roles}", string.Join(", ", authResult.User.Roles.Select(r => r.Name)));
                    _logger.LogInformation("   - Permissions: {Permissions}", string.Join(", ", authResult.User.Permissions));
                }
                else
                {
                    _logger.LogError("❌ API key authentication failed: {Error}", authResult.ErrorMessage);
                }

                // Test permission checking
                var hasNotificationRead = await _authenticationService.HasPermissionAsync(
                    authResult.User!.UserId, 
                    authResult.User.TenantId, 
                    Permissions.NotificationsRead, 
                    cancellationToken);
                
                var hasSystemAdmin = await _authenticationService.HasPermissionAsync(
                    authResult.User.UserId, 
                    authResult.User.TenantId, 
                    Permissions.SystemAdmin, 
                    cancellationToken);

                _logger.LogInformation("   - Has NotificationsRead permission: {HasPermission}", hasNotificationRead);
                _logger.LogInformation("   - Has SystemAdmin permission: {HasPermission}", hasSystemAdmin);

                // Get user roles
                var roles = await _authenticationService.GetUserRolesAsync(
                    authResult.User.UserId, 
                    authResult.User.TenantId, 
                    cancellationToken);

                _logger.LogInformation("   - User roles: {Roles}", string.Join(", ", roles.Select(r => r.Name)));
            }
            else
            {
                _logger.LogError("❌ Failed to create API key: {Error}", apiKeyResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in API key authentication demonstration");
        }
    }

    /// <summary>
    /// Demonstrates JWT token authentication.
    /// </summary>
    private async Task DemonstrateJwtAuthenticationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating JWT Token Authentication ===");

        try
        {
            // Create a demo JWT token (in a real implementation, this would be generated by an identity provider)
            var demoJwtToken = "demo_tenant-admin"; // Simplified for demo purposes

            // Test JWT authentication
            var authResult = await _authenticationService.AuthenticateWithJwtAsync(demoJwtToken, cancellationToken);
            
            if (authResult.IsSuccess && authResult.User != null)
            {
                _logger.LogInformation("✅ JWT authentication successful");
                _logger.LogInformation("   - User: {UserName} ({UserId})", authResult.User.Name, authResult.User.UserId);
                _logger.LogInformation("   - Tenant: {TenantId}", authResult.User.TenantId);
                _logger.LogInformation("   - Method: {Method}", authResult.Method);
                _logger.LogInformation("   - Authenticated at: {AuthenticatedAt}", authResult.AuthenticatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
                _logger.LogError("❌ JWT authentication failed: {Error}", authResult.ErrorMessage);
            }

            // Test token validation
            var validationResult = await _authenticationService.ValidateTokenAsync(demoJwtToken, cancellationToken);
            
            if (validationResult.IsValid && validationResult.User != null)
            {
                _logger.LogInformation("✅ JWT token validation successful");
                _logger.LogInformation("   - User: {UserName} ({UserId})", validationResult.User.Name, validationResult.User.UserId);
                _logger.LogInformation("   - Expires at: {ExpiresAt}", validationResult.ExpiresAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown");
                _logger.LogInformation("   - Issued at: {IssuedAt}", validationResult.IssuedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown");
            }
            else
            {
                _logger.LogError("❌ JWT token validation failed: {Error}", validationResult.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in JWT authentication demonstration");
        }
    }

    /// <summary>
    /// Demonstrates permission checking.
    /// </summary>
    private async Task DemonstratePermissionCheckingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating Permission Checking ===");

        try
        {
            var testCases = new[]
            {
                new { UserId = "developer", TenantId = "default", Permission = Permissions.NotificationsRead, Expected = true },
                new { UserId = "developer", TenantId = "default", Permission = Permissions.NotificationsWrite, Expected = true },
                new { UserId = "developer", TenantId = "default", Permission = Permissions.SystemAdmin, Expected = false },
                new { UserId = "tenant-admin", TenantId = "default", Permission = Permissions.TenantWrite, Expected = true },
                new { UserId = "tenant-admin", TenantId = "default", Permission = Permissions.SystemAdmin, Expected = false },
                new { UserId = "system-admin", TenantId = "system", Permission = Permissions.SystemAdmin, Expected = true },
                new { UserId = "system-admin", TenantId = "system", Permission = Permissions.NotificationsRead, Expected = true }
            };

            foreach (var testCase in testCases)
            {
                var hasPermission = await _authenticationService.HasPermissionAsync(
                    testCase.UserId, 
                    testCase.TenantId, 
                    testCase.Permission, 
                    cancellationToken);

                var result = hasPermission == testCase.Expected ? "✅" : "❌";
                _logger.LogInformation("{Result} User {UserId} in tenant {TenantId} - Permission {Permission}: {HasPermission} (Expected: {Expected})",
                    result, testCase.UserId, testCase.TenantId, testCase.Permission, hasPermission, testCase.Expected);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in permission checking demonstration");
        }
    }

    /// <summary>
    /// Demonstrates role management.
    /// </summary>
    private async Task DemonstrateRoleManagementAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating Role Management ===");

        try
        {
            var testUsers = new[] { "developer", "tenant-admin", "system-admin" };

            foreach (var userId in testUsers)
            {
                var roles = await _authenticationService.GetUserRolesAsync(userId, "default", cancellationToken);
                
                _logger.LogInformation("User {UserId} roles:", userId);
                foreach (var role in roles)
                {
                    _logger.LogInformation("   - {RoleName}: {Description}", role.Name, role.Description);
                    _logger.LogInformation("     Permissions: {Permissions}", string.Join(", ", role.Permissions));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in role management demonstration");
        }
    }

    /// <summary>
    /// Demonstrates audit logging.
    /// </summary>
    private async Task DemonstrateAuditLoggingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating Audit Logging ===");

        try
        {
            // Log various types of audit events
            var auditEvents = new[]
            {
                new AuditEvent
                {
                    EventType = "user_login",
                    Category = AuditEventCategory.Authentication,
                    UserId = "developer",
                    UserName = "Developer",
                    TenantId = "default",
                    Action = "Login",
                    Resource = "Authentication",
                    ResourceId = "developer",
                    Result = AuditEventResult.Success,
                    IpAddress = "192.168.1.100",
                    UserAgent = "NotifyX-Sample/1.0",
                    Details = new Dictionary<string, object>
                    {
                        ["LoginMethod"] = "API Key",
                        ["SessionId"] = Guid.NewGuid().ToString()
                    }
                },
                new AuditEvent
                {
                    EventType = "notification_sent",
                    Category = AuditEventCategory.Business,
                    UserId = "developer",
                    UserName = "Developer",
                    TenantId = "default",
                    Action = "SendNotification",
                    Resource = "Notification",
                    ResourceId = Guid.NewGuid().ToString(),
                    Result = AuditEventResult.Success,
                    Details = new Dictionary<string, object>
                    {
                        ["EventType"] = "welcome",
                        ["RecipientCount"] = 1,
                        ["Channel"] = "Email"
                    }
                },
                new AuditEvent
                {
                    EventType = "rule_created",
                    Category = AuditEventCategory.Configuration,
                    UserId = "developer",
                    UserName = "Developer",
                    TenantId = "default",
                    Action = "CreateRule",
                    Resource = "Rule",
                    ResourceId = Guid.NewGuid().ToString(),
                    Result = AuditEventResult.Success,
                    Details = new Dictionary<string, object>
                    {
                        ["RuleName"] = "High Priority Email Rule",
                        ["ConditionType"] = "Priority",
                        ["ActionType"] = "SendNotification"
                    }
                },
                new AuditEvent
                {
                    EventType = "permission_denied",
                    Category = AuditEventCategory.Authorization,
                    UserId = "developer",
                    UserName = "Developer",
                    TenantId = "default",
                    Action = "AccessSystemSettings",
                    Resource = "System",
                    ResourceId = "system-config",
                    Result = AuditEventResult.Denied,
                    Severity = AuditEventSeverity.Medium,
                    Details = new Dictionary<string, object>
                    {
                        ["RequiredPermission"] = Permissions.SystemAdmin,
                        ["UserPermissions"] = new[] { Permissions.NotificationsRead, Permissions.NotificationsWrite }
                    }
                },
                new AuditEvent
                {
                    EventType = "api_key_created",
                    Category = AuditEventCategory.Authentication,
                    UserId = "tenant-admin",
                    UserName = "Tenant Administrator",
                    TenantId = "default",
                    Action = "CreateApiKey",
                    Resource = "ApiKey",
                    ResourceId = Guid.NewGuid().ToString(),
                    Result = AuditEventResult.Success,
                    Details = new Dictionary<string, object>
                    {
                        ["KeyName"] = "Integration API Key",
                        ["Permissions"] = new[] { Permissions.NotificationsSend, Permissions.RulesRead },
                        ["ExpiresAt"] = DateTime.UtcNow.AddDays(90).ToString("O")
                    }
                }
            };

            // Log all audit events
            foreach (var auditEvent in auditEvents)
            {
                await _auditService.LogAuditEventAsync(auditEvent, cancellationToken);
                _logger.LogInformation("✅ Logged audit event: {EventType} - {Action} - {Result}", 
                    auditEvent.EventType, auditEvent.Action, auditEvent.Result);
            }

            // Retrieve and display audit events
            var recentEvents = await _auditService.GetAuditEventsAsync("developer", "default", 
                DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, cancellationToken);

            _logger.LogInformation("Retrieved {Count} recent audit events for developer:", recentEvents.Count());
            foreach (var auditEvent in recentEvents.Take(5))
            {
                _logger.LogInformation("   - {EventType} at {Timestamp}: {Action} on {Resource} - {Result}",
                    auditEvent.EventType, auditEvent.Timestamp.ToString("HH:mm:ss"), 
                    auditEvent.Action, auditEvent.Resource, auditEvent.Result);
            }

            // Retrieve audit events by type
            var authEvents = await _auditService.GetAuditEventsByTypeAsync("user_login", "default", 
                DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, cancellationToken);

            _logger.LogInformation("Retrieved {Count} authentication events:", authEvents.Count());
            foreach (var authEvent in authEvents)
            {
                _logger.LogInformation("   - {EventType} by {UserName} at {Timestamp} from {IpAddress}",
                    authEvent.EventType, authEvent.UserName, authEvent.Timestamp.ToString("HH:mm:ss"), authEvent.IpAddress);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in audit logging demonstration");
        }
    }
}