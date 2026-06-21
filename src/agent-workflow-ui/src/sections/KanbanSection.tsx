import { KanbanSquare, Loader2, Play } from "lucide-react";
import { useState, type DragEvent } from "react";
import type { ScheduledTask, TaskItem } from "../types/workflow";

type KanbanSectionProps = {
  agents: string[];
  isQueueing: boolean;
  isProcessing: boolean;
  onQueueSelected: () => void;
  onProcessNext: () => void;
  onQueueTask: (taskId: string) => void;
  onSelectTask: (taskId: string) => void;
  onStartTask: (taskId: string) => void;
  onAssignAgent: (taskId: string, agentName: string) => void;
  queuedTasks: ScheduledTask[];
  processingTasks: ScheduledTask[];
  completedTasks: ScheduledTask[];
  selectedTaskId: string | null;
  tasks: TaskItem[];
};

export function KanbanSection({
  agents,
  completedTasks,
  isQueueing,
  isProcessing,
  onProcessNext,
  onQueueTask,
  onQueueSelected,
  onSelectTask,
  onStartTask,
  onAssignAgent,
  processingTasks,
  queuedTasks,
  selectedTaskId,
  tasks
}: KanbanSectionProps) {
  const [draggedTask, setDraggedTask] = useState<DraggedTask | null>(null);
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
          agents={agents}
          description="Ideas and tasks that are not prioritized yet."
          title="Backlog"
          tasks={tasks}
          selectedTaskId={selectedTaskId}
          onSelectTask={onSelectTask}
          onAssignAgent={onAssignAgent}
          onDragTask={(taskId) => setDraggedTask(taskId ? { id: taskId, source: "backlog" } : null)}
        />
        <ScheduleColumn
          acceptsSource="backlog"
          description="Ready to start."
          draggedTask={draggedTask}
          isDraggable
          isStarting={isProcessing}
          onDragTask={(taskId) => setDraggedTask(taskId ? { id: taskId, source: "todo" } : null)}
          onDropTask={onQueueTask}
          onStartTask={onStartTask}
          title="Todo"
          tasks={queuedTasks}
        />
        <ScheduleColumn
          acceptsSource="todo"
          description="Development is in progress."
          draggedTask={draggedTask}
          onDragTask={(taskId) => setDraggedTask(taskId ? { id: taskId, source: "todo" } : null)}
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

type DraggedTask = {
  id: string;
  source: "backlog" | "todo";
};

type TaskColumnProps = {
  agents: string[];
  description: string;
  onSelectTask: (taskId: string) => void;
  onAssignAgent: (taskId: string, agentName: string) => void;
  onDragTask: (taskId: string | null) => void;
  selectedTaskId: string | null;
  tasks: TaskItem[];
  title: string;
};

function TaskColumn({
  agents,
  description,
  onAssignAgent,
  onDragTask,
  onSelectTask,
  selectedTaskId,
  tasks,
  title
}: TaskColumnProps) {
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
          <article
            className={"kanban-card" + (task.id === selectedTaskId ? " is-selected" : "")}
            draggable
            key={task.id}
            onDragEnd={() => onDragTask(null)}
            onDragStart={(event) => {
              event.dataTransfer.effectAllowed = "move";
              onDragTask(task.id);
            }}
            onClick={() => onSelectTask(task.id)}
          >
            <span>{task.key}</span>
            <strong>{task.title}</strong>
            <small>{task.priority}</small>
            <label className="kanban-agent-select" onClick={(event) => event.stopPropagation()}>
              <span>Agent</span>
              <select
                value={task.assignedAgent ?? ""}
                onChange={(event) => onAssignAgent(task.id, event.target.value)}
              >
                <option disabled value="">Assign agent</option>
                {agents.map((agent) => <option key={agent} value={agent}>{agent}</option>)}
              </select>
            </label>
          </article>
        ))}
      </div>
    </section>
  );
}

type ScheduleColumnProps = {
  acceptsSource?: DraggedTask["source"];
  description: string;
  draggedTask?: DraggedTask | null;
  isDraggable?: boolean;
  isStarting?: boolean;
  onDragTask?: (taskId: string | null) => void;
  onDropTask?: (taskId: string) => void;
  onStartTask?: (taskId: string) => void;
  tasks: ScheduledTask[];
  title: string;
};

function ScheduleColumn({
  acceptsSource,
  description,
  draggedTask,
  isDraggable = false,
  isStarting = false,
  onDragTask,
  onDropTask,
  onStartTask,
  tasks,
  title
}: ScheduleColumnProps) {
  const canDrop = Boolean(draggedTask && draggedTask.source === acceptsSource);

  function handleDrop() {
    if (!canDrop || !draggedTask || !onDropTask) return;
    onDropTask(draggedTask.id);
    onDragTask?.(null);
  }

  return (
    <section
      className={"kanban-column" + (canDrop ? " drop-target" : "")}
      onDragOver={(event) => {
        if (canDrop) {
          event.preventDefault();
          event.dataTransfer.dropEffect = "move";
        }
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
            onDragStart={(event: DragEvent<HTMLElement>) => {
              event.dataTransfer.effectAllowed = "move";
              onDragTask?.(task.id);
            }}
          >
            <span>{task.taskId}</span>
            <strong>{task.taskTitle}</strong>
            <small>{task.priority}</small>
            {task.assignedAgent && <small>Agent: {task.assignedAgent}</small>}
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
