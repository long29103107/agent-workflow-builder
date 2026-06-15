using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using OpenAI.Chat;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class OpenAiAgentReasoningService : IAgentReasoningService
{
    private readonly ChatClient? _client;

    public OpenAiAgentReasoningService()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var model = Environment.GetEnvironmentVariable("OPENAI_MODEL");

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _client = new ChatClient(
                model: string.IsNullOrWhiteSpace(model) ? "gpt-5.1" : model,
                apiKey: apiKey);
        }
    }

    public async Task<AgentReasoningResult> SummarizeInvestigationAsync(
        AgentReasoningRequest request,
        CancellationToken cancellationToken)
    {
        if (_client is null)
        {
            return CreateFallbackResult(request);
        }

        var prompt = $"""
            You are the Lead Agent for an agent workflow orchestration platform.
            Produce a concise investigation summary for task {request.TaskKey}: {request.TaskTitle}.
            Repository: {request.RepositoryName}
            Subagent summaries:
            {string.Join(Environment.NewLine, request.AgentSummaries.Select(summary => $"- {summary}"))}

            Return three short sections:
            Summary:
            Risks:
            Open questions:
            """;

        ChatCompletion completion = await _client.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            cancellationToken: cancellationToken);
        var text = completion.Content.Count == 0
            ? string.Empty
            : completion.Content[0].Text;

        return string.IsNullOrWhiteSpace(text)
            ? CreateFallbackResult(request)
            : new AgentReasoningResult(
                text.Trim(),
                ["Validate OpenAI-generated plan before implementation."],
                ["Should this run require human approval before execution?"]);
    }

    private static AgentReasoningResult CreateFallbackResult(AgentReasoningRequest request)
    {
        var summary = $"Lead Agent prepared a deterministic investigation for {request.TaskKey} in {request.RepositoryName}. Set OPENAI_API_KEY to enable OpenAI SDK reasoning.";
        return new AgentReasoningResult(
            summary,
            ["OpenAI SDK reasoning is disabled until OPENAI_API_KEY is configured."],
            ["Which model should be the default for production planning runs?"]);
    }
}
