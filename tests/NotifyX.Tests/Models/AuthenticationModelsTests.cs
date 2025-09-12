using NotifyX.Core.Models;
using FluentAssertions;
using Xunit;

namespace NotifyX.Tests.Models;

/// <summary>
/// Unit tests for the AuthenticationModels.
/// </summary>
public class AuthenticationModelsTests
{
    [Fact]
    public void ApiKey_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Act
        var apiKey = new ApiKey();

        // Assert
        apiKey.Id.Should().NotBeNullOrEmpty();
        apiKey.Key.Should().BeEmpty();
        apiKey.TenantId.Should().BeEmpty();
        apiKey.UserId.Should().BeEmpty();
        apiKey.Roles.Should().NotBeNull();
        apiKey.Roles.Should().BeEmpty();
        apiKey.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        apiKey.ExpiresAt.Should().BeNull();
        apiKey.IsActive.Should().BeTrue();
        apiKey.LastUsedAt.Should().BeNull();
        apiKey.UsageCount.Should().Be(0);
    }

    [Fact]
    public void ApiKey_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var key = "nx_test_api_key";
        var tenantId = "test-tenant";
        var userId = "user-123";
        var roles = new List<string> { "admin", "user" };
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var expiresAt = DateTime.UtcNow.AddDays(30);
        var lastUsedAt = DateTime.UtcNow.AddHours(-1);
        var usageCount = 42;

        // Act
        var apiKey = new ApiKey
        {
            Id = id,
            Key = key,
            TenantId = tenantId,
            UserId = userId,
            Roles = roles,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
            IsActive = false,
            LastUsedAt = lastUsedAt,
            UsageCount = usageCount
        };

        // Assert
        apiKey.Id.Should().Be(id);
        apiKey.Key.Should().Be(key);
        apiKey.TenantId.Should().Be(tenantId);
        apiKey.UserId.Should().Be(userId);
        apiKey.Roles.Should().BeEquivalentTo(roles);
        apiKey.CreatedAt.Should().Be(createdAt);
        apiKey.ExpiresAt.Should().Be(expiresAt);
        apiKey.IsActive.Should().BeFalse();
        apiKey.LastUsedAt.Should().Be(lastUsedAt);
        apiKey.UsageCount.Should().Be(usageCount);
    }

    [Fact]
    public void User_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().NotBeNullOrEmpty();
        user.TenantId.Should().BeEmpty();
        user.Username.Should().BeEmpty();
        user.Email.Should().BeEmpty();
        user.Roles.Should().NotBeNull();
        user.Roles.Should().BeEmpty();
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.LastLoginAt.Should().BeNull();
        user.Metadata.Should().NotBeNull();
        user.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void User_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var tenantId = "test-tenant";
        var username = "testuser";
        var email = "test@example.com";
        var roles = new List<string> { "admin" };
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var lastLoginAt = DateTime.UtcNow.AddHours(-1);
        var metadata = new Dictionary<string, object>
        {
            ["department"] = "IT",
            ["level"] = "senior"
        };

        // Act
        var user = new User
        {
            Id = id,
            TenantId = tenantId,
            Username = username,
            Email = email,
            Roles = roles,
            IsActive = false,
            CreatedAt = createdAt,
            LastLoginAt = lastLoginAt,
            Metadata = metadata
        };

        // Assert
        user.Id.Should().Be(id);
        user.TenantId.Should().Be(tenantId);
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.Roles.Should().BeEquivalentTo(roles);
        user.IsActive.Should().BeFalse();
        user.CreatedAt.Should().Be(createdAt);
        user.LastLoginAt.Should().Be(lastLoginAt);
        user.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void Role_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Act
        var role = new Role();

        // Assert
        role.Id.Should().NotBeNullOrEmpty();
        role.TenantId.Should().BeEmpty();
        role.Name.Should().BeEmpty();
        role.Description.Should().BeEmpty();
        role.Permissions.Should().NotBeNull();
        role.Permissions.Should().BeEmpty();
        role.IsActive.Should().BeTrue();
        role.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        role.Metadata.Should().NotBeNull();
        role.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Role_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var tenantId = "test-tenant";
        var name = "admin";
        var description = "Administrator role";
        var permissions = new List<string> { "user.manage", "notification.send" };
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var metadata = new Dictionary<string, object>
        {
            ["level"] = "high",
            ["category"] = "management"
        };

        // Act
        var role = new Role
        {
            Id = id,
            TenantId = tenantId,
            Name = name,
            Description = description,
            Permissions = permissions,
            IsActive = false,
            CreatedAt = createdAt,
            Metadata = metadata
        };

        // Assert
        role.Id.Should().Be(id);
        role.TenantId.Should().Be(tenantId);
        role.Name.Should().Be(name);
        role.Description.Should().Be(description);
        role.Permissions.Should().BeEquivalentTo(permissions);
        role.IsActive.Should().BeFalse();
        role.CreatedAt.Should().Be(createdAt);
        role.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void ApiKeyResult_WithSuccess_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var apiKey = "nx_test_api_key";
        var tenantId = "test-tenant";
        var userId = "user-123";
        var roles = new List<string> { "admin" };
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        var result = new ApiKeyResult
        {
            IsSuccess = true,
            ApiKey = apiKey,
            TenantId = tenantId,
            UserId = userId,
            Roles = roles,
            ExpiresAt = expiresAt
        };

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.ApiKey.Should().Be(apiKey);
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
        result.ExpiresAt.Should().Be(expiresAt);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ApiKeyResult_WithFailure_ShouldSetErrorProperties()
    {
        // Arrange
        var errorMessage = "API key generation failed";

        // Act
        var result = new ApiKeyResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.ApiKey.Should().BeNull();
        result.TenantId.Should().BeNull();
        result.UserId.Should().BeNull();
        result.Roles.Should().BeNull();
        result.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void ApiKeyValidationResult_WithValidKey_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "user-123";
        var roles = new List<string> { "admin", "user" };

        // Act
        var result = new ApiKeyValidationResult
        {
            IsValid = true,
            TenantId = tenantId,
            UserId = userId,
            Roles = roles
        };

        // Assert
        result.IsValid.Should().BeTrue();
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void ApiKeyValidationResult_WithInvalidKey_ShouldSetErrorProperties()
    {
        // Arrange
        var errorMessage = "Invalid API key";

        // Act
        var result = new ApiKeyValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.TenantId.Should().BeNull();
        result.UserId.Should().BeNull();
        result.Roles.Should().BeNull();
    }

    [Fact]
    public void JwtTokenResult_WithSuccess_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var token = "valid.jwt.token";
        var tenantId = "test-tenant";
        var userId = "user-123";
        var roles = new List<string> { "user" };
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var result = new JwtTokenResult
        {
            IsSuccess = true,
            Token = token,
            TenantId = tenantId,
            UserId = userId,
            Roles = roles,
            ExpiresAt = expiresAt
        };

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().Be(token);
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
        result.ExpiresAt.Should().Be(expiresAt);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void JwtTokenResult_WithFailure_ShouldSetErrorProperties()
    {
        // Arrange
        var errorMessage = "JWT token generation failed";

        // Act
        var result = new JwtTokenResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.Token.Should().BeNull();
        result.TenantId.Should().BeNull();
        result.UserId.Should().BeNull();
        result.Roles.Should().BeNull();
        result.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void JwtTokenValidationResult_WithValidToken_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "user-123";
        var roles = new List<string> { "admin" };

        // Act
        var result = new JwtTokenValidationResult
        {
            IsValid = true,
            TenantId = tenantId,
            UserId = userId,
            Roles = roles
        };

        // Assert
        result.IsValid.Should().BeTrue();
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void JwtTokenValidationResult_WithInvalidToken_ShouldSetErrorProperties()
    {
        // Arrange
        var errorMessage = "Invalid JWT token";

        // Act
        var result = new JwtTokenValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.TenantId.Should().BeNull();
        result.UserId.Should().BeNull();
        result.Roles.Should().BeNull();
    }

    [Fact]
    public void UserRoleResult_WithSuccess_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var roles = new List<string> { "admin", "user", "moderator" };

        // Act
        var result = new UserRoleResult
        {
            IsSuccess = true,
            Roles = roles
        };

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Roles.Should().BeEquivalentTo(roles);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void UserRoleResult_WithFailure_ShouldSetErrorProperties()
    {
        // Arrange
        var errorMessage = "Failed to retrieve user roles";

        // Act
        var result = new UserRoleResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.Roles.Should().BeNull();
    }

    [Fact]
    public void AuthenticationOptions_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Act
        var options = new AuthenticationOptions();

        // Assert
        options.Jwt.Should().NotBeNull();
        options.ApiKey.Should().NotBeNull();
        options.RequireHttps.Should().BeTrue();
        options.TokenExpiration.Should().Be(TimeSpan.FromHours(24));
        options.ApiKeyExpiration.Should().Be(TimeSpan.FromDays(365));
    }

    [Fact]
    public void AuthenticationOptions_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var jwtOptions = new JwtOptions
        {
            SecretKey = "test-secret",
            Issuer = "test-issuer",
            Audience = "test-audience"
        };
        var apiKeyOptions = new ApiKeyOptions
        {
            Prefix = "test_",
            Length = 32
        };
        var tokenExpiration = TimeSpan.FromHours(1);
        var apiKeyExpiration = TimeSpan.FromDays(30);

        // Act
        var options = new AuthenticationOptions
        {
            Jwt = jwtOptions,
            ApiKey = apiKeyOptions,
            RequireHttps = false,
            TokenExpiration = tokenExpiration,
            ApiKeyExpiration = apiKeyExpiration
        };

        // Assert
        options.Jwt.Should().Be(jwtOptions);
        options.ApiKey.Should().Be(apiKeyOptions);
        options.RequireHttps.Should().BeFalse();
        options.TokenExpiration.Should().Be(tokenExpiration);
        options.ApiKeyExpiration.Should().Be(apiKeyExpiration);
    }

    [Fact]
    public void JwtOptions_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Act
        var options = new JwtOptions();

        // Assert
        options.SecretKey.Should().BeEmpty();
        options.Issuer.Should().BeEmpty();
        options.Audience.Should().BeEmpty();
        options.Expiration.Should().Be(TimeSpan.FromHours(24));
    }

    [Fact]
    public void JwtOptions_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var secretKey = "test-secret-key";
        var issuer = "test-issuer";
        var audience = "test-audience";
        var expiration = TimeSpan.FromHours(1);

        // Act
        var options = new JwtOptions
        {
            SecretKey = secretKey,
            Issuer = issuer,
            Audience = audience,
            Expiration = expiration
        };

        // Assert
        options.SecretKey.Should().Be(secretKey);
        options.Issuer.Should().Be(issuer);
        options.Audience.Should().Be(audience);
        options.Expiration.Should().Be(expiration);
    }

    [Fact]
    public void ApiKeyOptions_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Act
        var options = new ApiKeyOptions();

        // Assert
        options.Prefix.Should().Be("nx_");
        options.Length.Should().Be(32);
        options.Expiration.Should().Be(TimeSpan.FromDays(365));
    }

    [Fact]
    public void ApiKeyOptions_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var prefix = "test_";
        var length = 64;
        var expiration = TimeSpan.FromDays(30);

        // Act
        var options = new ApiKeyOptions
        {
            Prefix = prefix,
            Length = length,
            Expiration = expiration
        };

        // Assert
        options.Prefix.Should().Be(prefix);
        options.Length.Should().Be(length);
        options.Expiration.Should().Be(expiration);
    }

    [Fact]
    public void AuditOptions_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Act
        var options = new AuditOptions();

        // Assert
        options.EnableAuditLogging.Should().BeTrue();
        options.LogLevel.Should().Be(LogLevel.Information);
        options.RetentionDays.Should().Be(90);
        options.IncludeRequestDetails.Should().BeTrue();
        options.IncludeResponseDetails.Should().BeFalse();
    }

    [Fact]
    public void AuditOptions_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var logLevel = LogLevel.Warning;
        var retentionDays = 30;
        var includeRequestDetails = false;
        var includeResponseDetails = true;

        // Act
        var options = new AuditOptions
        {
            EnableAuditLogging = false,
            LogLevel = logLevel,
            RetentionDays = retentionDays,
            IncludeRequestDetails = includeRequestDetails,
            IncludeResponseDetails = includeResponseDetails
        };

        // Assert
        options.EnableAuditLogging.Should().BeFalse();
        options.LogLevel.Should().Be(logLevel);
        options.RetentionDays.Should().Be(retentionDays);
        options.IncludeRequestDetails.Should().Be(includeRequestDetails);
        options.IncludeResponseDetails.Should().Be(includeResponseDetails);
    }
}