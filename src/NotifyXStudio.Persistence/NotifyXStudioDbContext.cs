using Microsoft.EntityFrameworkCore;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence
{
    public class NotifyXStudioDbContext : DbContext
    {
        public NotifyXStudioDbContext(DbContextOptions<NotifyXStudioDbContext> options) : base(options)
        {
        }

        // Workflow entities
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowRun> WorkflowRuns { get; set; }
        public DbSet<ConnectorRegistryEntry> Connectors { get; set; }
        public DbSet<NodeExecutionResult> NodeResults { get; set; }
        
        // Core system entities
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<WorkTask> WorkTasks { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Story> Stories { get; set; }
        public DbSet<Epic> Epics { get; set; }
        public DbSet<Subtask> Subtasks { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<Iteration> Iterations { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        
    // System entities
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Core.Models.File> Files { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<Audit> Audits { get; set; }
    public DbSet<Config> Configs { get; set; }
    public DbSet<Core.Models.System> Systems { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Core.Models.Monitor> Monitors { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Dashboard> Dashboards { get; set; }
    public DbSet<Integration> Integrations { get; set; }
    public DbSet<Webhook> Webhooks { get; set; }
    public DbSet<Queue> Queues { get; set; }
    
    // Development entities
    public DbSet<Repository> Repositories { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Commit> Commits { get; set; }
    public DbSet<Build> Builds { get; set; }
    public DbSet<Deploy> Deploys { get; set; }
    public DbSet<Core.Models.Environment> Environments { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<Core.Models.Version> Versions { get; set; }
    public DbSet<Backup> Backups { get; set; }
    public DbSet<Compliance> Compliances { get; set; }
    public DbSet<Credential> Credentials { get; set; }
        
        // Workflow execution entities
        public DbSet<WorkflowExecution> WorkflowExecutions { get; set; }
        public DbSet<WorkflowExecutionNode> WorkflowExecutionNodes { get; set; }
        public DbSet<WorkflowExecutionEdge> WorkflowExecutionEdges { get; set; }
        public DbSet<WorkflowExecutionLog> WorkflowExecutionLogs { get; set; }
        public DbSet<WorkflowExecutionTrigger> WorkflowExecutionTriggers { get; set; }
        public DbSet<WorkflowExecutionTriggerLog> WorkflowExecutionTriggerLogs { get; set; }
        public DbSet<WorkflowExecutionTriggerLogEntry> WorkflowExecutionTriggerLogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure all entities with common patterns
            ConfigureCoreEntities(modelBuilder);
            ConfigureWorkflowEntities(modelBuilder);
            ConfigureSystemEntities(modelBuilder);
            ConfigureDevelopmentEntities(modelBuilder);
        }

        private void ConfigureCoreEntities(ModelBuilder modelBuilder)
        {
            // User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TenantId).HasMaxLength(50);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Project entity
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.TenantId).HasMaxLength(50);
            });

            // WorkTask entity
            modelBuilder.Entity<WorkTask>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Issue entity
            modelBuilder.Entity<Issue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Story entity
            modelBuilder.Entity<Story>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Epic entity
            modelBuilder.Entity<Epic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Subtask entity
            modelBuilder.Entity<Subtask>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
                entity.HasOne<WorkTask>()
                      .WithMany()
                      .HasForeignKey(e => e.WorkTaskId);
            });

            // Milestone entity
            modelBuilder.Entity<Milestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Release entity
            modelBuilder.Entity<Release>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Iteration entity
            modelBuilder.Entity<Iteration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Tag entity
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Color).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TenantId).HasMaxLength(50);
            });

            // Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TenantId).HasMaxLength(50);
            });

            // Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TenantId).HasMaxLength(50);
            });

            // Tenant entity
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });
        }

        private void ConfigureWorkflowEntities(ModelBuilder modelBuilder)
        {
            // Workflow entity
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            });

            // WorkflowRun entity
            modelBuilder.Entity<WorkflowRun>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WorkflowId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.HasOne<Workflow>()
                      .WithMany()
                      .HasForeignKey(e => e.WorkflowId);
            });

            // ConnectorRegistryEntry entity
            modelBuilder.Entity<ConnectorRegistryEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            });

            // NodeExecutionResult entity
            modelBuilder.Entity<NodeExecutionResult>(entity =>
            {
                entity.HasKey(e => new { e.RunId, e.NodeId });
                entity.Property(e => e.NodeId).IsRequired();
                entity.Property(e => e.RunId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });

            // WorkflowExecution entity
            modelBuilder.Entity<WorkflowExecution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Workflow>()
                      .WithMany()
                      .HasForeignKey(e => e.WorkflowId);
            });

            // WorkflowExecutionNode entity
            modelBuilder.Entity<WorkflowExecutionNode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NodeId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<WorkflowExecution>()
                      .WithMany()
                      .HasForeignKey(e => e.ExecutionId);
            });

            // WorkflowExecutionEdge entity
            modelBuilder.Entity<WorkflowExecutionEdge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SourceNodeId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TargetNodeId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<WorkflowExecution>()
                      .WithMany()
                      .HasForeignKey(e => e.ExecutionId);
            });

            // WorkflowExecutionLog entity
            modelBuilder.Entity<WorkflowExecutionLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Level).IsRequired().HasMaxLength(20);
                entity.HasOne<WorkflowExecution>()
                      .WithMany()
                      .HasForeignKey(e => e.ExecutionId);
            });

            // WorkflowExecutionTrigger entity
            modelBuilder.Entity<WorkflowExecutionTrigger>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TriggerId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<WorkflowExecution>()
                      .WithMany()
                      .HasForeignKey(e => e.ExecutionId);
            });

            // WorkflowExecutionTriggerLog entity
            modelBuilder.Entity<WorkflowExecutionTriggerLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Level).IsRequired().HasMaxLength(20);
                entity.HasOne<WorkflowExecutionTrigger>()
                      .WithMany()
                      .HasForeignKey(e => e.TriggerId);
            });

            // WorkflowExecutionTriggerLogEntry entity
            modelBuilder.Entity<WorkflowExecutionTriggerLogEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Level).IsRequired().HasMaxLength(20);
                entity.HasOne<WorkflowExecutionTriggerLog>()
                      .WithMany()
                      .HasForeignKey(e => e.LogId);
            });
        }

        private void ConfigureSystemEntities(ModelBuilder modelBuilder)
        {
            // Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId);
            });

            // Event entity
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // File entity
            modelBuilder.Entity<Core.Models.File>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Path).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Log entity
            modelBuilder.Entity<Log>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Level).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Audit entity
            modelBuilder.Entity<Audit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TenantId).HasMaxLength(50);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            });

            // Config entity
            modelBuilder.Entity<Config>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Value).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.TenantId).HasMaxLength(50);
                entity.HasIndex(e => new { e.Key, e.TenantId }).IsUnique();
            });

            // System entity
            modelBuilder.Entity<Core.Models.System>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });

            // Status entity
            modelBuilder.Entity<Status>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TenantId).HasMaxLength(50);
            });

            // Monitor entity
            modelBuilder.Entity<Core.Models.Monitor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Alert entity
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Report entity
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Dashboard entity
            modelBuilder.Entity<Dashboard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Layout).IsRequired();
                entity.HasOne<User>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId);
            });

            // Integration entity
            modelBuilder.Entity<Integration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Webhook entity
            modelBuilder.Entity<Webhook>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Events).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Queue entity
            modelBuilder.Entity<Queue>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.TenantId).HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });
        }

        private void ConfigureDevelopmentEntities(ModelBuilder modelBuilder)
        {
            // Repository entity
            modelBuilder.Entity<Repository>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Branch entity
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Repository>()
                      .WithMany()
                      .HasForeignKey(e => e.RepositoryId);
            });

            // Commit entity
            modelBuilder.Entity<Commit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Hash).IsRequired().HasMaxLength(100);
                entity.HasOne<Repository>()
                      .WithMany()
                      .HasForeignKey(e => e.RepositoryId);
            });

            // Build entity
            modelBuilder.Entity<Build>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Deploy entity
            modelBuilder.Entity<Deploy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Environment entity
            modelBuilder.Entity<Core.Models.Environment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Test entity
            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Version entity
            modelBuilder.Entity<Core.Models.Version>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.VersionNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Backup entity
            modelBuilder.Entity<Backup>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Compliance entity
            modelBuilder.Entity<Compliance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne<Project>()
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId);
            });

            // Credential entity
            modelBuilder.Entity<Credential>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TenantId).HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });
        }
    }
}
