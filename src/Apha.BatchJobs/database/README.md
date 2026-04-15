# Batch DB Foundation Scripts

This folder contains infrastructure-only database scripts for local Docker Desktop PostgreSQL and future cloud PostgreSQL rollout.

## Structure

- `sql/001_batch_foundation_tables.sql` : Foundation schema and table creation.
- `sql/002_migration_template.sql` : Template for next migrations.
- `sql/003_runtime_orchestrator_tables.sql` : Runtime lock table setup and legacy cleanup.
- `sql/seeds/*.sql` : Seed scripts (optional, business-neutral by default).
- `sql/flush/*.sql` : Flush/reset scripts for local development cycles.
- `sink/` : Sink pipeline assets for cloud snapshot ingestion and curated sync.
- `Invoke-BatchDb.ps1` : PowerShell runner for apply/seed/flush/reset workflows.

## Prerequisites

- Docker Desktop running
- PostgreSQL container up (container name defaults to `batch_jobs_postgres`)

Start PostgreSQL if needed:

```bash
docker compose up -d postgres
```

## PowerShell Commands

Run from the solution folder:

```powershell
pwsh ./database/Invoke-BatchDb.ps1 -Action list
pwsh ./database/Invoke-BatchDb.ps1 -Action apply
pwsh ./database/Invoke-BatchDb.ps1 -Action seed
pwsh ./database/Invoke-BatchDb.ps1 -Action flush
pwsh ./database/Invoke-BatchDb.ps1 -Action reset
pwsh ./database/Invoke-BatchDb.ps1 -Action all
```

## Notes

- Scripts are intended to be re-runnable where possible.
- `flush` is destructive for schema `operational` and should only be used in local/dev environments.
- Keep business-specific seed data in dedicated files under `sql/seeds` and version them by prefixing with sequence numbers.
- Worker execution persistence writes to `operational.tbljobqueue` and `operational.tbljobqueue_log`.
- Runtime configuration follows the repo pattern: `ConnectionStrings:BatchJobsConnectionString`.

## Sink Pipeline

See `sink/README.md` for the VM workflow to:

1. Ingest a cloud snapshot into sink raw structures.
2. Register and reuse a `snapshot_id` for all pipeline stages.
3. Curate only required fields for foundation revisions.
4. Sync curated results into `operational` in an idempotent way.
