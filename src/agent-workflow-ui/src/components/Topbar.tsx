import { Activity } from "lucide-react";

type TopbarProps = {
  status: string;
};

export function Topbar({ status }: TopbarProps) {
  return (
    <header className="topbar">
      <div>
        <span className="eyebrow">Agent Workflow Orchestration</span>
        <h1>Investigation Console</h1>
      </div>
      <div className="status-pill">
        <Activity size={16} />
        {status}
      </div>
    </header>
  );
}
