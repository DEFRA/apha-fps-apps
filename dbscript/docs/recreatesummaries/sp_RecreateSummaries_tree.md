# sp_RecreateSummaries — Dependency Tree Report

## Overview

This document shows the complete SQL object dependency tree for `dbo.sp_RecreateSummaries`, in correct execution sequence. Each child procedure lists all sub-child objects (tables and views) it directly reads from or writes to, with the operation type noted.

## External Sources Used

| Object | Source Path | Notes |
|--------|-------------|-------|
| sp_Get_SP_No | Z:/Lot2/DB/sp_Get_SP_NO.sql | Provided externally by user; not under current workspace root |

---

## Legend

```
[Procedure]   = Stored Procedure (EXEC)
[Table]       = Database Table
[View]        = Database View
→ DELETE      = procedure deletes from this object
→ INSERT INTO = procedure writes into this object
→ SELECT FROM = procedure reads from this object
→ INNER JOIN  = procedure joins this object in a SELECT
→ LEFT JOIN   = procedure optionally joins this object in a SELECT
→ EXEC        = procedure calls this child procedure
```

---

## Dependency Tree

```
dbo.sp_RecreateSummaries (@Month int)              (FPS2025/Procedures/sp_RecreateSummaries.sql)
│
│   [Lock check — reads tblPeriod.periodLocked after step 14]
│
├── 1. sp_deleteFPSTotals                          (FPS2025/Procedures/sp_deleteFPSTotals.sql)
│   └── FPSYearTotals                              (FPS2025/Tables/FPSYearTotals.sql)
│                                                      → DELETE
│
├── 2. sp_createFPSTotals                          (FPS2025/Procedures/sp_createFPSTotals.sql)
│   ├── FPSYearTotals                              (FPS2025/Tables/FPSYearTotals.sql)
│   │                                                  → INSERT INTO
│   ├── qryTotalAdditionalCosts                    (FPS2025/Views/qryTotalAdditionalCosts.sql)
│   │                                                  → LEFT JOIN (on tlkpProject.ParentProject)
│   ├── qryTotalAnimalCosts                        (FPS2025/Views/qryTotalAnimalCosts.sql)
│   │                                                  → LEFT JOIN (on tlkpProject.ParentProject)
│   ├── qryTotalStaffCosts                         (FPS2025/Views/qryTotalStaffCosts.sql)
│   │                                                  → LEFT JOIN (on tlkpProject.ParentProject)
│   └── qryTotalTestCosts                          (FPS2025/Views/qryTotalTestCosts.sql)
│                                                      → LEFT JOIN (on tlkpProject.ParentProject)
│
├── 3. sp_InsertMissingProjects                    (FPS2025/Procedures/sp_InsertMissingProjects.sql)
│   └── ProjectMonth                               (FPS2025/Tables/ProjectMonth.sql)
│                                                      → INSERT INTO
│
├── 4. sp_deleteTimeCostCalcs                      (FPS2025/Procedures/sp_deleteTimeCostCalcs.sql)
│   └── TimeCostCalcs                              (FPS2025/Tables/TimeCostCalcs.sql)
│                                                      → DELETE
│
├── 5. sp_CreateTimeCostCalcs                      (FPS2025/Procedures/sp_CreateTimeCostCalcs.sql)
│   └── TimeCostCalcs                              (FPS2025/Tables/TimeCostCalcs.sql)
│                                                      → INSERT INTO
│
├── 6. sp_DeleteProjectMonthCasework               (FPS2025/Procedures/sp_DeleteProjectMonthCasework.sql)
│   └── ProjectMonthCasework                       (FPS2025/Tables/ProjectMonthCasework.sql)
│                                                      → DELETE
│
├── 7. sp_CreateProjectMonthCasework               (FPS2025/Procedures/sp_CreateProjectMonthCasework.sql)
│   ├── ProjectMonthCasework                       (FPS2025/Tables/ProjectMonthCasework.sql)
│   │                                                  → INSERT INTO
│   └── qryProjectMonthCW                          (FPS2025/Views/qryProjectMonthCW.sql)
│                                                      → SELECT FROM
│
├── 8. sp_DeleteProjectMonthFinal                  (FPS2025/Procedures/sp_DeleteProjectMonthFinal.sql)
│   └── ProjectMonthFinal                          (FPS2025/Tables/ProjectMonthFinal.sql)
│                                                      → DELETE
│
├── 9. sp_deleteProjectMonth2                      (FPS2025/Procedures/sp_deleteProjectMonth2.sql)
│   └── ProjectMonth2                              (FPS2025/Tables/ProjectMonth2.sql)
│                                                      → DELETE
│
├── 10. sp_qryJobMonth_Single                      (FPS2025/Procedures/sp_qryJobMonth_Single.sql)
│    ├── ProjectMonth2                             (FPS2025/Tables/ProjectMonth2.sql)
│    │                                                 → INSERT INTO
│    ├── ProjectMonth                              (FPS2025/Tables/ProjectMonth.sql)
│    │                                                 → SELECT FROM (base row driver)
│    ├── qryJobMonth_SubContracts                  (FPS2025/Views/qryJobMonth_SubContracts.sql)
│    │                                                 → LEFT JOIN (on Project, Month)
│    ├── qryJobMonth_Time                          (FPS2025/Views/qryJobMonth_Time.sql)
│    │                                                 → LEFT JOIN (on Project, Month)
│    ├── qryJobMonthMilestone                      (FPS2025/Views/qryJobMonthMilestone.sql)
│    │                                                 → LEFT JOIN (on Project, DueMonth)
│    ├── qryJobMonth_TransfersTotal                (FPS2025/Views/qryJobMonth_TransfersTotal.sql)
│    │                                                 → LEFT JOIN (on Project, Month)
│    ├── qryJobMonth_Invoices                      (FPS2025/Views/qryJobMonth_Invoices.sql)
│    │                                                 → LEFT JOIN (on Month, ProjectParent)
│    ├── qryJobMonthPortfolioSales                 (FPS2025/Views/qryJobMonthPortfolioSales.sql)
│    │                                                 → LEFT JOIN (on Month, PlanPortfolio)
│    └── qryJobMonth_TotProfile                   (FPS2025/Views/qryJobMonth_TotProfile.sql)
│                                                      → LEFT JOIN (on Project)
│
├── 11. sp_DeleteProjectMonth3                     (FPS2025/Procedures/sp_DeleteProjectMonth3.sql)
│    └── ProjectMonth3                             (FPS2025/Tables/ProjectMonth3.sql)
│                                                      → DELETE
│
├── 12. sp_qryJobMonthCum                          (FPS2025/Procedures/sp_qryJobMonthCum.sql)
│    ├── ProjectMonth3                             (FPS2025/Tables/ProjectMonth3.sql)
│    │                                                 → INSERT INTO
│    ├── tblPeriod                                 (FPS2025/Tables/tblPeriod.sql)
│    │                                                 → INNER JOIN (period structure driver)
│    ├── tblkPeriodMonth                           (FPS2025/Views/tblkPeriodMonth.sql)
│    │                                                 → INNER JOIN (month-to-period mapping)
│    ├── ProjectMonth2                             (FPS2025/Tables/ProjectMonth2.sql)
│    │                                                 → INNER JOIN (single-month data)
│    └── ProjectMonthCasework                      (FPS2025/Tables/ProjectMonthCasework.sql)
│                                                      → INNER JOIN (casework debit/credit)
│
├── 13. sp_qryJobMonth_Final (@Month)              (FPS2025/Procedures/sp_qryJobMonth_Final.sql)
│    ├── ProjectMonthFinal                         (FPS2025/Tables/ProjectMonthFinal.sql)
│    │                                                 → INSERT INTO
│    ├── ProjectMonth2                             (FPS2025/Tables/ProjectMonth2.sql)
│    │                                                 → INNER JOIN (single-month metrics)
│    ├── ProjectMonth3                             (FPS2025/Tables/ProjectMonth3.sql)
│    │                                                 → INNER JOIN (cumulative metrics)
│    └── ProjectMonthCasework                      (FPS2025/Tables/ProjectMonthCasework.sql)
│                                                      → INNER JOIN (casework debit/credit)
│
├── 14. usp_LogRecreateSummaries (@Month)          (FPS2025/Procedures/usp_LogRecreateSummaries.sql)
│    ├── RecreateSummaries_Log                     (FPS2025/Tables/RecreateSummaries_Log.sql)
│    │                                                 → INSERT INTO
│    └── sp_Get_SP_No                              (Z:/Lot2/DB/sp_Get_SP_NO.sql)
│                                                      → EXEC
│
│   ── Lock Check ──────────────────────────────────────────────────────────────
│   SELECT periodLocked FROM tblPeriod WHERE endperiod = @month
│   If periodLocked = 0  → execute steps 15–17
│   If periodLocked = 1  → SKIP steps 15–17
│   ────────────────────────────────────────────────────────────────────────────
│
├── 15. usp_Refresh_Period_MO (@month)  [CONDITIONAL]  (FPS2025/Procedures/usp_Refresh_Period_MO.sql)
│    └── Period_MonthlyOutput               (FPS2025/Tables/Period_MonthlyOutput.sql)
│                                               → DELETE + INSERT INTO
│
├── 16. usp_Refresh_Period_psc (@month) [CONDITIONAL]  (FPS2025/Procedures/usp_Refresh_Period_PSC.sql)
│    └── Period_Proj_Subcontract             (FPS2025/Tables/Period_Proj_Subcontract.sql)
│                                               → DELETE + INSERT INTO
│
└── 17. usp_Refresh_Period_tcc (@month) [CONDITIONAL]  (FPS2025/Procedures/usp_Refresh_Period_TCC.sql)
     └── Period_TimeCostCalcs                (FPS2025/Tables/Period_TimeCostCalcs.sql)
                                                → DELETE + INSERT INTO
```

---

## Object Summary by Type

### Procedures Called (17 total)

| # | Procedure | Source Path | Called With Parameter? |
|---|-----------|-------------|----------------------|
| 1 | sp_deleteFPSTotals | FPS2025/Procedures/sp_deleteFPSTotals.sql | No |
| 2 | sp_createFPSTotals | FPS2025/Procedures/sp_createFPSTotals.sql | No |
| 3 | sp_InsertMissingProjects | FPS2025/Procedures/sp_InsertMissingProjects.sql | No |
| 4 | sp_deleteTimeCostCalcs | FPS2025/Procedures/sp_deleteTimeCostCalcs.sql | No |
| 5 | sp_CreateTimeCostCalcs | FPS2025/Procedures/sp_CreateTimeCostCalcs.sql | No |
| 6 | sp_DeleteProjectMonthCasework | FPS2025/Procedures/sp_DeleteProjectMonthCasework.sql | No |
| 7 | sp_CreateProjectMonthCasework | FPS2025/Procedures/sp_CreateProjectMonthCasework.sql | No |
| 8 | sp_DeleteProjectMonthFinal | FPS2025/Procedures/sp_DeleteProjectMonthFinal.sql | No |
| 9 | sp_deleteProjectMonth2 | FPS2025/Procedures/sp_deleteProjectMonth2.sql | No |
| 10 | sp_qryJobMonth_Single | FPS2025/Procedures/sp_qryJobMonth_Single.sql | No |
| 11 | sp_DeleteProjectMonth3 | FPS2025/Procedures/sp_DeleteProjectMonth3.sql | No |
| 12 | sp_qryJobMonthCum | FPS2025/Procedures/sp_qryJobMonthCum.sql | No |
| 13 | sp_qryJobMonth_Final | FPS2025/Procedures/sp_qryJobMonth_Final.sql | Yes — @Month |
| 14 | usp_LogRecreateSummaries | FPS2025/Procedures/usp_LogRecreateSummaries.sql | Yes — @Month |
| 15 | usp_Refresh_Period_MO | FPS2025/Procedures/usp_Refresh_Period_MO.sql | Yes — @month (conditional) |
| 16 | usp_Refresh_Period_psc | FPS2025/Procedures/usp_Refresh_Period_PSC.sql | Yes — @month (conditional) |
| 17 | usp_Refresh_Period_tcc | FPS2025/Procedures/usp_Refresh_Period_TCC.sql | Yes — @month (conditional) |

### Tables Referenced (12 unique tables)

| Table | Source Path | Used By (Procedure) | Operation |
|-------|-------------|---------------------|-----------|
| FPSYearTotals | FPS2025/Tables/FPSYearTotals.sql | sp_deleteFPSTotals | DELETE |
| FPSYearTotals | FPS2025/Tables/FPSYearTotals.sql | sp_createFPSTotals | INSERT INTO |
| ProjectMonth | FPS2025/Tables/ProjectMonth.sql | sp_InsertMissingProjects | INSERT INTO |
| ProjectMonth | FPS2025/Tables/ProjectMonth.sql | sp_qryJobMonth_Single | SELECT FROM |
| TimeCostCalcs | FPS2025/Tables/TimeCostCalcs.sql | sp_deleteTimeCostCalcs | DELETE |
| TimeCostCalcs | FPS2025/Tables/TimeCostCalcs.sql | sp_CreateTimeCostCalcs | INSERT INTO |
| ProjectMonthCasework | FPS2025/Tables/ProjectMonthCasework.sql | sp_DeleteProjectMonthCasework | DELETE |
| ProjectMonthCasework | FPS2025/Tables/ProjectMonthCasework.sql | sp_CreateProjectMonthCasework | INSERT INTO |
| ProjectMonthCasework | FPS2025/Tables/ProjectMonthCasework.sql | sp_qryJobMonthCum | INNER JOIN |
| ProjectMonthCasework | FPS2025/Tables/ProjectMonthCasework.sql | sp_qryJobMonth_Final | INNER JOIN |
| ProjectMonthFinal | FPS2025/Tables/ProjectMonthFinal.sql | sp_DeleteProjectMonthFinal | DELETE |
| ProjectMonthFinal | FPS2025/Tables/ProjectMonthFinal.sql | sp_qryJobMonth_Final | INSERT INTO |
| ProjectMonth2 | FPS2025/Tables/ProjectMonth2.sql | sp_deleteProjectMonth2 | DELETE |
| ProjectMonth2 | FPS2025/Tables/ProjectMonth2.sql | sp_qryJobMonth_Single | INSERT INTO |
| ProjectMonth2 | FPS2025/Tables/ProjectMonth2.sql | sp_qryJobMonthCum | INNER JOIN |
| ProjectMonth2 | FPS2025/Tables/ProjectMonth2.sql | sp_qryJobMonth_Final | INNER JOIN |
| ProjectMonth3 | FPS2025/Tables/ProjectMonth3.sql | sp_DeleteProjectMonth3 | DELETE |
| ProjectMonth3 | FPS2025/Tables/ProjectMonth3.sql | sp_qryJobMonthCum | INSERT INTO |
| ProjectMonth3 | FPS2025/Tables/ProjectMonth3.sql | sp_qryJobMonth_Final | INNER JOIN |
| tblPeriod | FPS2025/Tables/tblPeriod.sql | sp_RecreateSummaries (lock check) | SELECT FROM |
| tblPeriod | FPS2025/Tables/tblPeriod.sql | sp_qryJobMonthCum | INNER JOIN |
| RecreateSummaries_Log | FPS2025/Tables/RecreateSummaries_Log.sql | usp_LogRecreateSummaries | INSERT INTO |
| Period_MonthlyOutput | FPS2025/Tables/Period_MonthlyOutput.sql | usp_Refresh_Period_MO | DELETE + INSERT INTO |
| Period_Proj_Subcontract | FPS2025/Tables/Period_Proj_Subcontract.sql | usp_Refresh_Period_psc | DELETE + INSERT INTO |
| Period_TimeCostCalcs | FPS2025/Tables/Period_TimeCostCalcs.sql | usp_Refresh_Period_tcc | DELETE + INSERT INTO |

### Views Referenced (13 unique views)

| View | Source Path | Used By (Procedure) | Operation |
|------|-------------|---------------------|-----------|
| qryTotalAdditionalCosts | FPS2025/Views/qryTotalAdditionalCosts.sql | sp_createFPSTotals | LEFT JOIN |
| qryTotalAnimalCosts | FPS2025/Views/qryTotalAnimalCosts.sql | sp_createFPSTotals | LEFT JOIN |
| qryTotalStaffCosts | FPS2025/Views/qryTotalStaffCosts.sql | sp_createFPSTotals | LEFT JOIN |
| qryTotalTestCosts | FPS2025/Views/qryTotalTestCosts.sql | sp_createFPSTotals | LEFT JOIN |
| qryProjectMonthCW | FPS2025/Views/qryProjectMonthCW.sql | sp_CreateProjectMonthCasework | SELECT FROM |
| qryJobMonth_SubContracts | FPS2025/Views/qryJobMonth_SubContracts.sql | sp_qryJobMonth_Single | LEFT JOIN |
| qryJobMonth_Time | FPS2025/Views/qryJobMonth_Time.sql | sp_qryJobMonth_Single | LEFT JOIN |
| qryJobMonthMilestone | FPS2025/Views/qryJobMonthMilestone.sql | sp_qryJobMonth_Single | LEFT JOIN |
| qryJobMonth_TransfersTotal | FPS2025/Views/qryJobMonth_TransfersTotal.sql | sp_qryJobMonth_Single | LEFT JOIN |
| qryJobMonth_Invoices | FPS2025/Views/qryJobMonth_Invoices.sql | sp_qryJobMonth_Single | LEFT JOIN |
| qryJobMonthPortfolioSales | FPS2025/Views/qryJobMonthPortfolioSales.sql | sp_qryJobMonth_Single | LEFT JOIN |
| qryJobMonth_TotProfile | FPS2025/Views/qryJobMonth_TotProfile.sql | sp_qryJobMonth_Single | LEFT JOIN |
| tblkPeriodMonth | FPS2025/Views/tblkPeriodMonth.sql | sp_qryJobMonthCum | INNER JOIN |

---

## Data Flow Summary

The pipeline processes in three logical phases:

### Phase 1 — Clear and Rebuild Totals (Steps 1–5)
Clears `FPSYearTotals` and `TimeCostCalcs`, then rebuilds them from source views and tables.

### Phase 2 — Rebuild Project Month Datasets (Steps 6–13)
- Clears and rebuilds `ProjectMonthCasework` (casework debit/credit per project/month)
- Clears and rebuilds `ProjectMonth2` (single-month metrics per project, joining 7 views)
- Clears and rebuilds `ProjectMonth3` (cumulative metrics per project/period, joining tblPeriod + tblkPeriodMonth)
- Clears and rebuilds `ProjectMonthFinal` (final output combining month + cumulative data, cut off at @Month)

### Phase 3 — Log and Conditional Refresh (Steps 14–17)
- Logs execution to `RecreateSummaries_Log` via `usp_LogRecreateSummaries`
- If period is **not locked** (`periodLocked = 0`): refreshes three period snapshot tables (`Period_MonthlyOutput`, `Period_Proj_Subcontract`, `Period_TimeCostCalcs`)
- If period is **locked**: snapshot refresh is skipped entirely

---

## Resolved External Object

| Object | Source Path | Status |
|--------|-------------|--------|
| sp_Get_SP_No | Z:/Lot2/DB/sp_Get_SP_NO.sql | SQL received and dependency resolved |

### sp_Get_SP_No SQL Code (Provided)

```sql
CREATE PROCEDURE [dbo].[sp_Get_SP_No]   @Mno  varchar(20)
OUTPUT AS
SELECT @MNo = SUBSTRING(SYSTEM_USER, CHARINDEX('\\', SYSTEM_USER) + 1, 20)
GO
```
