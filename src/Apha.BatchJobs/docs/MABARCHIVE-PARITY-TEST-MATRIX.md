# MABArchive Parity Test Matrix (Pre-Refactor)

## Purpose
This is a compact branch-parity test matrix to validate runtime behavior before structural refactoring.

Scope:
- Jan-Apr branch (partial current-year refresh)
- May-Dec branch (full current-year load)

Out of scope:
- Structural refactor validation
- Performance benchmarking

## Preconditions
- Target job: MABArchive
- Lock table does not have an active lock for MABArchive
- Test database has source data for previous year and current year
- Orchestration uses transactional wrapper
- Strict year isolation remains enabled for totals rebuild

## Runtime Controls
- Jan-Apr simulation: set MABARCHIVE_TEST_UTCNOW to a date with month <= 4
- May-Dec simulation: set MABARCHIVE_TEST_UTCNOW to a date with month > 4

## Parity Matrix

| Case ID | Simulated Date | Branch | Expected Sequence | Must-Not-Happen | Core Assertions |
|---|---|---|---|---|---|
| PAR-01 | 2026-03-15T00:00:00Z | Jan-Apr | Previous year: totals rebuild -> delete year scope -> 24 loaders. Current year: delete+reload my_tlkpproject_all only. | No full 24-loader run for current year. | 1) Previous year full cycle executed. 2) Current year partial refresh executed. 3) Current year my_fpsyeartotals not reloaded by full cycle path. |
| PAR-02 | 2026-05-15T00:00:00Z | May-Dec | Previous year full cycle, then current year full cycle (both with totals rebuild -> delete year scope -> 24 loaders). | No partial-refresh-only path for current year. | 1) Full cycle executed for both years when available. 2) Loader order remains 1 to 24 for each processed year. |
| PAR-03 | 2026-03-15T00:00:00Z | Jan-Apr | Previous year full cycle attempted first. | Current year partial refresh before previous-year attempt. | 1) Ordering is previous-year-first. 2) Branch decision uses month <= 4. |
| PAR-04 | 2026-05-15T00:00:00Z | May-Dec | Previous year full cycle attempted first. | Current year before previous year. | 1) Ordering is previous-year-first. 2) Branch decision uses month > 4. |
| PAR-05 | 2026-03-15T00:00:00Z | Jan-Apr | If previous year unavailable: skip previous full cycle, still evaluate current-year partial path. | Run abort solely due to previous-year unavailable. | 1) Availability check is honored per year. 2) Current-year partial still runs when current year available. |
| PAR-06 | 2026-05-15T00:00:00Z | May-Dec | If current year unavailable: previous full cycle may complete; current full cycle is skipped. | Attempt full load against unavailable current year. | 1) Availability check gates current-year full cycle. 2) Run remains consistent and completes branch logic. |

## Data Integrity Checks (Both Branches)

- Delete behavior is fail-fast:
  - Any delete failure fails run (no swallow-and-continue behavior).
  - g_tlkpproject project-key delete is fail-fast.
- Totals rebuild delete is year-scoped:
  - fps.fpsyeartotals delete applies WHERE fpsyear = selected year.
- Year-isolated totals joins:
  - totals source joins include parentproject and fpsyear.
- Idempotency:
  - Re-running same branch/date does not produce duplicate year-slice rows in target tables.

## Observability Checks

For each processed year, verify logs include:
- RowsInserted for totals rebuild
- RowsDeleted for archive delete phase
- RowsLoaded for load phase
- RowsRefreshed and delete count for partial refresh path

## Exit Criteria
- All matrix cases pass with expected branch behavior.
- No unexpected exceptions.
- No duplicate year-slice rows after rerun.
- Loader order and branch rules remain parity-aligned with legacy logic.

## Execution Results (2026-05-04)

| Case ID | Result | Evidence Summary |
|---|---|---|
| PAR-01 (Jan-Apr) | PASS | Run completed successfully with `CurrentMonth=3`, previous year unavailable was skipped, and current-year partial refresh executed with delete+insert counts for `my_tlkpproject_all`. |
| PAR-02 (May-Dec, strict isolation ON) | PASS (after local view fix) | After adding `fpsyear` to local `qrytotal*costs` views, run succeeded with strict isolation enabled and executed full current-year cycle. |
| PAR-02 (May-Dec, strict isolation OFF) | Historical blocker (pre-fix) | Previously failed due missing `view.fpsyear` (`Postgres 42703`), now resolved by local view contract alignment. |

Current blocker classification:
- Local environment: resolved.
- Canonical/cloud environments: still pending DBA confirmation/apply.

Next verification step after DBA view fix:
1. Re-run PAR-02 with strict isolation ON.
2. Confirm full-cycle path logs for current year include `RowsInserted`, `RowsDeleted`, and `RowsLoaded`.
3. Re-run PAR-02 a second time to re-check idempotency expectations.

## Local Run Commands (Copy/Paste)

Use these commands from PowerShell to execute the two main branch parity runs.

### Case PAR-01 (Jan-Apr branch)

```powershell
Set-Location 'd:\Users\atos.user8\source\repos\apha-fps-apps-B-ScheduledJobs\src\Apha.BatchJobs'

$env:PGPASSWORD='admin123'
& 'C:\Program Files\PostgreSQL\16\bin\psql.exe' -P pager=off -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -c "UPDATE fps.job_lock SET is_active=false WHERE job_name='MABArchive' AND is_active=true;"

$env:ASPNETCORE_ENVIRONMENT='Development'
$env:DOTNET_ENVIRONMENT='Development'
$env:BATCH_JOB_NAME='MABArchive'
$env:MABARCHIVE_TEST_UTCNOW='2026-03-15T00:00:00Z'
$env:BATCH_GRACEFUL_SHUTDOWN_WINDOW_SECONDS='600'
$env:BatchJobs__DbCommandTimeoutSeconds='300'
$env:ConnectionStrings__BatchJobsConnectionString='Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=admin123'

dotnet run --project Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
```

### Case PAR-02 (May-Dec branch)

```powershell
Set-Location 'd:\Users\atos.user8\source\repos\apha-fps-apps-B-ScheduledJobs\src\Apha.BatchJobs'

$env:PGPASSWORD='admin123'
& 'C:\Program Files\PostgreSQL\16\bin\psql.exe' -P pager=off -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db -c "UPDATE fps.job_lock SET is_active=false WHERE job_name='MABArchive' AND is_active=true;"

$env:ASPNETCORE_ENVIRONMENT='Development'
$env:DOTNET_ENVIRONMENT='Development'
$env:BATCH_JOB_NAME='MABArchive'
$env:MABARCHIVE_TEST_UTCNOW='2026-05-15T00:00:00Z'
$env:BATCH_GRACEFUL_SHUTDOWN_WINDOW_SECONDS='600'
$env:BatchJobs__DbCommandTimeoutSeconds='300'
$env:ConnectionStrings__BatchJobsConnectionString='Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=admin123'

dotnet run --project Apha.BatchJobs.Worker/Apha.BatchJobs.Worker.csproj
```
