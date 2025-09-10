using Microsoft.Extensions.Logging;
using Moq;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using NotifyX.Core.Services;
using FluentAssertions;
using Xunit;

namespace NotifyX.Tests.Services;

/// <summary>
/// Unit tests for the BulkOperationsService class.
/// </summary>
public class BulkOperationsServiceTests
{
    private readonly Mock<ILogger<BulkOperationsService>> _mockLogger;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly BulkOperationsService _service;

    public BulkOperationsServiceTests()
    {
        _mockLogger = new Mock<ILogger<BulkOperationsService>>();
        _mockNotificationService = new Mock<INotificationService>();
        _service = new BulkOperationsService(_mockLogger.Object, _mockNotificationService.Object);
    }

    [Fact]
    public async Task ImportRulesAsync_WithValidJsonData_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";
        var data = """
        {
            "rules": [
                {
                    "name": "Test Rule",
                    "description": "Test rule description",
                    "condition": {
                        "conditionType": "EventType",
                        "conditionOperator": "Equals",
                        "value": "test.event"
                    },
                    "actions": [
                        {
                            "actionType": "SendNotification",
                            "parameters": {
                                "channel": "Email"
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = await _service.ImportRulesAsync(tenantId, format, data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(1);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportRulesAsync_WithValidYamlData_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "yaml";
        var data = """
        rules:
          - name: "Test Rule"
            description: "Test rule description"
            condition:
              conditionType: "EventType"
              conditionOperator: "Equals"
              value: "test.event"
            actions:
              - actionType: "SendNotification"
                parameters:
                  channel: "Email"
        """;

        // Act
        var result = await _service.ImportRulesAsync(tenantId, format, data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(1);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportRulesAsync_WithInvalidFormat_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "invalid";
        var data = "invalid data";

        // Act
        var result = await _service.ImportRulesAsync(tenantId, format, data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unsupported format");
    }

    [Fact]
    public async Task ImportRulesAsync_WithInvalidJsonData_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";
        var data = "invalid json data";

        // Act
        var result = await _service.ImportRulesAsync(tenantId, format, data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to parse");
    }

    [Fact]
    public async Task ExportRulesAsync_WithValidFormat_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";

        // Act
        var result = await _service.ExportRulesAsync(tenantId, format);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(0); // No rules to export in current implementation
    }

    [Fact]
    public async Task ImportSubscriptionsAsync_WithValidJsonData_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";
        var data = """
        {
            "subscriptions": [
                {
                    "recipientId": "user-1",
                    "eventType": "test.event",
                    "channel": "Email",
                    "isActive": true
                }
            ]
        }
        """;

        // Act
        var result = await _service.ImportSubscriptionsAsync(tenantId, format, data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(1);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task ImportSubscriptionsAsync_WithValidYamlData_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "yaml";
        var data = """
        subscriptions:
          - recipientId: "user-1"
            eventType: "test.event"
            channel: "Email"
            isActive: true
        """;

        // Act
        var result = await _service.ImportSubscriptionsAsync(tenantId, format, data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(1);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task ExportSubscriptionsAsync_WithValidFormat_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";

        // Act
        var result = await _service.ExportSubscriptionsAsync(tenantId, format);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(0); // No subscriptions to export in current implementation
    }

    [Fact]
    public async Task IngestBatchEventsAsync_WithValidEvents_ShouldDelegateToNotificationService()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = new[]
        {
            CreateTestNotificationEvent("event-1"),
            CreateTestNotificationEvent("event-2"),
            CreateTestNotificationEvent("event-3")
        };

        var batchResult = new BatchNotificationResult
        {
            TotalCount = 3,
            SuccessCount = 3,
            FailureCount = 0,
            Status = BatchStatus.AllSuccessful,
            Results = events.Select(e => new NotificationResult
            {
                IsSuccess = true,
                NotificationId = e.Id
            }).ToList()
        };

        _mockNotificationService
            .Setup(s => s.SendBatchAsync(It.IsAny<IEnumerable<NotificationEvent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(batchResult);

        // Act
        var result = await _service.IngestBatchEventsAsync(tenantId, events);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);

        _mockNotificationService.Verify(
            s => s.SendBatchAsync(It.IsAny<IEnumerable<NotificationEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IngestBatchEventsAsync_WithEmptyEvents_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = Enumerable.Empty<NotificationEvent>();

        // Act
        var result = await _service.IngestBatchEventsAsync(tenantId, events);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task IngestBatchEventsAsync_WithNotificationServiceFailure_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = new[] { CreateTestNotificationEvent("event-1") };

        _mockNotificationService
            .Setup(s => s.SendBatchAsync(It.IsAny<IEnumerable<NotificationEvent>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Notification service error"));

        // Act
        var result = await _service.IngestBatchEventsAsync(tenantId, events);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Notification service error");
    }

    [Fact]
    public async Task ImportRulesAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";
        var data = """{"rules": []}""";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ImportRulesAsync(tenantId, format, data, cancellationToken));
    }

    [Fact]
    public async Task ExportRulesAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ExportRulesAsync(tenantId, format, cancellationToken));
    }

    [Fact]
    public async Task ImportSubscriptionsAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";
        var data = """{"subscriptions": []}""";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ImportSubscriptionsAsync(tenantId, format, data, cancellationToken));
    }

    [Fact]
    public async Task ExportSubscriptionsAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ExportSubscriptionsAsync(tenantId, format, cancellationToken));
    }

    [Fact]
    public async Task IngestBatchEventsAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = new[] { CreateTestNotificationEvent("event-1") };
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.IngestBatchEventsAsync(tenantId, events, cancellationToken));
    }

    private static NotificationEvent CreateTestNotificationEvent(string id)
    {
        return new NotificationEvent
        {
            Id = id,
            TenantId = "test-tenant",
            EventType = "test.event",
            Priority = NotificationPriority.Normal,
            Subject = "Test Notification",
            Content = "This is a test notification",
            Recipients = new List<NotificationRecipient>
            {
                new NotificationRecipient
                {
                    Id = "test-recipient",
                    Name = "Test User",
                    Email = "test@example.com"
                }
            },
            PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email }
        };
    }
}