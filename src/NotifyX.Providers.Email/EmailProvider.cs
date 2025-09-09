using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using SendGridContent = SendGrid.Helpers.Mail.Content;
using AwsContent = Amazon.SimpleEmail.Model.Content;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
using MailSmtpClient = System.Net.Mail.SmtpClient;

namespace NotifyX.Providers.Email;

/// <summary>
/// Email notification provider supporting multiple email services (SMTP, SendGrid, AWS SES).
/// </summary>
public sealed class EmailProvider : INotificationProvider
{
    private readonly ILogger<EmailProvider> _logger;
    private readonly EmailProviderOptions _options;
    private readonly IAmazonSimpleEmailService? _sesClient;
    private readonly ISendGridClient? _sendGridClient;

    /// <summary>
    /// Initializes a new instance of the EmailProvider class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The email provider options.</param>
    /// <param name="sesClient">Optional AWS SES client.</param>
    /// <param name="sendGridClient">Optional SendGrid client.</param>
    public EmailProvider(
        ILogger<EmailProvider> logger,
        IOptions<EmailProviderOptions> options,
        IAmazonSimpleEmailService? sesClient = null,
        ISendGridClient? sendGridClient = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _sesClient = sesClient;
        _sendGridClient = sendGridClient;
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.Email;

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
            _logger.LogDebug("Sending email notification {NotificationId} to {RecipientEmail}", 
                notification.Id, recipient.Email);

            if (string.IsNullOrEmpty(recipient.Email))
            {
                return DeliveryResult.Failure("Recipient email address is required", "MISSING_EMAIL");
            }

            // Validate email address format
            if (!IsValidEmail(recipient.Email))
            {
                return DeliveryResult.Failure($"Invalid email address: {recipient.Email}", "INVALID_EMAIL");
            }

            // Send based on configured provider
            return _options.ProviderType switch
            {
                EmailProviderType.SMTP => await SendViaSmtpAsync(notification, recipient, cancellationToken),
                EmailProviderType.SendGrid => await SendViaSendGridAsync(notification, recipient, cancellationToken),
                EmailProviderType.AWSSES => await SendViaAwsSesAsync(notification, recipient, cancellationToken),
                _ => DeliveryResult.Failure($"Unsupported email provider: {_options.ProviderType}", "UNSUPPORTED_PROVIDER")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notification {NotificationId} to {RecipientEmail}", 
                notification.Id, recipient.Email);
            return DeliveryResult.Failure($"Email delivery failed: {ex.Message}", "DELIVERY_ERROR");
        }
    }

    /// <inheritdoc />
    public ValidationResult Validate(NotificationEvent notification, NotificationRecipient recipient)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate recipient email
        if (string.IsNullOrEmpty(recipient.Email))
        {
            errors.Add("Recipient email address is required");
        }
        else if (!IsValidEmail(recipient.Email))
        {
            errors.Add($"Invalid email address: {recipient.Email}");
        }

        // Validate notification content
        if (string.IsNullOrEmpty(notification.Subject))
        {
            warnings.Add("Email subject is empty");
        }

        if (string.IsNullOrEmpty(notification.Content))
        {
            warnings.Add("Email content is empty");
        }

        // Validate content length
        if (!string.IsNullOrEmpty(notification.Content) && notification.Content.Length > _options.MaxContentLength)
        {
            errors.Add($"Email content exceeds maximum length of {_options.MaxContentLength} characters");
        }

        // Validate subject length
        if (!string.IsNullOrEmpty(notification.Subject) && notification.Subject.Length > _options.MaxSubjectLength)
        {
            errors.Add($"Email subject exceeds maximum length of {_options.MaxSubjectLength} characters");
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
                return ProviderHealthStatus.Unhealthy("Email provider is disabled");
            }

            // Test the configured provider
            var isHealthy = _options.ProviderType switch
            {
                EmailProviderType.SMTP => await TestSmtpConnectionAsync(cancellationToken),
                EmailProviderType.SendGrid => await TestSendGridConnectionAsync(cancellationToken),
                EmailProviderType.AWSSES => await TestAwsSesConnectionAsync(cancellationToken),
                _ => false
            };

            return isHealthy 
                ? ProviderHealthStatus.Healthy($"Email provider ({_options.ProviderType}) is healthy")
                : ProviderHealthStatus.Unhealthy($"Email provider ({_options.ProviderType}) is unhealthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email provider health");
            return ProviderHealthStatus.Unhealthy($"Health check failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task ConfigureAsync(ChannelConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Configuring email provider with channel configuration");

            // Update options based on configuration
            if (configuration.Settings.TryGetValue("ProviderType", out var providerTypeObj) &&
                Enum.TryParse<EmailProviderType>(providerTypeObj.ToString(), out var providerType))
            {
                _options.ProviderType = providerType;
            }

            if (configuration.Settings.TryGetValue("SmtpHost", out var smtpHost))
            {
                _options.SmtpHost = smtpHost.ToString();
            }

            if (configuration.Settings.TryGetValue("SmtpPort", out var smtpPortObj) &&
                int.TryParse(smtpPortObj.ToString(), out var smtpPort))
            {
                _options.SmtpPort = smtpPort;
            }

            if (configuration.Settings.TryGetValue("SmtpUsername", out var smtpUsername))
            {
                _options.SmtpUsername = smtpUsername.ToString();
            }

            if (configuration.Settings.TryGetValue("SmtpPassword", out var smtpPassword))
            {
                _options.SmtpPassword = smtpPassword.ToString();
            }

            if (configuration.Settings.TryGetValue("SendGridApiKey", out var sendGridApiKey))
            {
                _options.SendGridApiKey = sendGridApiKey.ToString();
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

            if (configuration.Settings.TryGetValue("FromEmail", out var fromEmail))
            {
                _options.FromEmail = fromEmail.ToString();
            }

            if (configuration.Settings.TryGetValue("FromName", out var fromName))
            {
                _options.FromName = fromName.ToString();
            }

            if (configuration.Settings.TryGetValue("MaxContentLength", out var maxContentLengthObj) &&
                int.TryParse(maxContentLengthObj.ToString(), out var maxContentLength))
            {
                _options.MaxContentLength = maxContentLength;
            }

            if (configuration.Settings.TryGetValue("MaxSubjectLength", out var maxSubjectLengthObj) &&
                int.TryParse(maxSubjectLengthObj.ToString(), out var maxSubjectLength))
            {
                _options.MaxSubjectLength = maxSubjectLength;
            }

            _logger.LogInformation("Email provider configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring email provider");
            throw;
        }
    }

    /// <summary>
    /// Sends email via SMTP.
    /// </summary>
    private async Task<DeliveryResult> SendViaSmtpAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient();
            
            // Configure SMTP client
            if (_options.SmtpEnableSsl)
            {
                await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.SslOnConnect, cancellationToken);
            }
            else
            {
                await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.None, cancellationToken);
            }

            if (!string.IsNullOrEmpty(_options.SmtpUsername) && !string.IsNullOrEmpty(_options.SmtpPassword))
            {
                await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword, cancellationToken);
            }

            // Create message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(new MailboxAddress(recipient.Name, recipient.Email));
            message.Subject = notification.Subject;

            var bodyBuilder = new BodyBuilder();
            if (IsHtmlContent(notification.Content))
            {
                bodyBuilder.HtmlBody = notification.Content;
            }
            else
            {
                bodyBuilder.TextBody = notification.Content;
            }

            message.Body = bodyBuilder.ToMessageBody();

            // Send message
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogDebug("Successfully sent email via SMTP to {RecipientEmail}", recipient.Email);
            return DeliveryResult.Success($"smtp_{Guid.NewGuid()}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via SMTP to {RecipientEmail}", recipient.Email);
            return DeliveryResult.Failure($"SMTP delivery failed: {ex.Message}", "SMTP_ERROR");
        }
    }

    /// <summary>
    /// Sends email via SendGrid.
    /// </summary>
    private async Task<DeliveryResult> SendViaSendGridAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (_sendGridClient == null)
            {
                return DeliveryResult.Failure("SendGrid client is not configured", "SENDGRID_NOT_CONFIGURED");
            }

            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var to = new EmailAddress(recipient.Email, recipient.Name);
            
            var message = new SendGridMessage
            {
                From = from,
                Subject = notification.Subject
            };

            message.AddTo(to);

            if (IsHtmlContent(notification.Content))
            {
                message.HtmlContent = notification.Content;
            }
            else
            {
                message.PlainTextContent = notification.Content;
            }

            // Add custom headers if needed
            if (!string.IsNullOrEmpty(notification.CorrelationId))
            {
                message.AddHeader("X-Correlation-ID", notification.CorrelationId);
            }

            if (!string.IsNullOrEmpty(notification.Source))
            {
                message.AddHeader("X-Source", notification.Source);
            }

            var response = await _sendGridClient.SendEmailAsync(message, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Successfully sent email via SendGrid to {RecipientEmail}", recipient.Email);
                return DeliveryResult.Success($"sendgrid_{Guid.NewGuid()}");
            }
            else
            {
                var errorContent = await response.Body.ReadAsStringAsync();
                _logger.LogError("SendGrid delivery failed with status {StatusCode}: {Error}", 
                    response.StatusCode, errorContent);
                return DeliveryResult.Failure($"SendGrid delivery failed: {response.StatusCode}", "SENDGRID_ERROR");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via SendGrid to {RecipientEmail}", recipient.Email);
            return DeliveryResult.Failure($"SendGrid delivery failed: {ex.Message}", "SENDGRID_ERROR");
        }
    }

    /// <summary>
    /// Sends email via AWS SES.
    /// </summary>
    private async Task<DeliveryResult> SendViaAwsSesAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (_sesClient == null)
            {
                return DeliveryResult.Failure("AWS SES client is not configured", "SES_NOT_CONFIGURED");
            }

            var request = new SendEmailRequest
            {
                Source = $"{_options.FromName} <{_options.FromEmail}>",
                Destination = new Destination
                {
                    ToAddresses = new List<string> { recipient.Email }
                },
                Message = new Message
                {
                    Subject = new AwsContent(notification.Subject),
                    Body = new Body()
                }
            };

            if (IsHtmlContent(notification.Content))
            {
                request.Message.Body.Html = new AwsContent(notification.Content);
            }
            else
            {
                request.Message.Body.Text = new AwsContent(notification.Content);
            }

            var response = await _sesClient.SendEmailAsync(request, cancellationToken);

            _logger.LogDebug("Successfully sent email via AWS SES to {RecipientEmail} with message ID {MessageId}", 
                recipient.Email, response.MessageId);
            
            return DeliveryResult.Success($"ses_{response.MessageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via AWS SES to {RecipientEmail}", recipient.Email);
            return DeliveryResult.Failure($"AWS SES delivery failed: {ex.Message}", "SES_ERROR");
        }
    }

    /// <summary>
    /// Tests SMTP connection.
    /// </summary>
    private async Task<bool> TestSmtpConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient();
            
            if (_options.SmtpEnableSsl)
            {
                await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.SslOnConnect, cancellationToken);
            }
            else
            {
                await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.None, cancellationToken);
            }

            if (!string.IsNullOrEmpty(_options.SmtpUsername) && !string.IsNullOrEmpty(_options.SmtpPassword))
            {
                await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword, cancellationToken);
            }

            await client.DisconnectAsync(true, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests SendGrid connection.
    /// </summary>
    private async Task<bool> TestSendGridConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_sendGridClient == null)
            {
                return false;
            }

            // SendGrid doesn't have a specific health check endpoint, so we'll just check if the client is configured
            return !string.IsNullOrEmpty(_options.SendGridApiKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SendGrid connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Tests AWS SES connection.
    /// </summary>
    private async Task<bool> TestAwsSesConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_sesClient == null)
            {
                return false;
            }

            // Test by getting send quota
            var request = new GetSendQuotaRequest();
            await _sesClient.GetSendQuotaAsync(request, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AWS SES connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Validates email address format.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Determines if content is HTML.
    /// </summary>
    private static bool IsHtmlContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        // Simple check for HTML tags
        return content.Contains("<") && content.Contains(">");
    }
}

/// <summary>
/// Configuration options for the email provider.
/// </summary>
public sealed class EmailProviderOptions
{
    /// <summary>
    /// Whether the email provider is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// The email provider type to use.
    /// </summary>
    public EmailProviderType ProviderType { get; set; } = EmailProviderType.SMTP;

    /// <summary>
    /// SMTP host for SMTP provider.
    /// </summary>
    public string SmtpHost { get; set; } = "localhost";

    /// <summary>
    /// SMTP port for SMTP provider.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// SMTP username for authentication.
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// SMTP password for authentication.
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Whether to enable SSL for SMTP.
    /// </summary>
    public bool SmtpEnableSsl { get; set; } = true;

    /// <summary>
    /// SendGrid API key.
    /// </summary>
    public string SendGridApiKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS access key ID for SES.
    /// </summary>
    public string AwsAccessKeyId { get; set; } = string.Empty;

    /// <summary>
    /// AWS secret access key for SES.
    /// </summary>
    public string AwsSecretAccessKey { get; set; } = string.Empty;

    /// <summary>
    /// AWS region for SES.
    /// </summary>
    public string AwsRegion { get; set; } = "us-east-1";

    /// <summary>
    /// From email address.
    /// </summary>
    public string FromEmail { get; set; } = "noreply@example.com";

    /// <summary>
    /// From name.
    /// </summary>
    public string FromName { get; set; } = "NotifyX";

    /// <summary>
    /// Maximum content length.
    /// </summary>
    public int MaxContentLength { get; set; } = 1000000; // 1MB

    /// <summary>
    /// Maximum subject length.
    /// </summary>
    public int MaxSubjectLength { get; set; } = 998; // RFC 5322 limit
}

/// <summary>
/// Email provider types.
/// </summary>
public enum EmailProviderType
{
    /// <summary>
    /// SMTP provider.
    /// </summary>
    SMTP = 0,

    /// <summary>
    /// SendGrid provider.
    /// </summary>
    SendGrid = 1,

    /// <summary>
    /// AWS SES provider.
    /// </summary>
    AWSSES = 2
}