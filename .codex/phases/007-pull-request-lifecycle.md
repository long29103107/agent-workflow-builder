# Phase 007: Pull Request Lifecycle

Goal: publish only approved artifacts and keep GitHub state linked to the EngineeringTask.

## Tasks

### 007_001: Add GitHub App Authentication

Things to do:

- Add GitHub App installation and repository authorization boundaries.
- Store secret references outside Project payloads and logs.
- Validate repository permission before any write.
- Keep a mock provider for tests and local runs.

Status: planned

### 007_002: Commit And Push Approved Changes

Things to do:

- Create commits only from approved diff artifacts.
- Bind commit metadata to the task and workflow run.
- Push the implementation branch without force by default.
- Make commit and push operations idempotent and auditable.

Status: planned

### 007_003: Add Pull Request Agent

Things to do:

- Generate PR title, body, release notes, and evidence links.
- Use the approved implementation and target branch.
- Keep the agent from creating the PR directly.
- Store a versioned PR proposal artifact.

Status: planned

### 007_004: Add Pull Request Approval And Creation

Things to do:

- Let users approve or edit PR title, body, release notes, and target branch.
- Invalidate approval when proposal inputs change.
- Create a draft PR after approval.
- Link PR number, URL, branches, and head SHA to the task.

Status: planned

### 007_005: Track Reviews, Checks, And Workflows

Things to do:

- Refresh PR state, reviews, comments, status checks, and GitHub Actions runs.
- Show blocking and pending checks.
- Record provider failures without leaking credentials.
- Keep task history aligned with GitHub state.

Status: planned

### 007_006: Add Merge Approval And Execution

Things to do:

- Request merge approval for the exact current head SHA.
- Revalidate required checks and policy immediately before merge.
- Prevent stale approvals and unauthorized merge methods.
- Record merge commit and final task outcome.

Status: planned

