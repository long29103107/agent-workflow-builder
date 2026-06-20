import { Send } from "lucide-react";
import type { WorkspaceUserRequest } from "../types/workflow";

type RequestPageProps = {
  onRequestChange: (value: string) => void;
  onSubmitRequest: () => void;
  requestHistory: WorkspaceUserRequest[];
  requestText: string;
};

export function RequestPage({
  onRequestChange,
  onSubmitRequest,
  requestHistory,
  requestText
}: RequestPageProps) {
  return (
    <section className="panel request-page" id="request">
      <div className="panel-header">
        <div className="section-title">
          <Send size={18} />
          <h2>Request</h2>
        </div>
      </div>

      <label>
        User request
        <textarea value={requestText} onChange={(event) => onRequestChange(event.target.value)} />
      </label>

      <div className="request-actions">
        <button className="primary" disabled={!requestText.trim()} onClick={onSubmitRequest}>
          <Send size={17} />
          Submit request
        </button>
      </div>

      <section className="request-history" aria-label="Previous requests">
        <h3>Previous requests</h3>
        {requestHistory.length === 0 ? (
          <p className="muted">No previous requests yet.</p>
        ) : (
          <div className="request-history-list">
            {requestHistory.map((request) => (
              <article className="request-history-item" key={request.id}>
                <p>{request.content}</p>
                <span>{new Date(request.createdAt).toLocaleString()}</span>
              </article>
            ))}
          </div>
        )}
      </section>
    </section>
  );
}
