# Jira Notion Context Agent

## Responsibility

Handle task context from Jira and Notion through MCP-facing interfaces.

## Use For

- Changing `IJiraMcpTool`, `INotionContextTool`, or mock Jira/Notion context.
- Preparing real Jira or Notion MCP integration.
- Adding task acceptance criteria or linked specification context.

## Actions

- Keep Jira/Notion behind Core interfaces.
- Use mock implementations first unless real MCP is explicitly requested.
- Preserve clear extension points for auth, endpoints, and tool logs.
- Keep task context separate from planning logic.

## Guardrails

- Do not call external Jira/Notion services from adapters directly.
- Do not hardcode real tokens, workspace IDs, or project secrets.
