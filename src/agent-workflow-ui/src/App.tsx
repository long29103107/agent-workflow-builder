import { Topbar } from "./components/Topbar";
import { InvestigationLane } from "./features/investigation/InvestigationLane";
import { Results } from "./features/results/Results";
import { RunStatus } from "./features/runs/RunStatus";
import { Timeline } from "./features/runs/Timeline";
import { SettingsPanel } from "./features/settings/SettingsPanel";
import { TaskPanel } from "./features/tasks/TaskPanel";
import { useInvestigationConsole } from "./hooks/useInvestigationConsole";

export function App() {
  const consoleState = useInvestigationConsole();

  return (
    <main className="shell">
      <Topbar status={consoleState.run?.status ?? "Ready"} />

      {consoleState.error && <div className="alert">{consoleState.error}</div>}

      <section className="workspace">
        <TaskPanel
          isLoading={consoleState.isLoadingTasks}
          onRefresh={consoleState.loadTasks}
          onSelectTask={consoleState.setSelectedTaskId}
          tasks={consoleState.tasks}
        />

        <InvestigationLane
          isInvestigating={consoleState.isInvestigating}
          onDropTask={consoleState.setSelectedTaskId}
          onStartInvestigation={consoleState.startInvestigation}
          selectedTask={consoleState.selectedTask}
        />

        <SettingsPanel
          isSaving={consoleState.isSavingSettings}
          jiraEndpoint={consoleState.jiraEndpoint}
          message={consoleState.settingsMessage}
          notionEndpoint={consoleState.notionEndpoint}
          onJiraEndpointChange={consoleState.setJiraEndpoint}
          onNotionEndpointChange={consoleState.setNotionEndpoint}
          onRepoPathChange={consoleState.setRepoPath}
          onSave={consoleState.saveSettings}
          repoPath={consoleState.repoPath}
        />
      </section>

      <section className="run-grid">
        <RunStatus run={consoleState.run} />
        <Timeline events={consoleState.events} />
      </section>

      {consoleState.run?.result && <Results result={consoleState.run.result} />}
    </main>
  );
}
