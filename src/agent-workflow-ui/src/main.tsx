import React, { useEffect, useMemo, useState } from "react";
import { createRoot } from "react-dom/client";
import {
  Activity,
  Brain,
  CheckCircle2,
  Database,
  GitBranch,
  Loader2,
  Play,
  RefreshCcw,
  Save,
  Settings,
  Sparkles
} from "lucide-react";
import "./styles.css";

type TaskItem = {
  id: string;
  source: string;
  key: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  tags: string[];
};

type WorkflowEvent = {
  id: string;
  timestamp: string;
  agent: string;
  type: string;
  message: string;
};

type AgentMessage = {
  agentName: string;
  role: string;
  content: string;
  createdAt: string;
};

type ExecutionStep = {
  order: number;
  title: string;
  description: string;
  ownerAgent: string;
  status: string;
};

type InvestigationResult = {
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

type WorkflowRun = {
  id: string;
  taskId: string;
  status: string;
  startedAt: string;
  completedAt?: string;
  result?: InvestigationResult;
};

type ToolEndpointSettings = {
  jiraMcpEndpoint: string;
  notionMcpEndpoint: string;
  repositoryPath: string;
};

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5275/api";

function App() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);
  const [run, setRun] = useState<WorkflowRun | null>(null);
  const [events, setEvents] = useState<WorkflowEvent[]>([]);
  const [isLoadingTasks, setIsLoadingTasks] = useState(true);
  const [isInvestigating, setIsInvestigating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [repoPath, setRepoPath] = useState("");
  const [jiraEndpoint, setJiraEndpoint] = useState("mock://jira");
  const [notionEndpoint, setNotionEndpoint] = useState("mock://notion");
  const [isSavingSettings, setIsSavingSettings] = useState(false);
  const [settingsMessage, setSettingsMessage] = useState<string | null>(null);

  const selectedTask = useMemo(
    () => tasks.find((task) => task.id === selectedTaskId) ?? null,
    [selectedTaskId, tasks]
  );

  useEffect(() => {
    void loadTasks();
    void loadSettings();
  }, []);

  async function loadTasks() {
    setIsLoadingTasks(true);
    setError(null);

    try {
      const response = await fetch(`${apiBaseUrl}/tasks`);
      if (!response.ok) throw new Error(`Task API returned ${response.status}`);
      setTasks(await response.json());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not load tasks.");
    } finally {
      setIsLoadingTasks(false);
    }
  }

  async function loadSettings() {
    try {
      const response = await fetch(`${apiBaseUrl}/settings`);
      if (!response.ok) return;
      const settings: ToolEndpointSettings = await response.json();
      setRepoPath(settings.repositoryPath);
      setJiraEndpoint(settings.jiraMcpEndpoint);
      setNotionEndpoint(settings.notionMcpEndpoint);
    } catch {
      setSettingsMessage("Using local mock settings.");
    }
  }

  async function saveSettings() {
    setIsSavingSettings(true);
    setSettingsMessage(null);

    try {
      const response = await fetch(`${apiBaseUrl}/settings`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          jiraMcpEndpoint: jiraEndpoint,
          notionMcpEndpoint: notionEndpoint,
          repositoryPath: repoPath
        })
      });
      if (!response.ok) throw new Error(`Settings API returned ${response.status}`);
      const settings: ToolEndpointSettings = await response.json();
      setRepoPath(settings.repositoryPath);
      setJiraEndpoint(settings.jiraMcpEndpoint);
      setNotionEndpoint(settings.notionMcpEndpoint);
      setSettingsMessage("Settings saved for this API session.");
    } catch (err) {
      setSettingsMessage(err instanceof Error ? err.message : "Could not save settings.");
    } finally {
      setIsSavingSettings(false);
    }
  }

  async function startInvestigation() {
    if (!selectedTask) return;
    setIsInvestigating(true);
    setError(null);
    setRun(null);
    setEvents([]);

    try {
      const response = await fetch(`${apiBaseUrl}/workflows/investigate`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          taskId: selectedTask.id,
          repositoryPath: repoPath,
          requestedAgents: []
        })
      });
      if (!response.ok) throw new Error(`Investigation API returned ${response.status}`);

      const nextRun: WorkflowRun = await response.json();
      setRun(nextRun);
      const eventResponse = await fetch(`${apiBaseUrl}/workflows/${nextRun.id}/events`);
      if (eventResponse.ok) {
        setEvents(await eventResponse.json());
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Investigation failed.");
    } finally {
      setIsInvestigating(false);
    }
  }

  function handleDragStart(event: React.DragEvent<HTMLDivElement>, taskId: string) {
    event.dataTransfer.setData("text/plain", taskId);
  }

  function handleDrop(event: React.DragEvent<HTMLDivElement>) {
    event.preventDefault();
    const taskId = event.dataTransfer.getData("text/plain");
    if (taskId) setSelectedTaskId(taskId);
  }

  return (
    <main className="shell">
      <header className="topbar">
        <div>
          <span className="eyebrow">Agent Workflow Orchestration</span>
          <h1>Investigation Console</h1>
        </div>
        <div className="status-pill">
          <Activity size={16} />
          {run?.status ?? "Ready"}
        </div>
      </header>

      {error && <div className="alert">{error}</div>}

      <section className="workspace">
        <div className="task-panel">
          <div className="panel-header">
            <h2>Jira Tasks</h2>
            <button className="icon-button" onClick={loadTasks} title="Refresh tasks">
              {isLoadingTasks ? <Loader2 className="spin" size={18} /> : <RefreshCcw size={18} />}
            </button>
          </div>

          <div className="task-list">
            {tasks.map((task) => (
              <div
                className="task-card"
                draggable
                key={task.id}
                onClick={() => setSelectedTaskId(task.id)}
                onDragStart={(event) => handleDragStart(event, task.id)}
              >
                <div className="task-row">
                  <strong>{task.key}</strong>
                  <span>{task.priority}</span>
                </div>
                <h3>{task.title}</h3>
                <p>{task.description}</p>
                <div className="tags">
                  {task.tags.map((tag) => (
                    <span key={tag}>{tag}</span>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="investigate-lane" onDragOver={(event) => event.preventDefault()} onDrop={handleDrop}>
          <div className="lane-title">
            <Sparkles size={18} />
            <h2>Investigate</h2>
          </div>

          {selectedTask ? (
            <div className="selected-task">
              <span>{selectedTask.source}</span>
              <h3>{selectedTask.key}: {selectedTask.title}</h3>
              <p>{selectedTask.description}</p>
              <div className="selected-meta">
                <span>{selectedTask.priority}</span>
                <span>{selectedTask.status}</span>
              </div>
              <button className="primary" onClick={startInvestigation} disabled={isInvestigating}>
                {isInvestigating ? <Loader2 className="spin" size={18} /> : <Play size={18} />}
                Start Investigation
              </button>
            </div>
          ) : (
            <div className="drop-empty">Drop a task here to prepare a lead-agent investigation.</div>
          )}
        </div>

        <aside className="settings-panel">
          <div className="panel-header">
            <h2>Settings</h2>
            <Settings size={18} />
          </div>
          <label>
            Repository path
            <input value={repoPath} onChange={(event) => setRepoPath(event.target.value)} />
          </label>
          <label>
            Jira MCP endpoint
            <input value={jiraEndpoint} onChange={(event) => setJiraEndpoint(event.target.value)} />
          </label>
          <label>
            Notion MCP endpoint
            <input value={notionEndpoint} onChange={(event) => setNotionEndpoint(event.target.value)} />
          </label>
          <button className="secondary" onClick={saveSettings} disabled={isSavingSettings}>
            {isSavingSettings ? <Loader2 className="spin" size={18} /> : <Save size={18} />}
            Save Settings
          </button>
          <p className="settings-note">{settingsMessage ?? "Endpoint fields are persisted in memory while the backend uses mock MCP tools."}</p>
        </aside>
      </section>

      <section className="run-grid">
        <RunStatus run={run} />
        <Timeline events={events} />
      </section>

      {run?.result && <Results result={run.result} />}
    </main>
  );
}

function RunStatus({ run }: { run: WorkflowRun | null }) {
  return (
    <section className="surface">
      <div className="section-title">
        <CheckCircle2 size={18} />
        <h2>Workflow Run</h2>
      </div>
      {run ? (
        <dl className="run-details">
          <div><dt>Run ID</dt><dd>{run.id}</dd></div>
          <div><dt>Task</dt><dd>{run.taskId}</dd></div>
          <div><dt>Status</dt><dd>{run.status}</dd></div>
          <div><dt>Started</dt><dd>{new Date(run.startedAt).toLocaleString()}</dd></div>
        </dl>
      ) : (
        <p className="muted">No investigation has run yet.</p>
      )}
    </section>
  );
}

function Timeline({ events }: { events: WorkflowEvent[] }) {
  return (
    <section className="surface">
      <div className="section-title">
        <Activity size={18} />
        <h2>Agent Activity</h2>
      </div>
      <div className="timeline">
        {events.length === 0 && <p className="muted">Activity appears after the lead agent starts.</p>}
        {events.map((event) => (
          <div className="timeline-item" key={event.id}>
            <span>{new Date(event.timestamp).toLocaleTimeString()}</span>
            <strong>{event.agent}</strong>
            <p>{event.message}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

function Results({ result }: { result: InvestigationResult }) {
  return (
    <section className="results">
      <div className="summary-band">
        <Brain size={22} />
        <div>
          <h2>Investigation Summary</h2>
          <p>{result.summary}</p>
        </div>
      </div>

      <div className="result-grid">
        <section className="surface">
          <div className="section-title">
            <GitBranch size={18} />
            <h2>{result.plan.title}</h2>
          </div>
          <div className="steps">
            {result.plan.steps.map((step) => (
              <article className="step" key={`${step.order}-${step.title}`}>
                <span>{step.order}</span>
                <div>
                  <h3>{step.title}</h3>
                  <p>{step.description}</p>
                  <small>{step.ownerAgent} - {step.status}</small>
                </div>
              </article>
            ))}
          </div>
        </section>

        <section className="surface">
          <div className="section-title">
            <Database size={18} />
            <h2>Context</h2>
          </div>
          <h3>{result.repositoryContext.name}</h3>
          <p>{result.repositoryContext.summary}</p>
          <div className="tags">
            {result.repositoryContext.technologies.map((tech) => (
              <span key={tech}>{tech}</span>
            ))}
          </div>
          <h3>Agent Notes</h3>
          {result.agentMessages.map((message) => (
            <p className="agent-note" key={message.agentName}><strong>{message.agentName}</strong>: {message.content}</p>
          ))}
          <h3>Risks</h3>
          <ul className="compact-list">
            {result.plan.risks.map((risk) => (
              <li key={risk}>{risk}</li>
            ))}
          </ul>
          <h3>Open Questions</h3>
          <ul className="compact-list">
            {result.plan.openQuestions.map((question) => (
              <li key={question}>{question}</li>
            ))}
          </ul>
          <h3>Memory Matches</h3>
          {result.memoryItems.map((memory) => (
            <p className="agent-note" key={memory.id}><strong>{memory.title}</strong>: {memory.content}</p>
          ))}
          <h3>Graph Links</h3>
          <div className="graph-list">
            {result.graphEntities.map((entity) => (
              <span key={entity.id}>{entity.type}: {entity.name}</span>
            ))}
          </div>
        </section>
      </div>
    </section>
  );
}

createRoot(document.getElementById("root")!).render(<App />);
