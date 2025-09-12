using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NotifyXStudio.Core.Models;
using NotifyXStudio.Core.Services;
using NotifyXStudio.Runtime.Services;
using Xunit;

namespace NotifyXStudio.IntegrationTests
{
    /// <summary>
    /// Integration tests for workflow execution engine.
    /// </summary>
    public class WorkflowExecutionTests : BaseIntegrationTest
    {
        private readonly IWorkflowExecutionEngine _executionEngine;
        private readonly IConnectorFactory _connectorFactory;

        public WorkflowExecutionTests()
        {
            _executionEngine = GetService<IWorkflowExecutionEngine>();
            _connectorFactory = GetService<IConnectorFactory>();
        }

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            base.ConfigureTestServices(services);
            
            // Register test implementations
            services.AddScoped<ICredentialService, TestCredentialService>();
            services.AddScoped<IExpressionEngine, TestExpressionEngine>();
        }

        [Fact]
        public async Task ExecuteWorkflow_WithValidWorkflow_ShouldSucceed()
        {
            // Arrange
            var workflow = CreateTestWorkflow();
            var run = CreateTestRun(workflow);

            // Act
            var result = await _executionEngine.ExecuteAsync(run);

            // Assert
            result.Should().NotBeNull();
            result.RunId.Should().Be(run.Id);
            result.Status.Should().Be(RunStatus.Completed);
            result.NodeResults.Should().HaveCount(workflow.Nodes.Count);
        }

        [Fact]
        public async Task ExecuteWorkflow_WithInvalidNode_ShouldFail()
        {
            // Arrange
            var workflow = CreateTestWorkflowWithInvalidNode();
            var run = CreateTestRun(workflow);

            // Act
            var result = await _executionEngine.ExecuteAsync(run);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(RunStatus.Failed);
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ValidateWorkflow_WithValidWorkflow_ShouldPass()
        {
            // Arrange
            var workflow = CreateTestWorkflow();

            // Act
            var result = await _executionEngine.ValidateWorkflowAsync(workflow);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateWorkflow_WithNoNodes_ShouldFail()
        {
            // Arrange
            var workflow = new Workflow
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Empty Workflow",
                TenantId = "test-tenant",
                Nodes = new List<WorkflowNode>(),
                Edges = new List<WorkflowEdge>(),
                Triggers = new List<WorkflowTrigger>()
            };

            // Act
            var result = await _executionEngine.ValidateWorkflowAsync(workflow);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Workflow must have at least one node");
        }

        [Fact]
        public async Task GetExecutionPlan_WithValidWorkflow_ShouldReturnPlan()
        {
            // Arrange
            var workflow = CreateTestWorkflow();

            // Act
            var plan = await _executionEngine.GetExecutionPlanAsync(workflow);

            // Assert
            plan.Should().NotBeNull();
            plan.Steps.Should().HaveCount(workflow.Nodes.Count);
            plan.Dependencies.Should().NotBeNull();
            plan.EstimatedDurationMs.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ExecuteNode_WithValidNode_ShouldSucceed()
        {
            // Arrange
            var workflow = CreateTestWorkflow();
            var run = CreateTestRun(workflow);
            var node = workflow.Nodes.First();
            var inputs = new Dictionary<string, object>
            {
                ["testInput"] = "testValue"
            };

            // Act
            var result = await _executionEngine.ExecuteNodeAsync(node, run, inputs);

            // Assert
            result.Should().NotBeNull();
            result.NodeId.Should().Be(node.Id);
            result.Status.Should().Be(ExecutionStatus.Success);
            result.Input.Should().NotBeNull();
        }

        private Workflow CreateTestWorkflow()
        {
            var node1 = new WorkflowNode
            {
                Id = "node-1",
                Type = "notifyx.sendNotification",
                Category = "action",
                Label = "Send Notification",
                Position = new NodePosition { X = 100, Y = 100 },
                Config = JsonSerializer.SerializeToElement(new
                {
                    channel = "email",
                    recipient = "test@example.com",
                    message = "Hello World"
                }),
                IsEnabled = true
            };

            var node2 = new WorkflowNode
            {
                Id = "node-2",
                Type = "http.request",
                Category = "action",
                Label = "HTTP Request",
                Position = new NodePosition { X = 300, Y = 100 },
                Config = JsonSerializer.SerializeToElement(new
                {
                    url = "https://api.example.com/webhook",
                    method = "POST"
                }),
                IsEnabled = true
            };

            var edge = new WorkflowEdge
            {
                From = node1.Id,
                To = node2.Id,
                Condition = ""
            };

            return new Workflow
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test Workflow",
                TenantId = "test-tenant",
                Nodes = new List<WorkflowNode> { node1, node2 },
                Edges = new List<WorkflowEdge> { edge },
                Triggers = new List<WorkflowTrigger>
                {
                    new WorkflowTrigger
                    {
                        Type = TriggerType.Manual,
                        Config = JsonSerializer.SerializeToElement(new { }),
                        IsActive = true
                    }
                },
                Version = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "test-user",
                UpdatedBy = "test-user"
            };
        }

        private Workflow CreateTestWorkflowWithInvalidNode()
        {
            var invalidNode = new WorkflowNode
            {
                Id = "invalid-node",
                Type = "invalid.connector",
                Category = "action",
                Label = "Invalid Node",
                Position = new NodePosition { X = 100, Y = 100 },
                Config = JsonSerializer.SerializeToElement(new { }),
                IsEnabled = true
            };

            return new Workflow
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Invalid Workflow",
                TenantId = "test-tenant",
                Nodes = new List<WorkflowNode> { invalidNode },
                Edges = new List<WorkflowEdge>(),
                Triggers = new List<WorkflowTrigger>
                {
                    new WorkflowTrigger
                    {
                        Type = TriggerType.Manual,
                        Config = JsonSerializer.SerializeToElement(new { }),
                        IsActive = true
                    }
                },
                Version = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "test-user",
                UpdatedBy = "test-user"
            };
        }

        private WorkflowRun CreateTestRun(Workflow workflow)
        {
            return new WorkflowRun
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflow.Id,
                TenantId = workflow.TenantId,
                Status = RunStatus.Pending,
                Mode = RunMode.Test,
                Input = JsonSerializer.SerializeToElement(new
                {
                    testData = "testValue"
                }),
                StartTime = DateTime.UtcNow,
                TriggeredBy = "test-user"
            };
        }
    }

    /// <summary>
    /// Test implementation of credential service.
    /// </summary>
    public class TestCredentialService : ICredentialService
    {
        public Task<string?> GetDecryptedSecretAsync(string credentialId, string tenantId)
        {
            return Task.FromResult<string?>("test-secret");
        }

        public Task<bool> ValidateCredentialAsync(string credentialId, string tenantId)
        {
            return Task.FromResult(true);
        }

        public Task<CredentialMetadata?> GetCredentialMetadataAsync(string credentialId, string tenantId)
        {
            return Task.FromResult(new CredentialMetadata
            {
                Id = credentialId,
                TenantId = tenantId,
                ConnectorType = "test",
                Name = "Test Credential",
                IsActive = true
            });
        }

        public Task<bool> RefreshCredentialAsync(string credentialId, string tenantId)
        {
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Test implementation of expression engine.
    /// </summary>
    public class TestExpressionEngine : IExpressionEngine
    {
        public Task<object> EvaluateAsync(string expression, Dictionary<string, object> context)
        {
            // Simple template resolution for testing
            var result = expression;
            foreach (var kvp in context)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value.ToString() ?? "");
            }
            return Task.FromResult<object>(result);
        }

        public Task<ValidationResult> ValidateExpressionAsync(string expression)
        {
            return Task.FromResult(new ValidationResult
            {
                IsValid = true,
                Errors = new List<string>(),
                Warnings = new List<string>()
            });
        }

        public Task<string> ResolveTemplateAsync(string template, Dictionary<string, object> context)
        {
            var result = template;
            foreach (var kvp in context)
            {
                result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value.ToString() ?? "");
            }
            return Task.FromResult(result);
        }

        public Task<ExpressionContext> GetExpressionContextAsync()
        {
            return Task.FromResult(new ExpressionContext
            {
                Variables = new Dictionary<string, object>(),
                Functions = new List<ExpressionFunction>(),
                AvailableTypes = new List<string> { "string", "number", "boolean", "object", "array" }
            });
        }
    }
}