```
New-Item -ItemType Directory -Force "src/AgentWorkflow.Core/Infrastructure/Persistence/MigrationScripts" | Out-Null; dotnet ef migrations script 0 20260621050123_InitialPostgresPersistence --idempotent --project src/AgentWorkflow.Core --startup-project src/AgentWorkflow.Api --output "src/AgentWorkflow.Core/Infrastructure/Persistence/MigrationScripts/20260621050123_InitialPostgresPersistence.sql"
```

```powershell
dotnet ef migrations script 20260621050123_InitialPostgresPersistence 20260621132104_AddDurableWorkflowStateMachine --idempotent --project src/AgentWorkflow.Core --startup-project src/AgentWorkflow.Api --output "src/AgentWorkflow.Core/Infrastructure/Persistence/MigrationScripts/20260621132104_AddDurableWorkflowStateMachine.sql"
```

```powershell
dotnet ef migrations script 20260621132104_AddDurableWorkflowStateMachine 20260622014134_AddStructuredEvidenceAndArtifacts --idempotent --project src/AgentWorkflow.Core --startup-project src/AgentWorkflow.Api --output "src/AgentWorkflow.Core/Infrastructure/Persistence/MigrationScripts/20260622014134_AddStructuredEvidenceAndArtifacts.sql"
```

```powershell
dotnet ef migrations script 20260622014134_AddStructuredEvidenceAndArtifacts 20260622022720_AddApprovalPolicyEngine --idempotent --project src/AgentWorkflow.Core --startup-project src/AgentWorkflow.Api --output "src/AgentWorkflow.Core/Infrastructure/Persistence/MigrationScripts/20260622022720_AddApprovalPolicyEngine.sql"
```

```powershell
dotnet ef migrations script 20260622022720_AddApprovalPolicyEngine 20260622032935_AddTaskActivityHistory --idempotent --project src/AgentWorkflow.Core --startup-project src/AgentWorkflow.Api --output "src/AgentWorkflow.Core/Infrastructure/Persistence/MigrationScripts/20260622032935_AddTaskActivityHistory.sql"
```
