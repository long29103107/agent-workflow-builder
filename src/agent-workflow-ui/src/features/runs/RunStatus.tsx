import { CheckCircle2 } from "lucide-react";
import type { WorkflowRun } from "../../types/workflow";

type RunStatusProps = {
  run: WorkflowRun | null;
};

export function RunStatus({ run }: RunStatusProps) {
  return (
    <section className="surface">
      <div className="section-title">
        <CheckCircle2 size={18} />
        <h2>Workflow Run</h2>
      </div>
      {run ? (
        <dl className="run-details">
          <div>
            <dt>Run ID</dt>
            <dd>{run.id}</dd>
          </div>
          <div>
            <dt>Task</dt>
            <dd>{run.taskId}</dd>
          </div>
          <div>
            <dt>Status</dt>
            <dd>{run.status}</dd>
          </div>
          <div>
            <dt>Started</dt>
            <dd>{new Date(run.startedAt).toLocaleString()}</dd>
          </div>
        </dl>
      ) : (
        <p className="muted">No investigation has run yet.</p>
      )}
    </section>
  );
}
