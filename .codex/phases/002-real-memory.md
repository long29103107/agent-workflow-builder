# Phase 002: Real Memory

Goal: replace mock memory behavior with real vector and graph memory while preserving Core interfaces.

## Tasks

### 002_001: Add Qdrant Vector Search Provider

Things to do:

- Implement a provider behind `IMemoryService`.
- Keep mock fallback for local runs.
- Add configuration for endpoint and collection.
- Verify vector search via CLI smoke test.

Status: planned

### 002_002: Add Neo4j Graph Memory Provider

Things to do:

- Implement graph relationship read/write behind `IMemoryService`.
- Add configuration for Neo4j connection.
- Keep entity relationships aligned with task/repo/context model.

Status: planned

### 002_003: Add Memory Ingestion Flow

Things to do:

- Define ingestion contracts.
- Store task, repo, decision, and context memory.
- Avoid storing secrets or transient logs as durable memory.

Status: planned

### 002_004: Add Entity Linking

Things to do:

- Link task/repo/entity/context nodes.
- Add deterministic IDs or lookup rules.
- Expose links in investigation result.

Status: planned
