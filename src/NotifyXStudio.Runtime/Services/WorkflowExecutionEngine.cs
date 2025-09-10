using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Connectors;
using NotifyXStudio.Core.Services;

namespace NotifyXStudio.Runtime.Services
{
    /// <summary>
    /// Engine for executing workflow runs with support for parallel execution and error handling.
    /// </summary>
    public class WorkflowExecutionEngine : IWorkflowExecutionEngine
    {
        private readonly IConnectorFactory _connectorFactory;
        private readonly ICredentialService _credentialService;
        private readonly IExpressionEngine _expressionEngine;
        private readonly ILogger<WorkflowExecutionEngine> _logger;
        private readonly Dictionary<string, CancellationTokenSource> _runningExecutions = new();

        public WorkflowExecutionEngine(
            IConnectorFactory connectorFactory,
            ICredentialService credentialService,
            IExpressionEngine expressionEngine,
            ILogger<WorkflowExecutionEngine> logger)
        {
            _connectorFactory = connectorFactory ?? throw new ArgumentNullException(nameof(connectorFactory));
            _credentialService = credentialService ?? throw new ArgumentNullException(nameof(credentialService));
            _expressionEngine = expressionEngine ?? throw new ArgumentNullException(nameof(expressionEngine));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<WorkflowRunResult> ExecuteAsync(WorkflowRun run, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var runId = run.Id;

            _logger.LogInformation("Starting workflow execution for run {RunId}, workflow {WorkflowId}", 
                runId, run.WorkflowId);

            try
            {
                // Create cancellation token source for this run
                var runCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _runningExecutions[runId] = runCancellationTokenSource;

                // Get workflow definition
                var workflow = await GetWorkflowAsync(run.WorkflowId, run.TenantId);
                if (workflow == null)
                {
                    throw new InvalidOperationException($"Workflow {run.WorkflowId} not found");
                }

                // Validate workflow
                var validation = await ValidateWorkflowAsync(workflow);
                if (!validation.IsValid)
                {
                    throw new InvalidOperationException($"Workflow validation failed: {string.Join(", ", validation.Errors)}");
                }

                // Get execution plan
                var executionPlan = await GetExecutionPlanAsync(workflow);

                // Execute workflow
                var result = await ExecuteWorkflowAsync(workflow, run, executionPlan, runCancellationTokenSource.Token);

                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;

                _logger.LogInformation("Workflow execution completed for run {RunId} in {Duration}ms with status {Status}", 
                    runId, duration, result.Status);

                return result with
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    DurationMs = (long)duration
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Workflow execution cancelled for run {RunId}", runId);
                return new WorkflowRunResult
                {
                    RunId = runId,
                    Status = RunStatus.Cancelled,
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds
                };
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;

                _logger.LogError(ex, "Workflow execution failed for run {RunId} after {Duration}ms", runId, duration);

                return new WorkflowRunResult
                {
                    RunId = runId,
                    Status = RunStatus.Failed,
                    ErrorMessage = ex.Message,
                    StartTime = startTime,
                    EndTime = endTime,
                    DurationMs = (long)duration
                };
            }
            finally
            {
                _runningExecutions.Remove(runId);
            }
        }

        public async Task<NodeExecutionResult> ExecuteNodeAsync(
            WorkflowNode node,
            WorkflowRun run,
            Dictionary<string, object> nodeInputs,
            CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow;
            var nodeId = node.Id;

            _logger.LogInformation("Executing node {NodeId} of type {NodeType} for run {RunId}", 
                nodeId, node.Type, run.Id);

            try
            {
                // Get connector adapter
                var adapter = _connectorFactory.Create(node.Type);
                if (adapter == null)
                {
                    throw new InvalidOperationException($"No adapter found for connector type: {node.Type}");
                }

                // Get credentials if needed
                string? credentialSecret = null;
                if (!string.IsNullOrEmpty(node.CredentialId))
                {
                    credentialSecret = await _credentialService.GetDecryptedSecretAsync(node.CredentialId, run.TenantId);
                }

                // Create execution context
                var context = new ConnectorExecutionContext
                {
                    TenantId = run.TenantId,
                    NodeConfig = JsonSerializer.SerializeToElement(node.Config),
                    Inputs = JsonSerializer.SerializeToElement(nodeInputs),
                    CredentialSecret = credentialSecret,
                    RunMetadata = new RunMetadata
                    {
                        RunId = run.Id,
                        WorkflowId = run.WorkflowId,
                        NodeId = nodeId,
                        MaxRetries = node.RetryConfig.MaxRetries,
                        RetryDelayMs = node.RetryConfig.InitialDelayMs
                    }
                };

                // Execute with retry logic
                var result = await ExecuteWithRetryAsync(adapter, context, node.RetryConfig, cancellationToken);

                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;

                _logger.LogInformation("Node {NodeId} execution completed in {Duration}ms with status {Status}", 
                    nodeId, duration, result.Success ? "Success" : "Failed");

                return new NodeExecutionResult
                {
                    RunId = run.Id,
                    NodeId = nodeId,
                    Status = result.Success ? ExecutionStatus.Success : ExecutionStatus.Failed,
                    Input = JsonSerializer.SerializeToElement(nodeInputs),
                    Output = result.Success ? result.Output : null,
                    ErrorMessage = result.ErrorMessage,
                    StartTime = startTime,
                    EndTime = endTime,
                    DurationMs = (long)duration,
                    Attempt = 1,
                    Metadata = result.Metadata
                };
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalMilliseconds;

                _logger.LogError(ex, "Node {NodeId} execution failed after {Duration}ms", nodeId, duration);

                return new NodeExecutionResult
                {
                    RunId = run.Id,
                    NodeId = nodeId,
                    Status = ExecutionStatus.Failed,
                    Input = JsonSerializer.SerializeToElement(nodeInputs),
                    ErrorMessage = ex.Message,
                    StartTime = startTime,
                    EndTime = endTime,
                    DurationMs = (long)duration,
                    Attempt = 1
                };
            }
        }

        public async Task<ValidationResult> ValidateWorkflowAsync(Workflow workflow)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Check for at least one node
            if (workflow.Nodes.Count == 0)
            {
                errors.Add("Workflow must have at least one node");
            }

            // Check for trigger nodes
            var triggerNodes = workflow.Nodes.Where(n => n.Type.Contains("trigger")).ToList();
            if (triggerNodes.Count == 0)
            {
                errors.Add("Workflow must have at least one trigger node");
            }

            // Check for orphaned nodes
            var connectedNodeIds = new HashSet<string>();
            foreach (var edge in workflow.Edges)
            {
                connectedNodeIds.Add(edge.From);
                connectedNodeIds.Add(edge.To);
            }

            var orphanedNodes = workflow.Nodes.Where(n => 
                !n.Type.Contains("trigger") && !connectedNodeIds.Contains(n.Id)).ToList();

            if (orphanedNodes.Count > 0)
            {
                warnings.Add($"Found {orphanedNodes.Count} orphaned nodes that are not connected");
            }

            // Validate node configurations
            foreach (var node in workflow.Nodes)
            {
                var nodeValidation = await ValidateNodeAsync(node);
                errors.AddRange(nodeValidation.Errors);
                warnings.AddRange(nodeValidation.Warnings);
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }

        public async Task<ExecutionPlan> GetExecutionPlanAsync(Workflow workflow)
        {
            var steps = new List<ExecutionStep>();
            var dependencies = new Dictionary<string, List<string>>();
            var nodeMap = workflow.Nodes.ToDictionary(n => n.Id, n => n);

            // Build dependency graph
            foreach (var edge in workflow.Edges)
            {
                if (!dependencies.ContainsKey(edge.To))
                {
                    dependencies[edge.To] = new List<string>();
                }
                dependencies[edge.To].Add(edge.From);
            }

            // Create execution steps
            var order = 0;
            foreach (var node in workflow.Nodes)
            {
                var nodeDependencies = dependencies.GetValueOrDefault(node.Id, new List<string>());
                var canRunInParallel = nodeDependencies.Count <= 1; // Simplified parallel logic

                steps.Add(new ExecutionStep
                {
                    NodeId = node.Id,
                    NodeType = node.Type,
                    Order = order++,
                    Dependencies = nodeDependencies,
                    CanRunInParallel = canRunInParallel,
                    EstimatedDurationMs = EstimateNodeDuration(node)
                });
            }

            // Sort by dependencies
            steps = TopologicalSort(steps, dependencies);

            return new ExecutionPlan
            {
                Steps = steps,
                Dependencies = dependencies,
                ParallelGroups = IdentifyParallelGroups(steps),
                EstimatedDurationMs = steps.Sum(s => s.EstimatedDurationMs),
                Metadata = new Dictionary<string, object>
                {
                    ["totalNodes"] = workflow.Nodes.Count,
                    ["totalEdges"] = workflow.Edges.Count,
                    ["parallelSteps"] = steps.Count(s => s.CanRunInParallel)
                }
            };
        }

        public async Task<bool> CancelExecutionAsync(string runId, CancellationToken cancellationToken = default)
        {
            if (_runningExecutions.TryGetValue(runId, out var cts))
            {
                cts.Cancel();
                _logger.LogInformation("Cancellation requested for run {RunId}", runId);
                return true;
            }

            return false;
        }

        private async Task<WorkflowRunResult> ExecuteWorkflowAsync(
            Workflow workflow,
            WorkflowRun run,
            ExecutionPlan executionPlan,
            CancellationToken cancellationToken)
        {
            var nodeResults = new List<NodeExecutionResult>();
            var nodeOutputs = new Dictionary<string, object>();
            var workflowOutput = new Dictionary<string, object>();

            // Initialize with run input
            var runInput = JsonSerializer.Deserialize<Dictionary<string, object>>(run.Input.GetRawText()) ?? new();
            nodeOutputs["__run_input__"] = runInput;

            foreach (var step in executionPlan.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var node = workflow.Nodes.First(n => n.Id == step.NodeId);
                
                // Prepare node inputs
                var nodeInputs = await PrepareNodeInputsAsync(node, nodeOutputs, runInput);

                // Execute node
                var nodeResult = await ExecuteNodeAsync(node, run, nodeInputs, cancellationToken);
                nodeResults.Add(nodeResult);

                // Store node output
                if (nodeResult.Status == ExecutionStatus.Success && nodeResult.Output.HasValue)
                {
                    var output = JsonSerializer.Deserialize<Dictionary<string, object>>(nodeResult.Output.Value.GetRawText()) ?? new();
                    nodeOutputs[node.Id] = output;
                }

                // Check for failures
                if (nodeResult.Status == ExecutionStatus.Failed)
                {
                    return new WorkflowRunResult
                    {
                        RunId = run.Id,
                        Status = RunStatus.Failed,
                        ErrorMessage = $"Node {node.Id} failed: {nodeResult.ErrorMessage}",
                        NodeResults = nodeResults,
                        Output = workflowOutput
                    };
                }
            }

            // Prepare final output
            var finalOutputs = nodeResults
                .Where(r => r.Status == ExecutionStatus.Success)
                .ToDictionary(r => r.NodeId, r => JsonSerializer.Deserialize<object>(r.Output?.GetRawText() ?? "{}") ?? new object());

            return new WorkflowRunResult
            {
                RunId = run.Id,
                Status = RunStatus.Completed,
                NodeResults = nodeResults,
                Output = finalOutputs
            };
        }

        private async Task<Dictionary<string, object>> PrepareNodeInputsAsync(
            WorkflowNode node,
            Dictionary<string, object> nodeOutputs,
            Dictionary<string, object> runInput)
        {
            var inputs = new Dictionary<string, object>();

            // Add run input
            foreach (var kvp in runInput)
            {
                inputs[kvp.Key] = kvp.Value;
            }

            // Add outputs from previous nodes
            foreach (var kvp in nodeOutputs)
            {
                if (kvp.Key != "__run_input__")
                {
                    inputs[kvp.Key] = kvp.Value;
                }
            }

            // Evaluate expressions in node config
            if (node.Config.ValueKind == JsonValueKind.Object)
            {
                var configDict = JsonSerializer.Deserialize<Dictionary<string, object>>(node.Config.GetRawText()) ?? new();
                foreach (var kvp in configDict)
                {
                    if (kvp.Value is string strValue && strValue.Contains("{{"))
                    {
                        var evaluated = await _expressionEngine.EvaluateAsync(strValue, inputs);
                        inputs[kvp.Key] = evaluated;
                    }
                    else
                    {
                        inputs[kvp.Key] = kvp.Value;
                    }
                }
            }

            return inputs;
        }

        private async Task<ConnectorExecutionResult> ExecuteWithRetryAsync(
            IConnectorAdapter adapter,
            ConnectorExecutionContext context,
            RetryConfig retryConfig,
            CancellationToken cancellationToken)
        {
            var attempt = 0;
            var delay = retryConfig.InitialDelayMs;

            while (attempt < retryConfig.MaxRetries)
            {
                attempt++;
                
                try
                {
                    var result = await adapter.ExecuteAsync(context, cancellationToken);
                    if (result.Success)
                    {
                        return result;
                    }

                    if (attempt >= retryConfig.MaxRetries)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    if (attempt >= retryConfig.MaxRetries)
                    {
                        return new ConnectorExecutionResult
                        {
                            Success = false,
                            ErrorMessage = ex.Message
                        };
                    }
                }

                // Wait before retry
                if (retryConfig.UseExponentialBackoff)
                {
                    delay = Math.Min(delay * (int)retryConfig.Multiplier, retryConfig.MaxDelayMs);
                }

                await Task.Delay(delay, cancellationToken);
            }

            return new ConnectorExecutionResult
            {
                Success = false,
                ErrorMessage = "Maximum retry attempts exceeded"
            };
        }

        private async Task<ValidationResult> ValidateNodeAsync(WorkflowNode node)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Check if connector adapter exists
            var adapter = _connectorFactory.Create(node.Type);
            if (adapter == null)
            {
                errors.Add($"No adapter found for connector type: {node.Type}");
            }

            // Validate credentials if required
            if (!string.IsNullOrEmpty(node.CredentialId))
            {
                // This would validate credential exists and is accessible
                // Implementation depends on credential service
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings
            };
        }

        private async Task<Workflow?> GetWorkflowAsync(string workflowId, string tenantId)
        {
            // This would typically come from a repository
            // For now, return null to indicate not implemented
            return null;
        }

        private int EstimateNodeDuration(WorkflowNode node)
        {
            // Simple duration estimation based on node type
            return node.Type switch
            {
                var t when t.Contains("http") => 2000,
                var t when t.Contains("notifyx") => 1000,
                var t when t.Contains("slack") => 1500,
                var t when t.Contains("database") => 3000,
                _ => 1000
            };
        }

        private List<ExecutionStep> TopologicalSort(List<ExecutionStep> steps, Dictionary<string, List<string>> dependencies)
        {
            var result = new List<ExecutionStep>();
            var visited = new HashSet<string>();
            var visiting = new HashSet<string>();

            foreach (var step in steps)
            {
                if (!visited.Contains(step.NodeId))
                {
                    Visit(step, steps, dependencies, visited, visiting, result);
                }
            }

            return result;
        }

        private void Visit(
            ExecutionStep step,
            List<ExecutionStep> allSteps,
            Dictionary<string, List<string>> dependencies,
            HashSet<string> visited,
            HashSet<string> visiting,
            List<ExecutionStep> result)
        {
            if (visiting.Contains(step.NodeId))
            {
                throw new InvalidOperationException($"Circular dependency detected involving node {step.NodeId}");
            }

            if (visited.Contains(step.NodeId))
            {
                return;
            }

            visiting.Add(step.NodeId);

            var nodeDependencies = dependencies.GetValueOrDefault(step.NodeId, new List<string>());
            foreach (var depId in nodeDependencies)
            {
                var depStep = allSteps.FirstOrDefault(s => s.NodeId == depId);
                if (depStep != null)
                {
                    Visit(depStep, allSteps, dependencies, visited, visiting, result);
                }
            }

            visiting.Remove(step.NodeId);
            visited.Add(step.NodeId);
            result.Add(step);
        }

        private List<string> IdentifyParallelGroups(List<ExecutionStep> steps)
        {
            var groups = new List<string>();
            var currentGroup = new List<string>();

            foreach (var step in steps)
            {
                if (step.CanRunInParallel)
                {
                    currentGroup.Add(step.NodeId);
                }
                else
                {
                    if (currentGroup.Count > 1)
                    {
                        groups.Add(string.Join(",", currentGroup));
                    }
                    currentGroup.Clear();
                }
            }

            if (currentGroup.Count > 1)
            {
                groups.Add(string.Join(",", currentGroup));
            }

            return groups;
        }
    }
}