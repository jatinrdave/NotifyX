using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;

namespace NotifyX.Samples;

/// <summary>
/// Sample demonstrating external connector functionality.
/// </summary>
public class ExternalConnectorsSample
{
    private readonly ILogger<ExternalConnectorsSample> _logger;
    private readonly IZapierConnector _zapierConnector;
    private readonly IN8nConnector _n8nConnector;
    private readonly IMakeConnector _makeConnector;
    private readonly IMuleSoftConnector _muleSoftConnector;

    public ExternalConnectorsSample(
        ILogger<ExternalConnectorsSample> logger,
        IZapierConnector zapierConnector,
        IN8nConnector n8nConnector,
        IMakeConnector makeConnector,
        IMuleSoftConnector muleSoftConnector)
    {
        _logger = logger;
        _zapierConnector = zapierConnector;
        _n8nConnector = n8nConnector;
        _makeConnector = makeConnector;
        _muleSoftConnector = muleSoftConnector;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting External Connectors Sample");

        try
        {
            // Configure connectors
            await ConfigureConnectorsAsync();

            // Test connector connections
            await TestConnectorConnectionsAsync();

            // Demonstrate Zapier functionality
            await DemonstrateZapierConnectorAsync();

            // Demonstrate n8n functionality
            await DemonstrateN8nConnectorAsync();

            // Demonstrate Make.com functionality
            await DemonstrateMakeConnectorAsync();

            // Demonstrate MuleSoft functionality
            await DemonstrateMuleSoftConnectorAsync();

            // Demonstrate webhook functionality
            await DemonstrateWebhookFunctionalityAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in External Connectors Sample");
        }
    }

    private async Task ConfigureConnectorsAsync()
    {
        _logger.LogInformation("=== Configuring External Connectors ===");

        // Configure Zapier connector
        var zapierConfig = new ConnectorConfiguration
        {
            Name = "Zapier",
            BaseUrl = "https://hooks.zapier.com",
            ApiKey = "test-zapier-api-key",
            IsEnabled = true,
            Timeout = TimeSpan.FromSeconds(30)
        };

        var zapierConfigured = await _zapierConnector.ConfigureAsync(zapierConfig);
        _logger.LogInformation("Zapier connector configured: {Success}", zapierConfigured);

        // Configure n8n connector
        var n8nConfig = new ConnectorConfiguration
        {
            Name = "n8n",
            BaseUrl = "https://n8n.example.com",
            ApiKey = "test-n8n-api-key",
            IsEnabled = true,
            Timeout = TimeSpan.FromSeconds(30)
        };

        var n8nConfigured = await _n8nConnector.ConfigureAsync(n8nConfig);
        _logger.LogInformation("n8n connector configured: {Success}", n8nConfigured);

        // Configure Make.com connector
        var makeConfig = new ConnectorConfiguration
        {
            Name = "Make.com",
            BaseUrl = "https://api.eu1.make.com",
            ApiKey = "test-make-api-key",
            IsEnabled = true,
            Timeout = TimeSpan.FromSeconds(30)
        };

        var makeConfigured = await _makeConnector.ConfigureAsync(makeConfig);
        _logger.LogInformation("Make.com connector configured: {Success}", makeConfigured);

        // Configure MuleSoft connector
        var muleSoftConfig = new ConnectorConfiguration
        {
            Name = "MuleSoft",
            BaseUrl = "https://anypoint.mulesoft.com",
            ApiKey = "test-mulesoft-client-id",
            Secret = "test-mulesoft-client-secret",
            IsEnabled = true,
            Timeout = TimeSpan.FromSeconds(30)
        };

        var muleSoftConfigured = await _muleSoftConnector.ConfigureAsync(muleSoftConfig);
        _logger.LogInformation("MuleSoft connector configured: {Success}", muleSoftConfigured);
    }

    private async Task TestConnectorConnectionsAsync()
    {
        _logger.LogInformation("=== Testing Connector Connections ===");

        // Test Zapier connection
        var zapierTest = await _zapierConnector.TestConnectionAsync();
        _logger.LogInformation("Zapier connection test: {Success} - {Message}", zapierTest.IsSuccess, zapierTest.Message);

        // Test n8n connection
        var n8nTest = await _n8nConnector.TestConnectionAsync();
        _logger.LogInformation("n8n connection test: {Success} - {Message}", n8nTest.IsSuccess, n8nTest.Message);

        // Test Make.com connection
        var makeTest = await _makeConnector.TestConnectionAsync();
        _logger.LogInformation("Make.com connection test: {Success} - {Message}", makeTest.IsSuccess, makeTest.Message);

        // Test MuleSoft connection
        var muleSoftTest = await _muleSoftConnector.TestConnectionAsync();
        _logger.LogInformation("MuleSoft connection test: {Success} - {Message}", muleSoftTest.IsSuccess, muleSoftTest.Message);
    }

    private async Task DemonstrateZapierConnectorAsync()
    {
        _logger.LogInformation("=== Zapier Connector Demonstration ===");

        // Create a Zap
        var zapConfig = new ZapierZapConfiguration
        {
            Name = "NotifyX User Login Zap",
            Description = "Triggers when a user logs in",
            TriggerUrl = "https://hooks.zapier.com/hooks/catch/123456/abcdef/",
            WebhookUrl = "https://api.notifyx.com/webhooks/zapier",
            IsEnabled = true
        };

        var zapResult = await _zapierConnector.CreateZapAsync(zapConfig);
        _logger.LogInformation("Created Zap: {Success} - {ZapId}", zapResult.IsSuccess, zapResult.ZapId);

        // Get available Zaps
        var zaps = await _zapierConnector.GetZapsAsync("test-user-id");
        _logger.LogInformation("Retrieved {Count} Zaps", zaps.Count());

        // Trigger a Zap
        var notification = CreateTestNotification("zapier-test", "user.login");
        var triggerResult = await _zapierConnector.TriggerZapAsync("test-zap-id", notification);
        _logger.LogInformation("Triggered Zap: {Success} - {ExecutionId}", triggerResult.IsSuccess, triggerResult.ExecutionId);
    }

    private async Task DemonstrateN8nConnectorAsync()
    {
        _logger.LogInformation("=== n8n Connector Demonstration ===");

        // Get available workflows
        var workflows = await _n8nConnector.GetWorkflowsAsync();
        _logger.LogInformation("Retrieved {Count} n8n workflows", workflows.Count());

        // Execute a workflow
        var notification = CreateTestNotification("n8n-test", "system.alert");
        var workflowResult = await _n8nConnector.ExecuteWorkflowAsync("test-workflow-id", notification);
        _logger.LogInformation("Executed workflow: {Success} - {ExecutionId}", workflowResult.IsSuccess, workflowResult.ExecutionId);

        // Create a webhook node
        var webhookConfig = new N8nWebhookConfiguration
        {
            WorkflowId = "test-workflow-id",
            NodeName = "NotifyX Webhook",
            WebhookPath = "/notifyx-webhook",
            HttpMethod = "POST"
        };

        var webhookResult = await _n8nConnector.CreateWebhookNodeAsync(webhookConfig);
        _logger.LogInformation("Created webhook node: {Success} - {WebhookUrl}", webhookResult.IsSuccess, webhookResult.WebhookUrl);
    }

    private async Task DemonstrateMakeConnectorAsync()
    {
        _logger.LogInformation("=== Make.com Connector Demonstration ===");

        // Get available scenarios
        var scenarios = await _makeConnector.GetScenariosAsync();
        _logger.LogInformation("Retrieved {Count} Make.com scenarios", scenarios.Count());

        // Execute a scenario
        var notification = CreateTestNotification("make-test", "notification.sent");
        var scenarioResult = await _makeConnector.ExecuteScenarioAsync("test-scenario-id", notification);
        _logger.LogInformation("Executed scenario: {Success} - {ExecutionId}", scenarioResult.IsSuccess, scenarioResult.ExecutionId);

        // Create a webhook module
        var webhookConfig = new MakeWebhookConfiguration
        {
            ScenarioId = "test-scenario-id",
            ModuleName = "NotifyX Webhook",
            WebhookPath = "/notifyx-webhook",
            HttpMethod = "POST"
        };

        var webhookResult = await _makeConnector.CreateWebhookModuleAsync(webhookConfig);
        _logger.LogInformation("Created webhook module: {Success} - {WebhookUrl}", webhookResult.IsSuccess, webhookResult.WebhookUrl);
    }

    private async Task DemonstrateMuleSoftConnectorAsync()
    {
        _logger.LogInformation("=== MuleSoft Connector Demonstration ===");

        // Get available applications
        var applications = await _muleSoftConnector.GetApplicationsAsync();
        _logger.LogInformation("Retrieved {Count} MuleSoft applications", applications.Count());

        // Send a message
        var notification = CreateTestNotification("mulesoft-test", "user.logout");
        var muleConfig = new MuleSoftConfiguration
        {
            BaseUrl = "https://anypoint.mulesoft.com",
            Username = "test-user",
            Password = "test-password",
            Environment = "Sandbox"
        };

        var messageResult = await _muleSoftConnector.SendMessageAsync(notification, muleConfig);
        _logger.LogInformation("Sent message: {Success} - {MessageId}", messageResult.IsSuccess, messageResult.MessageId);

        // Publish to exchange
        var exchangeResult = await _muleSoftConnector.PublishToExchangeAsync(notification, "notifyx-exchange", muleConfig);
        _logger.LogInformation("Published to exchange: {Success} - {MessageId}", exchangeResult.IsSuccess, exchangeResult.MessageId);
    }

    private async Task DemonstrateWebhookFunctionalityAsync()
    {
        _logger.LogInformation("=== Webhook Functionality Demonstration ===");

        var notification = CreateTestNotification("webhook-test", "webhook.event");
        var webhookConfig = new WebhookConfiguration
        {
            Url = "https://webhook.site/unique-id",
            Method = "POST",
            ContentType = "application/json",
            Headers = new Dictionary<string, string>
            {
                ["X-Custom-Header"] = "NotifyX-Webhook",
                ["Authorization"] = "Bearer test-token"
            }
        };

        // Test webhook with Zapier connector
        var zapierWebhookResult = await _zapierConnector.SendWebhookAsync(notification, webhookConfig);
        _logger.LogInformation("Zapier webhook result: {Success} - {StatusCode}", zapierWebhookResult.IsSuccess, zapierWebhookResult.StatusCode);

        // Test webhook with n8n connector
        var n8nWebhookResult = await _n8nConnector.SendWebhookAsync(notification, webhookConfig);
        _logger.LogInformation("n8n webhook result: {Success} - {StatusCode}", n8nWebhookResult.IsSuccess, n8nWebhookResult.StatusCode);

        // Test webhook with Make.com connector
        var makeWebhookResult = await _makeConnector.SendWebhookAsync(notification, webhookConfig);
        _logger.LogInformation("Make.com webhook result: {Success} - {StatusCode}", makeWebhookResult.IsSuccess, makeWebhookResult.StatusCode);

        // Validate webhook configuration
        var validationResult = await _zapierConnector.ValidateWebhookConfigurationAsync(webhookConfig);
        _logger.LogInformation("Webhook validation: {Success} - {Message}", validationResult.IsSuccess, validationResult.Message);
    }

    private static NotificationEvent CreateTestNotification(string id, string eventType)
    {
        return new NotificationEvent
        {
            Id = id,
            TenantId = "test-tenant",
            EventType = eventType,
            Priority = NotificationPriority.Normal,
            Subject = $"Test Notification - {eventType}",
            Content = $"This is a test notification for {eventType}",
            Recipients = new List<NotificationRecipient>
            {
                new NotificationRecipient
                {
                    Id = "test-recipient",
                    Name = "Test User",
                    Email = "test@example.com"
                }
            },
            PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email },
            Metadata = new Dictionary<string, object>
            {
                ["source"] = "external-connector-sample",
                ["timestamp"] = DateTime.UtcNow
            }
        };
    }
}