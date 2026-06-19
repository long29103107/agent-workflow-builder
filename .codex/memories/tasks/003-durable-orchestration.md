# Phase 003 Memory: Durable Orchestration, Approval, And Evidence

## Phase

003: Durable Orchestration, Approval, And Evidence

## 003_007: Add Mock-First Priority Task Scheduler

- Goal: prove Core-owned task priority, queue claiming, API processing, and UI monitoring before durable background orchestration is introduced.
- Implementation: added scheduler priority/status/request/result models and ITaskScheduler in Core; added a lock-protected InMemoryTaskScheduler with Critical-to-Low ordering, FIFO tie-breaking, duplicate-active validation, cancellation requeue, concurrent claiming, and workflow-engine processing; registered it in Core DI; added API list/get/enqueue/process-next endpoints with string enums; added a React Priority Scheduler panel and hook/client state; left CLI and MCP contracts unchanged.
- Verification: dotnet build AgentWorkflowBuilder.slnx passed with 0 warnings and 0 errors; dotnet test passed 4 Core unit tests and 2 API integration tests; bun run build passed; CLI and MCP compatibility smokes completed.
- Goal achieved: users can select a current task, queue it with source-derived priority, inspect queue status, and manually process the next task through the shared workflow engine.
- Next idea: 003_001 through 003_006 still own durable state, background workers, evidence, approvals, SSE, and recovery; do not treat this in-memory manual processor as their replacement.
