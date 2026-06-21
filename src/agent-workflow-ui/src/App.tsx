import { useEffect, useMemo, useState } from "react";
import { CircleDot } from "lucide-react";
import { Results } from "./features/results/Results";
import { useInvestigationConsole } from "./hooks/useInvestigationConsole";
import { DashboardSidebar } from "./layout/DashboardSidebar";
import { RequestPage } from "./pages/RequestPage";
import { getWorkspaceRoutePath, resolveWorkspaceRoute } from "./routes/workspaceRoutes";
import type { WorkspaceRoute } from "./routes/workspaceRoutes";
import { ConfigurationSection } from "./sections/ConfigurationSection";
import { KanbanSection } from "./sections/KanbanSection";
import { PipelineStatusSection } from "./sections/PipelineStatusSection";
import { PlannerSection } from "./sections/PlannerSection";

export function App() {
  const consoleState = useInvestigationConsole();
  const [route, setRoute] = useState<WorkspaceRoute>(() => resolveWorkspaceRoute(window.location.pathname));
  const queuedTasks = consoleState.scheduledTasks.filter((task) => task.status === "Queued");
  const processingTasks = consoleState.scheduledTasks.filter((task) => task.status === "Processing");
  const completedTasks = consoleState.scheduledTasks.filter((task) => task.status === "Completed");
  const currentPipelineTask = processingTasks[0] ?? queuedTasks[0] ?? completedTasks[0] ?? null;
  const pageTitle = useMemo(() => {
    if (route === "request") return "Request intake";
    if (route === "planner") return "Agent planner";
    if (route === "kanban") return "Kanban processing";
    return "Repository and API key";
  }, [route]);

  useEffect(() => {
    function syncRoute() {
      setRoute(resolveWorkspaceRoute(window.location.pathname));
    }

    window.addEventListener("popstate", syncRoute);
    return () => window.removeEventListener("popstate", syncRoute);
  }, []);

  function navigate(nextRoute: WorkspaceRoute) {
    const path = getWorkspaceRoutePath(nextRoute);
    if (window.location.pathname !== path) {
      window.history.pushState({}, "", path);
    }
    setRoute(nextRoute);
  }

  async function submitRequest() {
    if (await consoleState.submitRequest()) {
      navigate("planner");
    }
  }

  return (
    <main className="dashboard-shell">
      <DashboardSidebar
        activeWorkspaceId={consoleState.activeWorkspaceId}
        apiKey={consoleState.apiKey}
        completedCount={completedTasks.length}
        currentRoute={route}
        onCreateWorkspace={consoleState.createWorkspace}
        onNavigate={navigate}
        onWorkspaceChange={consoleState.setActiveWorkspaceId}
        queuedCount={queuedTasks.length}
        repoPath={consoleState.repoPath}
        repoUrl={consoleState.repoUrl}
        workspaces={consoleState.workspaces}
      />

      <section className="dashboard-main">
        <header className="dashboard-header">
          <div>
            <span className="eyebrow">Lead Agent Control Room</span>
            <h2>{pageTitle}</h2>
            <p className="header-workspace">{consoleState.activeWorkspace?.name ?? "Loading workspace"}</p>
          </div>
          <div className="status-pill">
            <CircleDot size={16} />
            {consoleState.run?.status ?? "Ready"}
          </div>
        </header>

        {consoleState.error && <div className="alert">{consoleState.error}</div>}

        {route === "request" && (
          <RequestPage
            onRequestChange={consoleState.setRequestText}
            onSubmitRequest={submitRequest}
            requestHistory={consoleState.requestHistory}
            requestText={consoleState.requestText}
          />
        )}

        {route === "planner" && (
          <>
            <PlannerSection
              agents={consoleState.agents}
              logs={consoleState.plannerLogs}
              onApprove={consoleState.approvePlannerLog}
              onUpdate={consoleState.updatePlannerLog}
              steps={consoleState.plannerSteps}
            />
            {consoleState.run?.result && <Results result={consoleState.run.result} />}
          </>
        )}

        {route === "kanban" && (
          <>
            <PipelineStatusSection currentTask={currentPipelineTask} />
            <KanbanSection
              agents={consoleState.agents}
              completedTasks={completedTasks}
              isProcessing={consoleState.isProcessingNext}
              isQueueing={consoleState.isQueueingTask}
              onProcessNext={consoleState.processNextTask}
              onQueueTask={consoleState.queueTask}
              onQueueSelected={consoleState.queueSelectedTask}
              onSelectTask={consoleState.setSelectedTaskId}
              onStartTask={consoleState.startQueuedTask}
              onAssignAgent={consoleState.assignTaskAgent}
              processingTasks={processingTasks}
              queuedTasks={queuedTasks}
              selectedTaskId={consoleState.selectedTask?.id ?? null}
              tasks={consoleState.backlogTasks}
            />
          </>
        )}

        {route === "configuration" && (
          <ConfigurationSection
            apiKey={consoleState.apiKey}
            events={consoleState.events}
            isSaving={consoleState.isSavingSettings}
            message={consoleState.settingsMessage}
            onApiKeyChange={consoleState.setApiKey}
            onJiraEndpointChange={consoleState.setJiraEndpoint}
            onNotionEndpointChange={consoleState.setNotionEndpoint}
            onRepoProviderChange={consoleState.setRepoProvider}
            onRepoPathChange={consoleState.setRepoPath}
            onRepoUrlChange={consoleState.setRepoUrl}
            onSave={consoleState.saveSettings}
            run={consoleState.run}
            settings={{
              jiraMcpEndpoint: consoleState.jiraEndpoint,
              notionMcpEndpoint: consoleState.notionEndpoint,
              repositoryPath: consoleState.repoPath,
              repositoryProvider: consoleState.repoProvider,
              repositoryUrl: consoleState.repoUrl
            }}
          />
        )}
      </section>
    </main>
  );
}
