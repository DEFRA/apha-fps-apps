# SP to .NET Parity Tracker (MABArchive Scheduled Load)

Status date: 2026-04-29
Scope: legacy `sp_LoadFromFPS` chain and yearly MABArchive refresh flow
Goal: 100% logic integrity with executable parity evidence

## Status Legend
- Converted: behavior matches legacy and has executable validation evidence.
- Partial: behavior is implemented and cross-checked against legacy SQL/DDL, but not yet fully proven by targeted parity tests.
- Missing: no equivalent implementation found.
## Summary
- Converted: 0
- Partial: 29
- Missing: 0
- Overall implementation coverage: 100% (29/29 procedures mapped)
- Overall validated parity: 0% pending Task 7 and Task 8 evidence

## Why Drift Happened
- Earlier .NET work inferred behavior from current schema and existing repository code instead of treating the legacy stored procedure text as the controlling source of truth.
- That produced modernized behavior in places where strict parity was required: `ON CONFLICT` upserts, defaulting `projectstatus`, injecting `source = 'FPS'`, partial delete coverage, and incomplete loader fan-out.
- The corrections in this tracker were made by cross-checking three anchors together:
	- legacy procedure text in `docs/ScheduledJobs.txt`
	- behavior baseline in `docs/SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md`
	- PostgreSQL source/target DDL in `dbscript/schemas/01fps/01tables` and `dbscript/schemas/02mabarchive/01tables`

## Master Procedure Parity
| Legacy SP | Expected Legacy Behavior | .NET Equivalent | Status | Gap / Note |
|---|---|---|---|---|
| `sp_LoadFromFPS` | Previous year always runs first; current year full cycle only when month > 4; before May only `MY_tlkpProject_all` refreshes | `MabArchiveLoadOrchestrator` | Partial | Branch order and year-availability guard implemented; still awaiting executable parity tests |
| `sp_deleteFPSTotals` | Clears `FPSYearTotals` before rebuild | `ReloadFpsTotalsService.RebuildYearTotalsAsync` | Partial | Full-table delete parity implemented; still awaiting fixture-based behavioral proof |
| `sp_createFPSTotals` | Rebuilds totals with legacy joins, null handling, and formulas | `ReloadFpsTotalsService.RebuildYearTotalsAsync` | Partial | Formula parity implemented; still awaiting executable data-shape verification |
| `sp_DeleteYearsFPSData` | Broad year-specific delete across archive footprint plus project-based delete from `G_tlkpProject` | `MyFpsYearlyDataService.DeleteYearDataAsync` | Partial | Legacy table coverage implemented; still awaiting per-table row-count tests |
| `sp_AddYearsFPSData` | 24-loader archive rebuild in fixed sequence | `MyFpsYearlyDataService.LoadYearDataAsync` | Partial | All 24 loaders implemented in legacy order; still awaiting targeted parity tests |

## Yearly Loader Chain Parity (`sp_AddYearsFPSData` children)

| Legacy SP | Legacy Source -> Target | .NET Equivalent | Status | Gap / Note |
|---|---|---|---|---|
| `sp_AddMY_tlkpProgram` | `fps.tlkpprogram` -> `mabarchive.my_tlkpprogram` | `LoadYearDataAsync` loader 1 | Partial | Implemented; pending row-count/value tests |
| `sp_AddG_tlkpProject` | `fps.tlkpproject` grouped -> `mabarchive.g_tlkpproject` | `LoadYearDataAsync` loader 2 | Partial | `GROUP BY` parity implemented; pending duplicate behavior test |
| `sp_AddMY_tlkpProject` | `fps.tlkpproject` -> `mabarchive.my_tlkpproject` | `LoadYearDataAsync` loader 3 | Partial | Legacy shape restored; `source` no longer injected |
| `sp_AddMY_FPSYearTotals` | `fps.fpsyeartotals` -> `mabarchive.my_fpsyeartotals` | `LoadYearDataAsync` loader 4 | Partial | Plain copy implemented; pending totals fixture checks |
| `sp_AddMY_MonthlyOutput` | `fps.monthlyoutput` -> `mabarchive.my_monthlyoutput` | `LoadYearDataAsync` loader 5 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_MonthlyTime` | `fps.monthlytime` -> `mabarchive.my_monthlytime` | `LoadYearDataAsync` loader 6 | Partial | `pactstaffid` column corrected; pending value tests |
| `sp_AddMY_Proj_Invoice` | `fps.proj_invoice` -> `mabarchive.my_proj_invoice` | `LoadYearDataAsync` loader 7 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_Proj_SubContract` | `fps.proj_subcontract` -> `mabarchive.my_proj_subcontract` | `LoadYearDataAsync` loader 8 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_ProjectMonthFinal` | `fps.projectmonthfinal` -> `mabarchive.my_projectmonthfinal` | `LoadYearDataAsync` loader 9 | Partial | 36-column legacy shape restored; pending fixture verification |
| `sp_AddMY_tblAdditionalCosts` | `fps.tbladditionalcosts` -> `mabarchive.my_tbladditionalcosts` | `LoadYearDataAsync` loader 10 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_tblAnimalReq` | `fps.tblanimalreq` -> `mabarchive.my_tblanimalreq` | `LoadYearDataAsync` loader 11 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_tblContract` | `fps.tblcontract` -> `mabarchive.my_tblcontract` | `LoadYearDataAsync` loader 12 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_tblStaffJob` | `fps.tblstaffjob` -> `mabarchive.my_tblstaffjob` | `LoadYearDataAsync` loader 13 | Partial | Legacy insert shape restored; `systimestamp` intentionally left null |
| `sp_AddMY_TimeCostCalcs` | `fps.timecostcalcs` -> `mabarchive.my_timecostcalcs` | `LoadYearDataAsync` loader 14 | Partial | Full legacy column set restored |
| `sp_AddMY_tlkpTestReqmt` | `fps.tlkptestreqmt` -> `mabarchive.my_tlkptestreqmt` | `LoadYearDataAsync` loader 15 | Partial | Legacy insert shape restored; `source` intentionally left null |
| `sp_addMY_YearDetails` | `fps.tbldb_variables` -> `mabarchive.tlkpyear` | `LoadYearDataAsync` loader 16 | Partial | Uses `db_var_name = 'month'`; pending data-shape verification |
| `sp_addMY_WorkGroupGrade` | `fps.workgroupgrade` -> `mabarchive.my_workgroupgrade` | `LoadYearDataAsync` loader 17 | Partial | Implemented; pending row-count tests |
| `sp_addMY_ProfitCentreGrade` | `fps.profitcentregrade` -> `mabarchive.my_profitcentregrade` | `LoadYearDataAsync` loader 18 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_tblProfitCentre` | `fps.tblkpprofitcentre` -> `mabarchive.my_tblprofitcentre` | `LoadYearDataAsync` loader 19 | Partial | No `fpsyear` filter by design because source table has no `fpsyear` |
| `sp_AddMY_TestOrProduct` | `fps.testorproduct` -> `mabarchive.my_testorproduct` | `LoadYearDataAsync` loader 20 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_Staff` | `fps.tblwgemployee` join `fps.tblemployee` -> `mabarchive.my_staff` | `LoadYearDataAsync` loader 21 | Partial | Batch implementation loads all year rows; legacy SQL had per-user security filter |
| `sp_AddMY_Workgroup` | `fps.workgroup` -> `mabarchive.my_workgroup` | `LoadYearDataAsync` loader 22 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_tblAnimals` | `fps.tblanimals` -> `mabarchive.my_tblanimals` | `LoadYearDataAsync` loader 23 | Partial | Implemented; pending row-count tests |
| `sp_AddMY_tlkpProject_All` | `fps.tlkpproject` -> `mabarchive.my_tlkpproject_all` | `LoadYearDataAsync` loader 24 and `RefreshProjectAllOnlyAsync` | Partial | Legacy full-year and pre-May refresh paths implemented; pending targeted tests |

## Residual Gaps Still Open

| Area | Current State | Status | Gap / Note |
|---|---|---|---|
| Totals rebuild proof | Legacy formulas now implemented | Partial | Needs fixture-based proof for null behavior and joins |
| Staff security filter semantics | Batch load copies all year rows | Partial | Legacy SQL scopes by executing SQL user's accessible workgroups/profit centres; likely intentional runtime adaptation but not yet formally approved |
| End-to-end validation evidence | Implementation compiles and has been cross-checked against SQL and DDL | Partial | Task 7 and Task 8 targeted runs passed; still needs fixture-level parity checks and final evidence pack |

## Evidence Sources
- Legacy procedure text: `docs/ScheduledJobs.txt`
- Legacy behavior baseline: `docs/SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md`
- Execution control / drift plan: `docs/MABARCHIVE-100-PERCENT-PARITY-PLAN.md`
- Orchestrator: `Apha.BatchJobs.Application/Jobs/ScheduledJobs/MABArchive/MabArchiveLoadOrchestrator.cs`
- Yearly data service contract: `Apha.BatchJobs.Application/Jobs/ScheduledJobs/MABArchive/Services/IMyFpsYearlyDataService.cs`
- Archive delete/load implementation: `Apha.BatchJobs.Infrastructure/Repositories/MabArchive/MyFpsYearlyDataService.cs`
- Totals rebuild implementation: `Apha.BatchJobs.Infrastructure/Repositories/MabArchive/ReloadFpsTotalsService.cs`
- Task 7 parity tests: `Apha.BatchJobs.UnitTests/MabArchiveLoadOrchestratorParityTests.cs`
- PostgreSQL source schema: `dbscript/schemas/01fps/01tables`
- PostgreSQL archive schema: `dbscript/schemas/02mabarchive/01tables`

## Implementation Change Evidence
- `74df9c8e` - orchestration parity and year-availability guard
- `9edf4dd4` - delete-years parity across legacy archive footprint
- `7f6cca98` - full 24-loader `sp_AddYearsFPSData` fan-out parity
- `b700d443` - full-table delete parity for `sp_deleteFPSTotals` + Task 7 parity tests

## Task 8 Execution Evidence (2026-04-29)
- Build: `dotnet build BatchJobs.sln` -> succeeded (0 warnings, 0 errors)
- Targeted tests: `MabArchiveLoadOrchestratorParityTests` -> passed (5/5)
- Complementary scheduler tests: `JobOrchestratorTests` -> passed (14/14)
- Aggregate targeted test pass count: 19/19

## Closure Checklist to Reach 100%
- Add targeted parity tests for branch order, delete coverage, totals formulas, and loader order/coverage.
- Reclassify validated rows from Partial to Converted only after tests pass.

## Owner Notes
- This tracker is intentionally strict: implementation without executable evidence remains Partial.
- The current state is materially different from the earlier tracker: nothing is missing anymore, but validation still remains to be done.
