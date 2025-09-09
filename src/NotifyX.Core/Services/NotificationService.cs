using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Collections.Concurrent;

namespace NotifyX.Core.Services;

/// <summary>
/// Default implementation of the notification service.
/// Orchestrates the notification pipeline including rule evaluation, template rendering, and delivery.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;
    private readonly IRuleEngine _ruleEngine;
    private readonly ITemplateService _templateService;
    private readonly IEnumerable<INotificationProvider> _providers;
    private readonly ConcurrentDictionary<string, NotificationStatus> _notificationStatuses = new();
    private readonly ConcurrentDictionary<string, List<DeliveryAttempt>> _deliveryHistory = new();

    /// <summary>
    /// Initializes a new instance of the NotificationService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ruleEngine">The rule engine.</param>
    /// <param name="templateService">The template service.</param>
    /// <param name="providers">The notification providers.</param>
    public NotificationService(
        ILogger<NotificationService> logger,
        IRuleEngine ruleEngine,
        ITemplateService templateService,
        IEnumerable<INotificationProvider> providers)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
    }

    /// <inheritdoc />
    public async Task<NotificationResult> SendAsync(NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing notification {NotificationId} of type {EventType} for tenant {TenantId}", 
                notification.Id, notification.EventType, notification.TenantId);

            // Initialize notification status
            var status = new NotificationStatus
            {
                NotificationId = notification.Id,
                State = NotificationState.Processing,
                Progress = 0,
                CreatedAt = notification.CreatedAt,
                LastUpdatedAt = DateTime.UtcNow
            };
            _notificationStatuses[notification.Id] = status;

            // Step 1: Evaluate rules and process workflow
            _logger.LogDebug("Evaluating rules for notification {NotificationId}", notification.Id);
            var ruleResult = await _ruleEngine.EvaluateRulesAsync(notification, cancellationToken);
            
            if (!ruleResult.IsSuccess)
            {
                status.State = NotificationState.Failed;
                status.ErrorMessage = ruleResult.ErrorMessage;
                status.LastUpdatedAt = DateTime.UtcNow;
                
                _logger.LogError("Rule evaluation failed for notification {NotificationId}: {Error}", 
                    notification.Id, ruleResult.ErrorMessage);
                
                return NotificationResult.Failure(notification.Id, ruleResult.ErrorMessage ?? "Rule evaluation failed");
            }

            // Process workflow if rules matched
            var processedNotification = notification;
            if (ruleResult.MatchedRules.Any())
            {
                _logger.LogDebug("Processing workflow for notification {NotificationId} with {RuleCount} matched rules", 
                    notification.Id, ruleResult.MatchedRules.Count);
                
                var workflowResult = await _ruleEngine.ProcessWorkflowAsync(processedNotification, ruleResult.MatchedRules, cancellationToken);
                
                if (!workflowResult.IsSuccess)
                {
                    status.State = NotificationState.Failed;
                    status.ErrorMessage = workflowResult.ErrorMessage;
                    status.LastUpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogError("Workflow processing failed for notification {NotificationId}: {Error}", 
                        notification.Id, workflowResult.ErrorMessage);
                    
                    return NotificationResult.Failure(notification.Id, workflowResult.ErrorMessage ?? "Workflow processing failed");
                }

                // Use modified notification from workflow
                if (workflowResult.ModifiedNotification != null)
                {
                    processedNotification = workflowResult.ModifiedNotification;
                }
            }

            status.Progress = 25;
            status.LastUpdatedAt = DateTime.UtcNow;

            // Step 2: Render templates if needed
            if (!string.IsNullOrEmpty(processedNotification.TemplateId))
            {
                _logger.LogDebug("Rendering template {TemplateId} for notification {NotificationId}", 
                    processedNotification.TemplateId, notification.Id);
                
                var templateResult = await _templateService.RenderAsync(processedNotification, processedNotification.TemplateId, cancellationToken);
                
                if (!templateResult.IsSuccess)
                {
                    status.State = NotificationState.Failed;
                    status.ErrorMessage = templateResult.ErrorMessage;
                    status.LastUpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogError("Template rendering failed for notification {NotificationId}: {Error}", 
                        notification.Id, templateResult.ErrorMessage);
                    
                    return NotificationResult.Failure(notification.Id, templateResult.ErrorMessage ?? "Template rendering failed");
                }

                // Update notification with rendered content
                processedNotification = processedNotification.With(builder =>
                {
                    if (!string.IsNullOrEmpty(templateResult.RenderedSubject))
                    {
                        builder.WithSubject(templateResult.RenderedSubject);
                    }
                    if (!string.IsNullOrEmpty(templateResult.RenderedContent))
                    {
                        builder.WithContent(templateResult.RenderedContent);
                    }
                });
            }

            status.Progress = 50;
            status.LastUpdatedAt = DateTime.UtcNow;

            // Step 3: Deliver notifications
            _logger.LogDebug("Delivering notification {NotificationId} to {RecipientCount} recipients", 
                notification.Id, processedNotification.Recipients.Count);
            
            var deliveryResults = new List<RecipientDeliveryResult>();
            var deliveryAttempts = new List<DeliveryAttempt>();

            foreach (var recipient in processedNotification.Recipients)
            {
                try
                {
                    var recipientResult = await DeliverToRecipientAsync(processedNotification, recipient, cancellationToken);
                    deliveryResults.Add(recipientResult);
                    
                    // Record delivery attempt
                    var attempt = new DeliveryAttempt
                    {
                        NotificationId = notification.Id,
                        RecipientId = recipient.Id,
                        Channel = recipientResult.Channel,
                        IsSuccess = recipientResult.IsSuccess,
                        DeliveryId = recipientResult.DeliveryId,
                        ErrorMessage = recipientResult.ErrorMessage,
                        AttemptedAt = recipientResult.AttemptedAt,
                        CompletedAt = recipientResult.CompletedAt
                    };
                    deliveryAttempts.Add(attempt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error delivering notification {NotificationId} to recipient {RecipientId}", 
                        notification.Id, recipient.Id);
                    
                    var failedResult = new RecipientDeliveryResult
                    {
                        RecipientId = recipient.Id,
                        Channel = NotificationChannel.Email, // Default channel
                        IsSuccess = false,
                        DeliveryId = Guid.NewGuid().ToString(),
                        ErrorMessage = ex.Message,
                        AttemptedAt = DateTime.UtcNow
                    };
                    deliveryResults.Add(failedResult);
                }
            }

            // Store delivery history
            _deliveryHistory[notification.Id] = deliveryAttempts;

            // Update status
            status.AttemptCount = deliveryAttempts.Count;
            status.SuccessCount = deliveryAttempts.Count(a => a.IsSuccess);
            status.FailureCount = deliveryAttempts.Count(a => !a.IsSuccess);
            status.Progress = 100;
            status.LastUpdatedAt = DateTime.UtcNow;

            if (status.SuccessCount > 0)
            {
                status.State = NotificationState.Delivered;
                status.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                status.State = NotificationState.Failed;
                status.ErrorMessage = "All delivery attempts failed";
            }

            _logger.LogInformation("Notification {NotificationId} processing completed. Success: {SuccessCount}, Failed: {FailureCount}", 
                notification.Id, status.SuccessCount, status.FailureCount);

            return NotificationResult.Success(notification.Id, deliveryResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification {NotificationId}", notification.Id);
            
            // Update status to failed
            if (_notificationStatuses.TryGetValue(notification.Id, out var status))
            {
                status.State = NotificationState.Failed;
                status.ErrorMessage = ex.Message;
                status.LastUpdatedAt = DateTime.UtcNow;
            }
            
            return NotificationResult.Failure(notification.Id, $"Notification processing failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<BatchNotificationResult> SendBatchAsync(IEnumerable<NotificationEvent> notifications, CancellationToken cancellationToken = default)
    {
        try
        {
            var notificationsList = notifications.ToList();
            _logger.LogInformation("Processing batch of {Count} notifications", notificationsList.Count);

            var results = new List<NotificationResult>();
            var successCount = 0;
            var failureCount = 0;

            // Process notifications in parallel (with some concurrency limit)
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = notificationsList.Select(async notification =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await SendAsync(notification, cancellationToken);
                    results.Add(result);
                    
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failureCount);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            var batchStatus = failureCount == 0 ? BatchStatus.AllSuccessful :
                             successCount == 0 ? BatchStatus.AllFailed :
                             BatchStatus.PartialFailure;

            _logger.LogInformation("Batch processing completed. Total: {Total}, Success: {Success}, Failed: {Failed}", 
                notificationsList.Count, successCount, failureCount);

            return new BatchNotificationResult
            {
                TotalCount = notificationsList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results,
                Status = batchStatus,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch notifications");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<NotificationResult> ScheduleAsync(NotificationEvent notification, DateTime scheduledFor, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Scheduling notification {NotificationId} for {ScheduledFor}", 
                notification.Id, scheduledFor);

            // Update notification with scheduled time
            var scheduledNotification = notification.With(builder => builder.WithScheduledFor(scheduledFor));

            // For now, we'll store it and process it immediately
            // In a real implementation, you'd use a job scheduler like Hangfire or Quartz
            if (scheduledFor <= DateTime.UtcNow)
            {
                return await SendAsync(scheduledNotification, cancellationToken);
            }
            else
            {
                // Store for later processing
                var status = new NotificationStatus
                {
                    NotificationId = notification.Id,
                    State = NotificationState.Scheduled,
                    CreatedAt = notification.CreatedAt,
                    LastUpdatedAt = DateTime.UtcNow
                };
                _notificationStatuses[notification.Id] = status;

                _logger.LogInformation("Notification {NotificationId} scheduled for {ScheduledFor}", 
                    notification.Id, scheduledFor);

                return NotificationResult.Success(notification.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling notification {NotificationId}", notification.Id);
            return NotificationResult.Failure(notification.Id, $"Scheduling failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<bool> CancelAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cancelling notification {NotificationId}", notificationId);

            if (_notificationStatuses.TryGetValue(notificationId, out var status))
            {
                if (status.State == NotificationState.Pending || status.State == NotificationState.Scheduled)
                {
                    status.State = NotificationState.Cancelled;
                    status.LastUpdatedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("Successfully cancelled notification {NotificationId}", notificationId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Cannot cancel notification {NotificationId} in state {State}", 
                        notificationId, status.State);
                    return false;
                }
            }
            else
            {
                _logger.LogWarning("Notification {NotificationId} not found for cancellation", notificationId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling notification {NotificationId}", notificationId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<NotificationStatus?> GetStatusAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting status for notification {NotificationId}", notificationId);

            if (_notificationStatuses.TryGetValue(notificationId, out var status))
            {
                return status;
            }

            _logger.LogWarning("Status not found for notification {NotificationId}", notificationId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for notification {NotificationId}", notificationId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DeliveryAttempt>> GetDeliveryHistoryAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting delivery history for notification {NotificationId}", notificationId);

            if (_deliveryHistory.TryGetValue(notificationId, out var history))
            {
                return history;
            }

            _logger.LogWarning("Delivery history not found for notification {NotificationId}", notificationId);
            return Enumerable.Empty<DeliveryAttempt>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting delivery history for notification {NotificationId}", notificationId);
            return Enumerable.Empty<DeliveryAttempt>();
        }
    }

    /// <inheritdoc />
    public async Task<NotificationResult> RetryAsync(string notificationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrying notification {NotificationId}", notificationId);

            if (!_notificationStatuses.TryGetValue(notificationId, out var status))
            {
                _logger.LogWarning("Notification {NotificationId} not found for retry", notificationId);
                return NotificationResult.Failure(notificationId, "Notification not found");
            }

            if (status.State != NotificationState.Failed)
            {
                _logger.LogWarning("Cannot retry notification {NotificationId} in state {State}", 
                    notificationId, status.State);
                return NotificationResult.Failure(notificationId, $"Cannot retry notification in state {status.State}");
            }

            // Get delivery history to determine what to retry
            var history = await GetDeliveryHistoryAsync(notificationId, cancellationToken);
            var failedAttempts = history.Where(a => !a.IsSuccess).ToList();

            if (!failedAttempts.Any())
            {
                _logger.LogWarning("No failed attempts found for notification {NotificationId}", notificationId);
                return NotificationResult.Failure(notificationId, "No failed attempts to retry");
            }

            // For now, we'll create a new notification based on the original
            // In a real implementation, you'd store the original notification
            var retryNotification = new NotificationEvent
            {
                Id = Guid.NewGuid().ToString(), // New ID for retry
                TenantId = status.NotificationId, // Using notification ID as tenant ID for demo
                EventType = "retry",
                Subject = "Retry Notification",
                Content = "This is a retry notification",
                Recipients = failedAttempts.Select(a => new NotificationRecipient
                {
                    Id = a.RecipientId,
                    Name = $"Recipient {a.RecipientId}",
                    Email = $"recipient{a.RecipientId}@example.com"
                }).ToList(),
                PreferredChannels = failedAttempts.Select(a => a.Channel).Distinct().ToList()
            };

            return await SendAsync(retryNotification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying notification {NotificationId}", notificationId);
            return NotificationResult.Failure(notificationId, $"Retry failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<bool> AcknowledgeAsync(string notificationId, string acknowledgedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Acknowledging notification {NotificationId} by {AcknowledgedBy}", 
                notificationId, acknowledgedBy);

            if (_notificationStatuses.TryGetValue(notificationId, out var status))
            {
                status.State = NotificationState.Acknowledged;
                status.LastUpdatedAt = DateTime.UtcNow;
                status.CompletedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Successfully acknowledged notification {NotificationId} by {AcknowledgedBy}", 
                    notificationId, acknowledgedBy);
                return true;
            }
            else
            {
                _logger.LogWarning("Notification {NotificationId} not found for acknowledgment", notificationId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging notification {NotificationId}", notificationId);
            return false;
        }
    }

    /// <summary>
    /// Delivers a notification to a specific recipient.
    /// </summary>
    private async Task<RecipientDeliveryResult> DeliverToRecipientAsync(
        NotificationEvent notification, 
        NotificationRecipient recipient, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Delivering notification {NotificationId} to recipient {RecipientId}", 
                notification.Id, recipient.Id);

            // Determine the best channel for this recipient
            var channel = DetermineBestChannel(notification, recipient);
            if (channel == null)
            {
                return new RecipientDeliveryResult
                {
                    RecipientId = recipient.Id,
                    Channel = NotificationChannel.Email, // Default
                    IsSuccess = false,
                    DeliveryId = Guid.NewGuid().ToString(),
                    ErrorMessage = "No suitable channel found for recipient",
                    AttemptedAt = DateTime.UtcNow
                };
            }

            // Find the appropriate provider
            var provider = _providers.FirstOrDefault(p => p.Channel == channel);
            if (provider == null)
            {
                return new RecipientDeliveryResult
                {
                    RecipientId = recipient.Id,
                    Channel = channel.Value,
                    IsSuccess = false,
                    DeliveryId = Guid.NewGuid().ToString(),
                    ErrorMessage = $"No provider found for channel {channel}",
                    AttemptedAt = DateTime.UtcNow
                };
            }

            // Validate the notification and recipient
            var validationResult = provider.Validate(notification, recipient);
            if (!validationResult.IsValid)
            {
                return new RecipientDeliveryResult
                {
                    RecipientId = recipient.Id,
                    Channel = channel.Value,
                    IsSuccess = false,
                    DeliveryId = Guid.NewGuid().ToString(),
                    ErrorMessage = string.Join(", ", validationResult.Errors),
                    AttemptedAt = DateTime.UtcNow
                };
            }

            // Send the notification
            var deliveryResult = await provider.SendAsync(notification, recipient, cancellationToken);
            
            return new RecipientDeliveryResult
            {
                RecipientId = recipient.Id,
                Channel = channel.Value,
                IsSuccess = deliveryResult.IsSuccess,
                DeliveryId = deliveryResult.DeliveryId,
                ErrorMessage = deliveryResult.ErrorMessage,
                AttemptedAt = deliveryResult.AttemptedAt,
                CompletedAt = deliveryResult.CompletedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering notification {NotificationId} to recipient {RecipientId}", 
                notification.Id, recipient.Id);
            
            return new RecipientDeliveryResult
            {
                RecipientId = recipient.Id,
                Channel = NotificationChannel.Email, // Default
                IsSuccess = false,
                DeliveryId = Guid.NewGuid().ToString(),
                ErrorMessage = ex.Message,
                AttemptedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Determines the best channel for delivering a notification to a recipient.
    /// </summary>
    private NotificationChannel? DetermineBestChannel(NotificationEvent notification, NotificationRecipient recipient)
    {
        // First, check recipient's channel preferences
        var preferredChannels = recipient.ChannelPreferences
            .Where(kvp => kvp.Value == ChannelPreference.Primary || kvp.Value == ChannelPreference.Preferred)
            .Select(kvp => kvp.Key)
            .ToList();

        if (preferredChannels.Any())
        {
            // Check if any preferred channels are available and in the notification's preferred channels
            var availablePreferred = preferredChannels
                .Where(channel => recipient.CanReceiveOnChannel(channel) && 
                                 (notification.PreferredChannels.Contains(channel) || !notification.PreferredChannels.Any()))
                .ToList();

            if (availablePreferred.Any())
            {
                return availablePreferred.First();
            }
        }

        // Fall back to notification's preferred channels
        if (notification.PreferredChannels.Any())
        {
            var availableChannels = notification.PreferredChannels
                .Where(channel => recipient.CanReceiveOnChannel(channel))
                .ToList();

            if (availableChannels.Any())
            {
                return availableChannels.First();
            }
        }

        // Fall back to any available channel for the recipient
        var allAvailableChannels = Enum.GetValues<NotificationChannel>()
            .Where(channel => recipient.CanReceiveOnChannel(channel))
            .ToList();

        return allAvailableChannels.FirstOrDefault();
    }
}