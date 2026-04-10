# Batch Solution Rollout Plan (Draft)

## Purpose
Create and release BatchJobs code in a controlled, low-risk way by separating structure setup, AppMod generation, storage of generated output, and phased release.

## Scope
- This is a planning and execution framework.
- This version captures the initial 4-step approach.
- Detailed technical tasks, owners, and dates can be added after further input.

## Consolidated Documentation Baseline
This file is the active execution plan and consolidates the delivery view that was previously tracked in separate analysis notes.

Active markdown set for `Apha.BatchJobs`:
- `BATCHJOBS_ARCHITECTURE_GUIDE.md` for standards and target architecture patterns.
- `BATCH-SOLUTION-ROLLOUT-PLAN.md` (this file) for delivery sequencing, procedure inventory, and release controls.

AppMod baseline summary (consolidated):
- AppMod output is useful as a structural starter but not release-ready on its own.
- Critical runnability elements must be validated in controlled phases (host wiring, project metadata, config, test gates).
- Procedure migration is executed incrementally by orchestrator-led waves, not as one bulk drop.

## Guiding Principles
- Align with existing solution conventions.
- Keep generated output reproducible and traceable.
- Avoid large one-shot merges.
- Release in small, testable increments.

## Step 1: Create a Blank/Skeleton Batch Solution Structure
### Objective
Create an empty structure that matches the overall repository and solution conventions.

### Target Outcome
A baseline BatchJobs solution skeleton exists with correct naming, folders, and placeholder projects, but with no business logic yet.

### Proposed Structure
- src/Apha.BatchJobs/
- src/Apha.BatchJobs/Apha.BatchJobs.Console/
- src/Apha.BatchJobs/Apha.BatchJobs.Console.UnitTests/
- src/Apha.BatchJobs/Apha.BatchJobs.Console.sln

### Controls
- Naming follows solution conventions.
- Nullable and implicit usings enabled in all new project files.
- Initial build must succeed before moving to Step 2.

### Exit Criteria
- Solution and project scaffolding committed.
- Build and restore pass.
- No business feature code introduced at this step.

## Step 2: Generate AppMod Output As Close To Target Solution As Possible
### Objective
Run AppMod against the agreed target shape so generated code requires minimum rework.

### Target Outcome
Generated output matches target architecture, naming, and project boundaries as closely as possible.

### Preparation Checklist
- Confirm expected layers and namespaces.
- Confirm target framework and packages.
- Confirm provider assumptions (SQL Server or PostgreSQL).
- Confirm mandatory components (Program, csproj files, config files, logging, job host).

### Controls
- Record prompt/config used to run AppMod.
- Keep raw output untouched initially.
- Run a first-pass structural validation after generation.

### Exit Criteria
- AppMod generation artifact collected.
- Structural comparison against skeleton completed.
- Gaps logged (missing files, mismatched layers, incomplete procedures).

## Step 3: Store AppMod Output As A Baseline Artifact
### Objective
Preserve generated output for audit, repeatability, and comparison.

### Target Outcome
A stable, immutable baseline of generation output is saved and traceable.

### Storage Strategy
- Keep output in a dedicated location under src/appmod-generated/.
- Include generation metadata:
  - generation date/time
  - source prompt/config summary
  - tool/package version (if available)
  - known limitations discovered in first pass

### Controls
- Do not manually edit baseline output in place.
- Any adaptation happens in controlled integration branches/folders.

### Exit Criteria
- Baseline stored and documented.
- Metadata note added.
- Comparison-ready state achieved.

## Step 4: Release Changes In Controlled Phases
### Objective
Integrate and release with strict scope control, test gates, and rollback readiness.

### Controlled Delivery Tracks
This rollout has two parallel delivery tracks:
- Scheduled Batch Jobs Development
- Adhoc Batch Jobs Development

## Track 1: Scheduled Batch Jobs Development
### Phase 1: Foundation and Infra Setup
- Solution setup (Console app and structure)
- Logging framework (Serilog and App Insights)
- Config management (ENV, secrets, DB)
- DB connectivity and repository layer
- Dockerization and ECR push

### Scheduled Procedure Inventory (Provided)
The following scheduled procedures are confirmed from the `Scheduled Batch Jobs` tab and are the current migration scope for Track 1.

Scheduled tab metrics:
- Total objects: 32
- Total estimated hours: 316
- Classification split: Data Loader (24), Notification (3), General Procedure (2), Data Cleaner (2), Data Builder (1)
- Complexity split: Low (20), Medium (12)

Notification procedures:
- spSendProjectEditMilestoneLink
- spSendProjectManagerEditEmail
- spSendProjectReportNotification_ProgM

Data loader procedures:
- sp_AddG_tlkpProject
- sp_AddMY_FPSYearTotals
- sp_AddMY_MonthlyOutput
- sp_AddMY_MonthlyTime
- sp_AddMY_ProfitCentreGrade
- sp_AddMY_Proj_Invoice
- sp_AddMY_Proj_SubContract
- sp_AddMY_ProjectMonthFinal
- sp_AddMY_Staff
- sp_AddMY_TestOrProduct
- sp_AddMY_TimeCostCalcs
- sp_AddMY_WorkGroup
- sp_AddMY_WorkGroupGrade
- sp_AddMY_tblAdditionalCosts
- sp_AddMY_tblAnimalReq
- sp_AddMY_tblAnimals
- sp_AddMY_tblContract
- sp_AddMY_tblProfitCentre
- sp_AddMY_tblStaffJob
- sp_AddMY_tlkpProgram
- sp_AddMY_tlkpProject
- sp_AddMY_tlkpProject_All
- sp_AddMY_tlkpTestReqmt
- sp_AddYearsFPSData

Additional scheduled procedures:
- sp_createFPSTotals
- sp_deleteFPSTotals

Cleanup and control procedures:
- sp_DeleteYearsFPSData
- sp_LoadFromFPS
- sp_addMY_YearDetails

Notes:
- The above list is captured from the provided planning image and is now the working migration list for Track 1.
- Exact naming, owner mapping, effort sizing, and run frequency should be validated from the source spreadsheet before development lock.

### Phase 2: Core Orchestrator Build (sp_LoadFromFPS)
- Main orchestrator build
- Scheduler integration (EventBridge and Fargate)
- Error handling and retry framework

### Phase 3: Data Load Modules
- Project data load jobs
- Financial data load jobs
- Time and output load jobs
- Reference data load jobs

### Phase 4: Cleanup and Validation Jobs
- Cleanup jobs (DeleteYearsFPSData)
- Validation and reconciliation
- Performance optimization

## Track 2: Adhoc Batch Jobs Development
### Phase 1: Orchestrator (sp_RecreateSummaries)
- Main orchestrator build
- CLI and manual trigger support

### Adhoc Procedure Inventory (Provided)
The following Adhoc procedures are confirmed from the `Adhoc Batch Jobs` tab and are the current migration scope for Track 2.

Adhoc tab metrics:
- Total objects: 24
- Total estimated hours: 276
- Classification split: Notification (8), Data Cleaner (5), User Procedure (4), General Procedure (4), Data Builder (2), Orchestrator (1)
- Complexity split: Low (15), Medium (9)

Core Adhoc data and orchestration procedures:
- sp_CreateProjectMonthCasework
- sp_CreateTimeCostCalcs
- sp_DeleteProjectMonth3
- sp_DeleteProjectMonthCasework
- sp_DeleteProjectMonthFinal
- sp_InsertMissingProjects
- sp_RecreateSummaries
- sp_deleteProjectMonth2
- sp_deleteTimeCostCalcs
- sp_qryJobMonthCum
- sp_qryJobMonth_Final
- sp_qryJobMonth_Single
- usp_LogRecreateSummaries
- usp_Refresh_Period_MO
- usp_Refresh_Period_PSC
- usp_Refresh_Period_TCC

Adhoc notification procedures:
- spResetSendEmail
- spSendProgramManagerReportEmail
- spSendProgramReportNotification
- spSendProjectManagerReportEmail
- spSendProjectReportNotification
- spSendRCManagerReportEmail
- spSendRCReportNotification
- spSendReportEmails_Manual

Notes:
- This inventory is transcribed from the provided planning image and is now the working list for Track 2.
- Exact procedure spellings and suffixes must be validated from the source spreadsheet before development lock.

### Phase 2: Core Modules
- Cleanup jobs
- Data rebuild jobs
- Calculation jobs
- Refresh jobs

### Phase 3: Notifications
- Email notification jobs
- Report hooks

### Phase 4: Testing and Hardening
- End-to-end testing
- Retry and performance optimization
- Deployment validation (Fargate)

## Adhoc Jobs Release Order (Controlled)
Planned release order for Adhoc jobs:
1. Main orchestrator spine (`sp_RecreateSummaries`) with CLI/manual trigger support.
2. Data cleanup and rebuild set (`sp_DeleteProjectMonth*`, `sp_deleteTimeCostCalcs`, `sp_InsertMissingProjects`).
3. Calculation and reconciliation set (`sp_CreateTimeCostCalcs`, query/recon procedures, refresh procedures).
4. Notification set (program/project/TCC/RC report notifications and manual report sender).
5. Final end-to-end test, retry tuning, and Fargate deployment validation.

## Control Gates For Every Phase
- Scope gate: only phase-approved modules are changed.
- Build gate: restore and build must pass.
- Test gate: agreed unit/integration checks must pass.
- Ops gate: logging, config, retry, and rollback path documented.
- Release gate: phase release note issued before merge.

## Scheduled Jobs Release Order (Controlled)
Planned release order for Scheduled jobs:
1. Orchestrator and scheduler spine (`sp_LoadFromFPS`) with no-op and dry-run support.
2. Notification set (3 procedures) with logging and retry controls.
3. Data loader core set (reference and lookup tables first).
4. Data loader financial and project aggregates.
5. Year-level operations (`sp_AddYearsFPSData`, `sp_DeleteYearsFPSData`).
6. Final validation, reconciliation, and performance hardening.

## Completion Criteria For Step 4
- Each phase is independently reviewed and signed off.
- Scheduled and Adhoc tracks have passed their Phase 4 hardening.
- Production release proceeds only after validation and rollback readiness are confirmed.

## Branching And Change Control
- Use a dedicated feature branch for BatchJobs rollout.
- Use small pull requests by phase.
- Require review checklist per PR:
  - architecture alignment
  - configuration safety
  - logging and exception handling
  - test evidence
  - migration impact

## Commit Tag And Release Strategy
### Release Intent
Support controlled deployment by phase, even when later phase code already exists in development branches.

### Branch Strategy
- Integration branch: `feature/batchjobs-mainline`
- Release branches by phase:
  - `release/batchjobs-foundation`
  - `release/batchjobs-scheduled-core`
  - `release/batchjobs-scheduled-loaders`
  - `release/batchjobs-adhoc-core`
  - `release/batchjobs-hardening`
- Hotfix branches when needed:
  - `hotfix/batchjobs-foundation-*`

### Commit Strategy
- Keep commits atomic and phase aligned.
- Do not mix foundation, conversion, and hardening in one commit.
- Commit categories:
  - `feat(batchjobs-foundation): ...`
  - `feat(batchjobs-scheduled): ...`
  - `feat(batchjobs-adhoc): ...`
  - `chore(batchjobs): ...`
  - `test(batchjobs): ...`
- Each commit must be deploy meaningful within its phase.

### Tag Strategy
- Use annotated tags at release points.
- Pre release tags:
  - `v0.1.0-foundation-rc1`
  - `v0.1.0-foundation-rc2`
- Release tags:
  - `v0.1.0-foundation`
  - `v0.2.0-scheduled-core`
  - `v0.3.0-scheduled-loaders`
  - `v0.4.0-adhoc-core`
  - `v1.0.0-batchjobs-ga`
- Patch tags for post release fixes:
  - `v0.1.1-foundation`
  - `v0.2.1-scheduled-core`

### Deployment Control Model
- Deploy only from release branches.
- Keep later phase work on integration branch until approved.
- Use release profile gates in pipeline:
  - `foundation`
  - `scheduled-core`
  - `scheduled-loaders`
  - `adhoc-core`
  - `hardening`
- Require manual approval for production deployment.

### Foundation First Deployment Policy
Release 1 deploys foundation only:
- Program host and startup wiring
- Dependency injection and configuration setup
- Logging and error handling baseline
- Scheduler and cli trigger contracts
- Docker and runtime baseline

Excluded from foundation release:
- Stored procedure conversion modules
- Data loader conversion logic
- Adhoc notification conversion modules

### Definition Of Done For Foundation Release
- Application starts in AWS runtime target.
- Configuration loads from approved environment and secret sources.
- Structured logs are visible in target log sink.
- Smoke execution path completes successfully.
- No destructive database operations are triggered.

### Promotion Flow
1. Build and validate on integration branch.
2. Cut release branch for target phase only.
3. Run phase specific pipeline gates.
4. Tag approved commit.
5. Deploy tagged release.
6. Merge release branch back to mainline with release notes.

## Documentation Artifacts To Maintain
- This rollout plan document.
- AppMod generation metadata note.
- Gap tracker between generated and target structure.
- Phase release notes.
- Test evidence summary.

## Initial Risks And Mitigations
- Risk: Generated output misses runnable essentials.
  - Mitigation: Step 1 skeleton and Phase B runtime enablement before feature integration.
- Risk: Large merge introduces instability.
  - Mitigation: Phased release with strict scope locks.
- Risk: Environment mismatch (provider/config/framework).
  - Mitigation: confirm assumptions in Step 2 preparation checklist.

## Remaining Clarifications (To Finalize This Plan)
1. Confirm target framework for BatchJobs.
2. Confirm primary DB provider for batch runtime path.
3. Confirm mandatory CI checks per phase.
4. Confirm production approval owners for release gates.
