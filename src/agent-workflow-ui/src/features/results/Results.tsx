import { Brain, Database, GitBranch } from "lucide-react";
import type { InvestigationResult } from "../../types/workflow";

type ResultsProps = {
  result: InvestigationResult;
};

export function Results({ result }: ResultsProps) {
  return (
    <section className="results">
      <div className="summary-band">
        <Brain size={22} />
        <div>
          <h2>Investigation Summary</h2>
          <p>{result.summary}</p>
        </div>
      </div>

      <div className="result-grid">
        <section className="surface">
          <div className="section-title">
            <GitBranch size={18} />
            <h2>{result.plan.title}</h2>
          </div>
          <div className="steps">
            {result.plan.steps.map((step) => (
              <article className="step" key={`${step.order}-${step.title}`}>
                <span>{step.order}</span>
                <div>
                  <h3>{step.title}</h3>
                  <p>{step.description}</p>
                  <small>
                    {step.ownerAgent} - {step.status}
                  </small>
                </div>
              </article>
            ))}
          </div>
        </section>

        <section className="surface">
          <div className="section-title">
            <Database size={18} />
            <h2>Context</h2>
          </div>
          <h3>{result.repositoryContext.name}</h3>
          <p>{result.repositoryContext.summary}</p>
          <div className="tags">
            {result.repositoryContext.technologies.map((tech) => (
              <span key={tech}>{tech}</span>
            ))}
          </div>
          <h3>Evidence</h3>
          <p className="agent-note">{result.plan.evidenceSummary}</p>
          <ul className="compact-list">
            {result.plan.sourceReferences.map((sourceReference) => (
              <li key={sourceReference}>{sourceReference}</li>
            ))}
          </ul>
          <h3>Agent Notes</h3>
          {result.agentMessages.map((message) => (
            <p className="agent-note" key={message.agentName}>
              <strong>{message.agentName}</strong>: {message.content}
            </p>
          ))}
          <h3>Risks</h3>
          <ul className="compact-list">
            {result.plan.risks.map((risk) => (
              <li key={risk}>{risk}</li>
            ))}
          </ul>
          <h3>Open Questions</h3>
          <ul className="compact-list">
            {result.plan.openQuestions.map((question) => (
              <li key={question}>{question}</li>
            ))}
          </ul>
          <h3>Memory Matches</h3>
          {result.memoryItems.map((memory) => (
            <p className="agent-note" key={memory.id}>
              <strong>{memory.title}</strong>: {memory.content}
            </p>
          ))}
          <h3>Graph Links</h3>
          <div className="graph-list">
            {result.graphEntities.map((entity) => (
              <span key={entity.id}>
                {entity.type}: {entity.name}
              </span>
            ))}
          </div>
        </section>
      </div>
    </section>
  );
}
