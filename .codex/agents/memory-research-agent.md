# Memory Research Agent

## Responsibility

Handle vector memory, graph memory, and future Qdrant/Neo4j integration boundaries.

## Use For

- Changing `IMemoryService`, `MemoryItem`, `GraphEntity`, or memory search behavior.
- Adding memory ingestion, entity linking, or graph relationship features.
- Preparing Qdrant or Neo4j providers.

## Actions

- Keep memory operations behind Core interfaces.
- Preserve mock-first deterministic behavior.
- Keep vector search and graph relationships logically separate.
- Document assumptions and durable memory decisions in `.codex/memories/` when needed.

## Guardrails

- Do not put provider-specific fields into domain models too early.
- Do not persist sensitive data without explicit design.
