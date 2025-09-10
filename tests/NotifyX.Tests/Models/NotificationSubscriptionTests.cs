using NotifyX.Core.Models;
using FluentAssertions;
using Xunit;

namespace NotifyX.Tests.Models;

/// <summary>
/// Unit tests for the NotificationSubscription model.
/// </summary>
public class NotificationSubscriptionTests
{
    [Fact]
    public void NotificationSubscription_WithDefaultValues_ShouldHaveCorrectDefaults()
    {
        // Act
        var subscription = new NotificationSubscription();

        // Assert
        subscription.Id.Should().NotBeNullOrEmpty();
        subscription.TenantId.Should().BeEmpty();
        subscription.RecipientId.Should().BeEmpty();
        subscription.EventType.Should().BeEmpty();
        subscription.Channel.Should().Be(NotificationChannel.Email);
        subscription.IsActive.Should().BeTrue();
        subscription.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        subscription.ExpiresAt.Should().BeNull();
        subscription.Metadata.Should().NotBeNull();
        subscription.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void NotificationSubscription_WithCustomValues_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid().ToString();
        var tenantId = "test-tenant";
        var recipientId = "user-123";
        var eventType = "user.login";
        var channel = NotificationChannel.SMS;
        var isActive = false;
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var expiresAt = DateTime.UtcNow.AddDays(30);
        var metadata = new Dictionary<string, object>
        {
            ["source"] = "api",
            ["priority"] = "high"
        };

        // Act
        var subscription = new NotificationSubscription
        {
            Id = id,
            TenantId = tenantId,
            RecipientId = recipientId,
            EventType = eventType,
            Channel = channel,
            IsActive = isActive,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt,
            Metadata = metadata
        };

        // Assert
        subscription.Id.Should().Be(id);
        subscription.TenantId.Should().Be(tenantId);
        subscription.RecipientId.Should().Be(recipientId);
        subscription.EventType.Should().Be(eventType);
        subscription.Channel.Should().Be(channel);
        subscription.IsActive.Should().Be(isActive);
        subscription.CreatedAt.Should().Be(createdAt);
        subscription.ExpiresAt.Should().Be(expiresAt);
        subscription.Metadata.Should().BeEquivalentTo(metadata);
    }

    [Fact]
    public void NotificationSubscription_WithBuilder_ShouldCreateCorrectly()
    {
        // Arrange
        var tenantId = "test-tenant";
        var recipientId = "user-123";
        var eventType = "notification.sent";
        var channel = NotificationChannel.Push;
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var subscription = new NotificationSubscription
        {
            TenantId = tenantId,
            RecipientId = recipientId,
            EventType = eventType,
            Channel = channel,
            ExpiresAt = expiresAt
        };

        // Assert
        subscription.TenantId.Should().Be(tenantId);
        subscription.RecipientId.Should().Be(recipientId);
        subscription.EventType.Should().Be(eventType);
        subscription.Channel.Should().Be(channel);
        subscription.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void NotificationSubscription_WithMethod_ShouldCreateModifiedInstance()
    {
        // Arrange
        var originalSubscription = new NotificationSubscription
        {
            TenantId = "original-tenant",
            RecipientId = "original-user",
            EventType = "original.event",
            Channel = NotificationChannel.Email,
            IsActive = true
        };

        // Act
        var modifiedSubscription = originalSubscription.With(builder =>
        {
            builder.WithTenantId("new-tenant")
                   .WithRecipientId("new-user")
                   .WithEventType("new.event")
                   .WithChannel(NotificationChannel.SMS)
                   .WithActiveStatus(false)
                   .WithExpiresAt(DateTime.UtcNow.AddDays(30))
                   .WithMetadata("source", "api")
                   .WithMetadata("priority", "high");
        });

        // Assert
        modifiedSubscription.TenantId.Should().Be("new-tenant");
        modifiedSubscription.RecipientId.Should().Be("new-user");
        modifiedSubscription.EventType.Should().Be("new.event");
        modifiedSubscription.Channel.Should().Be(NotificationChannel.SMS);
        modifiedSubscription.IsActive.Should().BeFalse();
        modifiedSubscription.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
        modifiedSubscription.Metadata.Should().ContainKey("source");
        modifiedSubscription.Metadata.Should().ContainKey("priority");
        modifiedSubscription.Metadata["source"].Should().Be("api");
        modifiedSubscription.Metadata["priority"].Should().Be("high");

        // Original should remain unchanged
        originalSubscription.TenantId.Should().Be("original-tenant");
        originalSubscription.RecipientId.Should().Be("original-user");
        originalSubscription.EventType.Should().Be("original.event");
        originalSubscription.Channel.Should().Be(NotificationChannel.Email);
        originalSubscription.IsActive.Should().BeTrue();
    }

    [Fact]
    public void NotificationSubscription_WithMetadata_ShouldPreserveExistingMetadata()
    {
        // Arrange
        var subscription = new NotificationSubscription
        {
            Metadata = new Dictionary<string, object>
            {
                ["existing"] = "value",
                ["count"] = 42
            }
        };

        // Act
        var modifiedSubscription = subscription.With(builder =>
        {
            builder.WithMetadata("new", "value");
        });

        // Assert
        modifiedSubscription.Metadata.Should().ContainKey("existing");
        modifiedSubscription.Metadata.Should().ContainKey("count");
        modifiedSubscription.Metadata.Should().ContainKey("new");
        modifiedSubscription.Metadata["existing"].Should().Be("value");
        modifiedSubscription.Metadata["count"].Should().Be(42);
        modifiedSubscription.Metadata["new"].Should().Be("value");
    }

    [Fact]
    public void NotificationSubscription_WithMetadata_ShouldOverwriteExistingKey()
    {
        // Arrange
        var subscription = new NotificationSubscription
        {
            Metadata = new Dictionary<string, object>
            {
                ["key"] = "original"
            }
        };

        // Act
        var modifiedSubscription = subscription.With(builder =>
        {
            builder.WithMetadata("key", "updated");
        });

        // Assert
        modifiedSubscription.Metadata.Should().ContainKey("key");
        modifiedSubscription.Metadata["key"].Should().Be("updated");
    }

    [Theory]
    [InlineData(NotificationChannel.Email)]
    [InlineData(NotificationChannel.SMS)]
    [InlineData(NotificationChannel.Push)]
    [InlineData(NotificationChannel.Webhook)]
    [InlineData(NotificationChannel.Slack)]
    [InlineData(NotificationChannel.Teams)]
    public void NotificationSubscription_WithDifferentChannels_ShouldSetCorrectly(NotificationChannel channel)
    {
        // Act
        var subscription = new NotificationSubscription
        {
            Channel = channel
        };

        // Assert
        subscription.Channel.Should().Be(channel);
    }

    [Fact]
    public void NotificationSubscription_WithExpiresAt_ShouldSetCorrectly()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        var subscription = new NotificationSubscription
        {
            ExpiresAt = expiresAt
        };

        // Assert
        subscription.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void NotificationSubscription_WithNullExpiresAt_ShouldAllowNull()
    {
        // Act
        var subscription = new NotificationSubscription
        {
            ExpiresAt = null
        };

        // Assert
        subscription.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public void NotificationSubscription_WithPastExpiresAt_ShouldAllowPastDate()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var subscription = new NotificationSubscription
        {
            ExpiresAt = pastDate
        };

        // Assert
        subscription.ExpiresAt.Should().Be(pastDate);
    }

    [Fact]
    public void NotificationSubscription_WithFutureExpiresAt_ShouldAllowFutureDate()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(365);

        // Act
        var subscription = new NotificationSubscription
        {
            ExpiresAt = futureDate
        };

        // Assert
        subscription.ExpiresAt.Should().Be(futureDate);
    }

    [Fact]
    public void NotificationSubscription_WithComplexMetadata_ShouldHandleComplexTypes()
    {
        // Arrange
        var metadata = new Dictionary<string, object>
        {
            ["string"] = "value",
            ["number"] = 42,
            ["boolean"] = true,
            ["array"] = new[] { "item1", "item2" },
            ["object"] = new Dictionary<string, object>
            {
                ["nested"] = "value"
            }
        };

        // Act
        var subscription = new NotificationSubscription
        {
            Metadata = metadata
        };

        // Assert
        subscription.Metadata.Should().BeEquivalentTo(metadata);
        subscription.Metadata["string"].Should().Be("value");
        subscription.Metadata["number"].Should().Be(42);
        subscription.Metadata["boolean"].Should().Be(true);
        subscription.Metadata["array"].Should().BeEquivalentTo(new[] { "item1", "item2" });
        subscription.Metadata["object"].Should().BeOfType<Dictionary<string, object>>();
    }

    [Fact]
    public void NotificationSubscription_WithEmptyStringValues_ShouldAllowEmptyStrings()
    {
        // Act
        var subscription = new NotificationSubscription
        {
            TenantId = "",
            RecipientId = "",
            EventType = ""
        };

        // Assert
        subscription.TenantId.Should().BeEmpty();
        subscription.RecipientId.Should().BeEmpty();
        subscription.EventType.Should().BeEmpty();
    }

    [Fact]
    public void NotificationSubscription_WithNullStringValues_ShouldAllowNullStrings()
    {
        // Act
        var subscription = new NotificationSubscription
        {
            TenantId = null!,
            RecipientId = null!,
            EventType = null!
        };

        // Assert
        subscription.TenantId.Should().BeNull();
        subscription.RecipientId.Should().BeNull();
        subscription.EventType.Should().BeNull();
    }

    [Fact]
    public void NotificationSubscription_WithBuilder_ShouldCreateNewBuilderInstance()
    {
        // Arrange
        var subscription = new NotificationSubscription
        {
            TenantId = "test-tenant",
            RecipientId = "test-user"
        };

        // Act
        var builder1 = new NotificationSubscriptionBuilder(subscription);
        var builder2 = builder1.WithTenantId("new-tenant");

        // Assert
        builder1.Should().NotBeSameAs(builder2);
        builder2.Build().TenantId.Should().Be("new-tenant");
    }

    [Fact]
    public void NotificationSubscription_WithBuilder_ShouldPreserveOriginalWhenBuilding()
    {
        // Arrange
        var originalSubscription = new NotificationSubscription
        {
            TenantId = "original-tenant",
            RecipientId = "original-user",
            EventType = "original.event"
        };

        // Act
        var builder = new NotificationSubscriptionBuilder(originalSubscription);
        var newSubscription = builder.WithTenantId("new-tenant").Build();

        // Assert
        originalSubscription.TenantId.Should().Be("original-tenant");
        newSubscription.TenantId.Should().Be("new-tenant");
        newSubscription.RecipientId.Should().Be("original-user");
        newSubscription.EventType.Should().Be("original.event");
    }
}