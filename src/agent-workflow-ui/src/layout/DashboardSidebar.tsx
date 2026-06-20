import { Bot, GitBranch, Inbox, KanbanSquare, KeyRound, Settings } from "lucide-react";
import type { WorkspaceRoute } from "../routes/workspaceRoutes";
import { workspaceRoutes } from "../routes/workspaceRoutes";

type DashboardSidebarProps = {
  apiKey: string;
  completedCount: number;
  currentRoute: WorkspaceRoute;
  onNavigate: (route: WorkspaceRoute) => void;
  queuedCount: number;
  repoPath: string;
  repoUrl: string;
};

export function DashboardSidebar({
  apiKey,
  completedCount,
  currentRoute,
  onNavigate,
  queuedCount,
  repoPath,
  repoUrl
}: DashboardSidebarProps) {
  return (
    <aside className="dashboard-sidebar">
      <div className="brand-lockup">
        <span className="brand-mark">AW</span>
        <div>
          <span className="eyebrow">Agent Workspace</span>
          <h1>Workflow Builder</h1>
        </div>
      </div>

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
