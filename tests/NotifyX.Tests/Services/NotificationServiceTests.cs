using Microsoft.Extensions.Logging;
using Moq;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using NotifyX.Core.Services;
using FluentAssertions;
using Xunit;

namespace NotifyX.Tests.Services;

/// <summary>
/// Unit tests for the NotificationService class.
/// </summary>
public class NotificationServiceTests
{
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly Mock<IRuleEngine> _mockRuleEngine;
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<INotificationProvider> _mockProvider;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _mockRuleEngine = new Mock<IRuleEngine>();
        _mockTemplateService = new Mock<ITemplateService>();
        _mockProvider = new Mock<INotificationProvider>();

        _mockProvider.Setup(p => p.Channel).Returns(NotificationChannel.Email);
        _mockProvider.Setup(p => p.IsAvailable).Returns(true);

        var providers = new[] { _mockProvider.Object };

        _service = new NotificationService(
            _mockLogger.Object,
            _mockRuleEngine.Object,
            _mockTemplateService.Object,
            providers);
    }

    [Fact]
    public async Task SendAsync_WithValidNotification_ShouldReturnSuccess()
    {
        // Arrange
        var notification = CreateTestNotification();
        var ruleResult = RuleEvaluationResult.Success(new List<NotificationRule>());
        var deliveryResult = DeliveryResult.Success("test-delivery-id");

        _mockRuleEngine.Setup(r => r.EvaluateRulesAsync(notification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ruleResult);
        _mockProvider.Setup(p => p.Validate(notification, It.IsAny<NotificationRecipient>()))
            .Returns(ValidationResult.Success());
        _mockProvider.Setup(p => p.SendAsync(notification, It.IsAny<NotificationRecipient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        // Act
        var result = await _service.SendAsync(notification);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.NotificationId.Should().Be(notification.Id);
        result.DeliveryResults.Should().HaveCount(1);
        result.DeliveryResults.First().IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_WithRuleEvaluationFailure_ShouldReturnFailure()
    {
        // Arrange
        var notification = CreateTestNotification();
        var ruleResult = RuleEvaluationResult.Failure("Rule evaluation failed");

        _mockRuleEngine.Setup(r => r.EvaluateRulesAsync(notification, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ruleResult);

        // Act
        var result = await _service.SendAsync(notification);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Rule evaluation failed");
    }

    [Fact]
    public async Task SendAsync_WithTemplateRendering_ShouldRenderTemplate()
    {
        // Arrange
        var notification = CreateTestNotification();
        notification = notification.With(builder => builder.WithTemplateId("test-template"));
        
        var ruleResult = RuleEvaluationResult.Success(new List<NotificationRule>());
        var templateResult = TemplateRenderResult.Success("Rendered content", "Rendered subject");
        var deliveryResult = DeliveryResult.Success("test-delivery-id");

        _mockRuleEngine.Setup(r => r.EvaluateRulesAsync(It.IsAny<NotificationEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ruleResult);
        _mockTemplateService.Setup(t => t.RenderAsync(It.IsAny<NotificationEvent>(), "test-template", It.IsAny<CancellationToken>()))
            .ReturnsAsync(templateResult);
        _mockProvider.Setup(p => p.Validate(It.IsAny<NotificationEvent>(), It.IsAny<NotificationRecipient>()))
            .Returns(ValidationResult.Success());
        _mockProvider.Setup(p => p.SendAsync(It.IsAny<NotificationEvent>(), It.IsAny<NotificationRecipient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        // Act
        var result = await _service.SendAsync(notification);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _mockTemplateService.Verify(t => t.RenderAsync(It.IsAny<NotificationEvent>(), "test-template", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendBatchAsync_WithMultipleNotifications_ShouldProcessAll()
    {
        // Arrange
        var notifications = new[]
        {
            CreateTestNotification("notification-1"),
            CreateTestNotification("notification-2"),
            CreateTestNotification("notification-3")
        };

        var ruleResult = RuleEvaluationResult.Success(new List<NotificationRule>());
        var deliveryResult = DeliveryResult.Success("test-delivery-id");

        _mockRuleEngine.Setup(r => r.EvaluateRulesAsync(It.IsAny<NotificationEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ruleResult);
        _mockProvider.Setup(p => p.Validate(It.IsAny<NotificationEvent>(), It.IsAny<NotificationRecipient>()))
            .Returns(ValidationResult.Success());
        _mockProvider.Setup(p => p.SendAsync(It.IsAny<NotificationEvent>(), It.IsAny<NotificationRecipient>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        // Act
        var result = await _service.SendBatchAsync(notifications);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.SuccessCount.Should().Be(3);
        result.FailureCount.Should().Be(0);
        result.Status.Should().Be(BatchStatus.AllSuccessful);
    }

    [Fact]
    public async Task ScheduleAsync_WithFutureDate_ShouldScheduleNotification()
    {
        // Arrange
        var notification = CreateTestNotification();
        var scheduledFor = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await _service.ScheduleAsync(notification, scheduledFor);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CancelAsync_WithPendingNotification_ShouldCancelSuccessfully()
    {
        // Arrange
        var notificationId = "test-notification-id";

        // Act
        var result = await _service.CancelAsync(notificationId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatusAsync_WithExistingNotification_ShouldReturnStatus()
    {
        // Arrange
        var notificationId = "test-notification-id";

        // Act
        var result = await _service.GetStatusAsync(notificationId);

        // Assert
        result.Should().NotBeNull();
        result.NotificationId.Should().Be(notificationId);
    }

    [Fact]
    public async Task GetDeliveryHistoryAsync_WithExistingNotification_ShouldReturnHistory()
    {
        // Arrange
        var notificationId = "test-notification-id";

        // Act
        var result = await _service.GetDeliveryHistoryAsync(notificationId);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RetryAsync_WithFailedNotification_ShouldRetry()
    {
        // Arrange
        var notificationId = "test-notification-id";

        // Act
        var result = await _service.RetryAsync(notificationId);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task AcknowledgeAsync_WithValidNotification_ShouldAcknowledge()
    {
        // Arrange
        var notificationId = "test-notification-id";
        var acknowledgedBy = "user-1";

        // Act
        var result = await _service.AcknowledgeAsync(notificationId, acknowledgedBy);

        // Assert
        result.Should().BeTrue();
    }

    private static NotificationEvent CreateTestNotification(string? id = null)
    {
        return new NotificationEvent
        {
            Id = id ?? Guid.NewGuid().ToString(),
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