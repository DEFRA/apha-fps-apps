# Ask from DBA

## Purpose
This document is the DBA handoff for Cloud DB readiness of RecreateSummaries.

## Cloud Scope (RecreateSummaries)

Please verify these dependency views exist in cloud and match canonical definitions:

- `fps.vpacttblstaff`
- `fps.vpacttlkptestcapability`
- `fps.qrymilestone1`
- `fps.qryjobmonthmilestone`
- `fps.qryprojectmonthcw`
- `fps.qryjobmonth_subcontracts1`
- `fps.qryjobmonth_subcontracts`
- `fps.qryjobmonth_invoices`
- `fps.qryjobmonthportfoliosales`
- `fps.qryjobmonth_tctransfers`
- `fps.qryjobmonth_transfers1`
- `fps.qryjobmonth_transferunion`
- `fps.qryjobmonth_transferstotal`

CloudDump reference indicates these views already exist; DBA action is definition/parity confirmation and drift correction if needed.

### 1) Required definition for `fps.qrymilestone1`

Required behavior:

- removed hardcoded filter `WHERE year = '2003/2004'`
- retained output columns including `year` and `fpsyear`

Current expected definition behavior:

- reads all rows from `fps.milestone` (no fixed-year predicate)

### 2) Required definition for `fps.vtbltestrequ`

Required behavior:

- removed nested `CURRENT_USER`-driven security filter chain
- now sources directly from `fps.tlkptestreqmt`

Reason:

- RecreateSummaries is a batch/system process and should not be restricted by session user mappings.

### 3) Required constraints on key upstream tables

Please verify constraints exist for:

- `fps.milestone`
   - `pk_milestone_1__12` PK `(project, milestoneref, objectiveref)`
   - `fk_milestone_project` FK `(fpsyear, project) -> fps.tlkpproject(fpsyear, parentproject)`
- `fps.timecodevalid`
   - `aaaaatimecodevalid_pk` PK `(workgroup, timecode, parentproject)`
   - `fk_timecodevalid_parentproject` FK `(fpsyear, parentproject) -> fps.tlkpproject(fpsyear, parentproject)`
- `fps.tlkptestcapability`
   - `pk__tlkptestcapabili__4e53a1aa` PK `(testcode, workgroup)`
   - `fk_tlkptestcapability_1__15` FK `(fpsyear, workgroup) -> fps.workgroup(fpsyear, workgroup)`
   - `fk_tlkptestcapability_1__18` FK `(fpsyear, planportfolio) -> fps.tlkpproject(fpsyear, parentproject)`
   - `fk_tlkptestcapability_2__18` FK `(fpsyear, testcode) -> fps.testorproduct(fpsyear, itemcode)`

- If cloud key model is year-composite, ensure FKs include `fpsyear` on both sides.

## DBA Actions Required in Cloud

Apply/confirm the required behavior in canonical Cloud DB.

### A) Ensure all RecreateSummaries dependency views exist

Please ensure the dependency views listed in the Cloud Scope section exist in schema `fps` with canonical definitions.

### B) Update/confirm `fps.qrymilestone1`

Required behavior:

- no hardcoded year predicate
- no fixed literal like `'2003/2004'`

### C) Update/confirm `fps.vtbltestrequ`

Required behavior:

- no `CURRENT_USER`-based filtering for RecreateSummaries data path
- batch-safe dataset source from canonical test requirement base tables

### D) Confirm constraints on key upstream tables

Please verify constraints exist for:

- `fps.milestone`
- `fps.timecodevalid`
- `fps.tlkptestcapability`

## Validation Queries for DBA (Cloud)

### 1) Missing required tables/views check (RecreateSummaries scope)

```sql
WITH req(obj_type,obj_name) AS (
   VALUES
   ('TABLE','fpsyeartotals'),('TABLE','tlkpproject'),('TABLE','projectmonth'),('TABLE','timecostcalcs'),
   ('TABLE','tblkpprofitcentre'),('TABLE','profitcentregrade'),('TABLE','workgroupgrade'),('TABLE','timecodevalid'),
   ('TABLE','monthlytime'),('TABLE','tlkpprogram'),('TABLE','projectmonthcasework'),('TABLE','projectmonthfinal'),
   ('TABLE','projectmonth2'),('TABLE','projectmonth3'),('TABLE','tblperiod'),('TABLE','recreatesummaries_log'),
   ('TABLE','period_monthlyoutput'),('TABLE','costcentre'),('TABLE','monthlyoutput'),('TABLE','workgroup'),
   ('TABLE','tlkptestreqmt'),('TABLE','period_proj_subcontract'),('TABLE','proj_subcontract'),('TABLE','period_timecostcalcs'),
   ('TABLE','tblwgemployee'),('TABLE','tbladditionalcosts'),('TABLE','tblanimalreq'),('TABLE','tblanimals'),
   ('TABLE','tblstaffjob'),('TABLE','tblemployee'),('TABLE','tbluser_program'),('TABLE','tblusers'),
   ('TABLE','testorproduct'),('TABLE','tblperiodmonth'),('TABLE','milestone'),('TABLE','tlkptestcapability'),
   ('VIEW','qrytotaladditionalcosts'),('VIEW','qrytotalanimalcosts'),('VIEW','qrytotalstaffcosts'),('VIEW','qrytotaltestcosts'),
   ('VIEW','vpacttblstaff'),('VIEW','qryprojectmonthcw'),('VIEW','qryjobmonth_subcontracts'),('VIEW','qryjobmonth_time'),
   ('VIEW','qryjobmonthmilestone'),('VIEW','qryjobmonth_transferstotal'),('VIEW','qryjobmonth_invoices'),
   ('VIEW','qryjobmonthportfoliosales'),('VIEW','qryjobmonth_totprofile'),('VIEW','tblkperiodmonth'),
   ('VIEW','qrymilestone1'),('VIEW','vtbltestrequ'),('VIEW','vprojectanimalplan'),('VIEW','vprojectstaffplan'),
   ('VIEW','qryjobmonth_subcontracts1'),('VIEW','qryjobmonth_transferunion'),('VIEW','qryjobmonth_tctransfers'),
   ('VIEW','qryjobmonth_transfers1'),('VIEW','vpacttlkptestcapability')
),
existing AS (
   SELECT 'TABLE' AS obj_type, table_name AS obj_name
   FROM information_schema.tables
   WHERE table_schema='fps' AND table_type='BASE TABLE'
   UNION ALL
   SELECT 'VIEW' AS obj_type, table_name AS obj_name
   FROM information_schema.views
   WHERE table_schema='fps'
)
SELECT r.obj_type, r.obj_name
FROM req r
LEFT JOIN existing e
   ON e.obj_type = r.obj_type
 AND lower(e.obj_name) = lower(r.obj_name)
WHERE e.obj_name IS NULL
ORDER BY r.obj_type, r.obj_name;
```

Expected result: zero rows.

### 2) Hardcoded-year regression check

```sql
SELECT schemaname, viewname
FROM pg_views
WHERE schemaname = 'fps'
   AND viewname = 'qrymilestone1'
   AND definition ILIKE '%2003/2004%';
```

Expected result: zero rows.

### 3) User-context filter regression check

```sql
SELECT schemaname, viewname
FROM pg_views
WHERE schemaname = 'fps'
   AND viewname = 'vtbltestrequ'
   AND definition ILIKE '%current_user%';
```

Expected result: zero rows.

### 4) Constraint existence check for 3 key tables

```sql
SELECT c.relname AS table_name,
          co.conname AS constraint_name,
          co.contype,
          pg_get_constraintdef(co.oid) AS constraint_def
FROM pg_constraint co
JOIN pg_class c ON c.oid = co.conrelid
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'fps'
   AND c.relname IN ('milestone','timecodevalid','tlkptestcapability')
ORDER BY c.relname, co.contype, co.conname;
```

## Evidence Requested Back from DBA
Please provide:

1. DDL for the dependency views listed in this document.
2. DDL for `fps.qrymilestone1` and `fps.vtbltestrequ` after changes.
3. Constraint metadata extract for `fps.milestone`, `fps.timecodevalid`, `fps.tlkptestcapability`.
4. Output of the 4 validation queries above.
