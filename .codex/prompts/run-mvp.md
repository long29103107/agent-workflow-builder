# Run MVP Prompt

Use this when checking the runnable skeleton.

1. Start the backend:

```powershell
dotnet run --project src/AgentWorkflow.Api
```

2. Start the frontend:

```powershell
cd src/agent-workflow-ui
bun install
bun run dev
```

3. Exercise the flow:

- Open `http://localhost:5173`.
- Drag a Jira task into the Investigate lane.
- Start investigation.
- Confirm the workflow status, agent timeline, investigation summary, execution plan, repository context, memory matches, and graph links render.
