# 001_008: Split AgentWorkflow.Cli Entrypoint

## Phase

001: Skeleton And Operating System

## Task

001_008: Split AgentWorkflow.Cli Entrypoint

## Goal

Refactor `src/AgentWorkflow.Cli` so `Program.cs` stays thin while preserving the existing task ID, repository path, workflow execution, and JSON output behavior.

## Implementation Log

- Added the task entry to `.codex/phases/001-skeleton.md`.
- Added `CliOptions` to parse the existing positional CLI arguments.
- Added `CliRunner` to execute the investigation workflow and write indented JSON output.
- Added `AddAgentWorkflowCli` to register Core services and the CLI runner.
- Reduced `Program.cs` to option parsing, DI setup, runner resolution, and process exit code.

## Verification

- `dotnet build src/AgentWorkflow.Cli/AgentWorkflow.Cli.csproj` passed with 0 warnings and 0 errors.
- `dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .` passed and returned JSON with `Status: Completed`.

## Goal Achieved

Yes. `Program.cs` is thin and the existing CLI smoke command still completes successfully.

## Next Idea

Add explicit subcommands such as `investigate` and `doctor` once the CLI needs a durable external command surface.
