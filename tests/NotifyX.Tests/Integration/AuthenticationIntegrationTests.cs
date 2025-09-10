using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Extensions;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using FluentAssertions;
using Xunit;

namespace NotifyX.Tests.Integration;

/// <summary>
/// Integration tests for AuthenticationService with other services.
/// </summary>
public class AuthenticationIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IAuthenticationService _authenticationService;
    private readonly IAuditService _auditService;

    public AuthenticationIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole());
        
        // Add NotifyX services
        services.AddNotifyX(configuration =>
        {
            configuration.Providers = new[] { typeof(MockNotificationProvider) };
        });

        _serviceProvider = services.BuildServiceProvider();
        _authenticationService = _serviceProvider.GetRequiredService<IAuthenticationService>();
        _auditService = _serviceProvider.GetRequiredService<IAuditService>();
    }

    [Fact]
    public async Task AuthenticationService_WithApiKeyGeneration_ShouldCreateValidKey()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "admin", "user" };
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        var result = await _authenticationService.GenerateApiKeyAsync(tenantId, userId, roles, expiresAt);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ApiKey.Should().NotBeNullOrEmpty();
        result.ApiKey.Should().StartWith("nx_");
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
        result.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public async Task AuthenticationService_WithApiKeyValidation_ShouldValidateCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "admin" };
        
        var generateResult = await _authenticationService.GenerateApiKeyAsync(tenantId, userId, roles);
        var apiKey = generateResult.ApiKey;

        // Act
        var validationResult = await _authenticationService.ValidateApiKeyAsync(apiKey);

        // Assert
        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeTrue();
        validationResult.TenantId.Should().Be(tenantId);
        validationResult.UserId.Should().Be(userId);
        validationResult.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task AuthenticationService_WithJwtTokenGeneration_ShouldCreateValidToken()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        var expiration = TimeSpan.FromHours(1);

        // Act
        var result = await _authenticationService.GenerateJwtTokenAsync(tenantId, userId, roles, expiration);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(expiration), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AuthenticationService_WithJwtTokenValidation_ShouldValidateCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        
        var generateResult = await _authenticationService.GenerateJwtTokenAsync(tenantId, userId, roles);
        var token = generateResult.Token;

        // Act
        var validationResult = await _authenticationService.ValidateJwtTokenAsync(token);

        // Assert
        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeTrue();
        validationResult.TenantId.Should().Be(tenantId);
        validationResult.UserId.Should().Be(userId);
        validationResult.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task AuthenticationService_WithUserRoleManagement_ShouldManageRolesCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        
        // Generate API key to create user context
        await _authenticationService.GenerateApiKeyAsync(tenantId, userId, roles);

        // Act - Assign new role
        var assignResult = await _authenticationService.AssignRoleAsync(tenantId, userId, "admin");
        
        // Get user roles
        var roleResult = await _authenticationService.GetUserRolesAsync(tenantId, userId);

        // Assert
        assignResult.Should().BeTrue();
        roleResult.Should().NotBeNull();
        roleResult.IsSuccess.Should().BeTrue();
        roleResult.Roles.Should().Contain("admin");
    }

    [Fact]
    public async Task AuthenticationService_WithRoleRemoval_ShouldRemoveRoleCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "admin", "user" };
        
        // Generate API key to create user context
        await _authenticationService.GenerateApiKeyAsync(tenantId, userId, roles);

        // Act - Remove role
        var removeResult = await _authenticationService.RemoveRoleAsync(tenantId, userId, "admin");
        
        // Get user roles
        var roleResult = await _authenticationService.GetUserRolesAsync(tenantId, userId);

        // Assert
        removeResult.Should().BeTrue();
        roleResult.Should().NotBeNull();
        roleResult.IsSuccess.Should().BeTrue();
        roleResult.Roles.Should().NotContain("admin");
    }

    [Fact]
    public async Task AuthenticationService_WithExpiredApiKey_ShouldRejectValidation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        var expiredDate = DateTime.UtcNow.AddDays(-1);
        
        var generateResult = await _authenticationService.GenerateApiKeyAsync(tenantId, userId, roles, expiredDate);
        var apiKey = generateResult.ApiKey;

        // Act
        var validationResult = await _authenticationService.ValidateApiKeyAsync(apiKey);

        // Assert
        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeFalse();
        validationResult.ErrorMessage.Should().Contain("expired");
    }

    [Fact]
    public async Task AuthenticationService_WithExpiredJwtToken_ShouldRejectValidation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        var shortExpiration = TimeSpan.FromMilliseconds(1);
        
        var generateResult = await _authenticationService.GenerateJwtTokenAsync(tenantId, userId, roles, shortExpiration);
        var token = generateResult.Token;

        // Wait for token to expire
        await Task.Delay(100);

        // Act
        var validationResult = await _authenticationService.ValidateJwtTokenAsync(token);

        // Assert
        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeFalse();
        validationResult.ErrorMessage.Should().Contain("expired");
    }

    [Fact]
    public async Task AuthenticationService_WithInvalidApiKey_ShouldRejectValidation()
    {
        // Arrange
        var invalidApiKey = "invalid_api_key";

        // Act
        var validationResult = await _authenticationService.ValidateApiKeyAsync(invalidApiKey);

        // Assert
        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeFalse();
        validationResult.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task AuthenticationService_WithInvalidJwtToken_ShouldRejectValidation()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var validationResult = await _authenticationService.ValidateJwtTokenAsync(invalidToken);

        // Assert
        validationResult.Should().NotBeNull();
        validationResult.IsValid.Should().BeFalse();
        validationResult.ErrorMessage.Should().Contain("Invalid");
    }

    [Fact]
    public async Task AuthenticationService_WithMultipleUsers_ShouldIsolateUsers()
    {
        // Arrange
        var tenantId = "test-tenant";
        var user1Id = "user-1";
        var user2Id = "user-2";
        var user1Roles = new List<string> { "admin" };
        var user2Roles = new List<string> { "user" };

        // Act
        var user1Result = await _authenticationService.GenerateApiKeyAsync(tenantId, user1Id, user1Roles);
        var user2Result = await _authenticationService.GenerateApiKeyAsync(tenantId, user2Id, user2Roles);

        var user1Validation = await _authenticationService.ValidateApiKeyAsync(user1Result.ApiKey);
        var user2Validation = await _authenticationService.ValidateApiKeyAsync(user2Result.ApiKey);

        // Assert
        user1Validation.IsValid.Should().BeTrue();
        user1Validation.UserId.Should().Be(user1Id);
        user1Validation.Roles.Should().BeEquivalentTo(user1Roles);

        user2Validation.IsValid.Should().BeTrue();
        user2Validation.UserId.Should().Be(user2Id);
        user2Validation.Roles.Should().BeEquivalentTo(user2Roles);
    }

    [Fact]
    public async Task AuthenticationService_WithMultipleTenants_ShouldIsolateTenants()
    {
        // Arrange
        var tenant1Id = "tenant-1";
        var tenant2Id = "tenant-2";
        var userId = "user-1";
        var roles = new List<string> { "admin" };

        // Act
        var tenant1Result = await _authenticationService.GenerateApiKeyAsync(tenant1Id, userId, roles);
        var tenant2Result = await _authenticationService.GenerateApiKeyAsync(tenant2Id, userId, roles);

        var tenant1Validation = await _authenticationService.ValidateApiKeyAsync(tenant1Result.ApiKey);
        var tenant2Validation = await _authenticationService.ValidateApiKeyAsync(tenant2Result.ApiKey);

        // Assert
        tenant1Validation.IsValid.Should().BeTrue();
        tenant1Validation.TenantId.Should().Be(tenant1Id);

        tenant2Validation.IsValid.Should().BeTrue();
        tenant2Validation.TenantId.Should().Be(tenant2Id);
    }

    [Fact]
    public async Task AuthenticationService_WithConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var tasks = Enumerable.Range(1, 10)
            .Select(i => Task.Run(async () =>
            {
                var userId = $"user-{i}";
                var roles = new List<string> { "user" };
                var result = await _authenticationService.GenerateApiKeyAsync(tenantId, userId, roles);
                return await _authenticationService.ValidateApiKeyAsync(result.ApiKey);
            }))
            .ToArray();

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(result =>
        {
            result.IsValid.Should().BeTrue();
            result.TenantId.Should().Be(tenantId);
            result.Roles.Should().BeEquivalentTo(new List<string> { "user" });
        });
    }

    [Fact]
    public async Task AuditService_WithAuthenticationEvents_ShouldLogCorrectly()
    {
        // Arrange
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            UserId = "test-user",
            Action = "api.key.generated",
            ResourceType = "ApiKey",
            ResourceId = "api-key-123",
            Timestamp = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                ["roles"] = new[] { "admin", "user" },
                ["expiresAt"] = DateTime.UtcNow.AddDays(30)
            },
            IpAddress = "192.168.1.1",
            UserAgent = "NotifyX-Client/1.0"
        };

        // Act
        await _auditService.LogAuditEntryAsync(auditEntry);

        // Assert
        // The audit service should log successfully without throwing exceptions
        // In a real implementation, we would verify the log was written
    }

    [Fact]
    public async Task AuditService_WithMultipleAuditEntries_ShouldLogAllCorrectly()
    {
        // Arrange
        var auditEntries = Enumerable.Range(1, 5)
            .Select(i => new AuditEntry
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = "test-tenant",
                UserId = "test-user",
                Action = $"action-{i}",
                ResourceType = "Test",
                ResourceId = $"resource-{i}",
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["index"] = i
                }
            })
            .ToArray();

        // Act
        var tasks = auditEntries.Select(entry => _auditService.LogAuditEntryAsync(entry));
        await Task.WhenAll(tasks);

        // Assert
        // All audit entries should be logged successfully
        // In a real implementation, we would verify all logs were written
    }

    [Fact]
    public async Task AuthenticationService_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _authenticationService.GenerateApiKeyAsync(tenantId, userId, roles, cancellationToken: cancellationToken));
    }

    [Fact]
    public async Task AuditService_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            UserId = "test-user",
            Action = "test.action",
            ResourceType = "Test",
            ResourceId = "test-123",
            Timestamp = DateTime.UtcNow
        };
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _auditService.LogAuditEntryAsync(auditEntry, cancellationToken));
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}