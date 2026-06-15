# Phase 004: Real Repository Intelligence

Goal: replace shallow local repository context with deeper code intelligence and remote provider support.

## Tasks

### 004_001: Add GitHub Or GitLab Connector

Things to do:

- Add provider interface for remote repository context.
- Keep local filesystem reader as fallback.
- Support branch/ref selection.

Status: planned

### 004_002: Add Code Search

Things to do:

- Search files by task keywords.
- Return high-signal matches.
- Avoid generated and dependency folders.

Status: planned

### 004_003: Add Dependency Graph

Things to do:

- Read project references and package dependencies.
- Summarize affected modules.
- Surface dependency risks in plan.

Status: planned

### 004_004: Add File Summarization

Things to do:

- Summarize selected files for agent context.
- Keep summaries bounded and source-linked.
- Avoid summarizing secrets.

Status: planned
