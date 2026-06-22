using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class WorkflowEvidenceStoreTests
{
    [Fact]
    public void AppendEvidenceAndArtifact_RedactsSecretsAndReturnsAppendOnlyTimeline()
    {
        var store = new InMemoryWorkflowEvidenceStore(new SecretRedactor(), TimeProvider.System);
        var runId = Guid.NewGuid();
        var execution = store.StartExecution(runId, "LeadAgent");

        var first = store.AppendEvidence(
            runId,
            execution.Id,
            EvidenceKind.ToolResult,
            "Called tool with api_key=top-secret",
            toolName: "Bearer abc.def.ghi",
            toolResult: "password=hunter2");
        var second = store.AppendEvidence(
            runId,
            execution.Id,
            EvidenceKind.Rationale,
            "Selected the repository evidence because it directly supports the plan.");
        var artifact = store.AppendArtifact(
            runId,
            execution.Id,
            "result.json",
            "ToolOutput",
            "{\"token\":\"secret-value\"}",
            "application/json");
        store.CompleteExecution(execution.Id, AgentExecutionStatus.Completed);

        var bundle = store.GetEvidence(runId);
        Assert.Equal([first.Id, second.Id], bundle.EvidenceItems.Select(item => item.Id));
        Assert.DoesNotContain("top-secret", first.Summary);
        Assert.DoesNotContain("abc.def.ghi", first.ToolName);
        Assert.DoesNotContain("hunter2", first.ToolResult);
        Assert.DoesNotContain("secret-value", artifact.Content);
        Assert.All(
            bundle.EvidenceItems.Where(item => item.Kind == EvidenceKind.Rationale),
            item => Assert.DoesNotContain("chain-of-thought", item.Summary, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EvidenceItemsCannotBeReplacedAndExecutionsCompleteOnlyOnce()
    {
        var store = new InMemoryWorkflowEvidenceStore(new SecretRedactor(), TimeProvider.System);
        var execution = store.StartExecution(Guid.NewGuid(), "LeadAgent");
        store.CompleteExecution(execution.Id, AgentExecutionStatus.Completed);

        Assert.Throws<InvalidOperationException>(() =>
            store.CompleteExecution(execution.Id, AgentExecutionStatus.Failed));
    }
}
