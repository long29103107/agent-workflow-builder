using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class PostgresWorkflowEvidenceStore(
    IDbContextFactory<AgentWorkflowDbContext> contextFactory,
    ISecretRedactor redactor,
    TimeProvider timeProvider) : IWorkflowEvidenceStore
{
    public AgentExecution StartExecution(Guid runId, string agentName)
    {
        var item = new AgentExecution(
            Guid.NewGuid(),
            runId,
            redactor.Redact(agentName),
            AgentExecutionStatus.Running,
            timeProvider.GetUtcNow(),
            null);
        using var context = contextFactory.CreateDbContext();
        context.AgentExecutions.Add(ToEntity(item));
        context.SaveChanges();
        return item;
    }

    public AgentExecution CompleteExecution(Guid executionId, AgentExecutionStatus status)
    {
        if (status == AgentExecutionStatus.Running)
        {
            throw new ArgumentException("A completed execution requires a terminal status.", nameof(status));
        }

        using var context = contextFactory.CreateDbContext();
        var entity = context.AgentExecutions.Single(item => item.Id == executionId);
        if (ParseExecutionStatus(entity.Status) != AgentExecutionStatus.Running)
        {
            throw new InvalidOperationException($"Agent execution '{executionId}' is already terminal.");
        }

        entity.Status = status.ToString();
        entity.CompletedAt = timeProvider.GetUtcNow();
        context.SaveChanges();
        return ToDomain(entity);
    }

    public EvidenceItem AppendEvidence(
        Guid runId,
        Guid agentExecutionId,
        EvidenceKind kind,
        string summary,
        string? sourceReference = null,
        string? action = null,
        string? toolName = null,
        string? toolResult = null)
    {
        EnsureExecution(runId, agentExecutionId);
        var item = new EvidenceItem(
            Guid.NewGuid(),
            runId,
            agentExecutionId,
            kind,
            redactor.Redact(summary),
            Redact(sourceReference),
            Redact(action),
            Redact(toolName),
            Redact(toolResult),
            timeProvider.GetUtcNow());
        using var context = contextFactory.CreateDbContext();
        context.EvidenceItems.Add(ToEntity(item));
        context.SaveChanges();
        return item;
    }

    public Artifact AppendArtifact(
        Guid runId,
        Guid? agentExecutionId,
        string name,
        string type,
        string content,
        string contentType)
    {
        if (agentExecutionId is { } executionId)
        {
            EnsureExecution(runId, executionId);
        }

        var item = new Artifact(
            Guid.NewGuid(),
            runId,
            agentExecutionId,
            redactor.Redact(name),
            redactor.Redact(type),
            redactor.Redact(content),
            redactor.Redact(contentType),
            timeProvider.GetUtcNow());
        using var context = contextFactory.CreateDbContext();
        context.Artifacts.Add(ToEntity(item));
        context.SaveChanges();
        return item;
    }

    public WorkflowEvidenceBundle GetEvidence(Guid runId)
    {
        using var context = contextFactory.CreateDbContext();
        return new WorkflowEvidenceBundle(
            context.AgentExecutions.AsNoTracking()
                .Where(item => item.RunId == runId)
                .OrderBy(item => item.StartedAt)
                .ToList()
                .Select(ToDomain)
                .ToList(),
            context.EvidenceItems.AsNoTracking()
                .Where(item => item.RunId == runId)
                .OrderBy(item => item.CreatedAt)
                .ToList()
                .Select(ToDomain)
                .ToList(),
            context.Artifacts.AsNoTracking()
                .Where(item => item.RunId == runId)
                .OrderBy(item => item.CreatedAt)
                .ToList()
                .Select(ToDomain)
                .ToList());
    }

    private static AgentExecutionEntity ToEntity(AgentExecution item) => new()
    {
        Id = item.Id,
        RunId = item.RunId,
        AgentName = item.AgentName,
        Status = item.Status.ToString(),
        StartedAt = item.StartedAt,
        CompletedAt = item.CompletedAt
    };

    private static AgentExecution ToDomain(AgentExecutionEntity item) =>
        new(item.Id, item.RunId, item.AgentName, ParseExecutionStatus(item.Status), item.StartedAt, item.CompletedAt);

    private static EvidenceItemEntity ToEntity(EvidenceItem item) => new()
    {
        Id = item.Id,
        RunId = item.RunId,
        AgentExecutionId = item.AgentExecutionId,
        Kind = item.Kind.ToString(),
        Summary = item.Summary,
        SourceReference = item.SourceReference,
        Action = item.Action,
        ToolName = item.ToolName,
        ToolResult = item.ToolResult,
        CreatedAt = item.CreatedAt
    };

    private static EvidenceItem ToDomain(EvidenceItemEntity item) =>
        new(
            item.Id,
            item.RunId,
            item.AgentExecutionId,
            Enum.Parse<EvidenceKind>(item.Kind),
            item.Summary,
            item.SourceReference,
            item.Action,
            item.ToolName,
            item.ToolResult,
            item.CreatedAt);

    private static ArtifactEntity ToEntity(Artifact item) => new()
    {
        Id = item.Id,
        RunId = item.RunId,
        AgentExecutionId = item.AgentExecutionId,
        Name = item.Name,
        Type = item.Type,
        Content = item.Content,
        ContentType = item.ContentType,
        CreatedAt = item.CreatedAt
    };

    private static Artifact ToDomain(ArtifactEntity item) =>
        new(
            item.Id,
            item.RunId,
            item.AgentExecutionId,
            item.Name,
            item.Type,
            item.Content,
            item.ContentType,
            item.CreatedAt);

    private static AgentExecutionStatus ParseExecutionStatus(string status) =>
        Enum.TryParse<AgentExecutionStatus>(status, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Unknown agent execution status '{status}'.");

    private void EnsureExecution(Guid runId, Guid executionId)
    {
        using var context = contextFactory.CreateDbContext();
        if (!context.AgentExecutions.AsNoTracking().Any(item =>
                item.Id == executionId && item.RunId == runId))
        {
            throw new KeyNotFoundException(
                $"Agent execution '{executionId}' was not found for workflow run '{runId}'.");
        }
    }

    private string? Redact(string? value) => value is null ? null : redactor.Redact(value);
}
