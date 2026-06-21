using Microsoft.EntityFrameworkCore;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class AgentWorkflowDbContext(DbContextOptions<AgentWorkflowDbContext> options)
    : DbContext(options)
{
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<EngineeringTaskEntity> EngineeringTasks => Set<EngineeringTaskEntity>();
    public DbSet<WorkItemEntity> WorkItems => Set<WorkItemEntity>();
    public DbSet<WorkflowRunEntity> WorkflowRuns => Set<WorkflowRunEntity>();
    public DbSet<WorkflowEventEntity> WorkflowEvents => Set<WorkflowEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectEntity>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasMaxLength(128);
            entity.Property(item => item.PayloadJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<EngineeringTaskEntity>(entity =>
        {
            entity.ToTable("engineering_tasks");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasMaxLength(128);
            entity.Property(item => item.ProjectId).HasMaxLength(128);
            entity.Property(item => item.Title).HasMaxLength(500);
            entity.Property(item => item.Status).HasMaxLength(64);
            entity.Property(item => item.Priority).HasMaxLength(32);
            entity.HasIndex(item => new { item.ProjectId, item.CreatedAt });
            entity.HasOne<ProjectEntity>()
                .WithMany()
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkItemEntity>(entity =>
        {
            entity.ToTable("work_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).HasMaxLength(128);
            entity.Property(item => item.EngineeringTaskId).HasMaxLength(128);
            entity.Property(item => item.Source).HasMaxLength(32);
            entity.Property(item => item.SourceKey).HasMaxLength(256);
            entity.Property(item => item.Title).HasMaxLength(500);
            entity.Property(item => item.Status).HasMaxLength(128);
            entity.Property(item => item.Priority).HasMaxLength(64);
            entity.HasIndex(item => new { item.Source, item.SourceKey });
            entity.HasOne<EngineeringTaskEntity>()
                .WithMany()
                .HasForeignKey(item => item.EngineeringTaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowRunEntity>(entity =>
        {
            entity.ToTable("workflow_runs");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.TaskId).HasMaxLength(128);
            entity.Property(item => item.Status).HasMaxLength(64);
            entity.Property(item => item.Stage).HasMaxLength(64);
            entity.Property(item => item.ResultJson).HasColumnType("jsonb");
            entity.HasIndex(item => item.TaskId);
        });

        modelBuilder.Entity<WorkflowEventEntity>(entity =>
        {
            entity.ToTable("workflow_events");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Agent).HasMaxLength(256);
            entity.Property(item => item.Type).HasMaxLength(128);
            entity.HasIndex(item => new { item.RunId, item.Timestamp });
            entity.HasOne<WorkflowRunEntity>()
                .WithMany()
                .HasForeignKey(item => item.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public sealed class ProjectEntity
{
    public string Id { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class EngineeringTaskEntity
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class WorkItemEntity
{
    public string Id { get; set; } = string.Empty;
    public string EngineeringTaskId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string SourceKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
}

public sealed class WorkflowRunEntity
{
    public Guid Id { get; set; }
    public string TaskId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public int Attempt { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? ResultJson { get; set; }
    public string? FailureDetails { get; set; }
}

public sealed class WorkflowEventEntity
{
    public Guid Id { get; set; }
    public Guid RunId { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Agent { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
