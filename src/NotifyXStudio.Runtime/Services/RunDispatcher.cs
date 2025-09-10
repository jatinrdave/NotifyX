using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Runtime.Services
{
    /// <summary>
    /// Service for dispatching workflow runs to the execution engine via Kafka.
    /// </summary>
    public class RunDispatcher : IRunDispatcher
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<RunDispatcher> _logger;
        private readonly RunDispatcherOptions _options;

        public RunDispatcher(
            IProducer<string, string> producer,
            ILogger<RunDispatcher> logger,
            RunDispatcherOptions options)
        {
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<string> EnqueueRunAsync(Workflow workflow, JsonElement payload, RunMode mode = RunMode.Manual)
        {
            var runId = Guid.NewGuid().ToString();
            
            var message = new WorkflowRunMessage
            {
                RunId = runId,
                WorkflowId = workflow.Id,
                TenantId = workflow.TenantId,
                Mode = mode,
                Payload = payload,
                QueuedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["workflowName"] = workflow.Name,
                    ["workflowVersion"] = workflow.Version,
                    ["triggeredBy"] = "api"
                }
            };

            try
            {
                var messageJson = JsonSerializer.Serialize(message);
                var kafkaMessage = new Message<string, string>
                {
                    Key = $"{workflow.TenantId}:{runId}",
                    Value = messageJson,
                    Headers = new Headers
                    {
                        { "tenant-id", System.Text.Encoding.UTF8.GetBytes(workflow.TenantId) },
                        { "workflow-id", System.Text.Encoding.UTF8.GetBytes(workflow.Id) },
                        { "run-mode", System.Text.Encoding.UTF8.GetBytes(mode.ToString()) }
                    }
                };

                var result = await _producer.ProduceAsync(_options.TopicName, kafkaMessage);
                
                _logger.LogInformation("Enqueued workflow run {RunId} for workflow {WorkflowId} in mode {Mode}", 
                    runId, workflow.Id, mode);

                return runId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue workflow run {RunId} for workflow {WorkflowId}", 
                    runId, workflow.Id);
                throw;
            }
        }

        public async Task<string> EnqueueScheduledRunAsync(string workflowId, string tenantId, JsonElement payload)
        {
            // For scheduled runs, we would typically get the workflow from a repository
            // For now, create a minimal workflow object
            var workflow = new Workflow
            {
                Id = workflowId,
                TenantId = tenantId,
                Name = "Scheduled Workflow",
                Version = 1
            };

            return await EnqueueRunAsync(workflow, payload, RunMode.Scheduled);
        }

        public async Task<string> EnqueueTriggeredRunAsync(string workflowId, string tenantId, JsonElement payload, string triggerType)
        {
            var workflow = new Workflow
            {
                Id = workflowId,
                TenantId = tenantId,
                Name = "Triggered Workflow",
                Version = 1
            };

            var runId = await EnqueueRunAsync(workflow, payload, RunMode.Triggered);
            
            _logger.LogInformation("Enqueued triggered run {RunId} for workflow {WorkflowId} with trigger type {TriggerType}", 
                runId, workflowId, triggerType);

            return runId;
        }

        public async Task<RunQueueStatus> GetRunStatusAsync(string runId)
        {
            // This would typically query a status store or database
            // For now, return a placeholder status
            return new RunQueueStatus
            {
                RunId = runId,
                Status = RunStatus.Pending,
                QueuedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> CancelQueuedRunAsync(string runId)
        {
            try
            {
                // This would typically send a cancellation message or update status
                _logger.LogInformation("Cancellation requested for run {RunId}", runId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel queued run {RunId}", runId);
                return false;
            }
        }

        public async Task<QueueStatistics> GetQueueStatisticsAsync(string? tenantId = null)
        {
            // This would typically query Kafka metrics or a monitoring system
            // For now, return placeholder statistics
            return new QueueStatistics
            {
                PendingRuns = 0,
                RunningRuns = 0,
                CompletedRuns = 0,
                FailedRuns = 0,
                AverageQueueTimeMs = 0,
                AverageExecutionTimeMs = 0,
                RunsByTenant = new Dictionary<string, int>(),
                RunsByStatus = new Dictionary<RunStatus, int>()
            };
        }
    }

    /// <summary>
    /// Configuration options for the run dispatcher.
    /// </summary>
    public class RunDispatcherOptions
    {
        public string TopicName { get; init; } = "notifyxstudio-runs";
        public int MessageTimeoutMs { get; init; } = 30000;
        public bool EnableIdempotence { get; init; } = true;
        public Dictionary<string, object> KafkaConfig { get; init; } = new();
    }

    /// <summary>
    /// Message sent to Kafka for workflow run execution.
    /// </summary>
    public class WorkflowRunMessage
    {
        public string RunId { get; init; } = string.Empty;
        public string WorkflowId { get; init; } = string.Empty;
        public string TenantId { get; init; } = string.Empty;
        public RunMode Mode { get; init; }
        public JsonElement Payload { get; init; }
        public DateTime QueuedAt { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; init; } = new();
    }
}