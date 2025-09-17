using System.ComponentModel.DataAnnotations;

namespace NotifyXStudio.Core.Models
{
    // User Model
    public record User
    {
        public string Id { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Project Model
    public record Project
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Task Model
    public record Task
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Issue Model
    public record Issue
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Story Model
    public record Story
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Epic Model
    public record Epic
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Subtask Model
    public record Subtask
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? TaskId { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Priority { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Milestone Model
    public record Milestone
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public DateTime DueDate { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Release Model
    public record Release
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Version { get; init; } = string.Empty;
        public DateTime ReleaseDate { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Iteration Model
    public record Iteration
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Tag Model
    public record Tag
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Color { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Role Model
    public record Role
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Permission Model
    public record Permission
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Tenant Model
    public record Tenant
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Notification Model
    public record Notification
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? UserId { get; init; }
        public string Type { get; init; } = string.Empty;
        public bool IsRead { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Event Model
    public record Event
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Type { get; init; } = string.Empty;
        public DateTime EventDate { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // File Model
    public record File
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public long Size { get; init; }
        public string MimeType { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Log Model
    public record Log
    {
        public string Id { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string Level { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Source { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Audit Model
    public record Audit
    {
        public string Id { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
        public string EntityType { get; init; } = string.Empty;
        public string EntityId { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public string UserId { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Config Model
    public record Config
    {
        public string Id { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // System Model
    public record System
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Status Model
    public record Status
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Color { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Monitor Model
    public record Monitor
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Alert Model
    public record Alert
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Severity { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Report Model
    public record Report
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Dashboard Model
    public record Dashboard
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? UserId { get; init; }
        public string Layout { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Integration Model
    public record Integration
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Webhook Model
    public record Webhook
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Events { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Queue Model
    public record Queue
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Repository Model
    public record Repository
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Url { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Branch Model
    public record Branch
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? RepositoryId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Commit Model
    public record Commit
    {
        public string Id { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? RepositoryId { get; init; }
        public string Hash { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Build Model
    public record Build
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime StartedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Deploy Model
    public record Deploy
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime StartedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Environment Model
    public record Environment
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Test Model
    public record Test
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Version Model
    public record Version
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string VersionNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Backup Model
    public record Backup
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime StartedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Compliance Model
    public record Compliance
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ProjectId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Credential Model
    public record Credential
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public string? TenantId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Workflow Execution Model
    public record WorkflowExecution
    {
        public string Id { get; init; } = string.Empty;
        public string? WorkflowId { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime StartedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Workflow Execution Node Model
    public record WorkflowExecutionNode
    {
        public string Id { get; init; } = string.Empty;
        public string? ExecutionId { get; init; }
        public string NodeId { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime StartedAt { get; init; }
        public DateTime? CompletedAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Workflow Execution Edge Model
    public record WorkflowExecutionEdge
    {
        public string Id { get; init; } = string.Empty;
        public string? ExecutionId { get; init; }
        public string SourceNodeId { get; init; } = string.Empty;
        public string TargetNodeId { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Workflow Execution Log Model
    public record WorkflowExecutionLog
    {
        public string Id { get; init; } = string.Empty;
        public string? ExecutionId { get; init; }
        public string Message { get; init; } = string.Empty;
        public string Level { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Workflow Execution Trigger Model
    public record WorkflowExecutionTrigger
    {
        public string Id { get; init; } = string.Empty;
        public string? ExecutionId { get; init; }
        public string TriggerId { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime TriggeredAt { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Workflow Execution Trigger Log Model
    public record WorkflowExecutionTriggerLog
    {
        public string Id { get; init; } = string.Empty;
        public string? TriggerId { get; init; }
        public string Message { get; init; } = string.Empty;
        public string Level { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Workflow Execution Trigger Log Entry Model
    public record WorkflowExecutionTriggerLogEntry
    {
        public string Id { get; init; } = string.Empty;
        public string? LogId { get; init; }
        public string Message { get; init; } = string.Empty;
        public string Level { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public string UpdatedBy { get; init; } = string.Empty;
    }

    // Additional models for stub implementations
    public class SystemInfo
    {
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string HostName { get; set; } = string.Empty;
    }

    public class SystemStatus
    {
        public string Status { get; set; } = "Healthy";
        public DateTime LastCheck { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SystemMetrics
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public int ActiveConnections { get; set; }
    }

    public class ComponentStatus
    {
        public string Component { get; set; } = string.Empty;
        public string Status { get; set; } = "Healthy";
        public DateTime LastCheck { get; set; }
    }

    public class ServiceStatus
    {
        public string Service { get; set; } = string.Empty;
        public string Status { get; set; } = "Running";
        public DateTime LastCheck { get; set; }
    }

    public class DatabaseStatus
    {
        public string Status { get; set; } = "Connected";
        public DateTime LastCheck { get; set; }
        public int ConnectionCount { get; set; }
    }

    public class QueueStatus
    {
        public string Status { get; set; } = "Active";
        public DateTime LastCheck { get; set; }
        public int MessageCount { get; set; }
    }

    public class ExternalServiceStatus
    {
        public string Service { get; set; } = string.Empty;
        public string Status { get; set; } = "Available";
        public DateTime LastCheck { get; set; }
    }

    public class PerformanceStatus
    {
        public double ResponseTime { get; set; }
        public int Throughput { get; set; }
        public double ErrorRate { get; set; }
    }

    public class SecurityStatus
    {
        public string Status { get; set; } = "Secure";
        public DateTime LastCheck { get; set; }
        public int ThreatCount { get; set; }
    }

    public class ComplianceStatus
    {
        public string Status { get; set; } = "Compliant";
        public DateTime LastCheck { get; set; }
        public int ViolationCount { get; set; }
    }

    public class MaintenanceStatus
    {
        public string Status { get; set; } = "Normal";
        public DateTime LastMaintenance { get; set; }
        public DateTime NextMaintenance { get; set; }
    }

    public class MonitoringReport
    {
        public string Id { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string Summary { get; set; } = string.Empty;
    }

    public class MonitoringConfig
    {
        public string Id { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public int CheckInterval { get; set; } = 60;
    }

    public class WebhookLog
    {
        public string Id { get; set; } = string.Empty;
        public string WebhookId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
    }

    public class QueueMessage
    {
        public string Id { get; set; } = string.Empty;
        public string QueueId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RepositoryFile
    {
        public string Id { get; set; } = string.Empty;
        public string RepositoryId { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class RepositoryStats
    {
        public string Id { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public int CommitCount { get; set; }
        public int BranchCount { get; set; }
    }

    public class TestResult
    {
        public string Id { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime ExecutedAt { get; set; }
    }

    public class LogLevel
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    // Additional missing models
    public class Stats
    {
        public string Id { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Settings
    {
        public string Id { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class Deployment
    {
        public string Id { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DeployedAt { get; set; }
    }

    public class VersionHistory
    {
        public string Id { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // Missing models for VersionService
    public class BuildInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime BuildDate { get; set; }
        public string BuildNumber { get; set; } = string.Empty;
        public string CommitHash { get; set; } = string.Empty;
    }

    public class RuntimeInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
    }

    public class EnvironmentInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class UpdateInfo
    {
        public string Id { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = string.Empty;
        public bool IsUpdateAvailable { get; set; }
        public DateTime LastChecked { get; set; }
    }

    // Missing models for WorkflowService
    public class WorkflowStatistics
    {
        public int TotalWorkflows { get; init; }
        public int ActiveWorkflows { get; init; }
        public int InactiveWorkflows { get; init; }
        public int TotalRuns { get; init; }
        public int SuccessfulRuns { get; init; }
        public int FailedRuns { get; init; }
        public double AverageRunDurationMs { get; init; }
        public double SuccessRate { get; init; }
    }

    // Missing statistics classes
    public class BranchStatistics
    {
        public string Id { get; set; } = string.Empty;
        public int TotalBranches { get; set; }
        public int ActiveBranches { get; set; }
        public int MergedBranches { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class CommitStatistics
    {
        public string Id { get; set; } = string.Empty;
        public int TotalCommits { get; set; }
        public int CommitsToday { get; set; }
        public int CommitsThisWeek { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}
