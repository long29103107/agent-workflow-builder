using System.Collections.Concurrent;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryWorkflowEvidenceStore(
    ISecretRedactor redactor,
    TimeProvider timeProvider) : IWorkflowEvidenceStore
{
    private readonly ConcurrentDictionary<Guid, AgentExecution> _executions = [];
    private readonly ConcurrentDictionary<Guid, EvidenceItem> _evidence = [];
    private readonly ConcurrentDictionary<Guid, Artifact> _artifacts = [];

    public AgentExecution StartExecution(Guid runId, string agentName)
    {
        var execution = new AgentExecution(
            Guid.NewGuid(),
            runId,
            redactor.Redact(agentName),
            AgentExecutionStatus.Running,
            timeProvider.GetUtcNow(),
            null);
        if (!_executions.TryAdd(execution.Id, execution))
        {
            throw new InvalidOperationException("Could not append agent execution.");
        }

        return execution;
    }

    public AgentExecution CompleteExecution(Guid executionId, AgentExecutionStatus status)
    {
        if (status == AgentExecutionStatus.Running)
        {
            throw new ArgumentException("A completed execution requires a terminal status.", nameof(status));
        }

        while (true)
        {
            if (!_executions.TryGetValue(executionId, out var current))
            {
                throw new KeyNotFoundException($"Agent execution '{executionId}' was not found.");
            }

            if (current.Status != AgentExecutionStatus.Running)
            {
                throw new InvalidOperationException($"Agent execution '{executionId}' is already terminal.");
            }

            var completed = current with { Status = status, CompletedAt = timeProvider.GetUtcNow() };
            if (_executions.TryUpdate(executionId, completed, current))
            {
                return completed;
            }
        }
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
        if (!_evidence.TryAdd(item.Id, item))
        {
            throw new InvalidOperationException("Could not append evidence item.");
        }

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

        var artifact = new Artifact(
            Guid.NewGuid(),
            runId,
            agentExecutionId,
            redactor.Redact(name),
            redactor.Redact(type),
            redactor.Redact(content),
            redactor.Redact(contentType),
            timeProvider.GetUtcNow());
        if (!_artifacts.TryAdd(artifact.Id, artifact))
        {
            throw new InvalidOperationException("Could not append artifact.");
        }

        return artifact;
    }

    public WorkflowEvidenceBundle GetEvidence(Guid runId) =>
        new(
            _executions.Values.Where(item => item.RunId == runId).OrderBy(item => item.StartedAt).ToList(),
            _evidence.Values.Where(item => item.RunId == runId).OrderBy(item => item.CreatedAt).ToList(),
            _artifacts.Values.Where(item => item.RunId == runId).OrderBy(item => item.CreatedAt).ToList());

    private void EnsureExecution(Guid runId, Guid executionId)
    {
        if (!_executions.TryGetValue(executionId, out var execution) || execution.RunId != runId)
        {
            throw new KeyNotFoundException(
                $"Agent execution '{executionId}' was not found for workflow run '{runId}'.");
        }
    }

    private string? Redact(string? value) => value is null ? null : redactor.Redact(value);
}
