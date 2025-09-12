using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Extensions;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using FluentAssertions;
using Xunit;

namespace NotifyX.Tests.Integration;

/// <summary>
/// Integration tests for BulkOperationsService with other services.
/// </summary>
public class BulkOperationsIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IBulkOperationsService _bulkOperationsService;
    private readonly INotificationService _notificationService;

    public BulkOperationsIntegrationTests()
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
        _bulkOperationsService = _serviceProvider.GetRequiredService<IBulkOperationsService>();
        _notificationService = _serviceProvider.GetRequiredService<INotificationService>();
    }

    [Fact]
    public async Task BulkOperationsService_WithNotificationService_ShouldProcessBatchEvents()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = new[]
        {
            CreateTestNotificationEvent("event-1", "user.login"),
            CreateTestNotificationEvent("event-2", "user.logout"),
            CreateTestNotificationEvent("event-3", "notification.sent")
        };

        // Act
        var result = await _bulkOperationsService.IngestBatchEventsAsync(tenantId, events);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task BulkOperationsService_WithLargeBatch_ShouldProcessAllEvents()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = Enumerable.Range(1, 100)
            .Select(i => CreateTestNotificationEvent($"event-{i}", "test.event"))
            .ToArray();

        // Act
        var result = await _bulkOperationsService.IngestBatchEventsAsync(tenantId, events);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(100);
        result.SuccessCount.Should().Be(100);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task BulkOperationsService_WithMixedEventTypes_ShouldProcessCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = new[]
        {
            CreateTestNotificationEvent("event-1", "user.login", NotificationPriority.High),
            CreateTestNotificationEvent("event-2", "user.logout", NotificationPriority.Normal),
            CreateTestNotificationEvent("event-3", "system.alert", NotificationPriority.Critical),
            CreateTestNotificationEvent("event-4", "notification.sent", NotificationPriority.Low)
        };

        // Act
        var result = await _bulkOperationsService.IngestBatchEventsAsync(tenantId, events);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(4);
        result.SuccessCount.Should().Be(4);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task BulkOperationsService_WithDifferentChannels_ShouldProcessCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = new[]
        {
            CreateTestNotificationEvent("event-1", "test.event", NotificationPriority.Normal, NotificationChannel.Email),
            CreateTestNotificationEvent("event-2", "test.event", NotificationPriority.Normal, NotificationChannel.SMS),
            CreateTestNotificationEvent("event-3", "test.event", NotificationPriority.Normal, NotificationChannel.Push)
        };

        // Act
        var result = await _bulkOperationsService.IngestBatchEventsAsync(tenantId, events);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task BulkOperationsService_WithEmptyBatch_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = Enumerable.Empty<NotificationEvent>();

        // Act
        var result = await _bulkOperationsService.IngestBatchEventsAsync(tenantId, events);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task BulkOperationsService_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        var tenantId = "test-tenant";
        var events = new[] { CreateTestNotificationEvent("event-1", "test.event") };
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _bulkOperationsService.IngestBatchEventsAsync(tenantId, events, cancellationToken));
    }

    [Fact]
    public async Task BulkOperationsService_WithConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var tasks = Enumerable.Range(1, 10)
            .Select(i => Task.Run(async () =>
            {
                var events = new[] { CreateTestNotificationEvent($"event-{i}", "test.event") };
                return await _bulkOperationsService.IngestBatchEventsAsync(tenantId, events);
            }))
            .ToArray();

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().AllSatisfy(result =>
        {
            result.IsSuccess.Should().BeTrue();
            result.ProcessedCount.Should().Be(1);
            result.SuccessCount.Should().Be(1);
            result.FailureCount.Should().Be(0);
        });
    }

    [Fact]
    public async Task BulkOperationsService_WithImportRules_ShouldProcessJsonData()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";
        var data = """
        {
            "rules": [
                {
                    "name": "Test Rule 1",
                    "description": "Test rule description 1",
                    "condition": {
                        "conditionType": "EventType",
                        "conditionOperator": "Equals",
                        "value": "user.login"
                    },
                    "actions": [
                        {
                            "actionType": "SendNotification",
                            "parameters": {
                                "channel": "Email"
                            }
                        }
                    ]
                },
                {
                    "name": "Test Rule 2",
                    "description": "Test rule description 2",
                    "condition": {
                        "conditionType": "EventType",
                        "conditionOperator": "Equals",
                        "value": "user.logout"
                    },
                    "actions": [
                        {
                            "actionType": "SendNotification",
                            "parameters": {
                                "channel": "SMS"
                            }
                        }
                    ]
                }
            ]
        }
        """;

        // Act
        var result = await _bulkOperationsService.ImportRulesAsync(tenantId, format, data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(2);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task BulkOperationsService_WithImportSubscriptions_ShouldProcessYamlData()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "yaml";
        var data = """
        subscriptions:
          - recipientId: "user-1"
            eventType: "user.login"
            channel: "Email"
            isActive: true
          - recipientId: "user-2"
            eventType: "user.logout"
            channel: "SMS"
            isActive: true
          - recipientId: "user-3"
            eventType: "system.alert"
            channel: "Push"
            isActive: false
        """;

        // Act
        var result = await _bulkOperationsService.ImportSubscriptionsAsync(tenantId, format, data);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
    }

    [Fact]
    public async Task BulkOperationsService_WithExportRules_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "json";

        // Act
        var result = await _bulkOperationsService.ExportRulesAsync(tenantId, format);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(0); // No rules to export in current implementation
    }

    [Fact]
    public async Task BulkOperationsService_WithExportSubscriptions_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = "test-tenant";
        var format = "yaml";

        // Act
        var result = await _bulkOperationsService.ExportSubscriptionsAsync(tenantId, format);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ProcessedCount.Should().Be(0); // No subscriptions to export in current implementation
    }

    private static NotificationEvent CreateTestNotificationEvent(
        string id,
        string eventType,
        NotificationPriority priority = NotificationPriority.Normal,
        NotificationChannel channel = NotificationChannel.Email)
    {
        return new NotificationEvent
        {
            Id = id,
            TenantId = "test-tenant",
            EventType = eventType,
            Priority = priority,
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
            PreferredChannels = new List<NotificationChannel> { channel }
        };
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}

/// <summary>
/// Mock notification provider for testing.
/// </summary>
public class MockNotificationProvider : INotificationProvider
{
    public NotificationChannel Channel => NotificationChannel.Email;
    public bool IsAvailable => true;

    public Task<DeliveryResult> SendAsync(NotificationEvent notification, NotificationRecipient recipient, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DeliveryResult.Success($"mock-delivery-{Guid.NewGuid()}"));
    }

    public ValidationResult Validate(NotificationEvent notification, NotificationRecipient recipient)
    {
        return ValidationResult.Success();
    }

    public Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthStatus.Healthy);
    }

    public Task ConfigureAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}