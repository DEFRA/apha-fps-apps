# MABArchive 100 Percent Logic Integrity Plan

## Purpose

This document is the execution control for strict SQL-to-.NET parity of the scheduled FPS load flow.

Goal:
- implement original legacy behavior first,
- validate parity with evidence,
- avoid silent enhancements.

## Legacy Baseline Source

Primary baseline:
- docs/SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md
- docs/ScheduledJobs.txt

## Non-Negotiable Parity Invariants

1. Previous year cycle is always attempted first.
2. Full current-year cycle runs only when month > 4.
3. Before May, current year only refreshes MY_tlkpProject_all.
4. FPSYearTotals source rebuild must mirror legacy formulas and null behavior.
5. sp_deleteFPSTotals behavior is full table delete (no year filter).
6. sp_DeleteYearsFPSData is year-specific broad archive wipe plus G_tlkpProject project-based delete.
7. sp_AddYearsFPSData is broad fan-out across all legacy sub-loaders.
8. Database existence check is required before each year cycle.
9. Missing FPS year database means skip that year (not fatal by default).
10. Preserve insert-only semantics unless an enhancement is explicitly approved.

## Current .NET Drift Summary (Confirmed)

### D1 Orchestration drift
- Current implementation processes one primary year and optional partial refresh, not explicit previous-year-then-current-year branching.

### D2 Missing FPS database existence guard
- Current implementation has no explicit per-year database existence checks equivalent to master.dbo.sysdatabases.

### D3 Totals delete behavior drift
- Current implementation deletes fps.fpsyeartotals by year, while legacy sp_deleteFPSTotals does full table delete.

### D4 Totals formula drift
- Current totals rebuild contains placeholder logic and does not implement legacy joins/formulas/null handling.

### D5 Delete scope drift
- Current year-delete covers only four tables, while legacy scope spans broad MY_* set plus tlkpYear and G_tlkpProject project-based delete behavior.

### D6 Add-years fan-out drift
- Current add-years load only handles my_fpsyeartotals and my_tlkpproject; legacy flow calls many additional loaders.

### D7 Upsert drift
- Current logic uses ON CONFLICT updates in places where legacy procedures are insert-only (after explicit deletes).

### D8 Data default drift
- Current load applies COALESCE projectstatus default to Active, which is not legacy behavior.

### D9 Verification gap
- No focused unit/integration parity tests found for MABArchive branch behavior, formulas, and table coverage.

## Update From Attached Schema Reference

Source:
- database/sink/schema-reference/latest-cloud-schema-columns.csv

### Confirmed from schema (resolved)

1. Year model in cloud PostgreSQL is shared-schema + year column semantics.
- FPS source tables use `fpsyear` (for example `fps.tlkpproject`, `fps.fpsyeartotals`, `fps.monthlyoutput`, `fps.monthlytime`, `fps.projectmonthfinal`).
- MABArchive target tables use `year` (for example `mabarchive.my_fpsyeartotals`, `mabarchive.my_tlkpproject_all`, `mabarchive.my_monthlyoutput`, `mabarchive.tlkpyear`).

2. Legacy totals dependency objects exist in PostgreSQL `fps` schema.
- `fps.qrytotaladditionalcosts(jobcode, fpsyear, totaladditionalcosts)`
- `fps.qrytotalanimalcosts(jobcode, fpsyear, totalanimalcosts)`
- `fps.qrytotalstaffcosts(jobcode, fpsyear, totalstaffcosts, totalpaycosts)`
- `fps.qrytotaltestcosts(jobcode, totaltestcosts)`

3. Legacy MABArchive yearly fan-out targets are present.
- Confirmed core targets include:
  `mabarchive.my_fpsyeartotals`,
  `mabarchive.my_monthlyoutput`,
  `mabarchive.my_monthlytime`,
  `mabarchive.my_proj_invoice`,
  `mabarchive.my_proj_subcontract`,
  `mabarchive.my_projectmonthfinal`,
  `mabarchive.my_tbladditionalcosts`,
  `mabarchive.my_tblanimalreq`,
  `mabarchive.my_tblcontract`,
  `mabarchive.my_tblstaffjob`,
  `mabarchive.my_timecostcalcs`,
  `mabarchive.my_tlkptestreqmt`,
  `mabarchive.my_tlkpproject`,
  `mabarchive.my_tlkpprogram`,
  `mabarchive.my_profitcentregrade`,
  `mabarchive.my_workgroupgrade`,
  `mabarchive.my_tblprofitcentre`,
  `mabarchive.my_testorproduct`,
  `mabarchive.my_staff`,
  `mabarchive.my_workgroup`,
  `mabarchive.my_tblanimals`,
  `mabarchive.my_tlkpproject_all`,
  `mabarchive.g_tlkpproject`,
  `mabarchive.tlkpyear`.

4. `G_tlkpProject` mapping is now concrete.
- Table exists as `mabarchive.g_tlkpproject`.
- Columns match legacy shape and have no year column: `parentproject`, `projecttitle`, `costbookno`, `disease`, `contract`, `shorttitle`, `projectstatus`.

5. `tlkpYear` mapping is now concrete.
- Destination exists as `mabarchive.tlkpyear(year, latestmonthreleased)`.
- Source metadata table exists as `fps.tbldb_variables(db_var_name, db_var_value)`.

## Update From Migration Overview (User-Provided)

Key confirmed context:

1. Consolidation model is explicit.
- Source SQL Server yearly DBs (`FPS2025`, `MAB_Archive`) are consolidated into one PostgreSQL database (`FPS`) with schemas `fps` and `mabarchive`.

2. `fpsyear` is the primary multiyear dimension by design.
- Added broadly to core FPS tables and views.
- Composite PK/FK strategy is intentional modernization, not accidental drift.

3. Business logic location is explicit.
- Stored procedures/functions/triggers are intentionally not migrated.
- Parity behavior must be implemented in application tier.

4. Year master governance exists.
- `fps.tblyearmaster` is the registered fiscal-year authority.
- This supports a stronger replacement for legacy database-exists checks.

5. Platform-level approved modernizations are now known.
- `citext` adoption where needed for case-insensitive matching.
- Identity-to-sequence migration.
- Composite PK migration for multiyear data.

Interpretation for parity work:
- These are approved platform/schema modernizations.
- They do not by themselves approve behavioral drift in orchestration, formulas, delete scope, or load scope.

## Remaining Inputs Pending (still needed)

1. Runtime equivalent of legacy database existence check.
- Preferred candidate is now `fps.tblyearmaster` (registered year), with optional data-presence check.
- Need confirmation whether guard is:
  a) year registered only (`tblyearmaster`), or
  b) year registered + data present (for example `tlkpproject`).

2. Final join key and year predicate for `qrytotaltestcosts`.
- Table has `jobcode` and `totaltestcosts` but no `fpsyear` column in schema export.
- Migration notes say `fpsyear` was broadly added to views, including `qrytotaltestcosts`, which conflicts with current CSV export.
- Need schema-of-record confirmation for this object.

3. Constraint policy for strict insert-only parity.
- Current .NET implementation uses upserts.
- We need approval on whether to remove upserts fully for parity or keep as approved enhancement.

4. Approved deviations list (if any).
- If any behavior differs intentionally from legacy SQL, list each approved deviation.

5. Value-level data check for `db_var_name = 'month'`.
- Schema confirms table/columns but not row values.
- Need one validation sample (or permission for us to query non-prod) to verify this row exists consistently.

## Provisional Assumptions (Best-Effort, Time-Boxed)

These assumptions are adopted to unblock implementation now and must be validated in parity testing.

### A1 Runtime replacement for legacy FPSYYYY existence check

Assumption:
- A year is considered "available" when the year exists in `fps.tblyearmaster`.

Operational behavior (provisional):
- Before each yearly cycle, run year-presence guard:
  `exists(select 1 from fps.tblyearmaster where fpsyear = :year)`
- If false, skip that year's cycle and log as non-fatal.
- Optional secondary observability check: log warning if no rows exist in `fps.tlkpproject` for that year.

Reasoning:
- Legacy behavior is "skip if database missing".
- In consolidated schema model, registered fiscal year in `tblyearmaster` is the closest semantic equivalent.

Risk:
- A year can be registered but incompletely loaded.

Validation follow-up:
- Compare registered years vs key source-table population for 2-3 sample years.

### A2 Year-safe behavior for `fps.qrytotaltestcosts`

Assumption:
- Until schema-of-record is confirmed, preserve legacy-style join on `jobcode` exactly and treat year predicate as feature-flagged option.

Operational behavior (provisional):
- Use LEFT JOIN from `fps.tlkpproject.parentproject` to `fps.qrytotaltestcosts.jobcode`.
- Keep `SELECT DISTINCT` parity to prevent duplicate insert rows.

Reasoning:
- Legacy SQL joins by jobcode only for this source.
- Current inputs conflict: migration notes suggest `fpsyear` exists, CSV export does not.

Risk:
- If `jobcode` is not globally unique across years, cross-year contamination is possible.

Validation follow-up:
- Confirm DDL for `fps.qrytotaltestcosts`; if `fpsyear` exists, add year-safe join and re-run parity checks.

### A3 Strict insert-only parity versus upsert

Assumption:
- Strict parity mode is default: remove `ON CONFLICT` upserts in MABArchive load paths.

Operational behavior (provisional):
- Keep delete-then-insert sequence as primary idempotency mechanism.
- Treat upsert as enhancement, not baseline parity.

Reasoning:
- Legacy procedures are insert-only after explicit delete steps.

Risk:
- If delete scope is incomplete, insert-only may surface duplicate-key failures.

Validation follow-up:
- Ensure full delete coverage is implemented before fan-out load and run constraint-focused tests.

### A4 Approved deviations register (current state)

Assumption:
- No behavioral deviations are currently approved.

Documented approved modernizations (non-behavioral baseline context):
- SQL Server yearly DBs consolidated into single PostgreSQL database with `fps` and `mabarchive` schemas.
- `fpsyear` multiyear schema dimension and composite PK/FK treatment.
- Identity-to-sequence migration.
- `citext` usage for case-insensitive matching.

Operational behavior (provisional):
- Any behavior difference from legacy is treated as drift until explicitly approved and recorded.

Change-control rule:
- If a deviation is approved later, add it to this register with owner/date/rationale.

### A5 `tbldb_variables` month value semantics

Assumption:
- `fps.tbldb_variables` contains at least one row where `db_var_name = 'month'`.
- Expected normal state is one row, but parity implementation must preserve SQL behavior if multiple rows exist.

Operational behavior (provisional):
- For year-details load, insert all rows returned by:
  `where db_var_name = 'month'`
- Do not force single-row selection in parity mode.

Reasoning:
- This mirrors legacy `INSERT ... SELECT` semantics.

Risk:
- Zero rows results in no `mabarchive.tlkpyear` insert for that year.

Validation follow-up:
- Add explicit parity assertion on inserted row count for year-details step.

## Assumptions To Confirm Later

- Confirm A1 with production-like year availability pattern.
- Confirm A2 by reconciling DDL for `fps.qrytotaltestcosts` between migration docs and schema export.
- Confirm A3 or explicitly approve upsert as modernization.
- Confirm A5 by sampling `db_var_name = 'month'` data in non-prod.

## Small Verifiable Task Sequence

### Task 1 Freeze invariants and gap matrix
Output:
- This document completed and reviewed.
Verify:
- Every baseline invariant is represented.
- Every known drift is documented.

### Task 2 Confirm missing inputs
Output:
- User responses for all blocking inputs above.
Verify:
- No unresolved unknowns for formula sources, table mappings, or branch semantics.

### Task 3 Implement orchestration parity
Output:
- Explicit previous-year cycle first.
- Conditional current-year full cycle when month > 4.
- Pre-May current-year MY_tlkpProject_all-only refresh.
- Per-year existence checks.
Verify:
- Tests for month <= 4 and month > 4 branch behavior and execution order.

### Task 4 Implement exact totals rebuild parity
Output:
- Full SQL-equivalent logic for create totals, including LEFT JOIN, CASE null handling, TotalCosts formula, TotalIncome null propagation, RequiredProfit mapping, SELECT DISTINCT semantics.
Verify:
- Deterministic fixture tests for null behavior and duplicate suppression.

### Task 5 Implement delete-years parity scope
Output:
- Full year-specific delete coverage across mapped archive tables, including G_tlkpProject project-based behavior and tlkpYear handling.
Verify:
- Per-table row count assertions before/after delete.

### Task 6 Implement add-years full fan-out
Output:
- End-to-end yearly load parity equivalent to legacy sp_AddYearsFPSData procedure chain.
Verify:
- Each mapped loader invoked and row counts captured.

### Task 7 Remove unintended enhancements
Output:
- Remove or explicitly flag upsert/defaulting behavior that changes legacy semantics.
Verify:
- SQL statements align with parity contract unless explicitly approved as enhancement.

### Task 8 Add parity evidence suite
Output:
- Targeted tests for formulas, branch orchestration, delete scope, loader coverage, and side effects.
Verify:
- Tests fail on drift and pass on parity.

### Task 9 Run build and tests and publish evidence
Output:
- Build/test results and parity checklist with pass/fail per invariant.
Verify:
- No failing targeted parity tests.

## Execution Rule

Do not mark strict parity complete until all invariants are validated by executable tests and evidence logs.
