# Phase 006: Implementation, Testing, And Review

Goal: turn an approved plan into an evidence-backed, reviewable diff inside the sandbox.

## Tasks

### 006_001: Add Coder Agent Boundary

Things to do:

- Define a role-specific Coder Agent contract in Core.
- Pass only approved plan steps, Project rules, and workspace references.
- Keep a deterministic mock implementation first.
- Prevent direct access to production environments.

Status: planned

### 006_002: Apply Approved Plan With File Evidence

Things to do:

- Modify only the isolated workspace.
- Record why each selected file is relevant and why it changed.
- Stop on unclear, protected, or out-of-plan actions.
- Preserve the generated diff before commit.

Status: planned

### 006_003: Add Tester Agent

Things to do:

- Generate or update focused tests.
- Run configured build and test commands through the sandbox.
- Capture redacted logs, exit codes, duration, and test summaries.
- Produce failure analysis and safe fix suggestions.

Status: planned

### 006_004: Add Reviewer Agent

Things to do:

- Review maintainability, security, performance, and architecture consistency.
- Link findings to diff locations and Project policies.
- Separate blocking findings from recommendations.
- Produce a typed review artifact.

Status: planned

### 006_005: Aggregate Implementation Artifacts

Things to do:

- Aggregate diff, changed files, test results, review findings, and release notes.
- Keep artifact versions stable and content-addressed.
- Show artifacts in API, CLI, MCP, and UI as appropriate.
- Do not commit or push yet.

Status: planned

### 006_006: Add Implementation Approval And Retry

Things to do:

- Approve the exact final diff and verification artifact hash.
- Allow reject, edit, retry, or regenerate.
- Invalidate approval when code or verification results change.
- Authorize commit and push only after approval.

Status: planned
