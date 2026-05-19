# Plan: Gradual SQL Removal & LINQ-to-Production Migration

**Status:** Planning phase  
**Target:** Remove all SQL fallback implementations; LINQ becomes authoritative production code  
**Current State:** All 14 `sp_RecreateSummaries` steps migrated to LINQ (5/5 parity tests passing)  
**Scope:** Batch jobs (RecreateSummaries, LoadFromFPS orchestrations)

---

## 1. Current Hybrid Architecture

### Dual-Mode Implementation (sp_LoadFromFPS & Loaders)
- **LINQ Mode (Default):** `MabArchiveDotNetLoaderBase` - All loaders using EF Core LINQ queries
- **SQL Mode (Fallback):** `MabArchiveSqlLoaderBase` - Raw SQL queries via Dapper/direct execution
- **Configuration:** `BatchJobs:MabArchiveImplementationMode` (values: "DotNet" or "Sql")
- **Fallback Logic:** 
  - If LINQ fails and `AllowSqlFallback = true`, retry with SQL
  - Logged as WARN; silent fallback unless validation enabled
- **Validation:** `ValidateLoadersAtRuntime`, `VerifyLogicEquivalence`, logs performance metrics

### SQL Files Still in Repository
**Location:** `src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Sql/`

#### RecreateSummaries Suite
- `01_delete_fps_totals.sql`
- `02_create_fps_totals.sql`
- `03_insert_missing_projects.sql`
- `04_delete_time_cost_calcs.sql`
- `05_create_time_cost_calcs.sql`
- `06_delete_project_month_casework.sql`
- `07_create_project_month_casework.sql`
- `08_delete_project_month_final.sql`
- `09_delete_project_month2.sql`
- `10_create_project_month_single.sql`
- `11_delete_project_month3.sql`
- `12_create_project_month_cumulative.sql`
- `13_create_project_month_final.sql`
- `14_log_recreate_summaries.sql`
- `15_refresh_period_mo.sql`
- `16_refresh_period_psc.sql`
- `17_refresh_period_tcc.sql`

#### LoadFromFPS Suites
- Multiple loader SQL files in subdirectories (e.g., delete year data, add program, add project, etc.)

---

## 2. Removal Strategy: Three-Phase Approach

### Phase 1: Code Cleanup (Weeks 1–2)

**Goal:** Prepare codebase for LINQ-only execution.

#### 1.1 Update RecreateSummariesStepCatalog Comment
- **File:** `RecreateSummariesStepCatalog.cs`
- **Current Comment:** "Steps 1-7 use LINQ, remaining use SQL adapters"
- **Change To:** "All 14 steps fully implemented in LINQ/EF Core (v1.0 prod-ready)"
- **Action:** Remove outdated documentation

#### 1.2 Remove SQL Fallback Configuration Options
- **Files:** `appsettings.json`, `appsettings.Development.json`, `Program.cs`
- **Remove:**
  - `BatchJobs:MabArchiveImplementationMode` key
  - `AllowSqlFallback` setting
  - `ValidateLoadersAtRuntime` (can consolidate to always-on minimal validation)
  - `VerifyLogicEquivalence` parameter
- **Rationale:** No need for mode switching if LINQ is authoritative

#### 1.3 Simplify Orchestrator Classes
- **Files:** `RecreateSummariesOrchestrator.cs`, `MabArchiveLoadOrchestrator.cs`
- **Changes:**
  - Remove configuration-based loader selection logic
  - Remove try-catch with SQL fallback logic
  - Keep LINQ paths only
  - Simplify logging (remove "fallback to SQL" messages)

#### 1.4 Update Unit Tests
- **File:** `Apha.BatchJobs.UnitTests`
- **Status:** Already passing (5/5 tests = LINQ parity verified)
- **Action:** Update test setup to remove mock SQL loader paths
- **Remove:** `MabArchiveSqlLoaderBase` test doubles

---

### Phase 2: Documentation & Decommissioning (Week 3)

**Goal:** Archive SQL implementations; document removal rationale.

#### 2.1 Create Decommissioning Document
- **File:** `src/Apha.BatchJobs/docs/SQL_DECOMMISSIONING_RECORD.md`
- **Content:**
  - Date of removal
  - List of SQL files decommissioned
  - Reason: LINQ parity achieved, cloud-native (PostgreSQL) requirement
  - Fallback to SQL no longer needed
  - Parity test results (5/5 passing)

#### 2.2 Archive SQL Files (Don't Delete Yet)
- **Approach:** Move to archive directory (keep git history)
  ```
  mkdir -p src/Apha.BatchJobs/docs/SQL_ARCHIVE_v1.0
  mv src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Sql/* \
     src/Apha.BatchJobs/docs/SQL_ARCHIVE_v1.0/
  ```
- **Rationale:** 
  - Preserve git history for future reference
  - Make it clear SQL is no longer active
  - Simplify build artifact size

#### 2.3 Update HTML Documentation
- **Files:**
  - `sp_RecreateSummaries_DotNet_Conversion.html`
  - `sp_LoadFromFPS_DotNet_Conversion.html`
- **Changes:**
  - Remove all "SQL Fallback" and "Validation Strategy" sections
  - Update title: "LINQ Implementation (Production)" instead of "vs Baseline"
  - Add decommissioning date and rationale
  - Simplify deployment stages (remove Stage 1 & 2, keep Stage 3 only)

#### 2.4 Update Code Comments
- **Search & Replace:** "SQL adapter", "fallback", "hybrid mode" → "LINQ/EF Core (authoritative)"
- **Files:** All step classes, orchestrators, loaders

---

### Phase 3: Build & Deployment (Week 4)

**Goal:** Remove SQL loader interfaces and base classes.

#### 3.1 Remove SQL Loader Infrastructure
- **Files to Delete:**
  - `MabArchiveSqlLoaderBase.cs` (if it exists)
  - `ISqlLoader.cs` (if extracted)
  - Any SQL execution factory classes
  - SQL parameter mapping classes

#### 3.2 Cleanup Dependency Injection
- **File:** `Program.cs`, `ProgramExtension.cs`
- **Remove:**
  - Registration of SQL loaders
  - SQL fallback service registration
  - Conditional DI based on `ImplementationMode`
- **Keep:** LINQ loaders, EF Core DbContext

#### 3.3 Remove Configuration Keys
- **Files:** `appsettings*.json`
- **Remove:**
  ```json
  "BatchJobs": {
    "MabArchiveImplementationMode": "DotNet",
    "AllowSqlFallback": true,
    "ValidateLoadersAtRuntime": true,
    "VerifyLogicEquivalence": true
  }
  ```

#### 3.4 Compile & Test
- Run full unit test suite
  ```bash
  dotnet test src/Apha.BatchJobs/Apha.BatchJobs.UnitTests/...
  ```
- Verify all parity tests still pass (5/5)
- Check no compilation errors after removal

#### 3.5 Git Commit & Documentation
- **Commit:** 
  ```
  chore: Remove SQL fallback implementations; LINQ is production code
  
  - Archive SQL files to docs/SQL_ARCHIVE_v1.0
  - Remove MabArchiveSqlLoaderBase and fallback logic
  - Simplify orchestrators (LINQ-only execution)
  - Update documentation to reflect LINQ-as-authoritative
  - All 5 parity tests passing; no drift from SQL baseline
  - Supports PostgreSQL cloud deployment
  ```

---

## 3. Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|-----------|
| **Unknown LINQ bugs in production** | Data loss or incorrect calculations | Parity testing (5/5 passing), incremental rollout by region |
| **PostgreSQL-specific behavior** | Breaks in prod if LINQ doesn't match PG semantics | Final staging validation on PG; test type casting edge cases |
| **Performance regression** | Query slowness if LINQ has suboptimal generated SQL | Profile LINQ queries before removal; compare execution plans |
| **Accidental SQL reference** | Runtime error if code path still uses SQL | Code search before commit; lint rules |
| **Rollback difficulty** | Can't easily revert if prod issue found | Keep git history; SQL archive in repo for 6+ months |

---

## 4. Testing Checklist

Before Phase 3 Production Release:

- [ ] All 5 parity tests passing (MabArchiveLoadOrchestratorParityTests)
- [ ] Staging environment load test with PostgreSQL
- [ ] Performance profiling: LINQ queries vs archived SQL baselines
- [ ] Code review: No hardcoded SQL strings in C# code
- [ ] Documentation audit: No SQL references in comments/docs
- [ ] Deployment runbook updated (LINQ-only setup)
- [ ] Monitoring alerts updated (no "SQL fallback" warnings)
- [ ] Backup of SQL archive created and stored off-repo

---

## 5. Timeline

| Phase | Duration | Key Deliverables |
|-------|----------|-----------------|
| **Phase 1: Cleanup** | 1–2 weeks | Config removal, orchestrator simplification |
| **Phase 2: Archive** | 1 week | SQL archive, documentation updates |
| **Phase 3: Production** | 1 week | Final tests, PR review, merge & deploy |
| **Total** | ~4 weeks | LINQ-only, cloud-native, production-ready |

---

## 6. Success Criteria

✅ **All criteria must be met before production release:**

1. **Zero SQL references** in active codebase (`src/Apha.BatchJobs/`)
2. **5/5 Parity tests passing** (LINQ == old SQL baseline)
3. **PostgreSQL compatibility verified** in staging
4. **Performance acceptable** (< 10% regression vs SQL baseline)
5. **Documentation complete** (HTML, code comments, runbooks)
6. **Git history preserved** (archived, not deleted)
7. **Backup created** (SQL archive accessible if needed)

---

## Implementation Notes

### Key Files to Modify

1. **RecreateSummariesStepCatalog.cs** — Update comment & remove mode logic
2. **RecreateSummariesOrchestrator.cs** — Remove SQL fallback try-catch
3. **appsettings.json** — Remove batch job config keys
4. **Program.cs / ProgramExtension.cs** — Remove SQL DI registration
5. **sp_RecreateSummaries_DotNet_Conversion.html** — Final doc update
6. **sp_LoadFromFPS_DotNet_Conversion.html** — Finalize loader status
7. **SQL directory** → Move to archive (git mv, not delete)

### Parallel Track: LoadFromFPS Loaders

If LoadFromFPS has similar SQL fallback architecture:
- Apply same three-phase plan
- Coordinate with RecreateSummaries removal
- Single merged PR at end

---

## Sign-Off

**Plan Created:** May 19, 2026  
**Owner:** BatchJobs Team  
**Next Steps:** Schedule Phase 1 kickoff; identify code review lead
