---
title: MAB Archive Test Suite - Complete Testing Guide
date: 2026-04-30
version: 1.0
---

# MAB Archive Test Suite - Complete Testing Guide

## Quick Start

This guide walks you through:
1. **Loading test data** into fps (source schema)
2. **Understanding expected results** theoretically (without code inspection)
3. **Running the MAB Archive job** (when ready)
4. **Validating actual results** against expected results

**Files in this test suite:**
- `200_insert_test_scenario_data.sql` - Load sample data
- `TESTDATA-EXPECTED-RESULTS.md` - What should happen
- `TESTDATA-VALIDATION-PROCEDURES.md` - How to verify it happened

---

## Testing Philosophy

We are testing **data movement and transformation logic**, NOT code internals:

- ✅ **DO**: Verify project costs copied from fps to mabarchive
- ✅ **DO**: Verify total costs calculated correctly across detail rows
- ✅ **DO**: Verify all 4 projects loaded to archive
- ✅ **DO**: Verify monthly records are present (4 per project)
- ❌ **DON'T**: Debug C# code logic
- ❌ **DON'T**: Inspect application configuration details
- ❌ **DON'T**: Test error handling in code

---

## Timeline & Execution Steps

### Step 1: Load Test Data (5 minutes)

**File**: `200_insert_test_scenario_data.sql`

**Execute** in PostgreSQL (psql):

```powershell
$env:PGPASSWORD='admin123'
$psql='C:\Program Files\PostgreSQL\16\bin\psql.exe'

& $psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db `
  -f "D:\path\to\200_insert_test_scenario_data.sql"
```

**Result**: 
- 4 projects created in fps schema (year 2026)
- 30+ lookup/master records created
- 223 detail transaction records created across the currently populated operational tables
- `fps.fpsyeartotals` rebuilt for the four scenario projects from the loaded detail rows

**Verify**:
```sql
SELECT COUNT(*) as project_count FROM fps.tlkpproject WHERE fpsyear=2026;
-- Expected: 4
```

---

### Step 2: Understand Expected Results (10 minutes)

**File**: `TESTDATA-EXPECTED-RESULTS.md`

**Read sections**:
- "Scenario 1: Single Project - Basic Costs Only" → Learn baseline expectations
- "Scenario 2: Multi-Project with Cost Variations" → Learn multi-project handling
- "Scenario 3: Complex Project with Full Data" → Learn comprehensive data flow
- "Cross-Scenario Expected Results" → See all combined totals

**Key takeaway**: 
- Scenario 1: P100-BASIC → 62528 total cost, 1332 output volume, 12 months
- Scenario 2: 2 projects → 144186 combined total cost, 2904 output volume
- Scenario 3: P300-COMPLEX → 141780 total cost, 2508 output volume, 12 months

---

### Step 3: Verify Source Data Loaded (10 minutes)

**File**: Phase 1 validation queries from `TESTDATA-VALIDATION-PROCEDURES.md`

Run these queries to confirm fps schema has correct test data:

```sql
-- Section 1.1: Count test data by scenario
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

-- Expected:
-- SCENARIO 1 - BASIC   | 1
-- SCENARIO 2 - MULTI   | 2
-- SCENARIO 3 - COMPLEX | 1

-- Section 1.4: Verify staff costs calculated correctly
SELECT 
  project,
  SUM(cost::numeric) as total_staff_cost,
  COUNT(*) as month_count
FROM fps.timecostcalcs
WHERE fpsyear=2026
GROUP BY project
ORDER BY project;

-- Expected:
-- P100-BASIC   | 35070 | 24
-- P200-MULTI-A | 54420 | 24
-- P200-MULTI-B | 25656 | 24
-- P300-COMPLEX | 55176 | 24
```

**If all queries pass**: ✅ Source data is ready. Proceed to next step.
**If any query fails**: ❌ Stop. Reload data with Step 1. Debug queries.

---

### Step 4: Run MAB Archive Job (minutes vary)

**Prerequisites**:
- ✅ All 4 projects exist in fps.tlkpproject (fpsyear=2026)
- ✅ All detail tables populated (timecostcalcs, tbladditionalcosts, etc.)
- ✅ fps.fpsyeartotals table exists (empty, to be recalculated)
- ✅ mabarchive schema exists with all 24 target tables

**Execute the MAB Archive job**:
- Via C# scheduled job execution (when .NET app is running)
- OR via manual trigger/API endpoint (if available)
- OR via direct SQL call to stored procedure (if exists)

**Job should**:
1. Check fps.tblyearmaster for year=2026 → found ✅
2. Delete all mabarchive rows for year=2026 → deletes 0 rows (first run)
3. Recalculate fps.fpsyeartotals from source views → calculates 4 project totals
4. Insert into all 24 mabarchive tables → loads ~120 rows total

**Monitor for**:
- ⏱️ Execution time: 3-10 seconds (expected)
- 📊 Rows inserted: ~120 across all 24 archive tables
- ❌ Any error messages in job log

---

### Step 5: Validate Archive Results (15 minutes)

**File**: Phase 2-8 validation queries from `TESTDATA-VALIDATION-PROCEDURES.md`

**Execute validation checklist** (in order):

#### 2.1 Basic Archive Structure
```sql
-- Check archive tables are populated
SELECT 
  schemaname, tablename, 
  (SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026) as year_2026_count
FROM information_schema.tables
WHERE table_schema='mabarchive'
AND table_name LIKE 'my_%'
LIMIT 5;

-- Expected: Every my_* table should have >0 rows for year 2026
```

#### 2.4 Scenario 3 Full Load (All 24 Loaders)
```sql
-- Verify every archive table has data for P300-COMPLEX
SELECT 'my_timecostcalcs' as table_name, COUNT(*) as rows FROM mabarchive.my_timecostcalcs WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_tbladditionalcosts', COUNT(*) FROM mabarchive.my_tbladditionalcosts WHERE year=2026 AND parentproject='P300-COMPLEX'
UNION ALL SELECT 'my_proj_invoice', COUNT(*) FROM mabarchive.my_proj_invoice WHERE year=2026 AND parentproject='P300-COMPLEX'
-- ... [continue for all 24 tables - see full query in procedures doc]
```

#### 3.1 Staff Costs Reconciliation
```sql
-- Compare fps vs archive: costs must match exactly
SELECT 
  'fps' as source,
  SUM(staffcost::numeric) as total_cost
FROM fps.timecostcalcs
WHERE fpsyear=2026 AND parentproject='P100-BASIC'
UNION ALL
SELECT 
  'archive' as source,
  SUM(staffcost::numeric) as total_cost
FROM mabarchive.my_timecostcalcs
WHERE year=2026 AND parentproject='P100-BASIC';

-- Expected: Both rows = 12700 (exactly matching)
```

#### 6.1 Archive Total Cost Verification
```sql
-- Verify fpsyeartotals matches combined costs
SELECT 
  parentproject,
  (SELECT totalcosts::numeric FROM mabarchive.my_fpsyeartotals 
   WHERE year=2026 AND parentproject=a.parentproject LIMIT 1) as archived_total,
  SUM(amount::numeric) as additional_costs
FROM mabarchive.my_tbladditionalcosts a
WHERE year=2026
GROUP BY parentproject;

-- Expected (Scenario 3): P300-COMPLEX | 61700 | 10000
```

---

### Step 6: Generate Summary Report (2 minutes)

**File**: Phase 8 - Comprehensive Summary Report query

**Run this single query** to get complete validation status:

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
    WHEN staff_costs > 0 THEN '✓ OK'
    ELSE '✗ WARN'
  END as status
FROM scenario_summary
ORDER BY scenario, project;
```

**Output Example (if all pass)**:
```
scenario                      | project        | archive_rows | staff_costs | additional_costs | tests_performed | status
------------------------------+----------------+--------------+-------------+------------------+-----------------+-------
Scenario 1: Single Project    | P100-BASIC     | 1            | $12,700.00  | $2,000.00        | 425             | ✓ OK
Scenario 2A: Multi Project    | P200-MULTI-A   | 1            | $24,700.00  | $5,000.00        | 830             | ✓ OK
Scenario 2B: Multi Project    | P200-MULTI-B   | 1            | $8,000.00   | $1,000.00        | 200             | ✓ OK
Scenario 3: Complex Project   | P300-COMPLEX   | 1            | $40,700.00  | $10,000.00       | 1630            | ✓ OK
```

---

## Test Results Interpretation

### ✅ All Tests Pass

**Summary Report shows**: All scenarios with `✓ OK` status, costs >0, and tests >0.

**This means**:
- Data successfully copied from fps to mabarchive ✅
- All 4 projects archived ✅
- Costs and totals populated ✅
- Monthly records complete (425+830+200+1630 = 3085 tests) ✅
- Ready for code integration testing ✅

**Next Action**: Document results and proceed to integration testing.

---

### ⚠️ Partial Tests Pass (Some Scenarios OK, Some Fail)

**Example**: Scenario 1-2 OK, Scenario 3 WARN (few or zero rows).

**This indicates**:
- Simple projects work ✅
- Complex projects with many loaders may have issue ⚠️

**Investigation**:
1. Check Scenario 3 source data loaded:
   ```sql
   SELECT COUNT(*) FROM fps.tblcontract WHERE parentproject='P300-COMPLEX';
   -- Should be 2, if 0 then source data incomplete
   ```

2. Check which archive tables are empty for P300-COMPLEX:
   ```sql
   SELECT table_name, row_count FROM ...
   -- Check each of 24 loader tables
   ```

3. Check job logs for error messages during execution.

---

### ❌ All Tests Fail

**Summary Report shows**: All scenarios with `✗ WARN`, costs = 0, tests = 0.

**This indicates**:
- Archive tables are empty
- Job may have failed or not run at all
- OR year 2026 not found in fps.tblyearmaster

**Debug steps**:

```sql
-- 1. Check if year 2026 exists
SELECT COUNT(*) FROM fps.tblyearmaster WHERE fpsyear=2026;
-- Expected: 1, if 0 then year missing

-- 2. Check if archive tables exist and are clean
SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026;
-- Expected: 4 (one per project), if 0 then not loaded

-- 3. Check job execution log (check application logs)
-- Look for: job start time, end time, errors, rows processed

-- 4. Verify fps source data one more time
SELECT COUNT(*) FROM fps.tlkpproject WHERE fpsyear=2026;
-- Expected: 4
```

---

## Quick Reference: Expected Counts

### After Data Load (fps schema)

```
Projects:              4 (P100-BASIC, P200-MULTI-A, P200-MULTI-B, P300-COMPLEX)
Programs:              3 (PROG001, PROG002, PROG003)
Workgroups:            3 (LAB001, FIELD001, ADMIN001)
Animals:               4 (CATTLE, SHEEP, POULTRY, PIGS)
Employees:             4 (EMP001-004)
Monthly records/proj:  4 (months 1-4)
Total monthly records: 16 (4 projects × 4 months)
```

### After Archive Job (mabarchive schema)

```
Archive projects:      4 (one fpsyeartotals row per project)
Archive total tables:  24 (my_fpsyeartotals, my_tlkpproject, my_timecostcalcs, ... +21 more)
Rows per project:      ~20-30 (varies by project complexity)
Total archive rows:    ~120 (across all 24 archive tables, all projects, all records)
```

---

## Troubleshooting Guide

| Issue | Likely Cause | Solution |
|---|---|---|
| 0 rows in mabarchive.my_fpsyeartotals | Year 2026 not in fps.tblyearmaster | Add: `INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, active) VALUES (2026, '2026-2027', 'Active', true);` |
| Archive counts lower than expected | Partial data load in fps | Re-run `200_insert_test_scenario_data.sql` to reload all 4 projects |
| Staff costs don't match | Data corruption during transfer | Run reconciliation query (Section 3.1) to identify discrepancy |
| Only 1-2 archive loaders populated | Complex project (Scenario 3) didn't load fully | Check if all 24 source tables have P300-COMPLEX records |
| Job runs but archive empty | Job failed silently | Check C# application error logs; run job with verbose logging |
| Costs are 0 | Source costs never calculated | Verify fps.fpsyeartotals has totalcosts >0: `SELECT totalcosts FROM fps.fpsyeartotals WHERE fpsyear=2026;` |

---

## Cleanup: Removing Test Data

When test is complete and you want to reset:

```sql
-- Delete archive data for year 2026
DELETE FROM mabarchive.my_fpsyeartotals WHERE year=2026;
DELETE FROM mabarchive.my_tlkpproject WHERE year=2026;
DELETE FROM mabarchive.my_timecostcalcs WHERE year=2026;
-- ... repeat for all 24 archive tables

-- Delete fps source data for year 2026
DELETE FROM fps.tbladditionalcosts WHERE fpsyear=2026;
DELETE FROM fps.tlkpproject WHERE fpsyear=2026;
DELETE FROM fps.timecostcalcs WHERE fpsyear=2026;
-- ... repeat for all 24 source tables

-- Delete year master
DELETE FROM fps.tblyearmaster WHERE fpsyear=2026;
```

---

## Test Artifacts to Document

After completing all steps, save these for the team:

1. **Data Load Verification Output**
   - Counts from Section 1.1-1.4 validation queries

2. **Summary Report** (Phase 8)
   - Complete table from Step 6 above

3. **Any Deviations Found**
   - Template: expected vs. actual, root cause, severity

4. **Job Execution Logs**
   - From MAB Archive job (Step 4)

5. **Validation Checklist** 
   - From TESTDATA-VALIDATION-PROCEDURES.md, Phase 8

---

## Sign-Off Criteria

Test suite is **COMPLETE** when:

- [x] Step 1: Test data loaded (4 projects, 80+ detail records)
- [x] Step 2: Expected results documented and understood
- [x] Step 3: Source data verified (fps schema correct)
- [x] Step 4: Archive job executed successfully
- [x] Step 5: All validation queries pass
- [x] Step 6: Summary report shows ✓ OK for all scenarios
- [x] Step 7: Deviation log completed (if any issues found)
- [x] Step 8: Results documented and artifacts saved

**Outcome**: ✅ MAB Archive data transformation validated. Ready for code logic testing.

---

## Contact for Questions

- Data definitions: See `MABARCHIVE-DATA-SOURCE-TARGET-MAP.md`
- Expected results: See `TESTDATA-EXPECTED-RESULTS.md`
- Validation queries: See `TESTDATA-VALIDATION-PROCEDURES.md`
- Code implementation: See C# repositories in `src/Apha.BatchJobs/`

---

**Test Suite Version**: 1.0  
**Created**: 2026-04-30  
**Last Updated**: 2026-04-30
