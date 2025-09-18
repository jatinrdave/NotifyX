using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text;
using System.Text.Json;

namespace NotifyX.Core.Services;

/// <summary>
/// n8n connector implementation.
/// </summary>
public class N8nConnector : IN8nConnector
{
    private readonly ILogger<N8nConnector> _logger;
    private readonly HttpClient _httpClient;
    private ConnectorConfiguration _configuration = new();

    public string Name => "n8n";
    public string Version => "1.0.0";
    public bool IsEnabled => _configuration.IsEnabled;

    public N8nConnector(ILogger<N8nConnector> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsEnabled)
            {
                return HealthStatus.Unhealthy;
            }

            // Test connection to n8n API
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_configuration.BaseUrl}/api/v1/workflows");
            request.Headers.Add("X-N8N-API-KEY", _configuration.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Degraded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get n8n connector health status");
            return HealthStatus.Unhealthy;
        }
    }

    public async Task<bool> ConfigureAsync(ConnectorConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _configuration = configuration;
            
            // Configure HTTP client
            _httpClient.BaseAddress = new Uri(_configuration.BaseUrl);
            _httpClient.Timeout = _configuration.Timeout;

            // Add default headers
            foreach (var header in _configuration.Headers)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            _logger.LogInformation("n8n connector configured successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure n8n connector");
            return false;
        }
    }

    public async Task<ConnectorTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/workflows");
            request.Headers.Add("X-N8N-API-KEY", _configuration.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var duration = DateTime.UtcNow - startTime;

            return new ConnectorTestResult
            {
                IsSuccess = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode ? "Connection successful" : $"Connection failed: {response.StatusCode}",
                Duration = duration,
                Details = new Dictionary<string, object>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["StatusMessage"] = response.ReasonPhrase ?? string.Empty
                }
            };
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Failed to test n8n connection");

            return new ConnectorTestResult
            {
                IsSuccess = false,
                Message = $"Connection test failed: {ex.Message}",
                Duration = duration,
                Details = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message,
                    ["ErrorType"] = ex.GetType().Name
                }
            };
        }
    }

    public async Task<WebhookResult> SendWebhookAsync(NotificationEvent notification, WebhookConfiguration webhookConfig, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var payload = CreateWebhookPayload(notification, webhookConfig);
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, webhookConfig.ContentType);

            var request = new HttpRequestMessage(new HttpMethod(webhookConfig.Method), webhookConfig.Url)
            {
                Content = content
            };

            // Add headers
            foreach (var header in webhookConfig.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var duration = DateTime.UtcNow - startTime;

            return new WebhookResult
            {
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                ResponseBody = responseBody,
                ResponseHeaders = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)),
                Duration = duration,
                ErrorMessage = response.IsSuccessStatusCode ? string.Empty : $"HTTP {response.StatusCode}: {response.ReasonPhrase}"
            };
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Failed to send webhook to n8n");

            return new WebhookResult
            {
                IsSuccess = false,
                StatusCode = 0,
                ResponseBody = string.Empty,
                ResponseHeaders = new Dictionary<string, string>(),
                Duration = duration,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ValidationResult> ValidateWebhookConfigurationAsync(WebhookConfiguration webhookConfig, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(webhookConfig.Url))
            {
                return ValidationResult.Failure(new List<string> { "Webhook URL is required" });
            }

            if (!Uri.TryCreate(webhookConfig.Url, UriKind.Absolute, out var uri))
            {
                return ValidationResult.Failure(new List<string> { "Invalid webhook URL format" });
            }

            if (uri.Scheme != "https" && uri.Scheme != "http")
            {
                return ValidationResult.Failure(new List<string> { "Webhook URL must use HTTP or HTTPS scheme" });
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate webhook configuration");
            return ValidationResult.Failure(new List<string> { $"Validation failed: {ex.Message}" });
        }
    }

    public async Task<N8nWorkflowResult> ExecuteWorkflowAsync(string workflowId, NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = CreateWebhookPayload(notification, new WebhookConfiguration());
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/workflows/{workflowId}/execute")
            {
                Content = content
            };
            request.Headers.Add("X-N8N-API-KEY", _configuration.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var executionId = responseData?.GetValueOrDefault("executionId")?.ToString() ?? string.Empty;

                return new N8nWorkflowResult
                {
                    IsSuccess = true,
                    ExecutionId = executionId,
                    Message = "Workflow executed successfully",
                    Output = responseData ?? new Dictionary<string, object>()
                };
            }

            return new N8nWorkflowResult
            {
                IsSuccess = false,
                ExecutionId = string.Empty,
                Message = $"Failed to execute workflow: {response.StatusCode}",
                Output = new Dictionary<string, object>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["ResponseBody"] = responseBody
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute n8n workflow {WorkflowId}", workflowId);
            return new N8nWorkflowResult
            {
                IsSuccess = false,
                ExecutionId = string.Empty,
                Message = $"Failed to execute workflow: {ex.Message}",
                Output = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                }
            };
        }
    }

    public async Task<IEnumerable<N8nWorkflow>> GetWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/workflows");
            request.Headers.Add("X-N8N-API-KEY", _configuration.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var workflows = responseData?.GetValueOrDefault("data") as JsonElement?;

                if (workflows.HasValue && workflows.Value.ValueKind == JsonValueKind.Array)
                {
                    var workflowList = new List<N8nWorkflow>();
                    foreach (var workflowElement in workflows.Value.EnumerateArray())
                    {
                        var workflow = JsonSerializer.Deserialize<N8nWorkflow>(workflowElement.GetRawText());
                        if (workflow != null)
                        {
                            workflowList.Add(workflow);
                        }
                    }
                    return workflowList;
                }
            }

            _logger.LogWarning("Failed to retrieve n8n workflows: {StatusCode}", response.StatusCode);
            return new List<N8nWorkflow>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get n8n workflows");
            return new List<N8nWorkflow>();
        }
    }

    public async Task<N8nWebhookResult> CreateWebhookNodeAsync(N8nWebhookConfiguration webhookConfig, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                workflowId = webhookConfig.WorkflowId,
                nodeName = webhookConfig.NodeName,
                webhookPath = webhookConfig.WebhookPath,
                httpMethod = webhookConfig.HttpMethod,
                nodeData = webhookConfig.NodeData
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/webhooks")
            {
                Content = content
            };
            request.Headers.Add("X-N8N-API-KEY", _configuration.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var webhookUrl = responseData?.GetValueOrDefault("webhookUrl")?.ToString() ?? string.Empty;

                return new N8nWebhookResult
                {
                    IsSuccess = true,
                    WebhookUrl = webhookUrl,
                    Message = "Webhook node created successfully",
                    Details = responseData ?? new Dictionary<string, object>()
                };
            }

            return new N8nWebhookResult
            {
                IsSuccess = false,
                WebhookUrl = string.Empty,
                Message = $"Failed to create webhook node: {response.StatusCode}",
                Details = new Dictionary<string, object>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["ResponseBody"] = responseBody
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create n8n webhook node");
            return new N8nWebhookResult
            {
                IsSuccess = false,
                WebhookUrl = string.Empty,
                Message = $"Failed to create webhook node: {ex.Message}",
                Details = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                }
            };
        }
    }

    private static object CreateWebhookPayload(NotificationEvent notification, WebhookConfiguration webhookConfig)
    {
        return new
        {
            notification_id = notification.Id,
            tenant_id = notification.TenantId,
            event_type = notification.EventType,
            priority = notification.Priority.ToString(),
            subject = notification.Subject,
            content = notification.Content,
            recipients = notification.Recipients.Select(r => new
            {
                id = r.Id,
                name = r.Name,
                email = r.Email,
                phone_number = r.PhoneNumber
            }),
            preferred_channels = notification.PreferredChannels.Select(c => c.ToString()),
            scheduled_for = notification.ScheduledFor,
            metadata = notification.Metadata,
            timestamp = DateTime.UtcNow
        };
    }
}