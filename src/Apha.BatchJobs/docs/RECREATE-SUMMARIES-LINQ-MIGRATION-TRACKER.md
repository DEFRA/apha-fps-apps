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
| 6 | Implement LINQ refresh steps 15-17 | Completed | Completed | Added LINQ implementations for refresh steps and switched LINQ catalog refresh path from SQL adapters to LINQ. |
| 7 | Wire runtime mode selector | Completed | Completed | Added explicit DotNetLinq runtime selection in DI and updated debug/docs to use it. |
| 8 | Build parity test harness | Completed | Completed | Added an opt-in xUnit parity harness that runs SQL baseline and DotNetLinq back to back, snapshots target tables, compares hashes, and writes a JSON report. |
| 9 | Run full parity test suite | Completed | Completed | Ran the opt-in parity harness against SqlFiles and DotNetLinq; fixed the worktree-safe repo-root lookup and PostgreSQL money-column translation issues, then confirmed identical snapshots. |

## Validation Evidence

Step 1 validation checklist:

- Confirmed mode selector supports SQL baseline mode through configuration key BatchJobs:RecreateSummariesImplementationMode.
- Added explicit debug profile named BatchJobs Worker - RecreateSummaries (SQL Baseline) in .vscode/launch.json.
- Profile forces BatchJobs__RecreateSummariesImplementationMode=SqlFiles for reproducible baseline runs.

Validation outcome: Pass.

Step 8 validation checklist:

- Added reusable RecreateSummariesParityHarness in the unit test project.
- Harness runs SqlFiles baseline and DotNetLinq candidate sequentially, resets target tables between runs, snapshots target tables, compares row-count and hash parity, and writes a JSON report under docs/database/validation.
- Added opt-in xUnit wrapper RecreateSummariesParityHarnessTests gated by RUN_RECREATE_SUMMARIES_PARITY=true so Step 9 can run parity intentionally against a real seeded database.
- Added focused selector coverage in ServiceCollectionSetupTests to validate DotNetLinq resolves the LINQ step catalog.
- Verified no diagnostics errors in the new harness and test files.
- Verified dotnet test passes for ConfigureBatchJobServices_WhenRecreateSummariesModeIsDotNetLinq_ShouldResolveLinqCatalog.

Validation outcome: Pass.

Step 7 validation checklist:

- Updated DI runtime selector to support DotNetLinq and Linq aliases.
- Preserved SqlFiles and existing DotNet SQL-based path, with DotNetSql alias for clarity.
- Updated RecreateSummaries debug launch profile to explicitly use DotNetLinq.
- Updated BatchJobs README to document supported implementation mode names.
- Verified no diagnostics errors in changed files.
- Verified worker build succeeds after selector changes.

Validation outcome: Pass.

Step 6 validation checklist:

- Added LINQ implementations for refresh steps 15-17:
	- RefreshPeriodMo
	- RefreshPeriodPsc
	- RefreshPeriodTcc
- Expanded period target models and DbContext mappings for period_monthlyoutput, period_proj_subcontract, and period_timecostcalcs to include full inserted payload columns.
- Updated LinqRecreateSummariesStepCatalog refresh pipeline to run LINQ implementations for steps 15-17.
- Verified no analyzer or compile errors in changed infrastructure/data files.
- Verified worker build succeeds after Step 6 implementation.

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

Step 9 validation checklist:

- Ran the opt-in parity harness with RUN_RECREATE_SUMMARIES_PARITY=true and a real PostgreSQL batch_jobs_foundation_db connection.
- Confirmed the harness runs SqlFiles and DotNetLinq back to back, resets target tables between runs, and compares row counts plus SHA256 hashes.
- Fixed repo-root resolution for the git worktree layout by locating the BatchJobs.csproj directory and returning its parent repo root.
- Fixed PostgreSQL money-column translation issues in the LINQ implementations by materializing raw rows first and moving defaults/aggregations into C# where needed.
- Verified the full parity test passes with 1 test succeeded and 0 failed.

Validation outcome: Pass.

## Change Log

- 2026-05-13: Created tracker and marked Step 1 complete.
- 2026-05-13: Added SQL baseline debug profile in .vscode/launch.json.
- 2026-05-13: Completed Step 2 abstraction layer design and wiring for orchestrator/catalog execution pipeline.
- 2026-05-13: Completed Step 3 by adding missing RecreateSummaries table/view models and DbContext mappings.
- 2026-05-13: Completed Step 4 by implementing LINQ execution steps for RecreateSummaries steps 1-7.
- 2026-05-13: Completed Step 5 by implementing LINQ execution steps for RecreateSummaries steps 8-14.
- 2026-05-13: Completed Step 6 by implementing LINQ execution steps for RecreateSummaries refresh steps 15-17.
- 2026-05-13: Completed Step 7 by wiring runtime selection for DotNetLinq and updating the debug profile/docs.
- 2026-05-13: Completed Step 8 by adding an opt-in parity harness, JSON report generation, and focused selector validation in the unit test project.
- 2026-05-13: Completed Step 9 by fixing parity harness repo-root detection for the git worktree layout, resolving PostgreSQL money-column translation issues in the LINQ steps, and passing the full parity suite.
