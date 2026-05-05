---
title: MAB Archive Test Validation Procedures
date: 2026-04-30
version: 1.0
---

# MAB Archive Test Validation Procedures

## Overview

This document provides step-by-step SQL validation procedures to run AFTER the MAB Archive job completes, to verify that actual results match expected theoretical results.

**Key Principle**: We validate data movement and total calculations WITHOUT inspecting code logic.

---

## Phase 1: Data Load Verification (Before Job Runs)

### 1.1 Verify Test Data in fps Schema

Run these queries to confirm test scenarios loaded correctly:

```sql
-- Count test data by scenario
SELECT 'SCENARIO 1 - BASIC' as test_scenario, COUNT(*) as project_count
FROM fps.tlkpproject 
WHERE fpsyear=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 'SCENARIO 2 - MULTI', COUNT(*) as project_count 
FROM fps.tlkpproject 
WHERE fpsyear=2026 AND parentproject LIKE 'P200-MULTI%'
UNION ALL
SELECT 'SCENARIO 3 - COMPLEX', COUNT(*) as project_count 
FROM fps.tlkpproject 
WHERE fpsyear=2026 AND parentproject='P300-COMPLEX';

-- Expected output:
-- SCENARIO 1 - BASIC      | 1
-- SCENARIO 2 - MULTI      | 2
-- SCENARIO 3 - COMPLEX    | 1
```

### 1.2 Verify Master Data

```sql
-- Master lookup tables
SELECT COUNT(*) as program_count FROM fps.tlkpprogram WHERE fpsyear=2026;
-- Expected: 3

SELECT COUNT(*) as workgroup_count FROM fps.workgroup WHERE fpsyear=2026;
-- Expected: 3

SELECT COUNT(*) as animal_count FROM fps.tblanimals WHERE fpsyear=2026;
-- Expected: 4

SELECT COUNT(*) as employee_count FROM fps.tblemployee WHERE fpsyear=2026;
-- Expected: 4

SELECT COUNT(*) as test_req_count FROM fps.tlkptestreqmt WHERE fpsyear=2026;
-- Expected: 4

SELECT COUNT(*) as profit_centre_count FROM fps.tblkpprofitcentre;
-- Expected: 3 (no fpsyear filter, global reference)
```

### 1.3 Verify Detail Transaction Data

```sql
-- Additional costs across all scenarios
SELECT jobcode as parentproject, SUM(itemcost::numeric) as total_amount
FROM fps.tbladditionalcosts
WHERE fpsyear=2026
GROUP BY jobcode
ORDER BY jobcode;

-- Expected output:
-- P100-BASIC      | 2150
-- P200-MULTI-A    | 4700
-- P200-MULTI-B    | 1000
-- P300-COMPLEX    | 8700

-- Staff job allocations
SELECT jobcode as parentproject, COUNT(*) as workgroup_assignments
FROM fps.tblstaffjob
WHERE fpsyear=2026
GROUP BY jobcode
ORDER BY jobcode;

-- Expected output:
-- P100-BASIC      | 2
-- P200-MULTI-A    | 2
-- P200-MULTI-B    | 2
-- P300-COMPLEX    | 2

-- Monthly cost records
SELECT project as parentproject, COUNT(*) as monthly_records
FROM fps.timecostcalcs
WHERE fpsyear=2026
GROUP BY project
ORDER BY project;

-- Expected output:
-- P100-BASIC      | 24
-- P200-MULTI-A    | 24
-- P200-MULTI-B    | 24
-- P300-COMPLEX    | 24
```

### 1.4 Verify Staff Costs Calculation

```sql
-- Total staff costs by project (before job runs)
SELECT 
  project as parentproject,
  SUM(cost::numeric) as total_staff_cost,
  COUNT(*) as month_count
FROM fps.timecostcalcs
WHERE fpsyear=2026
GROUP BY project
ORDER BY project;

-- Expected output:
-- P100-BASIC      | 12700 | 4
-- P200-MULTI-A    | 24700 | 4
-- P200-MULTI-B    | 8000  | 4
-- P300-COMPLEX    | 40700 | 4
```

---

## Phase 2: Archive Post-Job Validation

### 2.1 Basic Archive Structure Check

After the MAB Archive job runs, verify archive schema is populated:

```sql
-- Check all 24 archive target tables have 2026 data
SELECT 
  schemaname, tablename, 
  (SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026) as year_2026_count
FROM information_schema.tables
WHERE table_schema='mabarchive'
AND table_name LIKE 'my_%'
ORDER BY tablename;

-- Each my_* table should have >0 rows for year 2026
```

### 2.2 Archive Row Counts by Scenario

```sql
-- Scenario 1 archive rows
SELECT 
  'my_fpsyeartotals' as archive_table,
  COUNT(*) as row_count
FROM mabarchive.my_fpsyeartotals
WHERE year=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 'my_tlkpproject', COUNT(*)
FROM mabarchive.my_tlkpproject
WHERE year=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 'my_timecostcalcs', COUNT(*)
FROM mabarchive.my_timecostcalcs
WHERE year=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 'my_monthlyoutput', COUNT(*)
FROM mabarchive.my_monthlyoutput
WHERE year=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 'my_monthlytime', COUNT(*)
FROM mabarchive.my_monthlytime
WHERE year=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 'my_tbladditionalcosts', COUNT(*)
FROM mabarchive.my_tbladditionalcosts
WHERE year=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 'my_tblanimalreq', COUNT(*)
FROM mabarchive.my_tblanimalreq
WHERE year=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 'my_tblstaffjob', COUNT(*)
FROM mabarchive.my_tblstaffjob
WHERE year=2026 AND parentproject='P100-BASIC';

-- Expected (Scenario 1):
-- my_fpsyeartotals     | 1
-- my_tlkpproject       | 1
-- my_timecostcalcs     | 4
-- my_monthlyoutput     | 4
-- my_monthlytime       | 4
-- my_tbladditionalcosts| 1
-- my_tblanimalreq      | 2
-- my_tblstaffjob       | 1
```

### 2.3 Scenario 2 Archive Validation

```sql
-- Count rows for Scenario 2 (both projects combined)
SELECT 
  'Scenario 2' as scenario,
  COUNT(*) as total_archive_rows
FROM mabarchive.my_fpsyeartotals
WHERE year=2026 AND parentproject LIKE 'P200-MULTI%';

-- Expected: 2 (one per project: P200-MULTI-A, P200-MULTI-B)

-- Staff costs by project
SELECT parentproject, SUM(staffcost::numeric) as total_staff_cost
FROM mabarchive.my_timecostcalcs
WHERE year=2026 AND parentproject LIKE 'P200-MULTI%'
GROUP BY parentproject
ORDER BY parentproject;

-- Expected output:
-- P200-MULTI-A | 24700
-- P200-MULTI-B | 8000
```

### 2.4 Scenario 3 Archive Validation (Full Load)

```sql
-- Verify all 24 loader tables populated for P300-COMPLEX
SELECT 'my_tlkpprogram' as table_name, COUNT(*) as rows FROM mabarchive.my_tlkpprogram WHERE year=2026
UNION ALL SELECT 'my_tlkpproject', COUNT(*) FROM mabarchive.my_tlkpproject WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_fpsyeartotals', COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_monthlyoutput', COUNT(*) FROM mabarchive.my_monthlyoutput WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_monthlytime', COUNT(*) FROM mabarchive.my_monthlytime WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_proj_invoice', COUNT(*) FROM mabarchive.my_proj_invoice WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_proj_subcontract', COUNT(*) FROM mabarchive.my_proj_subcontract WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_projectmonthfinal', COUNT(*) FROM mabarchive.my_projectmonthfinal WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_tbladditionalcosts', COUNT(*) FROM mabarchive.my_tbladditionalcosts WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_tblanimalreq', COUNT(*) FROM mabarchive.my_tblanimalreq WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_tblcontract', COUNT(*) FROM mabarchive.my_tblcontract WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_tblstaffjob', COUNT(*) FROM mabarchive.my_tblstaffjob WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_timecostcalcs', COUNT(*) FROM mabarchive.my_timecostcalcs WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_tlkptestreqmt', COUNT(*) FROM mabarchive.my_tlkptestreqmt WHERE year=2026
UNION ALL SELECT 'my_workgroupgrade', COUNT(*) FROM mabarchive.my_workgroupgrade WHERE year=2026
UNION ALL SELECT 'my_profitcentregrade', COUNT(*) FROM mabarchive.my_profitcentregrade WHERE year=2026
UNION ALL SELECT 'my_tblprofitcentre', COUNT(*) FROM mabarchive.my_tblprofitcentre
UNION ALL SELECT 'my_testorproduct', COUNT(*) FROM mabarchive.my_testorproduct WHERE year=2026
UNION ALL SELECT 'my_staff', COUNT(*) FROM mabarchive.my_staff WHERE year=2026
UNION ALL SELECT 'my_workgroup', COUNT(*) FROM mabarchive.my_workgroup WHERE year=2026
UNION ALL SELECT 'my_tblanimals', COUNT(*) FROM mabarchive.my_tblanimals WHERE year=2026
UNION ALL SELECT 'my_tlkpproject_all', COUNT(*) FROM mabarchive.my_tlkpproject_all WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'g_tlkpproject', COUNT(*) FROM mabarchive.g_tlkpproject WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'tlkpyear', COUNT(*) FROM mabarchive.tlkpyear WHERE year=2026
ORDER BY table_name;

-- Expected: Every table >0 rows
```

---

## Phase 3: Cost Calculation Validation

### 3.1 Staff Costs Reconciliation

Verify staff costs copied correctly from fps to mabarchive:

```sql
-- Compare fps vs mabarchive staff costs for Scenario 1
SELECT 
  'fps.timecostcalcs' as source,
  parentproject,
  SUM(staffcost::numeric) as total_cost
FROM fps.timecostcalcs
WHERE fpsyear=2026 AND parentproject='P100-BASIC'
GROUP BY parentproject
UNION ALL
SELECT 
  'mabarchive.my_timecostcalcs' as source,
  parentproject,
  SUM(staffcost::numeric) as total_cost
FROM mabarchive.my_timecostcalcs
WHERE year=2026 AND parentproject='P100-BASIC'
GROUP BY parentproject;

-- Expected (both rows should be identical):
-- fps.timecostcalcs            | P100-BASIC | 12700
-- mabarchive.my_timecostcalcs  | P100-BASIC | 12700
```

### 3.2 Additional Costs Reconciliation

```sql
-- Compare fps vs mabarchive additional costs for Scenario 3
SELECT 
  'fps.tbladditionalcosts' as source,
  parentproject,
  SUM(amount::numeric) as total_amount
FROM fps.tbladditionalcosts
WHERE fpsyear=2026 AND parentproject='P300-COMPLEX'
GROUP BY parentproject
UNION ALL
SELECT 
  'mabarchive.my_tbladditionalcosts' as source,
  parentproject,
  SUM(amount::numeric) as total_amount
FROM mabarchive.my_tbladditionalcosts
WHERE year=2026 AND parentproject='P300-COMPLEX'
GROUP BY parentproject;

-- Expected (both should be identical):
-- fps.tbladditionalcosts           | P300-COMPLEX | 10000
-- mabarchive.my_tbladditionalcosts | P300-COMPLEX | 10000
```

### 3.3 Totals Consistency Check

```sql
-- Check if monthly staff costs sum equals invoice and contract values
-- (Scenario 3: Complex Project)

WITH fps_costs AS (
  SELECT SUM(staffcost::numeric) as staff_cost
  FROM fps.timecostcalcs
  WHERE fpsyear=2026 AND parentproject='P300-COMPLEX'
),
archive_staff_costs AS (
  SELECT SUM(staffcost::numeric) as staff_cost
  FROM mabarchive.my_timecostcalcs
  WHERE year=2026 AND parentproject='P300-COMPLEX'
),
archive_invoices AS (
  SELECT SUM(invoiceamount::numeric) as invoice_total
  FROM mabarchive.my_proj_invoice
  WHERE year=2026 AND parentproject='P300-COMPLEX'
)
SELECT 
  (SELECT staff_cost FROM fps_costs) as fps_staff_costs,
  (SELECT staff_cost FROM archive_staff_costs) as archive_staff_costs,
  (SELECT invoice_total FROM archive_invoices) as archive_invoices,
  CASE 
    WHEN (SELECT staff_cost FROM fps_costs) = (SELECT staff_cost FROM archive_staff_costs)
    THEN 'PASS: Staff costs match'
    ELSE 'FAIL: Staff costs mismatch'
  END as validation;

-- Expected:
-- fps_staff_costs | archive_staff_costs | archive_invoices | validation
-- 40700           | 40700               | 40000            | PASS: Staff costs match
```

---

## Phase 4: Detail Row Count Validation

### 4.1 Monthly Records Rollup

```sql
-- Verify each project has 4 monthly records in each transaction table
SELECT 
  'my_timecostcalcs' as archive_table,
  parentproject,
  COUNT(*) as month_count,
  CASE WHEN COUNT(*) = 4 THEN 'PASS' ELSE 'FAIL' END as validation
FROM mabarchive.my_timecostcalcs
WHERE year=2026
GROUP BY parentproject
UNION ALL
SELECT 
  'my_monthlyoutput' as archive_table,
  parentproject,
  COUNT(*) as month_count,
  CASE WHEN COUNT(*) = 4 THEN 'PASS' ELSE 'FAIL' END as validation
FROM mabarchive.my_monthlyoutput
WHERE year=2026
GROUP BY parentproject
UNION ALL
SELECT 
  'my_monthlytime' as archive_table,
  parentproject,
  COUNT(*) as month_count,
  CASE WHEN COUNT(*) = 4 THEN 'PASS' ELSE 'FAIL' END as validation
FROM mabarchive.my_monthlytime
WHERE year=2026
GROUP BY parentproject
ORDER BY archive_table, parentproject;

-- Expected: All rows should show month_count=4 and PASS
```

### 4.2 Animal Requirements Rollup

```sql
-- Verify animal counts match between fps and archive
SELECT 
  'fps' as source,
  parentproject,
  COUNT(*) as animal_req_count,
  SUM(unitsrequired) as total_units
FROM fps.tblanimalreq
WHERE fpsyear=2026
GROUP BY parentproject
UNION ALL
SELECT 
  'archive' as source,
  parentproject,
  COUNT(*) as animal_req_count,
  SUM(unitsrequired) as total_units
FROM mabarchive.my_tblanimalreq
WHERE year=2026
GROUP BY parentproject
ORDER BY parentproject, source;

-- Expected:
-- Source | parentproject  | animal_req_count | total_units
-- fps    | P100-BASIC     | 2                | 80
-- archive| P100-BASIC     | 2                | 80
-- fps    | P200-MULTI-A   | 3                | 220
-- archive| P200-MULTI-A   | 3                | 220
-- ... (all should match)
```

---

## Phase 5: Master Data Consistency

### 5.1 Reference Data Population

```sql
-- Verify master lookups are present in archive
SELECT COUNT(*) as program_count FROM mabarchive.my_tlkpprogram WHERE year=2026;
-- Expected: 3

SELECT COUNT(*) as workgroup_count FROM mabarchive.my_workgroup WHERE year=2026;
-- Expected: 3

SELECT COUNT(*) as animal_count FROM mabarchive.my_tblanimals WHERE year=2026;
-- Expected: 4

SELECT COUNT(*) as employee_count FROM mabarchive.my_staff WHERE year=2026;
-- Expected: 4

SELECT COUNT(*) as test_req_count FROM mabarchive.my_tlkptestreqmt WHERE year=2026;
-- Expected: 3
```

### 5.2 Orphaned Reference Check

```sql
-- Verify no orphaned project references
SELECT DISTINCT parentproject FROM mabarchive.my_timecostcalcs WHERE year=2026
EXCEPT
SELECT DISTINCT parentproject FROM mabarchive.my_tlkpproject WHERE year=2026;

-- Expected: (no rows returned = no orphans)

-- Verify no orphaned workgroup references
SELECT DISTINCT workgroup FROM mabarchive.my_workgroupgrade WHERE year=2026
EXCEPT
SELECT DISTINCT workgroup FROM mabarchive.my_workgroup WHERE year=2026;

-- Expected: (no rows returned = no orphans)
```

---

## Phase 6: Total Calculations Validation

### 6.1 Archive Total Cost Verification

```sql
-- Verify fpsyeartotals matches combined costs
SELECT 
  parentproject,
  (SELECT totalcosts FROM mabarchive.my_fpsyeartotals 
   WHERE year=2026 AND parentproject=a.parentproject) as archived_total,
  SUM(amount::numeric) as additional_costs
FROM mabarchive.my_tbladditionalcosts a
WHERE year=2026
GROUP BY parentproject;

-- Expected (Scenario 3 example):
-- P300-COMPLEX | 61700 | 10000
-- (61700 includes: 10000 additional + 40700 staff + ~6000 animal + ~5000 test)
```

### 6.2 Test Output Totals

```sql
-- Verify test performance totals for Scenario 3
SELECT 
  parentproject,
  SUM(testsperformed) as total_tests_performed,
  COUNT(*) as month_count
FROM mabarchive.my_monthlyoutput
WHERE year=2026 AND parentproject='P300-COMPLEX'
GROUP BY parentproject;

-- Expected:
-- P300-COMPLEX | 1630 | 4

-- Compare with fps
SELECT 
  parentproject,
  SUM(testsperformed) as total_tests_performed,
  COUNT(*) as month_count
FROM fps.monthlyoutput
WHERE fpsyear=2026 AND parentproject='P300-COMPLEX'
GROUP BY parentproject;

-- Expected (should match):
-- P300-COMPLEX | 1630 | 4
```

---

## Phase 7: Idempotency Test

### 7.1 Re-run Job and Validate No Duplication

```sql
-- Before re-run, count total archive rows for 2026
SELECT COUNT(*) as total_rows_before FROM mabarchive.my_fpsyeartotals WHERE year=2026;
-- Example: 4 rows

-- [RUN JOB AGAIN]

-- After re-run, count again
SELECT COUNT(*) as total_rows_after FROM mabarchive.my_fpsyeartotals WHERE year=2026;
-- Expected: 4 (same count, no duplication)

-- Verify costs are identical
SELECT 
  parentproject,
  totalcosts
FROM mabarchive.my_fpsyeartotals
WHERE year=2026
ORDER BY parentproject;

-- Expected: Same rows as before re-run (sorted for comparison)
```

---

## Phase 8: Comprehensive Summary Report

Run this query to generate a single validation summary:

```sql
WITH scenario_summary AS (
  SELECT 
    'Scenario 1: Single Project' as scenario,
    'P100-BASIC' as project,
    (SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026 AND parentproject='P100-BASIC') as total_rows,
    (SELECT SUM(staffcost::numeric) FROM mabarchive.my_timecostcalcs WHERE year=2026 AND parentproject='P100-BASIC') as staff_costs,
    (SELECT SUM(amount::numeric) FROM mabarchive.my_tbladditionalcosts WHERE year=2026 AND parentproject='P100-BASIC') as add_costs,
    (SELECT SUM(testsperformed) FROM mabarchive.my_monthlyoutput WHERE year=2026 AND parentproject='P100-BASIC') as tests
  UNION ALL
  SELECT 
    'Scenario 2A: Multi Project',
    'P200-MULTI-A',
    (SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026 AND parentproject='P200-MULTI-A'),
    (SELECT SUM(staffcost::numeric) FROM mabarchive.my_timecostcalcs WHERE year=2026 AND parentproject='P200-MULTI-A'),
    (SELECT SUM(amount::numeric) FROM mabarchive.my_tbladditionalcosts WHERE year=2026 AND parentproject='P200-MULTI-A'),
    (SELECT SUM(testsperformed) FROM mabarchive.my_monthlyoutput WHERE year=2026 AND parentproject='P200-MULTI-A')
  UNION ALL
  SELECT 
    'Scenario 2B: Multi Project',
    'P200-MULTI-B',
    (SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026 AND parentproject='P200-MULTI-B'),
    (SELECT SUM(staffcost::numeric) FROM mabarchive.my_timecostcalcs WHERE year=2026 AND parentproject='P200-MULTI-B'),
    (SELECT SUM(amount::numeric) FROM mabarchive.my_tbladditionalcosts WHERE year=2026 AND parentproject='P200-MULTI-B'),
    (SELECT SUM(testsperformed) FROM mabarchive.my_monthlyoutput WHERE year=2026 AND parentproject='P200-MULTI-B')
  UNION ALL
  SELECT 
    'Scenario 3: Complex Project',
    'P300-COMPLEX',
    (SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026 AND parentproject='P300-COMPLEX'),
    (SELECT SUM(staffcost::numeric) FROM mabarchive.my_timecostcalcs WHERE year=2026 AND parentproject='P300-COMPLEX'),
    (SELECT SUM(amount::numeric) FROM mabarchive.my_tbladditionalcosts WHERE year=2026 AND parentproject='P300-COMPLEX'),
    (SELECT SUM(testsperformed) FROM mabarchive.my_monthlyoutput WHERE year=2026 AND parentproject='P300-COMPLEX')
)
SELECT 
  scenario,
  project,
  COALESCE(total_rows, 0) as archive_rows,
  COALESCE(staff_costs, 0)::money as staff_costs,
  COALESCE(add_costs, 0)::money as additional_costs,
  COALESCE(tests, 0) as tests_performed,
  CASE 
    WHEN staff_costs > 0 THEN 'OK'
    ELSE 'WARN: No data'
  END as status
FROM scenario_summary
ORDER BY scenario, project;
```

---

## Test Execution Checklist

- [ ] Phase 1: Data Load Verification (all counts match expected)
- [ ] Phase 2: Archive Post-Job Validation (all tables populated)
- [ ] Phase 3: Cost Calculation Validation (fps vs archive match)
- [ ] Phase 4: Detail Row Count Validation (monthly records complete)
- [ ] Phase 5: Master Data Consistency (all lookups present)
- [ ] Phase 6: Total Calculations Validation (rolled-up totals correct)
- [ ] Phase 7: Idempotency Test (re-run produces same results)
- [ ] Phase 8: Summary Report (all scenarios pass)

---

## Pass/Fail Criteria

**TEST PASSES** if:
✅ All archive tables have >0 rows for year 2026
✅ Staff costs match between fps.timecostcalcs and mabarchive.my_timecostcalcs
✅ Additional costs match between fps.tbladditionalcosts and mabarchive.my_tbladditionalcosts
✅ Monthly counts = 4 for all transaction tables
✅ No orphaned foreign key references in archive
✅ Re-run produces identical counts (idempotent)
✅ Scenario 1: 4 projects present in archive
✅ All 24 loaders populated data to archive

**TEST FAILS** if:
❌ Any archive table for year 2026 is empty
❌ Cost mismatch >1% between fps and archive
❌ Monthly counts != 4 for any transaction table
❌ Orphaned references found in archive
❌ Re-run produces duplicate rows
❌ Master data missing or inconsistent
❌ Any validation query returns unexpected row counts

---

## Deviation Documentation

For any test failures, document:
1. **Test Name**: Which phase and checkpoint failed
2. **Expected Value**: What should have happened (per TESTDATA-EXPECTED-RESULTS.md)
3. **Actual Value**: What actually happened (from validation query)
4. **Difference**: Expected - Actual
5. **Severity**: Critical (blocks archive) vs. Minor (informational)
6. **Root Cause Hypothesis**: What might have caused it
7. **Investigation Query**: SQL to drill down deeper

Example template:

```
DEVIATION: Scenario 1 Staff Costs Mismatch
Expected: 12700
Actual:   12650
Difference: -50
Severity: Minor
Root Cause Hypothesis: Rounding issue in cost calculation?
Investigation Query: SELECT staffcost, ROW_NUMBER() OVER (ORDER BY costmonth)
                     FROM fps.timecostcalcs WHERE parentproject='P100-BASIC'
```

---

End of Test Validation Procedures
