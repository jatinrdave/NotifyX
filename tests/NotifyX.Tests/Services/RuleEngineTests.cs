using Microsoft.Extensions.Logging;
using Moq;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using NotifyX.Core.Services;
using FluentAssertions;
using Xunit;

namespace NotifyX.Tests.Services;

/// <summary>
/// Unit tests for the RuleEngine class.
/// </summary>
public class RuleEngineTests
{
    private readonly Mock<ILogger<RuleEngine>> _mockLogger;
    private readonly RuleEngine _ruleEngine;

    public RuleEngineTests()
    {
        _mockLogger = new Mock<ILogger<RuleEngine>>();
        _ruleEngine = new RuleEngine(_mockLogger.Object);
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithMatchingRule_ShouldReturnMatchedRule()
    {
        // Arrange
        var notification = CreateTestNotification();
        var rule = CreateTestRule();

        await _ruleEngine.AddRuleAsync(rule);

        // Act
        var result = await _ruleEngine.EvaluateRulesAsync(notification);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.MatchedRules.Should().HaveCount(1);
        result.MatchedRules.First().Id.Should().Be(rule.Id);
    }

    [Fact]
    public async Task EvaluateRulesAsync_WithNonMatchingRule_ShouldReturnUnmatchedRule()
    {
        // Arrange
        var notification = CreateTestNotification();
        var rule = CreateTestRule();
        rule = rule.With(builder => builder.WithCondition(new RuleCondition
        {
            Type = ConditionType.EventType,
            Operator = ConditionOperator.Equals,
            FieldPath = "EventType",
            ExpectedValues = new List<object> { "different.event" }
        }));

        await _ruleEngine.AddRuleAsync(rule);

        // Act
        var result = await _ruleEngine.EvaluateRulesAsync(notification);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.MatchedRules.Should().BeEmpty();
        result.UnmatchedRules.Should().HaveCount(1);
        result.UnmatchedRules.First().Id.Should().Be(rule.Id);
    }

    [Fact]
    public async Task ProcessWorkflowAsync_WithValidActions_ShouldExecuteActions()
    {
        // Arrange
        var notification = CreateTestNotification();
        var rule = CreateTestRule();
        rule = rule.With(builder => builder.WithAction(new RuleAction
        {
            Type = ActionType.SetPriority,
            Parameters = new Dictionary<string, object>
            {
                ["priority"] = NotificationPriority.High.ToString()
            }
        }));

        // Act
        var result = await _ruleEngine.ProcessWorkflowAsync(notification, new[] { rule });

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.ExecutedActions.Should().HaveCount(1);
    }

    [Fact]
    public async Task CheckEscalationAsync_WithFailedDeliveries_ShouldReturnEscalationNeeded()
    {
        // Arrange
        var notification = CreateTestNotification();
        notification = notification.With(builder => builder.WithDeliveryOptions(new DeliveryOptions
        {
            EnableEscalation = true,
            EscalationDelay = TimeSpan.Zero,
            EscalationChannels = new List<NotificationChannel> { NotificationChannel.SMS }
        }));

        var failedAttempts = new List<DeliveryAttempt>
        {
            new DeliveryAttempt
            {
                NotificationId = notification.Id,
                RecipientId = "test-recipient",
                Channel = NotificationChannel.Email,
                IsSuccess = false,
                ErrorMessage = "Delivery failed",
                AttemptedAt = DateTime.UtcNow.AddMinutes(-10)
            }
        };

        // Act
        var result = await _ruleEngine.CheckEscalationAsync(notification, failedAttempts);

        // Assert
        result.Should().NotBeNull();
        result.IsEscalationNeeded.Should().BeTrue();
        result.EscalationActions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddRuleAsync_WithValidRule_ShouldAddSuccessfully()
    {
        // Arrange
        var rule = CreateTestRule();

        // Act
        var result = await _ruleEngine.AddRuleAsync(rule);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateRuleAsync_WithExistingRule_ShouldUpdateSuccessfully()
    {
        // Arrange
        var rule = CreateTestRule();
        await _ruleEngine.AddRuleAsync(rule);

        var updatedRule = rule.With(builder => builder.WithName("Updated Rule"));

        // Act
        var result = await _ruleEngine.UpdateRuleAsync(updatedRule);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateRuleAsync_WithNonExistingRule_ShouldReturnFalse()
    {
        // Arrange
        var rule = CreateTestRule();

        // Act
        var result = await _ruleEngine.UpdateRuleAsync(rule);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveRuleAsync_WithExistingRule_ShouldRemoveSuccessfully()
    {
        // Arrange
        var rule = CreateTestRule();
        await _ruleEngine.AddRuleAsync(rule);

        // Act
        var result = await _ruleEngine.RemoveRuleAsync(rule.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveRuleAsync_WithNonExistingRule_ShouldReturnFalse()
    {
        // Arrange
        var ruleId = "non-existing-rule";

        // Act
        var result = await _ruleEngine.RemoveRuleAsync(ruleId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetRulesAsync_WithExistingRules_ShouldReturnRules()
    {
        // Arrange
        var rule1 = CreateTestRule();
        var rule2 = CreateTestRule();
        await _ruleEngine.AddRuleAsync(rule1);
        await _ruleEngine.AddRuleAsync(rule2);

        // Act
        var result = await _ruleEngine.GetRulesAsync("test-tenant");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRuleAsync_WithExistingRule_ShouldReturnRule()
    {
        // Arrange
        var rule = CreateTestRule();
        await _ruleEngine.AddRuleAsync(rule);

        // Act
        var result = await _ruleEngine.GetRuleAsync(rule.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(rule.Id);
    }

    [Fact]
    public async Task GetRuleAsync_WithNonExistingRule_ShouldReturnNull()
    {
        // Arrange
        var ruleId = "non-existing-rule";

        // Act
        var result = await _ruleEngine.GetRuleAsync(ruleId);

        // Assert
        result.Should().BeNull();
    }

    private static NotificationEvent CreateTestNotification()
    {
        return new NotificationEvent
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            EventType = "test.event",
            Priority = NotificationPriority.Normal,
            Subject = "Test Notification",
            Content = "This is a test notification"
        };
    }

    private static NotificationRule CreateTestRule()
    {
        return new NotificationRule
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = "test-tenant",
            Name = "Test Rule",
            Description = "A test rule",
            IsActive = true,
            Priority = 1,
            Condition = new RuleCondition
            {
                Type = ConditionType.EventType,
                Operator = ConditionOperator.Equals,
                FieldPath = "EventType",
                ExpectedValues = new List<object> { "test.event" }
            },
            Actions = new List<RuleAction>()
        };
    }
}