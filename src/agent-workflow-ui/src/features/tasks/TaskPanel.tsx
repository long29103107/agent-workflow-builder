import { Loader2, RefreshCcw } from "lucide-react";
import type { DragEvent } from "react";
import type { TaskItem } from "../../types/workflow";

type TaskPanelProps = {
  isLoading: boolean;
  onRefresh: () => void;
  onSelectTask: (taskId: string) => void;
  tasks: TaskItem[];
};

export function TaskPanel({ isLoading, onRefresh, onSelectTask, tasks }: TaskPanelProps) {
  function handleDragStart(event: DragEvent<HTMLDivElement>, taskId: string) {
    event.dataTransfer.setData("text/plain", taskId);
  }

  return (
    <div className="task-panel">
      <div className="panel-header">
        <h2>Jira Tasks</h2>
        <button className="icon-button" onClick={onRefresh} title="Refresh tasks">
          {isLoading ? <Loader2 className="spin" size={18} /> : <RefreshCcw size={18} />}
        </button>
      </div>

      <div className="task-list">
        {tasks.map((task) => (
          <div
            className="task-card"
            draggable
            key={task.id}
            onClick={() => onSelectTask(task.id)}
            onDragStart={(event) => handleDragStart(event, task.id)}
          >
            <div className="task-row">
              <strong>{task.key}</strong>
              <span>{task.priority}</span>
            </div>
            <h3>{task.title}</h3>
            <p>{task.description}</p>
            <div className="tags">
              {task.tags.map((tag) => (
                <span key={tag}>{tag}</span>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
