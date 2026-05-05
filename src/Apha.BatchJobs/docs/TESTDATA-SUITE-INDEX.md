---
title: MAB Archive Test Suite - Complete Index
date: 2026-04-30
---

# MAB Archive Test Suite - Complete Index

## Overview

This test suite provides **comprehensive testing materials** for the MAB Archive scheduled job, focused on **data transformation validation** rather than code inspection.

**Key Concept**: We validate that data flows correctly from fps (source) to mabarchive (archive) and that totals are calculated as expected—WITHOUT testing C# code logic.

---

## Files in This Test Suite

### 1. Data Loading

**File**: `200_insert_test_scenario_data.sql`

**Purpose**: Populate fps schema with realistic test data across 3 scenarios

**Contains**:
- 4 test projects (P100-BASIC, P200-MULTI-A, P200-MULTI-B, P300-COMPLEX)
- Master lookup data (programs, workgroups, animals, employees, etc.)
- Transaction detail data across the operational tables that are present locally (`tbladditionalcosts`, `tblanimalreq`, `tblstaffjob`, `timecostcalcs`, `monthlyoutput`, `monthlytime`)
- A 12-month source-side sample with 223 detail rows in total
- Rebuilt `fps.fpsyeartotals` for the four scenario projects using source-side rollups

**When to use**: 
- Before running the MAB Archive job
- When you want to reset and reload test data

**How to use**:
```powershell
$env:PGPASSWORD='admin123'
$psql='C:\Program Files\PostgreSQL\16\bin\psql.exe'
& $psql -h localhost -p 5432 -U postgres -d batch_jobs_foundation_db `
  -f "src\Apha.BatchJobs\database\sql\200_insert_test_scenario_data.sql"
```

---

### 2. Expected Results Documentation

**File**: `TESTDATA-EXPECTED-RESULTS.md`

**Purpose**: Define what SHOULD happen when MAB Archive job processes test data

**Sections**:
- **Scenario 1: Single Project - Basic Costs Only**
  - 1 project (P100-BASIC)
  - 2150 additional + 35070 staff + 5328 animal + 19980 test = 62528 total
  - 1332 output volume over 12 months
  - Expected archive rows and counts

- **Scenario 2: Multi-Project with Cost Variations**
  - 2 projects (P200-MULTI-A, P200-MULTI-B)
  - Combined 144186 total costs
  - 2904 output volume over 12 months
  - Multi-project consistency checks

- **Scenario 3: Complex Project with Full Data**
  - 1 project (P300-COMPLEX) using all 24 archive loaders
  - 141780 total costs from the current source-side sample
  - 2508 output volume and 1878 monthly hours
  - Highest-volume local source sample before archive execution

- **Cross-Scenario Summary**
  - Combined totals across all 4 projects
  - Master data consistency expectations
  - Data deletion/reload idempotency scenario
  - Partial refresh scenario (Jan-April behavior)
  - Data integrity checks

**When to use**:
- BEFORE running the job: Understand what should happen
- DURING validation: Compare actual results to these expected values
- WHEN troubleshooting: Check if archive has expected row counts and costs

**How to use**:
- Read Scenario 1 (baseline) → Scenario 2 (multi-project) → Scenario 3 (full coverage)
- Reference the "Expected output" tables when writing validation queries
- Use "Validation Checkpoints" table to create custom verification queries

---

### 3. Validation Procedures

**File**: `TESTDATA-VALIDATION-PROCEDURES.md`

**Purpose**: Provide ready-to-run SQL queries to verify actual results match expected results

**Sections**:
- **Phase 1: Data Load Verification** (before job runs)
  - Check test data loaded correctly into fps
  - Verify master lookups present
  - Verify detail transactions populated

- **Phase 2: Archive Post-Job Validation** (after job runs)
  - Check all 24 archive tables have data
  - Verify row counts by scenario
  - Validate full load for Scenario 3

- **Phase 3: Cost Calculation Validation**
  - Compare fps source costs vs. archive costs
  - Verify costs match exactly (no data loss)
  - Reconciliation of invoices and contracts

- **Phase 4: Detail Row Count Validation**
  - Verify monthly records complete (4 months per project)
  - Verify animal requirements counts
  - Verify staff allocations

- **Phase 5: Master Data Consistency**
  - Verify all lookups present in archive
  - Check for orphaned references (no data integrity violations)

- **Phase 6: Total Calculations Validation**
  - Verify fpsyeartotals matches combined detail costs
  - Cross-check test output totals

- **Phase 7: Idempotency Test**
  - Re-run job and verify same counts (no duplication)

- **Phase 8: Comprehensive Summary Report**
  - Single query showing all 4 projects with costs, tests, and status

**When to use**:
- After loading test data (Phase 1) → Before running job
- After running MAB Archive job (Phase 2-8) → Validate results

**How to use**:
- Copy queries from relevant Phase section
- Run in PostgreSQL/psql
- Compare actual output to expected values in TESTDATA-EXPECTED-RESULTS.md

---

### 4. Complete Testing Guide

**File**: `TESTDATA-COMPLETE-GUIDE.md`

**Purpose**: Walk through entire testing workflow from start to finish

**Sections**:
- **Quick Start** → Overview of all steps
- **Testing Philosophy** → What we're testing (data movement, NOT code)
- **Timeline & Execution Steps** (Step 1-6):
  - Step 1: Load test data (5 min)
  - Step 2: Understand expected results (10 min)
  - Step 3: Verify source data (10 min)
  - Step 4: Run MAB Archive job
  - Step 5: Validate archive results (15 min)
  - Step 6: Generate summary report (2 min)
- **Test Results Interpretation**
  - If all pass ✅
  - If partial pass ⚠️
  - If all fail ❌
- **Troubleshooting Guide** → Common issues and solutions
- **Sign-Off Criteria** → Checklist to complete testing
- **Quick Reference** → Expected counts and data volumes

**When to use**:
- First time running full test suite: Read entire guide
- Quick refresh: Check "Quick Reference" and "Step 5-6" sections
- Troubleshooting: Consult "Test Results Interpretation" and "Troubleshooting Guide"

**How to use**:
- Follow steps in order (1 through 6)
- Execute SQL from referenced sections in TESTDATA-VALIDATION-PROCEDURES.md
- Compare results to expected values from TESTDATA-EXPECTED-RESULTS.md

---

## Related Documentation

### Process Understanding

- **[MABARCHIVE-PROCESS-PLAIN-LANGUAGE.md](MABARCHIVE-PROCESS-PLAIN-LANGUAGE.md)**
  - Plain English explanation of what the MAB Archive job does
  - Start here if you're new to the process

- **[MABARCHIVE-DATA-SOURCE-TARGET-MAP.md](MABARCHIVE-DATA-SOURCE-TARGET-MAP.md)**
  - Detailed mapping of 24 data loaders
  - Which fps tables → which mabarchive tables
  - Filtering and deletion rules

### Database Migration

- **[MABARCHIVE-REQUIRED-SQL-OBJECTS.md](MABARCHIVE-REQUIRED-SQL-OBJECTS.md)**
  - Complete inventory of all 41 required database objects
  - Schema, tables, views, and sequences needed
  - Migration status verified against cloud

### Code Implementation

- **[src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/MyFpsYearlyDataService.cs](../../../src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/MyFpsYearlyDataService.cs)**
  - C# implementation of data loading logic
  - 24 loaders + rebuild totals logic

- **[src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/ReloadFpsTotalsService.cs](../../../src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/ReloadFpsTotalsService.cs)**
  - C# implementation of totals recalculation

---

## Test Data Scenarios Summary

### Scenario 1: Single Project - Basic (P100-BASIC)
```
Complexity:     Simple
Projects:       1
Cost types:     Additional (2150) + Staff (35070) + Animal (5328) + Test (19980)
Staff costs:    35070 (24 timecost rows)
Tests:          1332 output volume (12 months)
Hours:          1338 (monthlytime)
Expected total: 62528
```

### Scenario 2A: Multi-Project A (P200-MULTI-A)
```
Complexity:     Medium
Projects:       1 (part of multi-project test)
Cost types:     Additional (4700) + Staff (54420) + Animal (12900) + Test (33150)
Staff costs:    54420 (24 timecost rows)
Tests:          1950 output volume (12 months)
Hours:          1578 (monthlytime)
Expected total: 105170
```

### Scenario 2B: Multi-Project B (P200-MULTI-B)
```
Complexity:     Medium
Projects:       1 (part of multi-project test)
Cost types:     Additional (1000) + Staff (25656) + Animal (2820) + Test (9540)
Staff costs:    25656 (24 timecost rows)
Tests:          954 output volume (12 months)
Hours:          966 (monthlytime)
Expected total: 39016
```

### Scenario 3: Complex - Full Coverage (P300-COMPLEX)
```
Complexity:     High
Projects:       1
Cost types:     Additional (8700) + Staff (55176) + Animal (27744) + Test (50160)
Archive loaders: Source-side operational sample only; archive execution remains a separate step
Additional rows: 3
Animal rows:     4
Staff costs:     55176 (24 timecost rows)
Tests:           2508 output volume (12 months)
Hours:           1878 (monthlytime)
Expected total:  141780
```

### Combined All Scenarios
```
Total projects:    4
Total costs:       348494
Total tests:       6744 output volume
Total hours:       5760 monthlytime hours
Total detail rows: 223
Archive tables:    Not yet refreshed by the current local job run
```

---

## Quick Start Checklist

- [ ] **Review**: Read MABARCHIVE-PROCESS-PLAIN-LANGUAGE.md (understand the "why")
- [ ] **Reference**: Read MABARCHIVE-DATA-SOURCE-TARGET-MAP.md (understand the "what")
- [ ] **Guide**: Read TESTDATA-COMPLETE-GUIDE.md steps 1-3 (understand the "how")
- [ ] **Load**: Execute 200_insert_test_scenario_data.sql (populate fps schema)
- [ ] **Verify**: Run Phase 1 validation queries (confirm source data loaded)
- [ ] **Execute**: Run MAB Archive job (trigger via C# app or manual trigger)
- [ ] **Validate**: Run Phase 2-8 validation queries (verify archive populated)
- [ ] **Report**: Generate Phase 8 summary report (confirm all scenarios pass)
- [ ] **Document**: Save results and any deviations found

---

## Expected Results at a Glance

### After Data Load (fps schema)
- ✅ 4 projects with fpsyear=2026
- ✅ 3 programs
- ✅ 3 workgroups
- ✅ 4 animals
- ✅ 4 employees
- ✅ 16 monthly transaction records (4 projects × 4 months)

### After Archive Job (mabarchive schema)
- ✅ 24 archive tables populated for year 2026
- ✅ ~120 total rows across all tables
- ✅ P100-BASIC: ~18700 cost
- ✅ P200-MULTI-A: ~36700 cost
- ✅ P200-MULTI-B: ~11500 cost
- ✅ P300-COMPLEX: ~61700 cost (all 24 loaders active)
- ✅ No orphaned foreign keys
- ✅ Costs match between fps and archive

---

## Pass/Fail Criteria

### ✅ TEST PASSES if:
- All archive tables have >0 rows for year 2026
- Costs match exactly between fps and archive (within rounding)
- All 4 projects present in my_fpsyeartotals
- Monthly counts = 4 for all transaction tables
- No orphaned foreign key references
- Re-run produces identical counts (idempotent)
- All 24 loaders active for Scenario 3 project

### ❌ TEST FAILS if:
- Any archive table for year 2026 is empty
- Cost mismatch >1% between fps and archive
- < 4 projects in archive
- Monthly counts != 4 for any transaction table
- Orphaned references found
- Re-run produces duplicate rows
- Scenario 3: fewer than 24 loaders populated

---

## How to Use This Suite

### First Time Testing
1. Read: TESTDATA-COMPLETE-GUIDE.md (entire document)
2. Load: Execute 200_insert_test_scenario_data.sql
3. Validate: Follow Steps 1-6 in Complete Guide
4. Review: Expected results from TESTDATA-EXPECTED-RESULTS.md
5. Verify: Run validation queries from TESTDATA-VALIDATION-PROCEDURES.md

### Regression Testing (Periodic)
1. Load: Execute 200_insert_test_scenario_data.sql (reload fresh)
2. Run: MAB Archive job
3. Validate: Run Phase 8 summary report query
4. Compare: Against expected values in this index

### Troubleshooting
1. Check: TESTDATA-COMPLETE-GUIDE.md → "Troubleshooting Guide"
2. Debug: Run Phase 1 validation (check source data)
3. Verify: Run Phase 2 validation (check archive population)
4. Analyze: Compare actual vs. expected from TESTDATA-EXPECTED-RESULTS.md

---

## Contact & Support

- **Data Mapping Questions**: See MABARCHIVE-DATA-SOURCE-TARGET-MAP.md
- **Expected Results Questions**: See TESTDATA-EXPECTED-RESULTS.md
- **Validation Query Questions**: See TESTDATA-VALIDATION-PROCEDURES.md
- **Testing Workflow Questions**: See TESTDATA-COMPLETE-GUIDE.md
- **Process Understanding**: See MABARCHIVE-PROCESS-PLAIN-LANGUAGE.md
- **Database Objects**: See MABARCHIVE-REQUIRED-SQL-OBJECTS.md
- **Code Implementation**: See C# repositories in src/Apha.BatchJobs/

---

## Document Versions

| Document | Version | Date | Notes |
|---|---|---|---|
| 200_insert_test_scenario_data.sql | 1.0 | 2026-04-30 | Initial: 3 scenarios, 4 projects, 80+ records |
| TESTDATA-EXPECTED-RESULTS.md | 1.0 | 2026-04-30 | Initial: complete expected results for all scenarios |
| TESTDATA-VALIDATION-PROCEDURES.md | 1.0 | 2026-04-30 | Initial: 8-phase validation with 30+ SQL queries |
| TESTDATA-COMPLETE-GUIDE.md | 1.0 | 2026-04-30 | Initial: step-by-step testing guide |
| **THIS FILE** | 1.0 | 2026-04-30 | Initial: index and quick reference |

---

## Key Principles

**What We Test**:
- ✅ Data copies from fps to mabarchive correctly
- ✅ Totals calculated from detail records
- ✅ All 4 projects archived successfully
- ✅ No data loss in transfer
- ✅ Referential integrity maintained

**What We DON'T Test**:
- ❌ C# code logic or algorithms
- ❌ Application configuration or settings
- ❌ Error handling in code
- ❌ Performance optimization
- ❌ Security or permissions

**Test Scope**:
- Database schema: fps (source) and mabarchive (archive)
- Data: 4 projects, 24 loaders, ~120 rows total
- Scenarios: 3 (basic, multi-project, complex)
- Time: ~1 hour for complete testing

---

## Summary

This test suite provides **everything needed** to validate the MAB Archive scheduled job's data transformation logic through realistic test scenarios and comprehensive SQL validation.

**Start with**: TESTDATA-COMPLETE-GUIDE.md (follow steps 1-6)  
**Reference**: TESTDATA-EXPECTED-RESULTS.md (compare actual to expected)  
**Validate**: TESTDATA-VALIDATION-PROCEDURES.md (run SQL queries)  
**Troubleshoot**: TESTDATA-COMPLETE-GUIDE.md (see troubleshooting section)

---

**Test Suite Release Date**: 2026-04-30  
**Status**: Ready for testing  
**Maintained By**: MAB Archive Development Team
