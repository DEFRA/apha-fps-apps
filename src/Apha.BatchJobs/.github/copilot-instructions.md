# Workspace Copilot Instructions

## Required Prerequisites
- Docker Desktop is required for this workspace.
- PostgreSQL is required for this workspace.

## Mandatory Environment Rule
- Do not restart Docker Desktop.
- Do not restart PostgreSQL.
- Treat both services as long-running shared dependencies for this workspace.
- If a task appears to need a restart, stop and ask for explicit user approval first.

## Startup Checklist
- Assume Docker Desktop and PostgreSQL are already running.
- Use non-disruptive checks only (status/readiness), never restart commands.
