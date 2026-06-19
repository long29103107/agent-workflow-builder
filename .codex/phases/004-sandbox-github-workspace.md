# Phase 004: Sandbox And GitHub Workspace

Goal: execute repository reads and writes inside isolated, policy-controlled per-run workspaces.

## Tasks

### 004_001: Add Execution Sandbox Boundary

Things to do:

- Define sandbox provision, execute, artifact, and destroy contracts in Core.
- Model workspace leases and lifecycle events.
- Require every code, command, and Git action to reference a workspace.
- Keep a mock provider for deterministic tests.

Status: planned

### 004_002: Add Local Docker Sandbox Provider

Things to do:

- Provision an isolated container per workflow run.
- Apply CPU, memory, timeout, network, environment, and filesystem limits.
- Keep production paths and credentials outside writable mounts.
- Capture redacted stdout, stderr, exit codes, and runtime.

Status: planned

### 004_003: Clone Repository And Detect Metadata

Things to do:

- Authenticate through the GitHub boundary.
- Clone into the configured workspace root.
- Detect default branch, base SHA, repository metadata, and project type.
- Emit clone evidence without logging credentials.

Status: planned

### 004_004: Checkout Clean Base And Create Branch

Things to do:

- Recreate or reset the run workspace safely.
- Checkout the selected base SHA.
- Apply the Project branch naming convention.
- Never mutate the default branch directly.

Status: planned

### 004_005: Enforce Workspace And Protected-Path Policy

Things to do:

- Restrict filesystem access to the verified workspace root.
- Enforce Project protected paths before and after changes.
- Require explicit policy for network and external write access.
- Block deployment commands by default.

Status: planned

### 004_006: Capture Artifacts And Destroy Workspace

Things to do:

- Capture repository metadata, diffs, logs, and generated artifacts.
- Apply artifact-retention rules.
- Destroy or quarantine the workspace after completion or failure.
- Prove cleanup cannot delete outside the configured workspace root.

Status: planned
