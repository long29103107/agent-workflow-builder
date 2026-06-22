using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace AgentWorkflow.Core.Tests;

public sealed class PersistenceModelTests
{
    [Fact]
    public void PostgresModel_MapsAuthoritativeTablesAndRelationships()
    {
        var options = new DbContextOptionsBuilder<AgentWorkflowDbContext>()
            .UseNpgsql("Host=localhost;Database=model_test;Username=test;Password=test")
            .Options;
        using var context = new AgentWorkflowDbContext(options);

        var project = context.Model.FindEntityType(typeof(ProjectEntity))
            ?? throw new InvalidOperationException("Project persistence mapping is missing.");
        var engineeringTask = context.Model.FindEntityType(typeof(EngineeringTaskEntity))
            ?? throw new InvalidOperationException("EngineeringTask persistence mapping is missing.");
        var workItem = context.Model.FindEntityType(typeof(WorkItemEntity))
            ?? throw new InvalidOperationException("WorkItem persistence mapping is missing.");
        var workflowRun = context.Model.FindEntityType(typeof(WorkflowRunEntity))
            ?? throw new InvalidOperationException("WorkflowRun persistence mapping is missing.");
        var workflowCommand = context.Model.FindEntityType(typeof(WorkflowCommandEntity))
            ?? throw new InvalidOperationException("WorkflowCommand persistence mapping is missing.");
        var workflowEvent = context.Model.FindEntityType(typeof(WorkflowEventEntity))
            ?? throw new InvalidOperationException("WorkflowEvent persistence mapping is missing.");
        var agentExecution = context.Model.FindEntityType(typeof(AgentExecutionEntity))
            ?? throw new InvalidOperationException("AgentExecution persistence mapping is missing.");
        var evidenceItem = context.Model.FindEntityType(typeof(EvidenceItemEntity))
            ?? throw new InvalidOperationException("EvidenceItem persistence mapping is missing.");
        var artifact = context.Model.FindEntityType(typeof(ArtifactEntity))
            ?? throw new InvalidOperationException("Artifact persistence mapping is missing.");
        var approval = context.Model.FindEntityType(typeof(ApprovalEntity))
            ?? throw new InvalidOperationException("Approval persistence mapping is missing.");
        var activity = context.Model.FindEntityType(typeof(TaskActivityEntity))
            ?? throw new InvalidOperationException("TaskActivity persistence mapping is missing.");

        Assert.Equal("projects", project.GetTableName());
        Assert.Equal("jsonb", project.FindProperty(nameof(ProjectEntity.PayloadJson))!.GetColumnType());
        Assert.Equal("engineering_tasks", engineeringTask.GetTableName());
        Assert.Equal("work_items", workItem.GetTableName());
        Assert.Equal("workflow_runs", workflowRun.GetTableName());
        Assert.Equal("workflow_commands", workflowCommand.GetTableName());
        Assert.Contains(workflowCommand.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(
                [nameof(WorkflowCommandEntity.RunId), nameof(WorkflowCommandEntity.IdempotencyKey)]));
        Assert.NotNull(workflowRun.FindProperty(nameof(WorkflowRunEntity.Stage)));
        Assert.NotNull(workflowRun.FindProperty(nameof(WorkflowRunEntity.Attempt)));
        Assert.NotNull(workflowRun.FindProperty(nameof(WorkflowRunEntity.FailureDetails)));
        Assert.Equal("jsonb", workflowRun.FindProperty(nameof(WorkflowRunEntity.ResultJson))!.GetColumnType());
        Assert.Equal("workflow_events", workflowEvent.GetTableName());
        Assert.Equal("agent_executions", agentExecution.GetTableName());
        Assert.Equal("evidence_items", evidenceItem.GetTableName());
        Assert.Equal("artifacts", artifact.GetTableName());
        Assert.Equal("approvals", approval.GetTableName());
        Assert.Equal("task_activities", activity.GetTableName());
        Assert.Contains(engineeringTask.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(ProjectEntity) &&
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(workItem.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(EngineeringTaskEntity));
        Assert.Contains(workflowEvent.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(WorkflowRunEntity));
        Assert.Contains(workflowCommand.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(WorkflowRunEntity));
        Assert.Contains(evidenceItem.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(AgentExecutionEntity));
        Assert.Contains(artifact.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(WorkflowRunEntity));
        Assert.Contains(approval.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(ProjectEntity));
        Assert.Contains(activity.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(WorkflowRunEntity));
    }

    [Fact]
    public void CoreRegistration_UsesInMemoryStoresWithoutConnectionString()
    {
        using var provider = new ServiceCollection()
            .AddAgentWorkflowCore()
            .BuildServiceProvider();

        Assert.IsType<InMemoryProjectStore>(provider.GetRequiredService<IProjectStore>());
        Assert.IsType<InMemoryEngineeringTaskStore>(provider.GetRequiredService<IEngineeringTaskStore>());
        Assert.IsType<InMemoryWorkflowRunStore>(provider.GetRequiredService<IWorkflowRunStore>());
        Assert.IsType<InMemoryWorkflowEvidenceStore>(provider.GetRequiredService<IWorkflowEvidenceStore>());
        Assert.IsType<InMemoryApprovalStore>(provider.GetRequiredService<IApprovalStore>());
        Assert.IsType<InMemoryTaskActivityStore>(provider.GetRequiredService<ITaskActivityStore>());
    }

    [Fact]
    public void CoreRegistration_UsesPostgresStoresWithConnectionString()
    {
        using var provider = new ServiceCollection()
            .AddAgentWorkflowCore("Host=localhost;Database=registration_test;Username=test;Password=test")
            .BuildServiceProvider();

        Assert.IsType<PostgresProjectStore>(provider.GetRequiredService<IProjectStore>());
        Assert.IsType<PostgresEngineeringTaskStore>(provider.GetRequiredService<IEngineeringTaskStore>());
        Assert.IsType<PostgresWorkflowRunStore>(provider.GetRequiredService<IWorkflowRunStore>());
        Assert.IsType<PostgresWorkflowEvidenceStore>(provider.GetRequiredService<IWorkflowEvidenceStore>());
        Assert.IsType<PostgresApprovalStore>(provider.GetRequiredService<IApprovalStore>());
        Assert.IsType<PostgresTaskActivityStore>(provider.GetRequiredService<ITaskActivityStore>());
    }
}
