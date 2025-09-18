using NotifyX.Core.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotifyX.Core.Interfaces;

/// <summary>
/// Interface for external connector services.
/// </summary>
public interface IExternalConnector
{
    /// <summary>
    /// Gets the connector name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the connector version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets whether the connector is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets the health status of the connector.
    /// </summary>
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures the connector with the provided settings.
    /// </summary>
    Task<bool> ConfigureAsync(ConnectorConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connector connection.
    /// </summary>
    Task<ConnectorTestResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for webhook-based connectors.
/// </summary>
public interface IWebhookConnector : IExternalConnector
{
    /// <summary>
    /// Sends a notification via webhook.
    /// </summary>
    Task<WebhookResult> SendWebhookAsync(NotificationEvent notification, WebhookConfiguration webhookConfig, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates webhook configuration.
    /// </summary>
    Task<ValidationResult> ValidateWebhookConfigurationAsync(WebhookConfiguration webhookConfig, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for Zapier connector.
/// </summary>
public interface IZapierConnector : IWebhookConnector
{
    /// <summary>
    /// Creates a Zap in Zapier.
    /// </summary>
    Task<ZapierZapResult> CreateZapAsync(ZapierZapConfiguration zapConfig, CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers a Zap in Zapier.
    /// </summary>
    Task<ZapierTriggerResult> TriggerZapAsync(string zapId, NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available Zaps for a user.
    /// </summary>
    Task<IEnumerable<ZapierZap>> GetZapsAsync(string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for n8n connector.
/// </summary>
public interface IN8nConnector : IWebhookConnector
{
    /// <summary>
    /// Executes an n8n workflow.
    /// </summary>
    Task<N8nWorkflowResult> ExecuteWorkflowAsync(string workflowId, NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available workflows.
    /// </summary>
    Task<IEnumerable<N8nWorkflow>> GetWorkflowsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a webhook node in n8n.
    /// </summary>
    Task<N8nWebhookResult> CreateWebhookNodeAsync(N8nWebhookConfiguration webhookConfig, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for Make.com connector.
/// </summary>
public interface IMakeConnector : IWebhookConnector
{
    /// <summary>
    /// Executes a Make.com scenario.
    /// </summary>
    Task<MakeScenarioResult> ExecuteScenarioAsync(string scenarioId, NotificationEvent notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available scenarios.
    /// </summary>
    Task<IEnumerable<MakeScenario>> GetScenariosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a webhook module in Make.com.
    /// </summary>
    Task<MakeWebhookResult> CreateWebhookModuleAsync(MakeWebhookConfiguration webhookConfig, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for MuleSoft connector.
/// </summary>
public interface IMuleSoftConnector : IExternalConnector
{
    /// <summary>
    /// Sends a message to MuleSoft.
    /// </summary>
    Task<MuleSoftResult> SendMessageAsync(NotificationEvent notification, MuleSoftConfiguration muleConfig, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes to a MuleSoft exchange.
    /// </summary>
    Task<MuleSoftResult> PublishToExchangeAsync(NotificationEvent notification, string exchangeName, MuleSoftConfiguration muleConfig, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available MuleSoft applications.
    /// </summary>
    Task<IEnumerable<MuleSoftApplication>> GetApplicationsAsync(CancellationToken cancellationToken = default);
}