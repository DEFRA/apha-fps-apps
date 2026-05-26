# MABArchive Unit Test Tracker

Last updated: 2026-05-26
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
| MA-008 | Phase 2 | P0 | BuildExecutionContext month branch parity from runtime clock (`>4` vs `<=4`) | Not Started | Includes optional test override `MABARCHIVE_TEST_UTCNOW` |
| MA-009 | Phase 2 | P1 | BuildExecutionContext override parsing fallback behavior for invalid env values | Not Started | Guard against brittle month-branch test hooks |
| MA-010 | Phase 2 | P1 | MabArchiveJobHandler resolves orchestrator, builds context, executes transaction wrapper once | Not Started | Happy-path orchestration glue |
| MA-011 | Phase 2 | P1 | MabArchiveJobHandler cancellation/exception propagation behavior | Not Started | Logs and rethrow semantics |
| MA-012 | Phase 2 | P1 | ExecutionYearContext default `YearSource` and mutable year behavior | Not Started | Lightweight context coverage |
| MA-013 | Phase 3 | P0 | MyFpsYearlyDataService constructor rejects loader registration mismatch count | Not Started | Expected loader count = 24 |
| MA-014 | Phase 3 | P0 | MyFpsYearlyDataService constructor rejects duplicate sequence values | Not Started | Strict sequence uniqueness |
| MA-015 | Phase 3 | P0 | MyFpsYearlyDataService constructor rejects non-contiguous sequence values | Not Started | Strict 1..24 contract |
| MA-016 | Phase 3 | P1 | IsYearAvailableAsync handles missing year and positive year cases | Not Started | SQL query contract |
| MA-017 | Phase 3 | P1 | RefreshProjectAllOnlyAsync deletes target year and runs loader seq 24 only | Not Started | Partial refresh contract |
| MA-018 | Phase 3 | P1 | LoadYearDataAsync executes loaders in sequence order and aggregates rows | Not Started | Baseline fan-out behavior |
| MA-019 | Phase 3 | P1 | LoadYearDataAsync stops on failing loader and includes loader metadata in error path | Not Started | Diagnostic quality |
| MA-020 | Phase 4 | P0 | ReloadFpsTotalsService strict-year-isolation view checks fail on missing fpsyear columns | Not Started | Prevent cross-year bleed |
| MA-021 | Phase 4 | P0 | ReloadFpsTotalsService rebuild totals formula parity (null handling and computed totals) | Not Started | Financial parity critical |
| MA-022 | Phase 4 | P1 | ReloadFpsTotalsService no-source-rows path returns zero without insert | Not Started | Empty-year behavior |
| MA-023 | Phase 5 | P0 | PostgreSQL integration: DeleteYearDataAsync year-scoped deletes preserve out-of-year rows | Not Started | Regression guard for destructive ops |
| MA-024 | Phase 5 | P0 | PostgreSQL integration: g_tlkpproject project-key delete semantics | Not Started | Special-case baseline behavior |
| MA-025 | Phase 5 | P0 | PostgreSQL integration: high-risk loader 10 (`my_tbladditionalcosts`) counter semantics | Not Started | Counter/order parity |
| MA-026 | Phase 5 | P1 | PostgreSQL integration: `my_staff` name composition parity | Not Started | String composition drift guard |
| MA-027 | Phase 5 | P1 | PostgreSQL integration: `tlkpyear` month lookup/cast parity | Not Started | Medium-risk mapping guard |

## 5) Suggested Test File Layout

Suggested location: `src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/MabArchive/`

- MabArchiveLoadOrchestratorParityTests.cs (existing)
- MabArchiveLoaderMetadataTests.cs (existing)
- MabArchiveExecutionContextTests.cs
- MabArchiveJobHandlerTests.cs
- ExecutionYearContextTests.cs
- MyFpsYearlyDataServiceTests.cs
- ReloadFpsTotalsServiceTests.cs
- MabArchivePostgresIntegrationTests.cs

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