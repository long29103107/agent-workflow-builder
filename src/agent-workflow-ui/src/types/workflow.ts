export type TaskItem = {
  id: string;
  source: string;
  key: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  tags: string[];
};

export type WorkflowEvent = {
  id: string;
  timestamp: string;
  agent: string;
  type: string;
  message: string;
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
    importantFiles: string[];
    technologies: string[];
    summary: string;
  };
  memoryItems: Array<{ id: string; title: string; content: string; tags: string[] }>;
  graphEntities: Array<{ id: string; type: string; name: string; relatedEntityIds: string[] }>;
};

export type WorkflowRun = {
  id: string;
  taskId: string;
  status: string;
  startedAt: string;
  completedAt?: string;
  result?: InvestigationResult;
};

export type ToolEndpointSettings = {
  jiraMcpEndpoint: string;
  notionMcpEndpoint: string;
  repositoryPath: string;
};
