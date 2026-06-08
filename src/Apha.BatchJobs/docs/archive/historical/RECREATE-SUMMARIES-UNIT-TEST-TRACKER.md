# RecreateSummaries Unit Test Tracker

Last updated: 2026-05-26
Owner: BatchJobs team
Scope: RecreateSummaries process (LINQ implementation)

## 1) Framework Strategy (Aligned with Sibling Repos)

Observed sibling strategy across Apha.PACT, Apha.FPS, Apha.Costbook, Apha.PIMS, Apha.FPSApps:
- Test framework: xUnit (`[Fact]`, `[Theory]`)
- Mocking: NSubstitute (primary), Moq in some DataAccess projects
- Assertions: xUnit Assert; FluentAssertions used in richer suites
- Naming style: `MethodName_WhenCondition_ExpectedResult` (or close equivalent)

Decision for BatchJobs RecreateSummaries tests:
- Keep xUnit + NSubstitute as primary style (already used in BatchJobs tests)
- Add FluentAssertions only where it improves readability for deep object assertions
- Keep test names explicit and behavior-oriented

## 2) Objectives

- Validate SQL parity-critical logic in LINQ implementation
- Lock down orchestration order and period-lock branch behavior
- Improve coverage for RecreateSummaries orchestration + step logic
- Prevent regressions in financial totals and cumulative outputs

## 3) Coverage Focus Areas

Primary files to raise coverage:
- Apha.BatchJobs.Infrastructure/Repositories/RecreateSummaries/RecreateSummariesOrchestrator.cs
- Apha.BatchJobs.Infrastructure/Repositories/RecreateSummaries/RecreateSummariesStepCatalog.cs
- Apha.BatchJobs.Infrastructure/Repositories/RecreateSummaries/CreateFpsTotalsStep.cs
- Apha.BatchJobs.Infrastructure/Repositories/RecreateSummaries/CreateTimeCostCalcsStep.cs
- Apha.BatchJobs.Infrastructure/Repositories/RecreateSummaries/CreateProjectMonthSingleStep.cs
- Apha.BatchJobs.Infrastructure/Repositories/RecreateSummaries/CreateProjectMonthCumulativeStep.cs
- Apha.BatchJobs.Infrastructure/Repositories/RecreateSummaries/CreateProjectMonthFinalStep.cs
- Apha.BatchJobs.Infrastructure/Repositories/RecreateSummaries/LogRecreateSummariesStep.cs
- Apha.BatchJobs.Infrastructure/Context/RecreateSummariesContext.cs

## 4) Tracker (Phased Backlog)

Legend:
- Status: Not Started | In Progress | Blocked | Done
- Priority: P0 (critical), P1 (high), P2 (nice to have)

| ID | Phase | Priority | Test Group | Status | Notes |
|---|---|---|---|---|---|
| RS-001 | Phase 1 | P0 | Step catalog mandatory order is strict (ProjectMonth2 -> ProjectMonth3 -> ProjectMonthFinal dependency preserved) | Done | Covered in RecreateSummariesStepCatalogTests |
| RS-002 | Phase 1 | P1 | Step catalog refresh order fixed (Mo -> Psc -> Tcc) | Done | Covered in RecreateSummariesStepCatalogTests |
| RS-003 | Phase 1 | P1 | RecreateSummariesContext defaults (Month=1, TriggeredBy=system) | Done | Covered in RecreateSummariesContextTests |
| RS-004 | Phase 1 | P1 | RecreateSummariesContext env overrides valid values | Done | Covered in RecreateSummariesContextTests |
| RS-005 | Phase 1 | P1 | RecreateSummariesContext rejects invalid month values | Done | Covered in RecreateSummariesContextTests |
| RS-006 | Phase 1 | P1 | ExecutionStepBase success result mapping | Done | Covered in RecreateSummariesExecutionStepContractTests via derived test step |
| RS-007 | Phase 1 | P1 | ExecutionStepBase failure result mapping | Done | Covered in RecreateSummariesExecutionStepContractTests via throwing derived test step |
| RS-008 | Phase 2 | P0 | Orchestrator executes mandatory steps in order | Done | Covered in RecreateSummariesOrchestratorIntegrationTests (unlocked flow with ordered mandatory + refresh execution) |
| RS-009 | Phase 2 | P0 | Orchestrator rolls back when mandatory step fails | Done | Covered in RecreateSummariesOrchestratorIntegrationTests (failed mandatory step throws and short-circuits remaining pipeline) |
| RS-010 | Phase 2 | P0 | Orchestrator executes refresh steps when period unlocked | Done | Covered in RecreateSummariesOrchestratorIntegrationTests (periodLocked == 0 executes refresh list) |
| RS-011 | Phase 2 | P0 | Orchestrator marks refresh steps skipped when period locked | Done | Covered in RecreateSummariesOrchestratorIntegrationTests (periodLocked != 0 returns skipped refresh StepResults) |
| RS-012 | Phase 2 | P1 | Orchestrator handles unexpected exception with rollback attempt | Done | Covered in RecreateSummariesOrchestratorIntegrationTests (unexpected exception after first success propagates via catch-guard branch) |
| RS-013 | Phase 2 | P1 | Orchestrator tracker-clearing behavior (start/failure/commit) | Not Started | No tracked-entity leak |
| RS-014 | Phase 3 | P0 | CreateFpsTotals null handling parity (`ISNULL`/`COALESCE` vs `??`) | Not Started | Financial baseline sensitivity |
| RS-015 | Phase 3 | P0 | CreateFpsTotals left joins preserve rows | Not Started | Missing related aggregates still keep projects |
| RS-016 | Phase 3 | P0 | CreateProjectMonthSingle left joins + default values | Not Started | High risk for row loss |
| RS-017 | Phase 3 | P0 | CreateProjectMonthCumulative aggregation and defaults | Not Started | Numeric parity checks |
| RS-018 | Phase 3 | P0 | CreateProjectMonthFinal month cutoff logic (`MonthNo <= month` else null) | In Progress | PostgreSQL-backed assertion test added in RecreateSummariesPostgresStepIntegrationTests with strict skip semantics when DB prerequisites are missing |
| RS-019 | Phase 3 | P1 | CreateTimeCostCalcs defra vs non-defra charge rate path | Not Started | Branch correctness |
| RS-020 | Phase 3 | P1 | LogRecreateSummaries writes user + period + timestamp | Done | Covered in RecreateSummariesExecutionStepContractTests success-path execution |
| RS-021 | Phase 4 | P0 | Sparse-data scenario does not silently drop expected rows | Not Started | Join/null regression guard |
| RS-022 | Phase 4 | P0 | Locked-period scenario does not refresh period tables | Not Started | Safety guarantee |
| RS-023 | Phase 4 | P1 | Unlocked-period scenario refreshes all period tables | In Progress | PostgreSQL-backed refresh step tests added for MO/PSC/TCC in RecreateSummariesPostgresStepIntegrationTests with strict skip semantics when DB prerequisites are missing |
| RS-024 | Phase 4 | P1 | Boundary month scenarios (1, 12) | Not Started | Edge-case reliability |
| RS-025 | Phase 4 | P1 | Failure injection mid-pipeline leaves no partial writes | Not Started | Atomicity guarantee |

## 5) Suggested Test File Layout

Suggested location: `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/RecreateSummaries/`

- RecreateSummariesStepCatalogTests.cs
- RecreateSummariesContextTests.cs
- RecreateSummariesExecutionStepContractTests.cs
- RecreateSummariesOrchestratorTests.cs
- CreateFpsTotalsStepTests.cs
- CreateTimeCostCalcsStepTests.cs
- CreateProjectMonthSingleStepTests.cs
- CreateProjectMonthCumulativeStepTests.cs
- CreateProjectMonthFinalStepTests.cs
- LogRecreateSummariesStepTests.cs

## 6) Data/Scenario Checklist (SQL Parity Sensitive)

Use baseline SQL docs for expected behavior reference:
- `src/Apha.BatchJobs/docs/Baseline`

Minimum data scenarios to seed in step-level tests:
- Null-heavy monetary fields
- Missing related rows for left-joined sources
- Mixed month data with cutoff before/after row month
- Defra and non-defra projects
- Both locked and unlocked period rows

## 7) Coverage Tracking

Initial target (incremental):
- RecreateSummaries-related files: >= 80% line coverage
- Orchestrator + catalog + context: >= 90% line coverage

Stretch target:
- RecreateSummaries-related files: >= 90% line coverage

Run suggestion:
- `dotnet test src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj`
- Then run coverage command adopted by team CI profile (if available in pipeline)

## 8) Execution Plan (Recommended Order)

1. Implement Phase 1 tests (fast, no DB complexity)
2. Implement Phase 2 orchestrator branch tests
3. Implement P0 tests in Phase 3 (nulls/joins/cutoff)
4. Add Phase 4 scenario tests to catch integration regressions
5. Review coverage report and add targeted tests for uncovered branches

## 9) Sign-off Criteria

All must be true:
- RS-001, RS-008, RS-010, RS-011, RS-014, RS-016, RS-018, RS-021, RS-022 are Done
- No failing tests in BatchJobs.UnitTests
- Coverage target met for RecreateSummaries-related files
- No parity-critical behavior drift versus SQL baseline
