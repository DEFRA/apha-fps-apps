# MABArchive Unit Test Tracker

Last updated: 2026-05-27
Owner: BatchJobs team
Scope: MABArchive process (LINQ implementation)

## 1) Framework Strategy (Aligned with Existing BatchJobs Tests)

Observed strategy in current BatchJobs tests:
- Test framework: xUnit (`[Fact]`)
- Mocking: NSubstitute
- Assertions: xUnit Assert
- Naming style: `MethodName_WhenCondition_ExpectedResult`

Decision for MABArchive tests:
- Keep xUnit + NSubstitute as primary style.
- Keep orchestrator behavior tests fast and deterministic with mocked collaborators.
- Add PostgreSQL-backed integration tests selectively for parity-critical data behaviors.

## 2) Objectives

- Lock down orchestration parity for month-branch logic (`<=4` vs `>4`).
- Validate failure behavior (rollback trigger, notification path, exception propagation).
- Validate loader registration contract (24 loaders, contiguous sequence, expected names).
- Increase confidence in year-scoped delete/load/refresh behavior.
- Prevent regressions in high-risk loaders and totals rebuild logic.

## 3) Coverage Focus Areas

Primary files to raise coverage:
- Apha.BatchJobs.Application/Jobs/ScheduledJobs/MABArchive/MabArchiveLoadOrchestrator.cs
- Apha.BatchJobs.Application/Jobs/ScheduledJobs/MABArchive/MabArchiveJobHandler.cs
- Apha.BatchJobs.Infrastructure/Context/ExecutionYearContext.cs
- Apha.BatchJobs.Infrastructure/Repositories/MabArchive/MyFpsYearlyDataService.cs
- Apha.BatchJobs.Infrastructure/Repositories/MabArchive/ReloadFpsTotalsService.cs
- Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/MabArchiveLoaders.cs
- Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/*Loader.cs (24 loaders)

## 4) Tracker (Phased Backlog)

Legend:
- Status: Not Started | In Progress | Blocked | Done
- Priority: P0 (critical), P1 (high), P2 (nice to have)

| ID | Phase | Priority | Test Group | Status | Notes |
|---|---|---|---|---|---|
| MA-001 | Phase 1 | P0 | Orchestrator month `>4` runs previous-year and current-year full cycle in order | Done | Covered in MabArchiveLoadOrchestratorParityTests |
| MA-002 | Phase 1 | P0 | Orchestrator month `<=4` runs previous-year full cycle and current-year partial refresh only | Done | Covered in MabArchiveLoadOrchestratorParityTests |
| MA-003 | Phase 1 | P1 | Orchestrator skips unavailable previous year and continues current-year branch | Done | Covered in MabArchiveLoadOrchestratorParityTests |
| MA-004 | Phase 1 | P1 | Orchestrator skips unavailable current-year partial refresh branch | Done | Covered in MabArchiveLoadOrchestratorParityTests |
| MA-005 | Phase 1 | P0 | Orchestrator failure path sends notification and rethrows | Done | Covered in MabArchiveLoadOrchestratorParityTests |
| MA-006 | Phase 1 | P1 | Loader metadata contract: 24 execution loaders, unique names, contiguous sequence 1..24 | Done | Covered in MabArchiveLoaderMetadataTests |
| MA-007 | Phase 1 | P1 | DI registers only MABArchive loaders in default/configured mode | Done | Covered in ServiceCollectionSetupTests |
| MA-008 | Phase 2 | P0 | BuildExecutionContext month branch parity from runtime clock (`>4` vs `<=4`) | Done | Covered in MabArchiveExecutionContextTests |
| MA-009 | Phase 2 | P1 | BuildExecutionContext override parsing fallback behavior for invalid env values | Done | Covered in MabArchiveExecutionContextTests |
| MA-010 | Phase 2 | P1 | MabArchiveJobHandler resolves orchestrator, builds context, executes transaction wrapper once | Done | Covered in MabArchiveJobHandlerTests |
| MA-011 | Phase 2 | P1 | MabArchiveJobHandler cancellation/exception propagation behavior | Done | Covered in MabArchiveJobHandlerTests |
| MA-012 | Phase 2 | P1 | ExecutionYearContext default `YearSource` and mutable year behavior | Done | Covered in ExecutionYearContextTests |
| MA-013 | Phase 3 | P0 | MyFpsYearlyDataService constructor rejects loader registration mismatch count | Done | Covered in MyFpsYearlyDataServiceTests |
| MA-014 | Phase 3 | P0 | MyFpsYearlyDataService constructor rejects duplicate sequence values | Done | Covered in MyFpsYearlyDataServiceTests |
| MA-015 | Phase 3 | P0 | MyFpsYearlyDataService constructor rejects non-contiguous sequence values | Done | Covered in MyFpsYearlyDataServiceTests |
| MA-016 | Phase 3 | P1 | IsYearAvailableAsync handles missing year and positive year cases | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact) |
| MA-017 | Phase 3 | P1 | RefreshProjectAllOnlyAsync deletes target year and runs loader seq 24 only | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact) |
| MA-018 | Phase 3 | P1 | LoadYearDataAsync executes loaders in sequence order and aggregates rows | Done | Covered in MyFpsYearlyDataServiceTests |
| MA-019 | Phase 3 | P1 | LoadYearDataAsync stops on failing loader and includes loader metadata in error path | Done | Covered in MyFpsYearlyDataServiceTests |
| MA-020 | Phase 4 | P0 | ReloadFpsTotalsService strict-year-isolation view checks fail on missing fpsyear columns | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact, transactional view-shape mutation) |
| MA-021 | Phase 4 | P0 | ReloadFpsTotalsService rebuild totals formula parity (null handling and computed totals) | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact) |
| MA-022 | Phase 4 | P1 | ReloadFpsTotalsService no-source-rows path returns zero without insert | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact) |
| MA-023 | Phase 5 | P0 | PostgreSQL integration: DeleteYearDataAsync year-scoped deletes preserve out-of-year rows | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact, transactional) |
| MA-024 | Phase 5 | P0 | PostgreSQL integration: g_tlkpproject project-key delete semantics | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact, transactional) |
| MA-025 | Phase 5 | P0 | PostgreSQL integration: high-risk loader 10 (`my_tbladditionalcosts`) counter semantics | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact, transactional) |
| MA-026 | Phase 5 | P1 | PostgreSQL integration: `my_staff` name composition parity | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact, transactional) |
| MA-027 | Phase 5 | P1 | PostgreSQL integration: `tlkpyear` month lookup/cast parity | Done | Covered in MabArchivePostgresIntegrationTests (SkippableFact, transactional) |

## 5) Suggested Test File Layout

Suggested location: `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/MabArchive/`

- MabArchiveLoadOrchestratorParityTests.cs (existing)
- MabArchiveLoaderMetadataTests.cs (existing)
- MabArchiveExecutionContextTests.cs (new)
- MabArchiveJobHandlerTests.cs (new)
- ExecutionYearContextTests.cs (new)
- MyFpsYearlyDataServiceTests.cs (new)
- ReloadFpsTotalsServiceTests.cs
- MabArchivePostgresIntegrationTests.cs (new)

Note:
- Existing MABArchive tests currently live at the UnitTests root. Moving into a dedicated `MabArchive` folder is optional and can be done later without behavior changes.

## 6) Data/Scenario Checklist (Parity Sensitive)

Use baseline SQL docs for expected behavior reference:
- `src/Apha.BatchJobs/docs/Baseline`
- `src/Apha.BatchJobs/docs/MABARCHIVE-LINQ-CONVERSION-TRACKER.md`

Minimum scenarios to seed in step/service integration tests:
- Year exists vs year missing in source master.
- Month branch (`<=4` and `>4`) path behavior.
- Null-heavy totals source fields.
- Special delete semantics for `g_tlkpproject` by project key set.
- Loader ordering and failure injection at different sequence points.
- High-risk counter generation in `my_tbladditionalcosts`.

## 7) Coverage Tracking

Initial target (incremental):
- MABArchive orchestrator + handler + context files: >= 90% line coverage
- MABArchive services (`MyFpsYearlyDataService`, `ReloadFpsTotalsService`): >= 80% line coverage

Stretch target:
- MABArchive-related files overall: >= 90% line coverage

Run suggestion:
- `dotnet test src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj --filter FullyQualifiedName~MabArchive`
- Then run coverage command using the team CI profile where available.

## 8) Execution Plan (Recommended Order)

1. Finalize Phase 2 context/handler unit tests.
2. Complete Phase 3 service contract tests (constructor/ordering/failure guards).
3. Add Phase 4 totals parity-focused tests.
4. Add Phase 5 PostgreSQL integration tests for year-scope and high-risk parity areas.
5. Review coverage output and add targeted branch tests for remaining gaps.

## 9) Sign-off Criteria

All must be true:
- MA-001, MA-002, MA-005, MA-013, MA-014, MA-015, MA-021, MA-023, MA-025 are Done
- No failing tests in BatchJobs.UnitTests for MABArchive scope
- Coverage target met for MABArchive-related files
- No parity-critical behavior drift versus SQL baseline evidence

## 10) Verification Snapshot (2026-05-27)

Command run (status only):
- `dotnet test src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj --no-restore --filter FullyQualifiedName~Apha.BatchJobs.UnitTests.MabArchiveLoadOrchestratorParityTests -v minimal`
- Result: 5 total, 5 passed, 0 failed, 0 skipped.

Command run (status + coverage):
- `dotnet test Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj --no-restore --filter 'FullyQualifiedName~MabArchive|FullyQualifiedName~ServiceCollectionSetupTests.ConfigureBatchJobServices_DefaultMabArchiveMode_ShouldRegisterMabArchiveLoadersOnly|FullyQualifiedName~ServiceCollectionSetupTests.ConfigureBatchJobServices_WhenMabArchiveModeIsConfigured_ShouldStillRegisterMabArchiveLoadersOnly' --collect:"Code Coverage" -v minimal`
- Result: 8 total, 8 passed, 0 failed, 0 skipped.

Coverage artifact:
- `src/Apha.BatchJobs/TestResults/MabArchiveCoverage/mabarchive-coverage.cobertura.xml`
- Report summary: `src/Apha.BatchJobs/TestResults/MabArchiveCoverage/report/Summary.txt`

Observed coverage (selected, line coverage):
- Overall (report scope): 5.6%
- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveExecutionContext`: 75%
- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveLoadOrchestrator`: 73.3%
- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveJobHandler`: 0%
- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.MyFpsYearlyDataService`: 0%
- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.ReloadFpsTotalsService`: 0%

Observed unit test class coverage (selected):
- `Apha.BatchJobs.UnitTests.MabArchiveLoadOrchestratorParityTests`: 100%
- `Apha.BatchJobs.UnitTests.MabArchiveLoaderMetadataTests`: 90.3%
- `Apha.BatchJobs.UnitTests.ServiceCollectionSetupTests`: 49.5%

Reality check against targets in section 7:
- Target not yet met for orchestrator + handler + context (>= 90%).
- Target not yet met for services (>= 80%).
- Tracker rows marked Done indicate intended coverage completion, but measured coverage on 2026-05-27 does not yet support sign-off.

## 11) Wave Progress Snapshot (2026-05-27)

Wave 1 + Wave 2 test additions (new files):
- `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/ExecutionYearContextTests.cs`
- `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/EmailNotificationServiceTests.cs`
- `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/MabArchiveExecutionContextTests.cs`
- `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/MabArchiveJobHandlerTests.cs`
- `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/MyFpsYearlyDataServiceTests.cs`
- `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/ReloadFpsTotalsServiceTests.cs`

Latest comparable scoped run (localhost integration enabled, RecreateSummaries excluded):
- `dotnet test BatchJobs.sln --no-restore --filter "FullyQualifiedName!~RecreateSummaries" --collect:"Code Coverage"`
- Result: 86 total, 86 passed, 0 failed, 0 skipped.

Coverage trend (BatchJobs assembly only, RecreateSummaries excluded):
- Baseline before Wave 1: 51.0%
- After Wave 1: 58.1%
- After Wave 2: 62.2%

Current MABArchive-focused line coverage (selected):
- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveExecutionContext`: 100%
- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveJobHandler`: 94.2%
- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveLoadOrchestrator`: 92.5%
- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.EmailNotificationService`: 91.3%
- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.MyFpsYearlyDataService`: 88.3%
- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.ReloadFpsTotalsService`: 92.7%

Wave 2 testing note:
- New test coverage is EF/LINQ-first and localhost-backed; direct SQL usage in newly added tests was removed.

Wave 2 continuation snapshot (2026-05-27):
- Command run (Wave 2 suite with focused coverage): `dotnet test src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj --no-restore --filter "FullyQualifiedName~MabArchiveExecutionContextTests|FullyQualifiedName~MabArchiveJobHandlerTests|FullyQualifiedName~ExecutionYearContextTests|FullyQualifiedName~MyFpsYearlyDataServiceTests|FullyQualifiedName~ReloadFpsTotalsServiceTests|FullyQualifiedName~EmailNotificationServiceTests" --collect:"Code Coverage" -v minimal`
- Result: 42 total, 42 passed, 0 failed, 0 skipped.
- Updated focused line coverage:
	- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveExecutionContext`: 100%
	- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveJobHandler`: 100%
	- `Apha.BatchJobs.Infrastructure.Context.ExecutionYearContext`: 100%
	- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.MyFpsYearlyDataService`: 98.1%
	- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.ReloadFpsTotalsService`: 95.4%
	- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.EmailNotificationService`: 100%
- Remaining uncovered branches in focused run:
	- `MyFpsYearlyDataService`: slow-loader warning path (`sw.ElapsedMilliseconds > 30000`).
	- `ReloadFpsTotalsService`: strict view-missing error branch and source-row projection branch requiring populated/local DB source views.

Wave 3 snapshot (2026-05-27):
- New integration test file added:
	- `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/MabArchivePostgresIntegrationTests.cs`
- Added tests:
	- `RebuildSourceTotalsAsync_WhenStrictIsolationAndViewMissingFpsYear_ShouldThrowInvalidOperationException`
	- `RebuildSourceTotalsAsync_WhenSourceProjectExists_ShouldInsertRows_AndRollback`
- Command run (with explicit localhost credential env): `ConnectionStrings__FPSConnectionString=Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=***;Timeout=30 dotnet test src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj --no-restore --filter FullyQualifiedName~MabArchivePostgresIntegrationTests -v minimal`
- Result: 2 total, 2 passed, 0 failed, 0 skipped.
- Notes:
	- Tests are transaction/rollback safe and designed for local postgres when credentials and object permissions are available.
	- These tests target ReloadFpsTotalsService strict-view-missing and non-empty-source execution branches that were previously not executable in unit-only runs.

Wave 4 snapshot (2026-05-27):
- New test added:
	- `MyFpsYearlyDataServiceTests.LoadYearDataAsync_WhenLoaderIsSlow_ShouldCompleteAndAggregateRows`
- Command run (MABArchive-focused suite with localhost credentials): `ConnectionStrings__FPSConnectionString=Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=***;Timeout=30 dotnet test src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/Apha.BatchJobs.UnitTests.csproj --no-restore --filter "FullyQualifiedName~MabArchiveExecutionContextTests|FullyQualifiedName~MabArchiveJobHandlerTests|FullyQualifiedName~ExecutionYearContextTests|FullyQualifiedName~MyFpsYearlyDataServiceTests|FullyQualifiedName~ReloadFpsTotalsServiceTests|FullyQualifiedName~EmailNotificationServiceTests|FullyQualifiedName~MabArchivePostgresIntegrationTests|FullyQualifiedName~MabArchiveLoadOrchestratorParityTests|FullyQualifiedName~MabArchiveLoaderMetadataTests" --collect:"Code Coverage" -v minimal`
- Result: 50 total, 50 passed, 0 failed, 0 skipped.
- Updated focused coverage snapshot (tool-reported):
	- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveExecutionContext`: 100%
	- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveJobHandler`: 100%
	- `Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.MabArchiveLoadOrchestrator`: 95.8%
	- `Apha.BatchJobs.Infrastructure.Context.ExecutionYearContext`: 100%
	- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.MyFpsYearlyDataService`: 100%
	- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.ReloadFpsTotalsService`: 95.4%
	- `Apha.BatchJobs.Infrastructure.Repositories.MabArchive.EmailNotificationService`: 100%
- Remaining high-value gaps:
	- `ReloadFpsTotalsService`: strict-view missing aggregation for multiple views and populated projection branch details.
	- `MabArchiveLoadOrchestrator`: constructor null-guard branches and ExecuteFullYearCycle unavailable-year/logging branches.