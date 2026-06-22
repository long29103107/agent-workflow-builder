export type TaskItem = {
  id: string;
  source: string;
  key: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  tags: string[];
  assignedAgent: string | null;
};

export type ScheduledTaskPriority = "Low" | "Medium" | "High" | "Critical";
export type ScheduledTaskStatus = "Queued" | "Processing" | "Completed" | "Failed";

export type ScheduledTask = {
  id: string;
  workspaceId: string | null;
  taskId: string;
  taskTitle: string;
  priority: ScheduledTaskPriority;
  status: ScheduledTaskStatus;
  queuedAt: string;
  startedAt: string | null;
  completedAt: string | null;
  workflowRunId: string | null;
  error: string | null;
  assignedAgent: string | null;
  lastHeartbeatAt: string | null;
  leaseExpiresAt: string | null;
  requestedAgents: string[] | null;
};

export type WorkspaceProject = {
  id: string;
  name: string;
  code: string;
  repositoryPath: string;
  repositoryUrl: string;
  repositoryProvider: string;
  createdAt: string;
  updatedAt: string;
};

export type WorkspaceUserRequest = {
  id: string;
  workspaceId: string;
  content: string;
  createdAt: string;
};

export type PlannerStep = {
  title: string;
  detail: string;
  owner: string;
};

export type PlannerLogStatus = "PendingApproval" | "Approved";

export type PlannerLog = {
  id: string;
  workspaceId: string;
  requestId: string;
  request: string;
  status: PlannerLogStatus;
  steps: PlannerStep[];
  createdAt: string;
  updatedAt: string;
};

export type RequestSubmissionResult = {
  request: WorkspaceUserRequest;
  plannerLog: PlannerLog;
};

export type PlannerApprovalResult = {
  plannerLog: PlannerLog;
  tasks: TaskItem[];
};

export type WorkflowEvent = {
  id: string;
  timestamp: string;
  agent: string;
  type: string;
  message: string;
};

export type AgentExecutionStatus = "Running" | "Completed" | "Failed" | "Cancelled";
export type EvidenceKind = "Rationale" | "SourceReference" | "Action" | "ToolResult";

export type AgentExecution = {
  id: string;
  runId: string;
  agentName: string;
  status: AgentExecutionStatus;
  startedAt: string;
  completedAt: string | null;
};

export type EvidenceItem = {
  id: string;
  runId: string;
  agentExecutionId: string;
  kind: EvidenceKind;
  summary: string;
  sourceReference: string | null;
  action: string | null;
  toolName: string | null;
  toolResult: string | null;
  createdAt: string;
};

export type Artifact = {
  id: string;
  runId: string;
  agentExecutionId: string | null;
  name: string;
  type: string;
  content: string;
  contentType: string;
  createdAt: string;
};

export type WorkflowEvidenceBundle = {
  agentExecutions: AgentExecution[];
  evidenceItems: EvidenceItem[];
  artifacts: Artifact[];
};

export type AgentMessage = {
  agentName: string;
  role: string;
  content: string;
  createdAt: string;
};

export type ExecutionStep = {
  order: number;
  title: string;
  description: string;
  ownerAgent: string;
  status: string;
};

export type InvestigationResult = {
  summary: string;
  plan: {
    title: string;
    steps: ExecutionStep[];
    risks: string[];
    openQuestions: string[];
  };
  agentMessages: AgentMessage[];
  repositoryContext: {
    path: string;
    name: string;
    connection: RepositoryConnection;
    importantFiles: string[];
    technologies: string[];
    summary: string;
  };
  memoryItems: Array<{ id: string; title: string; content: string; tags: string[] }>;
  graphEntities: Array<{ id: string; type: string; name: string; relatedEntityIds: string[] }>;
};

export type WorkflowStage =
  | "Created"
  | "LoadingTaskContext"
  | "ResolvingRepository"
  | "LoadingMemory"
  | "Investigating"
  | "Aggregating"
  | "Completed"
  | "Failed";

export type WorkflowRun = {
  id: string;
  taskId: string;
  status: string;
  stage: WorkflowStage;
  attempt: number;
  failureDetails?: string;
  startedAt: string;
  completedAt?: string;
  result?: InvestigationResult;
};

export type ToolEndpointSettings = {
  jiraMcpEndpoint: string;
  notionMcpEndpoint: string;
  repositoryPath: string;
  repositoryUrl: string;
  repositoryProvider: string;
};

export type RepositoryConnection = {
  provider: string;
  url: string | null;
  localPath: string | null;
  owner: string;
  name: string;
  defaultBranch: string;
  status: string;
  summary: string;
};
