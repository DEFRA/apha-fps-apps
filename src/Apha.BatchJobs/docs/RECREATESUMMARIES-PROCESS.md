# RecreateSummaries Process

## Purpose

RecreateSummaries is a user-initiated batch process intended to rebuild FPS summary outputs used by reporting and analytics.

## Ownership

- **Business Owner:** PACT API
- **Current Development Home:** Apha.BatchJobs.Api (transitional; will migrate to PACT API repository post-development)

## Current Runtime Status (Codebase)

- Trigger pipeline is implemented end-to-end (API pre-check, trigger, worker startup, orchestrator lifecycle).
- The current RecreateSummaries handler is a foundation placeholder and does not yet execute business SQL steps.
- Locking, execution tracking, and completion status updates still run through the shared orchestrator.

Code references:

- API trigger endpoint: Apha.BatchJobs.Api/Controllers/JobStatusController.cs
- Worker entrypoint and job dispatch: Apha.BatchJobs.Worker/Program.cs
- Orchestration lifecycle (lock, execution record, retry, release): Apha.BatchJobs.Application/JobOrchestrator.cs
- RecreateSummaries placeholder handler: Apha.BatchJobs.Application/Jobs/ManualJobs/RecreateSummaries/RecreateSummaries/RecreateSummariesHandler.cs

## Trigger and Execution Flow

1. PACT API calls POST /api/batch-jobs/RecreateSummaries/trigger (currently routed via Apha.BatchJobs.Api during development).
2. API checks job status and current lock ownership.
3. If running, API returns conflict; if idle, API accepts and dispatches worker task.
4. Worker starts with BATCH_JOB_NAME=RecreateSummaries and BATCH_RUN_MODE=Manual.
5. Worker calls JobOrchestrator, which performs:
   - lock acquisition
   - execution record create/update
   - job handler execution
   - lock release
6. Current handler returns quickly (placeholder), so only framework lifecycle executes today.

**Transition Plan:** Post-development, trigger endpoint ownership will move to PACT API repository; Apha.BatchJobs.Api will remain the shared worker/orchestrator host.

## SQL Object Inventory for RecreateSummaries (From Business Mapping)

Database: FPS2025
Target Bucket: Adhoc / User-Initiated
Execution Target: BatchJobs Console App
In Scope: Yes

| Object Name | Object Type | Classification |
|---|---|---|
| sp_CreateProjectMonthCasework | Stored Procedure | Data Builder |
| sp_CreateTimeCostCalcs | Stored Procedure | Data Builder |
| sp_DeleteProjectMonth3 | Stored Procedure | Data Cleaner |
| sp_DeleteProjectMonthCasework | Stored Procedure | Data Cleaner |
| sp_DeleteProjectMonthFinal | Stored Procedure | Data Cleaner |
| sp_InsertMissingProjects | Stored Procedure | General Procedure |
| sp_RecreateSummaries | Stored Procedure | Orchestrator |
| sp_deleteProjectMonth2 | Stored Procedure | Data Cleaner |
| sp_deleteTimeCostCalcs | Stored Procedure | Data Cleaner |
| sp_qryJobMonthCum | Stored Procedure | General Procedure |
| sp_qryJobMonth_Final | Stored Procedure | General Procedure |
| sp_qryJobMonth_Single | Stored Procedure | General Procedure |
| usp_LogRecreateSummaries | Stored Procedure | User Procedure |
| usp_Refresh_Period_MO | Stored Procedure | User Procedure |
| usp_Refresh_Period_PSC | Stored Procedure | User Procedure |
| usp_Refresh_Period_TCC | Stored Procedure | User Procedure |

## Intended Functional Grouping

- Orchestration:
  - sp_RecreateSummaries
- Data cleaning/reset:
  - sp_DeleteProjectMonth3
  - sp_DeleteProjectMonthCasework
  - sp_DeleteProjectMonthFinal
  - sp_deleteProjectMonth2
  - sp_deleteTimeCostCalcs
- Data build/rebuild:
  - sp_CreateProjectMonthCasework
  - sp_CreateTimeCostCalcs
- Reconciliation and monthly projection:
  - sp_InsertMissingProjects
  - sp_qryJobMonth_Single and cumulative/final monthly procedures
- User-facing refresh and audit:
  - usp_Refresh_Period_MO
  - usp_Refresh_Period_PSC
  - usp_Refresh_Period_TCC
  - usp_LogRecreateSummaries

## SQL Baseline Documentation

All 16 RecreateSummaries stored procedures have been documented and stored as baseline in:

**File:** [src/Apha.BatchJobs/database/sql/200_recreate_summaries_procedures.sql](../../database/sql/200_recreate_summaries_procedures.sql)

This includes:

The orchestration sequence (from sp_RecreateSummaries) is:
### External Dependencies

The baseline SQL references 31 external objects (views, tables, procedures) not defined in the file. These must be sourced from FPS2025 schema:

**Complete inventory:** [RECREATESUMMARIES-SQL-DEPENDENCIES.md](./RECREATESUMMARIES-SQL-DEPENDENCIES.md)

**Quick summary:**
- **2 Views/Queries:** qryProjectMonthCW, vPacttblStaff
- **26 Tables:** ProjectMonth*, TimeCostCalcs, tlkpProject, Period_*, etc.
- **3 Procedures:** sp_deleteFPSTotals, sp_createFPSTotals, sp_Get_SP_No

The orchestration sequence (from sp_RecreateSummaries) is:
1. sp_deleteFPSTotals / sp_createFPSTotals
2. sp_InsertMissingProjects
3. sp_deleteTimeCostCalcs / sp_CreateTimeCostCalcs
4. sp_DeleteProjectMonthCasework / sp_CreateProjectMonthCasework
5. sp_DeleteProjectMonthFinal / sp_deleteProjectMonth2
6. sp_qryJobMonth_Single / sp_DeleteProjectMonth3
7. sp_qryJobMonthCum / sp_qryJobMonth_Final
8. usp_LogRecreateSummaries
9. (Conditional) usp_Refresh_Period_MO / PSC / TCC (only if period not locked)

## Expected Control Points

- Pre-run guard: lock prevents concurrent RecreateSummaries runs.
- Runtime observability: execution record stores run state and outcome.
- Post-run state: lock released regardless of success/failure.
- Business-level audit trail should be written via usp_LogRecreateSummaries once SQL implementation is wired.

## Implementation Roadmap

### Current Status

- ✅ SQL procedures baseline documented and versioned
- ✅ Orchestration sequence and control flow documented
- ⏳ RecreateSummaries handler is a foundation placeholder (business logic not yet wired)

### Next Implementation Steps

1. **Phase 1: Service Layer** — Implement `IRecreateSummariesService` in Apha.BatchJobs.Application
   - Orchestrate stored procedure execution in documented sequence
   - Handle transaction boundaries and error scenarios
   - Wire parameter passing (e.g., @Month to sp_qryJobMonth_Final, usp_Refresh_Period_* procedures)

2. **Phase 2: Handler Wiring** — Update RecreateSummariesHandler.ExecuteAsync()
   - Inject `IRecreateSummariesService`
   - Call service orchestration with cancellation token support
   - Emit structured logs per phase

3. **Phase 3: Testing & Validation**
   - Unit tests: service logic and parameter contracts
   - Integration tests: lock behavior, sequence ordering, transaction safety
   - End-to-end: API trigger → worker → SQL execution → status persistence
   - Validate row-level effects in target FPS2025 tables

4. **Phase 4: Deployment & Operations**
   - Deploy 200_recreate_summaries_procedures.sql to FPS2025 target database
   - Wire PACT API trigger (currently via Apha.BatchJobs.Api)
   - Monitor execution logs and audit trail (RecreateSummaries_Log table)
