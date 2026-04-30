---
title: MAB Archive Test Scenarios - Expected Results
date: 2026-04-30
version: 1.0
---

# MAB Archive Test Scenarios - Expected Results

## Overview

This document defines three test scenarios for the MAB Archive scheduled job and documents the expected results based on the source-to-target mapping rules.

**Key Testing Principle**: We are NOT testing code logic—we are establishing what data should theoretically flow from fps source tables to mabarchive target tables, and what totals should be calculated.

---

## Terminology

- **fps schema**: Source of truth. Contains operational data updated daily. Contains 24 source tables + 4 totals views.
- **mabarchive schema**: Archive/reporting schema. Contains 24 target tables that are refreshed nightly.
- **fpsyear**: Year key in fps tables (e.g., 2026).
- **year**: Year key in mabarchive tables (e.g., 2026).
- **Nightly Job Process**:
  1. Delete all mabarchive rows for year 2026
  2. Recalculate fps.fpsyeartotals from source detail tables
  3. Insert recalculated data into mabarchive target tables

---

## Test Data Configuration

### Year
- **FPS Year**: 2026
- **Archive Year**: 2026

## Current Source-Side Sample Story (Authoritative)

This is the current local sample loaded by `200_insert_test_scenario_data.sql`.

- The script creates **4 test projects** plus the separate `P100-BASELINE` day-change row.
- The enriched source sample loads **223 detail rows** across `timecostcalcs`, `monthlyoutput`, `monthlytime`, `tbladditionalcosts`, `tblanimalreq`, and `tblstaffjob`.
- The script then rebuilds `fps.fpsyeartotals` for the 4 scenario projects using transparent source-side formulas:
  - `totaladditionalcosts = SUM(tbladditionalcosts.itemcost)`
  - `totalstaffcosts = SUM(timecostcalcs.cost)`
  - `totalanimalcosts = SUM(numberofanimals * numberofdays * tblanimals.dailyrate)`
  - `totaltestcosts = SUM(monthlyoutput.volume * tlkptestcapability.unitcost)`
  - `totalcosts = totaladditionalcosts + totalstaffcosts + totalanimalcosts + totaltestcosts`

Use the figures in this section as the authoritative local data story. The older scenario narratives below are legacy planning examples and should not be treated as the current source-of-truth for the enriched 12-month sample.

### Current 2026 Sample at a Glance

| Project | Additional Cost | Staff Cost | Animal Cost | Test Cost | Total Cost | Time Rows | Output Rows | Time Rows (monthlytime) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| `P100-BASIC` | 2150 | 35070 | 5328 | 19980 | 62528 | 24 | 12 | 12 |
| `P200-MULTI-A` | 4700 | 54420 | 12900 | 33150 | 105170 | 24 | 12 | 12 |
| `P200-MULTI-B` | 1000 | 25656 | 2820 | 9540 | 39016 | 24 | 12 | 12 |
| `P300-COMPLEX` | 8700 | 55176 | 27744 | 50160 | 141780 | 24 | 12 | 12 |

### Current Per-Project Detail Shape

| Project | Additional Cost Rows | Animal Rows | Staff Assignment Rows | Planned Hours | Total Output Volume | Total Monthly Hours |
|---|---:|---:|---:|---:|---:|---:|
| `P100-BASIC` | 3 | 2 | 2 | 540 | 1332 | 1338 |
| `P200-MULTI-A` | 3 | 3 | 2 | 1180 | 1950 | 1578 |
| `P200-MULTI-B` | 3 | 2 | 2 | 620 | 954 | 966 |
| `P300-COMPLEX` | 3 | 4 | 2 | 1200 | 2508 | 1878 |

### Test Scenarios

| Scenario | Project | FPS Tables Used | Volume | Purpose |
|----------|---------|-----------------|--------|---------|
| **1: Basic** | P100-BASIC | `tlkpproject`, `tbladditionalcosts`, `tblanimalreq`, `tblstaffjob`, `timecostcalcs`, `monthlyoutput`, `monthlytime` | 55 detail rows | Single-project baseline with 12 months of activity |
| **2: Multi-Project** | P200-MULTI-A, P200-MULTI-B | Same operational tables as Scenario 1 across two projects | 110 detail rows | Multi-project consistency and mixed workgroup coverage |
| **3: Complex** | P300-COMPLEX | Same operational tables with the highest row volume and cost totals | 58 detail rows | Highest-cost source sample before archive execution |

---

## Scenario 1: Single Project - Basic Costs Only

### 1a. Source Data (fps schema)

#### fps.tlkpproject
```
parentproject      = 'P100-BASIC'
projecttitle       = 'Basic Disease Monitoring - Scenario 1'
program            = 'PROG001'
customer           = 'CUST001'
manager            = 'Manager1'
transferincome     = 10000
custincome         = 5000
projectstatus      = 'Active'
disease            = 'TB'
fpsyear            = 2026
```

#### fps.tbladditionalcosts
| parentproject | costtype | amount | fpsyear |
|---|---|---|---|
| P100-BASIC | EQUIPMENT | 2000 | 2026 |

**Total Additional Costs** = 2000

#### fps.tblanimalreq
| parentproject | animal | unitsrequired | fpsyear |
|---|---|---|---|
| P100-BASIC | CATTLE | 50 | 2026 |
| P100-BASIC | SHEEP | 30 | 2026 |

#### fps.tblstaffjob
| parentproject | workgroup | hourspercent | fpsyear |
|---|---|---|---|
| P100-BASIC | LAB001 | 500 | 2026 |

#### fps.timecostcalcs (Monthly staff costs)
| parentproject | costmonth | staffcost | fpsyear |
|---|---|---|---|
| P100-BASIC | 1 | 3000 | 2026 |
| P100-BASIC | 2 | 3500 | 2026 |
| P100-BASIC | 3 | 3200 | 2026 |
| P100-BASIC | 4 | 3000 | 2026 |

**Total Staff Costs** = 3000 + 3500 + 3200 + 3000 = 12700

#### fps.monthlyoutput (Test performance)
| parentproject | outputmonth | testsperformed | fpsyear |
|---|---|---|---|
| P100-BASIC | 1 | 100 | 2026 |
| P100-BASIC | 2 | 120 | 2026 |
| P100-BASIC | 3 | 110 | 2026 |
| P100-BASIC | 4 | 95 | 2026 |

**Total Tests Performed (4-month run)** = 100 + 120 + 110 + 95 = 425 tests

#### fps.monthlytime (Hours tracking)
| parentproject | timemonth | totalhours | fpsyear |
|---|---|---|---|
| P100-BASIC | 1 | 120 | 2026 |
| P100-BASIC | 2 | 130 | 2026 |
| P100-BASIC | 3 | 125 | 2026 |
| P100-BASIC | 4 | 120 | 2026 |

**Total Hours (4-month run)** = 120 + 130 + 125 + 120 = 495 hours

### 1b. Calculated Totals (fps.fpsyeartotals for P100-BASIC, year 2026)

The job recalculates totals from source views:
- **qrytotaladditionalcosts**: SUM(tbladditionalcosts.amount) for year = 2000
- **qrytotalanimalcosts**: LOOKUP animal requirement costs from staffing = (estimated 1000-2000)
- **qrytotalstaffcosts**: SUM(timecostcalcs.staffcost) for year = 12700
- **qrytotaltestcosts**: LOOKUP test performance costs = (estimated 2000-3000)

#### fps.fpsyeartotals (Rebuilt)
```
fpsyear           = 2026
parentproject     = 'P100-BASIC'
additionalcosts   = 2000
staffcosts        = 12700
animalcosts       = ~1500 (estimated from staffing)
testcosts         = ~2500 (estimated from output)
totalcosts        = ~18700 (additionalcosts + staffcosts + animalcosts + testcosts)
```

### 1c. Expected Archive Output (mabarchive)

After nightly job runs:

#### mabarchive.my_fpsyeartotals
```
year              = 2026
parentproject     = 'P100-BASIC'
additionalcosts   = 2000
staffcosts        = 12700
animalcosts       = ~1500
testcosts         = ~2500
totalcosts        = ~18700
```

#### mabarchive.my_tlkpproject
```
year              = 2026
parentproject     = 'P100-BASIC'
projecttitle      = 'Basic Disease Monitoring - Scenario 1'
program           = 'PROG001'
[... all columns from fps.tlkpproject ...]
```

#### mabarchive.my_tbladditionalcosts
```
year              = 2026
parentproject     = 'P100-BASIC'
costtype          = 'EQUIPMENT'
amount            = 2000
[... rest of columns ...]
```

#### mabarchive.my_tblanimalreq
```
year              = 2026
parentproject     = 'P100-BASIC'
animal            = 'CATTLE'
unitsrequired     = 50
```
(and one more row for SHEEP)

#### mabarchive.my_tblstaffjob
```
year              = 2026
parentproject     = 'P100-BASIC'
workgroup         = 'LAB001'
hourspercent      = 500
```

#### mabarchive.my_timecostcalcs
4 rows (one per month), copied from fps.timecostcalcs with year stamped.

#### mabarchive.my_monthlyoutput
4 rows (one per month), copied from fps.monthlyoutput with year stamped.

#### mabarchive.my_monthlytime
4 rows (one per month), copied from fps.monthlytime with year stamped.

### 1d. Validation Checkpoints

| Checkpoint | Expected Count | Validation Query |
|---|---|---|
| my_fpsyeartotals rows for P100-BASIC | 1 | `SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year=2026 AND parentproject='P100-BASIC'` |
| my_tlkpproject rows for P100-BASIC | 1 | `SELECT COUNT(*) FROM mabarchive.my_tlkpproject WHERE year=2026 AND parentproject='P100-BASIC'` |
| my_timecostcalcs rows for P100-BASIC | 4 | `SELECT COUNT(*) FROM mabarchive.my_timecostcalcs WHERE year=2026 AND parentproject='P100-BASIC'` |
| Total archive cost for P100-BASIC | 18700 | `SELECT SUM(totalcosts) FROM mabarchive.my_fpsyeartotals WHERE year=2026 AND parentproject='P100-BASIC'` |
| All P100-BASIC costs (sum across 3 months) | 12700 | `SELECT SUM(staffcost) FROM mabarchive.my_timecostcalcs WHERE year=2026 AND parentproject='P100-BASIC'` |
| Total tests performed P100-BASIC | 425 | `SELECT SUM(testsperformed) FROM mabarchive.my_monthlyoutput WHERE year=2026 AND parentproject='P100-BASIC'` |
| Total hours worked P100-BASIC | 495 | `SELECT SUM(totalhours) FROM mabarchive.my_monthlytime WHERE year=2026 AND parentproject='P100-BASIC'` |

---

## Scenario 2: Multi-Project with Cost Variations

### 2a. Source Data Summary (fps schema)

#### Project A: P200-MULTI-A
```
Additional Costs:    3000 (MATERIALS) + 2000 (EQUIPMENT) = 5000
Staff Costs/Month:   6000 + 6500 + 6200 + 6000 = 24700 total
Animal Requirements: CATTLE (100), SHEEP (80), PIGS (40)
Monthly Tests:       200 + 220 + 210 + 200 = 830 tests
Monthly Hours:       240 + 250 + 245 + 240 = 975 hours
```

#### Project B: P200-MULTI-B
```
Additional Costs:    1000 (TRANSPORT) = 1000
Staff Costs/Month:   2000 + 2100 + 2000 + 1900 = 8000 total
Animal Requirements: POULTRY (500)
Monthly Tests:       50 + 55 + 50 + 45 = 200 tests
Monthly Hours:       80 + 85 + 80 + 75 = 320 hours
```

### 2b. Calculated Totals (fps.fpsyeartotals)

#### For P200-MULTI-A
```
additionalcosts   = 5000
staffcosts        = 24700
animalcosts       = ~3000 (estimated: high animal volume)
testcosts         = ~4000 (estimated: high test volume)
totalcosts        = ~36700
```

#### For P200-MULTI-B
```
additionalcosts   = 1000
staffcosts        = 8000
animalcosts       = ~1500 (estimated: poultry only)
testcosts         = ~1000 (estimated: low volume)
totalcosts        = ~11500
```

### 2c. Expected Archive Output

#### Validation Checkpoints

| Item | Project A | Project B | Combined |
|---|---|---|---|
| **Rows in my_fpsyeartotals** | 1 | 1 | 2 |
| **Rows in my_timecostcalcs** | 4 | 4 | 8 |
| **Rows in my_monthlyoutput** | 4 | 4 | 8 |
| **Rows in my_monthlytime** | 4 | 4 | 8 |
| **Total staff costs (sum across months)** | 24700 | 8000 | 32700 |
| **Total tests performed** | 830 | 200 | 1030 |
| **Total hours tracked** | 975 | 320 | 1295 |
| **Total cost across both projects** | ~36700 | ~11500 | ~48200 |

### 2d. Key Validation

- **Deletion Rule Verification**: All 2025 data (if any) must be deleted before 2026 is inserted.
- **No Data Leakage**: Only rows with fpsyear=2026 should exist in archive for year 2026.
- **Completeness**: All 8 loaders (timecostcalcs, monthlyoutput, monthlytime, etc.) must populate both projects.

---

## Scenario 3: Complex Project with Full Data

### 3a. Source Data Summary (fps schema)

#### Project: P300-COMPLEX (All 24 source tables populated)

```
Base Project:
  income            = 50000 (transfer) + 30000 (customer) = 80000
  
Contracts:
  CONTRACT-2026-001 = 50000
  CONTRACT-2026-002 = 25000
  TOTAL CONTRACT VALUE = 75000

Invoices (Billed):
  INV-001           = 15000
  INV-002           = 15000
  INV-003           = 10000
  TOTAL INVOICED    = 40000

Subcontracts:
  SUB-001           = 8000
  SUB-002           = 5000
  TOTAL SUBCONTRACTED = 13000

Additional Costs:
  MATERIALS         = 4000
  EQUIPMENT         = 3000
  TRANSPORT         = 2000
  TRAINING          = 1000
  TOTAL ADD'L COSTS = 10000

Animal Requirements:
  CATTLE            = 150 units
  SHEEP             = 100 units
  POULTRY           = 300 units
  PIGS              = 75 units
  TOTAL ANIMALS     = 625 units

Staff Allocation:
  LAB001            = 1200 hours
  FIELD001          = 600 hours
  ADMIN001          = 200 hours
  TOTAL STAFF HOURS = 2000 hours

Monthly Staff Costs:
  Month 1           = 10000
  Month 2           = 10500
  Month 3           = 10200
  Month 4           = 10000
  TOTAL STAFF COST  = 40700

Monthly Test Output:
  Month 1           = 400 tests
  Month 2           = 420 tests
  Month 3           = 410 tests
  Month 4           = 400 tests
  TOTAL TESTS       = 1630 tests

Monthly Time Tracking:
  Month 1           = 400 hours
  Month 2           = 420 hours
  Month 3           = 410 hours
  Month 4           = 400 hours
  TOTAL HOURS       = 1630 hours

Project Final Monthly Costs:
  Month 1           = 15000
  Month 2           = 15800
  Month 3           = 15500
  Month 4           = 15000
  TOTAL FINAL COST  = 61300
```

### 3b. Calculated Totals (fps.fpsyeartotals for P300-COMPLEX)

Based on source views:

```
fpsyear           = 2026
parentproject     = 'P300-COMPLEX'
additionalcosts   = 10000
staffcosts        = 40700
animalcosts       = ~6000 (estimated: 625 animals × avg cost/unit)
testcosts         = ~5000 (estimated: 1630 tests × avg cost/test)
totalcosts        = ~61700
```

### 3c. Expected Archive Rows

All 24 loaders should populate for this project:

| Loader # | Target Table | Expected Rows | Comments |
|---|---|---|---|
| 1 | my_tlkpprogram | 1 | PROG001 for year 2026 |
| 2 | g_tlkpproject | 1 | P300-COMPLEX (grouped) |
| 3 | my_tlkpproject | 1 | P300-COMPLEX project master |
| 4 | my_fpsyeartotals | 1 | Calculated totals for P300-COMPLEX |
| 5 | my_monthlyoutput | 4 | Month 1-4 test output |
| 6 | my_monthlytime | 4 | Month 1-4 time tracking |
| 7 | my_proj_invoice | 3 | INV-001, INV-002, INV-003 |
| 8 | my_proj_subcontract | 2 | SUB-001, SUB-002 |
| 9 | my_projectmonthfinal | 4 | Month 1-4 final costs |
| 10 | my_tbladditionalcosts | 4 | MATERIALS, EQUIPMENT, TRANSPORT, TRAINING |
| 11 | my_tblanimalreq | 4 | CATTLE, SHEEP, POULTRY, PIGS |
| 12 | my_tblcontract | 2 | CONTRACT-2026-001, CONTRACT-2026-002 |
| 13 | my_tblstaffjob | 3 | LAB001, FIELD001, ADMIN001 |
| 14 | my_timecostcalcs | 4 | Month 1-4 staff costs |
| 15 | my_tlkptestreqmt | 3 | TEST001, TEST002, TEST003 |
| 16 | tlkpyear | 1 | Current month indicator |
| 17 | my_workgroupgrade | 3 | LAB001, FIELD001, ADMIN001 grades |
| 18 | my_profitcentregrade | 3 | PC001, PC002, PC003 grades |
| 19 | my_tblprofitcentre | 3 | PC001, PC002, PC003 |
| 20 | my_testorproduct | 3 | DIAG001, VACC001, SURV001 |
| 21 | my_staff | 4 | Employee/workgroup join (4 employees) |
| 22 | my_workgroup | 3 | LAB001, FIELD001, ADMIN001 |
| 23 | my_tblanimals | 4 | CATTLE, SHEEP, POULTRY, PIGS |
| 24 | my_tlkpproject_all | 1 | P300-COMPLEX (current year refresh) |

### 3d. Comprehensive Validation Checkpoints

| Checkpoint | Expected Value | Validation Query |
|---|---|---|
| Total rows archived (all tables) | 77+ | `SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals UNION ALL ... [sum all my_* tables]` |
| Project cost P300-COMPLEX | 61700 | `SELECT totalcosts FROM mabarchive.my_fpsyeartotals WHERE parentproject='P300-COMPLEX'` |
| Total invoiced amount | 40000 | `SELECT SUM(invoiceamount) FROM mabarchive.my_proj_invoice WHERE parentproject='P300-COMPLEX'` |
| Total subcontract value | 13000 | `SELECT SUM(subcontractamount) FROM mabarchive.my_proj_subcontract WHERE parentproject='P300-COMPLEX'` |
| Total additional costs | 10000 | `SELECT SUM(amount) FROM mabarchive.my_tbladditionalcosts WHERE parentproject='P300-COMPLEX'` |
| Total staff cost (time costs) | 40700 | `SELECT SUM(staffcost) FROM mabarchive.my_timecostcalcs WHERE parentproject='P300-COMPLEX'` |
| Total tests performed | 1630 | `SELECT SUM(testsperformed) FROM mabarchive.my_monthlyoutput WHERE parentproject='P300-COMPLEX'` |
| Total hours tracked | 1630 | `SELECT SUM(totalhours) FROM mabarchive.my_monthlytime WHERE parentproject='P300-COMPLEX'` |
| Total final project costs (monthly sum) | 61300 | `SELECT SUM(finalcosts) FROM mabarchive.my_projectmonthfinal WHERE parentproject='P300-COMPLEX'` |
| Animal requirements count | 4 | `SELECT COUNT(*) FROM mabarchive.my_tblanimalreq WHERE parentproject='P300-COMPLEX'` |
| Total animals required | 625 | `SELECT SUM(unitsrequired) FROM mabarchive.my_tblanimalreq WHERE parentproject='P300-COMPLEX'` |
| Staff job assignments | 3 | `SELECT COUNT(*) FROM mabarchive.my_tblstaffjob WHERE parentproject='P300-COMPLEX'` |
| Staff records (employees) | 4 | `SELECT COUNT(*) FROM mabarchive.my_staff WHERE fpsyear=2026` |
| Workgroups archived | 3 | `SELECT COUNT(*) FROM mabarchive.my_workgroup WHERE fpsyear=2026` |

---

## Cross-Scenario Expected Results

### Combined Totals (All 3 Scenarios)

| Metric | Scenario 1 | Scenario 2A | Scenario 2B | Scenario 3 | COMBINED |
|---|---|---|---|---|---|
| Projects | 1 | 1 | 1 | 1 | 4 |
| Total archive cost | ~18700 | ~36700 | ~11500 | ~61700 | ~128600 |
| Staff costs | 12700 | 24700 | 8000 | 40700 | 86100 |
| Additional costs | 2000 | 5000 | 1000 | 10000 | 18000 |
| Tests performed | 425 | 830 | 200 | 1630 | 3085 |
| Hours tracked | 495 | 975 | 320 | 1630 | 3420 |
| Animals required | 80 | 220 | 500 | 625 | 1425 |
| Rows in my_fpsyeartotals | 1 | 2 | - | 1 | 4 |
| Rows in my_timecostcalcs | 4 | 8 | - | 4 | 16 |

### Master Data Consistency

After all scenarios load to archive:

| Master Table | Rows Expected |
|---|---|
| my_tlkpprogram | 3 (PROG001, PROG002, PROG003) |
| my_workgroup | 3 (LAB001, FIELD001, ADMIN001) |
| my_tblanimals | 4 (CATTLE, SHEEP, POULTRY, PIGS) |
| my_tblprofitcentre | 3 (PC001, PC002, PC003) |
| my_staff (employees in year 2026) | 4 (EMP001-004) |

---

## Deletion & Reload Scenario

### Test Case: Re-running the job (idempotency)

**Precondition**: All 3 scenarios have loaded once to archive.

**Job Run #2** (same year 2026):

1. Delete all archive rows for year=2026 from all 24 tables
2. Recalculate totals from fps source tables
3. Re-insert all data

**Expected Result**: 
- All counts remain identical (no duplicates, no orphans)
- Total costs remain same (recalculation deterministic)
- All validation checkpoints pass again
- No data loss

---

## Partial Refresh Scenario (January-April)

**Test Case**: Current month during Jan-Apr (only my_tlkpproject_all refreshed)

**Precondition**: Full load for year 2026 complete.

**Action**: Job runs with month in Jan-April range → partial refresh behavior.

**Expected**: 
- Only `my_tlkpproject_all` rows for year 2026 deleted and reloaded
- All 23 other archive tables remain untouched
- `my_tlkpproject_all` row count same or updated per current project list

---

## Data Integrity Checks

### Before Archive Runs (fps schema)

- [ ] `fps.tblyearmaster` has fpsyear=2026 record (prerequisite)
- [ ] All projects have fpsyear=2026 set
- [ ] Master lookups (programs, workgroups, animals, etc.) have fpsyear=2026
- [ ] Detail tables (costs, invoices, contracts) reference valid masters
- [ ] No orphaned foreign keys in fps schema

### After Archive Runs (mabarchive schema)

- [ ] All 24 target tables populated for year=2026
- [ ] No rows exist for year != 2026 from this run
- [ ] Sum of detail costs ≈ totals in fpsyeartotals (within rounding)
- [ ] No orphaned foreign keys in mabarchive schema
- [ ] All 4 project records present in my_fpsyeartotals
- [ ] Master tables (animals, programs, etc.) consistent across all 24 loaders

---

## Performance Baseline

When running full load with 4 projects across 24 loaders:

| Component | Expected Time |
|---|---|
| Delete year rows (24 tables) | <1 second |
| Recalculate fps.fpsyeartotals | 1-2 seconds |
| Insert all 24 archive tables | 2-3 seconds |
| **Total Job Duration** | **3-6 seconds** (expected) |

---

## Summary Table: All Expected Results

| Scenario | Project | Rows in Archive (24 tables) | Total Cost | Tests | Hours | Key Validation |
|---|---|---|---|---|---|---|
| 1 | P100-BASIC | ~20 | 18700 | 425 | 495 | Basic load complete, all 4 months |
| 2A | P200-MULTI-A | ~20 | 36700 | 830 | 975 | Multi-project A complete |
| 2B | P200-MULTI-B | ~20 | 11500 | 200 | 320 | Multi-project B complete |
| 3 | P300-COMPLEX | ~60+ | 61700 | 1630 | 1630 | All 24 loaders populate, full data |
| **COMBINED** | **4 projects** | **~120** | **~128600** | **3085** | **3420** | **Complete suite load validated** |

---

## Next Steps

1. **Execute** `200_insert_test_scenario_data.sql` to populate fps schema with test data.
2. **Verify** fps data matches these expected values in this document.
3. **Run** the MAB Archive job (when code is ready).
4. **Validate** archive rows against expected result tables above.
5. **Compare** actual vs. expected costs, counts, and totals.
6. **Document** any discrepancies in deviation report.
