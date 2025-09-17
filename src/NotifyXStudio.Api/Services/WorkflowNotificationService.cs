using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Api.Hubs;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Api.Services
{
    /// <summary>
    /// Service for sending real-time notifications about workflow execution via SignalR.
    /// </summary>
    public class WorkflowNotificationService : IWorkflowNotificationService
    {
        private readonly IHubContext<WorkflowHub> _hubContext;
        private readonly ILogger<WorkflowNotificationService> _logger;

        public WorkflowNotificationService(
            IHubContext<WorkflowHub> hubContext,
            ILogger<WorkflowNotificationService> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async System.Threading.Tasks.Task NotifyRunStatusChangeAsync(string runId, RunStatus status, string? errorMessage = null)
        {
            try
            {
                var notification = new
                {
                    type = "run_status_change",
                    runId,
                    status = status.ToString(),
                    errorMessage,
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"run:{runId}").SendAsync("RunStatusChanged", notification);
                
                _logger.LogDebug("Sent run status change notification for run {RunId} with status {Status}", 
                    runId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send run status change notification for run {RunId}", runId);
            }
        }

        public async System.Threading.Tasks.Task NotifyNodeExecutionAsync(string runId, NodeExecutionResult nodeResult)
        {
            try
            {
                var notification = new
                {
                    type = "node_execution",
                    runId,
                    nodeId = nodeResult.NodeId,
                    status = nodeResult.Status.ToString(),
                    input = nodeResult.Input,
                    output = nodeResult.Output,
                    errorMessage = nodeResult.ErrorMessage,
                    durationMs = nodeResult.DurationMs,
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"run:{runId}").SendAsync("NodeExecuted", notification);
                
                _logger.LogDebug("Sent node execution notification for run {RunId}, node {NodeId} with status {Status}", 
                    runId, nodeResult.NodeId, nodeResult.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send node execution notification for run {RunId}, node {NodeId}", 
                    runId, nodeResult.NodeId);
            }
        }

        public async System.Threading.Tasks.Task NotifyExecutionProgressAsync(string runId, ExecutionProgress progress)
        {
            try
            {
                var notification = new
                {
                    type = "execution_progress",
                    runId,
                    completedNodes = progress.CompletedNodes,
                    totalNodes = progress.TotalNodes,
                    progressPercentage = progress.ProgressPercentage,
                    currentNodeId = progress.CurrentNodeId,
                    currentNodeType = progress.CurrentNodeType,
                    timestamp = progress.Timestamp
                };

                await _hubContext.Clients.Group($"run:{runId}").SendAsync("ExecutionProgress", notification);
                
                _logger.LogDebug("Sent execution progress notification for run {RunId}: {Completed}/{Total} nodes", 
                    runId, progress.CompletedNodes, progress.TotalNodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send execution progress notification for run {RunId}", runId);
            }
        }

        public async System.Threading.Tasks.Task NotifyExecutionCompletedAsync(string runId, WorkflowRunResult result)
        {
            try
            {
                var notification = new
                {
                    type = "execution_completed",
                    runId,
                    status = result.Status.ToString(),
                    errorMessage = result.ErrorMessage,
                    output = result.Output,
                    durationMs = result.DurationMs,
                    nodeResults = result.NodeResults,
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"run:{runId}").SendAsync("ExecutionCompleted", notification);
                
                _logger.LogDebug("Sent execution completed notification for run {RunId} with status {Status}", 
                    runId, result.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send execution completed notification for run {RunId}", runId);
            }
        }

        public async System.Threading.Tasks.Task NotifyExecutionErrorAsync(string runId, string errorMessage, Exception? exception = null)
        {
            try
            {
                var notification = new
                {
                    type = "execution_error",
                    runId,
                    errorMessage,
                    exception = exception?.ToString(),
                    timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.Group($"run:{runId}").SendAsync("ExecutionError", notification);
                
                _logger.LogDebug("Sent execution error notification for run {RunId}: {ErrorMessage}", 
                    runId, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send execution error notification for run {RunId}", runId);
            }
        }

        public async System.Threading.Tasks.Task NotifyWorkflowChangedAsync(string workflowId, string tenantId, WorkflowChangeType changeType)
        {
            try
            {
                var notification = new
                {
                    type = "workflow_changed",
                    workflowId,
                    changeType = changeType.ToString(),
                    timestamp = DateTime.UtcNow
                };

                // Notify all clients in the tenant
                await _hubContext.Clients.Group($"tenant:{tenantId}").SendAsync("WorkflowChanged", notification);
                
                // Also notify clients specifically subscribed to this workflow
                await _hubContext.Clients.Group($"workflow:{workflowId}").SendAsync("WorkflowChanged", notification);
                
                _logger.LogDebug("Sent workflow change notification for workflow {WorkflowId} in tenant {TenantId}: {ChangeType}", 
                    workflowId, tenantId, changeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send workflow change notification for workflow {WorkflowId}", workflowId);
            }
        }

        public async System.Threading.Tasks.Task NotifySystemEventAsync(string tenantId, SystemEvent systemEvent)
        {
            try
            {
                var notification = new
                {
                    type = "system_event",
                    eventId = systemEvent.Id,
                    eventType = systemEvent.Type,
                    message = systemEvent.Message,
                    severity = systemEvent.Severity,
                    data = systemEvent.Data,
                    timestamp = systemEvent.Timestamp
                };

                await _hubContext.Clients.Group($"tenant:{tenantId}").SendAsync("SystemEvent", notification);
                
                _logger.LogDebug("Sent system event notification for tenant {TenantId}: {EventType} - {Message}", 
                    tenantId, systemEvent.Type, systemEvent.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send system event notification for tenant {TenantId}", tenantId);
            }
        }
    }
}