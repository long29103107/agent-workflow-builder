namespace AgentWorkflow.Core.Domain;

public sealed class ApprovalPolicyException(string message) : InvalidOperationException(message);
