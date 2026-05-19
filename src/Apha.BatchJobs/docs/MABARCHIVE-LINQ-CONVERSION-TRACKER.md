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
| 4 | Low-risk loader conversion | Completed | In Progress | LINQ loaders implemented for all low-risk sequences 1,3,4,5,6,7,8,9,11,12,13,14,15,17,18,20,22,23,24; selector remains strict DotNetLinq mode without SQL fallback. |
| 5 | Medium-risk loader conversion | Completed | In Progress | LINQ loaders implemented for sequences 2,16,19,21 (group/join/cast/string logic). |
| 6 | High-risk loader conversion | Completed | In Progress | Implemented LINQ loader for seq 10 with MAX(ac_counter) + ordered row numbering semantics. |
| 7 | Full parity + performance validation | In Progress | In Progress | Full SQL-vs-.NET snapshot compares now pass for month <=4 and month >4 across all 24 loaders after loader 3 DateCreated mapping alignment and loader 11 ar_counter sequence alignment; performance tolerance validation still pending. |
| 8 | Rollout and fallback retirement | Not Started | Not Started | Keep switch for controlled rollout then retire SQL path. |
| 9 | Cleanup and final sign-off | Not Started | Not Started | Remove retired SQL code and finalize tracker evidence. |

## Loader Coverage Tracker (24/24)

| Seq | Loader | Complexity | LINQ Status | Parity Status | Notes |
| --- | --- | --- | --- | --- | --- |
| 1 | my_tlkpprogram | Low | Completed | Completed | LINQ loader implemented (MyTlkpProgramDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 2 | g_tlkpproject | Medium | Completed | Completed | LINQ loader implemented (GTlkpProjectDotNetLoader) with GROUP BY-equivalent dedupe projection; full snapshot compare passed for month <=4 and month >4. |
| 3 | my_tlkpproject | Low | Completed | Completed | LINQ loader implemented (MyTlkpProjectDotNetLoader); DateCreated mapping aligned and full snapshot compare passed for month <=4 and month >4. |
| 4 | my_fpsyeartotals | Low | Completed | Completed | LINQ loader implemented (MyFpsYearTotalsDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 5 | my_monthlyoutput | Low | Completed | Completed | LINQ loader implemented (MyMonthlyOutputDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 6 | my_monthlytime | Low | Completed | Completed | LINQ loader implemented (MyMonthlyTimeDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 7 | my_proj_invoice | Low | Completed | Completed | LINQ loader implemented (MyProjInvoiceDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 8 | my_proj_subcontract | Low | Completed | Completed | LINQ loader implemented (MyProjSubcontractDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 9 | my_projectmonthfinal | Low | Completed | Completed | LINQ loader implemented (MyProjectMonthFinalDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 10 | my_tbladditionalcosts | High | Completed | Completed | LINQ loader implemented (MyTblAdditionalCostsDotNetLoader) preserving ORDER BY(jobcode,account,description) + MAX(ac_counter)+ROW_NUMBER semantics; full snapshot compare passed for month <=4 and month >4. |
| 11 | my_tblanimalreq | Low | Completed | Completed | LINQ loader implemented (MyTblAnimalReqDotNetLoader); ar_counter sequence alignment fixed and full snapshot compare passed for month <=4 and month >4. |
| 12 | my_tblcontract | Low | Completed | Completed | LINQ loader implemented (MyTblContractDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 13 | my_tblstaffjob | Low | Completed | Completed | LINQ loader implemented (MyTblStaffJobDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 14 | my_timecostcalcs | Low | Completed | Completed | LINQ loader implemented (MyTimeCostCalcsDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 15 | my_tlkptestreqmt | Low | Completed | Completed | LINQ loader implemented (MyTlkpTestReqmtDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 16 | tlkpyear | Medium | Completed | Completed | LINQ loader implemented (TlkpYearDotNetLoader) preserving month variable lookup + cast behavior; full snapshot compare passed for month <=4 and month >4. |
| 17 | my_workgroupgrade | Low | Completed | Completed | LINQ loader implemented (MyWorkgroupGradeDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 18 | my_profitcentregrade | Low | Completed | Completed | LINQ loader implemented (MyProfitCentreGradeDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 19 | my_tblprofitcentre | Medium | Completed | Completed | LINQ loader implemented (MyTblProfitCentreDotNetLoader); no fpsyear filter preserved per current SQL behavior; full snapshot compare passed for month <=4 and month >4. |
| 20 | my_testorproduct | Low | Completed | Completed | LINQ loader implemented (MyTestOrProductDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 21 | my_staff | Medium | Completed | Completed | LINQ loader implemented (MyStaffDotNetLoader) preserving join and COALESCE-style name composition; full snapshot compare passed for month <=4 and month >4. |
| 22 | my_workgroup | Low | Completed | Completed | LINQ loader implemented (MyWorkgroupDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 23 | my_tblanimals | Low | Completed | Completed | LINQ loader implemented (MyTblAnimalsDotNetLoader); full snapshot compare passed for month <=4 and month >4. |
| 24 | my_tlkpproject_all | Low | Completed | Completed | LINQ loader implemented (MyTlkpProjectAllDotNetLoader); full snapshot compare passed for month <=4 and month >4; used by refresh-only path too. |

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
| 2026-05-15 | 3 | Refactored loader framework by adding MabArchiveLoaderBase and MabArchiveDotNetLoaderBase in Infrastructure/Repositories/MabArchive/Loaders/MabArchiveLoaders.cs; SQL loaders continue through MabArchiveSqlLoaderBase and IMabArchiveLoader.LoadAsync signature remains unchanged. Verified with build-batchjobs-worker task (success) and MABArchive unit tests (6 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 3 | Added configurable MABArchive loader mode selector in DependencyInjection/ServiceCollectionSetup.cs (BatchJobs:MabArchiveImplementationMode). Added ServiceCollectionSetupTests coverage for default SQL registration and DotNetLinq mode wiring. Verified with build-batchjobs-worker task (success) and targeted unit tests (10 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 4 | Implemented first LINQ loader (my_tlkpprogram) via MyTlkpProgramDotNetLoader and switched DotNetLinq mode to strict no-fallback registration (LINQ loaders only). Updated unit tests to validate strict mode behavior. | Pass | Copilot |
| 2026-05-15 | 4 | Implemented additional low-risk LINQ loaders: MyTlkpProjectDotNetLoader (seq 3) and MyFpsYearTotalsDotNetLoader (seq 4). Updated strict DotNetLinq selector test to assert LINQ-only set {1,3,4}. | Pass | Copilot |
| 2026-05-15 | 4 | Implemented additional low-risk LINQ loaders for seq 5-9: MyMonthlyOutputDotNetLoader, MyMonthlyTimeDotNetLoader, MyProjInvoiceDotNetLoader, MyProjSubcontractDotNetLoader, and MyProjectMonthFinalDotNetLoader. Updated strict DotNetLinq selector test to assert LINQ-only set {1,3,4,5,6,7,8,9}. Verified with build-batchjobs-worker task (success) and targeted unit tests (7 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 4 | Implemented additional low-risk LINQ loaders for seq 11-15 as separate step files: MyTblAnimalReqDotNetLoader, MyTblContractDotNetLoader, MyTblStaffJobDotNetLoader, MyTimeCostCalcsDotNetLoader, and MyTlkpTestReqmtDotNetLoader. Updated strict DotNetLinq selector test to assert LINQ-only set {1,3,4,5,6,7,8,9,11,12,13,14,15}. Verified with build-batchjobs-worker task (success) and targeted unit tests (7 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 4 | Implemented additional low-risk LINQ loaders for seq 17,18,20,22,23,24 as separate step files: MyWorkgroupGradeDotNetLoader, MyProfitCentreGradeDotNetLoader, MyTestOrProductDotNetLoader, MyWorkgroupDotNetLoader, MyTblAnimalsDotNetLoader, and MyTlkpProjectAllDotNetLoader. Updated strict DotNetLinq selector test to assert LINQ-only set {1,3,4,5,6,7,8,9,11,12,13,14,15,17,18,20,22,23,24}. Verified with build-batchjobs-worker task (success) and targeted unit tests (7 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 5 | Implemented medium-risk LINQ loaders for seq 2,16,19,21 as separate step files: GTlkpProjectDotNetLoader, TlkpYearDotNetLoader, MyTblProfitCentreDotNetLoader, and MyStaffDotNetLoader. Updated strict DotNetLinq selector test to assert LINQ-only set {1,2,3,4,5,6,7,8,9,11,12,13,14,15,16,17,18,19,20,21,22,23,24}. Verified with build-batchjobs-worker task (success) and targeted dotnet test filter for ServiceCollectionSetupTests/MabArchiveLoaderMetadataTests (7 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 6 | Implemented high-risk LINQ loader for seq 10 (MyTblAdditionalCostsDotNetLoader) as a separate step file, preserving ORDER BY(jobcode,account,description) and MAX(ac_counter)+ROW_NUMBER equivalent counter generation. Updated strict DotNetLinq selector test to assert full LINQ-only set {1..24}. Verified with build-batchjobs-worker task (success) and targeted dotnet test filter for ServiceCollectionSetupTests/MabArchiveLoaderMetadataTests (7 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 7 | Completed LINQ suffix removal for MABArchive loader abstractions/implementations (MabArchiveDotNetLoaderBase + *DotNetLoader class names), fixed DotNet mode DI selection, and validated with targeted tests: ServiceCollectionSetupTests, MabArchiveLoaderMetadataTests, and MabArchiveLoadOrchestratorParityTests (12 passed, 0 failed). | Pass | Copilot |
| 2026-05-15 | 7 | Executed full SQL vs .NET snapshot compare for month <=4 path using cloned DBs (mabarchive_sql_month_le4, mabarchive_dotnet_month_le4). Compare artifacts: docs/database/validation/mabarchive-baseline-20260515-182012.json vs docs/database/validation/mabarchive-baseline-20260515-182122.json. Result: 24 compared, 2 mismatches (seq 3 my_tlkpproject hash mismatch; seq 11 my_tblanimalreq hash mismatch; row counts matched for both). | Fail | Copilot |
| 2026-05-15 | 7 | Executed full SQL vs .NET snapshot compare for month >4 path using cloned DBs (mabarchive_sql_month_gt4, mabarchive_dotnet_month_gt4). Compare artifacts: docs/database/validation/mabarchive-baseline-20260515-182249.json vs docs/database/validation/mabarchive-baseline-20260515-182322.json. Result: 24 compared, 2 mismatches (seq 3 my_tlkpproject hash mismatch; seq 11 my_tblanimalreq hash mismatch; row counts matched for both). | Fail | Copilot |
| 2026-05-15 | 7 | Resolved loader 3 parity drift by mapping my_tlkpproject datecreated as timestamp without time zone in DbContext and removing UTC coercion in MyTlkpProjectDotNetLoader. Revalidated worker build (success). | Pass | Copilot |
| 2026-05-15 | 7 | Resolved loader 11 parity drift by aligning MyTblAnimalReqDotNetLoader ar_counter generation to sequence semantics (read next sequence value + explicit assignment + setval synchronization) while preserving source indcounter ordering. Revalidated worker build (success). | Pass | Copilot |
| 2026-05-15 | 7 | Re-executed full SQL vs .NET snapshot compare for month <=4 path using cloned DBs (mabarchive_sql_month_le4, mabarchive_dotnet_month_le4). Compare artifacts: docs/database/validation/mabarchive-baseline-20260515-185445.json vs docs/database/validation/mabarchive-baseline-20260515-185531.json. Result: 24 compared, 0 mismatches. | Pass | Copilot |
| 2026-05-15 | 7 | Re-executed full SQL vs .NET snapshot compare for month >4 path using cloned DBs (mabarchive_sql_month_gt4, mabarchive_dotnet_month_gt4). Compare artifacts: docs/database/validation/mabarchive-baseline-20260515-185620.json vs docs/database/validation/mabarchive-baseline-20260515-185653.json. Result: 24 compared, 0 mismatches. | Pass | Copilot |
| 2026-05-19 | 8 | Completed Phase 3 LINQ-only decommissioning: removed MabArchiveSqlLoaderBase and all SQL loader implementations from runtime source, switched DI to always register MabArchiveDotNetLoaderBase loaders, removed SQL fallback/equivalence toggles from root appsettings, and deleted unused legacy validation framework with SQL-mode settings. Verified with build-batchjobs-worker (success), build-batchjobs-api (success), targeted unit tests (5 passed), and MabArchiveLoadOrchestratorParityTests (5 passed). | Pass | Copilot |
| TBD | TBD | TBD | TBD | TBD |

## Daily Update Template

- Date:
- Current phase:
- Completed today:
- Logic integrity checks run:
- Deviations found:
- Decisions required:
- Next step:

