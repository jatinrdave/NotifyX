using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Middleware;
using NotifyX.Core.Models;
using FluentAssertions;
using Xunit;
using System.Security.Claims;
using System.Text;

namespace NotifyX.Tests.Middleware;

/// <summary>
/// Unit tests for the AuthenticationMiddleware class.
/// </summary>
public class AuthenticationMiddlewareTests
{
    private readonly Mock<ILogger<AuthenticationMiddleware>> _mockLogger;
    private readonly Mock<IAuthenticationService> _mockAuthenticationService;
    private readonly RequestDelegate _next;
    private readonly AuthenticationMiddleware _middleware;

    public AuthenticationMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<AuthenticationMiddleware>>();
        _mockAuthenticationService = new Mock<IAuthenticationService>();
        _next = context => Task.CompletedTask;
        _middleware = new AuthenticationMiddleware(_next, _mockLogger.Object, _mockAuthenticationService.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithValidApiKey_ShouldSetUserContext()
    {
        // Arrange
        var context = CreateHttpContext();
        var apiKey = "nx_test_api_key";
        context.Request.Headers["X-API-Key"] = apiKey;

        var validationResult = new ApiKeyValidationResult
        {
            IsValid = true,
            TenantId = "test-tenant",
            UserId = "test-user",
            Roles = new List<string> { "admin", "user" }
        };

        _mockAuthenticationService
            .Setup(s => s.ValidateApiKeyAsync(apiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.User.Should().NotBeNull();
        context.User.Identity.Should().NotBeNull();
        context.User.Identity.IsAuthenticated.Should().BeTrue();
        context.User.FindFirst("tenant_id")?.Value.Should().Be("test-tenant");
        context.User.FindFirst("user_id")?.Value.Should().Be("test-user");
        context.User.FindFirst(ClaimTypes.Role)?.Value.Should().Be("admin");
    }

    [Fact]
    public async Task InvokeAsync_WithValidJwtToken_ShouldSetUserContext()
    {
        // Arrange
        var context = CreateHttpContext();
        var token = "valid.jwt.token";
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        var validationResult = new JwtTokenValidationResult
        {
            IsValid = true,
            TenantId = "test-tenant",
            UserId = "test-user",
            Roles = new List<string> { "user" }
        };

        _mockAuthenticationService
            .Setup(s => s.ValidateJwtTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.User.Should().NotBeNull();
        context.User.Identity.Should().NotBeNull();
        context.User.Identity.IsAuthenticated.Should().BeTrue();
        context.User.FindFirst("tenant_id")?.Value.Should().Be("test-tenant");
        context.User.FindFirst("user_id")?.Value.Should().Be("test-user");
        context.User.FindFirst(ClaimTypes.Role)?.Value.Should().Be("user");
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        var apiKey = "invalid_api_key";
        context.Request.Headers["X-API-Key"] = apiKey;

        var validationResult = new ApiKeyValidationResult
        {
            IsValid = false,
            ErrorMessage = "Invalid API key"
        };

        _mockAuthenticationService
            .Setup(s => s.ValidateApiKeyAsync(apiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidJwtToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        var token = "invalid.jwt.token";
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        var validationResult = new JwtTokenValidationResult
        {
            IsValid = false,
            ErrorMessage = "Invalid JWT token"
        };

        _mockAuthenticationService
            .Setup(s => s.ValidateJwtTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithNoAuthentication_ShouldContinueWithoutAuthentication()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.User.Identity?.IsAuthenticated.Should().BeFalse();
        context.Response.StatusCode.Should().Be(200); // Default status code
    }

    [Fact]
    public async Task InvokeAsync_WithMalformedAuthorizationHeader_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["Authorization"] = "InvalidFormat token";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyAuthorizationHeader_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers["Authorization"] = "Bearer ";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WithApiKeyAndJwtToken_ShouldPreferApiKey()
    {
        // Arrange
        var context = CreateHttpContext();
        var apiKey = "nx_test_api_key";
        var token = "valid.jwt.token";
        
        context.Request.Headers["X-API-Key"] = apiKey;
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        var apiKeyValidationResult = new ApiKeyValidationResult
        {
            IsValid = true,
            TenantId = "api-tenant",
            UserId = "api-user",
            Roles = new List<string> { "admin" }
        };

        _mockAuthenticationService
            .Setup(s => s.ValidateApiKeyAsync(apiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiKeyValidationResult);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.User.FindFirst("tenant_id")?.Value.Should().Be("api-tenant");
        context.User.FindFirst("user_id")?.Value.Should().Be("api-user");
        
        // Verify that JWT validation was not called
        _mockAuthenticationService.Verify(
            s => s.ValidateJwtTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_WithAuthenticationServiceException_ShouldReturnInternalServerError()
    {
        // Arrange
        var context = CreateHttpContext();
        var apiKey = "nx_test_api_key";
        context.Request.Headers["X-API-Key"] = apiKey;

        _mockAuthenticationService
            .Setup(s => s.ValidateApiKeyAsync(apiKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Authentication service error"));

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var context = CreateHttpContext();
        var apiKey = "nx_test_api_key";
        context.Request.Headers["X-API-Key"] = apiKey;

        var cancellationToken = new CancellationToken(true);

        _mockAuthenticationService
            .Setup(s => s.ValidateApiKeyAsync(apiKey, cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _middleware.InvokeAsync(context));
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleRoles_ShouldSetAllRoles()
    {
        // Arrange
        var context = CreateHttpContext();
        var apiKey = "nx_test_api_key";
        context.Request.Headers["X-API-Key"] = apiKey;

        var validationResult = new ApiKeyValidationResult
        {
            IsValid = true,
            TenantId = "test-tenant",
            UserId = "test-user",
            Roles = new List<string> { "admin", "user", "moderator" }
        };

        _mockAuthenticationService
            .Setup(s => s.ValidateApiKeyAsync(apiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var roleClaims = context.User.FindAll(ClaimTypes.Role).ToList();
        roleClaims.Should().HaveCount(3);
        roleClaims.Select(c => c.Value).Should().BeEquivalentTo("admin", "user", "moderator");
    }

    [Fact]
    public async Task InvokeAsync_WithExpiredApiKey_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        var apiKey = "nx_expired_api_key";
        context.Request.Headers["X-API-Key"] = apiKey;

        var validationResult = new ApiKeyValidationResult
        {
            IsValid = false,
            ErrorMessage = "API key has expired"
        };

        _mockAuthenticationService
            .Setup(s => s.ValidateApiKeyAsync(apiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_WithExpiredJwtToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        var token = "expired.jwt.token";
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        var validationResult = new JwtTokenValidationResult
        {
            IsValid = false,
            ErrorMessage = "JWT token has expired"
        };

        _mockAuthenticationService
            .Setup(s => s.ValidateJwtTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        context.User.Identity?.IsAuthenticated.Should().BeFalse();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("localhost");
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        return context;
    }
}