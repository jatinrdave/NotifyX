using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Runtime.Services;

namespace NotifyXStudio.Runtime.Workers
{
    /// <summary>
    /// Background worker service that consumes workflow run messages from Kafka and executes them.
    /// </summary>
    public class WorkflowWorker : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IWorkflowExecutionEngine _executionEngine;
        private readonly IRunService _runService;
        private readonly ILogger<WorkflowWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly WorkflowWorkerOptions _options;

        public WorkflowWorker(
            IConsumer<string, string> consumer,
            IWorkflowExecutionEngine executionEngine,
            IRunService runService,
            ILogger<WorkflowWorker> logger,
            IServiceProvider serviceProvider,
            WorkflowWorkerOptions options)
        {
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _executionEngine = executionEngine ?? throw new ArgumentNullException(nameof(executionEngine));
            _runService = runService ?? throw new ArgumentNullException(nameof(runService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Workflow worker starting...");

            try
            {
                _consumer.Subscribe(_options.TopicName);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult?.Message?.Value != null)
                        {
                            await ProcessWorkflowRunAsync(consumeResult.Message.Value, stoppingToken);
                            _consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message from Kafka");
                        await Task.Delay(1000, stoppingToken); // Brief delay before retry
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error in workflow worker");
                        await Task.Delay(5000, stoppingToken); // Longer delay for unexpected errors
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Workflow worker stopping...");
            }
            finally
            {
                _consumer.Close();
                _logger.LogInformation("Workflow worker stopped");
            }
        }

        private async Task ProcessWorkflowRunAsync(string messageValue, CancellationToken cancellationToken)
        {
            try
            {
                var runMessage = JsonSerializer.Deserialize<WorkflowRunMessage>(messageValue);
                if (runMessage == null)
                {
                    _logger.LogWarning("Failed to deserialize workflow run message: {Message}", messageValue);
                    return;
                }

                _logger.LogInformation("Processing workflow run {RunId} for workflow {WorkflowId}", 
                    runMessage.RunId, runMessage.WorkflowId);

                // Update run status to running
                await _runService.UpdateRunStatusAsync(runMessage.RunId, RunStatus.Running);

                // Get the workflow run
                var run = await _runService.GetByIdAsync(runMessage.RunId, runMessage.TenantId);
                if (run == null)
                {
                    _logger.LogError("Workflow run {RunId} not found", runMessage.RunId);
                    await _runService.UpdateRunStatusAsync(runMessage.RunId, RunStatus.Failed, "Run not found");
                    return;
                }

                // Execute the workflow
                var result = await _executionEngine.ExecuteAsync(run, cancellationToken);

                // Update run with results
                await UpdateRunWithResultsAsync(run, result);

                _logger.LogInformation("Completed processing workflow run {RunId} with status {Status}", 
                    runMessage.RunId, result.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing workflow run message: {Message}", messageValue);
                
                // Try to extract run ID from message for error reporting
                try
                {
                    var runMessage = JsonSerializer.Deserialize<WorkflowRunMessage>(messageValue);
                    if (runMessage != null)
                    {
                        await _runService.UpdateRunStatusAsync(runMessage.RunId, RunStatus.Failed, ex.Message);
                    }
                }
                catch
                {
                    // If we can't even deserialize the message, just log the error
                    _logger.LogError("Could not extract run ID from failed message");
                }
            }
        }

        private async Task UpdateRunWithResultsAsync(WorkflowRun run, WorkflowRunResult result)
        {
            try
            {
                // Update run status
                await _runService.UpdateRunStatusAsync(run.Id, result.Status, result.ErrorMessage);

                // Update node results
                foreach (var nodeResult in result.NodeResults)
                {
                    await _runService.UpdateNodeResultAsync(nodeResult);
                }

                // Update final run output if successful
                if (result.Status == RunStatus.Completed)
                {
                    // This would typically update the run with final output
                    // Implementation depends on the run service
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating run {RunId} with results", run.Id);
            }
        }
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

    /// <summary>
    /// Configuration options for the workflow worker.
    /// </summary>
    public class WorkflowWorkerOptions
    {
        public string TopicName { get; init; } = "notifyxstudio-runs";
        public string GroupId { get; init; } = "notifyxstudio-workers";
        public int MaxConcurrentRuns { get; init; } = 10;
        public int MessageTimeoutMs { get; init; } = 30000;
        public bool EnableAutoCommit { get; init; } = false;
        public Dictionary<string, object> KafkaConfig { get; init; } = new();
    }
}