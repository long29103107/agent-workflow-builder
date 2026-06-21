namespace AgentWorkflow.Core.Domain;

public static class WorkflowStateMachine
{
    private static readonly IReadOnlyDictionary<WorkflowStage, IReadOnlySet<WorkflowStage>> LegalTransitions =
        new Dictionary<WorkflowStage, IReadOnlySet<WorkflowStage>>
        {
            [WorkflowStage.Created] = Set(WorkflowStage.LoadingTaskContext, WorkflowStage.Failed),
            [WorkflowStage.LoadingTaskContext] = Set(WorkflowStage.ResolvingRepository, WorkflowStage.Failed),
            [WorkflowStage.ResolvingRepository] = Set(WorkflowStage.LoadingMemory, WorkflowStage.Failed),
            [WorkflowStage.LoadingMemory] = Set(WorkflowStage.Investigating, WorkflowStage.Failed),
            [WorkflowStage.Investigating] = Set(WorkflowStage.Aggregating, WorkflowStage.Failed),
            [WorkflowStage.Aggregating] = Set(WorkflowStage.Completed, WorkflowStage.Failed),
            [WorkflowStage.Completed] = Set(),
            [WorkflowStage.Failed] = Set()
        };

    public static bool CanTransition(WorkflowStage current, WorkflowStage next) =>
        LegalTransitions[current].Contains(next);

    public static void EnsureTransition(WorkflowStage current, WorkflowStage next)
    {
        if (!CanTransition(current, next))
        {
            throw new InvalidOperationException(
                $"Workflow cannot transition from '{current}' to '{next}'.");
        }
    }

    private static IReadOnlySet<WorkflowStage> Set(params WorkflowStage[] stages) =>
        stages.ToHashSet();
}
