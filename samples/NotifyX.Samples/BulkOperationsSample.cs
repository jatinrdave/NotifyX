using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Samples;

/// <summary>
/// Sample application demonstrating bulk operations functionality.
/// </summary>
public class BulkOperationsSample
{
    private readonly ILogger<BulkOperationsSample> _logger;
    private readonly IBulkOperationsService _bulkOperationsService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the BulkOperationsSample class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="bulkOperationsService">The bulk operations service.</param>
    /// <param name="notificationService">The notification service.</param>
    public BulkOperationsSample(
        ILogger<BulkOperationsSample> logger,
        IBulkOperationsService bulkOperationsService,
        INotificationService notificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bulkOperationsService = bulkOperationsService ?? throw new ArgumentNullException(nameof(bulkOperationsService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    /// Runs the bulk operations sample.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting bulk operations sample...");

        try
        {
            // Sample 1: Bulk Rule Creation
            await DemonstrateBulkRuleCreationAsync(cancellationToken);

            // Sample 2: Bulk Subscription Management
            await DemonstrateBulkSubscriptionManagementAsync(cancellationToken);

            // Sample 3: Bulk Event Ingestion
            await DemonstrateBulkEventIngestionAsync(cancellationToken);

            // Sample 4: Import/Export Operations
            await DemonstrateImportExportOperationsAsync(cancellationToken);

            _logger.LogInformation("Bulk operations sample completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running bulk operations sample");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates bulk rule creation.
    /// </summary>
    private async Task DemonstrateBulkRuleCreationAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating Bulk Rule Creation ===");

        // Create sample rules
        var rules = new List<NotificationRule>
        {
            new NotificationRule
            {
                TenantId = "tenant1",
                Name = "High Priority Email Rule",
                Description = "Send email for high priority notifications",
                Condition = new RuleCondition
                {
                    Type = ConditionType.Priority,
                    Operator = ConditionOperator.Equals,
                    FieldPath = "Priority",
                    ExpectedValues = new List<object> { NotificationPriority.High }
                },
                Actions = new List<RuleAction>
                {
                    new RuleAction
                    {
                        Type = ActionType.SendNotification,
                        Parameters = new Dictionary<string, object>
                        {
                            ["Channel"] = NotificationChannel.Email.ToString(),
                            ["Template"] = "high-priority-email"
                        }
                    }
                },
                CreatedBy = "admin",
                UpdatedBy = "admin"
            },
            new NotificationRule
            {
                TenantId = "tenant1",
                Name = "Critical SMS Rule",
                Description = "Send SMS for critical notifications",
                Condition = new RuleCondition
                {
                    Type = ConditionType.Priority,
                    Operator = ConditionOperator.Equals,
                    FieldPath = "Priority",
                    ExpectedValues = new List<object> { NotificationPriority.Critical }
                },
                Actions = new List<RuleAction>
                {
                    new RuleAction
                    {
                        Type = ActionType.SendNotification,
                        Parameters = new Dictionary<string, object>
                        {
                            ["Channel"] = NotificationChannel.SMS.ToString(),
                            ["Template"] = "critical-sms"
                        }
                    }
                },
                CreatedBy = "admin",
                UpdatedBy = "admin"
            },
            new NotificationRule
            {
                TenantId = "tenant2",
                Name = "Order Status Rule",
                Description = "Handle order status notifications",
                Condition = new RuleCondition
                {
                    Type = ConditionType.EventType,
                    Operator = ConditionOperator.StartsWith,
                    FieldPath = "EventType",
                    ExpectedValues = new List<object> { "order." }
                },
                Actions = new List<RuleAction>
                {
                    new RuleAction
                    {
                        Type = ActionType.SendNotification,
                        Parameters = new Dictionary<string, object>
                        {
                            ["Channel"] = NotificationChannel.Email.ToString(),
                            ["Template"] = "order-status"
                        }
                    }
                },
                CreatedBy = "admin",
                UpdatedBy = "admin"
            }
        };

        // Create rules in bulk
        var result = await _bulkOperationsService.CreateRulesBulkAsync(rules, cancellationToken);

        _logger.LogInformation("Bulk rule creation result: {TotalCount} total, {SuccessCount} successful, {FailureCount} failed",
            result.TotalCount, result.SuccessCount, result.FailureCount);

        // Log individual results
        foreach (var ruleResult in result.Results)
        {
            if (ruleResult.IsSuccess)
            {
                _logger.LogInformation("✓ Rule '{RuleName}' created successfully", ruleResult.Rule?.Name);
            }
            else
            {
                _logger.LogWarning("✗ Rule '{RuleId}' failed: {Error}", ruleResult.RuleId, ruleResult.ErrorMessage);
            }
        }
    }

    /// <summary>
    /// Demonstrates bulk subscription management.
    /// </summary>
    private async Task DemonstrateBulkSubscriptionManagementAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating Bulk Subscription Management ===");

        // Create sample subscriptions
        var subscriptions = new List<NotificationSubscription>
        {
            new NotificationSubscription
            {
                TenantId = "tenant1",
                Recipient = new NotificationRecipient
                {
                    Id = "user1",
                    Name = "John Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "+1234567890",
                    Language = "en",
                    TimeZone = "America/New_York"
                },
                EventTypes = new List<string> { "order.shipped", "order.delivered", "payment.completed" },
                Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.SMS },
                PriorityLevels = new List<NotificationPriority> { NotificationPriority.High, NotificationPriority.Critical },
                DeliveryPreferences = new SubscriptionDeliveryPreferences
                {
                    PreferredTimeWindow = new DeliveryTimeWindow
                    {
                        StartTime = TimeSpan.FromHours(9),
                        EndTime = TimeSpan.FromHours(17),
                        AllowedDays = new List<DayOfWeek>
                        {
                            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                            DayOfWeek.Thursday, DayOfWeek.Friday
                        },
                        TimeZone = "America/New_York",
                        IsEnabled = true
                    },
                    QuietHours = new List<QuietHour>
                    {
                        new QuietHour
                        {
                            StartTime = TimeSpan.FromHours(22),
                            EndTime = TimeSpan.FromHours(8),
                            Days = new List<DayOfWeek>
                            {
                                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
                            },
                            TimeZone = "America/New_York",
                            IsEnabled = true,
                            ExemptPriorities = new List<NotificationPriority> { NotificationPriority.Critical }
                        }
                    },
                    MaxNotificationsPerDay = 50,
                    MaxNotificationsPerHour = 10,
                    EnableDigestMode = false,
                    EnableEscalation = true,
                    EscalationDelay = TimeSpan.FromMinutes(30),
                    EscalationChannels = new List<NotificationChannel> { NotificationChannel.SMS },
                    EnableRateLimiting = true,
                    RateLimitPerMinute = 5,
                    RateLimitPerHour = 50
                },
                CreatedBy = "admin",
                UpdatedBy = "admin"
            },
            new NotificationSubscription
            {
                TenantId = "tenant1",
                Recipient = new NotificationRecipient
                {
                    Id = "user2",
                    Name = "Jane Smith",
                    Email = "jane.smith@example.com",
                    PhoneNumber = "+1987654321",
                    Language = "en",
                    TimeZone = "America/Los_Angeles"
                },
                EventTypes = new List<string> { "system.alert", "security.breach" },
                Channels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.Push },
                PriorityLevels = new List<NotificationPriority> { NotificationPriority.Critical },
                DeliveryPreferences = new SubscriptionDeliveryPreferences
                {
                    PreferredTimeWindow = new DeliveryTimeWindow
                    {
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(18),
                        AllowedDays = new List<DayOfWeek>
                        {
                            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                            DayOfWeek.Thursday, DayOfWeek.Friday
                        },
                        TimeZone = "America/Los_Angeles",
                        IsEnabled = true
                    },
                    MaxNotificationsPerDay = 100,
                    MaxNotificationsPerHour = 20,
                    EnableDigestMode = true,
                    DigestFrequency = DigestFrequency.Hourly,
                    EnableEscalation = true,
                    EscalationDelay = TimeSpan.FromMinutes(15),
                    EscalationChannels = new List<NotificationChannel> { NotificationChannel.SMS, NotificationChannel.Push },
                    EnableRateLimiting = true,
                    RateLimitPerMinute = 10,
                    RateLimitPerHour = 100
                },
                CreatedBy = "admin",
                UpdatedBy = "admin"
            }
        };

        // Create subscriptions in bulk
        var result = await _bulkOperationsService.CreateSubscriptionsBulkAsync(subscriptions, cancellationToken);

        _logger.LogInformation("Bulk subscription creation result: {TotalCount} total, {SuccessCount} successful, {FailureCount} failed",
            result.TotalCount, result.SuccessCount, result.FailureCount);

        // Log individual results
        foreach (var subscriptionResult in result.Results)
        {
            if (subscriptionResult.IsSuccess)
            {
                _logger.LogInformation("✓ Subscription for '{RecipientName}' created successfully", 
                    subscriptionResult.Subscription?.Recipient.Name);
            }
            else
            {
                _logger.LogWarning("✗ Subscription '{SubscriptionId}' failed: {Error}", 
                    subscriptionResult.SubscriptionId, subscriptionResult.ErrorMessage);
            }
        }
    }

    /// <summary>
    /// Demonstrates bulk event ingestion.
    /// </summary>
    private async Task DemonstrateBulkEventIngestionAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating Bulk Event Ingestion ===");

        // Create sample events
        var events = new List<NotificationEvent>
        {
            new NotificationEvent
            {
                TenantId = "tenant1",
                EventType = "order.shipped",
                Priority = NotificationPriority.High,
                Subject = "Your order has been shipped!",
                Content = "Your order #12345 has been shipped and is on its way to you.",
                Recipients = new List<NotificationRecipient>
                {
                    new NotificationRecipient
                    {
                        Id = "user1",
                        Name = "John Doe",
                        Email = "john.doe@example.com"
                    }
                },
                PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email },
                DeliveryOptions = new DeliveryOptions
                {
                    Guarantee = DeliveryGuarantee.AtLeastOnce,
                    MaxAttempts = 3,
                    RetryOnFailure = true,
                    RetryStrategy = RetryStrategy.ExponentialBackoff,
                    InitialRetryDelay = TimeSpan.FromSeconds(5),
                    MaxRetryDelay = TimeSpan.FromMinutes(5),
                    EnableAggregation = false,
                    EnableEscalation = true,
                    EscalationDelay = TimeSpan.FromMinutes(15),
                    EscalationChannels = new List<NotificationChannel> { NotificationChannel.SMS }
                },
                Metadata = new Dictionary<string, object>
                {
                    ["OrderId"] = "12345",
                    ["TrackingNumber"] = "TRK123456789",
                    ["EstimatedDelivery"] = "2024-01-15"
                },
                Tags = new List<string> { "order", "shipping", "customer" }
            },
            new NotificationEvent
            {
                TenantId = "tenant1",
                EventType = "payment.completed",
                Priority = NotificationPriority.Normal,
                Subject = "Payment received",
                Content = "We have received your payment of $99.99 for order #12345.",
                Recipients = new List<NotificationRecipient>
                {
                    new NotificationRecipient
                    {
                        Id = "user1",
                        Name = "John Doe",
                        Email = "john.doe@example.com"
                    }
                },
                PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email },
                DeliveryOptions = new DeliveryOptions
                {
                    Guarantee = DeliveryGuarantee.AtLeastOnce,
                    MaxAttempts = 2,
                    RetryOnFailure = true,
                    RetryStrategy = RetryStrategy.FixedDelay,
                    InitialRetryDelay = TimeSpan.FromSeconds(10),
                    EnableAggregation = false
                },
                Metadata = new Dictionary<string, object>
                {
                    ["OrderId"] = "12345",
                    ["Amount"] = 99.99,
                    ["PaymentMethod"] = "Credit Card",
                    ["TransactionId"] = "TXN987654321"
                },
                Tags = new List<string> { "payment", "order", "customer" }
            },
            new NotificationEvent
            {
                TenantId = "tenant2",
                EventType = "system.alert",
                Priority = NotificationPriority.Critical,
                Subject = "System Alert: High CPU Usage",
                Content = "Server CPU usage has exceeded 90% for the last 5 minutes.",
                Recipients = new List<NotificationRecipient>
                {
                    new NotificationRecipient
                    {
                        Id = "user2",
                        Name = "Jane Smith",
                        Email = "jane.smith@example.com",
                        PhoneNumber = "+1987654321"
                    }
                },
                PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email, NotificationChannel.SMS },
                DeliveryOptions = new DeliveryOptions
                {
                    Guarantee = DeliveryGuarantee.AtLeastOnce,
                    MaxAttempts = 5,
                    RetryOnFailure = true,
                    RetryStrategy = RetryStrategy.ExponentialBackoff,
                    InitialRetryDelay = TimeSpan.FromSeconds(2),
                    MaxRetryDelay = TimeSpan.FromMinutes(2),
                    EnableAggregation = false,
                    EnableEscalation = true,
                    EscalationDelay = TimeSpan.FromMinutes(5),
                    EscalationChannels = new List<NotificationChannel> { NotificationChannel.SMS, NotificationChannel.Push }
                },
                Metadata = new Dictionary<string, object>
                {
                    ["ServerId"] = "web-server-01",
                    ["CpuUsage"] = 95.5,
                    ["MemoryUsage"] = 78.2,
                    ["Timestamp"] = DateTime.UtcNow
                },
                Tags = new List<string> { "system", "alert", "monitoring", "critical" }
            }
        };

        // Ingest events in bulk
        var result = await _bulkOperationsService.IngestEventsBulkAsync(events, cancellationToken);

        _logger.LogInformation("Bulk event ingestion result: {TotalCount} total, {SuccessCount} successful, {FailureCount} failed",
            result.TotalCount, result.SuccessCount, result.FailureCount);

        // Log individual results
        foreach (var eventResult in result.Results)
        {
            if (eventResult.IsSuccess)
            {
                _logger.LogInformation("✓ Event '{EventType}' for tenant '{TenantId}' processed successfully", 
                    eventResult.Event?.EventType, eventResult.Event?.TenantId);
            }
            else
            {
                _logger.LogWarning("✗ Event '{EventId}' failed: {Error}", 
                    eventResult.EventId, eventResult.ErrorMessage);
            }
        }
    }

    /// <summary>
    /// Demonstrates import/export operations.
    /// </summary>
    private async Task DemonstrateImportExportOperationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== Demonstrating Import/Export Operations ===");

        // Export rules to JSON
        var exportResult = await _bulkOperationsService.ExportRulesAsync(null, BulkImportFormat.Json, cancellationToken);
        _logger.LogInformation("Exported {Count} rules to JSON format", exportResult.ExportedCount);

        // Export subscriptions to CSV
        var subscriptionExportResult = await _bulkOperationsService.ExportSubscriptionsAsync(null, BulkImportFormat.Csv, cancellationToken);
        _logger.LogInformation("Exported {Count} subscriptions to CSV format", subscriptionExportResult.ExportedCount);

        // Sample JSON content for import
        var sampleJsonContent = """
        [
          {
            "tenantId": "tenant3",
            "name": "Imported Rule 1",
            "description": "This rule was imported from JSON",
            "condition": {
              "type": "EventType",
              "operator": "Equals",
              "fieldPath": "EventType",
              "expectedValues": ["test.event"]
            },
            "actions": [
              {
                "type": "SendNotification",
                "parameters": {
                  "Channel": "Email",
                  "Template": "test-template"
                }
              }
            ],
            "createdBy": "import-process",
            "updatedBy": "import-process"
          }
        ]
        """;

        // Import rules from JSON
        var importResult = await _bulkOperationsService.ImportRulesAsync(sampleJsonContent, BulkImportFormat.Json, cancellationToken);
        _logger.LogInformation("Imported rules result: {TotalCount} total, {SuccessCount} successful, {FailureCount} failed",
            importResult.TotalCount, importResult.SuccessCount, importResult.FailureCount);

        // Sample CSV content for subscription import
        var sampleCsvContent = """
        TenantId,RecipientId,RecipientName,RecipientEmail,EventTypes,Channels,IsActive,IsEnabled
        tenant3,user3,Test User,test.user@example.com,test.event;test.alert,Email;SMS,True,True
        """;

        // Import subscriptions from CSV
        var subscriptionImportResult = await _bulkOperationsService.ImportSubscriptionsAsync(sampleCsvContent, BulkImportFormat.Csv, cancellationToken);
        _logger.LogInformation("Imported subscriptions result: {TotalCount} total, {SuccessCount} successful, {FailureCount} failed",
            subscriptionImportResult.TotalCount, subscriptionImportResult.SuccessCount, subscriptionImportResult.FailureCount);
    }
}