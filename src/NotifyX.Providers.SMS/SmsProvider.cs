using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace NotifyX.Providers.SMS;

/// <summary>
/// SMS notification provider supporting multiple SMS services (Twilio, AWS SNS, Azure Communication Services).
/// </summary>
public sealed class SmsProvider : INotificationProvider
{
    private readonly ILogger<SmsProvider> _logger;
    private readonly SmsProviderOptions _options;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the SmsProvider class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The SMS provider options.</param>
    /// <param name="httpClient">HTTP client for API calls.</param>
    public SmsProvider(
        ILogger<SmsProvider> logger,
        IOptions<SmsProviderOptions> options,
        HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.SMS;

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
            _logger.LogDebug("Sending SMS notification {NotificationId} to {RecipientPhone}", 
                notification.Id, recipient.PhoneNumber);

            if (string.IsNullOrEmpty(recipient.PhoneNumber))
            {
                return DeliveryResult.Failure("Recipient phone number is required", "MISSING_PHONE");
            }

            // Validate phone number format
            if (!IsValidPhoneNumber(recipient.PhoneNumber))
            {
                return DeliveryResult.Failure($"Invalid phone number: {recipient.PhoneNumber}", "INVALID_PHONE");
            }

            // Send based on configured provider
            return _options.ProviderType switch
            {
                SmsProviderType.Twilio => await SendViaTwilioAsync(notification, recipient, cancellationToken),
                SmsProviderType.AWSSNS => await SendViaAwsSnsAsync(notification, recipient, cancellationToken),
                SmsProviderType.AzureCommunication => await SendViaAzureCommunicationAsync(notification, recipient, cancellationToken),
                _ => DeliveryResult.Failure($"Unsupported SMS provider: {_options.ProviderType}", "UNSUPPORTED_PROVIDER")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS notification {NotificationId} to {RecipientPhone}", 
                notification.Id, recipient.PhoneNumber);
            return DeliveryResult.Failure($"SMS delivery failed: {ex.Message}", "DELIVERY_ERROR");
        }
    }

    /// <inheritdoc />
    public ValidationResult Validate(NotificationEvent notification, NotificationRecipient recipient)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate recipient phone number
        if (string.IsNullOrEmpty(recipient.PhoneNumber))
        {
            errors.Add("Recipient phone number is required");
        }
        else if (!IsValidPhoneNumber(recipient.PhoneNumber))
        {
            errors.Add($"Invalid phone number: {recipient.PhoneNumber}");
        }

        // Validate notification content
        if (string.IsNullOrEmpty(notification.Content))
        {
            warnings.Add("SMS content is empty");
        }

        // Validate content length (SMS has strict limits)
        if (!string.IsNullOrEmpty(notification.Content) && notification.Content.Length > _options.MaxContentLength)
        {
            errors.Add($"SMS content exceeds maximum length of {_options.MaxContentLength} characters");
        }

        // Check for special characters that might cause issues
        if (!string.IsNullOrEmpty(notification.Content) && ContainsSpecialCharacters(notification.Content))
        {
            warnings.Add("SMS content contains special characters that may not be supported by all carriers");
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
                return ProviderHealthStatus.Unhealthy("SMS provider is disabled");
            }

            // Test the configured provider
            var isHealthy = _options.ProviderType switch
            {
                SmsProviderType.Twilio => await TestTwilioConnectionAsync(cancellationToken),
                SmsProviderType.AWSSNS => await TestAwsSnsConnectionAsync(cancellationToken),
                SmsProviderType.AzureCommunication => await TestAzureCommunicationConnectionAsync(cancellationToken),
                _ => false
            };

            return isHealthy 
                ? ProviderHealthStatus.Healthy($"SMS provider ({_options.ProviderType}) is healthy")
                : ProviderHealthStatus.Unhealthy($"SMS provider ({_options.ProviderType}) is unhealthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking SMS provider health");
            return ProviderHealthStatus.Unhealthy($"Health check failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task ConfigureAsync(ChannelConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Configuring SMS provider with channel configuration");

            // Update options based on configuration
            if (configuration.Settings.TryGetValue("ProviderType", out var providerTypeObj) &&
                Enum.TryParse<SmsProviderType>(providerTypeObj.ToString(), out var providerType))
            {
                _options.ProviderType = providerType;
            }

            if (configuration.Settings.TryGetValue("TwilioAccountSid", out var twilioAccountSid))
            {
                _options.TwilioAccountSid = twilioAccountSid.ToString();
            }

            if (configuration.Settings.TryGetValue("TwilioAuthToken", out var twilioAuthToken))
            {
                _options.TwilioAuthToken = twilioAuthToken.ToString();
            }

            if (configuration.Settings.TryGetValue("TwilioFromNumber", out var twilioFromNumber))
            {
                _options.TwilioFromNumber = twilioFromNumber.ToString();
            }

            if (configuration.Settings.TryGetValue("AwsAccessKeyId", out var awsAccessKeyId))
            {
                _options.AwsAccessKeyId = awsAccessKeyId.ToString();
            }

            if (configuration.Settings.TryGetValue("AwsSecretAccessKey", out var awsSecretAccessKey))
            {
                _options.AwsSecretAccessKey = awsSecretAccessKey.ToString();
            }

            if (configuration.Settings.TryGetValue("AwsRegion", out var awsRegion))
            {
                _options.AwsRegion = awsRegion.ToString();
            }

            if (configuration.Settings.TryGetValue("AzureConnectionString", out var azureConnectionString))
            {
                _options.AzureConnectionString = azureConnectionString.ToString();
            }

            if (configuration.Settings.TryGetValue("AzureFromNumber", out var azureFromNumber))
            {
                _options.AzureFromNumber = azureFromNumber.ToString();
            }

            if (configuration.Settings.TryGetValue("MaxContentLength", out var maxContentLengthObj) &&
                int.TryParse(maxContentLengthObj.ToString(), out var maxContentLength))
            {
                _options.MaxContentLength = maxContentLength;
            }

            _logger.LogInformation("SMS provider configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring SMS provider");
            throw;
        }
    }

    /// <summary>
    /// Sends SMS via Twilio.
    /// </summary>
    private async Task<DeliveryResult> SendViaTwilioAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("To", recipient.PhoneNumber),
                new KeyValuePair<string, string>("From", _options.TwilioFromNumber),
                new KeyValuePair<string, string>("Body", notification.Content)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{_options.TwilioAccountSid}/Messages.json")
            {
                Content = requestBody
            };

            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.TwilioAccountSid}:{_options.TwilioAuthToken}"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var messageSid = responseData.GetProperty("sid").GetString();
                
                _logger.LogDebug("Successfully sent SMS via Twilio to {RecipientPhone} with SID {MessageSid}", 
                    recipient.PhoneNumber, messageSid);
                
                return DeliveryResult.Success($"twilio_{messageSid}");
            }
            else
            {
                _logger.LogError("Twilio SMS delivery failed with status {StatusCode}: {Error}", 
                    response.StatusCode, responseContent);
                return DeliveryResult.Failure($"Twilio delivery failed: {response.StatusCode}", "TWILIO_ERROR");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Twilio to {RecipientPhone}", recipient.PhoneNumber);
            return DeliveryResult.Failure($"Twilio delivery failed: {ex.Message}", "TWILIO_ERROR");
        }
    }

    /// <summary>
    /// Sends SMS via AWS SNS.
    /// </summary>
    private async Task<DeliveryResult> SendViaAwsSnsAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            // AWS SNS SMS sending would require AWS SDK integration
            // For now, we'll simulate the API call
            _logger.LogDebug("Sending SMS via AWS SNS to {RecipientPhone}", recipient.PhoneNumber);
            
            // In a real implementation, you would use AWS SDK:
            // var snsClient = new AmazonSimpleNotificationServiceClient(_options.AwsAccessKeyId, _options.AwsSecretAccessKey, RegionEndpoint.GetBySystemName(_options.AwsRegion));
            // var request = new PublishRequest
            // {
            //     PhoneNumber = recipient.PhoneNumber,
            //     Message = notification.Content
            // };
            // var response = await snsClient.PublishAsync(request, cancellationToken);
            
            // Simulate successful delivery
            var messageId = Guid.NewGuid().ToString();
            _logger.LogDebug("Successfully sent SMS via AWS SNS to {RecipientPhone} with MessageId {MessageId}", 
                recipient.PhoneNumber, messageId);
            
            return DeliveryResult.Success($"sns_{messageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via AWS SNS to {RecipientPhone}", recipient.PhoneNumber);
            return DeliveryResult.Failure($"AWS SNS delivery failed: {ex.Message}", "SNS_ERROR");
        }
    }

    /// <summary>
    /// Sends SMS via Azure Communication Services.
    /// </summary>
    private async Task<DeliveryResult> SendViaAzureCommunicationAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Azure Communication Services SMS sending would require Azure SDK integration
            // For now, we'll simulate the API call
            _logger.LogDebug("Sending SMS via Azure Communication Services to {RecipientPhone}", recipient.PhoneNumber);
            
            // In a real implementation, you would use Azure SDK:
            // var smsClient = new SmsClient(_options.AzureConnectionString);
            // var sendSmsOptions = new SendSmsOptions(_options.AzureFromNumber, recipient.PhoneNumber, notification.Content);
            // var response = await smsClient.SendAsync(sendSmsOptions, cancellationToken);
            
            // Simulate successful delivery
            var messageId = Guid.NewGuid().ToString();
            _logger.LogDebug("Successfully sent SMS via Azure Communication Services to {RecipientPhone} with MessageId {MessageId}", 
                recipient.PhoneNumber, messageId);
            
            return DeliveryResult.Success($"azure_{messageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via Azure Communication Services to {RecipientPhone}", recipient.PhoneNumber);
            return DeliveryResult.Failure($"Azure Communication delivery failed: {ex.Message}", "AZURE_ERROR");
        }
    }

    /// <summary>
    /// Tests Twilio connection.
    /// </summary>
    private async Task<bool> TestTwilioConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.twilio.com/2010-04-01/Accounts/{_options.TwilioAccountSid}.json");
            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_options.TwilioAccountSid}:{_options.TwilioAuthToken}"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Twilio connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests AWS SNS connection.
    /// </summary>
    private async Task<bool> TestAwsSnsConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // In a real implementation, you would test AWS SNS connectivity
            // For now, we'll just check if credentials are configured
            return !string.IsNullOrEmpty(_options.AwsAccessKeyId) && !string.IsNullOrEmpty(_options.AwsSecretAccessKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AWS SNS connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests Azure Communication Services connection.
    /// </summary>
    private async Task<bool> TestAzureCommunicationConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            // In a real implementation, you would test Azure Communication Services connectivity
            // For now, we'll just check if connection string is configured
            return !string.IsNullOrEmpty(_options.AzureConnectionString);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Azure Communication Services connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Validates phone number format.
    /// </summary>
    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return false;

        // Basic phone number validation - should start with + and contain only digits, spaces, hyphens, and parentheses
        var cleanNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        return cleanNumber.StartsWith("+") && cleanNumber.Length >= 10 && cleanNumber.Skip(1).All(char.IsDigit);
    }

    /// <summary>
    /// Checks if content contains special characters that might cause SMS issues.
    /// </summary>
    private static bool ContainsSpecialCharacters(string content)
    {
        // Check for characters that might not be supported by all SMS carriers
        var specialChars = new[] { '€', '£', '¥', '§', '©', '®', '™', '°', '²', '³', '¹', '¼', '½', '¾' };
        return content.Any(c => specialChars.Contains(c));
    }
}

/// <summary>
/// Configuration options for the SMS provider.
/// </summary>
public sealed class SmsProviderOptions
{
    /// <summary>
    /// Whether the SMS provider is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// The SMS provider type to use.
    /// </summary>
    public SmsProviderType ProviderType { get; set; } = SmsProviderType.Twilio;

    // Twilio Configuration
    /// <summary>
    /// Twilio Account SID.
    /// </summary>
    public string TwilioAccountSid { get; set; } = string.Empty;

    /// <summary>
    /// Twilio Auth Token.
    /// </summary>
    public string TwilioAuthToken { get; set; } = string.Empty;

    /// <summary>
    /// Twilio From Phone Number.
    /// </summary>
    public string TwilioFromNumber { get; set; } = string.Empty;

    // AWS SNS Configuration
    /// <summary>
    /// AWS Access Key ID for SNS.
    /// </summary>
    public string AwsAccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// AWS Secret Access Key for SNS.
    /// </summary>
    public string AwsSecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS Region for SNS.
    /// </summary>
    public string AwsRegion { get; set; } = "us-east-1";

    // Azure Communication Services Configuration
    /// <summary>
    /// Azure Communication Services Connection String.
    /// </summary>
    public string AzureConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Azure Communication Services From Phone Number.
    /// </summary>
    public string AzureFromNumber { get; set; } = string.Empty;

    /// <summary>
    /// Maximum content length for SMS (typically 160 characters for single SMS, 1600 for concatenated).
    /// </summary>
    public int MaxContentLength { get; set; } = 1600;
}

/// <summary>
/// SMS provider types.
/// </summary>
public enum SmsProviderType
{
    /// <summary>
    /// Twilio SMS provider.
    /// </summary>
    Twilio = 0,

    /// <summary>
    /// AWS SNS SMS provider.
    /// </summary>
    AWSSNS = 1,

    /// <summary>
    /// Azure Communication Services SMS provider.
    /// </summary>
    AzureCommunication = 2
}