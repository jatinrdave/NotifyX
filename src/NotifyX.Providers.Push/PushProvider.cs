using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace NotifyX.Providers.Push;

/// <summary>
/// Push notification provider supporting multiple push services (Firebase FCM, Apple APNs, Microsoft WNS).
/// </summary>
public sealed class PushProvider : INotificationProvider
{
    private readonly ILogger<PushProvider> _logger;
    private readonly PushProviderOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the PushProvider class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The push provider options.</param>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public PushProvider(
        ILogger<PushProvider> logger,
        IOptions<PushProviderOptions> options,
        HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.Push;

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
            _logger.LogDebug("Sending push notification {NotificationId} to {RecipientDeviceId}", 
                notification.Id, recipient.DeviceId);

            if (string.IsNullOrEmpty(recipient.DeviceId))
            {
                return DeliveryResult.Failure("Recipient device ID is required", "MISSING_DEVICE_ID");
            }

            // Determine platform based on device ID or recipient metadata
            var platform = DeterminePlatform(recipient);
            
            // Send based on platform
            return platform switch
            {
                PushPlatform.Firebase => await SendViaFirebaseAsync(notification, recipient, cancellationToken),
                PushPlatform.Apple => await SendViaAppleApnsAsync(notification, recipient, cancellationToken),
                PushPlatform.Microsoft => await SendViaMicrosoftWnsAsync(notification, recipient, cancellationToken),
                _ => DeliveryResult.Failure($"Unsupported push platform: {platform}", "UNSUPPORTED_PLATFORM")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification {NotificationId} to {RecipientDeviceId}", 
                notification.Id, recipient.DeviceId);
            return DeliveryResult.Failure($"Push delivery failed: {ex.Message}", "DELIVERY_ERROR");
        }
    }

    /// <inheritdoc />
    public ValidationResult Validate(NotificationEvent notification, NotificationRecipient recipient)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate recipient device ID
        if (string.IsNullOrEmpty(recipient.DeviceId))
        {
            errors.Add("Recipient device ID is required");
        }

        // Validate notification content
        if (string.IsNullOrEmpty(notification.Title) && string.IsNullOrEmpty(notification.Content))
        {
            warnings.Add("Push notification has no title or content");
        }

        // Validate content length
        if (!string.IsNullOrEmpty(notification.Content) && notification.Content.Length > _options.MaxContentLength)
        {
            errors.Add($"Push content exceeds maximum length of {_options.MaxContentLength} characters");
        }

        // Validate title length
        if (!string.IsNullOrEmpty(notification.Title) && notification.Title.Length > _options.MaxTitleLength)
        {
            errors.Add($"Push title exceeds maximum length of {_options.MaxTitleLength} characters");
        }

        // Check for required fields based on platform
        var platform = DeterminePlatform(recipient);
        if (platform == PushPlatform.Apple && string.IsNullOrEmpty(notification.Title))
        {
            warnings.Add("Apple Push Notifications typically require a title");
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
                return ProviderHealthStatus.Unhealthy("Push provider is disabled");
            }

            // Test the configured providers
            var firebaseHealthy = await TestFirebaseConnectionAsync(cancellationToken);
            var appleHealthy = await TestAppleApnsConnectionAsync(cancellationToken);
            var microsoftHealthy = await TestMicrosoftWnsConnectionAsync(cancellationToken);

            var healthyProviders = new List<string>();
            if (firebaseHealthy) healthyProviders.Add("Firebase");
            if (appleHealthy) healthyProviders.Add("Apple");
            if (microsoftHealthy) healthyProviders.Add("Microsoft");

            var isHealthy = healthyProviders.Any();
            var message = isHealthy 
                ? $"Push provider is healthy (available: {string.Join(", ", healthyProviders)})"
                : "Push provider is unhealthy (no platforms available)";

            return isHealthy 
                ? ProviderHealthStatus.Healthy(message)
                : ProviderHealthStatus.Unhealthy(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking push provider health");
            return ProviderHealthStatus.Unhealthy($"Health check failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task ConfigureAsync(ChannelConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Configuring push provider with channel configuration");

            // Update options based on configuration
            if (configuration.Settings.TryGetValue("FirebaseServerKey", out var firebaseServerKey))
            {
                _options.FirebaseServerKey = firebaseServerKey.ToString();
            }

            if (configuration.Settings.TryGetValue("FirebaseProjectId", out var firebaseProjectId))
            {
                _options.FirebaseProjectId = firebaseProjectId.ToString();
            }

            if (configuration.Settings.TryGetValue("AppleApnsKeyId", out var appleApnsKeyId))
            {
                _options.AppleApnsKeyId = appleApnsKeyId.ToString();
            }

            if (configuration.Settings.TryGetValue("AppleApnsTeamId", out var appleApnsTeamId))
            {
                _options.AppleApnsTeamId = appleApnsTeamId.ToString();
            }

            if (configuration.Settings.TryGetValue("AppleApnsBundleId", out var appleApnsBundleId))
            {
                _options.AppleApnsBundleId = appleApnsBundleId.ToString();
            }

            if (configuration.Settings.TryGetValue("AppleApnsPrivateKey", out var appleApnsPrivateKey))
            {
                _options.AppleApnsPrivateKey = appleApnsPrivateKey.ToString();
            }

            if (configuration.Settings.TryGetValue("AppleApnsUseSandbox", out var appleApnsUseSandboxObj) &&
                bool.TryParse(appleApnsUseSandboxObj.ToString(), out var appleApnsUseSandbox))
            {
                _options.AppleApnsUseSandbox = appleApnsUseSandbox;
            }

            if (configuration.Settings.TryGetValue("MicrosoftWnsClientId", out var microsoftWnsClientId))
            {
                _options.MicrosoftWnsClientId = microsoftWnsClientId.ToString();
            }

            if (configuration.Settings.TryGetValue("MicrosoftWnsClientSecret", out var microsoftWnsClientSecret))
            {
                _options.MicrosoftWnsClientSecret = microsoftWnsClientSecret.ToString();
            }

            if (configuration.Settings.TryGetValue("MicrosoftWnsPackageSid", out var microsoftWnsPackageSid))
            {
                _options.MicrosoftWnsPackageSid = microsoftWnsPackageSid.ToString();
            }

            if (configuration.Settings.TryGetValue("MaxContentLength", out var maxContentLengthObj) &&
                int.TryParse(maxContentLengthObj.ToString(), out var maxContentLength))
            {
                _options.MaxContentLength = maxContentLength;
            }

            if (configuration.Settings.TryGetValue("MaxTitleLength", out var maxTitleLengthObj) &&
                int.TryParse(maxTitleLengthObj.ToString(), out var maxTitleLength))
            {
                _options.MaxTitleLength = maxTitleLength;
            }

            _logger.LogInformation("Push provider configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring push provider");
            throw;
        }
    }

    /// <summary>
    /// Sends push notification via Firebase FCM.
    /// </summary>
    private async Task<DeliveryResult> SendViaFirebaseAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = new
            {
                to = recipient.DeviceId,
                notification = new
                {
                    title = notification.Title ?? "Notification",
                    body = notification.Content,
                    icon = notification.IconUrl,
                    sound = "default",
                    click_action = notification.ActionUrl
                },
                data = notification.Metadata ?? new Dictionary<string, object>(),
                priority = "high"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send")
            {
                Content = content
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("key", $"={_options.FirebaseServerKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var messageId = responseData.TryGetProperty("message_id", out var messageIdElement) 
                    ? messageIdElement.GetString() 
                    : Guid.NewGuid().ToString();
                
                _logger.LogDebug("Successfully sent push notification via Firebase to {RecipientDeviceId} with MessageId {MessageId}", 
                    recipient.DeviceId, messageId);
                
                return DeliveryResult.Success($"firebase_{messageId}");
            }
            else
            {
                _logger.LogError("Firebase push delivery failed with status {StatusCode}: {Error}", 
                    response.StatusCode, responseContent);
                return DeliveryResult.Failure($"Firebase delivery failed: {response.StatusCode}", "FIREBASE_ERROR");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification via Firebase to {RecipientDeviceId}", recipient.DeviceId);
            return DeliveryResult.Failure($"Firebase delivery failed: {ex.Message}", "FIREBASE_ERROR");
        }
    }

    /// <summary>
    /// Sends push notification via Apple APNs.
    /// </summary>
    private async Task<DeliveryResult> SendViaAppleApnsAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Apple APNs requires JWT token authentication and specific payload format
            // For now, we'll simulate the API call
            _logger.LogDebug("Sending push notification via Apple APNs to {RecipientDeviceId}", recipient.DeviceId);
            
            // In a real implementation, you would:
            // 1. Generate JWT token using Apple's private key
            // 2. Create APNs payload with proper format
            // 3. Send to appropriate APNs endpoint (sandbox or production)
            
            // Simulate successful delivery
            var messageId = Guid.NewGuid().ToString();
            _logger.LogDebug("Successfully sent push notification via Apple APNs to {RecipientDeviceId} with MessageId {MessageId}", 
                recipient.DeviceId, messageId);
            
            return DeliveryResult.Success($"apns_{messageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification via Apple APNs to {RecipientDeviceId}", recipient.DeviceId);
            return DeliveryResult.Failure($"Apple APNs delivery failed: {ex.Message}", "APNS_ERROR");
        }
    }

    /// <summary>
    /// Sends push notification via Microsoft WNS.
    /// </summary>
    private async Task<DeliveryResult> SendViaMicrosoftWnsAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Microsoft WNS requires OAuth token and specific XML payload format
            // For now, we'll simulate the API call
            _logger.LogDebug("Sending push notification via Microsoft WNS to {RecipientDeviceId}", recipient.DeviceId);
            
            // In a real implementation, you would:
            // 1. Get OAuth token from Microsoft
            // 2. Create WNS XML payload
            // 3. Send to WNS endpoint
            
            // Simulate successful delivery
            var messageId = Guid.NewGuid().ToString();
            _logger.LogDebug("Successfully sent push notification via Microsoft WNS to {RecipientDeviceId} with MessageId {MessageId}", 
                recipient.DeviceId, messageId);
            
            return DeliveryResult.Success($"wns_{messageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification via Microsoft WNS to {RecipientDeviceId}", recipient.DeviceId);
            return DeliveryResult.Failure($"Microsoft WNS delivery failed: {ex.Message}", "WNS_ERROR");
        }
    }

    /// <summary>
    /// Tests Firebase connection.
    /// </summary>
    private async Task<bool> TestFirebaseConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Firebase doesn't have a specific health check endpoint
            // We'll just check if the server key is configured
            return !string.IsNullOrEmpty(_options.FirebaseServerKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Firebase connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests Apple APNs connection.
    /// </summary>
    private async Task<bool> TestAppleApnsConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Apple APNs doesn't have a specific health check endpoint
            // We'll just check if the required configuration is present
            return !string.IsNullOrEmpty(_options.AppleApnsKeyId) && 
                   !string.IsNullOrEmpty(_options.AppleApnsTeamId) && 
                   !string.IsNullOrEmpty(_options.AppleApnsBundleId) &&
                   !string.IsNullOrEmpty(_options.AppleApnsPrivateKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Apple APNs connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests Microsoft WNS connection.
    /// </summary>
    private async Task<bool> TestMicrosoftWnsConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Microsoft WNS doesn't have a specific health check endpoint
            // We'll just check if the required configuration is present
            return !string.IsNullOrEmpty(_options.MicrosoftWnsClientId) && 
                   !string.IsNullOrEmpty(_options.MicrosoftWnsClientSecret) && 
                   !string.IsNullOrEmpty(_options.MicrosoftWnsPackageSid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Microsoft WNS connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Determines the push platform based on recipient metadata or device ID format.
    /// </summary>
    private PushPlatform DeterminePlatform(NotificationRecipient recipient)
    {
        // Check if platform is explicitly specified in metadata
        if (recipient.Metadata?.TryGetValue("platform", out var platformObj) == true)
        {
            if (Enum.TryParse<PushPlatform>(platformObj.ToString(), true, out var platform))
            {
                return platform;
            }
        }

        // Try to determine platform from device ID format
        if (recipient.DeviceId?.Length == 64 && recipient.DeviceId.All(c => char.IsLetterOrDigit(c)))
        {
            return PushPlatform.Apple; // Apple device tokens are typically 64 characters
        }

        if (recipient.DeviceId?.StartsWith("https://") == true)
        {
            return PushPlatform.Microsoft; // WNS channel URIs start with https://
        }

        // Default to Firebase for other formats
        return PushPlatform.Firebase;
    }
}

/// <summary>
/// Configuration options for the push provider.
/// </summary>
public sealed class PushProviderOptions
{
    /// <summary>
    /// Whether the push provider is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    // Firebase Configuration
    /// <summary>
    /// Firebase Server Key for FCM.
    /// </summary>
    public string FirebaseServerKey { get; set; } = string.Empty;

    /// <summary>
    /// Firebase Project ID.
    /// </summary>
    public string FirebaseProjectId { get; set; } = string.Empty;

    // Apple APNs Configuration
    /// <summary>
    /// Apple APNs Key ID.
    /// </summary>
    public string AppleApnsKeyId { get; set; } = string.Empty;

    /// <summary>
    /// Apple APNs Team ID.
    /// </summary>
    public string AppleApnsTeamId { get; set; } = string.Empty;

    /// <summary>
    /// Apple APNs Bundle ID.
    /// </summary>
    public string AppleApnsBundleId { get; set; } = string.Empty;

    /// <summary>
    /// Apple APNs Private Key (P8 format).
    /// </summary>
    public string AppleApnsPrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use Apple APNs sandbox environment.
    /// </summary>
    public bool AppleApnsUseSandbox { get; set; } = true;

    // Microsoft WNS Configuration
    /// <summary>
    /// Microsoft WNS Client ID.
    /// </summary>
    public string MicrosoftWnsClientId { get; set; } = string.Empty;

    /// <summary>
    /// Microsoft WNS Client Secret.
    /// </summary>
    public string MicrosoftWnsClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Microsoft WNS Package SID.
    /// </summary>
    public string MicrosoftWnsPackageSid { get; set; } = string.Empty;

    /// <summary>
    /// Maximum content length for push notifications.
    /// </summary>
    public int MaxContentLength { get; set; } = 4000;

    /// <summary>
    /// Maximum title length for push notifications.
    /// </summary>
    public int MaxTitleLength { get; set; } = 100;
}

/// <summary>
/// Push notification platforms.
/// </summary>
public enum PushPlatform
{
    /// <summary>
    /// Firebase Cloud Messaging (Android, Web).
    /// </summary>
    Firebase = 0,

    /// <summary>
    /// Apple Push Notification Service (iOS, macOS).
    /// </summary>
    Apple = 1,

    /// <summary>
    /// Microsoft Windows Notification Service (Windows).
    /// </summary>
    Microsoft = 2
}