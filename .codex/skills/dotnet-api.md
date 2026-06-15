# .NET API Skill

Use this when modifying `src/AgentWorkflow.Api`.

## Checklist

- Keep Minimal API endpoints grouped under `/api`.
- Validate required request fields before starting a workflow run.
- Pass `CancellationToken` into async service calls.
- Register new services through dependency injection in `Program.cs`.
- Keep mock implementations deterministic and easy to replace.
- Prefer records for simple contracts and DTOs.

## Verification

```powershell
dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
```
