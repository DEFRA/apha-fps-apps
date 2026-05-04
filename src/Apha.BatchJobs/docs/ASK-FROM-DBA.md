# Ask from DBA

## Purpose
This document is the consolidated DBA handoff for promoting current MABArchive runtime assumptions to canonical cloud database definitions.

## What Engineering Changed (App Side)
1. MABArchive totals rebuild is now year-isolated.
2. Runtime enforces strict guard: all `fps.qrytotal*costs` views must expose `fpsyear`.
3. Runtime now joins totals sources by `(parentproject, fpsyear)` and performs year-scoped totals delete.

Because of this, canonical DB objects must match these contracts.

## Design-Spec Mismatch Observed Locally
The approved target DB design says year-bearing FPS tables should use composite primary keys of the form `(fpsyear, natural_key)` and year-aligned foreign keys.

Local verification against the current PostgreSQL snapshot shows that this is not yet true for the MABArchive source tables we inspected. Examples:
- `fps.tlkpprogram` still has PK `(programno)` instead of `(fpsyear, programno)`.
- `fps.tlkpproject` still has PK `(parentproject)` instead of `(fpsyear, parentproject)`.
- `fps.fpsyeartotals` still has PK `(parentproject)` instead of `(fpsyear, parentproject)`.
- `fps.monthlyoutput` still has PK `(testcode, buyer, month, workgroup)` instead of `(fpsyear, testcode, buyer, month, workgroup)`.
- `fps.monthlytime` still has PK `(pactstaffid, timecode, month, parentproject)` instead of `(fpsyear, pactstaffid, timecode, month, parentproject)`.

This mismatch blocks safe multi-year source data loading because a 2025 row with the same natural key as a 2026 row collides in the same table.

## DBA Actions Required (Blocking)

### 0) CRITICAL: Apply composite PK/FK model for multiyear FPS tables (FULL SCOPE)

**⚠️ BLOCKING ISSUE**: Local DB cannot be safely modernized to composite-key model due to cascading dependencies. This work MUST be done by DBA on the canonical cloud DB.

**Why this is critical**:
- Engineering verified that applying composite PKs locally requires rewriting FKs across **27+ dependent relationships** spanning tables beyond the 21 core FPS source tables
- Incomplete FK rewrites risk data corruption and silent constraint violations
- This is architectural DB work, not a local hotfix

**Full scope of composite-PK redesign** (not limited to MABArchive source tables):

*Primary year-bearing tables (requiring composite PKs):*
- `fps.tlkpprogram`, `fps.tlkpproject`, `fps.fpsyeartotals`, `fps.monthlyoutput`, `fps.monthlytime`
- `fps.proj_invoice`, `fps.proj_subcontract`, `fps.projectmonthfinal`, `fps.tbladditionalcosts`, `fps.tblanimalreq`
- `fps.tblcontract`, `fps.tblstaffjob`, `fps.timecostcalcs`, `fps.tlkptestreqmt`, `fps.workgroupgrade`
- `fps.profitcentregrade`, `fps.testorproduct`, `fps.tblwgemployee`, `fps.tblemployee`, `fps.workgroup`, `fps.tblanimals`

*Dependent tables with cascading FK rewrites required (27 relationships)*:
- `fps.tlkptestcapability` → references `fps.testorproduct`, `fps.tlkptestreqmt`, `fps.workgroup`
- `fps.tlkpjobcode` → references `fps.tlkpproject`
- `fps.milestone` → references `fps.tlkpproject`
- `fps.timecodevalid` → references `fps.tlkpproject`
- `fps.tbltestrccost` → references `fps.testorproduct`
- `fps.tbltestrequirementrccost` → references `fps.tlkptestreqmt`
- `fps.tblpaymentschedule` → references `fps.tblcontract`
- `fps.plancatwggrade` → references `fps.workgroupgrade`
- `fps.tblbid` → references `fps.workgroup`
- Plus internal dependencies within the 21-table core group (e.g., `fps.workgroupgrade` → `fps.tlkptestreqmt`)

**Required actions** (DBA on canonical cloud DB):
1. For each of the 21 primary year-bearing tables:
   - Drop existing single-column PKs
   - Create new composite PKs: `(fpsyear, natural_key)`
   - Example: `ALTER TABLE fps.tlkpproject DROP CONSTRAINT ...; ALTER TABLE fps.tlkpproject ADD PRIMARY KEY (fpsyear, parentproject);`

2. For each of the 27+ dependent FKs:
   - Verify if the dependent table also has `fpsyear` column
   - If yes: Rewrite FK to include `fpsyear` on both sides
   - If no: Assess whether `fpsyear` should be added or if table is out-of-scope for multiyear model
   - Example: `ALTER TABLE fps.tlkptestcapability ADD CONSTRAINT fk_tlkptestcapability_testcode FOREIGN KEY (fpsyear, testcode) REFERENCES fps.testorproduct (fpsyear, testcode);`

3. Confirm views that join year-bearing tables propagate `fpsyear` consistently

**Temporary local workaround** (valid until canonical DB is updated):
- Engineering created a rerunnable seed script (`src/Apha.BatchJobs/database/sql/200_insert_test_scenario_data.sql`) that clones 2026 FPS data into non-colliding 2025 keys using suffix naming (e.g., `P100-BASIC` → `P100-BASIC_25`)
- This allows parity testing of MABArchive Jan–Apr vs. May–Dec branch logic with two years of data **without** modifying the local schema
- The workaround is valid only for local testing; canonical DB must implement the proper composite-key design

**Why blocking**:
- Local sample-data loading for both 2025 and 2026 cannot use real business keys if PKs exclude `fpsyear`
- FK definitions that still point to single-column parent keys prevent safe conversion of source data to the intended multiyear model
- Cascading scope makes local implementation too risky; DBA must own this change on canonical DB

**Confirm and apply composite PK/FK model for multiyear FPS tables**:
Please confirm the canonical cloud DB matches the approved multiyear design:
- year-bearing tables use composite PKs of the form `(fpsyear, natural_key)`
- year-bearing child tables use FKs that include `fpsyear` on both sides of the relationship
- views that join year-bearing tables propagate `fpsyear` consistently through joins and projections

At minimum, this must hold for the FPS source tables used by MABArchive:
- `fps.tlkpprogram`
- `fps.tlkpproject`
- `fps.fpsyeartotals`
- `fps.monthlyoutput`
- `fps.monthlytime`
- `fps.proj_invoice`
- `fps.proj_subcontract`
- `fps.projectmonthfinal`
- `fps.tbladditionalcosts`
- `fps.tblanimalreq`
- `fps.tblcontract`
- `fps.tblstaffjob`
- `fps.timecostcalcs`
- `fps.tlkptestreqmt`
- `fps.workgroupgrade`
- `fps.profitcentregrade`
- `fps.testorproduct`
- `fps.tblwgemployee`
- `fps.tblemployee`
- `fps.workgroup`
- `fps.tblanimals`

### 1) Update/Confirm 4 canonical totals views include `fpsyear`
Please ensure the following views project `fpsyear` in their output:
- `fps.qrytotaladditionalcosts`
- `fps.qrytotalanimalcosts`
- `fps.qrytotalstaffcosts`
- `fps.qrytotaltestcosts`

Minimum expected output columns per view:
- `jobcode`
- `fpsyear`
- corresponding total column (`totaladditionalcosts` / `totalanimalcosts` / `totalstaffcosts` / `totaltestcosts`)

Why blocking:
- Without `fpsyear`, strict-year-isolation fails and May-Dec full cycle is blocked.
- Missing `fpsyear` also risks cross-year joins.

### 2) Confirm canonical join derivation for `fpsyear` inside each view
Please confirm the authoritative join path used to derive `fpsyear` in each `qrytotal*costs` view so engineering and DB remain consistent.

### 3) Confirm canonical datatype policy for totals arithmetic
Please confirm whether canonical contract is:
- PostgreSQL `money` (current behavior), or
- `numeric(18,2)`

If `money` is canonical, confirm any required casting guidance for deterministic aggregates.

## DBA Actions Required (Operational Foundation)
Please confirm these operational objects exist and are managed in cloud:
- `fps.job_master`
- `fps.job_status`
- `fps.job_queue`
- `fps.job_queue_log`
- `fps.job_lock`

Please also enforce single active lock per job:
- unique partial index on `fps.job_lock(job_name)` where `is_active = true`
- expected index name: `uq_job_lock_job_name_active`

## Evidence Requested Back from DBA
Please provide the following artifacts after apply/confirmation:
1. DDL or catalog extract proving composite PKs for the year-bearing FPS source tables listed above.
2. DDL or catalog extract proving year-aligned FKs were recreated with `fpsyear` included.
3. DDL for each `fps.qrytotal*costs` view listed above.
4. Column metadata extract proving `jobcode`, `fpsyear`, and total column are present.
5. Confirmation of datatype policy for totals calculations (`money` vs `numeric(18,2)`).
6. DDL/metadata proof for `fps.job_*` foundation tables and active-lock unique partial index.

## Local Engineering Note (Informational)
Engineering already applied local-only hotfixes to unblock parity testing by adding `fpsyear` to local `qrytotal*costs` views.
This must now be reflected (or confirmed) in canonical cloud DB definitions.

Engineering also verified that the current local DB snapshot does not yet reflect the composite PK/FK model described in the target design document.

**Attempted local composite-PK redesign (2026-05-04)**: Engineering attempted to apply the composite-PK redesign locally but discovered the scope requires rewriting **27+ foreign-key relationships** across dependent tables. This work is too risky and too large for local implementation; it must be done by DBA on the canonical cloud DB. 

**Current local testing strategy**: Engineering uses a rerunnable SQL seed script (`200_insert_test_scenario_data.sql`) that clones 2026 FPS data into non-colliding 2025 keys using suffix naming. This allows full two-year parity testing of the MABArchive Jan–Apr vs. May–Dec branch logic without schema changes. This workaround is valid until the canonical DB is modernized with the composite-PK design.

**Reference**: Failed migration scripts are in `src/Apha.BatchJobs/database/sql/`:
- `010_apply_composite_pk_fk_redesign.sql` (initial attempt, syntax issues)
- `011_apply_composite_pk_fk_redesign_v2.sql` (refined attempt, failed on cascading FK dependencies)

These remain as reference for the DBA's implementation on the canonical cloud DB.
