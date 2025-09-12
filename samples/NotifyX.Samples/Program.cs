using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Extensions;
using NotifyX.Core.Models;
using NotifyX.Core.Interfaces;
using NotifyX.SDK;
using NotifyX.SDK.Extensions;
using NotifyX.Providers.Email;

namespace NotifyX.Samples;

/// <summary>
/// Sample application demonstrating NotifyX usage.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        // Create host builder
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add NotifyX core services
                services.AddNotifyX(options =>
                {
                    options.IsEnabled = true;
                    options.DefaultTenantId = "sample-tenant";
                    options.EnableRuleEngine = true;
                    options.EnableTemplateService = true;
                    options.EnableEscalation = true;
                    options.EnableAggregation = true;
                    options.EnableRateLimiting = true;
                });

                // Add email provider
                services.AddNotificationProvider<EmailProvider, EmailProviderOptions>(options =>
                {
                    options.IsEnabled = true;
                    options.ProviderType = EmailProviderType.SMTP;
                    options.SmtpHost = "localhost";
                    options.SmtpPort = 587;
                    options.SmtpEnableSsl = true;
                    options.FromEmail = "noreply@example.com";
                    options.FromName = "NotifyX Sample";
                });

                // Add NotifyX SDK
                services.AddNotifyXSDK(options =>
                {
                    options.DefaultTenantId = "sample-tenant";
                });
            })
            .Build();

        // Get logger
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Starting NotifyX sample application");

            // Get the NotifyX client
            var notifyXClient = host.Services.GetRequiredService<NotifyXClient>();

            // Run samples
            await RunBasicNotificationSample(notifyXClient, logger);
            await RunTemplateSample(notifyXClient, logger);
            await RunRuleSample(notifyXClient, logger);
            await RunBatchNotificationSample(notifyXClient, logger);
            
            // Run bulk operations sample
            var bulkOperationsSample = host.Services.GetRequiredService<BulkOperationsSample>();
            await bulkOperationsSample.RunAsync();
            
            // Run authentication sample
            var authenticationSample = host.Services.GetRequiredService<AuthenticationSample>();
            await authenticationSample.RunAsync();

            logger.LogInformation("Sample application completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running sample application");
        }
    }

    /// <summary>
    /// Demonstrates basic notification sending.
    /// </summary>
    static async Task RunBasicNotificationSample(NotifyXClient client, ILogger logger)
    {
        logger.LogInformation("=== Basic Notification Sample ===");

        try
        {
            // Create a simple notification
            var result = await client.SendAsync(builder =>
            {
                builder
                    .WithEventType("welcome")
                    .WithPriority(NotificationPriority.Normal)
                    .WithSubject("Welcome to NotifyX!")
                    .WithContent("Thank you for using NotifyX notification platform. This is a test notification.")
                    .WithRecipient(new NotificationRecipient
                    {
                        Id = "user-1",
                        Name = "John Doe",
                        Email = "john.doe@example.com"
                    })
                    .WithPreferredChannel(NotificationChannel.Email)
                    .WithTag("welcome")
                    .WithTag("test");
            });

            if (result.IsSuccess)
            {
                logger.LogInformation("✅ Notification sent successfully. ID: {NotificationId}", result.NotificationId);
            }
            else
            {
                logger.LogError("❌ Failed to send notification: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in basic notification sample");
        }
    }

    /// <summary>
    /// Demonstrates template usage.
    /// </summary>
    static async Task RunTemplateSample(NotifyXClient client, ILogger logger)
    {
        logger.LogInformation("=== Template Sample ===");

        try
        {
            // Create a template
            var template = client.CreateTemplate()
                .WithName("Order Confirmation")
                .WithDescription("Template for order confirmation emails")
                .WithChannel(NotificationChannel.Email)
                .WithEventType("order.confirmed")
                .WithSubject("Order Confirmation - Order #{{OrderNumber}}")
                .WithContent(@"
                    <h1>Thank you for your order!</h1>
                    <p>Dear {{CustomerName}},</p>
                    <p>Your order #{{OrderNumber}} has been confirmed.</p>
                    <p>Order Total: ${{OrderTotal}}</p>
                    <p>Expected Delivery: {{DeliveryDate}}</p>
                    <p>Thank you for shopping with us!</p>
                ")
                .WithEngine(TemplateEngine.Simple)
                .WithVariable(new TemplateVariable
                {
                    Name = "CustomerName",
                    Type = TemplateVariableType.String,
                    IsRequired = true,
                    Description = "Customer's full name"
                })
                .WithVariable(new TemplateVariable
                {
                    Name = "OrderNumber",
                    Type = TemplateVariableType.String,
                    IsRequired = true,
                    Description = "Order number"
                })
                .WithVariable(new TemplateVariable
                {
                    Name = "OrderTotal",
                    Type = TemplateVariableType.Decimal,
                    IsRequired = true,
                    Description = "Order total amount"
                })
                .WithVariable(new TemplateVariable
                {
                    Name = "DeliveryDate",
                    Type = TemplateVariableType.Date,
                    IsRequired = true,
                    Description = "Expected delivery date"
                })
                .Build();

            // Add the template
            var templateAdded = await client.AddTemplateAsync(template);
            if (templateAdded)
            {
                logger.LogInformation("✅ Template added successfully. ID: {TemplateId}", template.Id);
            }
            else
            {
                logger.LogError("❌ Failed to add template");
                return;
            }

            // Send notification using template
            var result = await client.SendAsync(builder =>
            {
                builder
                    .WithEventType("order.confirmed")
                    .WithTemplateId(template.Id)
                    .WithTemplateVariable("CustomerName", "Jane Smith")
                    .WithTemplateVariable("OrderNumber", "ORD-12345")
                    .WithTemplateVariable("OrderTotal", 99.99m)
                    .WithTemplateVariable("DeliveryDate", DateTime.Now.AddDays(3))
                    .WithRecipient(new NotificationRecipient
                    {
                        Id = "user-2",
                        Name = "Jane Smith",
                        Email = "jane.smith@example.com"
                    })
                    .WithPreferredChannel(NotificationChannel.Email);
            });

            if (result.IsSuccess)
            {
                logger.LogInformation("✅ Template-based notification sent successfully. ID: {NotificationId}", result.NotificationId);
            }
            else
            {
                logger.LogError("❌ Failed to send template-based notification: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in template sample");
        }
    }

    /// <summary>
    /// Demonstrates rule usage.
    /// </summary>
    static async Task RunRuleSample(NotifyXClient client, ILogger logger)
    {
        logger.LogInformation("=== Rule Sample ===");

        try
        {
            // Create a rule for high-priority notifications
            var rule = client.CreateRule()
                .WithName("High Priority Email Rule")
                .WithDescription("Automatically sends high-priority notifications via email")
                .WithPriority(10)
                .WithCondition(new RuleCondition
                {
                    Type = ConditionType.Priority,
                    Operator = ConditionOperator.Equals,
                    FieldPath = "Priority",
                    ExpectedValues = new List<object> { NotificationPriority.High, NotificationPriority.Critical }
                })
                .WithAction(new RuleAction
                {
                    Type = ActionType.SetChannels,
                    Parameters = new Dictionary<string, object>
                    {
                        ["channels"] = new[] { NotificationChannel.Email.ToString() }
                    }
                })
                .WithAction(new RuleAction
                {
                    Type = ActionType.SetPriority,
                    Parameters = new Dictionary<string, object>
                    {
                        ["priority"] = NotificationPriority.Critical.ToString()
                    }
                })
                .Build();

            // Add the rule
            var ruleAdded = await client.AddRuleAsync(rule);
            if (ruleAdded)
            {
                logger.LogInformation("✅ Rule added successfully. ID: {RuleId}", rule.Id);
            }
            else
            {
                logger.LogError("❌ Failed to add rule");
                return;
            }

            // Send a high-priority notification to trigger the rule
            var result = await client.SendAsync(builder =>
            {
                builder
                    .WithEventType("system.alert")
                    .WithPriority(NotificationPriority.High)
                    .WithSubject("System Alert")
                    .WithContent("This is a high-priority system alert that should trigger the rule.")
                    .WithRecipient(new NotificationRecipient
                    {
                        Id = "admin-1",
                        Name = "System Admin",
                        Email = "admin@example.com"
                    })
                    .WithTag("system")
                    .WithTag("alert");
            });

            if (result.IsSuccess)
            {
                logger.LogInformation("✅ Rule-triggered notification sent successfully. ID: {NotificationId}", result.NotificationId);
            }
            else
            {
                logger.LogError("❌ Failed to send rule-triggered notification: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in rule sample");
        }
    }

    /// <summary>
    /// Demonstrates batch notification sending.
    /// </summary>
    static async Task RunBatchNotificationSample(NotifyXClient client, ILogger logger)
    {
        logger.LogInformation("=== Batch Notification Sample ===");

        try
        {
            // Create multiple notifications
            var notifications = new List<NotificationEvent>();

            for (int i = 1; i <= 5; i++)
            {
                var notification = client.CreateNotification()
                    .WithEventType("batch.test")
                    .WithPriority(NotificationPriority.Normal)
                    .WithSubject($"Batch Test Notification #{i}")
                    .WithContent($"This is batch test notification number {i}.")
                    .WithRecipient(new NotificationRecipient
                    {
                        Id = $"batch-user-{i}",
                        Name = $"Batch User {i}",
                        Email = $"batchuser{i}@example.com"
                    })
                    .WithPreferredChannel(NotificationChannel.Email)
                    .WithTag("batch")
                    .WithTag("test")
                    .Build();

                notifications.Add(notification);
            }

            // Send batch notifications
            var result = await client.SendBatchAsync(notifications);

            logger.LogInformation("✅ Batch notification completed. Total: {Total}, Success: {Success}, Failed: {Failed}", 
                result.TotalCount, result.SuccessCount, result.FailureCount);

            if (result.Status == BatchStatus.AllSuccessful)
            {
                logger.LogInformation("🎉 All batch notifications sent successfully!");
            }
            else if (result.Status == BatchStatus.PartialFailure)
            {
                logger.LogWarning("⚠️ Some batch notifications failed");
            }
            else
            {
                logger.LogError("❌ All batch notifications failed");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in batch notification sample");
        }
    }
}