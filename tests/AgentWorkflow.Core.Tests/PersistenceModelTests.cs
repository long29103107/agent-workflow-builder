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
        var workflowEvent = context.Model.FindEntityType(typeof(WorkflowEventEntity))
            ?? throw new InvalidOperationException("WorkflowEvent persistence mapping is missing.");
        var agentExecution = context.Model.FindEntityType(typeof(AgentExecutionEntity))
            ?? throw new InvalidOperationException("AgentExecution persistence mapping is missing.");
        var evidenceItem = context.Model.FindEntityType(typeof(EvidenceItemEntity))
            ?? throw new InvalidOperationException("EvidenceItem persistence mapping is missing.");
        var artifact = context.Model.FindEntityType(typeof(ArtifactEntity))
            ?? throw new InvalidOperationException("Artifact persistence mapping is missing.");

        Assert.Equal("projects", project.GetTableName());
        Assert.Equal("jsonb", project.FindProperty(nameof(ProjectEntity.PayloadJson))!.GetColumnType());
        Assert.Equal("engineering_tasks", engineeringTask.GetTableName());
        Assert.Equal("work_items", workItem.GetTableName());
        Assert.Equal("workflow_runs", workflowRun.GetTableName());
        Assert.NotNull(workflowRun.FindProperty(nameof(WorkflowRunEntity.Stage)));
        Assert.NotNull(workflowRun.FindProperty(nameof(WorkflowRunEntity.Attempt)));
        Assert.NotNull(workflowRun.FindProperty(nameof(WorkflowRunEntity.FailureDetails)));
        Assert.Equal("jsonb", workflowRun.FindProperty(nameof(WorkflowRunEntity.ResultJson))!.GetColumnType());
        Assert.Equal("workflow_events", workflowEvent.GetTableName());
        Assert.Equal("agent_executions", agentExecution.GetTableName());
        Assert.Equal("evidence_items", evidenceItem.GetTableName());
        Assert.Equal("artifacts", artifact.GetTableName());
        Assert.Contains(engineeringTask.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(ProjectEntity) &&
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(workItem.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(EngineeringTaskEntity));
        Assert.Contains(workflowEvent.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(WorkflowRunEntity));
        Assert.Contains(evidenceItem.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(AgentExecutionEntity));
        Assert.Contains(artifact.GetForeignKeys(), foreignKey =>
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
    }
}
