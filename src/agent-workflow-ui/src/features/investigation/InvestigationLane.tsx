import { Loader2, Play, Sparkles } from "lucide-react";
import type { DragEvent } from "react";
import type { TaskItem } from "../../types/workflow";

type InvestigationLaneProps = {
  isInvestigating: boolean;
  onDropTask: (taskId: string) => void;
  onStartInvestigation: () => void;
  selectedTask: TaskItem | null;
};

export function InvestigationLane({
  isInvestigating,
  onDropTask,
  onStartInvestigation,
  selectedTask
}: InvestigationLaneProps) {
  function handleDrop(event: DragEvent<HTMLDivElement>) {
    event.preventDefault();
    const taskId = event.dataTransfer.getData("text/plain");
    if (taskId) onDropTask(taskId);
  }

  return (
    <div className="investigate-lane" onDragOver={(event) => event.preventDefault()} onDrop={handleDrop}>
      <div className="lane-title">
        <Sparkles size={18} />
        <h2>Investigate</h2>
      </div>

      {selectedTask ? (
        <div className="selected-task">
          <span>{selectedTask.source}</span>
          <h3>
            {selectedTask.key}: {selectedTask.title}
          </h3>
          <p>{selectedTask.description}</p>
          <div className="selected-meta">
            <span>{selectedTask.priority}</span>
            <span>{selectedTask.status}</span>
          </div>
          <button className="primary" onClick={onStartInvestigation} disabled={isInvestigating}>
            {isInvestigating ? <Loader2 className="spin" size={18} /> : <Play size={18} />}
            Start Investigation
          </button>
        </div>
      ) : (
        <div className="drop-empty">Drop a task here to prepare a lead-agent investigation.</div>
      )}
    </div>
  );
}
