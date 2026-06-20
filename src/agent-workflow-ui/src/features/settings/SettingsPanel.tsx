import { Loader2, Save, Settings } from "lucide-react";

type SettingsPanelProps = {
  apiKey: string;
  isSaving: boolean;
  jiraEndpoint: string;
  message: string | null;
  onApiKeyChange: (value: string) => void;
  notionEndpoint: string;
  onJiraEndpointChange: (value: string) => void;
  onNotionEndpointChange: (value: string) => void;
  onRepoProviderChange: (value: string) => void;
  onRepoPathChange: (value: string) => void;
  onRepoUrlChange: (value: string) => void;
  onSave: () => void;
  repoProvider: string;
  repoPath: string;
  repoUrl: string;
};

export function SettingsPanel({
  apiKey,
  isSaving,
  jiraEndpoint,
  message,
  onApiKeyChange,
  notionEndpoint,
  onJiraEndpointChange,
  onNotionEndpointChange,
  onRepoProviderChange,
  onRepoPathChange,
  onRepoUrlChange,
  onSave,
  repoProvider,
  repoPath,
  repoUrl
}: SettingsPanelProps) {
  return (
    <aside className="settings-panel">
      <div className="panel-header">
        <h2>Repository & Secrets</h2>
        <Settings size={18} />
      </div>
      <label>
        Repository path
        <input value={repoPath} onChange={(event) => onRepoPathChange(event.target.value)} />
      </label>
      <label>
        Repository URL
        <input value={repoUrl} onChange={(event) => onRepoUrlChange(event.target.value)} />
      </label>
      <label>
        Repository provider
        <select value={repoProvider} onChange={(event) => onRepoProviderChange(event.target.value)}>
          <option value="github">GitHub</option>
          <option value="local">Local</option>
        </select>
      </label>
      <label>
        API key
        <input
          autoComplete="off"
          type="password"
          value={apiKey}
          onChange={(event) => onApiKeyChange(event.target.value)}
        />
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
        {message ?? "Repository settings are saved to the API session. The API key field stays in this UI session."}
      </p>
    </aside>
  );
}
