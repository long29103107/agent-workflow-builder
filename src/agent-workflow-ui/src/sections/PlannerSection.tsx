import { Bot, CheckCircle2 } from "lucide-react";
import type { PlannerLog, PlannerStep } from "../hooks/useInvestigationConsole";

type PlannerSectionProps = {
  logs: PlannerLog[];
  onApprove: (plannerLogId: string) => void;
  steps: PlannerStep[];
};

export function PlannerSection({ logs, onApprove, steps }: PlannerSectionProps) {
  const pendingLogs = logs.filter((log) => log.status === "PendingApproval");
  const approvedLogs = logs.filter((log) => log.status === "Approved");

  return (
    <section className="panel planner-panel" id="planner">
      <div className="section-title">
        <Bot size={18} />
        <h2>Agent Planner</h2>
      </div>

      {logs.length === 0 ? (
        <>
          <p className="muted">No submitted requests are waiting for planning.</p>
          <PlannerStepList steps={steps} />
        </>
      ) : (
        <div className="planner-log-list">
          {pendingLogs.map((log) => (
            <article className="planner-log" key={log.id}>
              <div className="planner-log-header">
                <div>
                  <span className="status-chip pending">Pending approval</span>
                  <h3>{log.request}</h3>
                  <small>{new Date(log.createdAt).toLocaleString()}</small>
                </div>
                <button className="primary compact-action" onClick={() => onApprove(log.id)}>
                  <CheckCircle2 size={16} />
                  Approve plan
                </button>
              </div>
              <PlannerStepList steps={log.steps} />
            </article>
          ))}

          {approvedLogs.map((log) => (
            <article className="planner-log approved" key={log.id}>
              <div className="planner-log-header">
                <div>
                  <span className="status-chip approved">Approved</span>
                  <h3>{log.request}</h3>
                  <small>{new Date(log.createdAt).toLocaleString()}</small>
                </div>
              </div>
              <PlannerStepList steps={log.steps} />
            </article>
          ))}
        </div>
      )}
    </section>
  );
}

type PlannerStepListProps = {
  steps: PlannerStep[];
};

function PlannerStepList({ steps }: PlannerStepListProps) {
  return (
    <div className="planner-steps">
      {steps.map((step, index) => (
        <article className="planner-step" key={step.title}>
          <span>{index + 1}</span>
          <div>
            <strong>{step.title}</strong>
            <p>{step.detail}</p>
            <small>{step.owner}</small>
          </div>
        </article>
      ))}
    </div>
  );
}
