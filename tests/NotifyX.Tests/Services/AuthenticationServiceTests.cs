using Microsoft.Extensions.Logging;
using Moq;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using NotifyX.Core.Services;
using FluentAssertions;
using Xunit;
using System.Security.Claims;

namespace NotifyX.Tests.Services;

/// <summary>
/// Unit tests for the AuthenticationService class.
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _service;

    public AuthenticationServiceTests()
    {
        _mockLogger = new Mock<ILogger<AuthenticationService>>();
        _service = new AuthenticationService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateApiKeyAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "admin", "user" };
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        var result = await _service.GenerateApiKeyAsync(tenantId, userId, roles, expiresAt);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ApiKey.Should().NotBeNullOrEmpty();
        result.ApiKey.Should().StartWith("nx_");
        result.ExpiresAt.Should().Be(expiresAt);
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task GenerateApiKeyAsync_WithNullExpiresAt_ShouldSetDefaultExpiration()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };

        // Act
        var result = await _service.GenerateApiKeyAsync(tenantId, userId, roles);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ApiKey.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(365), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GenerateApiKeyAsync_WithEmptyRoles_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string>();

        // Act
        var result = await _service.GenerateApiKeyAsync(tenantId, userId, roles);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateApiKeyAsync_WithValidApiKey_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "admin" };
        var generateResult = await _service.GenerateApiKeyAsync(tenantId, userId, roles);
        var apiKey = generateResult.ApiKey;

        // Act
        var result = await _service.ValidateApiKeyAsync(apiKey);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task ValidateApiKeyAsync_WithInvalidApiKey_ShouldReturnFailure()
    {
        // Arrange
        var invalidApiKey = "invalid-api-key";

        // Act
        var result = await _service.ValidateApiKeyAsync(invalidApiKey);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid API key");
    }

    [Fact]
    public async Task ValidateApiKeyAsync_WithExpiredApiKey_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        var expiredDate = DateTime.UtcNow.AddDays(-1);
        var generateResult = await _service.GenerateApiKeyAsync(tenantId, userId, roles, expiredDate);
        var apiKey = generateResult.ApiKey;

        // Act
        var result = await _service.ValidateApiKeyAsync(apiKey);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("API key has expired");
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "admin", "user" };
        var expiration = TimeSpan.FromHours(1);

        // Act
        var result = await _service.GenerateJwtTokenAsync(tenantId, userId, roles, expiration);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(expiration), TimeSpan.FromMinutes(1));
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_WithNullExpiration_ShouldSetDefaultExpiration()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };

        // Act
        var result = await _service.GenerateJwtTokenAsync(tenantId, userId, roles);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ValidateJwtTokenAsync_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "admin" };
        var generateResult = await _service.GenerateJwtTokenAsync(tenantId, userId, roles);
        var token = generateResult.Token;

        // Act
        var result = await _service.ValidateJwtTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TenantId.Should().Be(tenantId);
        result.UserId.Should().Be(userId);
        result.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task ValidateJwtTokenAsync_WithInvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var result = await _service.ValidateJwtTokenAsync(invalidToken);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid JWT token");
    }

    [Fact]
    public async Task ValidateJwtTokenAsync_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        var shortExpiration = TimeSpan.FromMilliseconds(1);
        var generateResult = await _service.GenerateJwtTokenAsync(tenantId, userId, roles, shortExpiration);
        var token = generateResult.Token;

        // Wait for token to expire
        await Task.Delay(100);

        // Act
        var result = await _service.ValidateJwtTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("JWT token has expired");
    }

    [Fact]
    public async Task GetUserRolesAsync_WithValidUser_ShouldReturnRoles()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "admin", "user" };
        await _service.GenerateApiKeyAsync(tenantId, userId, roles);

        // Act
        var result = await _service.GetUserRolesAsync(tenantId, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task GetUserRolesAsync_WithNonExistentUser_ShouldReturnEmptyRoles()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "non-existent-user";

        // Act
        var result = await _service.GetUserRolesAsync(tenantId, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task AssignRoleAsync_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var role = "admin";

        // Act
        var result = await _service.AssignRoleAsync(tenantId, userId, role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveRoleAsync_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var role = "admin";

        // Act
        var result = await _service.RemoveRoleAsync(tenantId, userId, role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateApiKeyAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.GenerateApiKeyAsync(tenantId, userId, roles, cancellationToken: cancellationToken));
    }

    [Fact]
    public async Task ValidateApiKeyAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var apiKey = "test-api-key";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ValidateApiKeyAsync(apiKey, cancellationToken));
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var roles = new List<string> { "user" };
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.GenerateJwtTokenAsync(tenantId, userId, roles, cancellationToken: cancellationToken));
    }

    [Fact]
    public async Task ValidateJwtTokenAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var token = "test-token";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ValidateJwtTokenAsync(token, cancellationToken));
    }

    [Fact]
    public async Task GetUserRolesAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.GetUserRolesAsync(tenantId, userId, cancellationToken));
    }

    [Fact]
    public async Task AssignRoleAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var role = "admin";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.AssignRoleAsync(tenantId, userId, role, cancellationToken));
    }

    [Fact]
    public async Task RemoveRoleAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var userId = "test-user";
        var role = "admin";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.RemoveRoleAsync(tenantId, userId, role, cancellationToken));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GenerateApiKeyAsync_WithInvalidTenantId_ShouldThrowArgumentException(string tenantId)
    {
        // Arrange
        var userId = "test-user";
        var roles = new List<string> { "user" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateApiKeyAsync(tenantId, userId, roles));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GenerateApiKeyAsync_WithInvalidUserId_ShouldThrowArgumentException(string userId)
    {
        // Arrange
        var tenantId = "test-tenant";
        var roles = new List<string> { "user" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateApiKeyAsync(tenantId, userId, roles));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateApiKeyAsync_WithInvalidApiKey_ShouldThrowArgumentException(string apiKey)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ValidateApiKeyAsync(apiKey));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidateJwtTokenAsync_WithInvalidToken_ShouldThrowArgumentException(string token)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ValidateJwtTokenAsync(token));
    }
}