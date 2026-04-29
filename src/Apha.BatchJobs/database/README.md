# BatchJobs Database

Minimal reference for local schema setup and maintenance.

## Purpose

<<<<<<< HEAD
- `sql/001_batch_foundation_tables.sql` : Foundation schema and table creation.
- `sql/002_migration_template.sql` : Template for next migrations.
- `sql/003_runtime_orchestrator_tables.sql` : Runtime lock table setup and legacy cleanup.
- `sql/seeds/*.sql` : Seed scripts (optional, business-neutral by default).
- `sql/flush/*.sql` : Flush/reset scripts for local development cycles.
- `sink/` : Sink pipeline assets for cloud snapshot ingestion and curated sync.
- `Invoke-BatchDb.ps1` : PowerShell runner for apply/seed/flush/reset/validate workflows.
=======
- Holds SQL for foundation and runtime orchestration tables.
- Supports local Docker and local PostgreSQL test flows.
>>>>>>> A-Foundation

## Important Paths

- database/sql/001_batch_foundation_tables.sql
- database/sql/003_runtime_orchestrator_tables.sql
- database/Invoke-BatchDb.ps1

## Common Commands

From src/Apha.BatchJobs:

```powershell
pwsh ./database/Invoke-BatchDb.ps1 -Action list
pwsh ./database/Invoke-BatchDb.ps1 -Action apply
pwsh ./database/Invoke-BatchDb.ps1 -Action reset
<<<<<<< HEAD
pwsh ./database/Invoke-BatchDb.ps1 -Action all
pwsh ./database/Invoke-BatchDb.ps1 -Action validate
=======
>>>>>>> A-Foundation
```

## Notes

<<<<<<< HEAD
- Scripts are intended to be re-runnable where possible.
- `flush` is destructive for the local seeded ScheduledLoadFromFps footprint in `fps` and `mabarchive` and should only be used in local/dev environments.
- `reset` now performs `flush` + `apply` + `seed` so the database returns to a known seeded baseline.
- `validate` is non-destructive and fails fast if required ScheduledLoadFromFps tables or key constraints are missing.
- Keep business-specific seed data in dedicated files under `sql/seeds` and version them by prefixing with sequence numbers.
- Worker execution persistence writes to `fps.job_queue` and `fps.job_queue_log`.
- Runtime configuration follows the repo pattern: `ConnectionStrings:BatchJobsConnectionString`.

## Local Reset Script

For a shell-based local reset workflow, run:

```bash
bash database/sql/reset_scheduled_load_locally.sh
```

This performs the same sequence as the PowerShell `reset` action:
1. flush seeded tables
2. apply migrations
3. reseed baseline data

It is intended for local/dev use only.

## Sink Pipeline

See `sink/README.md` for the VM workflow to:

1. Ingest a cloud snapshot into sink raw structures.
2. Register and reuse a `snapshot_id` for all pipeline stages.
3. Curate only required fields for foundation revisions.
4. Sync curated results into `fps` in an idempotent way.
=======
- Use apply for non-destructive schema update.
- Use reset only when you intend to clear and recreate local state.
>>>>>>> A-Foundation
