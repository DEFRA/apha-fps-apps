# BatchJobs Docs Summary

This folder is consolidated to a short operational summary.

## Architecture

- Trigger and status via API.
- Execution and orchestration in worker.
- Application layer resolves job handlers.
- Infrastructure layer persists lock and execution data.

## Execution Modes

- NoDb (Demo): in-memory execution path.
- WithDb (Development/Production): PostgreSQL-backed execution path.

## Deployment Essentials

- Container image built from Dockerfile.
- docker-compose profiles: withdb and nodb.
- Required production secret: ConnectionStrings__BatchJobsConnectionString.

## Testing Essentials

- Preferred: test-locally.ps1 or test-locally.sh.
- Verify expected logs, exit code, and job completion status.

## Canonical References

- ../README.md
- ../Apha.BatchJobs.Worker/README.md
- ../database/README.md