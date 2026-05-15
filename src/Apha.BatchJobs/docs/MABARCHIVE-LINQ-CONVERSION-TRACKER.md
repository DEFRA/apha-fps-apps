# MABArchive LINQ Conversion Tracker

Status date: 2026-05-15
Owner: BatchJobs migration stream
Scope: Convert MABArchive year-load pipeline from SQL templates in code to LINQ/.NET implementation while preserving exact behavior.

## Logic Integrity Contract (Non-Negotiable)

1. Preserve current loader order and orchestration semantics (sequence 1..24).
2. Preserve target row sets exactly for each loader (same key set and values).
3. Preserve delete then load idempotency model and transaction boundaries.
4. Preserve special-case behavior:
- g_tlkpproject delete by parentproject set.
- my_tbladditionalcosts ac_counter generation semantics.
- my_staff name composition behavior.
- tlkpyear month lookup/cast behavior.
5. Preserve observability:
- per-loader logging
- row-count tracking
- failure behavior and exception propagation.
6. No silent behavior fixes during conversion. Any intentional behavior change must be explicitly approved and documented.

## Phase Plan

1. Baseline freeze and parity harness design.
2. Source and target schema mapping in DbContext.
3. Loader framework refactor for LINQ execution base.
4. Convert low-risk projection loaders (single-table, fpsyear filter).
5. Convert medium-risk loaders (grouping, joins, casts, string composition).
6. Convert high-risk loader (my_tbladditionalcosts row numbering/counter offset).
7. End-to-end parity and performance validation.
8. Controlled rollout and SQL fallback retirement.
9. Final cleanup and docs sign-off.

## Master Tracker

| Phase | Title | Status | Validation | Notes |
| --- | --- | --- | --- | --- |
| 1 | Baseline freeze and harness design | Completed | Completed | Baseline snapshot runner + comparator implemented; canonical dataset documented in docs/database/validation/MABARCHIVE-BASELINE-CANONICAL-DATASET.md. |
| 2 | DbContext source/target mappings | Completed | Completed | All 24 loader source/target mappings now registered in ConfigureMabArchiveModels and tracked in docs/MABARCHIVE-DBCONTEXT-MAPPING-GAP-MATRIX.md (0/24 remaining). |
| 3 | Loader framework refactor | Completed | Completed | Added shared loader execution base, LINQ-ready base, and DI mode selector while preserving IMabArchiveLoader contract and orchestration flow. |
| 4 | Low-risk loader conversion | In Progress | In Progress | First projection loader converted to LINQ; selector now uses strict DotNetLinq mode without SQL fallback. |
| 5 | Medium-risk loader conversion | Not Started | Not Started | Convert group/join/cast/string logic loaders. |
| 6 | High-risk loader conversion | Not Started | Not Started | Implement deterministic ac_counter generation parity. |
| 7 | Full parity + performance validation | Not Started | Not Started | Compare SQL baseline vs LINQ output for all loaders. |
| 8 | Rollout and fallback retirement | Not Started | Not Started | Keep switch for controlled rollout then retire SQL path. |
| 9 | Cleanup and final sign-off | Not Started | Not Started | Remove retired SQL code and finalize tracker evidence. |

## Loader Coverage Tracker (24/24)

| Seq | Loader | Complexity | LINQ Status | Parity Status | Notes |
| --- | --- | --- | --- | --- | --- |
| 1 | my_tlkpprogram | Low | Completed | Not Started | LINQ loader implemented (MyTlkpProgramLinqLoader); parity snapshot compare pending. |
| 2 | g_tlkpproject | Medium | Not Started | Not Started | GROUP BY dedupe parity required. |
| 3 | my_tlkpproject | Low | Not Started | Not Started | Single source table filter/projection. |
| 4 | my_fpsyeartotals | Low | Not Started | Not Started | Single source table filter/projection. |
| 5 | my_monthlyoutput | Low | Not Started | Not Started | Single source table filter/projection. |
| 6 | my_monthlytime | Low | Not Started | Not Started | Single source table filter/projection. |
| 7 | my_proj_invoice | Low | Not Started | Not Started | Single source table filter/projection. |
| 8 | my_proj_subcontract | Low | Not Started | Not Started | Single source table filter/projection. |
| 9 | my_projectmonthfinal | Low | Not Started | Not Started | Single source table filter/projection. |
| 10 | my_tbladditionalcosts | High | Not Started | Not Started | ROW_NUMBER + MAX(ac_counter) parity critical. |
| 11 | my_tblanimalreq | Low | Not Started | Not Started | Single source table filter/projection. |
| 12 | my_tblcontract | Low | Not Started | Not Started | Single source table filter/projection. |
| 13 | my_tblstaffjob | Low | Not Started | Not Started | Single source table filter/projection. |
| 14 | my_timecostcalcs | Low | Not Started | Not Started | Single source table filter/projection. |
| 15 | my_tlkptestreqmt | Low | Not Started | Not Started | Single source table filter/projection. |
| 16 | tlkpyear | Medium | Not Started | Not Started | Month variable lookup + cast behavior. |
| 17 | my_workgroupgrade | Low | Not Started | Not Started | Single source table filter/projection. |
| 18 | my_profitcentregrade | Low | Not Started | Not Started | Single source table filter/projection. |
| 19 | my_tblprofitcentre | Medium | Not Started | Not Started | No fpsyear filter in current SQL, preserve decision. |
| 20 | my_testorproduct | Low | Not Started | Not Started | Single source table filter/projection. |
| 21 | my_staff | Medium | Not Started | Not Started | Join and COALESCE-based name composition. |
| 22 | my_workgroup | Low | Not Started | Not Started | Single source table filter/projection. |
| 23 | my_tblanimals | Low | Not Started | Not Started | Single source table filter/projection. |
| 24 | my_tlkpproject_all | Low | Not Started | Not Started | Used by refresh-only path too. |

## Validation Gates By Phase

### Phase 1 gate

- Baseline SQL snapshot runner created for each loader.
- Canonical dataset and environment documented.
- Row-count and hash-comparison format agreed.

### Phase 2 gate

- All required source and target entity models added.
- Mappings validated by successful worker and API builds.
- No runtime SQL text dependency added in new code.

### Phase 3 gate

- Loader abstraction supports LINQ loaders without orchestration behavior drift.
- Existing sequence and metadata tests pass.

### Phase 4-6 gates

For each converted loader:

- SQL baseline rows captured.
- LINQ output rows matched by:
- row count
- key set
- canonical row hash.
- Target table sample spot checks completed.
- Error handling and logging unchanged.

### Phase 7 gate

- Full 24-loader run parity passes.
- Month <=4 and month >4 orchestration paths both pass.
- DeleteYearDataAsync and RefreshProjectAllOnlyAsync behavior validated.
- Performance within agreed tolerance window versus SQL baseline.

### Phase 8 gate

- Feature flag or mode selector rollout plan executed.
- At least one full non-prod run with LINQ only and parity monitor.
- Rollback strategy documented and tested.

### Phase 9 gate

- SQL loader code moved to docs/legacy reference.
- Docs and runbooks updated.
- Tracker completed with evidence links.

## Evidence Log (fill during execution)

| Date | Phase | Evidence | Result | Owner |
| --- | --- | --- | --- | --- |
| 2026-05-15 | 1 | Added docs/database/sql/validate-mabarchive-baseline.ps1 with deterministic per-loader snapshot export format. Verified with PowerShell parse check and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 1 | Executed baseline runner and produced docs/database/validation/mabarchive-baseline-20260515-142030.json with row-count and canonical hash snapshots for loaders 1..24 (year 2025). | Pass | Copilot |
| 2026-05-15 | 1 | Added docs/database/sql/compare-mabarchive-baseline.ps1 and verified compare pass (24 compared, 0 mismatches) using mabarchive-baseline-20260515-142030.json as both baseline and candidate. | Pass | Copilot |
| 2026-05-15 | 1 | Added canonical dataset/environment doc: docs/database/validation/MABARCHIVE-BASELINE-CANONICAL-DATASET.md (year, environment, hash format, pass criteria). | Pass | Copilot |
| 2026-05-15 | 2 | Added docs/MABARCHIVE-DBCONTEXT-MAPPING-GAP-MATRIX.md after auditing loader source/target footprint against BatchJobsDbContext mappings. Verified no ConfigureMabArchiveModels block and no DbSet mappings for mabarchive loader targets. | Pass | Copilot |
| 2026-05-15 | 2 | Implemented initial MABArchive DbContext mapping slice for loaders 1-4 (new models in Infrastructure/Data/MabArchiveLinqTables.cs; DbSet + ConfigureMabArchiveModels wiring in Infrastructure/Data/BatchJobsDbContext.cs). Verified with build-batchjobs-worker task (success) and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 2 | Implemented MABArchive DbContext mapping slice for loaders 5-6 (monthlyoutput/monthlytime source + target models, DbSet registration, and ConfigureMabArchiveModels mappings). Verified with build-batchjobs-worker task (success) and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 2 | Implemented MABArchive DbContext mapping slice for loaders 7-10 (proj_invoice, proj_subcontract, projectmonthfinal, tbladditionalcosts source + target models, DbSet registration, and ConfigureMabArchiveModels mappings). Verified with build-batchjobs-worker task (success) and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 2 | Implemented MABArchive DbContext mapping slice for loaders 11-14 (tblanimalreq, tblcontract, tblstaffjob, timecostcalcs source + target models, DbSet registration, and ConfigureMabArchiveModels mappings). Verified with build-batchjobs-worker task (success) and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 2 | Implemented MABArchive DbContext mapping slice for loaders 15-18 (tlkptestreqmt, tlkpyear with tbldb_variables source, workgroupgrade, profitcentregrade source + target models, DbSet registration, and ConfigureMabArchiveModels mappings). Verified with build-batchjobs-worker task (success) and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 2 | Implemented MABArchive DbContext mapping slice for loaders 19-24 (tblprofitcentre, testorproduct, staff join sources, workgroup, tblanimals, tlkpproject_all target models; DbSet registration; ConfigureMabArchiveModels mappings). Verified with build-batchjobs-worker task (success) and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 3 | Refactored loader framework by adding MabArchiveLoaderBase and MabArchiveLinqLoaderBase in Infrastructure/Repositories/MabArchive/Loaders/MabArchiveLoaders.cs; SQL loaders continue through MabArchiveSqlLoaderBase and IMabArchiveLoader.LoadAsync signature remains unchanged. Verified with build-batchjobs-worker task (success) and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 3 | Added configurable MABArchive loader mode selector in DependencyInjection/ServiceCollectionSetup.cs (BatchJobs:MabArchiveImplementationMode). Added ServiceCollectionSetupTests coverage for default SQL registration and DotNetLinq mode wiring. Verified with build-batchjobs-worker task (success) and targeted unit tests (10 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 4 | Implemented first LINQ loader (my_tlkpprogram) via MyTlkpProgramLinqLoader and switched DotNetLinq mode to strict no-fallback registration (LINQ loaders only). Updated unit tests to validate strict mode behavior. | Pass | Copilot |
| TBD | TBD | TBD | TBD | TBD |

## Daily Update Template

- Date:
- Current phase:
- Completed today:
- Logic integrity checks run:
- Deviations found:
- Decisions required:
- Next step:
