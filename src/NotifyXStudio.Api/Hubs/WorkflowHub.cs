using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Api.Hubs
{
    /// <summary>
    /// SignalR hub for real-time workflow execution updates.
    /// </summary>
    [Authorize]
    public class WorkflowHub : Hub
    {
        private readonly ILogger<WorkflowHub> _logger;

        public WorkflowHub(ILogger<WorkflowHub> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Called when a client connects to the hub.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();

            _logger.LogInformation("Client {ConnectionId} connected for tenant {TenantId}, user {UserId}", 
                Context.ConnectionId, tenantId, userId);

            // Add client to tenant group for tenant-specific updates
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenantId}");

            // Add client to user group for user-specific updates
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var tenantId = GetTenantId();
            var userId = GetUserId();

            _logger.LogInformation("Client {ConnectionId} disconnected for tenant {TenantId}, user {UserId}", 
                Context.ConnectionId, tenantId, userId);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to updates for a specific workflow run.
        /// </summary>
        public async Task SubscribeToRun(string runId)
        {
            var tenantId = GetTenantId();
            
            _logger.LogDebug("Client {ConnectionId} subscribing to run {RunId} for tenant {TenantId}", 
                Context.ConnectionId, runId, tenantId);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"run:{runId}");
        }

        /// <summary>
        /// Unsubscribe from updates for a specific workflow run.
        /// </summary>
        public async Task UnsubscribeFromRun(string runId)
        {
            _logger.LogDebug("Client {ConnectionId} unsubscribing from run {RunId}", 
                Context.ConnectionId, runId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"run:{runId}");
        }

        /// <summary>
        /// Subscribe to updates for a specific workflow.
        /// </summary>
        public async Task SubscribeToWorkflow(string workflowId)
        {
            var tenantId = GetTenantId();
            
            _logger.LogDebug("Client {ConnectionId} subscribing to workflow {WorkflowId} for tenant {TenantId}", 
                Context.ConnectionId, workflowId, tenantId);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"workflow:{workflowId}");
        }

        /// <summary>
        /// Unsubscribe from updates for a specific workflow.
        /// </summary>
        public async Task UnsubscribeFromWorkflow(string workflowId)
        {
            _logger.LogDebug("Client {ConnectionId} unsubscribing from workflow {WorkflowId}", 
                Context.ConnectionId, workflowId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"workflow:{workflowId}");
        }

        private string GetTenantId()
        {
            return Context.User?.FindFirst("tenant_id")?.Value ?? "unknown";
        }

        private string GetUserId()
        {
            return Context.User?.FindFirst("sub")?.Value ?? "unknown";
        }
    }
}