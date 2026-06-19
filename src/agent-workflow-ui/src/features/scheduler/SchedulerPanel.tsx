import { ListRestart, Play, RefreshCw } from "lucide-react";
import type { ScheduledTask, TaskItem } from "../../types/workflow";

type SchedulerPanelProps = {
  isLoading: boolean;
  isProcessing: boolean;
  isQueueing: boolean;
  onProcessNext: () => void;
  onQueueSelected: () => void;
  onRefresh: () => void;
  scheduledTasks: ScheduledTask[];
  selectedTask: TaskItem | null;
};

export function SchedulerPanel({
  isLoading,
  isProcessing,
  isQueueing,
  onProcessNext,
  onQueueSelected,
  onRefresh,
  scheduledTasks,
  selectedTask
}: SchedulerPanelProps) {
  const hasQueuedTasks = scheduledTasks.some((task) => task.status === "Queued");

  return (
    <section className="surface scheduler-panel">
      <div className="panel-header">
        <div className="section-title">
          <ListRestart size={18} />
          <h2>Priority Scheduler</h2>
        </div>
        <button className="icon-button" disabled={isLoading} onClick={onRefresh} title="Refresh queue">
          <RefreshCw className={isLoading ? "spin" : ""} size={17} />
        </button>
      </div>

      <p>
        Core schedules Critical → High → Medium → Low and keeps FIFO order inside the same priority.
      </p>

      <div className="scheduler-actions">
        <button
          className="secondary"
          disabled={!selectedTask || isQueueing}
          onClick={onQueueSelected}
        >
          {isQueueing ? "Queueing..." : "Queue " + (selectedTask?.key ?? "selected task")}
        </button>
        <button
          className="primary"
          disabled={!hasQueuedTasks || isProcessing}
          onClick={onProcessNext}
        >
          <Play size={16} />
          {isProcessing ? "Processing..." : "Process next"}
        </button>
      </div>

      <div className="schedule-list">
        {scheduledTasks.length === 0 && <p className="muted">The scheduler queue is empty.</p>}
        {scheduledTasks.map((task) => (
          <article className="schedule-item" key={task.id}>
            <div>
              <strong>{task.taskTitle}</strong>
              <small>{task.taskId}</small>
            </div>
            <span className={"priority priority-" + task.priority.toLowerCase()}>
              {task.priority}
            </span>
            <span className={"queue-status queue-status-" + task.status.toLowerCase()}>
              {task.status}
            </span>
          </article>
        ))}
      </div>
    </section>
  );
}
