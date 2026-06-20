import { RunStatus } from "../features/runs/RunStatus";
import { Timeline } from "../features/runs/Timeline";
import { SettingsPanel } from "../features/settings/SettingsPanel";
import type { ToolEndpointSettings, WorkflowEvent, WorkflowRun } from "../types/workflow";

type ConfigurationSectionProps = {
  apiKey: string;
  events: WorkflowEvent[];
  isSaving: boolean;
  message: string | null;
  onApiKeyChange: (value: string) => void;
  onJiraEndpointChange: (value: string) => void;
  onNotionEndpointChange: (value: string) => void;
  onRepoProviderChange: (value: string) => void;
  onRepoPathChange: (value: string) => void;
  onRepoUrlChange: (value: string) => void;
  onSave: () => void;
  run: WorkflowRun | null;
  settings: ToolEndpointSettings;
};

export function ConfigurationSection({
  apiKey,
  events,
  isSaving,
  message,
  onApiKeyChange,
  onJiraEndpointChange,
  onNotionEndpointChange,
  onRepoProviderChange,
  onRepoPathChange,
  onRepoUrlChange,
  onSave,
  run,
  settings
}: ConfigurationSectionProps) {
  return (
    <section className="config-run-grid" id="configuration">
      <SettingsPanel
        apiKey={apiKey}
        isSaving={isSaving}
        jiraEndpoint={settings.jiraMcpEndpoint}
        message={message}
        notionEndpoint={settings.notionMcpEndpoint}
        onApiKeyChange={onApiKeyChange}
        onJiraEndpointChange={onJiraEndpointChange}
        onNotionEndpointChange={onNotionEndpointChange}
        onRepoProviderChange={onRepoProviderChange}
        onRepoPathChange={onRepoPathChange}
        onRepoUrlChange={onRepoUrlChange}
        onSave={onSave}
        repoProvider={settings.repositoryProvider}
        repoPath={settings.repositoryPath}
        repoUrl={settings.repositoryUrl}
      />

      <div className="run-stack">
        <RunStatus run={run} />
        <Timeline events={events} />
      </div>
    </section>
  );
}
