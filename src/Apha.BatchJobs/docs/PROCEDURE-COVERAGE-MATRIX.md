# Stored Procedure Coverage Matrix (From Provided Scope List)

Status date: 2026-04-29
Source: user-provided in-scope object list image (Scheduled Batch Jobs bucket)

## Purpose

This matrix records, for each listed legacy procedure:
- current implementation coverage in this branch,
- intended owner job,
- whether it is safe to schedule as a standalone executable unit.

## Status Legend

- Converted: behavior implemented in current .NET flow and considered mapped.
- Partial: behavior exists, but not yet full legacy parity.
- Missing: no implementation found in executable job code.

## Scheduling Safety Legend

- Yes: can be scheduled as top-level orchestration.
- No: do not schedule standalone; belongs inside parent orchestration.
- No (not implemented): cannot be scheduled because no job implementation exists.

## Coverage Summary (This 32-row list)

- Converted: 2
- Partial: 7
- Missing: 23

## Procedure Matrix

| Object Name | Classification (Provided) | Status | Owner Job | Standalone Scheduling Safe? | Notes |
|---|---|---|---|---|---|
| sp_createFPSTotals | Data Builder | Partial | ScheduledLoadFromFps | No | Mapped via RebuildYearTotalsAsync in repository, but formula/source parity not complete. |
| sp_deleteFPSTotals | Data Cleaner | Partial | ScheduledLoadFromFps | No | Delete is represented inside totals rebuild flow; legacy semantics not fully proven equivalent. |
| spSendProjectEditMilestoneLink | Notification | Missing | None | No (not implemented) | No dedicated notification batch job implementation found. |
| spSendProjectManagerEditEmail | Notification | Missing | None | No (not implemented) | No dedicated notification batch job implementation found. |
| spSendProjectReportNotification_ProgM | Notification | Missing | None | No (not implemented) | No dedicated notification batch job implementation found. |
| sp_AddG_tlkpProject | Data Loader | Converted | ScheduledLoadFromFps | No | Implemented in RefreshCurrentYearProjectAllAsync (g_tlkpproject upsert). |
| sp_AddMY_FPSYearTotals | Data Loader | Partial | ScheduledLoadFromFps | No | Implemented via AddArchiveYearSliceAsync, parity still partial. |
| sp_AddMY_MonthlyOutput | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_MonthlyTime | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_ProfitCentreGrade | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_Proj_Invoice | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_Proj_SubContract | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_ProjectMonthFinal | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_Staff | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_TestOrProduct | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_TimeCostCalcs | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_WorkGroup | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_WorkGroupGrade | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_tblAdditionalCosts | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_tblAnimalReq | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_tblAnimals | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_tblContract | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_tblProfitCentre | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_tblStaffJob | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_tlkpProgram | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddMY_tlkpProject | Data Loader | Converted | ScheduledLoadFromFps | No | Implemented in AddArchiveYearSliceAsync (my_tlkpproject load/upsert). |
| sp_AddMY_tlkpProject_All | Data Loader | Partial | ScheduledLoadFromFps | No | Current-year path implemented; full yearly fan-out parity still incomplete. |
| sp_AddMY_tlkpTestReqmt | Data Loader | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |
| sp_AddYearsFPSData | Data Loader | Partial | ScheduledLoadFromFps | No | Parent fan-out partially represented by AddArchiveYearSliceAsync + refresh current-year project all. |
| sp_DeleteYearsFPSData | Data Cleaner | Partial | ScheduledLoadFromFps | No | Current code deletes limited archive footprint vs legacy broad wipe. |
| sp_LoadFromFPS | General Procedure | Partial | ScheduledLoadFromFps | Yes | Top-level orchestration exists with deterministic 5-step plan and cutover branch. |
| sp_addMY_YearDetails | General Procedure | Missing | ScheduledLoadFromFps | No (not implemented) | Not implemented in repository or handlers. |

## Key Governance Rules

1. Schedule only the parent orchestration (sp_LoadFromFPS equivalent), not individual child loaders/cleaners.
2. Keep notification procedures separated into a dedicated notification job family when implemented.
3. Treat all "Missing" entries as backlog scope before claiming full legacy parity.

## Evidence Basis

- src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/ScheduledLoadFromFpsRepository.cs
- src/Apha.BatchJobs/Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsJobHandler.cs
- src/Apha.BatchJobs/Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/ScheduledLoadFromFpsPlanBuilder.cs
- src/Apha.BatchJobs/docs/SP-TO-DOTNET-PARITY-TRACKER.md
