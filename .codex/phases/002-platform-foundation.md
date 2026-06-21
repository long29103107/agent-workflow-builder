# Phase 002: Platform Foundation

Goal: establish the durable Project and EngineeringTask foundation before repository write automation.

## Tasks

### 002_001: Add GitHub Repository Connection Boundary

Things to do:

- Add repository connection contracts and models in Core.
- Keep local repository path behavior working.
- Add mock GitHub URL resolution without network calls.
- Expose repository connection through API, CLI, MCP, and UI settings.

Status: done

### 002_002: Restore Solution And Test Baseline

Things to do:

- Restore AgentWorkflowBuilder.slnx with every current source project.
- Add Core unit tests and API integration-test projects.
- Add compatibility smoke tests for CLI and MCP.
- Make the solution build and test commands the Phase 2 quality gate.

Status: done

### 002_003: Add Project Aggregate And Policies

Things to do:

- Add Project repository, GitHub, agent, coding-standard, command, branch, protected-path, and approval settings.
- Keep domain models and contracts in Core.
- Add validation for unsafe or incomplete project policies.
- Seed one default local project without breaking current settings.

Status: done

### 002_004: Add EngineeringTask And WorkItem Separation

Things to do:

- Add EngineeringTask as the platform-owned user request.
- Add typed lifecycle states from New through Completed or Failed.
- Keep Jira and Notion inputs as source WorkItems linked to a task.
- Preserve current mock task-list compatibility.

Status: done

### 002_005: Add PostgreSQL Persistence

Things to do:

- Replace in-memory project, task, run, approval, and evidence storage behind Core interfaces.
- Add schema migrations and development configuration.
- Keep Qdrant and Neo4j out of authoritative workflow state.
- Avoid persisting secrets.

Status: done

### 002_006: Add Project And Task APIs

Things to do:

- Add project CRUD and project-scoped task APIs.
- Add task lifecycle and source-link responses.
- Preserve current API routes during migration.
- Keep API handlers thin over Core application services.

Status: planned
