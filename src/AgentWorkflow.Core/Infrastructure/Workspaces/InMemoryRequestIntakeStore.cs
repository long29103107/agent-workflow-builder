using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryRequestIntakeStore(
    IWorkspaceStore workspaceStore,
    IEngineeringTaskStore engineeringTaskStore) : IRequestIntakeStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, List<WorkspaceUserRequest>> _requests = new(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<WorkspaceUserRequest>> GetRequestsAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        await EnsureWorkspaceExistsAsync(workspaceId, cancellationToken);
        lock (_sync)
        {
            return _requests.TryGetValue(workspaceId, out var requests)
                ? requests.OrderByDescending(request => request.CreatedAt).ToList()
                : [];
        }
    }

    public async Task<WorkspaceUserRequest> CreateRequestAsync(
        string workspaceId,
        CreateWorkspaceUserRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureWorkspaceExistsAsync(workspaceId, cancellationToken);
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Request content is required.", nameof(request));
        }

        var content = request.Content.Trim();
        var engineeringTask = await engineeringTaskStore.CreateTaskAsync(
            new CreateEngineeringTaskRequest(
                workspaceId,
                CreateTitle(content),
                content,
                ScheduledTaskPriority.Medium,
                []),
            cancellationToken);
        var item = new WorkspaceUserRequest(
            engineeringTask.Id,
            workspaceId,
            content,
            DateTimeOffset.UtcNow);

        lock (_sync)
        {
            if (!_requests.TryGetValue(workspaceId, out var requests))
            {
                requests = [];
                _requests[workspaceId] = requests;
            }

            requests.Add(item);
        }

        return item;
    }

    private async Task EnsureWorkspaceExistsAsync(string workspaceId, CancellationToken cancellationToken)
    {
        if (await workspaceStore.GetWorkspaceAsync(workspaceId, cancellationToken) is null)
        {
            throw new KeyNotFoundException($"Workspace '{workspaceId}' was not found.");
        }
    }

    private static string CreateTitle(string content)
    {
        var firstLine = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?? content;
        return firstLine.Length <= 120 ? firstLine : $"{firstLine[..117]}...";
    }
}
