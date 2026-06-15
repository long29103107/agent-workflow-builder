# Phase 003: Real MCP

Goal: replace mock Jira/Notion context with real MCP tool integration and auditable execution.

## Tasks

### 003_001: Add Jira MCP Provider

Things to do:

- Implement Jira task lookup behind `IJiraMcpTool`.
- Keep mock fallback.
- Add endpoint/auth configuration.
- Record tool execution outcome.

Status: planned

### 003_002: Add Notion MCP Provider

Things to do:

- Implement Notion page/database context behind `INotionContextTool`.
- Keep mock fallback.
- Add endpoint/auth configuration.
- Record retrieved context source.

Status: planned

### 003_003: Add Tool Authentication Boundary

Things to do:

- Define safe configuration for tool credentials.
- Keep secrets out of frontend and logs.
- Add validation and failure modes.

Status: planned

### 003_004: Add Tool Execution Logs

Things to do:

- Persist tool request, status, timing, and safe metadata.
- Avoid storing secrets.
- Show relevant events in workflow timeline.

Status: planned
