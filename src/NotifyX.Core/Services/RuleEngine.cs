using Microsoft.Extensions.Logging;
using NotifyX.Core.Interfaces;
using NotifyX.Core.Models;
using System.Text.Json;

namespace NotifyX.Core.Services;

/// <summary>
/// Default implementation of the notification rule engine.
/// Handles rule evaluation, workflow processing, and escalation logic.
/// </summary>
public sealed class RuleEngine : IRuleEngine
{
    private readonly ILogger<RuleEngine> _logger;
    private readonly Dictionary<string, NotificationRule> _rules = new();
    private readonly object _rulesLock = new();

    /// <summary>
    /// Initializes a new instance of the RuleEngine class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public RuleEngine(ILogger<RuleEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<RuleEvaluationResult> EvaluateRulesAsync(NotificationEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Evaluating rules for notification {NotificationId} of type {EventType}", 
                notification.Id, notification.EventType);

            var matchedRules = new List<NotificationRule>();
            var unmatchedRules = new List<NotificationRule>();

            // Get all active rules for the tenant
            var tenantRules = GetActiveRulesForTenant(notification.TenantId);

            foreach (var rule in tenantRules)
            {
                try
                {
                    if (await EvaluateRuleAsync(rule, notification, cancellationToken))
                    {
                        matchedRules.Add(rule);
                        _logger.LogDebug("Rule {RuleId} matched for notification {NotificationId}", 
                            rule.Id, notification.Id);
                    }
                    else
                    {
                        unmatchedRules.Add(rule);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error evaluating rule {RuleId} for notification {NotificationId}", 
                        rule.Id, notification.Id);
                    // Continue with other rules even if one fails
                }
            }

            // Sort matched rules by priority (higher priority first)
            matchedRules = matchedRules.OrderByDescending(r => r.Priority).ToList();

            _logger.LogInformation("Rule evaluation completed for notification {NotificationId}. " +
                "Matched {MatchedCount} rules, {UnmatchedCount} rules did not match", 
                notification.Id, matchedRules.Count, unmatchedRules.Count);

            return RuleEvaluationResult.Success(matchedRules, unmatchedRules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rules for notification {NotificationId}", notification.Id);
            return RuleEvaluationResult.Failure($"Rule evaluation failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<WorkflowResult> ProcessWorkflowAsync(
        NotificationEvent notification, 
        IEnumerable<NotificationRule> rules, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Processing workflow for notification {NotificationId} with {RuleCount} rules", 
                notification.Id, rules.Count());

            var executedActions = new List<WorkflowAction>();
            var failedActions = new List<WorkflowAction>();
            var modifiedNotification = notification;

            foreach (var rule in rules.OrderByDescending(r => r.Priority))
            {
                try
                {
                    var ruleResult = await ProcessRuleActionsAsync(rule, modifiedNotification, cancellationToken);
                    
                    executedActions.AddRange(ruleResult.ExecutedActions);
                    failedActions.AddRange(ruleResult.FailedActions);
                    
                    // Update notification if it was modified
                    if (ruleResult.ModifiedNotification != null)
                    {
                        modifiedNotification = ruleResult.ModifiedNotification;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing rule {RuleId} for notification {NotificationId}", 
                        rule.Id, notification.Id);
                    
                    // Create a failed action for the entire rule
                    var failedAction = new WorkflowAction
                    {
                        Id = Guid.NewGuid().ToString(),
                        Type = WorkflowActionType.Custom,
                        Name = $"Rule {rule.Name}",
                        Description = $"Processing rule {rule.Name}",
                        IsExecuted = true,
                        IsSuccess = false,
                        ErrorMessage = ex.Message,
                        ExecutedAt = DateTime.UtcNow
                    };
                    
                    failedActions.Add(failedAction);
                }
            }

            var isSuccess = !failedActions.Any() || failedActions.All(a => a.ContinueOnFailure);

            _logger.LogInformation("Workflow processing completed for notification {NotificationId}. " +
                "Executed {ExecutedCount} actions, {FailedCount} actions failed", 
                notification.Id, executedActions.Count, failedActions.Count);

            return isSuccess 
                ? WorkflowResult.Success(executedActions, modifiedNotification)
                : WorkflowResult.Failure("Some workflow actions failed", failedActions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing workflow for notification {NotificationId}", notification.Id);
            return WorkflowResult.Failure($"Workflow processing failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<EscalationResult> CheckEscalationAsync(
        NotificationEvent notification, 
        IEnumerable<DeliveryAttempt> deliveryResults, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking escalation for notification {NotificationId}", notification.Id);

            // Check if escalation is enabled
            if (!notification.DeliveryOptions.EnableEscalation)
            {
                return EscalationResult.NoEscalationNeeded();
            }

            var deliveryResultsList = deliveryResults.ToList();
            var failedAttempts = deliveryResultsList.Where(r => !r.IsSuccess).ToList();
            var successfulAttempts = deliveryResultsList.Where(r => r.IsSuccess).ToList();

            // If there are successful deliveries, no escalation needed
            if (successfulAttempts.Any())
            {
                return EscalationResult.NoEscalationNeeded();
            }

            // Check if enough time has passed for escalation
            var lastAttempt = deliveryResultsList.OrderByDescending(r => r.AttemptedAt).FirstOrDefault();
            if (lastAttempt != null)
            {
                var timeSinceLastAttempt = DateTime.UtcNow - lastAttempt.AttemptedAt;
                if (timeSinceLastAttempt < notification.DeliveryOptions.EscalationDelay)
                {
                    return EscalationResult.NoEscalationNeeded();
                }
            }

            // Check if we've exceeded max attempts
            if (failedAttempts.Count >= notification.DeliveryOptions.MaxAttempts)
            {
                var escalationActions = CreateEscalationActions(notification, failedAttempts);
                return EscalationResult.EscalationNeeded(
                    escalationActions, 
                    $"All {failedAttempts.Count} delivery attempts failed");
            }

            // Check for critical priority notifications
            if (notification.Priority == NotificationPriority.Critical && failedAttempts.Any())
            {
                var escalationActions = CreateEscalationActions(notification, failedAttempts);
                return EscalationResult.EscalationNeeded(
                    escalationActions, 
                    "Critical priority notification failed");
            }

            return EscalationResult.NoEscalationNeeded();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking escalation for notification {NotificationId}", notification.Id);
            return EscalationResult.NoEscalationNeeded();
        }
    }

    /// <inheritdoc />
    public async Task<bool> AddRuleAsync(NotificationRule rule, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Adding rule {RuleId} for tenant {TenantId}", rule.Id, rule.TenantId);

            lock (_rulesLock)
            {
                _rules[rule.Id] = rule;
            }

            _logger.LogInformation("Successfully added rule {RuleId} for tenant {TenantId}", rule.Id, rule.TenantId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding rule {RuleId} for tenant {TenantId}", rule.Id, rule.TenantId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateRuleAsync(NotificationRule rule, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating rule {RuleId} for tenant {TenantId}", rule.Id, rule.TenantId);

            lock (_rulesLock)
            {
                if (_rules.ContainsKey(rule.Id))
                {
                    _rules[rule.Id] = rule;
                    _logger.LogInformation("Successfully updated rule {RuleId} for tenant {TenantId}", rule.Id, rule.TenantId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Rule {RuleId} not found for update", rule.Id);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rule {RuleId} for tenant {TenantId}", rule.Id, rule.TenantId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Removing rule {RuleId}", ruleId);

            lock (_rulesLock)
            {
                var removed = _rules.Remove(ruleId);
                if (removed)
                {
                    _logger.LogInformation("Successfully removed rule {RuleId}", ruleId);
                }
                else
                {
                    _logger.LogWarning("Rule {RuleId} not found for removal", ruleId);
                }
                return removed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing rule {RuleId}", ruleId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<NotificationRule>> GetRulesAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting rules for tenant {TenantId}", tenantId);

            lock (_rulesLock)
            {
                var tenantRules = _rules.Values.Where(r => r.TenantId == tenantId).ToList();
                _logger.LogDebug("Found {RuleCount} rules for tenant {TenantId}", tenantRules.Count, tenantId);
                return tenantRules;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rules for tenant {TenantId}", tenantId);
            return Enumerable.Empty<NotificationRule>();
        }
    }

    /// <inheritdoc />
    public async Task<NotificationRule?> GetRuleAsync(string ruleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting rule {RuleId}", ruleId);

            lock (_rulesLock)
            {
                _rules.TryGetValue(ruleId, out var rule);
                return rule;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rule {RuleId}", ruleId);
            return null;
        }
    }

    /// <summary>
    /// Evaluates a single rule against a notification.
    /// </summary>
    /// <param name="rule">The rule to evaluate.</param>
    /// <param name="notification">The notification to evaluate against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the rule matches, false otherwise.</returns>
    private async Task<bool> EvaluateRuleAsync(NotificationRule rule, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            return await EvaluateConditionAsync(rule.Condition, notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating condition for rule {RuleId}", rule.Id);
            return false;
        }
    }

    /// <summary>
    /// Evaluates a condition against a notification.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="notification">The notification to evaluate against.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the condition is met, false otherwise.</returns>
    private async Task<bool> EvaluateConditionAsync(RuleCondition condition, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // Handle nested conditions
            if (condition.NestedConditions.Any())
            {
                var results = new List<bool>();
                foreach (var nestedCondition in condition.NestedConditions)
                {
                    var result = await EvaluateConditionAsync(nestedCondition, notification, cancellationToken);
                    results.Add(result);
                }

                return condition.LogicalOperator switch
                {
                    LogicalOperator.And => results.All(r => r),
                    LogicalOperator.Or => results.Any(r => r),
                    LogicalOperator.Not => !results.Any(r => r),
                    _ => false
                };
            }

            // Get the actual value from the notification
            var actualValue = GetFieldValue(notification, condition.FieldPath);
            var expectedValues = condition.ExpectedValues;

            // Evaluate the condition based on the operator
            return condition.Operator switch
            {
                ConditionOperator.Equals => EvaluateEquals(actualValue, expectedValues, condition.CaseSensitive),
                ConditionOperator.NotEquals => !EvaluateEquals(actualValue, expectedValues, condition.CaseSensitive),
                ConditionOperator.GreaterThan => EvaluateGreaterThan(actualValue, expectedValues),
                ConditionOperator.GreaterThanOrEqual => EvaluateGreaterThanOrEqual(actualValue, expectedValues),
                ConditionOperator.LessThan => EvaluateLessThan(actualValue, expectedValues),
                ConditionOperator.LessThanOrEqual => EvaluateLessThanOrEqual(actualValue, expectedValues),
                ConditionOperator.Contains => EvaluateContains(actualValue, expectedValues, condition.CaseSensitive),
                ConditionOperator.DoesNotContain => !EvaluateContains(actualValue, expectedValues, condition.CaseSensitive),
                ConditionOperator.StartsWith => EvaluateStartsWith(actualValue, expectedValues, condition.CaseSensitive),
                ConditionOperator.EndsWith => EvaluateEndsWith(actualValue, expectedValues, condition.CaseSensitive),
                ConditionOperator.Regex => EvaluateRegex(actualValue, expectedValues),
                ConditionOperator.In => EvaluateIn(actualValue, expectedValues, condition.CaseSensitive),
                ConditionOperator.NotIn => !EvaluateIn(actualValue, expectedValues, condition.CaseSensitive),
                ConditionOperator.IsNull => actualValue == null,
                ConditionOperator.IsNotNull => actualValue != null,
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating condition for field {FieldPath}", condition.FieldPath);
            return false;
        }
    }

    /// <summary>
    /// Gets the value of a field from a notification using a field path.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="fieldPath">The field path (e.g., "EventType", "Priority", "Metadata.Status").</param>
    /// <returns>The field value, or null if not found.</returns>
    private object? GetFieldValue(NotificationEvent notification, string fieldPath)
    {
        if (string.IsNullOrEmpty(fieldPath))
            return null;

        var parts = fieldPath.Split('.');
        object? current = notification;

        foreach (var part in parts)
        {
            if (current == null)
                return null;

            var property = current.GetType().GetProperty(part);
            if (property != null)
            {
                current = property.GetValue(current);
            }
            else if (current is Dictionary<string, object> dict)
            {
                dict.TryGetValue(part, out current);
            }
            else
            {
                return null;
            }
        }

        return current;
    }

    /// <summary>
    /// Evaluates an equals condition.
    /// </summary>
    private bool EvaluateEquals(object? actualValue, List<object> expectedValues, bool caseSensitive)
    {
        if (actualValue == null)
            return expectedValues.Contains(null);

        foreach (var expectedValue in expectedValues)
        {
            if (actualValue.Equals(expectedValue))
                return true;

            if (actualValue is string actualStr && expectedValue is string expectedStr)
            {
                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                if (string.Equals(actualStr, expectedStr, comparison))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Evaluates a greater than condition.
    /// </summary>
    private bool EvaluateGreaterThan(object? actualValue, List<object> expectedValues)
    {
        if (actualValue == null || !expectedValues.Any())
            return false;

        var expectedValue = expectedValues.First();
        return CompareValues(actualValue, expectedValue) > 0;
    }

    /// <summary>
    /// Evaluates a greater than or equal condition.
    /// </summary>
    private bool EvaluateGreaterThanOrEqual(object? actualValue, List<object> expectedValues)
    {
        if (actualValue == null || !expectedValues.Any())
            return false;

        var expectedValue = expectedValues.First();
        return CompareValues(actualValue, expectedValue) >= 0;
    }

    /// <summary>
    /// Evaluates a less than condition.
    /// </summary>
    private bool EvaluateLessThan(object? actualValue, List<object> expectedValues)
    {
        if (actualValue == null || !expectedValues.Any())
            return false;

        var expectedValue = expectedValues.First();
        return CompareValues(actualValue, expectedValue) < 0;
    }

    /// <summary>
    /// Evaluates a less than or equal condition.
    /// </summary>
    private bool EvaluateLessThanOrEqual(object? actualValue, List<object> expectedValues)
    {
        if (actualValue == null || !expectedValues.Any())
            return false;

        var expectedValue = expectedValues.First();
        return CompareValues(actualValue, expectedValue) <= 0;
    }

    /// <summary>
    /// Evaluates a contains condition.
    /// </summary>
    private bool EvaluateContains(object? actualValue, List<object> expectedValues, bool caseSensitive)
    {
        if (actualValue == null)
            return false;

        var actualStr = actualValue.ToString();
        if (string.IsNullOrEmpty(actualStr))
            return false;

        foreach (var expectedValue in expectedValues)
        {
            var expectedStr = expectedValue?.ToString();
            if (string.IsNullOrEmpty(expectedStr))
                continue;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (actualStr.Contains(expectedStr, comparison))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Evaluates a starts with condition.
    /// </summary>
    private bool EvaluateStartsWith(object? actualValue, List<object> expectedValues, bool caseSensitive)
    {
        if (actualValue == null)
            return false;

        var actualStr = actualValue.ToString();
        if (string.IsNullOrEmpty(actualStr))
            return false;

        foreach (var expectedValue in expectedValues)
        {
            var expectedStr = expectedValue?.ToString();
            if (string.IsNullOrEmpty(expectedStr))
                continue;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (actualStr.StartsWith(expectedStr, comparison))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Evaluates an ends with condition.
    /// </summary>
    private bool EvaluateEndsWith(object? actualValue, List<object> expectedValues, bool caseSensitive)
    {
        if (actualValue == null)
            return false;

        var actualStr = actualValue.ToString();
        if (string.IsNullOrEmpty(actualStr))
            return false;

        foreach (var expectedValue in expectedValues)
        {
            var expectedStr = expectedValue?.ToString();
            if (string.IsNullOrEmpty(expectedStr))
                continue;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (actualStr.EndsWith(expectedStr, comparison))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Evaluates a regex condition.
    /// </summary>
    private bool EvaluateRegex(object? actualValue, List<object> expectedValues)
    {
        if (actualValue == null)
            return false;

        var actualStr = actualValue.ToString();
        if (string.IsNullOrEmpty(actualStr))
            return false;

        foreach (var expectedValue in expectedValues)
        {
            var pattern = expectedValue?.ToString();
            if (string.IsNullOrEmpty(pattern))
                continue;

            try
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(actualStr, pattern))
                    return true;
            }
            catch (ArgumentException)
            {
                // Invalid regex pattern, skip
                continue;
            }
        }

        return false;
    }

    /// <summary>
    /// Evaluates an in condition.
    /// </summary>
    private bool EvaluateIn(object? actualValue, List<object> expectedValues, bool caseSensitive)
    {
        if (actualValue == null)
            return expectedValues.Contains(null);

        foreach (var expectedValue in expectedValues)
        {
            if (actualValue.Equals(expectedValue))
                return true;

            if (actualValue is string actualStr && expectedValue is string expectedStr)
            {
                var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                if (string.Equals(actualStr, expectedStr, comparison))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Compares two values for ordering.
    /// </summary>
    private int CompareValues(object? value1, object? value2)
    {
        if (value1 == null && value2 == null)
            return 0;
        if (value1 == null)
            return -1;
        if (value2 == null)
            return 1;

        if (value1 is IComparable comparable1 && value2 is IComparable comparable2)
        {
            try
            {
                return comparable1.CompareTo(comparable2);
            }
            catch
            {
                // If comparison fails, fall back to string comparison
            }
        }

        var str1 = value1.ToString();
        var str2 = value2.ToString();
        return string.Compare(str1, str2, StringComparison.Ordinal);
    }

    /// <summary>
    /// Processes the actions for a rule.
    /// </summary>
    private async Task<WorkflowResult> ProcessRuleActionsAsync(NotificationRule rule, NotificationEvent notification, CancellationToken cancellationToken)
    {
        var executedActions = new List<WorkflowAction>();
        var failedActions = new List<WorkflowAction>();
        var modifiedNotification = notification;

        foreach (var action in rule.Actions)
        {
            try
            {
                var workflowAction = new WorkflowAction
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = (WorkflowActionType)action.Type,
                    Name = action.Type.ToString(),
                    Description = $"Executing {action.Type} action",
                    Parameters = action.Parameters,
                    IsAsync = action.IsAsync,
                    Timeout = action.Timeout,
                    ContinueOnFailure = action.ContinueOnFailure,
                    RetryConfiguration = action.RetryConfiguration != null ? new WorkflowActionRetryConfiguration
                    {
                        MaxRetryAttempts = action.RetryConfiguration.MaxRetryAttempts,
                        InitialDelay = action.RetryConfiguration.InitialDelay,
                        MaxDelay = action.RetryConfiguration.MaxDelay,
                        BackoffMultiplier = action.RetryConfiguration.BackoffMultiplier,
                        UseExponentialBackoff = action.RetryConfiguration.UseExponentialBackoff
                    } : null
                };

                var startTime = DateTime.UtcNow;
                workflowAction.ExecutedAt = startTime;

                // Execute the action based on its type
                var result = await ExecuteActionAsync(workflowAction, modifiedNotification, cancellationToken);
                
                workflowAction.IsExecuted = true;
                workflowAction.IsSuccess = result.IsSuccess;
                workflowAction.ErrorMessage = result.ErrorMessage;
                workflowAction.CompletedAt = DateTime.UtcNow;
                workflowAction.Duration = workflowAction.CompletedAt - startTime;
                workflowAction.Result = result.Metadata;

                if (result.IsSuccess)
                {
                    executedActions.Add(workflowAction);
                    
                    // Update notification if it was modified
                    if (result.ModifiedNotification != null)
                    {
                        modifiedNotification = result.ModifiedNotification;
                    }
                }
                else
                {
                    failedActions.Add(workflowAction);
                    
                    if (!action.ContinueOnFailure)
                    {
                        break; // Stop processing if we shouldn't continue on failure
                    }
                }
            }
            catch (Exception ex)
            {
                var failedAction = new WorkflowAction
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = (WorkflowActionType)action.Type,
                    Name = action.Type.ToString(),
                    Description = $"Executing {action.Type} action",
                    IsExecuted = true,
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    ExecutedAt = DateTime.UtcNow
                };

                failedActions.Add(failedAction);

                if (!action.ContinueOnFailure)
                {
                    break; // Stop processing if we shouldn't continue on failure
                }
            }
        }

        return WorkflowResult.Success(executedActions, modifiedNotification);
    }

    /// <summary>
    /// Executes a workflow action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            return action.Type switch
            {
                WorkflowActionType.SendNotification => await ExecuteSendNotificationActionAsync(action, notification, cancellationToken),
                WorkflowActionType.ModifyNotification => await ExecuteModifyNotificationActionAsync(action, notification, cancellationToken),
                WorkflowActionType.AddRecipients => await ExecuteAddRecipientsActionAsync(action, notification, cancellationToken),
                WorkflowActionType.RemoveRecipients => await ExecuteRemoveRecipientsActionAsync(action, notification, cancellationToken),
                WorkflowActionType.SetPriority => await ExecuteSetPriorityActionAsync(action, notification, cancellationToken),
                WorkflowActionType.SetChannels => await ExecuteSetChannelsActionAsync(action, notification, cancellationToken),
                WorkflowActionType.DelayDelivery => await ExecuteDelayDeliveryActionAsync(action, notification, cancellationToken),
                WorkflowActionType.CancelDelivery => await ExecuteCancelDeliveryActionAsync(action, notification, cancellationToken),
                WorkflowActionType.ExecuteWebhook => await ExecuteWebhookActionAsync(action, notification, cancellationToken),
                WorkflowActionType.LogEvent => await ExecuteLogEventActionAsync(action, notification, cancellationToken),
                WorkflowActionType.Escalate => await ExecuteEscalateActionAsync(action, notification, cancellationToken),
                WorkflowActionType.Aggregate => await ExecuteAggregateActionAsync(action, notification, cancellationToken),
                WorkflowActionType.Custom => await ExecuteCustomActionAsync(action, notification, cancellationToken),
                _ => ActionExecutionResult.Failure($"Unknown action type: {action.Type}")
            };
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error executing action {action.Type}: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a send notification action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteSendNotificationActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        // This would typically delegate to the notification service
        // For now, we'll just log the action
        _logger.LogDebug("Executing send notification action for notification {NotificationId}", notification.Id);
        
        return ActionExecutionResult.Success(new Dictionary<string, object>
        {
            ["action"] = "send_notification",
            ["notification_id"] = notification.Id
        });
    }

    /// <summary>
    /// Executes a modify notification action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteModifyNotificationActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var modifiedNotification = notification;

            // Apply modifications based on action parameters
            if (action.Parameters.TryGetValue("subject", out var subject))
            {
                modifiedNotification = modifiedNotification.With(builder => builder.WithSubject(subject.ToString() ?? ""));
            }

            if (action.Parameters.TryGetValue("content", out var content))
            {
                modifiedNotification = modifiedNotification.With(builder => builder.WithContent(content.ToString() ?? ""));
            }

            if (action.Parameters.TryGetValue("priority", out var priority) && 
                Enum.TryParse<NotificationPriority>(priority.ToString(), out var priorityValue))
            {
                modifiedNotification = modifiedNotification.With(builder => builder.WithPriority(priorityValue));
            }

            return ActionExecutionResult.Success(new Dictionary<string, object>
            {
                ["action"] = "modify_notification",
                ["notification_id"] = notification.Id
            }, modifiedNotification);
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error modifying notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes an add recipients action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteAddRecipientsActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (action.Parameters.TryGetValue("recipients", out var recipientsObj) && 
                recipientsObj is IEnumerable<object> recipientsList)
            {
                var newRecipients = new List<NotificationRecipient>(notification.Recipients);
                
                foreach (var recipientObj in recipientsList)
                {
                    if (recipientObj is NotificationRecipient recipient)
                    {
                        newRecipients.Add(recipient);
                    }
                }

                var modifiedNotification = notification.With(builder => 
                {
                    foreach (var recipient in newRecipients)
                    {
                        builder.WithRecipient(recipient);
                    }
                });

                return ActionExecutionResult.Success(new Dictionary<string, object>
                {
                    ["action"] = "add_recipients",
                    ["recipients_added"] = newRecipients.Count
                }, modifiedNotification);
            }

            return ActionExecutionResult.Failure("No recipients provided in action parameters");
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error adding recipients: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a remove recipients action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteRemoveRecipientsActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (action.Parameters.TryGetValue("recipient_ids", out var recipientIdsObj) && 
                recipientIdsObj is IEnumerable<object> recipientIdsList)
            {
                var recipientIds = recipientIdsList.Select(id => id.ToString()).ToHashSet();
                var remainingRecipients = notification.Recipients.Where(r => !recipientIds.Contains(r.Id)).ToList();

                var modifiedNotification = notification with { Recipients = remainingRecipients };

                return ActionExecutionResult.Success(new Dictionary<string, object>
                {
                    ["action"] = "remove_recipients",
                    ["recipients_removed"] = recipientIds.Count
                }, modifiedNotification);
            }

            return ActionExecutionResult.Failure("No recipient IDs provided in action parameters");
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error removing recipients: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a set priority action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteSetPriorityActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (action.Parameters.TryGetValue("priority", out var priorityObj) && 
                Enum.TryParse<NotificationPriority>(priorityObj.ToString(), out var priority))
            {
                var modifiedNotification = notification.With(builder => builder.WithPriority(priority));

                return ActionExecutionResult.Success(new Dictionary<string, object>
                {
                    ["action"] = "set_priority",
                    ["new_priority"] = priority.ToString()
                }, modifiedNotification);
            }

            return ActionExecutionResult.Failure("Invalid priority value in action parameters");
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error setting priority: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a set channels action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteSetChannelsActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (action.Parameters.TryGetValue("channels", out var channelsObj) && 
                channelsObj is IEnumerable<object> channelsList)
            {
                var channels = new List<NotificationChannel>();
                
                foreach (var channelObj in channelsList)
                {
                    if (Enum.TryParse<NotificationChannel>(channelObj.ToString(), out var channel))
                    {
                        channels.Add(channel);
                    }
                }

                var modifiedNotification = notification with { PreferredChannels = channels };

                return ActionExecutionResult.Success(new Dictionary<string, object>
                {
                    ["action"] = "set_channels",
                    ["channels"] = channels.Select(c => c.ToString()).ToArray()
                }, modifiedNotification);
            }

            return ActionExecutionResult.Failure("No valid channels provided in action parameters");
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error setting channels: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a delay delivery action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteDelayDeliveryActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (action.Parameters.TryGetValue("delay", out var delayObj) && 
                TimeSpan.TryParse(delayObj.ToString(), out var delay))
            {
                var scheduledFor = DateTime.UtcNow.Add(delay);
                var modifiedNotification = notification.With(builder => builder.WithScheduledFor(scheduledFor));

                return ActionExecutionResult.Success(new Dictionary<string, object>
                {
                    ["action"] = "delay_delivery",
                    ["scheduled_for"] = scheduledFor,
                    ["delay"] = delay.ToString()
                }, modifiedNotification);
            }

            return ActionExecutionResult.Failure("Invalid delay value in action parameters");
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error delaying delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a cancel delivery action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteCancelDeliveryActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // This would typically delegate to the notification service to cancel the notification
            _logger.LogDebug("Executing cancel delivery action for notification {NotificationId}", notification.Id);
            
            return ActionExecutionResult.Success(new Dictionary<string, object>
            {
                ["action"] = "cancel_delivery",
                ["notification_id"] = notification.Id
            });
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error canceling delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a webhook action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteWebhookActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            if (action.Parameters.TryGetValue("url", out var urlObj))
            {
                var url = urlObj.ToString();
                if (string.IsNullOrEmpty(url))
                {
                    return ActionExecutionResult.Failure("Webhook URL is required");
                }

                // This would typically make an HTTP request to the webhook URL
                _logger.LogDebug("Executing webhook action for notification {NotificationId} to {Url}", notification.Id, url);
                
                return ActionExecutionResult.Success(new Dictionary<string, object>
                {
                    ["action"] = "execute_webhook",
                    ["url"] = url,
                    ["notification_id"] = notification.Id
                });
            }

            return ActionExecutionResult.Failure("Webhook URL not provided in action parameters");
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error executing webhook: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a log event action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteLogEventActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var message = action.Parameters.TryGetValue("message", out var messageObj) 
                ? messageObj.ToString() 
                : $"Workflow action executed for notification {notification.Id}";

            var level = action.Parameters.TryGetValue("level", out var levelObj) && 
                       Enum.TryParse<LogLevel>(levelObj.ToString(), out var logLevel)
                ? logLevel 
                : LogLevel.Information;

            _logger.Log(logLevel, message);
            
            return ActionExecutionResult.Success(new Dictionary<string, object>
            {
                ["action"] = "log_event",
                ["message"] = message,
                ["level"] = level.ToString()
            });
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error logging event: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes an escalate action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteEscalateActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // This would typically delegate to the escalation service
            _logger.LogDebug("Executing escalate action for notification {NotificationId}", notification.Id);
            
            return ActionExecutionResult.Success(new Dictionary<string, object>
            {
                ["action"] = "escalate",
                ["notification_id"] = notification.Id
            });
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error escalating notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes an aggregate action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteAggregateActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // This would typically delegate to the aggregation service
            _logger.LogDebug("Executing aggregate action for notification {NotificationId}", notification.Id);
            
            return ActionExecutionResult.Success(new Dictionary<string, object>
            {
                ["action"] = "aggregate",
                ["notification_id"] = notification.Id
            });
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error aggregating notification: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a custom action.
    /// </summary>
    private async Task<ActionExecutionResult> ExecuteCustomActionAsync(WorkflowAction action, NotificationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            // This would typically delegate to a custom action handler
            _logger.LogDebug("Executing custom action for notification {NotificationId}", notification.Id);
            
            return ActionExecutionResult.Success(new Dictionary<string, object>
            {
                ["action"] = "custom",
                ["notification_id"] = notification.Id
            });
        }
        catch (Exception ex)
        {
            return ActionExecutionResult.Failure($"Error executing custom action: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates escalation actions for a failed notification.
    /// </summary>
    private List<EscalationAction> CreateEscalationActions(NotificationEvent notification, List<DeliveryAttempt> failedAttempts)
    {
        var escalationActions = new List<EscalationAction>();

        // Add escalation channels if specified
        foreach (var channel in notification.DeliveryOptions.EscalationChannels)
        {
            escalationActions.Add(new EscalationAction
            {
                Id = Guid.NewGuid().ToString(),
                Type = EscalationActionType.SendToEscalationChannel,
                Name = $"Escalate to {channel}",
                Description = $"Send notification to {channel} channel",
                EscalationLevel = 1,
                Parameters = new Dictionary<string, object>
                {
                    ["channel"] = channel.ToString(),
                    ["notification_id"] = notification.Id
                }
            });
        }

        // Add on-call team escalation for critical notifications
        if (notification.Priority == NotificationPriority.Critical)
        {
            escalationActions.Add(new EscalationAction
            {
                Id = Guid.NewGuid().ToString(),
                Type = EscalationActionType.SendToOnCallTeam,
                Name = "Escalate to On-Call Team",
                Description = "Send notification to on-call team",
                EscalationLevel = 1,
                Parameters = new Dictionary<string, object>
                {
                    ["notification_id"] = notification.Id,
                    ["priority"] = "critical"
                }
            });
        }

        return escalationActions;
    }

    /// <summary>
    /// Gets all active rules for a tenant.
    /// </summary>
    private List<NotificationRule> GetActiveRulesForTenant(string tenantId)
    {
        lock (_rulesLock)
        {
            return _rules.Values
                .Where(r => r.TenantId == tenantId && r.IsActive)
                .OrderByDescending(r => r.Priority)
                .ToList();
        }
    }
}

/// <summary>
/// Result of executing a workflow action.
/// </summary>
internal sealed class ActionExecutionResult
{
    /// <summary>
    /// Whether the action was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Modified notification if applicable.
    /// </summary>
    public NotificationEvent? ModifiedNotification { get; init; }

    /// <summary>
    /// Additional metadata about the action execution.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Creates a successful action execution result.
    /// </summary>
    public static ActionExecutionResult Success(Dictionary<string, object>? metadata = null, NotificationEvent? modifiedNotification = null)
    {
        return new ActionExecutionResult
        {
            IsSuccess = true,
            ModifiedNotification = modifiedNotification,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Creates a failed action execution result.
    /// </summary>
    public static ActionExecutionResult Failure(string errorMessage)
    {
        return new ActionExecutionResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}