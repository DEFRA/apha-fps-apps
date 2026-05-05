# Database Migration: Operational → FPS Schema

**Date Completed:** 2026-04-30  
**Database:** batch_jobs_foundation_db  
**Migration Type:** Schema consolidation with name normalization

## Migration Summary

Successfully migrated runtime orchestration tables from `operational` legacy schema to `fps` modern schema with full name normalization and backward-compatibility support.

### What Moved

| Legacy Table (operational) | Modern Table (fps) | Rows Migrated |
|---------------------------|-------------------|---------------|
| batch_lock | job_lock | 0 |
| tbljobmaster | job_master | 3 |
| tbljobstatus | job_status | 6 |
| tbljobqueue | job_queue | 13 |
| tbljobqueue_log | job_queue_log | 25 |

**Total Data Rows:** 47

### Backward Compatibility

All legacy tables in `operational` schema have been:
1. Renamed to `*_legacy` format for archival
2. Replaced with views pointing to fps tables for transparent access

This means existing code referencing `operational.batch_lock`, `operational.tbljobmaster`, etc. will continue to work without modification.

### Database Objects Created

**Fps Schema**
- 5 normalized tables with modern naming
- 5 primary key constraints
- 8 foreign key constraints (with proper CASCADE/RESTRICT rules)
- 3 check constraints (timetolive > 0, end after start, etc.)
- 3 unique constraints (jobname, jobid+status)
- 7 performance indexes

**Operational Schema**
- 5 backward-compatibility views (transparent redirect to fps)
- 5 legacy tables renamed to `*_legacy` (can be dropped after validation period)

### Constraints Preserved

All constraints successfully recreated with identical semantics:

**Job Master**
- PK: jobid
- UK: jobname
- CK: timetolive > 0

**Job Status**
- PK: statusid
- FK: jobid → job_master (ON DELETE CASCADE)
- UK: (jobid, status)

**Job Queue**
- PK: jobqueueid (UUID)
- FK: jobid → job_master (ON DELETE RESTRICT)
- FK: statusid → job_status (ON DELETE RESTRICT)
- CK: enddatetime IS NULL OR enddatetime >= startdatetime

**Job Queue Log**
- PK: jobqueuelogid
- FK: jobqueueid → job_queue (ON DELETE CASCADE)
- FK: statusid → job_status (ON DELETE RESTRICT)

**Job Lock**
- PK: lock_id

### Data Integrity Validation Results

✅ All foreign key references valid (0 orphaned records)  
✅ All check constraints valid (0 violations)  
✅ All unique constraints valid (0 duplicates)  
✅ Schema consistency verified across all 5 tables  
✅ Row count parity confirmed (operational ↔ fps)

### Files

**Migration Scripts:**
- `100_migrate_operational_to_fps_runtime_tables.sql` — Main migration (14 steps)
- `101_rollback_operational_to_fps_migration.sql` — Rollback procedure
- `102_validate_operational_to_fps_migration.sql` — Validation script

**This Document:**
- `MIGRATION_OPERATIONAL_TO_FPS_COMPLETION.md`

### Next Steps

1. **Code Migration** — Update EF Core context and connection strings to use fps schema
2. **Testing** — Run batch job execution tests to validate lock/queue interactions
3. **Legacy Cleanup** (optional after 30-day observation period):
   - Drop views in operational schema
   - Drop `*_legacy` tables
   - Verify no dependencies remain on operational schema

### Rollback Procedure

If immediate rollback needed:

```bash
psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db \
  -f database/sql/101_rollback_operational_to_fps_migration.sql
```

This will drop fps schema and all its tables/views, leaving operational legacy tables intact for data recovery.

### Notes

- No data was lost during migration
- All constraints and indexes recreated with proper semantics
- Migration was transactional (ACID compliant)
- Views provide transparent backward compatibility
- Legacy tables available as `operational.*_legacy` for reference
- EF Core now correctly aligns with database schema (fps schema expected by Entity Framework)
