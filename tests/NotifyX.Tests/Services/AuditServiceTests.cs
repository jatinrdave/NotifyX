using Microsoft.Extensions.Logging;
using Moq;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using NotifyX.Core.Services;
using FluentAssertions;
using Xunit;

namespace NotifyX.Tests.Services;

/// <summary>
/// Unit tests for the AuditService class.
/// </summary>
public class AuditServiceTests
{
    private readonly Mock<ILogger<AuditService>> _mockLogger;
    private readonly AuditService _service;

    public AuditServiceTests()
    {
        _mockLogger = new Mock<ILogger<AuditService>>();
        _service = new AuditService(_mockLogger.Object);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithValidAuditEntry_ShouldLogSuccessfully()
    {
        // Arrange
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            UserId = "test-user",
            Action = "notification.sent",
            ResourceType = "Notification",
            ResourceId = "notification-123",
            Timestamp = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                ["channel"] = "Email",
                ["recipient"] = "user@example.com"
            },
            IpAddress = "192.168.1.1",
            UserAgent = "NotifyX-Client/1.0"
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        // Verify that the logger was called with the expected log level and message
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit entry logged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithMinimalAuditEntry_ShouldLogSuccessfully()
    {
        // Arrange
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            UserId = "test-user",
            Action = "user.login",
            ResourceType = "User",
            ResourceId = "user-123",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit entry logged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithNullAuditEntry_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _service.LogAuditEntryAsync(null!));
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithEmptyTenantId_ShouldLogWithWarning()
    {
        // Arrange
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "",
            UserId = "test-user",
            Action = "test.action",
            ResourceType = "Test",
            ResourceId = "test-123",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Empty tenant ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithEmptyUserId_ShouldLogWithWarning()
    {
        // Arrange
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            UserId = "",
            Action = "test.action",
            ResourceType = "Test",
            ResourceId = "test-123",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Empty user ID")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithComplexDetails_ShouldLogSuccessfully()
    {
        // Arrange
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            UserId = "test-user",
            Action = "bulk.import",
            ResourceType = "Rules",
            ResourceId = "bulk-import-123",
            Timestamp = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                ["format"] = "json",
                ["count"] = 150,
                ["successCount"] = 148,
                ["failureCount"] = 2,
                ["errors"] = new[] { "Invalid rule format", "Missing required field" },
                ["metadata"] = new Dictionary<string, object>
                {
                    ["source"] = "api",
                    ["version"] = "1.0.0"
                }
            },
            IpAddress = "10.0.0.1",
            UserAgent = "NotifyX-Admin/2.0"
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit entry logged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithCancellationToken_ShouldRespectCancellation()
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
            () => _service.LogAuditEntryAsync(auditEntry, cancellationToken));
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithException_ShouldLogError()
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

        // Setup logger to throw exception
        _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Throws(new Exception("Logging failed"));

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        // The service should handle the exception gracefully
        // and not throw it back to the caller
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to log audit entry")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("user.login")]
    [InlineData("user.logout")]
    [InlineData("notification.sent")]
    [InlineData("notification.failed")]
    [InlineData("rule.created")]
    [InlineData("rule.updated")]
    [InlineData("rule.deleted")]
    [InlineData("bulk.import")]
    [InlineData("bulk.export")]
    [InlineData("api.key.generated")]
    [InlineData("api.key.revoked")]
    public async Task LogAuditEntryAsync_WithDifferentActions_ShouldLogSuccessfully(string action)
    {
        // Arrange
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            UserId = "test-user",
            Action = action,
            ResourceType = "Test",
            ResourceId = "test-123",
            Timestamp = DateTime.UtcNow
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit entry logged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithFutureTimestamp_ShouldLogSuccessfully()
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
            Timestamp = DateTime.UtcNow.AddMinutes(5) // Future timestamp
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit entry logged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithPastTimestamp_ShouldLogSuccessfully()
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
            Timestamp = DateTime.UtcNow.AddMinutes(-5) // Past timestamp
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit entry logged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithNullDetails_ShouldLogSuccessfully()
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
            Timestamp = DateTime.UtcNow,
            Details = null
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit entry logged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAuditEntryAsync_WithEmptyDetails_ShouldLogSuccessfully()
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
            Timestamp = DateTime.UtcNow,
            Details = new Dictionary<string, object>()
        };

        // Act
        await _service.LogAuditEntryAsync(auditEntry);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Audit entry logged")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}