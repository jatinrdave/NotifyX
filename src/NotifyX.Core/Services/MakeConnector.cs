using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Text;
using System.Text.Json;

namespace NotifyX.Core.Services;

/// <summary>
/// Make.com connector implementation.
/// </summary>
public class MakeConnector : IMakeConnector
{
    private readonly ILogger<MakeConnector> _logger;
    private readonly HttpClient _httpClient;
    private ConnectorConfiguration _configuration = new();

    public string Name => "Make.com";
    public string Version => "1.0.0";
    public bool IsEnabled => _configuration.IsEnabled;

    public MakeConnector(ILogger<MakeConnector> logger, HttpClient httpClient)
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

            // Test connection to Make.com API
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_configuration.BaseUrl}/v2/scenarios");
            request.Headers.Add("Authorization", $"Token {_configuration.ApiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Degraded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Make.com connector health status");
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

            _logger.LogInformation("Make.com connector configured successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Make.com connector");
            return false;
        }
    }

    public async Task<ConnectorTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/v2/scenarios");
            request.Headers.Add("Authorization", $"Token {_configuration.ApiKey}");

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
            _logger.LogError(ex, "Failed to test Make.com connection");

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
            _logger.LogError(ex, "Failed to send webhook to Make.com");

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
                return ValidationResult.Failure("Webhook URL is required");
            }

            if (!Uri.TryCreate(webhookConfig.Url, UriKind.Absolute, out var uri))
            {
                return ValidationResult.Failure("Invalid webhook URL format");
            }

            if (uri.Scheme != "https" && uri.Scheme != "http")
            {
                return ValidationResult.Failure("Webhook URL must use HTTP or HTTPS scheme");
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate webhook configuration");
            return ValidationResult.Failure($"Validation failed: {ex.Message}");
        }
    }

    public async Task<MakeScenarioResult> ExecuteScenarioAsync(string scenarioId, NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = CreateWebhookPayload(notification, new WebhookConfiguration());
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/v2/scenarios/{scenarioId}/executions")
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Token {_configuration.ApiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var executionId = responseData?.GetValueOrDefault("id")?.ToString() ?? string.Empty;

                return new MakeScenarioResult
                {
                    IsSuccess = true,
                    ExecutionId = executionId,
                    Message = "Scenario executed successfully",
                    Output = responseData ?? new Dictionary<string, object>()
                };
            }

            return new MakeScenarioResult
            {
                IsSuccess = false,
                ExecutionId = string.Empty,
                Message = $"Failed to execute scenario: {response.StatusCode}",
                Output = new Dictionary<string, object>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["ResponseBody"] = responseBody
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Make.com scenario {ScenarioId}", scenarioId);
            return new MakeScenarioResult
            {
                IsSuccess = false,
                ExecutionId = string.Empty,
                Message = $"Failed to execute scenario: {ex.Message}",
                Output = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                }
            };
        }
    }

    public async Task<IEnumerable<MakeScenario>> GetScenariosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/v2/scenarios");
            request.Headers.Add("Authorization", $"Token {_configuration.ApiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var scenarios = responseData?.GetValueOrDefault("scenarios") as JsonElement?;

                if (scenarios.HasValue && scenarios.Value.ValueKind == JsonValueKind.Array)
                {
                    var scenarioList = new List<MakeScenario>();
                    foreach (var scenarioElement in scenarios.Value.EnumerateArray())
                    {
                        var scenario = JsonSerializer.Deserialize<MakeScenario>(scenarioElement.GetRawText());
                        if (scenario != null)
                        {
                            scenarioList.Add(scenario);
                        }
                    }
                    return scenarioList;
                }
            }

            _logger.LogWarning("Failed to retrieve Make.com scenarios: {StatusCode}", response.StatusCode);
            return new List<MakeScenario>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Make.com scenarios");
            return new List<MakeScenario>();
        }
    }

    public async Task<MakeWebhookResult> CreateWebhookModuleAsync(MakeWebhookConfiguration webhookConfig, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                scenarioId = webhookConfig.ScenarioId,
                moduleName = webhookConfig.ModuleName,
                webhookPath = webhookConfig.WebhookPath,
                httpMethod = webhookConfig.HttpMethod,
                moduleData = webhookConfig.ModuleData
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "/v2/webhooks")
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Token {_configuration.ApiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var webhookUrl = responseData?.GetValueOrDefault("webhookUrl")?.ToString() ?? string.Empty;

                return new MakeWebhookResult
                {
                    IsSuccess = true,
                    WebhookUrl = webhookUrl,
                    Message = "Webhook module created successfully",
                    Details = responseData ?? new Dictionary<string, object>()
                };
            }

            return new MakeWebhookResult
            {
                IsSuccess = false,
                WebhookUrl = string.Empty,
                Message = $"Failed to create webhook module: {response.StatusCode}",
                Details = new Dictionary<string, object>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["ResponseBody"] = responseBody
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Make.com webhook module");
            return new MakeWebhookResult
            {
                IsSuccess = false,
                WebhookUrl = string.Empty,
                Message = $"Failed to create webhook module: {ex.Message}",
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