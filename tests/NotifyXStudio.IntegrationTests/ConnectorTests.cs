using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NotifyXStudio.Core.Connectors;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Connectors.NotifyX;
using Xunit;

namespace NotifyXStudio.IntegrationTests
{
    /// <summary>
    /// Integration tests for connector adapters.
    /// </summary>
    public class ConnectorTests : BaseIntegrationTest
    {
        private readonly IConnectorFactory _connectorFactory;

        public ConnectorTests()
        {
            _connectorFactory = GetService<IConnectorFactory>();
        }

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            base.ConfigureTestServices(services);
            
            // Register test implementations
            services.AddScoped<INotifyXSdk, TestNotifyXSdk>();
            services.AddScoped<INotifyXEventService, TestNotifyXEventService>();
        }

        [Fact]
        public async Task NotifyXSendNotificationAdapter_WithValidConfig_ShouldSucceed()
        {
            // Arrange
            var adapter = _connectorFactory.Create("notifyx.sendNotification");
            adapter.Should().NotBeNull();

            var context = new ConnectorExecutionContext
            {
                TenantId = "test-tenant",
                NodeConfig = JsonSerializer.SerializeToElement(new
                {
                    channel = "email",
                    recipient = "test@example.com",
                    message = "Hello from NotifyX Studio!",
                    priority = "normal"
                }),
                Inputs = JsonSerializer.SerializeToElement(new
                {
                    orderId = "12345",
                    customerName = "John Doe"
                }),
                RunMetadata = new RunMetadata
                {
                    RunId = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    NodeId = "test-node"
                }
            };

            // Act
            var result = await adapter!.ExecuteAsync(context);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Output.Should().NotBeNull();
            result.DurationMs.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task NotifyXSendNotificationAdapter_WithInvalidConfig_ShouldFail()
        {
            // Arrange
            var adapter = _connectorFactory.Create("notifyx.sendNotification");
            adapter.Should().NotBeNull();

            var context = new ConnectorExecutionContext
            {
                TenantId = "test-tenant",
                NodeConfig = JsonSerializer.SerializeToElement(new
                {
                    // Missing required fields
                    channel = "email"
                }),
                Inputs = JsonSerializer.SerializeToElement(new { }),
                RunMetadata = new RunMetadata
                {
                    RunId = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    NodeId = "test-node"
                }
            };

            // Act
            var result = await adapter!.ExecuteAsync(context);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task NotifyXOnDeliveryStatusAdapter_WithValidConfig_ShouldSucceed()
        {
            // Arrange
            var adapter = _connectorFactory.Create("notifyx.onDeliveryStatus");
            adapter.Should().NotBeNull();

            var context = new ConnectorExecutionContext
            {
                TenantId = "test-tenant",
                NodeConfig = JsonSerializer.SerializeToElement(new
                {
                    statuses = new[] { "success", "failed" },
                    timeRange = 3600,
                    maxEvents = 100
                }),
                Inputs = JsonSerializer.SerializeToElement(new { }),
                RunMetadata = new RunMetadata
                {
                    RunId = Guid.NewGuid().ToString(),
                    WorkflowId = Guid.NewGuid().ToString(),
                    NodeId = "test-node"
                }
            };

            // Act
            var result = await adapter!.ExecuteAsync(context);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Output.Should().NotBeNull();
            result.DurationMs.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ConnectorFactory_WithRegisteredAdapters_ShouldCreateAdapters()
        {
            // Act & Assert
            var sendNotificationAdapter = _connectorFactory.Create("notifyx.sendNotification");
            sendNotificationAdapter.Should().NotBeNull();
            sendNotificationAdapter!.Type.Should().Be("notifyx.sendNotification");

            var deliveryStatusAdapter = _connectorFactory.Create("notifyx.onDeliveryStatus");
            deliveryStatusAdapter.Should().NotBeNull();
            deliveryStatusAdapter!.Type.Should().Be("notifyx.onDeliveryStatus");
        }

        [Fact]
        public void ConnectorFactory_WithUnregisteredType_ShouldReturnNull()
        {
            // Act
            var adapter = _connectorFactory.Create("nonexistent.connector");

            // Assert
            adapter.Should().BeNull();
        }

        [Fact]
        public void ConnectorFactory_GetAvailableTypes_ShouldReturnRegisteredTypes()
        {
            // Act
            var types = _connectorFactory.GetAvailableTypes();

            // Assert
            types.Should().NotBeEmpty();
            types.Should().Contain("notifyx.sendNotification");
            types.Should().Contain("notifyx.onDeliveryStatus");
        }

        [Fact]
        public void ConnectorFactory_IsAvailable_ShouldReturnCorrectStatus()
        {
            // Act & Assert
            _connectorFactory.IsAvailable("notifyx.sendNotification").Should().BeTrue();
            _connectorFactory.IsAvailable("notifyx.onDeliveryStatus").Should().BeTrue();
            _connectorFactory.IsAvailable("nonexistent.connector").Should().BeFalse();
        }
    }

    /// <summary>
    /// Test implementation of NotifyX SDK.
    /// </summary>
    public class TestNotifyXSdk : INotifyXSdk
    {
        public async Task<NotifyXNotificationResponse> SendNotificationAsync(NotifyXNotificationRequest request, CancellationToken cancellationToken = default)
        {
            // Simulate API call delay
            await Task.Delay(100, cancellationToken);

            return new NotifyXNotificationResponse
            {
                NotificationId = Guid.NewGuid().ToString(),
                DeliveryStatus = "queued",
                Channels = new List<string> { request.Channel },
                SentAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Test implementation of NotifyX Event Service.
    /// </summary>
    public class TestNotifyXEventService : INotifyXEventService
    {
        public async Task<List<DeliveryStatusEvent>> GetDeliveryStatusEventsAsync(NotifyXDeliveryStatusConfig config, string runId, CancellationToken cancellationToken = default)
        {
            // Simulate API call delay
            await Task.Delay(50, cancellationToken);

            // Return mock events
            return new List<DeliveryStatusEvent>
            {
                new DeliveryStatusEvent
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    Status = "success",
                    Channel = "email",
                    Recipient = "test@example.com",
                    Timestamp = DateTime.UtcNow
                },
                new DeliveryStatusEvent
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    Status = "failed",
                    Channel = "sms",
                    Recipient = "+1234567890",
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = "Invalid phone number"
                }
            };
        }
    }
}