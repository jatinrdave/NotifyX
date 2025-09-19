using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace NotifyX.Providers.Webhook;

/// <summary>
/// Webhook notification provider for delivering notifications to custom HTTP endpoints.
/// </summary>
public sealed class WebhookProvider : INotificationProvider
{
    private readonly ILogger<WebhookProvider> _logger;
    private readonly WebhookProviderOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the WebhookProvider class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The webhook provider options.</param>
    /// <param name="httpClient">HTTP client for webhook calls.</param>
    public WebhookProvider(
        ILogger<WebhookProvider> logger,
        IOptions<WebhookProviderOptions> options,
        HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.Webhook;

    /// <inheritdoc />
    public bool IsAvailable => _options.IsEnabled;

    /// <inheritdoc />
    public async Task<DeliveryResult> SendAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Sending webhook notification {NotificationId} to {WebhookUrl}", 
                notification.Id, recipient.WebhookUrl);

            if (string.IsNullOrEmpty(recipient.WebhookUrl))
            {
                return DeliveryResult.Failure("Recipient webhook URL is required", "MISSING_WEBHOOK_URL");
            }

            // Validate webhook URL
            if (!IsValidUrl(recipient.WebhookUrl))
            {
                return DeliveryResult.Failure($"Invalid webhook URL: {recipient.WebhookUrl}", "INVALID_WEBHOOK_URL");
            }

            // Send webhook based on configured method
            return _options.HttpMethod.ToUpperInvariant() switch
            {
                "POST" => await SendWebhookPostAsync(notification, recipient, cancellationToken),
                "PUT" => await SendWebhookPutAsync(notification, recipient, cancellationToken),
                "PATCH" => await SendWebhookPatchAsync(notification, recipient, cancellationToken),
                _ => DeliveryResult.Failure($"Unsupported HTTP method: {_options.HttpMethod}", "UNSUPPORTED_METHOD")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook notification {NotificationId} to {WebhookUrl}", 
                notification.Id, recipient.WebhookUrl);
            return DeliveryResult.Failure($"Webhook delivery failed: {ex.Message}", "DELIVERY_ERROR");
        }
    }

    /// <inheritdoc />
    public ValidationResult Validate(NotificationEvent notification, NotificationRecipient recipient)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate recipient webhook URL
        if (string.IsNullOrEmpty(recipient.WebhookUrl))
        {
            errors.Add("Recipient webhook URL is required");
        }
        else if (!IsValidUrl(recipient.WebhookUrl))
        {
            errors.Add($"Invalid webhook URL: {recipient.WebhookUrl}");
        }

        // Validate notification content
        if (string.IsNullOrEmpty(notification.Content) && notification.Metadata?.Count == 0)
        {
            warnings.Add("Webhook has no content or metadata to send");
        }

        // Validate content length
        if (!string.IsNullOrEmpty(notification.Content) && notification.Content.Length > _options.MaxContentLength)
        {
            errors.Add($"Webhook content exceeds maximum length of {_options.MaxContentLength} characters");
        }

        // Check for required authentication
        if (_options.RequireAuthentication && string.IsNullOrEmpty(_options.AuthenticationHeader))
        {
            warnings.Add("Webhook requires authentication but no authentication header is configured");
        }

        // Validate timeout settings
        if (_options.TimeoutSeconds <= 0)
        {
            errors.Add("Webhook timeout must be greater than 0");
        }

        if (_options.TimeoutSeconds > 300) // 5 minutes
        {
            warnings.Add("Webhook timeout is very long, consider reducing it");
        }

        return errors.Any() 
            ? ValidationResult.Failure(errors, warnings)
            : ValidationResult.Success(warnings);
    }

    /// <inheritdoc />
    public async Task<ProviderHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.IsEnabled)
            {
                return ProviderHealthStatus.Unhealthy("Webhook provider is disabled");
            }

            // Test webhook connectivity by making a HEAD request to a test endpoint
            var isHealthy = await TestWebhookConnectivityAsync(cancellationToken);

            return isHealthy 
                ? ProviderHealthStatus.Healthy("Webhook provider is healthy")
                : ProviderHealthStatus.Unhealthy("Webhook provider is unhealthy - connectivity test failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking webhook provider health");
            return ProviderHealthStatus.Unhealthy($"Health check failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task ConfigureAsync(ChannelConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Configuring webhook provider with channel configuration");

            // Update options based on configuration
            if (configuration.Settings.TryGetValue("HttpMethod", out var httpMethod))
            {
                _options.HttpMethod = httpMethod.ToString();
            }

            if (configuration.Settings.TryGetValue("ContentType", out var contentType))
            {
                _options.ContentType = contentType.ToString();
            }

            if (configuration.Settings.TryGetValue("AuthenticationHeader", out var authHeader))
            {
                _options.AuthenticationHeader = authHeader.ToString();
            }

            if (configuration.Settings.TryGetValue("RequireAuthentication", out var requireAuthObj) &&
                bool.TryParse(requireAuthObj.ToString(), out var requireAuth))
            {
                _options.RequireAuthentication = requireAuth;
            }

            if (configuration.Settings.TryGetValue("TimeoutSeconds", out var timeoutObj) &&
                int.TryParse(timeoutObj.ToString(), out var timeout))
            {
                _options.TimeoutSeconds = timeout;
            }

            if (configuration.Settings.TryGetValue("MaxRetryAttempts", out var maxRetryObj) &&
                int.TryParse(maxRetryObj.ToString(), out var maxRetry))
            {
                _options.MaxRetryAttempts = maxRetry;
            }

            if (configuration.Settings.TryGetValue("RetryDelaySeconds", out var retryDelayObj) &&
                int.TryParse(retryDelayObj.ToString(), out var retryDelay))
            {
                _options.RetryDelaySeconds = retryDelay;
            }

            if (configuration.Settings.TryGetValue("MaxContentLength", out var maxContentLengthObj) &&
                int.TryParse(maxContentLengthObj.ToString(), out var maxContentLength))
            {
                _options.MaxContentLength = maxContentLength;
            }

            if (configuration.Settings.TryGetValue("IncludeHeaders", out var includeHeadersObj) &&
                bool.TryParse(includeHeadersObj.ToString(), out var includeHeaders))
            {
                _options.IncludeHeaders = includeHeaders;
            }

            if (configuration.Settings.TryGetValue("TestEndpoint", out var testEndpoint))
            {
                _options.TestEndpoint = testEndpoint.ToString();
            }

            _logger.LogInformation("Webhook provider configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring webhook provider");
            throw;
        }
    }

    /// <summary>
    /// Sends webhook via POST method.
    /// </summary>
    private async Task<DeliveryResult> SendWebhookPostAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        return await SendWebhookAsync(notification, recipient, HttpMethod.Post, cancellationToken);
    }

    /// <summary>
    /// Sends webhook via PUT method.
    /// </summary>
    private async Task<DeliveryResult> SendWebhookPutAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        return await SendWebhookAsync(notification, recipient, HttpMethod.Put, cancellationToken);
    }

    /// <summary>
    /// Sends webhook via PATCH method.
    /// </summary>
    private async Task<DeliveryResult> SendWebhookPatchAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        return await SendWebhookAsync(notification, recipient, HttpMethod.Patch, cancellationToken);
    }

    /// <summary>
    /// Sends webhook with the specified HTTP method.
    /// </summary>
    private async Task<DeliveryResult> SendWebhookAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        HttpMethod method,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create webhook payload
            var payload = CreateWebhookPayload(notification, recipient);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var content = new StringContent(json, Encoding.UTF8, _options.ContentType);

            // Create HTTP request
            var request = new HttpRequestMessage(method, recipient.WebhookUrl)
            {
                Content = content
            };

            // Add authentication header if configured
            if (!string.IsNullOrEmpty(_options.AuthenticationHeader))
            {
                request.Headers.Add("Authorization", _options.AuthenticationHeader);
            }

            // Add custom headers
            if (_options.IncludeHeaders)
            {
                request.Headers.Add("X-Notification-Id", notification.Id);
                request.Headers.Add("X-Notification-Source", notification.Source ?? "NotifyX");
                request.Headers.Add("X-Notification-Timestamp", notification.CreatedAt.ToString("O"));
                
                if (!string.IsNullOrEmpty(notification.CorrelationId))
                {
                    request.Headers.Add("X-Correlation-ID", notification.CorrelationId);
                }
            }

            // Set timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            // Send webhook with retry logic
            var attempt = 0;
            Exception? lastException = null;

            while (attempt < _options.MaxRetryAttempts)
            {
                try
                {
                    var response = await _httpClient.SendAsync(request, cts.Token);
                    var responseContent = await response.Content.ReadAsStringAsync(cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogDebug("Successfully sent webhook to {WebhookUrl} with status {StatusCode}", 
                            recipient.WebhookUrl, response.StatusCode);
                        
                        return DeliveryResult.Success($"webhook_{response.StatusCode}_{Guid.NewGuid()}");
                    }
                    else
                    {
                        _logger.LogWarning("Webhook delivery failed with status {StatusCode}: {Error}", 
                            response.StatusCode, responseContent);
                        
                        // Don't retry on client errors (4xx)
                        if (response.StatusCode >= System.Net.HttpStatusCode.BadRequest && 
                            response.StatusCode < System.Net.HttpStatusCode.InternalServerError)
                        {
                            return DeliveryResult.Failure($"Webhook delivery failed: {response.StatusCode}", 
                                "WEBHOOK_CLIENT_ERROR", false);
                        }
                        
                        lastException = new HttpRequestException($"HTTP {response.StatusCode}: {responseContent}");
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    _logger.LogWarning("Webhook delivery timed out after {TimeoutSeconds} seconds", _options.TimeoutSeconds);
                    lastException = ex;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Webhook delivery attempt {Attempt} failed", attempt + 1);
                    lastException = ex;
                }

                attempt++;
                if (attempt < _options.MaxRetryAttempts)
                {
                    var delay = TimeSpan.FromSeconds(_options.RetryDelaySeconds * Math.Pow(2, attempt - 1)); // Exponential backoff
                    _logger.LogDebug("Retrying webhook delivery in {Delay} seconds (attempt {Attempt}/{MaxAttempts})", 
                        delay.TotalSeconds, attempt + 1, _options.MaxRetryAttempts);
                    await Task.Delay(delay, cancellationToken);
                }
            }

            return DeliveryResult.Failure($"Webhook delivery failed after {_options.MaxRetryAttempts} attempts: {lastException?.Message}", 
                "WEBHOOK_DELIVERY_FAILED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending webhook to {WebhookUrl}", recipient.WebhookUrl);
            return DeliveryResult.Failure($"Webhook delivery failed: {ex.Message}", "WEBHOOK_ERROR");
        }
    }

    /// <summary>
    /// Creates the webhook payload from notification and recipient data.
    /// </summary>
    private object CreateWebhookPayload(NotificationEvent notification, NotificationRecipient recipient)
    {
        return new
        {
            notification = new
            {
                id = notification.Id,
                title = notification.Title,
                content = notification.Content,
                source = notification.Source,
                correlationId = notification.CorrelationId,
                createdAt = notification.CreatedAt,
                metadata = notification.Metadata
            },
            recipient = new
            {
                id = recipient.Id,
                name = recipient.Name,
                email = recipient.Email,
                phoneNumber = recipient.PhoneNumber,
                deviceId = recipient.DeviceId,
                webhookUrl = recipient.WebhookUrl,
                metadata = recipient.Metadata
            },
            delivery = new
            {
                channel = Channel.ToString(),
                timestamp = DateTime.UtcNow,
                attempt = 1 // This would be tracked in a real implementation
            }
        };
    }

    /// <summary>
    /// Tests webhook connectivity.
    /// </summary>
    private async Task<bool> TestWebhookConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_options.TestEndpoint))
            {
                // If no test endpoint is configured, we'll assume connectivity is available
                return true;
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10)); // Short timeout for health check

            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, _options.TestEndpoint), cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook connectivity test failed");
            return false;
        }
    }

    /// <summary>
    /// Validates URL format.
    /// </summary>
    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Configuration options for the webhook provider.
/// </summary>
public sealed class WebhookProviderOptions
{
    /// <summary>
    /// Whether the webhook provider is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// HTTP method to use for webhook calls.
    /// </summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// Content type for webhook payloads.
    /// </summary>
    public string ContentType { get; set; } = "application/json";

    /// <summary>
    /// Authentication header value for webhook calls.
    /// </summary>
    public string AuthenticationHeader { get; set; } = string.Empty;

    /// <summary>
    /// Whether authentication is required for webhook calls.
    /// </summary>
    public bool RequireAuthentication { get; set; } = false;

    /// <summary>
    /// Timeout in seconds for webhook calls.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts for failed webhook calls.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay in seconds between retry attempts.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Maximum content length for webhook payloads.
    /// </summary>
    public int MaxContentLength { get; set; } = 1000000; // 1MB

    /// <summary>
    /// Whether to include additional headers in webhook calls.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Test endpoint URL for connectivity testing.
    /// </summary>
    public string TestEndpoint { get; set; } = string.Empty;
}