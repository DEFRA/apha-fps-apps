# BatchJobs DBA Anomalies and Actions (2026-06-11)

## 1. Purpose

This document is for DBA and Infrastructure teams.
It captures database-related anomalies observed while enabling and stabilizing RecreateSummaries and MABArchive, and defines explicit verification and remediation actions.

Primary objective:
- Remove environment drift that caused execution failures, partial runs, and inconsistent operational telemetry during RecreateSummaries/MABArchive rollout.

Scope:
- Runtime/control schema in `fps` used by BatchJobs orchestration.
- Operational reliability and performance risks related to lock, execution, status, and cancellation tables.

Out of scope:
- UI/API route design topics.
- EventBridge rule design details.

---

## 2. Baseline Runtime Objects

Expected runtime/control tables:
- `fps.job_master`
- `fps.job_status`
- `fps.job_queue`
- `fps.job_queue_log`
- `fps.job_lock`
- `fps.job_cancellation_request`

Expected critical indexes (minimum baseline):
- `uq_job_lock_job_name_active` (partial unique on active locks)
- `uq_job_queue_jobexecutionid` (unique correlation key)
- `idx_job_queue_requestedby`
- `idx_job_queue_requested_at_utc`
- `idx_job_cancel_requested_at`
- `idx_job_cancel_status`

---

## 3. Anomaly Register

## A1. Status Vocabulary Drift Across Environments

Observed anomaly:
- Some environments have incomplete status rows in `fps.job_status` per job.
- Missing values commonly include `Pending`, `Retry`, `Skipped`, and occasionally `CancelRequested`.

Why this is a problem:
- Worker and APIs can emit/expect states that are not present in lookup data.
- Leads to mapping failures, inconsistent reporting, or runtime update failures.

Impact:
- Incorrect status transitions.
- Query/report mismatch for operational dashboards.

Exact scenarios observed and how this causes issues:
- Scenario S1-1 (missing `Retry` status):
  - Trigger: worker attempts transition from `Failed` to `Retry` after transient dependency failure.
  - DB anomaly: required lookup row does not exist in `fps.job_status` for that job.
  - Technical failure: transition cannot be persisted consistently.
  - Resulting issue: execution lifecycle becomes non-deterministic, retries are skipped or misreported.
- Scenario S1-2 (missing `CancelRequested` status):
  - Trigger: API writes cancellation intent while execution is still active.
  - DB anomaly: `CancelRequested` status absent.
  - Technical failure: cancellation state cannot be represented in canonical status model.
  - Resulting issue: cancel workflows appear "accepted" to caller but runtime status remains ambiguous, causing support escalations.

Required action:
1. Ensure each job in `fps.job_master` has all required statuses.
2. Seed idempotently using `ON CONFLICT DO NOTHING`.

Verification SQL:
```sql
SELECT jm.jobname, js.status
FROM fps.job_master jm
CROSS JOIN (
  VALUES
    ('Pending'),('Running'),('Retry'),('Completed'),('Failed'),('Cancelled'),('Skipped'),('CancelRequested')
) AS req(status)
LEFT JOIN fps.job_status js
  ON js.jobid = jm.jobid AND js.status = req.status
WHERE js.statusid IS NULL
ORDER BY jm.jobname, req.status;
```

Acceptance:
- Query returns zero rows.

---

## A2. `job_lock` Schema Drift (legacy `run_id` vs current `jobqueueid`)

Observed anomaly:
- Legacy deployments may still hold older lock-table shape using `run_id` instead of `jobqueueid` UUID.

Why this is a problem:
- Current worker lock flow correlates locks to `jobqueueid`.
- Legacy column shape can break lock acquisition/release behavior and diagnostics.

Impact:
- Lock corruption risk.
- Inability to trace lock to execution row.

Exact scenarios observed and how this causes issues:
- Scenario S2-1 (legacy `run_id` shape):
  - Trigger: worker acquires lock for new execution and persists queue row.
  - DB anomaly: lock-table correlation is still based on legacy `run_id` semantics.
  - Technical failure: lock cannot be deterministically joined back to `job_queue` by `jobqueueid`.
  - Resulting issue: stale/phantom lock diagnosis is unreliable and can block valid triggers.
- Scenario S2-2 (active-lock uniqueness not enforced):
  - Trigger: near-simultaneous requests for same job name.
  - DB anomaly: missing/misconfigured partial unique index for active locks.
  - Technical failure: two active lock rows can coexist.
  - Resulting issue: duplicate execution starts or inconsistent lock release behavior.

Required action:
1. Confirm `fps.job_lock` has `jobqueueid UUID NOT NULL UNIQUE`.
2. Confirm partial unique index on active locks exists.
3. Remove legacy lock column usage where present.

Verification SQL:
```sql
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'fps' AND table_name = 'job_lock'
ORDER BY ordinal_position;

SELECT indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'fps' AND tablename = 'job_lock'
ORDER BY indexname;
```

Acceptance:
- `jobqueueid` exists and is UUID.
- `uq_job_lock_job_name_active` exists with predicate `WHERE is_active = true`.

---

## A3. Missing or Unindexed `requested_at_utc` on `job_queue`

Observed anomaly:
- Older schemas may not have `requested_at_utc`.
- Some environments may have the column but no index.

Why this is a problem:
- Trigger-to-start latency cannot be measured reliably.
- Status/ops queries using acceptance timestamp become slow.

Impact:
- Reduced observability for startup delay.
- Slower investigations for trigger pipeline issues.

Exact scenarios observed and how this causes issues:
- Scenario S3-1 (`requested_at_utc` missing):
  - Trigger: job queued from API trigger.
  - DB anomaly: acceptance timestamp column absent.
  - Technical failure: no persisted acceptance time for queue row.
  - Resulting issue: trigger-to-start SLA cannot be measured or audited.
- Scenario S3-2 (`requested_at_utc` unindexed):
  - Trigger: ops asks for oldest pending requests and delayed starts.
  - DB anomaly: index absent on time-based filter/sort column.
  - Technical failure: heavy scans on large queue tables.
  - Resulting issue: triage queries are slow during incidents, extending MTTR.

Required action:
1. Ensure nullable `requested_at_utc TIMESTAMPTZ` exists on `fps.job_queue`.
2. Ensure `idx_job_queue_requested_at_utc` exists.

Verification SQL:
```sql
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'fps'
  AND table_name = 'job_queue'
  AND column_name = 'requested_at_utc';

SELECT indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'fps'
  AND tablename = 'job_queue'
  AND indexname = 'idx_job_queue_requested_at_utc';
```

Acceptance:
- Column exists as nullable `timestamp with time zone`.
- Index exists.

---

## A4. Cancellation Table Not Fully Operationalized

Observed anomaly:
- `fps.job_cancellation_request` exists, but full operational usage is inconsistent across paths.
- Data quality and operational clean-up practices are not uniformly enforced.

Why this is a problem:
- Cancel requests can remain stale (`Pending`) beyond reasonable SLA.
- Operational views can become noisy and misleading.

Impact:
- Unclear cancellation state.
- Manual triage burden.

Exact scenarios observed and how this causes issues:
- Scenario S4-1 (pending rows never terminalized):
  - Trigger: cancel request submitted for execution that eventually completes/fails.
  - DB anomaly: no consistent process updates cancellation row to terminal state.
  - Technical failure: cancellation table accumulates stale `Pending` records.
  - Resulting issue: dashboards/reporting overstate active cancellation backlog.
- Scenario S4-2 (cancel query performance degradation):
  - Trigger: worker/ops repeatedly queries pending cancellations.
  - DB anomaly: status/request-time indexes missing or not used.
  - Technical failure: repeated full scans for operational checks.
  - Resulting issue: delayed cancel observability and slow operations dashboards.

Required action:
1. Ensure table shape and indexes match runtime expectations.
2. Track and clean stale pending rows via job.
3. Monitor growth and terminalization behavior.

Verification SQL:
```sql
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'fps'
  AND table_name = 'job_cancellation_request'
ORDER BY ordinal_position;

SELECT indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'fps'
  AND tablename = 'job_cancellation_request'
ORDER BY indexname;

SELECT COUNT(*) AS pending_count
FROM fps.job_cancellation_request
WHERE status = 'Pending';
```

Acceptance:
- Table includes: `jobexecutionid`, `requested_by`, `requested_at_utc`, `status`, `source`, `consumed_at_utc`, `consumed_by`, `terminalized_at_utc`.
- Both `idx_job_cancel_requested_at` and `idx_job_cancel_status` exist.

---

## A5. No Automated Cleanup for Stale Active Locks

Observed anomaly:
- Cleanup is mostly manual/ad-hoc in some environments.

Why this is a problem:
- Stale active locks can block legitimate triggers.

Impact:
- False `AlreadyRunning`/lock conflicts.
- Operational support tickets.

Exact scenarios observed and how this causes issues:
- Scenario S5-1 (orphaned expired lock):
  - Trigger: worker process restarts after lock acquisition but before lock release.
  - DB anomaly: no automated stale-lock cleanup.
  - Technical failure: expired lock remains `is_active = TRUE`.
  - Resulting issue: subsequent valid trigger is blocked with false running-state conflict.
- Scenario S5-2 (lock row without active execution):
  - Trigger: partial failure leaves lock row but queue/status path not active.
  - DB anomaly: manual cleanup only, no scheduled reconciliation.
  - Technical failure: lock table and queue state drift apart.
  - Resulting issue: recurring false positives and repetitive support intervention.

Required action:
1. Add scheduled cleanup job for stale lock rows.
2. Cleanup should verify lock is stale and no active execution exists.
3. Emit audit logs/metrics for cleanup actions.

Recommended safety query:
```sql
SELECT l.lock_id, l.job_name, l.jobqueueid, l.acquired_at, l.expires_at
FROM fps.job_lock l
LEFT JOIN fps.job_queue q
  ON q.jobqueueid = l.jobqueueid
LEFT JOIN fps.job_status s
  ON s.statusid = q.statusid
WHERE l.is_active = TRUE
  AND l.expires_at < NOW()
  AND (q.jobqueueid IS NULL OR s.status NOT IN ('Running','Pending','Retry'));
```

Acceptance:
- Scheduled cleanup exists and is monitored.
- Active lock count trends stable.

---

## A6. Production Index Tuning Not Yet Evidence-Based

Observed anomaly:
- Baseline indexes exist, but additional indexes are discussed without completed post-load query-plan evidence.

Why this is a problem:
- Premature indexing can increase write overhead and maintenance costs.

Impact:
- Potential slower writes and index bloat.

Exact scenarios observed and how this causes issues:
- Scenario S6-1 (premature indexing):
  - Trigger: index added before representative production plan evidence.
  - DB anomaly: index does not match dominant filter/selectivity patterns.
  - Technical failure: write amplification without proportional read gain.
  - Resulting issue: queue/cancel write paths slow down and maintenance overhead increases.
- Scenario S6-2 (wrong hotspot assumptions):
  - Trigger: tuning decisions based on short-lived/test traffic patterns.
  - DB anomaly: missing long-window query statistics.
  - Technical failure: optimized index does not serve actual production workload.
  - Resulting issue: repeated index churn and avoidable DBA toil.

Required action:
1. Collect query stats and plans for 2-3 months of production load.
2. Only then apply targeted indexes for observed hotspots.

Candidate future indexes (only after evidence):
- `idx_job_lock_job_name_expires_at_active` on `fps.job_lock(job_name, expires_at)` with `WHERE is_active = TRUE`.
- `idx_job_cancel_status_pending` on `fps.job_cancellation_request(requested_at_utc DESC)` with `WHERE status = 'Pending'`.

Acceptance:
- Index additions are justified by plan data and approved.

---

## A7. RecreateSummaries Dependency Drift (Missing Views/Constraints in FPS Schema)

Observed anomaly:
- While enabling RecreateSummaries, some environments were missing required FPS views and relational constraints expected by batch SQL paths.
- Local remediation scripts were required for:
  - missing views used by RecreateSummaries query paths
  - missing PK/FK constraints on core source tables (`milestone`, `timecodevalid`, `tlkptestcapability`)

Why this is a problem:
- RecreateSummaries relies on legacy FPS objects being present and consistent.
- Missing views/constraints causes SQL failures, incorrect joins, or silent data quality issues.

Impact:
- RecreateSummaries runs fail or produce inconsistent outputs.
- Environment parity between local/non-prod/prod is lost.

Exact scenarios observed and how this causes issues:
- Scenario S7-1 (missing view dependency):
  - Trigger: RecreateSummaries runs summary SQL that expects FPS view chain.
  - DB anomaly: one or more expected views are missing (for example `qrymilestone1`, `qryjobmonthmilestone`).
  - Technical failure: SQL execution fails with relation/view not found.
  - Resulting issue: entire summary rebuild run stops; downstream users receive stale summary data.
- Scenario S7-2 (missing PK/FK assumptions):
  - Trigger: summary joins and aggregations assume canonical relational constraints.
  - DB anomaly: constraints missing on `milestone`, `timecodevalid`, or `tlkptestcapability`.
  - Technical failure: join cardinality guarantees are lost.
  - Resulting issue: duplicate/missing aggregates and non-reproducible output between environments.

Cloud vs local comparison (checked 2026-06-11):
- In cloud (`dbmig`/`fps`), required RecreateSummaries views are present.
- In cloud (`dbmig`/`fps`), the following expected constraint names are not present:
  - `pk_milestone_1__12`
  - `aaaaatimecodevalid_pk`
  - `pk__tlkptestcapabili__4e53a1aa`
  - `fk_tlkptestcapability_1__15`
  - `fk_tlkptestcapability_1__18`
  - `fk_tlkptestcapability_2__18`
- This indicates environment drift in constraint naming/definition, even when views exist.

How this is causing issues in practice:
- RecreateSummaries can run in one environment but produce different row-level outcomes in another because relational guarantees differ.
- The same SQL can have different execution plans and join behavior due to missing PK/FK metadata.
- Support teams see "works in local after fix" but "different output/behavior in cloud" because schema guarantees are not equivalent.

Required DBA action:
1. Validate required RecreateSummaries views exist in schema `fps`.
2. Validate required PK/FK constraints exist on referenced source tables.
3. Align any drifted environment to canonical schema used by working environments.

Verification SQL (views):
```sql
SELECT table_schema, table_name
FROM information_schema.views
WHERE table_schema = 'fps'
  AND table_name IN (
    'vpacttblstaff',
    'vpacttlkptestcapability',
    'qrymilestone1',
    'qryjobmonthmilestone',
    'qryprojectmonthcw',
    'qryjobmonth_subcontracts',
    'qryjobmonth_invoices',
    'qryjobmonth_transferstotal'
  )
ORDER BY table_name;
```

Verification SQL (constraints):
```sql
SELECT n.nspname AS schema_name, c.relname AS table_name, co.conname, co.contype
FROM pg_constraint co
JOIN pg_class c ON c.oid = co.conrelid
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'fps'
  AND c.relname IN ('milestone', 'timecodevalid', 'tlkptestcapability')
ORDER BY c.relname, co.conname;
```

Acceptance:
- All required views are present and valid.
- Required PK/FK constraints exist on the three source tables above.

---

## A8. MABArchive Source/Target Schema Alignment Gaps

Observed anomaly:
- During MABArchive enablement, schema alignment scripts were needed to match cloud/reference expectations.
- Typical drift included:
  - structure mismatch for `fps.tblyearmaster`
  - nullable `fpsyear` columns where non-null was required on source tables
  - sequence/default naming drift for `mabarchive.my_tblanimalreq.ar_counter`

Why this is a problem:
- MABArchive assumes source/target structures match expected contract shape.
- Drift causes load failures, bad data movement, or partial loads.

Impact:
- Scheduled archive runs fail or produce incomplete archive data.
- Operational recovery requires manual DBA intervention.

Exact scenarios observed and how this causes issues:
- Scenario S8-1 (`tblyearmaster` structure drift):
  - Trigger: MABArchive process reads/writes against expected year master contract.
  - DB anomaly: table definition diverges from cloud/reference expectations.
  - Technical failure: mapping/type/nullability incompatibilities.
  - Resulting issue: archive stage fails or produces schema-dependent data inconsistencies.
- Scenario S8-2 (`fpsyear` nullability drift across source tables):
  - Trigger: archive extraction joins/filtering by `fpsyear`.
  - DB anomaly: nullable `fpsyear` allowed where non-null is required.
  - Technical failure: orphaned or excluded rows during joins/grouping.
  - Resulting issue: incomplete archive datasets and reconciliation mismatch.
- Scenario S8-3 (`ar_counter` sequence/default drift):
  - Trigger: inserts into `mabarchive.my_tblanimalreq` rely on default sequence.
  - DB anomaly: sequence naming/default expression not aligned with canonical contract.
  - Technical failure: default generation errors or non-standard sequence behavior.
  - Resulting issue: interrupted archive load and manual correction steps.

Cloud vs local comparison (checked 2026-06-11):
- In cloud (`dbmig`/`fps`), `fps.tblyearmaster` currently matches expected shape:
  - `fpsyear` int not null
  - `fpsyearcode` varchar not null
  - `yearstatus` varchar not null
  - `remarks` text nullable
  - `active` boolean not null
  - `createdon` timestamptz not null
  - `createdby` varchar nullable
- In cloud (`dbmig`/`fps`), required source tables checked currently have non-null `fpsyear`.
- In cloud (`dbmig`/`fps`), `mabarchive.my_tblanimalreq.ar_counter` default is missing.

How this is causing issues in practice:
- MABArchive runs that rely on implicit `ar_counter` default can fail on insert in cloud while succeeding in corrected local setups.
- Even when table shapes look aligned, missing default wiring causes runtime failure at write-time, not at startup validation.
- This creates intermittent, stage-specific failures that are hard to detect from static schema checks alone.

Required DBA action:
1. Validate `fps.tblyearmaster` shape matches reference.
2. Enforce non-null `fpsyear` on required MABArchive source tables.
3. Validate target sequence/default wiring for `my_tblanimalreq.ar_counter`.

Verification SQL:
```sql
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'fps' AND table_name = 'tblyearmaster'
ORDER BY ordinal_position;

SELECT table_name, column_name, is_nullable
FROM information_schema.columns
WHERE table_schema = 'fps'
  AND column_name = 'fpsyear'
  AND table_name IN (
    'fpsyeartotals','monthlyoutput','monthlytime','profitcentregrade','proj_invoice',
    'proj_subcontract','projectmonthfinal','tbladditionalcosts','tblanimalreq',
    'tblanimals','tblcontract','tblemployee','tblstaffjob','tblwgemployee',
    'testorproduct','timecostcalcs','tlkpprogram','tlkpproject','tlkptestreqmt',
    'workgroup','workgroupgrade'
  )
ORDER BY table_name;

SELECT pg_get_expr(ad.adbin, ad.adrelid) AS default_expr
FROM pg_attrdef ad
JOIN pg_class c ON c.oid = ad.adrelid
JOIN pg_namespace n ON n.oid = c.relnamespace
JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = ad.adnum
WHERE n.nspname = 'mabarchive'
  AND c.relname = 'my_tblanimalreq'
  AND a.attname = 'ar_counter';
```

Acceptance:
- `tblyearmaster` matches expected shape.
- All listed source tables have non-null `fpsyear`.
- `ar_counter` default correctly points to the expected sequence.

---

## 4. Immediate DBA Runbook (Priority Order)

P0 (same day):
1. Validate table shapes for `job_lock`, `job_queue`, `job_cancellation_request`.
2. Validate critical unique indexes:
   - `uq_job_lock_job_name_active`
   - `uq_job_queue_jobexecutionid`
3. Validate required status vocabulary coverage (A1 query).
4. Validate RecreateSummaries dependency objects (A7 views + constraints).
5. Validate MABArchive schema alignment objects (A8 checks).

P1 (this sprint):
1. Backfill missing statuses idempotently.
2. Ensure `requested_at_utc` + index exists everywhere.
3. Baseline dashboards:
   - active locks
  - pending cancellation count
   - running execution count
4. Normalize all drifted environments to canonical schema used by a known-good execution environment.

P2 (next sprint):
1. Automate stale lock cleanup.
2. Automate stale cancellation cleanup policy.
3. Capture query plans and index tuning proposal.
4. Add recurring drift validation for A7/A8 objects to release-readiness checks.

---

## 5. Detailed Reproduction Scenarios for DBA Validation

Scenario R1: RecreateSummaries failure caused by missing view
1. Precondition: remove/rename one required FPS view in non-prod clone.
2. Trigger: execute RecreateSummaries.
3. Expected pre-fix behavior: SQL relation/view-not-found failure and job run abort.
4. Expected post-fix behavior: run completes with no missing-object failures.

Scenario R2: RecreateSummaries data inconsistency caused by missing constraints
1. Precondition: missing PK/FK constraints on `milestone`, `timecodevalid`, `tlkptestcapability`.
2. Trigger: execute same summary period in two environments.
3. Expected pre-fix behavior: row count/value mismatch across environments.
4. Expected post-fix behavior: deterministic and matching outputs for identical inputs.

Scenario M1: MABArchive failure caused by `tblyearmaster` schema drift
1. Precondition: `fps.tblyearmaster` differs from canonical cloud/reference shape.
2. Trigger: execute archive stage consuming year master contract.
3. Expected pre-fix behavior: mapping/type/nullability failure or stage abort.
4. Expected post-fix behavior: stage executes successfully and schema parity checks pass.

Scenario M2: MABArchive incompleteness caused by nullable `fpsyear`
1. Precondition: null `fpsyear` rows exist in one or more required source tables.
2. Trigger: run archive extraction and reconciliation.
3. Expected pre-fix behavior: mismatched totals or dropped/orphaned records.
4. Expected post-fix behavior: no null `fpsyear` in required tables and reconciled totals.

Scenario M3: MABArchive insert disruption caused by `ar_counter` default drift
1. Precondition: incorrect default expression/sequence linkage for `my_tblanimalreq.ar_counter`.
2. Trigger: insert archive records without explicit `ar_counter`.
3. Expected pre-fix behavior: insert failure or sequence progression anomaly.
4. Expected post-fix behavior: inserts succeed with correct sequence-backed default behavior.

---

## 6. Canonical Monitoring Queries

Active locks:
```sql
SELECT COUNT(*)
FROM fps.job_lock
WHERE is_active = TRUE;
```

Pending cancellations:
```sql
SELECT COUNT(*)
FROM fps.job_cancellation_request
WHERE status = 'Pending';
```

Running/pending/retry executions:
```sql
SELECT COUNT(DISTINCT q.jobid)
FROM fps.job_queue q
JOIN fps.job_status s ON s.statusid = q.statusid
WHERE s.status IN ('Running','Pending','Retry');
```

Potential stale locks:
```sql
SELECT l.lock_id, l.job_name
FROM fps.job_lock l
WHERE l.is_active = TRUE
  AND l.jobqueueid NOT IN (
    SELECT q.jobqueueid
    FROM fps.job_queue q
    JOIN fps.job_status s ON s.statusid = q.statusid
    WHERE s.status IN ('Running','Pending','Retry')
  );
```

---

## 7. CR Coverage Check (CR001-CR010)

Source checked:
- CR001 to CR010 sheet shared on 2026-06-11 (views/tables/seeding requests and statuses).

Summary result:
- A1 to A8 are not fully covered by CR001-CR010.
- Only A1 has substantial partial coverage from CR010.
- A7 has limited partial coverage from CR003.
- A8 has limited partial coverage from CR007.

Anomaly-to-CR mapping:

1. A1 Status vocabulary drift
- Coverage in CRs: Partial
- Related CRs: CR010 (seed data for job master/status), CR009 (execution control tables)
- Gap remaining:
  - Need guaranteed full status set per job, including `Retry`, `Skipped`, and `CancelRequested` across environments.
  - Need repeatable idempotent verification/remediation in all target DBs, not only initial seed execution.

2. A2 job_lock schema drift
- Coverage in CRs: Not covered
- Related CRs: None explicitly include `job_lock` shape remediation.
- Gap remaining:
  - No CR explicitly enforces `jobqueueid`-based lock schema and active-lock uniqueness behavior.

3. A3 requested_at_utc on job_queue
- Coverage in CRs: Partial at best
- Related CRs: CR009 (job queue creation likely in scope)
- Gap remaining:
  - No explicit CR evidence for required `requested_at_utc` column + index verification across environments.

4. A4 cancellation table operationalization
- Coverage in CRs: Not covered
- Related CRs: None in CR001-CR010 for cancellation process hardening.
- Gap remaining:
  - No explicit CR for operational lifecycle controls, stale pending cleanup, and performance checks on cancellation paths.

5. A5 automated stale-lock cleanup
- Coverage in CRs: Not covered
- Related CRs: None in CR001-CR010.
- Gap remaining:
  - No scheduled cleanup mechanism or runbook governance captured in CR list.

6. A6 evidence-based index tuning
- Coverage in CRs: Not covered
- Related CRs: None in CR001-CR010.
- Gap remaining:
  - No CR for post-load plan capture and controlled index decisions.

7. A7 RecreateSummaries dependency drift
- Coverage in CRs: Partial
- Related CRs: CR003 (modify `vpacttblStaff`), CR001/CR002/CR004/CR005/CR006/CR008 (view work)
- Gap remaining:
  - CRs cover selected view objects, but do not fully guarantee required PK/FK constraints and deterministic parity for all RecreateSummaries dependencies.

8. A8 MABArchive alignment gaps
- Coverage in CRs: Partial
- Related CRs: CR007 (`mabarchive.tblstagingmilestone` create)
- Gap remaining:
  - CR list does not explicitly cover `tblyearmaster` contract alignment, full `fpsyear` nullability enforcement set, and `my_tblanimalreq.ar_counter` default wiring.

Conclusion:
- Current anomaly register remains valid and actionable.
- CR001-CR010 provides useful baseline progress but does not close all runtime parity and reliability risks identified in A1-A8.

---

## 8. Notes

- This document is explicit by design for DBA execution and audit traceability.
- It is aligned to the finalized BatchJobs contract and CR decisions as of 2026-06-11.
- If schema changes are applied from this runbook, update the main integration documentation and migration history together.
- Cloud connectivity validation on 2026-06-11 showed available databases include `dbmig` and `fps`; `fps_dbmig` was not present.
