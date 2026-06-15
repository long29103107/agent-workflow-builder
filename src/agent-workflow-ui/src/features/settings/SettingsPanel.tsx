import { Loader2, Save, Settings } from "lucide-react";

type SettingsPanelProps = {
  isSaving: boolean;
  jiraEndpoint: string;
  message: string | null;
  notionEndpoint: string;
  onJiraEndpointChange: (value: string) => void;
  onNotionEndpointChange: (value: string) => void;
  onRepoPathChange: (value: string) => void;
  onSave: () => void;
  repoPath: string;
};

export function SettingsPanel({
  isSaving,
  jiraEndpoint,
  message,
  notionEndpoint,
  onJiraEndpointChange,
  onNotionEndpointChange,
  onRepoPathChange,
  onSave,
  repoPath
}: SettingsPanelProps) {
  return (
    <aside className="settings-panel">
      <div className="panel-header">
        <h2>Settings</h2>
        <Settings size={18} />
      </div>
      <label>
        Repository path
        <input value={repoPath} onChange={(event) => onRepoPathChange(event.target.value)} />
      </label>
      <label>
        Jira MCP endpoint
        <input value={jiraEndpoint} onChange={(event) => onJiraEndpointChange(event.target.value)} />
      </label>
      <label>
        Notion MCP endpoint
        <input value={notionEndpoint} onChange={(event) => onNotionEndpointChange(event.target.value)} />
      </label>
      <button className="secondary" onClick={onSave} disabled={isSaving}>
        {isSaving ? <Loader2 className="spin" size={18} /> : <Save size={18} />}
        Save Settings
      </button>
      <p className="settings-note">
        {message ?? "Endpoint fields are persisted in memory while the backend uses mock MCP tools."}
      </p>
    </aside>
  );
}
