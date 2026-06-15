# REQUEST.md

## Goal

Build a runnable skeleton for an **Agent Workflow Orchestration Platform**.

The system should support one **Lead Agent** that orchestrates multiple **Subagents** to investigate tasks, read project repositories, use external MCP tools such as Jira/Notion, query memory, generate an execution plan, and expose everything through a React drag-and-drop UI.

## Tech Stack

* Frontend: React
* Backend: .NET
* Databases:

  * Neo4j for graph memory
  * Qdrant for vector RAG memory
  * Postgres if needed for metadata, sessions, runs, audit logs, task states
* External tools:

  * MCP connector for Jira
  * MCP connector for Notion
  * Git/repository reader tool

## Core Architecture

Implement the project around these components:

1. **Lead Agent**

   * Receives a task from the UI.
   * Reads Jira/Notion task details through MCP.
   * Reads relevant repository files.
   * Queries memory service.
   * Delegates work to Subagents.
   * Aggregates results.
   * Produces an investigation summary and execution plan.

2. **Subagents**

   * Create at least these placeholder subagents:

     * Repository Investigator Agent
     * Jira/Notion Context Agent
     * Memory Research Agent
     * Planning Agent
   * Each subagent should have a clear interface and mock implementation first.

3. **Memory Service**

   * RAG memory using Qdrant.
   * Graph memory using Neo4j.
   * Provide backend interfaces for:

     * storing memory
     * searching vector memory
     * reading graph relationships
     * linking task/repo/entity/context nodes

4. **MCP Tool Layer**

   * Define clean interfaces for MCP tools.
   * Add mock implementations for Jira and Notion first.
   * Design it so real MCP servers can be plugged in later.

5. **Repository Tool**

   * Support reading local repo metadata and files.
   * Start with a mock/local filesystem implementation.
   * Provide interface for future GitHub/GitLab integration.

6. **Workflow Engine**

   * Create orchestration flow:

     * user selects task
     * Lead Agent loads task context
     * Lead Agent queries repo context
     * Lead Agent queries memory
     * Lead Agent calls subagents
     * Lead Agent generates final plan
     * result is persisted
     * frontend displays investigation and plan

## Frontend Requirements

Create a React UI with:

* Task board / task list
* Drag-and-drop area to move a Jira task into an “Investigate” lane
* Button to start investigation
* Workflow run status
* Agent activity timeline
* Final investigation summary
* Generated execution plan
* Basic settings page for MCP endpoints and repo path

Use mock data if backend integration is not complete, but wire the API shape clearly.

## Backend Requirements

Create a .NET backend with:

* Clean project structure
* Domain models
* Application services
* Interfaces
* API controllers/endpoints
* Dependency injection setup
* Mock implementations for external systems
* Docker Compose support

Suggested APIs:

* `GET /api/tasks`
* `POST /api/workflows/investigate`
* `GET /api/workflows/{runId}`
* `GET /api/workflows/{runId}/events`
* `GET /api/memory/search`
* `POST /api/memory`
* `GET /api/repos/context`

## Data Models

Create initial models for:

* TaskItem
* WorkflowRun
* WorkflowEvent
* AgentMessage
* InvestigationResult
* ExecutionPlan
* ExecutionStep
* MemoryItem
* GraphEntity
* RepositoryContext

## Docker Compose

Provide local development setup for:

* backend
* frontend
* neo4j
* qdrant
* postgres if used

Include environment variables and default ports.

## Deliverables

Please create:

1. Repository structure
2. Minimal runnable backend
3. Minimal runnable frontend
4. Docker Compose dev setup
5. Mock Jira/Notion MCP tools
6. Mock repository reader
7. Mock Lead Agent and Subagents
8. Memory service interfaces for Neo4j and Qdrant
9. One working end-to-end investigation flow
10. README with:

    * architecture overview
    * how to run locally
    * API list
    * agent workflow explanation
    * future roadmap

## Implementation Style

* Prefer clean architecture but keep it pragmatic.
* Use interfaces so real MCP, real LLM, real GitHub, real Qdrant, and real Neo4j integrations can be added later.
* Keep the first version simple and runnable.
* Do not over-engineer.
* Add TODO comments where real integrations should be implemented.
* Make sure the project can start locally with clear instructions.

## Phased Backlog

Add a `BACKLOG.md` with this roadmap:

### Phase 1: Skeleton

* React UI
* .NET API
* Mock agents
* Mock MCP tools
* Mock repo reader
* Basic workflow run

### Phase 2: Real Memory

* Qdrant vector search
* Neo4j graph memory
* Memory ingestion
* Entity linking

### Phase 3: Real MCP

* Jira MCP integration
* Notion MCP integration
* Tool authentication
* Tool execution logs

### Phase 4: Real Repo Intelligence

* GitHub/GitLab connector
* Code search
* Dependency graph
* File summarization

### Phase 5: Advanced Agent Orchestration

* Parallel subagent execution
* Retry policy
* Approval gates
* Human-in-the-loop review
* Plan-to-PR workflow

## Expected First Output

Start by generating the repo structure and initial code files. Prioritize a runnable MVP over perfect completeness.
