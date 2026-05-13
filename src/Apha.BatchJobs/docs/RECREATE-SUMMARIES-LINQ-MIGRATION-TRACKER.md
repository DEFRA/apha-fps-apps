# RecreateSummaries LINQ Migration Tracker

Status date: 2026-05-13
Owner: BatchJobs migration stream
Scope: Build a true parallel .NET and LINQ implementation for RecreateSummaries steps with no logic changes, then run parity testing.

## Execution Plan

1. Freeze SQL baseline behavior.
2. Design LINQ step abstractions.
3. Add missing FPS model mappings.
4. Implement LINQ steps 1-7.
5. Implement LINQ steps 8-14.
6. Implement LINQ refresh steps 15-17.
7. Wire runtime mode selector.
8. Build parity test harness.
9. Run full parity test suite.

## Tracker

| Step | Title | Status | Validation | Notes |
| --- | --- | --- | --- | --- |
| 1 | Freeze SQL baseline behavior | Completed | Completed | Added a dedicated SQL baseline launch profile to make baseline runs repeatable. |
| 2 | Design LINQ step abstractions | Completed | Completed | Added transport-agnostic execution contract and context, with SQL adapter bridge to preserve current behavior. |
| 3 | Add missing FPS model mappings | Completed | Completed | Added RecreateSummaries table/view models and DbContext mappings for step dependencies across 1-17. |
| 4 | Implement LINQ steps 1-7 | Completed | Completed | Added LINQ execution steps for 1-7 and a partial LINQ catalog that keeps 8-17 on SQL adapters. |
| 5 | Implement LINQ steps 8-14 | Completed | Completed | Added LINQ steps for 8-14 and switched mandatory pipeline in LINQ catalog to fully LINQ for 1-14. |
| 6 | Implement LINQ refresh steps 15-17 | Not started | Not started | Pending user approval to start. |
| 7 | Wire runtime mode selector | Not started | Not started | Pending user approval to start. |
| 8 | Build parity test harness | Not started | Not started | Pending user approval to start. |
| 9 | Run full parity test suite | Not started | Not started | Pending user approval to start. |

## Validation Evidence

Step 1 validation checklist:

- Confirmed mode selector supports SQL baseline mode through configuration key BatchJobs:RecreateSummariesImplementationMode.
- Added explicit debug profile named BatchJobs Worker - RecreateSummaries (SQL Baseline) in .vscode/launch.json.
- Profile forces BatchJobs__RecreateSummariesImplementationMode=SqlFiles for reproducible baseline runs.

Validation outcome: Pass.

Step 5 validation checklist:

- Added LINQ implementations for steps 8-14:
	- DeleteProjectMonthFinal
	- DeleteProjectMonth2
	- CreateProjectMonthSingle
	- DeleteProjectMonth3
	- CreateProjectMonthCumulative
	- CreateProjectMonthFinal
	- LogRecreateSummaries
- Expanded RecreateSummaries table models and DbContext mappings for projectmonth2/projectmonth3/projectmonthfinal to support full LINQ inserts for step 10/12/13 payload columns.
- Updated LinqRecreateSummariesStepCatalog so mandatory steps 1-14 execute via LINQ implementations.
- Kept refresh steps 15-17 on SQL adapters for Step 6.
- Verified no analyzer or compile errors in changed infrastructure/data files.
- Verified worker build succeeds after Step 5 implementation.

Validation outcome: Pass.

Step 4 validation checklist:

- Added LINQ step base: LinqRecreateSummariesExecutionStepBase.
- Added LINQ implementations for steps 1-7:
	- DeleteFpsTotals
	- CreateFpsTotals
	- InsertMissingProjects
	- DeleteTimeCostCalcs
	- CreateTimeCostCalcs
	- DeleteProjectMonthCasework
	- CreateProjectMonthCasework
- Added incremental LinqRecreateSummariesStepCatalog with steps 1-7 on LINQ and 8-17 on SQL adapters.
- Verified no analyzer or compile errors in RecreateSummaries infrastructure files.
- Verified worker build succeeds after Step 4 implementation.

Validation outcome: Pass.

Step 3 validation checklist:

- Added RecreateSummaries-specific data models in Apha.BatchJobs.Infrastructure/Data/RecreateSummariesTables.cs.
- Added DbSet registrations for RecreateSummaries tables/views in BatchJobsDbContext.
- Added model mappings for source/target tables and keyless views required by RecreateSummaries SQL dependency set.
- Added dedicated ConfigureRecreateSummariesModels model-builder section and invoked it from OnModelCreating.
- Verified no analyzer or compile errors in BatchJobsDbContext.cs and RecreateSummariesTables.cs.
- Verified worker build succeeds after mapping additions.

Validation outcome: Pass.

Step 2 validation checklist:

- Added execution abstraction contract IRecreateSummariesExecutionStep for implementation-agnostic step orchestration.
- Added RecreateSummariesExecutionContext to carry shared dependencies for both SQL and future LINQ steps.
- Added SqlRecreateSummariesExecutionStepAdapter to preserve existing SQL step classes while moving orchestrator/catalogs to the new contract.
- Updated IRecreateSummariesStepCatalog and both catalogs to emit execution steps via adapter wrapping.
- Updated RecreateSummariesOrchestrator to execute through execution context without changing transaction flow or step order.
- Verified no analyzer or compile errors in RecreateSummaries infrastructure files.
- Verified worker build succeeds after abstraction changes.

Validation outcome: Pass.

## Change Log

- 2026-05-13: Created tracker and marked Step 1 complete.
- 2026-05-13: Added SQL baseline debug profile in .vscode/launch.json.
- 2026-05-13: Completed Step 2 abstraction layer design and wiring for orchestrator/catalog execution pipeline.
- 2026-05-13: Completed Step 3 by adding missing RecreateSummaries table/view models and DbContext mappings.
- 2026-05-13: Completed Step 4 by implementing LINQ execution steps for RecreateSummaries steps 1-7.
- 2026-05-13: Completed Step 5 by implementing LINQ execution steps for RecreateSummaries steps 8-14.
