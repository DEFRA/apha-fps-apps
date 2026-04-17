# SP to .NET Parity Tracker (ScheduledLoadFromFps)

Status date: 2026-04-17
Scope: legacy stored procedure chain for LoadFromFPS and yearly archive reload
Goal: 100% logic integrity (apple-to-apple parity)

## Status Legend
- Converted: equivalent behavior implemented and validated.
- Partial: some behavior implemented, but not full legacy equivalent.
- Missing: no equivalent implementation found.

## Summary
- Converted: 2
- Partial: 5
- Missing: 22
- Overall parity completion: 6.9% (2/29 fully converted)

## Master Procedure Parity

| Legacy SP | Expected Legacy Behavior | .NET Equivalent | Status | Gap / Note |
|---|---|---|---|---|
| sp_LoadFromFPS | Orchestrates yearly flow with conditional current-year branch | ScheduledLoadFromFpsJobHandler + plan builder | Partial | Sequence exists, but full fan-out parity and transactional parity are incomplete |
| sp_deleteFPSTotals | Clears FPSYearTotals before rebuild | RebuildYearTotalsAsync deletes fps.fpsyeartotals by year | Partial | Legacy delete semantics not fully proven equivalent across historical usage |
| sp_createFPSTotals | Rebuilds totals from source/calculated queries with legacy formulas | RebuildYearTotalsAsync | Partial | Uses simplified source mapping and null placeholders for cost components |
| sp_DeleteYearsFPSData | Broad year-slice wipe across archive footprint | DeleteArchiveYearSliceAsync | Partial | Only 3 archive tables deleted; legacy expects full archive table set |
| sp_AddYearsFPSData | Broad fan-out yearly reload by calling SP chain | AddArchiveYearSliceAsync + RefreshCurrentYearProjectAllAsync | Partial | Only subset of legacy loader targets implemented |

## Yearly Loader Chain Parity (sp_AddYearsFPSData children)

| Legacy SP | Target Pattern | .NET Equivalent | Status | Gap / Note |
|---|---|---|---|---|
| sp_AddMY_tlkpProgram | year-scoped load to my_tlkpprogram | none found | Missing | Not implemented |
| sp_AddG_tlkpProject | grouped project reference load | RefreshCurrentYearProjectAllAsync (g_tlkpproject upsert) | Converted | Implemented for current-year refresh path |
| sp_AddMY_tlkpProject | year-scoped load to my_tlkpproject | AddArchiveYearSliceAsync | Converted | Implemented for selected years |
| sp_AddMY_FPSYearTotals | year-scoped load to my_fpsyeartotals | AddArchiveYearSliceAsync | Partial | Loader exists but source/transformation parity not fully proven |
| sp_AddMY_MonthlyOutput | year-scoped load to my_monthlyoutput | none found | Missing | Not implemented |
| sp_AddMY_MonthlyTime | year-scoped load to my_monthlytime | none found | Missing | Not implemented |
| sp_AddMY_Proj_Invoice | year-scoped load to my_proj_invoice | none found | Missing | Not implemented |
| sp_AddMY_Proj_SubContract | year-scoped load to my_proj_subcontract | none found | Missing | Not implemented |
| sp_AddMY_ProjectMonthFinal | year-scoped load to my_projectmonthfinal | none found | Missing | Not implemented |
| sp_AddMY_tblAdditionalCosts | year-scoped load to my_tbladditionalcosts | none found | Missing | Not implemented |
| sp_AddMY_tblAnimalReq | year-scoped load to my_tblanimalreq | none found | Missing | Not implemented |
| sp_AddMY_tblContract | year-scoped load to my_tblcontract | none found | Missing | Not implemented |
| sp_AddMY_tblStaffJob | year-scoped load to my_tblstaffjob | none found | Missing | Not implemented |
| sp_AddMY_TimeCostCalcs | year-scoped load to my_timecostcalcs | none found | Missing | Not implemented |
| sp_AddMY_tlkpTestReqmt | year-scoped load to my_tlkptestreqmt | none found | Missing | Not implemented |
| sp_addMY_YearDetails | year-scoped load to tlkpyear and related year metadata | none found | Missing | Not implemented |
| sp_addMY_WorkGroupGrade | year-scoped load to my_workgroupgrade | none found | Missing | Not implemented |
| sp_addMY_ProfitCentreGrade | year-scoped load to my_profitcentregrade | none found | Missing | Not implemented |
| sp_AddMY_tblProfitCentre | year-scoped load to my_tblprofitcentre | none found | Missing | Not implemented |
| sp_AddMY_TestOrProduct | year-scoped load to my_testorproduct | none found | Missing | Not implemented |
| sp_AddMY_Staff | year-scoped load to my_staff | none found | Missing | Not implemented |
| sp_AddMY_Workgroup | year-scoped load to my_workgroup | none found | Missing | Not implemented |
| sp_AddMY_tblAnimals | year-scoped load to my_tblanimals | none found | Missing | Not implemented |
| sp_AddMY_tlkpProject_All | year-scoped load to my_tlkpproject_all | RefreshCurrentYearProjectAllAsync | Partial | Current-year path implemented, but full yearly fan-out parity incomplete |

## Cross-Validation Integrity

| Area | Current State | Status | Gap / Note |
|---|---|---|---|
| Validation assertion persistence | RunCrossValidationAsync writes assertion rows | Partial | Assertions exist but not tied to full legacy SP coverage |
| E2E validation fidelity | E2E uses pass-through/stub engines in key scenarios | Partial | Does not fully prove real DB validation integrity |
| Release gate behavior | Job fails when any assertion fails | Converted | Implemented in orchestrator |

## Evidence Sources
- Legacy SP chain and parity expectations: docs/SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md
- Legacy breadth statements (26 tables, 24+ loaders): docs/LEGACY-TO-CURRENT-TABLE-MAPPING.md
- Current implementation: Apha.BatchJobs.Infrastructure/Repositories/ScheduledLoadFromFpsRepository.cs
- Orchestrator/release gate: Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsJobHandler.cs
- E2E tests: Apha.BatchJobs.UnitTests/ScheduledLoadFromFps/E2E/ScheduledLoadE2ETests.cs

## Closure Checklist to Reach 100%
- Implement full year-slice delete parity across the complete legacy archive footprint.
- Implement missing yearly loaders for all legacy sp_AddMY_* targets.
- Align create totals logic with legacy source/calculation semantics.
- Validate transaction boundaries for delete/load fan-out parity.
- Replace pass-through E2E validation with real assertion-engine execution.
- Execute full build/test parity suite in environment with dotnet available.

## Owner Notes
- This tracker is intentionally strict: only full equivalence is marked Converted.
- Partial marks indicate logic exists but is not yet proven apple-to-apple.
