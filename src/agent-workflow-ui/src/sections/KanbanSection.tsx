import { KanbanSquare, Loader2, Play } from "lucide-react";
import { useState } from "react";
import type { ScheduledTask, TaskItem } from "../types/workflow";

type KanbanSectionProps = {
  isQueueing: boolean;
  isProcessing: boolean;
  onQueueSelected: () => void;
  onProcessNext: () => void;
  onSelectTask: (taskId: string) => void;
  onStartTask: (taskId: string) => void;
  queuedTasks: ScheduledTask[];
  processingTasks: ScheduledTask[];
  completedTasks: ScheduledTask[];
  selectedTaskId: string | null;
  tasks: TaskItem[];
};

export function KanbanSection({
  completedTasks,
  isQueueing,
  isProcessing,
  onProcessNext,
  onQueueSelected,
  onSelectTask,
  onStartTask,
  processingTasks,
  queuedTasks,
  selectedTaskId,
  tasks
}: KanbanSectionProps) {
  const [draggedTaskId, setDraggedTaskId] = useState<string | null>(null);
  const codeReviewTasks: ScheduledTask[] = [];
  const testingTasks: ScheduledTask[] = [];

  return (
    <section className="panel board-panel" id="kanban">
      <div className="panel-header">
        <div className="section-title">
          <KanbanSquare size={18} />
          <h2>Kanban Board</h2>
        </div>
        <div className="kanban-actions">
          <button className="secondary compact-action" disabled={!selectedTaskId || isQueueing} onClick={onQueueSelected}>
            {isQueueing ? <Loader2 className="spin" size={16} /> : <KanbanSquare size={16} />}
            Move to Todo
          </button>
          <button className="primary compact-action" disabled={queuedTasks.length === 0 || isProcessing} onClick={onProcessNext}>
            {isProcessing ? <Loader2 className="spin" size={16} /> : <Play size={16} />}
            Start next
          </button>
        </div>
      </div>

      <div className="kanban-board">
        <TaskColumn
          description="Ideas and tasks that are not prioritized yet."
          title="Backlog"
          tasks={tasks}
          selectedTaskId={selectedTaskId}
          onSelectTask={onSelectTask}
        />
        <ScheduleColumn
          description="Ready to start."
          isDraggable
          isStarting={isProcessing}
          onDragTask={setDraggedTaskId}
          onStartTask={onStartTask}
          title="Todo"
          tasks={queuedTasks}
        />
        <ScheduleColumn
          description="Development is in progress."
          draggedTaskId={draggedTaskId}
          isDropTarget
          onDragTask={setDraggedTaskId}
          onDropTask={onStartTask}
          title="In Progress"
          tasks={processingTasks}
        />
        <ScheduleColumn description="Waiting for review or merge." title="Code Review" tasks={codeReviewTasks} />
        <ScheduleColumn description="QA or UAT validation." title="Testing" tasks={testingTasks} />
        <ScheduleColumn description="Completed work." title="Done" tasks={completedTasks} />
      </div>
    </section>
  );
}

type TaskColumnProps = {
  description: string;
  onSelectTask: (taskId: string) => void;
  selectedTaskId: string | null;
  tasks: TaskItem[];
  title: string;
};

function TaskColumn({ description, onSelectTask, selectedTaskId, tasks, title }: TaskColumnProps) {
  return (
    <section className="kanban-column">
      <header>
        <div>
          <h3>{title}</h3>
          <p>{description}</p>
        </div>
        <span>{tasks.length}</span>
      </header>
      <div className="kanban-list">
        {tasks.map((task) => (
          <button
            className={"kanban-card" + (task.id === selectedTaskId ? " is-selected" : "")}
            key={task.id}
            onClick={() => onSelectTask(task.id)}
          >
            <span>{task.key}</span>
            <strong>{task.title}</strong>
            <small>{task.priority}</small>
          </button>
        ))}
      </div>
    </section>
  );
}

type ScheduleColumnProps = {
  description: string;
  draggedTaskId?: string | null;
  isDraggable?: boolean;
  isDropTarget?: boolean;
  isStarting?: boolean;
  onDragTask?: (taskId: string | null) => void;
  onDropTask?: (taskId: string) => void;
  onStartTask?: (taskId: string) => void;
  tasks: ScheduledTask[];
  title: string;
};

function ScheduleColumn({
  description,
  draggedTaskId,
  isDraggable = false,
  isDropTarget = false,
  isStarting = false,
  onDragTask,
  onDropTask,
  onStartTask,
  tasks,
  title
}: ScheduleColumnProps) {
  function handleDrop() {
    if (!draggedTaskId || !onDropTask) return;
    onDropTask(draggedTaskId);
    onDragTask?.(null);
  }

  return (
    <section
      className={"kanban-column" + (isDropTarget ? " drop-target" : "")}
      onDragOver={(event) => {
        if (isDropTarget) event.preventDefault();
      }}
      onDrop={handleDrop}
    >
      <header>
        <div>
          <h3>{title}</h3>
          <p>{description}</p>
        </div>
        <span>{tasks.length}</span>
      </header>
      <div className="kanban-list">
        {tasks.length === 0 && <p className="muted">Empty</p>}
        {tasks.map((task) => (
          <article
            className={"kanban-card readonly" + (isDraggable ? " draggable-card" : "")}
            draggable={isDraggable}
            key={task.id}
            onDragEnd={() => onDragTask?.(null)}
            onDragStart={() => onDragTask?.(task.id)}
          >
            <span>{task.taskId}</span>
            <strong>{task.taskTitle}</strong>
            <small>{task.priority}</small>
            {task.error && <small>{task.error}</small>}
            {onStartTask && (
              <button className="secondary inline-action" disabled={isStarting} onClick={() => onStartTask(task.id)}>
                {isStarting ? <Loader2 className="spin" size={14} /> : <Play size={14} />}
                Start
              </button>
            )}
          </article>
        ))}
      </div>
    </section>
  );
}
