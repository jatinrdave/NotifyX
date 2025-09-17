using Microsoft.EntityFrameworkCore;
using NotifyXStudio.Core.Models;

namespace NotifyXStudio.Persistence
{
    public class NotifyXStudioDbContext : DbContext
    {
        public NotifyXStudioDbContext(DbContextOptions<NotifyXStudioDbContext> options) : base(options)
        {
        }

        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowRun> WorkflowRuns { get; set; }
        public DbSet<ConnectorRegistryEntry> Connectors { get; set; }
        public DbSet<NodeExecutionResult> NodeResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Workflow entity
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            });

            // Configure WorkflowRun entity
            modelBuilder.Entity<WorkflowRun>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WorkflowId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.HasOne<Workflow>()
                      .WithMany()
                      .HasForeignKey(e => e.WorkflowId);
            });

            // Configure ConnectorRegistryEntry entity
            modelBuilder.Entity<ConnectorRegistryEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            });

            // Configure NodeExecutionResult entity
            modelBuilder.Entity<NodeExecutionResult>(entity =>
            {
                entity.HasKey(e => new { e.RunId, e.NodeId });
                entity.Property(e => e.NodeId).IsRequired();
                entity.Property(e => e.RunId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });
        }
    }
}
