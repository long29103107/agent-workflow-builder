# Phase 002: GitHub Repository Workspace

Goal: let workflows target a GitHub repository through a mock-first repository connection boundary before adding real clone, checkout, branch, push, and draft PR behavior.

## Tasks

### 002_001: Add GitHub Repository Connection Boundary

Things to do:

- Add repository connection contracts and models in Core.
- Keep local repository path behavior working.
- Add mock GitHub URL resolution without network calls.
- Expose repository connection through API, CLI, MCP, and UI settings.
- Update knowledge and task memory.
- Verify API, CLI, MCP, and frontend builds.

Status: done

### 002_002: Clone Repository Into Workflow Workspace

Things to do:

- Add a workspace abstraction behind Core.
- Clone GitHub repositories into isolated local workflow directories.
- Detect default branch and repository metadata.
- Keep mock fallback runnable without credentials.

Status: planned

### 002_003: Checkout And Clean Workspace Per Run

Things to do:

- Reset or recreate the run workspace before investigation.
- Checkout the selected base branch.
- Emit workspace lifecycle events.
- Avoid deleting files outside the configured workspace root.

Status: planned

### 002_004: Read Repository Context From Workspace

Things to do:

- Point repository context reads at the cloned workspace.
- Include repository metadata in investigation results.
- Keep local path and mock GitHub URL modes working.

Status: planned
