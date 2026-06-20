import { CheckCircle2, Circle, GitBranch, Loader2 } from "lucide-react";
import type { ScheduledTask } from "../types/workflow";

type PipelineStatusSectionProps = {
  currentTask: ScheduledTask | null;
};

const pipelineStages = [
  "Todo",
  "In Progress",
  "Code Review",
  "Testing",
  "Done"
];

export function PipelineStatusSection({ currentTask }: PipelineStatusSectionProps) {
  const activeIndex = getActiveStageIndex(currentTask);

  return (
    <section className="panel pipeline-panel" id="task-pipeline">
      <div className="panel-header">
        <div className="section-title">
          <GitBranch size={18} />
          <h2>Task Pipeline</h2>
        </div>
        <span className={"pipeline-state " + getPipelineStateClass(currentTask)}>
          {currentTask?.status ?? "Waiting"}
        </span>
      </div>

      <div className="pipeline-current">
        <span>{currentTask?.taskId ?? "No active task"}</span>
        <strong>{currentTask?.taskTitle ?? "Move a Todo task into In Progress to start the pipeline."}</strong>
      </div>

      <div className="pipeline-stages">
        {pipelineStages.map((stage, index) => {
          const stageState = getStageState(index, activeIndex, currentTask);
          return (
            <div className={"pipeline-stage " + stageState} key={stage}>
              <span>
                {stageState === "complete" && <CheckCircle2 size={16} />}
                {stageState === "active" && <Loader2 className="spin" size={16} />}
                {stageState === "pending" && <Circle size={16} />}
              </span>
              <strong>{stage}</strong>
            </div>
          );
        })}
      </div>
    </section>
  );
}

function getActiveStageIndex(task: ScheduledTask | null) {
  if (!task) return -1;
  if (task.status === "Queued") return 0;
  if (task.status === "Processing") return 1;
  if (task.status === "Completed") return 4;
  return 1;
}

function getStageState(index: number, activeIndex: number, task: ScheduledTask | null) {
  if (!task) return "pending";
  if (task.status === "Completed") return "complete";
  if (index < activeIndex) return "complete";
  if (index === activeIndex) return "active";
  return "pending";
}

function getPipelineStateClass(task: ScheduledTask | null) {
  if (!task) return "waiting";
  return task.status.toLowerCase();
}
