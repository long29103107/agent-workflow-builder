import { Bot, CheckCircle2, Pencil, Plus, Save, Trash2, X } from "lucide-react";
import { useState } from "react";
import type { PlannerLog, PlannerStep } from "../types/workflow";

type PlannerSectionProps = {
  agents: string[];
  logs: PlannerLog[];
  onApprove: (plannerLogId: string) => void;
  onUpdate: (plannerLogId: string, steps: PlannerStep[]) => Promise<boolean>;
  steps: PlannerStep[];
};

export function PlannerSection({ agents, logs, onApprove, onUpdate, steps }: PlannerSectionProps) {
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
            <PendingPlannerLog
              agents={agents}
              key={log.id}
              log={log}
              onApprove={onApprove}
              onUpdate={onUpdate}
            />
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

type PendingPlannerLogProps = {
  agents: string[];
  log: PlannerLog;
  onApprove: (plannerLogId: string) => void;
  onUpdate: (plannerLogId: string, steps: PlannerStep[]) => Promise<boolean>;
};

function PendingPlannerLog({ agents, log, onApprove, onUpdate }: PendingPlannerLogProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [draftSteps, setDraftSteps] = useState<PlannerStep[]>(log.steps);

  function beginEditing() {
    setDraftSteps(log.steps);
    setIsEditing(true);
  }

  function cancelEditing() {
    setDraftSteps(log.steps);
    setIsEditing(false);
  }

  function updateStep(index: number, patch: Partial<PlannerStep>) {
    setDraftSteps((current) =>
      current.map((step, stepIndex) => (stepIndex === index ? { ...step, ...patch } : step))
    );
  }

  function addStep() {
    setDraftSteps((current) => [
      ...current,
      {
        title: "New plan step",
        detail: "Describe the expected work and result.",
        owner: agents[0] ?? "Lead Agent"
      }
    ]);
  }

  function removeStep(index: number) {
    setDraftSteps((current) => current.filter((_, stepIndex) => stepIndex !== index));
  }

  async function savePlan() {
    setIsSaving(true);
    const saved = await onUpdate(log.id, draftSteps);
    setIsSaving(false);
    if (saved) setIsEditing(false);
  }

  const isValid = draftSteps.length > 0 && draftSteps.every((step) =>
    step.title.trim() && step.detail.trim() && step.owner.trim()
  );

  return (
    <article className="planner-log">
      <div className="planner-log-header">
        <div>
          <span className="status-chip pending">Pending approval</span>
          <h3>{log.request}</h3>
          <small>{new Date(log.createdAt).toLocaleString()}</small>
        </div>
        <div className="planner-log-actions">
          {isEditing ? (
            <>
              <button className="secondary compact-action" onClick={cancelEditing}>
                <X size={16} />
                Cancel
              </button>
              <button
                className="primary compact-action"
                disabled={!isValid || isSaving}
                onClick={savePlan}
              >
                <Save size={16} />
                {isSaving ? "Saving..." : "Save plan"}
              </button>
            </>
          ) : (
            <>
              <button className="secondary compact-action" onClick={beginEditing}>
                <Pencil size={16} />
                Edit plan
              </button>
              <button className="primary compact-action" onClick={() => onApprove(log.id)}>
                <CheckCircle2 size={16} />
                Approve plan
              </button>
            </>
          )}
        </div>
      </div>

      {isEditing ? (
        <div className="planner-step-editor-list">
          {draftSteps.map((step, index) => (
            <article className="planner-step-editor" key={index}>
              <span>{index + 1}</span>
              <div className="planner-step-fields">
                <label>
                  <span>Title</span>
                  <input
                    value={step.title}
                    onChange={(event) => updateStep(index, { title: event.target.value })}
                  />
                </label>
                <label>
                  <span>Details</span>
                  <textarea
                    rows={3}
                    value={step.detail}
                    onChange={(event) => updateStep(index, { detail: event.target.value })}
                  />
                </label>
                <label>
                  <span>Assigned agent</span>
                  <select
                    value={step.owner}
                    onChange={(event) => updateStep(index, { owner: event.target.value })}
                  >
                    {!agents.includes(step.owner) && <option value={step.owner}>{step.owner}</option>}
                    {agents.map((agent) => <option key={agent} value={agent}>{agent}</option>)}
                  </select>
                </label>
              </div>
              <button
                aria-label="Remove plan step"
                className="icon-button planner-remove-step"
                disabled={draftSteps.length === 1}
                onClick={() => removeStep(index)}
              >
                <Trash2 size={16} />
              </button>
            </article>
          ))}
          <button className="secondary compact-action planner-add-step" onClick={addStep}>
            <Plus size={16} />
            Add step
          </button>
        </div>
      ) : (
        <PlannerStepList steps={log.steps} />
      )}
    </article>
  );
}

type PlannerStepListProps = {
  steps: PlannerStep[];
};

function PlannerStepList({ steps }: PlannerStepListProps) {
  return (
    <div className="planner-steps">
      {steps.map((step, index) => (
        <article className="planner-step" key={`${step.title}-${index}`}>
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
