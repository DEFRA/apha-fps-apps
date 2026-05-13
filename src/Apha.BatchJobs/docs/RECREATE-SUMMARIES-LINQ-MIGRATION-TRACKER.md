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
| 3 | Add missing FPS model mappings | Not started | Not started | Pending user approval to start. |
| 4 | Implement LINQ steps 1-7 | Not started | Not started | Pending user approval to start. |
| 5 | Implement LINQ steps 8-14 | Not started | Not started | Pending user approval to start. |
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
