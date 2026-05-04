# Ask from DBA

## Purpose
This document is the consolidated DBA handoff for promoting current MABArchive runtime assumptions to canonical cloud database definitions.

## What Engineering Changed (App Side)
1. MABArchive totals rebuild is now year-isolated.
2. Runtime enforces strict guard: all `fps.qrytotal*costs` views must expose `fpsyear`.
3. Runtime now joins totals sources by `(parentproject, fpsyear)` and performs year-scoped totals delete.

Because of this, canonical DB objects must match these contracts.

## DBA Actions Required (Blocking)

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
1. DDL for each `fps.qrytotal*costs` view listed above.
2. Column metadata extract proving `jobcode`, `fpsyear`, and total column are present.
3. Confirmation of datatype policy for totals calculations (`money` vs `numeric(18,2)`).
4. DDL/metadata proof for `fps.job_*` foundation tables and active-lock unique partial index.

## Local Engineering Note (Informational)
Engineering already applied local-only hotfixes to unblock parity testing by adding `fpsyear` to local `qrytotal*costs` views.
This must now be reflected (or confirmed) in canonical cloud DB definitions.
