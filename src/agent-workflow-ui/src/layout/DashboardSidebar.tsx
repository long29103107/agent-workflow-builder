import { Bot, GitBranch, Inbox, KanbanSquare, KeyRound, Plus, Settings } from "lucide-react";
import type { WorkspaceProject } from "../hooks/useInvestigationConsole";
import type { WorkspaceRoute } from "../routes/workspaceRoutes";
import { workspaceRoutes } from "../routes/workspaceRoutes";

type DashboardSidebarProps = {
  activeWorkspaceId: string;
  apiKey: string;
  completedCount: number;
  currentRoute: WorkspaceRoute;
  onCreateWorkspace: () => void;
  onNavigate: (route: WorkspaceRoute) => void;
  onWorkspaceChange: (workspaceId: string) => void;
  queuedCount: number;
  repoPath: string;
  repoUrl: string;
  workspaces: WorkspaceProject[];
};

export function DashboardSidebar({
  activeWorkspaceId,
  apiKey,
  completedCount,
  currentRoute,
  onCreateWorkspace,
  onNavigate,
  onWorkspaceChange,
  queuedCount,
  repoPath,
  repoUrl,
  workspaces
}: DashboardSidebarProps) {
  const activeWorkspace = workspaces.find((workspace) => workspace.id === activeWorkspaceId);

  return (
    <aside className="dashboard-sidebar">
      <div className="brand-lockup">
        <span className="brand-mark">AW</span>
        <div>
          <span className="eyebrow">Agent Workspace</span>
          <h1>Workflow Builder</h1>
        </div>
      </div>

      <section className="sidebar-section workspace-switcher">
        <div className="sidebar-section-header">
          <span className="eyebrow">Workspaces</span>
          <button className="sidebar-icon-button" aria-label="Create workspace" onClick={onCreateWorkspace}>
            <Plus size={15} />
          </button>
        </div>
        <label className="workspace-select-label">
          <span>Project</span>
          <select
            className="workspace-select"
            value={activeWorkspaceId}
            onChange={(event) => onWorkspaceChange(event.target.value)}
          >
            {workspaces.map((workspace) => (
              <option key={workspace.id} value={workspace.id}>
                {workspace.name}
              </option>
            ))}
          </select>
        </label>
        <p className="workspace-target">{activeWorkspace?.repoUrl || activeWorkspace?.repoPath || "No repo target"}</p>
      </section>

      <nav className="sidebar-nav" aria-label="Workspace sections">
        {workspaceRoutes.map((route) => (
          <a
            className={route.id === currentRoute ? "is-active" : ""}
            href={route.path}
            key={route.id}
            onClick={(event) => {
              event.preventDefault();
              onNavigate(route.id);
            }}
          >
            {route.id === "request" && <Inbox size={17} />}
            {route.id === "planner" && <Bot size={17} />}
            {route.id === "kanban" && <KanbanSquare size={17} />}
            {route.id === "configuration" && <Settings size={17} />}
            {route.label}
          </a>
        ))}
      </nav>

      <section className="sidebar-section">
        <span className="eyebrow">Processing</span>
        <div className="metric-row">
          <strong>{queuedCount}</strong>
          <span>Queued</span>
        </div>
        <div className="metric-row">
          <strong>{completedCount}</strong>
          <span>Completed</span>
        </div>
      </section>

      <section className="sidebar-section">
        <span className="eyebrow">Config</span>
        <div className="repo-target">
          <GitBranch size={16} />
          <span>{repoUrl || repoPath || "No repository target"}</span>
        </div>
        <div className="repo-target">
          <KeyRound size={16} />
          <span>{apiKey ? "API key provided" : "API key not set"}</span>
        </div>
      </section>
    </aside>
  );
}
