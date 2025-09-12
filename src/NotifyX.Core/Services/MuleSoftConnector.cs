using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Text;
using System.Text.Json;

namespace NotifyX.Core.Services;

/// <summary>
/// MuleSoft connector implementation.
/// </summary>
public class MuleSoftConnector : IMuleSoftConnector
{
    private readonly ILogger<MuleSoftConnector> _logger;
    private readonly HttpClient _httpClient;
    private ConnectorConfiguration _configuration = new();
    private string? _accessToken;

    public string Name => "MuleSoft";
    public string Version => "1.0.0";
    public bool IsEnabled => _configuration.IsEnabled;

    public MuleSoftConnector(ILogger<MuleSoftConnector> logger, HttpClient httpClient)
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

            // Test connection to MuleSoft API
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_configuration.BaseUrl}/apiplatform/repository/v2/organizations");
            request.Headers.Add("Authorization", $"Bearer {await GetAccessTokenAsync(cancellationToken)}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Degraded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MuleSoft connector health status");
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

            // Get access token
            await GetAccessTokenAsync(cancellationToken);

            _logger.LogInformation("MuleSoft connector configured successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure MuleSoft connector");
            return false;
        }
    }

    public async Task<ConnectorTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/apiplatform/repository/v2/organizations");
            request.Headers.Add("Authorization", $"Bearer {await GetAccessTokenAsync(cancellationToken)}");

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
            _logger.LogError(ex, "Failed to test MuleSoft connection");

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

    public async Task<MuleSoftResult> SendMessageAsync(NotificationEvent notification, MuleSoftConfiguration muleConfig, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = CreateMessagePayload(notification, muleConfig);
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "/apiplatform/messaging/v1/messages")
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {await GetAccessTokenAsync(cancellationToken)}");

            // Add custom headers
            foreach (var header in muleConfig.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var messageId = responseData?.GetValueOrDefault("messageId")?.ToString() ?? string.Empty;

                return new MuleSoftResult
                {
                    IsSuccess = true,
                    MessageId = messageId,
                    Message = "Message sent successfully",
                    Response = responseData ?? new Dictionary<string, object>()
                };
            }

            return new MuleSoftResult
            {
                IsSuccess = false,
                MessageId = string.Empty,
                Message = $"Failed to send message: {response.StatusCode}",
                Response = new Dictionary<string, object>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["ResponseBody"] = responseBody
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to MuleSoft");
            return new MuleSoftResult
            {
                IsSuccess = false,
                MessageId = string.Empty,
                Message = $"Failed to send message: {ex.Message}",
                Response = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                }
            };
        }
    }

    public async Task<MuleSoftResult> PublishToExchangeAsync(NotificationEvent notification, string exchangeName, MuleSoftConfiguration muleConfig, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = CreateMessagePayload(notification, muleConfig);
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"/apiplatform/messaging/v1/exchanges/{exchangeName}/publish")
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {await GetAccessTokenAsync(cancellationToken)}");

            // Add custom headers
            foreach (var header in muleConfig.Headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var messageId = responseData?.GetValueOrDefault("messageId")?.ToString() ?? string.Empty;

                return new MuleSoftResult
                {
                    IsSuccess = true,
                    MessageId = messageId,
                    Message = "Message published to exchange successfully",
                    Response = responseData ?? new Dictionary<string, object>()
                };
            }

            return new MuleSoftResult
            {
                IsSuccess = false,
                MessageId = string.Empty,
                Message = $"Failed to publish to exchange: {response.StatusCode}",
                Response = new Dictionary<string, object>
                {
                    ["StatusCode"] = (int)response.StatusCode,
                    ["ResponseBody"] = responseBody
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to MuleSoft exchange {ExchangeName}", exchangeName);
            return new MuleSoftResult
            {
                IsSuccess = false,
                MessageId = string.Empty,
                Message = $"Failed to publish to exchange: {ex.Message}",
                Response = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message
                }
            };
        }
    }

    public async Task<IEnumerable<MuleSoftApplication>> GetApplicationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/apiplatform/repository/v2/applications");
            request.Headers.Add("Authorization", $"Bearer {await GetAccessTokenAsync(cancellationToken)}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                var applications = responseData?.GetValueOrDefault("data") as JsonElement?;

                if (applications.HasValue && applications.Value.ValueKind == JsonValueKind.Array)
                {
                    var applicationList = new List<MuleSoftApplication>();
                    foreach (var appElement in applications.Value.EnumerateArray())
                    {
                        var application = JsonSerializer.Deserialize<MuleSoftApplication>(appElement.GetRawText());
                        if (application != null)
                        {
                            applicationList.Add(application);
                        }
                    }
                    return applicationList;
                }
            }

            _logger.LogWarning("Failed to retrieve MuleSoft applications: {StatusCode}", response.StatusCode);
            return new List<MuleSoftApplication>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MuleSoft applications");
            return new List<MuleSoftApplication>();
        }
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_accessToken))
        {
            return _accessToken;
        }

        try
        {
            var tokenRequest = new
            {
                client_id = _configuration.ApiKey,
                client_secret = _configuration.Secret,
                grant_type = "client_credentials"
            };

            var json = JsonSerializer.Serialize(tokenRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "/accounts/api/v2/oauth2/token")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
                _accessToken = tokenData?.GetValueOrDefault("access_token")?.ToString() ?? string.Empty;
                return _accessToken;
            }

            _logger.LogError("Failed to get MuleSoft access token: {StatusCode}", response.StatusCode);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get MuleSoft access token");
            return string.Empty;
        }
    }

    private static object CreateMessagePayload(NotificationEvent notification, MuleSoftConfiguration muleConfig)
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
            environment = muleConfig.Environment,
            timestamp = DateTime.UtcNow
        };
    }
}