# BatchJobs Database

Minimal reference for local schema setup and maintenance.

## Purpose

- Holds SQL for foundation and runtime orchestration tables.
- Supports local Docker and local PostgreSQL test flows.

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
```

## Notes

- Use apply for non-destructive schema update.
- Use reset only when you intend to clear and recreate local state.
