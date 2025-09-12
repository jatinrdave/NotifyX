using System.Text.Json.Serialization;

namespace NotifyX.Core.Models;

/// <summary>
/// Represents an authenticated user.
/// </summary>
public sealed record AuthenticatedUser
{
    /// <summary>
    /// The user ID.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The user's display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The tenant ID this user belongs to.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// The user's roles.
    /// </summary>
    public List<UserRole> Roles { get; init; } = new();

    /// <summary>
    /// The user's permissions.
    /// </summary>
    public List<string> Permissions { get; init; } = new();

    /// <summary>
    /// Whether the user is active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Whether the user is verified.
    /// </summary>
    public bool IsVerified { get; init; } = false;

    /// <summary>
    /// Custom properties for the user.
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();

    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user last logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; init; }

    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="roleName">The role name to check.</param>
    /// <returns>True if the user has the role.</returns>
    public bool HasRole(string roleName)
    {
        return Roles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the user has the permission.</returns>
    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has any of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if the user has any of the permissions.</returns>
    public bool HasAnyPermission(params string[] permissions)
    {
        return permissions.Any(p => HasPermission(p));
    }

    /// <summary>
    /// Checks if the user has all of the specified permissions.
    /// </summary>
    /// <param name="permissions">The permissions to check.</param>
    /// <returns>True if the user has all of the permissions.</returns>
    public bool HasAllPermissions(params string[] permissions)
    {
        return permissions.All(p => HasPermission(p));
    }
}

/// <summary>
/// Represents a user role.
/// </summary>
public sealed record UserRole
{
    /// <summary>
    /// The role ID.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The role name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The role description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The tenant ID this role belongs to.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// The permissions associated with this role.
    /// </summary>
    public List<string> Permissions { get; init; } = new();

    /// <summary>
    /// Whether this role is active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// Whether this role is a system role (cannot be deleted).
    /// </summary>
    public bool IsSystemRole { get; init; } = false;

    /// <summary>
    /// Custom properties for the role.
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();

    /// <summary>
    /// Timestamp when the role was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the role was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this role.
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Who last updated this role.
    /// </summary>
    public string UpdatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Represents an API key.
/// </summary>
public sealed record ApiKeyInfo
{
    /// <summary>
    /// The API key ID.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The API key name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The API key description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The user ID this API key belongs to.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The tenant ID this API key belongs to.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// The permissions associated with this API key.
    /// </summary>
    public List<string> Permissions { get; init; } = new();

    /// <summary>
    /// Whether this API key is active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// When this API key expires.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// When this API key was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; init; }

    /// <summary>
    /// The IP address this API key was last used from.
    /// </summary>
    public string? LastUsedFromIp { get; init; }

    /// <summary>
    /// Custom properties for the API key.
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new();

    /// <summary>
    /// Timestamp when the API key was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the API key was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this API key.
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Who last updated this API key.
    /// </summary>
    public string UpdatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Checks if the API key is expired.
    /// </summary>
    /// <returns>True if the API key is expired.</returns>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the API key has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check.</param>
    /// <returns>True if the API key has the permission.</returns>
    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Represents an audit event.
/// </summary>
public sealed record AuditEvent
{
    /// <summary>
    /// The audit event ID.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The event type.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// The event category.
    /// </summary>
    public AuditEventCategory Category { get; init; } = AuditEventCategory.General;

    /// <summary>
    /// The user ID who performed the action.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The user name who performed the action.
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// The tenant ID.
    /// </summary>
    public string TenantId { get; init; } = string.Empty;

    /// <summary>
    /// The action performed.
    /// </summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>
    /// The resource that was affected.
    /// </summary>
    public string Resource { get; init; } = string.Empty;

    /// <summary>
    /// The resource ID that was affected.
    /// </summary>
    public string ResourceId { get; init; } = string.Empty;

    /// <summary>
    /// The result of the action.
    /// </summary>
    public AuditEventResult Result { get; init; } = AuditEventResult.Success;

    /// <summary>
    /// The IP address where the action was performed from.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// The user agent of the client.
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Additional details about the event.
    /// </summary>
    public Dictionary<string, object> Details { get; init; } = new();

    /// <summary>
    /// The timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The correlation ID for tracking related events.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// The session ID.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Whether this event is sensitive and should be encrypted.
    /// </summary>
    public bool IsSensitive { get; init; } = false;

    /// <summary>
    /// The severity level of the event.
    /// </summary>
    public AuditEventSeverity Severity { get; init; } = AuditEventSeverity.Info;
}

/// <summary>
/// Audit event categories.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuditEventCategory
{
    /// <summary>
    /// General category.
    /// </summary>
    General = 0,

    /// <summary>
    /// Authentication events.
    /// </summary>
    Authentication = 1,

    /// <summary>
    /// Authorization events.
    /// </summary>
    Authorization = 2,

    /// <summary>
    /// Data access events.
    /// </summary>
    DataAccess = 3,

    /// <summary>
    /// Configuration changes.
    /// </summary>
    Configuration = 4,

    /// <summary>
    /// Security events.
    /// </summary>
    Security = 5,

    /// <summary>
    /// System events.
    /// </summary>
    System = 6,

    /// <summary>
    /// Business events.
    /// </summary>
    Business = 7
}

/// <summary>
/// Audit event results.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuditEventResult
{
    /// <summary>
    /// The action was successful.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The action failed.
    /// </summary>
    Failure = 1,

    /// <summary>
    /// The action was denied.
    /// </summary>
    Denied = 2,

    /// <summary>
    /// The action was cancelled.
    /// </summary>
    Cancelled = 3
}

/// <summary>
/// Audit event severity levels.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuditEventSeverity
{
    /// <summary>
    /// Low severity.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium severity.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High severity.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical severity.
    /// </summary>
    Critical = 3,

    /// <summary>
    /// Information level.
    /// </summary>
    Info = 4
}

/// <summary>
/// Predefined system roles.
/// </summary>
public static class SystemRoles
{
    /// <summary>
    /// System administrator role.
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";

    /// <summary>
    /// Tenant administrator role.
    /// </summary>
    public const string TenantAdmin = "TenantAdmin";

    /// <summary>
    /// Developer role.
    /// </summary>
    public const string Developer = "Developer";

    /// <summary>
    /// Auditor role.
    /// </summary>
    public const string Auditor = "Auditor";

    /// <summary>
    /// Service account role.
    /// </summary>
    public const string ServiceAccount = "ServiceAccount";

    /// <summary>
    /// Read-only user role.
    /// </summary>
    public const string ReadOnly = "ReadOnly";
}

/// <summary>
/// Predefined permissions.
/// </summary>
public static class Permissions
{
    // Notification permissions
    public const string NotificationsRead = "notifications:read";
    public const string NotificationsWrite = "notifications:write";
    public const string NotificationsDelete = "notifications:delete";
    public const string NotificationsSend = "notifications:send";
    public const string NotificationsBulk = "notifications:bulk";

    // Rule permissions
    public const string RulesRead = "rules:read";
    public const string RulesWrite = "rules:write";
    public const string RulesDelete = "rules:delete";
    public const string RulesBulk = "rules:bulk";

    // Template permissions
    public const string TemplatesRead = "templates:read";
    public const string TemplatesWrite = "templates:write";
    public const string TemplatesDelete = "templates:delete";

    // Subscription permissions
    public const string SubscriptionsRead = "subscriptions:read";
    public const string SubscriptionsWrite = "subscriptions:write";
    public const string SubscriptionsDelete = "subscriptions:delete";
    public const string SubscriptionsBulk = "subscriptions:bulk";

    // User management permissions
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string UsersDelete = "users:delete";

    // Role management permissions
    public const string RolesRead = "roles:read";
    public const string RolesWrite = "roles:write";
    public const string RolesDelete = "roles:delete";

    // API key management permissions
    public const string ApiKeysRead = "apikeys:read";
    public const string ApiKeysWrite = "apikeys:write";
    public const string ApiKeysDelete = "apikeys:delete";

    // Audit permissions
    public const string AuditRead = "audit:read";

    // Tenant management permissions
    public const string TenantRead = "tenant:read";
    public const string TenantWrite = "tenant:write";
    public const string TenantDelete = "tenant:delete";

    // System permissions
    public const string SystemRead = "system:read";
    public const string SystemWrite = "system:write";
    public const string SystemAdmin = "system:admin";
}