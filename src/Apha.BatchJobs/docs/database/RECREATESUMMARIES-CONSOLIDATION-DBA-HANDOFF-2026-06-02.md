# RecreateSummaries Consolidation DBA Handoff (2026-06-02)

## Purpose
This note documents the validated database risks found during PostgreSQL RecreateSummaries parity/testing after multi-year consolidation.

The implementation team has already completed all application-side changes currently in our control.
Remaining items below require DBA-led schema/data remediation to avoid production failures.

## Current Status
- Application flow and step order remain aligned to the RecreateSummaries baseline.
- Year-aware joins are in place to support a single consolidated multi-year database.
- Focused PostgreSQL test slice improved and is mostly stable.
- Remaining failures are caused by data/schema integrity issues in the consolidated DB clone.

## Validated Findings

### 1) Null fpsyear values in projectmonth
- Check run:
  - `SELECT COUNT(*) FROM fps.projectmonth WHERE fpsyear IS NULL;`
- Result:
  - `1344`
- Impact:
  - Consolidated year-aware processing cannot safely partition these rows.
  - If silently included, cross-year aggregation can mix data.
  - If blocked (current fail-fast), job/test execution stops to prevent wrong totals.

### 2) Ambiguous year assignment for null-year rows
- Profile check run:
  - Distinct null-year projects joined to tlkpproject year counts.
- Result summary:
  - `projects_missing_in_tlkpproject = 0`
  - `projects_with_single_year = 8`
  - `projects_with_multi_year = 104`
- Impact:
  - Most null-year projects map to multiple years, so simple default update is unsafe.
  - Requires explicit backfill rule (replicate per year or business mapping).

### 3) Key design mismatch for consolidated model
- Checks run:
  - `projectmonth` PK = `(project, monthno)`
  - `projectmonth2` PK = `(project, monthno)`
  - `projectmonth3` PK = `(endperiod, project)`
  - `projectmonthfinal` PK = `(fpsyear, project, monthno)`
- Impact:
  - Upstream month tables are not consistently year-keyed while downstream final table is.
  - This permits ambiguous source rows and increases risk of cross-year merge errors.

### 4) Program width overflow case (mostly test residue)
- Check run:
  - `SELECT COUNT(*) FROM fps.tlkpproject WHERE length(program::text) > 10;`
- Result:
  - `1`
- Observed row:
  - `UT7377_P1 | UT7377_PRG1 | 2026`
- Impact:
  - Causes insert failure when target columns enforce varchar(10).
  - This appears to be leftover UT data, but same pattern in production data would fail similarly.

## What Engineering Has Already Done
- Removed non-baseline program truncation in runtime logic (no silent mutation).
- Added explicit fail-fast for null `projectmonth.fpsyear` to prevent silent cross-year data corruption.
- Hardened tests to use schema-safe values and transactional isolation where feasible.
- Preserved only the intended consolidation change: explicit year partitioning.

## DBA Action Plan (Required Before Production)

### Priority A: Normalize year integrity in projectmonth family
1. Define approved business rule for null-year rows:
   - Option A: replicate each null-year `(project, monthno)` row for every valid project year.
   - Option B: map each row to exactly one canonical year via business-owned mapping.
2. Backfill `projectmonth.fpsyear` using that approved rule.
3. Set `projectmonth.fpsyear` to `NOT NULL`.

### Priority B: Align keys with consolidated year model
1. Update primary/unique keys to include `fpsyear` where applicable:
   - `projectmonth`
   - `projectmonth2`
   - `projectmonth3`
2. Rebuild dependent indexes and validate affected views/procedures.
3. Confirm application mappings match final DBA-approved key definitions.

### Priority C: Clean invalid data patterns
1. Remove UT residue rows in shared integration/prod-like DB clones.
2. Enforce domain/length validation at ingestion points for code fields (such as program).

## Recommended Validation Queries (Post-DBA Fix)

```sql
-- 1) Null year must be zero
SELECT COUNT(*) AS null_projectmonth_fpsyear
FROM fps.projectmonth
WHERE fpsyear IS NULL;

-- 2) Upstream keys should include fpsyear (verify definitions)
SELECT tc.table_name, tc.constraint_name,
       string_agg(kcu.column_name, ',' ORDER BY kcu.ordinal_position) AS key_cols
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
  ON tc.constraint_name = kcu.constraint_name
 AND tc.table_schema = kcu.table_schema
WHERE tc.table_schema = 'fps'
  AND tc.table_name IN ('projectmonth', 'projectmonth2', 'projectmonth3', 'projectmonthfinal')
  AND tc.constraint_type = 'PRIMARY KEY'
GROUP BY tc.table_name, tc.constraint_name
ORDER BY tc.table_name;

-- 3) Program width violations must be zero
SELECT COUNT(*) AS over_len_program
FROM fps.tlkpproject
WHERE length(program::text) > 10;
```

## Release Gate Recommendation
Do not release RecreateSummaries to production on consolidated PostgreSQL until:
1. Null year backfill is complete and validated.
2. Key alignment is completed and validated.
3. Regression run is green on production-like clone after DBA changes.

## Owner Split
- Engineering owner:
  - application behavior, mapping, unit/integration tests
- DBA owner:
  - schema constraints, key redesign, data backfill, data quality guardrails

## Notes
Date captured: 2026-06-02
Scope: batch_jobs_foundation_db (local cloned consolidation environment)

## Local Clone Remediation Executed (Engineering)

The following local clone remediation was executed to unblock validation and verify the implementation path:

1. Removed UT test residue rows (`UT%`) across RecreateSummaries-related tables.
2. Backfilled `fps.projectmonth.fpsyear` for NULL rows:
   - Single-year projects: assigned the single known year.
   - Multi-year projects: local operational fallback assigned `MAX(fpsyear)`.
3. Enforced local integrity guardrail:
   - `ALTER TABLE fps.projectmonth ALTER COLUMN fpsyear SET NOT NULL`.

### Before / After (local clone)
- Before:
  - `projectmonth_null_fpsyear = 1344`
  - `over_len_program = 1`
  - `ut_projects_remaining = 2`
  - `projectmonth.fpsyear is_nullable = YES`
- After:
  - `projectmonth_null_fpsyear = 0`
  - `over_len_program = 0`
  - `ut_projects_remaining = 0`
  - `projectmonth.fpsyear is_nullable = NO`

### SQL Scripts Added For DBA Consultation

- Local hotfix script (executed on clone):
  - [src/Apha.BatchJobs/docs/database/sql/104_recreatesummaries_local_clone_hotfix.sql](src/Apha.BatchJobs/docs/database/sql/104_recreatesummaries_local_clone_hotfix.sql)
- Post-fix validator script:
  - [src/Apha.BatchJobs/docs/database/sql/105_validate_recreatesummaries_local_clone_hotfix.sql](src/Apha.BatchJobs/docs/database/sql/105_validate_recreatesummaries_local_clone_hotfix.sql)

### Important Production Caveat

The `MAX(fpsyear)` assignment for ambiguous multi-year projects is a local operational fallback for testability.
For production, DBA/business must approve the final backfill strategy and key model changes listed in this handoff.

## DBA Addendum: MABArchive Production Readiness (2026-06-04)

This addendum records MABArchive-specific production prerequisites identified during isolation runs.

### DBA-required DB rollout

1. Deploy `fps.qrytotaltestcosts` with `fpsyear` included in projection and grouping.
2. Use controlled migration script:
  - [src/Apha.BatchJobs/docs/database/sql/2026-06-03-qrytotaltestcosts-add-fpsyear.sql](src/Apha.BatchJobs/docs/database/sql/2026-06-03-qrytotaltestcosts-add-fpsyear.sql)

### DBA operational runbook check (before reruns after abnormal stop)

```sql
-- Check active lock state for MABArchive
SELECT job_name, jobqueueid, acquired_at, expires_at, is_active
FROM fps.job_lock
WHERE job_name = 'MABArchive'
ORDER BY acquired_at DESC;

-- Cleanup only when confirmed stale/orphaned
DELETE FROM fps.job_lock
WHERE job_name = 'MABArchive';
```

### Application-only fixes already implemented (no new DB migration)

1. EF key mapping for MAB animal requirement flow aligned to row identity used by data load path.
2. Contract date values normalized to UTC-kind before writes to avoid `timestamp with time zone` provider write failures.

### Production verification recommendation

1. Confirm target contract date columns remain `timestamp with time zone`.
2. Confirm source contract dates are compatible with UTC-normalized writes.
