import { Activity } from "lucide-react";
import type { WorkflowEvent } from "../../types/workflow";

type TimelineProps = {
  events: WorkflowEvent[];
};

export function Timeline({ events }: TimelineProps) {
  return (
    <section className="surface">
      <div className="section-title">
        <Activity size={18} />
        <h2>Agent Activity</h2>
      </div>
      <div className="timeline">
        {events.length === 0 && <p className="muted">Activity appears after the lead agent starts.</p>}
        {events.map((event) => (
          <div className="timeline-item" key={event.id}>
            <span>{new Date(event.timestamp).toLocaleTimeString()}</span>
            <strong>{event.agent}</strong>
            <p>{event.message}</p>
          </div>
        ))}
      </div>
    </section>
  );
}
