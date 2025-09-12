using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NotifyX.Core.Middleware;
using FluentAssertions;
using Xunit;
using System.Security.Claims;

namespace NotifyX.Tests.Middleware;

/// <summary>
/// Unit tests for the AuthorizationMiddleware class.
/// </summary>
public class AuthorizationMiddlewareTests
{
    private readonly Mock<ILogger<AuthorizationMiddleware>> _mockLogger;
    private readonly RequestDelegate _next;
    private readonly AuthorizationMiddleware _middleware;

    public AuthorizationMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<AuthorizationMiddleware>>();
        _next = context => Task.CompletedTask;
        _middleware = new AuthorizationMiddleware(_next, _mockLogger.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithRequiredRole_ShouldAllowAccess()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("admin", "user");
        context.Items["RequiredRoles"] = new[] { "admin" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleRequiredRoles_ShouldAllowAccessIfUserHasAny()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("user");
        context.Items["RequiredRoles"] = new[] { "admin", "user", "moderator" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithRequiredRole_ShouldDenyAccessIfUserLacksRole()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("user");
        context.Items["RequiredRoles"] = new[] { "admin" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_WithNoRequiredRoles_ShouldAllowAccess()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("user");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthenticatedUser_ShouldDenyAccess()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUnauthenticatedUser();
        context.Items["RequiredRoles"] = new[] { "admin" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyRequiredRoles_ShouldAllowAccess()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("user");
        context.Items["RequiredRoles"] = new string[0];

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithNullRequiredRoles_ShouldAllowAccess()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("user");
        context.Items["RequiredRoles"] = null;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithTenantIdRequirement_ShouldAllowAccessIfTenantMatches()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithTenant("tenant-1");
        context.Items["RequiredTenantId"] = "tenant-1";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithTenantIdRequirement_ShouldDenyAccessIfTenantMismatch()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithTenant("tenant-1");
        context.Items["RequiredTenantId"] = "tenant-2";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_WithTenantIdRequirement_ShouldDenyAccessIfNoTenantClaim()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("admin");
        context.Items["RequiredTenantId"] = "tenant-1";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_WithBothRoleAndTenantRequirements_ShouldAllowAccessIfBothMatch()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRolesAndTenant("admin", "tenant-1");
        context.Items["RequiredRoles"] = new[] { "admin" };
        context.Items["RequiredTenantId"] = "tenant-1";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithBothRoleAndTenantRequirements_ShouldDenyAccessIfRoleMismatch()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRolesAndTenant("user", "tenant-1");
        context.Items["RequiredRoles"] = new[] { "admin" };
        context.Items["RequiredTenantId"] = "tenant-1";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_WithBothRoleAndTenantRequirements_ShouldDenyAccessIfTenantMismatch()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRolesAndTenant("admin", "tenant-1");
        context.Items["RequiredRoles"] = new[] { "admin" };
        context.Items["RequiredTenantId"] = "tenant-2";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_WithCaseSensitiveRoles_ShouldBeCaseSensitive()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("Admin"); // Capital A
        context.Items["RequiredRoles"] = new[] { "admin" }; // lowercase a

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_WithWhitespaceInRoles_ShouldTrimWhitespace()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles(" admin ", " user ");
        context.Items["RequiredRoles"] = new[] { "admin", "user" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithNullUser_ShouldDenyAccess()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = null!;
        context.Items["RequiredRoles"] = new[] { "admin" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WithNullUserIdentity_ShouldDenyAccess()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = new ClaimsPrincipal();
        context.Items["RequiredRoles"] = new[] { "admin" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_WithExceptionInNext_ShouldNotCatchException()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithRoles("admin");
        
        var exceptionThrowingNext = new RequestDelegate(ctx => throw new InvalidOperationException("Test exception"));
        var exceptionMiddleware = new AuthorizationMiddleware(exceptionThrowingNext, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => exceptionMiddleware.InvokeAsync(context));
    }

    [Fact]
    public async Task InvokeAsync_WithCustomPermissionRequirement_ShouldAllowAccess()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithPermissions("notification.send", "user.manage");
        context.Items["RequiredPermissions"] = new[] { "notification.send" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithCustomPermissionRequirement_ShouldDenyAccessIfMissingPermission()
    {
        // Arrange
        var context = CreateHttpContext();
        context.User = CreateUserWithPermissions("user.manage");
        context.Items["RequiredPermissions"] = new[] { "notification.send" };

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(403);
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

    private static ClaimsPrincipal CreateUserWithRoles(params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.NameIdentifier, "user-123")
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUserWithTenant(string tenantId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.NameIdentifier, "user-123"),
            new("tenant_id", tenantId)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUserWithRolesAndTenant(string role, string tenantId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.NameIdentifier, "user-123"),
            new(ClaimTypes.Role, role),
            new("tenant_id", tenantId)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUserWithPermissions(params string[] permissions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.NameIdentifier, "user-123")
        };

        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var identity = new ClaimsIdentity(claims, "Test");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateUnauthenticatedUser()
    {
        var identity = new ClaimsIdentity();
        return new ClaimsPrincipal(identity);
    }
}