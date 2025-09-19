using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Collections.Concurrent;
using System.Text.Json;
using YamlDotNet.Serialization;

namespace NotifyX.Core.Services;

/// <summary>
/// Default implementation of the bulk operations service.
/// Provides bulk operations for rules, subscriptions, and events.
/// </summary>
public sealed class BulkOperationsService : IBulkOperationsService
{
    private readonly ILogger<BulkOperationsService> _logger;
    private readonly IRuleEngine _ruleEngine;
    private readonly INotificationService _notificationService;
    private readonly ConcurrentDictionary<string, BulkOperationStatus> _operationStatuses = new();
    private readonly ConcurrentDictionary<string, List<NotificationRule>> _rules = new();
    private readonly ConcurrentDictionary<string, List<NotificationSubscription>> _subscriptions = new();

    /// <summary>
    /// Initializes a new instance of the BulkOperationsService class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ruleEngine">The rule engine.</param>
    /// <param name="notificationService">The notification service.</param>
    public BulkOperationsService(
        ILogger<BulkOperationsService> logger,
        IRuleEngine ruleEngine,
        INotificationService notificationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ruleEngine = ruleEngine ?? throw new ArgumentNullException(nameof(ruleEngine));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <inheritdoc />
    public async Task<BulkRuleResult> CreateRulesBulkAsync(IEnumerable<NotificationRule> rules, CancellationToken cancellationToken = default)
    {
        try
        {
            var rulesList = rules.ToList();
            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Starting bulk rule creation for {Count} rules with operation ID {OperationId}", 
                rulesList.Count, operationId);

            var status = new BulkOperationStatus
            {
                OperationId = operationId,
                State = BulkOperationState.InProgress,
                TotalItems = rulesList.Count,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _operationStatuses[operationId] = status;

            var results = new List<RuleOperationResult>();
            var successCount = 0;
            var failureCount = 0;

            // Process rules in parallel with concurrency limit
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = rulesList.Select(async rule =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await ProcessRuleCreationAsync(rule, cancellationToken);
                    results.Add(result);
                    
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failureCount);

                    // Update progress
                    status.ProcessedItems++;
                    status.Progress = (int)((double)status.ProcessedItems / status.TotalItems * 100);
                    status.LastUpdatedAt = DateTime.UtcNow;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // Update final status
            status.State = failureCount == 0 ? BulkOperationState.Completed :
                          successCount == 0 ? BulkOperationState.Failed :
                          BulkOperationState.PartialFailure;
            status.SuccessfulItems = successCount;
            status.FailedItems = failureCount;
            status.Progress = 100;
            status.CompletedAt = DateTime.UtcNow;
            status.LastUpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Bulk rule creation completed. Operation ID: {OperationId}, Success: {Success}, Failed: {Failed}", 
                operationId, successCount, failureCount);

            return new BulkRuleResult
            {
                OperationId = operationId,
                TotalCount = rulesList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results,
                Status = status,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk rule creation");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkRuleResult> UpdateRulesBulkAsync(IEnumerable<NotificationRule> rules, CancellationToken cancellationToken = default)
    {
        try
        {
            var rulesList = rules.ToList();
            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Starting bulk rule update for {Count} rules with operation ID {OperationId}", 
                rulesList.Count, operationId);

            var status = new BulkOperationStatus
            {
                OperationId = operationId,
                State = BulkOperationState.InProgress,
                TotalItems = rulesList.Count,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _operationStatuses[operationId] = status;

            var results = new List<RuleOperationResult>();
            var successCount = 0;
            var failureCount = 0;

            // Process rules in parallel with concurrency limit
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = rulesList.Select(async rule =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await ProcessRuleUpdateAsync(rule, cancellationToken);
                    results.Add(result);
                    
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failureCount);

                    // Update progress
                    status.ProcessedItems++;
                    status.Progress = (int)((double)status.ProcessedItems / status.TotalItems * 100);
                    status.LastUpdatedAt = DateTime.UtcNow;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // Update final status
            status.State = failureCount == 0 ? BulkOperationState.Completed :
                          successCount == 0 ? BulkOperationState.Failed :
                          BulkOperationState.PartialFailure;
            status.SuccessfulItems = successCount;
            status.FailedItems = failureCount;
            status.Progress = 100;
            status.CompletedAt = DateTime.UtcNow;
            status.LastUpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Bulk rule update completed. Operation ID: {OperationId}, Success: {Success}, Failed: {Failed}", 
                operationId, successCount, failureCount);

            return new BulkRuleResult
            {
                OperationId = operationId,
                TotalCount = rulesList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results,
                Status = status,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk rule update");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkOperationResult> DeleteRulesBulkAsync(IEnumerable<string> ruleIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var ruleIdsList = ruleIds.ToList();
            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Starting bulk rule deletion for {Count} rules with operation ID {OperationId}", 
                ruleIdsList.Count, operationId);

            var status = new BulkOperationStatus
            {
                OperationId = operationId,
                State = BulkOperationState.InProgress,
                TotalItems = ruleIdsList.Count,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _operationStatuses[operationId] = status;

            var successCount = 0;
            var failureCount = 0;

            // Process deletions in parallel with concurrency limit
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = ruleIdsList.Select(async ruleId =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var success = await ProcessRuleDeletionAsync(ruleId, cancellationToken);
                    
                    if (success)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failureCount);

                    // Update progress
                    status.ProcessedItems++;
                    status.Progress = (int)((double)status.ProcessedItems / status.TotalItems * 100);
                    status.LastUpdatedAt = DateTime.UtcNow;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // Update final status
            status.State = failureCount == 0 ? BulkOperationState.Completed :
                          successCount == 0 ? BulkOperationState.Failed :
                          BulkOperationState.PartialFailure;
            status.SuccessfulItems = successCount;
            status.FailedItems = failureCount;
            status.Progress = 100;
            status.CompletedAt = DateTime.UtcNow;
            status.LastUpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Bulk rule deletion completed. Operation ID: {OperationId}, Success: {Success}, Failed: {Failed}", 
                operationId, successCount, failureCount);

            return new BulkOperationResult
            {
                OperationId = operationId,
                TotalCount = ruleIdsList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Status = status,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk rule deletion");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkRuleResult> ImportRulesAsync(string content, BulkImportFormat format, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting rule import from {Format} content", format);

            List<NotificationRule> rules;
            
            switch (format)
            {
                case BulkImportFormat.Json:
                    rules = await ImportRulesFromJsonAsync(content, cancellationToken);
                    break;
                case BulkImportFormat.Yaml:
                    rules = await ImportRulesFromYamlAsync(content, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Import format {format} is not supported for rules");
            }

            return await CreateRulesBulkAsync(rules, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing rules from {Format}", format);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkExportResult> ExportRulesAsync(IEnumerable<string>? ruleIds, BulkImportFormat format, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting rule export to {Format}", format);

            var rulesToExport = new List<NotificationRule>();
            
            if (ruleIds != null)
            {
                var ruleIdsList = ruleIds.ToList();
                foreach (var ruleId in ruleIdsList)
                {
                    var rule = await GetRuleByIdAsync(ruleId, cancellationToken);
                    if (rule != null)
                    {
                        rulesToExport.Add(rule);
                    }
                }
            }
            else
            {
                // Export all rules
                foreach (var tenantRules in _rules.Values)
                {
                    rulesToExport.AddRange(tenantRules);
                }
            }

            string content;
            switch (format)
            {
                case BulkImportFormat.Json:
                    content = await ExportRulesToJsonAsync(rulesToExport, cancellationToken);
                    break;
                case BulkImportFormat.Yaml:
                    content = await ExportRulesToYamlAsync(rulesToExport, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Export format {format} is not supported for rules");
            }

            return new BulkExportResult
            {
                OperationId = Guid.NewGuid().ToString(),
                Content = content,
                Format = format,
                ExportedCount = rulesToExport.Count,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting rules to {Format}", format);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkSubscriptionResult> CreateSubscriptionsBulkAsync(IEnumerable<NotificationSubscription> subscriptions, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionsList = subscriptions.ToList();
            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Starting bulk subscription creation for {Count} subscriptions with operation ID {OperationId}", 
                subscriptionsList.Count, operationId);

            var status = new BulkOperationStatus
            {
                OperationId = operationId,
                State = BulkOperationState.InProgress,
                TotalItems = subscriptionsList.Count,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _operationStatuses[operationId] = status;

            var results = new List<SubscriptionOperationResult>();
            var successCount = 0;
            var failureCount = 0;

            // Process subscriptions in parallel with concurrency limit
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = subscriptionsList.Select(async subscription =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await ProcessSubscriptionCreationAsync(subscription, cancellationToken);
                    results.Add(result);
                    
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failureCount);

                    // Update progress
                    status.ProcessedItems++;
                    status.Progress = (int)((double)status.ProcessedItems / status.TotalItems * 100);
                    status.LastUpdatedAt = DateTime.UtcNow;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // Update final status
            status.State = failureCount == 0 ? BulkOperationState.Completed :
                          successCount == 0 ? BulkOperationState.Failed :
                          BulkOperationState.PartialFailure;
            status.SuccessfulItems = successCount;
            status.FailedItems = failureCount;
            status.Progress = 100;
            status.CompletedAt = DateTime.UtcNow;
            status.LastUpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Bulk subscription creation completed. Operation ID: {OperationId}, Success: {Success}, Failed: {Failed}", 
                operationId, successCount, failureCount);

            return new BulkSubscriptionResult
            {
                OperationId = operationId,
                TotalCount = subscriptionsList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results,
                Status = status,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk subscription creation");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkSubscriptionResult> UpdateSubscriptionsBulkAsync(IEnumerable<NotificationSubscription> subscriptions, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionsList = subscriptions.ToList();
            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Starting bulk subscription update for {Count} subscriptions with operation ID {OperationId}", 
                subscriptionsList.Count, operationId);

            var status = new BulkOperationStatus
            {
                OperationId = operationId,
                State = BulkOperationState.InProgress,
                TotalItems = subscriptionsList.Count,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _operationStatuses[operationId] = status;

            var results = new List<SubscriptionOperationResult>();
            var successCount = 0;
            var failureCount = 0;

            // Process subscriptions in parallel with concurrency limit
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = subscriptionsList.Select(async subscription =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await ProcessSubscriptionUpdateAsync(subscription, cancellationToken);
                    results.Add(result);
                    
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failureCount);

                    // Update progress
                    status.ProcessedItems++;
                    status.Progress = (int)((double)status.ProcessedItems / status.TotalItems * 100);
                    status.LastUpdatedAt = DateTime.UtcNow;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // Update final status
            status.State = failureCount == 0 ? BulkOperationState.Completed :
                          successCount == 0 ? BulkOperationState.Failed :
                          BulkOperationState.PartialFailure;
            status.SuccessfulItems = successCount;
            status.FailedItems = failureCount;
            status.Progress = 100;
            status.CompletedAt = DateTime.UtcNow;
            status.LastUpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Bulk subscription update completed. Operation ID: {OperationId}, Success: {Success}, Failed: {Failed}", 
                operationId, successCount, failureCount);

            return new BulkSubscriptionResult
            {
                OperationId = operationId,
                TotalCount = subscriptionsList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results,
                Status = status,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk subscription update");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkOperationResult> DeleteSubscriptionsBulkAsync(IEnumerable<string> subscriptionIds, CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionIdsList = subscriptionIds.ToList();
            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Starting bulk subscription deletion for {Count} subscriptions with operation ID {OperationId}", 
                subscriptionIdsList.Count, operationId);

            var status = new BulkOperationStatus
            {
                OperationId = operationId,
                State = BulkOperationState.InProgress,
                TotalItems = subscriptionIdsList.Count,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _operationStatuses[operationId] = status;

            var successCount = 0;
            var failureCount = 0;

            // Process deletions in parallel with concurrency limit
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = subscriptionIdsList.Select(async subscriptionId =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var success = await ProcessSubscriptionDeletionAsync(subscriptionId, cancellationToken);
                    
                    if (success)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failureCount);

                    // Update progress
                    status.ProcessedItems++;
                    status.Progress = (int)((double)status.ProcessedItems / status.TotalItems * 100);
                    status.LastUpdatedAt = DateTime.UtcNow;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // Update final status
            status.State = failureCount == 0 ? BulkOperationState.Completed :
                          successCount == 0 ? BulkOperationState.Failed :
                          BulkOperationState.PartialFailure;
            status.SuccessfulItems = successCount;
            status.FailedItems = failureCount;
            status.Progress = 100;
            status.CompletedAt = DateTime.UtcNow;
            status.LastUpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Bulk subscription deletion completed. Operation ID: {OperationId}, Success: {Success}, Failed: {Failed}", 
                operationId, successCount, failureCount);

            return new BulkOperationResult
            {
                OperationId = operationId,
                TotalCount = subscriptionIdsList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Status = status,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk subscription deletion");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkSubscriptionResult> ImportSubscriptionsAsync(string content, BulkImportFormat format, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting subscription import from {Format} content", format);

            List<NotificationSubscription> subscriptions;
            
            switch (format)
            {
                case BulkImportFormat.Json:
                    subscriptions = await ImportSubscriptionsFromJsonAsync(content, cancellationToken);
                    break;
                case BulkImportFormat.Csv:
                    subscriptions = await ImportSubscriptionsFromCsvAsync(content, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Import format {format} is not supported for subscriptions");
            }

            return await CreateSubscriptionsBulkAsync(subscriptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing subscriptions from {Format}", format);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkExportResult> ExportSubscriptionsAsync(IEnumerable<string>? subscriptionIds, BulkImportFormat format, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting subscription export to {Format}", format);

            var subscriptionsToExport = new List<NotificationSubscription>();
            
            if (subscriptionIds != null)
            {
                var subscriptionIdsList = subscriptionIds.ToList();
                foreach (var subscriptionId in subscriptionIdsList)
                {
                    var subscription = await GetSubscriptionByIdAsync(subscriptionId, cancellationToken);
                    if (subscription != null)
                    {
                        subscriptionsToExport.Add(subscription);
                    }
                }
            }
            else
            {
                // Export all subscriptions
                foreach (var tenantSubscriptions in _subscriptions.Values)
                {
                    subscriptionsToExport.AddRange(tenantSubscriptions);
                }
            }

            string content;
            switch (format)
            {
                case BulkImportFormat.Json:
                    content = await ExportSubscriptionsToJsonAsync(subscriptionsToExport, cancellationToken);
                    break;
                case BulkImportFormat.Csv:
                    content = await ExportSubscriptionsToCsvAsync(subscriptionsToExport, cancellationToken);
                    break;
                default:
                    throw new NotSupportedException($"Export format {format} is not supported for subscriptions");
            }

            return new BulkExportResult
            {
                OperationId = Guid.NewGuid().ToString(),
                Content = content,
                Format = format,
                ExportedCount = subscriptionsToExport.Count,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting subscriptions to {Format}", format);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkEventResult> IngestEventsBulkAsync(IEnumerable<NotificationEvent> events, CancellationToken cancellationToken = default)
    {
        try
        {
            var eventsList = events.ToList();
            var operationId = Guid.NewGuid().ToString();
            
            _logger.LogInformation("Starting bulk event ingestion for {Count} events with operation ID {OperationId}", 
                eventsList.Count, operationId);

            var status = new BulkOperationStatus
            {
                OperationId = operationId,
                State = BulkOperationState.InProgress,
                TotalItems = eventsList.Count,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };
            _operationStatuses[operationId] = status;

            var results = new List<EventOperationResult>();
            var successCount = 0;
            var failureCount = 0;

            // Process events in parallel with concurrency limit
            var semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            var tasks = eventsList.Select(async notificationEvent =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await ProcessEventIngestionAsync(notificationEvent, cancellationToken);
                    results.Add(result);
                    
                    if (result.IsSuccess)
                        Interlocked.Increment(ref successCount);
                    else
                        Interlocked.Increment(ref failureCount);

                    // Update progress
                    status.ProcessedItems++;
                    status.Progress = (int)((double)status.ProcessedItems / status.TotalItems * 100);
                    status.LastUpdatedAt = DateTime.UtcNow;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // Update final status
            status.State = failureCount == 0 ? BulkOperationState.Completed :
                          successCount == 0 ? BulkOperationState.Failed :
                          BulkOperationState.PartialFailure;
            status.SuccessfulItems = successCount;
            status.FailedItems = failureCount;
            status.Progress = 100;
            status.CompletedAt = DateTime.UtcNow;
            status.LastUpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Bulk event ingestion completed. Operation ID: {OperationId}, Success: {Success}, Failed: {Failed}", 
                operationId, successCount, failureCount);

            return new BulkEventResult
            {
                OperationId = operationId,
                TotalCount = eventsList.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results,
                Status = status,
                CompletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk event ingestion");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BulkOperationStatus?> GetBulkOperationStatusAsync(string operationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting status for bulk operation {OperationId}", operationId);

            if (_operationStatuses.TryGetValue(operationId, out var status))
            {
                return status;
            }

            _logger.LogWarning("Status not found for bulk operation {OperationId}", operationId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for bulk operation {OperationId}", operationId);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> CancelBulkOperationAsync(string operationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cancelling bulk operation {OperationId}", operationId);

            if (_operationStatuses.TryGetValue(operationId, out var status))
            {
                if (status.State == BulkOperationState.Pending || status.State == BulkOperationState.InProgress)
                {
                    status.State = BulkOperationState.Cancelled;
                    status.LastUpdatedAt = DateTime.UtcNow;
                    status.CompletedAt = DateTime.UtcNow;
                    
                    _logger.LogInformation("Successfully cancelled bulk operation {OperationId}", operationId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Cannot cancel bulk operation {OperationId} in state {State}", 
                        operationId, status.State);
                    return false;
                }
            }
            else
            {
                _logger.LogWarning("Bulk operation {OperationId} not found for cancellation", operationId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling bulk operation {OperationId}", operationId);
            return false;
        }
    }

    #region Private Helper Methods

    private async Task<RuleOperationResult> ProcessRuleCreationAsync(NotificationRule rule, CancellationToken cancellationToken)
    {
        try
        {
            // Validate rule
            if (string.IsNullOrEmpty(rule.TenantId))
            {
                return new RuleOperationResult
                {
                    RuleId = rule.Id,
                    IsSuccess = false,
                    ErrorMessage = "Tenant ID is required",
                    ErrorCode = "MISSING_TENANT_ID"
                };
            }

            // Store rule (in a real implementation, this would be stored in a database)
            if (!_rules.ContainsKey(rule.TenantId))
            {
                _rules[rule.TenantId] = new List<NotificationRule>();
            }
            _rules[rule.TenantId].Add(rule);

            _logger.LogDebug("Successfully created rule {RuleId} for tenant {TenantId}", rule.Id, rule.TenantId);

            return new RuleOperationResult
            {
                RuleId = rule.Id,
                IsSuccess = true,
                Rule = rule
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating rule {RuleId}", rule.Id);
            return new RuleOperationResult
            {
                RuleId = rule.Id,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "RULE_CREATION_ERROR"
            };
        }
    }

    private async Task<RuleOperationResult> ProcessRuleUpdateAsync(NotificationRule rule, CancellationToken cancellationToken)
    {
        try
        {
            // Validate rule
            if (string.IsNullOrEmpty(rule.TenantId))
            {
                return new RuleOperationResult
                {
                    RuleId = rule.Id,
                    IsSuccess = false,
                    ErrorMessage = "Tenant ID is required",
                    ErrorCode = "MISSING_TENANT_ID"
                };
            }

            // Update rule (in a real implementation, this would be updated in a database)
            if (_rules.TryGetValue(rule.TenantId, out var tenantRules))
            {
                var existingRule = tenantRules.FirstOrDefault(r => r.Id == rule.Id);
                if (existingRule != null)
                {
                    var updatedRule = rule with { UpdatedAt = DateTime.UtcNow, Version = existingRule.Version + 1 };
                    var index = tenantRules.IndexOf(existingRule);
                    tenantRules[index] = updatedRule;

                    _logger.LogDebug("Successfully updated rule {RuleId} for tenant {TenantId}", rule.Id, rule.TenantId);

                    return new RuleOperationResult
                    {
                        RuleId = rule.Id,
                        IsSuccess = true,
                        Rule = updatedRule
                    };
                }
                else
                {
                    return new RuleOperationResult
                    {
                        RuleId = rule.Id,
                        IsSuccess = false,
                        ErrorMessage = "Rule not found",
                        ErrorCode = "RULE_NOT_FOUND"
                    };
                }
            }
            else
            {
                return new RuleOperationResult
                {
                    RuleId = rule.Id,
                    IsSuccess = false,
                    ErrorMessage = "Tenant not found",
                    ErrorCode = "TENANT_NOT_FOUND"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rule {RuleId}", rule.Id);
            return new RuleOperationResult
            {
                RuleId = rule.Id,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "RULE_UPDATE_ERROR"
            };
        }
    }

    private async Task<bool> ProcessRuleDeletionAsync(string ruleId, CancellationToken cancellationToken)
    {
        try
        {
            // Find and remove rule (in a real implementation, this would be deleted from a database)
            foreach (var tenantRules in _rules.Values)
            {
                var rule = tenantRules.FirstOrDefault(r => r.Id == ruleId);
                if (rule != null)
                {
                    tenantRules.Remove(rule);
                    _logger.LogDebug("Successfully deleted rule {RuleId}", ruleId);
                    return true;
                }
            }

            _logger.LogWarning("Rule {RuleId} not found for deletion", ruleId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting rule {RuleId}", ruleId);
            return false;
        }
    }

    private async Task<SubscriptionOperationResult> ProcessSubscriptionCreationAsync(NotificationSubscription subscription, CancellationToken cancellationToken)
    {
        try
        {
            // Validate subscription
            if (string.IsNullOrEmpty(subscription.TenantId))
            {
                return new SubscriptionOperationResult
                {
                    SubscriptionId = subscription.Id,
                    IsSuccess = false,
                    ErrorMessage = "Tenant ID is required",
                    ErrorCode = "MISSING_TENANT_ID"
                };
            }

            // Store subscription (in a real implementation, this would be stored in a database)
            if (!_subscriptions.ContainsKey(subscription.TenantId))
            {
                _subscriptions[subscription.TenantId] = new List<NotificationSubscription>();
            }
            _subscriptions[subscription.TenantId].Add(subscription);

            _logger.LogDebug("Successfully created subscription {SubscriptionId} for tenant {TenantId}", subscription.Id, subscription.TenantId);

            return new SubscriptionOperationResult
            {
                SubscriptionId = subscription.Id,
                IsSuccess = true,
                Subscription = subscription
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription {SubscriptionId}", subscription.Id);
            return new SubscriptionOperationResult
            {
                SubscriptionId = subscription.Id,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "SUBSCRIPTION_CREATION_ERROR"
            };
        }
    }

    private async Task<SubscriptionOperationResult> ProcessSubscriptionUpdateAsync(NotificationSubscription subscription, CancellationToken cancellationToken)
    {
        try
        {
            // Validate subscription
            if (string.IsNullOrEmpty(subscription.TenantId))
            {
                return new SubscriptionOperationResult
                {
                    SubscriptionId = subscription.Id,
                    IsSuccess = false,
                    ErrorMessage = "Tenant ID is required",
                    ErrorCode = "MISSING_TENANT_ID"
                };
            }

            // Update subscription (in a real implementation, this would be updated in a database)
            if (_subscriptions.TryGetValue(subscription.TenantId, out var tenantSubscriptions))
            {
                var existingSubscription = tenantSubscriptions.FirstOrDefault(s => s.Id == subscription.Id);
                if (existingSubscription != null)
                {
                    var updatedSubscription = subscription with { UpdatedAt = DateTime.UtcNow, Version = existingSubscription.Version + 1 };
                    var index = tenantSubscriptions.IndexOf(existingSubscription);
                    tenantSubscriptions[index] = updatedSubscription;

                    _logger.LogDebug("Successfully updated subscription {SubscriptionId} for tenant {TenantId}", subscription.Id, subscription.TenantId);

                    return new SubscriptionOperationResult
                    {
                        SubscriptionId = subscription.Id,
                        IsSuccess = true,
                        Subscription = updatedSubscription
                    };
                }
                else
                {
                    return new SubscriptionOperationResult
                    {
                        SubscriptionId = subscription.Id,
                        IsSuccess = false,
                        ErrorMessage = "Subscription not found",
                        ErrorCode = "SUBSCRIPTION_NOT_FOUND"
                    };
                }
            }
            else
            {
                return new SubscriptionOperationResult
                {
                    SubscriptionId = subscription.Id,
                    IsSuccess = false,
                    ErrorMessage = "Tenant not found",
                    ErrorCode = "TENANT_NOT_FOUND"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscription.Id);
            return new SubscriptionOperationResult
            {
                SubscriptionId = subscription.Id,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "SUBSCRIPTION_UPDATE_ERROR"
            };
        }
    }

    private async Task<bool> ProcessSubscriptionDeletionAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            // Find and remove subscription (in a real implementation, this would be deleted from a database)
            foreach (var tenantSubscriptions in _subscriptions.Values)
            {
                var subscription = tenantSubscriptions.FirstOrDefault(s => s.Id == subscriptionId);
                if (subscription != null)
                {
                    tenantSubscriptions.Remove(subscription);
                    _logger.LogDebug("Successfully deleted subscription {SubscriptionId}", subscriptionId);
                    return true;
                }
            }

            _logger.LogWarning("Subscription {SubscriptionId} not found for deletion", subscriptionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subscription {SubscriptionId}", subscriptionId);
            return false;
        }
    }

    private async Task<EventOperationResult> ProcessEventIngestionAsync(NotificationEvent notificationEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Send the notification through the notification service
            var result = await _notificationService.SendAsync(notificationEvent, cancellationToken);

            return new EventOperationResult
            {
                EventId = notificationEvent.Id,
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage,
                ErrorCode = result.ErrorCode,
                Event = notificationEvent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ingesting event {EventId}", notificationEvent.Id);
            return new EventOperationResult
            {
                EventId = notificationEvent.Id,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = "EVENT_INGESTION_ERROR",
                Event = notificationEvent
            };
        }
    }

    private async Task<NotificationRule?> GetRuleByIdAsync(string ruleId, CancellationToken cancellationToken)
    {
        foreach (var tenantRules in _rules.Values)
        {
            var rule = tenantRules.FirstOrDefault(r => r.Id == ruleId);
            if (rule != null)
            {
                return rule;
            }
        }
        return null;
    }

    private async Task<NotificationSubscription?> GetSubscriptionByIdAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        foreach (var tenantSubscriptions in _subscriptions.Values)
        {
            var subscription = tenantSubscriptions.FirstOrDefault(s => s.Id == subscriptionId);
            if (subscription != null)
            {
                return subscription;
            }
        }
        return null;
    }

    #region Import/Export Methods

    private async Task<List<NotificationRule>> ImportRulesFromJsonAsync(string content, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var rules = JsonSerializer.Deserialize<List<NotificationRule>>(content, options);
        return rules ?? new List<NotificationRule>();
    }

    private async Task<List<NotificationRule>> ImportRulesFromYamlAsync(string content, CancellationToken cancellationToken)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();

        var rules = deserializer.Deserialize<List<NotificationRule>>(content);
        return rules ?? new List<NotificationRule>();
    }

    private async Task<string> ExportRulesToJsonAsync(List<NotificationRule> rules, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(rules, options);
    }

    private async Task<string> ExportRulesToYamlAsync(List<NotificationRule> rules, CancellationToken cancellationToken)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(rules);
    }

    private async Task<List<NotificationSubscription>> ImportSubscriptionsFromJsonAsync(string content, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var subscriptions = JsonSerializer.Deserialize<List<NotificationSubscription>>(content, options);
        return subscriptions ?? new List<NotificationSubscription>();
    }

    private async Task<List<NotificationSubscription>> ImportSubscriptionsFromCsvAsync(string content, CancellationToken cancellationToken)
    {
        var subscriptions = new List<NotificationSubscription>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length < 2) return subscriptions;

        var headers = lines[0].Split(',');
        
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            if (values.Length != headers.Length) continue;

            var subscription = new NotificationSubscription();
            for (int j = 0; j < headers.Length; j++)
            {
                var header = headers[j].Trim().ToLower();
                var value = values[j].Trim();

                switch (header)
                {
                    case "tenantid":
                        subscription = subscription with { TenantId = value };
                        break;
                    case "recipientid":
                        subscription = subscription with { Recipient = subscription.Recipient with { Id = value } };
                        break;
                    case "recipientname":
                        subscription = subscription with { Recipient = subscription.Recipient with { Name = value } };
                        break;
                    case "recipientemail":
                        subscription = subscription with { Recipient = subscription.Recipient with { Email = value } };
                        break;
                    case "eventtypes":
                        if (!string.IsNullOrEmpty(value))
                        {
                            var eventTypes = value.Split(';').Select(et => et.Trim()).ToList();
                            subscription = subscription with { EventTypes = eventTypes };
                        }
                        break;
                    case "channels":
                        if (!string.IsNullOrEmpty(value))
                        {
                            var channels = value.Split(';')
                                .Select(c => Enum.TryParse<NotificationChannel>(c.Trim(), true, out var channel) ? channel : NotificationChannel.Email)
                                .ToList();
                            subscription = subscription with { Channels = channels };
                        }
                        break;
                }
            }
            subscriptions.Add(subscription);
        }

        return subscriptions;
    }

    private async Task<string> ExportSubscriptionsToJsonAsync(List<NotificationSubscription> subscriptions, CancellationToken cancellationToken)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(subscriptions, options);
    }

    private async Task<string> ExportSubscriptionsToCsvAsync(List<NotificationSubscription> subscriptions, CancellationToken cancellationToken)
    {
        var csv = new System.Text.StringBuilder();
        
        // Headers
        csv.AppendLine("TenantId,RecipientId,RecipientName,RecipientEmail,EventTypes,Channels,IsActive,IsEnabled");
        
        // Data
        foreach (var subscription in subscriptions)
        {
            var eventTypes = string.Join(";", subscription.EventTypes);
            var channels = string.Join(";", subscription.Channels.Select(c => c.ToString()));
            
            csv.AppendLine($"{subscription.TenantId},{subscription.Recipient.Id},{subscription.Recipient.Name},{subscription.Recipient.Email},{eventTypes},{channels},{subscription.IsActive},{subscription.IsEnabled}");
        }
        
        return csv.ToString();
    }

    #endregion

    #endregion
}