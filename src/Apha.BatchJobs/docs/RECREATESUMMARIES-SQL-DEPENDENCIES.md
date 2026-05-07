# RecreateSummaries SQL Dependencies Inventory

This document lists all external objects referenced by the RecreateSummaries procedures that are not defined within the baseline SQL file. These must be sourced from the FPS2025 database schema.

## Categorization Status

- **✅ Defined** — Included in baseline SQL or referenced as internal dependencies
- **❓ Missing** — Definition must be sourced from FPS2025 database
- **📋 External** — References other stored procedures that must exist pre-deployment

---

## 1. VIEWS & QUERIES (❓ Missing)

These are materialized or query-based objects that must be created or referenced from FPS2025:

| Object Name | Referenced In | Type | Purpose | Status |
|---|---|---|---|---|
| qryProjectMonthCW | sp_CreateProjectMonthCasework | Query/View | Project month casework query source | ❓ |
| vPacttblStaff | sp_CreateTimeCostCalcs | View | PACT staff dimension | ❓ |

---

## 2. TABLES (❓ Missing or Needs Confirmation)

### Summary/Output Tables (Target/Calculation Tables)

| Object Name | Referenced In | Type | Purpose | Status |
|---|---|---|---|---|
| ProjectMonth | sp_InsertMissingProjects | Table | Base project monthly calendar | ❓ |
| ProjectMonth2 | sp_qryJobMonth_Single, sp_qryJobMonthCum, sp_qryJobMonth_Final | Table | Single-month project totals | ❓ |
| ProjectMonth3 | sp_qryJobMonthCum, sp_qryJobMonth_Final | Table | Cumulative project month totals | ❓ |
| ProjectMonthCasework | sp_CreateProjectMonthCasework, sp_DeleteProjectMonthCasework, sp_qryJobMonthCum | Table | Project month casework allocations | ❓ |
| ProjectMonthFinal | sp_DeleteProjectMonthFinal, sp_qryJobMonth_Final | Table | Final project month consolidated totals | ❓ |
| TimeCostCalcs | sp_CreateTimeCostCalcs, sp_deleteTimeCostCalcs, usp_Refresh_Period_TCC | Table | Time-based cost calculations | ❓ |
| RecreateSummaries_Log | usp_LogRecreateSummaries | Table | Audit log for RecreateSummaries execution | ❓ |

### Dimension/Lookup Tables

| Object Name | Referenced In | Type | Purpose | Status |
|---|---|---|---|---|
| tlkpProject | sp_CreateTimeCostCalcs, sp_InsertMissingProjects, usp_Refresh_Period_MO, usp_Refresh_Period_PSC, usp_Refresh_Period_TCC | Table | Project lookup/dimension | ❓ |
| tlkpProgram | sp_CreateTimeCostCalcs | Table | Program lookup dimension | ❓ |
| CostCentre | usp_Refresh_Period_MO, usp_Refresh_Period_PSC, usp_Refresh_Period_TCC | Table | Cost centre dimension | ❓ |
| WorkGroup | usp_Refresh_Period_MO, usp_Refresh_Period_TCC | Table | Work group dimension | ❓ |
| WorkGroupGrade | sp_CreateTimeCostCalcs | Table | Work group grade lookup | ❓ |
| ProfitCentreGrade | sp_CreateTimeCostCalcs | Table | Profit centre grade rates | ❓ |
| tblkpProfitCentre | sp_CreateTimeCostCalcs | Table | Profit centre lookup | ❓ |
| tblPeriod | sp_RecreateSummaries, sp_qryJobMonthCum | Table | Period/fiscal calendar | ❓ |
| tblkPeriodMonth | sp_qryJobMonthCum | Table | Period-to-month mapping | ❓ |

### Transactional/Operational Tables

| Object Name | Referenced In | Type | Purpose | Status |
|---|---|---|---|---|
| MonthlyTime | sp_CreateTimeCostCalcs | Table | Monthly timesheet/hours data | ❓ |
| MonthlyOutput | usp_Refresh_Period_MO | Table | Monthly output/test results | ❓ |
| TimeCodeValid | sp_CreateTimeCostCalcs | Table | Time code validation/hierarchy | ❓ |
| Proj_SubContract | usp_Refresh_Period_PSC | Table | Project subcontract data | ❓ |
| tblWGEmployee | usp_Refresh_Period_TCC | Table | Work group employee roster | ❓ |

### Period Output/Archive Tables

| Object Name | Referenced In | Type | Purpose | Status |
|---|---|---|---|---|
| Period_MonthlyOutput | usp_Refresh_Period_MO | Table | Period-archived monthly output | ❓ |
| Period_Proj_SubContract | usp_Refresh_Period_PSC | Table | Period-archived project subcontracts | ❓ |
| Period_TimeCostCalcs | usp_Refresh_Period_TCC | Table | Period-archived time cost calcs | ❓ |

---

## 3. STORED PROCEDURES (📋 External Dependencies)

These procedures are called by RecreateSummaries but are NOT defined in the baseline file. They must exist in FPS2025 before deployment:

| Object Name | Called By | Purpose | Status |
|---|---|---|---|
| sp_deleteFPSTotals | sp_RecreateSummaries | Delete FPS total/summary aggregates | 📋 |
| sp_createFPSTotals | sp_RecreateSummaries | Create/rebuild FPS total aggregates | 📋 |
| sp_Get_SP_No | usp_LogRecreateSummaries | Get current user/operator identifier | 📋 |

---

## 4. SUMMARY BY OBJECT TYPE

### Views/Queries to Source: 2
- qryProjectMonthCW
- vPacttblStaff

### Tables to Define or Confirm: 26
- **Summary/Output (7):** ProjectMonth, ProjectMonth2, ProjectMonth3, ProjectMonthCasework, ProjectMonthFinal, TimeCostCalcs, RecreateSummaries_Log
- **Dimension/Lookup (9):** tlkpProject, tlkpProgram, CostCentre, WorkGroup, WorkGroupGrade, ProfitCentreGrade, tblkpProfitCentre, tblPeriod, tblkPeriodMonth
- **Transactional (5):** MonthlyTime, MonthlyOutput, TimeCodeValid, Proj_SubContract, tblWGEmployee
- **Period Archive (3):** Period_MonthlyOutput, Period_Proj_SubContract, Period_TimeCostCalcs

### Procedures to Source: 3
- sp_deleteFPSTotals
- sp_createFPSTotals
- sp_Get_SP_No

---

## 5. DEPENDENCY DELIVERY CHECKLIST

Use this to track which objects have been located and shared:

### Views/Queries
- [ ] qryProjectMonthCW — Definition source: _______
- [ ] vPacttblStaff — Definition source: _______

### Critical Procedures (Must exist before baseline can run)
- [ ] sp_deleteFPSTotals — Definition source: _______
- [ ] sp_createFPSTotals — Definition source: _______
- [ ] sp_Get_SP_No — Definition source: _______

### Tables (Verify existence/schema in target FPS2025)
- [ ] ProjectMonth — Confirmed schema: _______
- [ ] ProjectMonth2 — Confirmed schema: _______
- [ ] ProjectMonth3 — Confirmed schema: _______
- [ ] ProjectMonthCasework — Confirmed schema: _______
- [ ] ProjectMonthFinal — Confirmed schema: _______
- [ ] TimeCostCalcs — Confirmed schema: _______
- [ ] RecreateSummaries_Log — Create if missing
- [ ] tlkpProject — Confirmed schema: _______
- [ ] tlkpProgram — Confirmed schema: _______
- [ ] CostCentre — Confirmed schema: _______
- [ ] WorkGroup — Confirmed schema: _______
- [ ] WorkGroupGrade — Confirmed schema: _______
- [ ] ProfitCentreGrade — Confirmed schema: _______
- [ ] tblkpProfitCentre — Confirmed schema: _______
- [ ] tblPeriod — Confirmed schema: _______
- [ ] tblkPeriodMonth — Confirmed schema: _______
- [ ] MonthlyTime — Confirmed schema: _______
- [ ] MonthlyOutput — Confirmed schema: _______
- [ ] TimeCodeValid — Confirmed schema: _______
- [ ] Proj_SubContract — Confirmed schema: _______
- [ ] tblWGEmployee — Confirmed schema: _______
- [ ] Period_MonthlyOutput — Create if missing
- [ ] Period_Proj_SubContract — Create if missing
- [ ] Period_TimeCostCalcs — Create if missing

---

## 6. RECOMMENDED NEXT STEPS

1. **Immediate:** Locate and share SQL definitions for the 3 critical procedures
2. **High Priority:** Locate definitions for the 2 views/queries
3. **Schema Validation:** Export FPS2025 table schemas for all 26 referenced tables
4. **Table Creation:** If any tables are missing, create them based on procedure logic and column references
5. **Integration:** Merge all definitions into a comprehensive FPS2025 migration script

---

**Note:** This inventory was generated by static analysis of the baseline SQL file. Final validation should compare against actual FPS2025 schema to confirm all objects exist and column contracts match procedure expectations.
