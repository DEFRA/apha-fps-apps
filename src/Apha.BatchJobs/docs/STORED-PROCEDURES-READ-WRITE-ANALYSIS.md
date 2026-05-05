# Stored Procedures: Read/Write Analysis & Batch Job Mapping

## Legacy Procedure Pattern Analysis

### FPS2025 Procedures

#### sp_createFPSTotals
**Purpose**: Calculate and load year totals from multiple cost queries
- **READ FROM**:
  - `tlkpProject` — project master (36 cols, includes income/cost fields)
  - `qryTotalAdditionalCosts` — aggregated additional costs by job
  - `qryTotalAnimalCosts` — aggregated animal costs
  - `qryTotalStaffCosts` — aggregated staff costs + pay costs
  - `qryTotalTestCosts` — aggregated test costs

- **WRITE TO**:
  - `FPSYearTotals` (19 cols) — project year cost/income totals
  
- **TRANSFORM**:
  - COALESCE nulls to 0
  - JOIN 4 cost queries on ParentProject
  - CALCULATE TotalCosts = sum of 4 cost types + PlanCaseworkDebit
  - CALCULATE TotalIncome = CustIncome + TransferIncome
  - SELECT DISTINCT on ParentProject

---

#### sp_deleteFPSTotals
**Purpose**: Clean slate before reload
- **DELETE FROM**: `FPSYearTotals`

---

### MAB_Archive Procedures

#### sp_AddG_tlkpProject
**Purpose**: Copy/archive project master with year context
- **READ FROM**: 
  - `{cFPSVersion}.dbo.tlkpProject` — dynamic source (parameterized)
  
- **WRITE TO**: 
  - `G_tlkpProject` — archived project master
  
- **TRANSFORM**:
  - SELECT only: ParentProject, ProjectTitle, CostBookNo, Disease, Contract, ShortTitle, ProjectStatus
  - GROUP BY these 7 columns

---

#### sp_AddMY_FPSYearTotals
**Purpose**: Archive year totals with year prefix
- **READ FROM**: 
  - `{cFPSVersion}.dbo.FPSYearTotals` — source year totals (19 cols)
  
- **WRITE TO**: 
  - `MY_FPSYearTotals` — archive with year column
  
- **TRANSFORM**:
  - Add year column (cast parameter to int)
  - All 19 FPS columns + year = 20 cols in archive

---

#### sp_AddMY_MonthlyOutput, sp_AddMY_MonthlyTime, sp_AddMY_TimeCostCalcs, etc.
**Pattern** (11 procedures follow this same pattern):
- **READ FROM**: `{cFPSVersion}.dbo.{SourceTable}`
- **WRITE TO**: `MY_{SourceTable}` (archive with year prefix)
- **TRANSFORM**: Add year column, select subset of columns relevant to current year

---

## Key ETL Insights

### 1. **Read Sources (FPS Cloud)**
```
Dimension/Lookup Tables:
- tlkpProject (36 cols, master projects)
- WorkGroup, Grade, User_*, ProfitCentre* (dimensions)

Fact/Calculated Tables:
- MonthlyOutput, MonthlyTime, TimeCostCalcs (monthly metrics)
- Proj_Invoice, Proj_SubContract (financial txns)
- ProjectMonthFinal (consolidated month results)
- FPSYearTotals (yearly aggregates)
- Staff, TestOrProduct (operational data)

Query Views:
- qryTotalAdditionalCosts, qryTotalAnimalCosts, qryTotalStaffCosts, qryTotalTestCosts
  (pre-aggregated cost queries for efficiency)
```

### 2. **Write Targets (MAB_Archive)**
```
Archive Tables (with MY_ prefix and year column):
- MY_FPSYearTotals
- MY_MonthlyOutput
- MY_MonthlyTime
- MY_TimeCostCalcs
- MY_ProjectMonthFinal
- MY_Proj_Invoice
- MY_Proj_SubContract
- MY_Staff
- MY_TestOrProduct
- MY_WorkGroup, MY_WorkGroupGrade
- MY_tblAdditionalCosts, MY_tblAnimalReq, MY_ProfitCentreGrade
- G_tlkpProject (project master archive)
```

### 3. **Transform Pattern**
```
Traditional Hierarchy:
1. DELETE archive target (clear old data)
2. SELECT from source with COALESCE, JOIN, GROUP BY
3. ADD year context column
4. INSERT INTO archive

Idempotence: Replace/Upsert (not used here, just truncate+insert)
```

---

## Proposed Batch Job Mapping (ScheduledLoadFromFps)

### **Our 7 New Tables Map to Legacy ETL:**

| New Local Table | Legacy Equivalent | Read Source | Write Target | Transform Pattern |
|---|---|---|---|---|
| **fps_source_project_year** | (new fixture) | `fps.tlkpproject` (36 cols) | operational schema | Extract project metadata; seed test scenarios |
| **fps_year_totals** | `sp_createFPSTotals` → `FPSYearTotals` | `fps.fpsyeartotals` (via sink) | `operational.fps_year_totals` | Copy schema; seed expected totals; no aggregation (pre-computed in cloud) |
| **fps_year_archive** | `MY_FPSYearTotals` pattern | *(derived from logic)* | `operational.fps_year_archive` | Store prev year totals as JSON before delete (audit trail) |
| **fps_project_all_current_year** | `G_tlkpProject` pattern | `fps.tlkpproject` (36 cols) | `operational.fps_project_all_current_year` | Snapshot current year projects as JSON payload |
| **scheduled_load_run** | (batch framework only) | *(N/A)* | `fps.scheduled_load_run` | Inserted by JobOrchestrator; tracks run lifecycle |
| **scheduled_load_step_run** | (batch framework only) | *(N/A)* | `fps.scheduled_load_step_run` | Inserted by step handlers; tracks step execution |
| **scheduled_load_validation_result** | (batch framework validation) | *(N/A)* | `fps.scheduled_load_validation_result` | Inserted by cross-validation queries; tracks assertion results |

---

## Batch Job Implementation Pattern

Following the legacy procedure model, the batch job should:

### **Phase 1: ProcessPreviousYearTotals**
```
1. READ: operational.fps_year_totals (previous year's data)
2. TRANSFORM: Serialize current data to JSON
3. WRITE: operational.fps_year_archive (audit trail before deletion)
4. DELETE: TRUNCATE operational.fps_year_totals
5. RECORD: scheduled_load_step_run(step='ProcessPreviousYearTotals', status='completed')
```

### **Phase 2: ProcessCurrentYearTotals**
```
1. READ: sink_raw.fps__fpsyeartotals (curated from cloud snapshot)
2. TRANSFORM: Apply any local business rules (COALESCE nulls, etc.)
3. WRITE: operational.fps_year_totals (load new data)
4. RECORD: scheduled_load_step_run(step='ProcessCurrentYearTotals', status='completed')
```

### **Phase 3: HandleCurrentYearProjectAll**
```
1. READ: sink_raw.fps__tlkpproject (all current projects)
2. TRANSFORM: SELECT subset of columns, serialize to JSON
3. WRITE: operational.fps_project_all_current_year (snapshot)
4. RECORD: scheduled_load_step_run(step='HandleCurrentYearProjectAll', status='completed')
```

### **Phase 4: DeleteYearsFpsData** (conditional)
```
1. READ: scheduled_load_run (find prior year cutoff)
2. TRANSFORM: Identify years outside retention policy
3. DELETE: scheduled_load_validation_result WHERE fps_year < cutoff
4. DELETE: fps_year_archive WHERE fps_year < cutoff
5. RECORD: scheduled_load_step_run(step='DeleteYearsFpsData', status='completed')
```

### **Phase 5: AddYearsFpsData** (conditional)
```
1. READ: sink_raw (new year's snapshot)
2. TRANSFORM: Filter to new fps_year, apply defaults
3. INSERT: operational.fps_year_totals (additive)
4. INSERT: operational.fps_project_all_current_year (additive)
5. RECORD: scheduled_load_step_run(step='AddYearsFpsData', status='completed')
```

### **Phase 6: CrossValidateYearTotals** (validation gate)
```
1. READ: operational.fps_year_totals (current year data)
2. TRANSFORM: Execute 12+ assertion queries
3. WRITE: fps.scheduled_load_validation_result (one row per assertion)
4. RECORD: scheduled_load_step_run(step='CrossValidateYearTotals', status='completed'/'failed')
5. GATE: Fail job if all assertions NOT passed
```

---

## Code Architecture Alignment

### **Handler Implementation (ScheduledLoadFromFpsJobHandler.cs)**
```csharp
// Each phase maps to a step handler
public class ProcessPreviousYearTotalsHandler : IScheduledLoadStepHandler
{
    // 1. Query operational.fps_year_totals
    // 2. Serialize to JSON
    // 3. Insert into operational.fps_year_archive
    // 4. Delete from operational.fps_year_totals
    // 5. Insert audit record into scheduled_load_step_run
}

public class ProcessCurrentYearTotalsHandler : IScheduledLoadStepHandler
{
    // 1. Query sink_raw.fps__fpsyeartotals (or direct cloud via service)
    // 2. Apply COALESCE, NULL defaults
    // 3. Upsert into operational.fps_year_totals
    // 4. Insert audit record into scheduled_load_step_run
}

// Similar handlers for other phases
```

### **Database Context Mapping (BatchJobsDbContext.cs)**
```csharp
// Map EF Core to 7 table contracts
public DbSet<ScheduledLoadRun> ScheduledLoadRuns { get; set; }
public DbSet<ScheduledLoadStepRun> ScheduledLoadStepRuns { get; set; }
public DbSet<ScheduledLoadValidationResult> ScheduledLoadValidationResults { get; set; }
public DbSet<FpsSourceProjectYear> FpsSourceProjectYears { get; set; }
public DbSet<FpsYearTotals> FpsYearTotals { get; set; }
public DbSet<FpsYearArchive> FpsYearArchives { get; set; }
public DbSet<FpsProjectAllCurrentYear> FpsProjectAllCurrentYears { get; set; }
```

---

## Migration Strategy

### **004_scheduled_load_tables.sql**
Create all 7 tables with:
- Exact column specifications from planning doc
- Proper indexes (fps_year, run_id, step_id where applicable)
- FK constraints linking child tables to parent ScheduledLoadRun
- JSONB columns for archive/payload tables

### **Seed Strategy**
- `001_seed_scheduled_job_master.sql`: Insert ScheduledLoadFromFps job into job_master
- `002_seed_scheduled_source_baseline.sql`: Populate fps_source_project_year test fixtures
- `003_seed_scheduled_expected_assertions.sql`: Pre-populate fps_year_totals with known-good baseline

### **Validation Strategy**
Create 12+ cross-validation queries (assertions A-L):
- Assert row counts match between source and target
- Assert totals sum correctly (cost components)
- Assert no NULL values in required columns
- Assert data freshness (updated_at within SLA)
- Insert results into scheduled_load_validation_result for audit trail

---

## Next Steps

1. **Create migration 004** with 7 table DDL
2. **Create EF Core Domain entities** for each table
3. **Create DbContext mappings** in BatchJobsDbContext
4. **Implement phase handlers** following the pattern above
5. **Implement assertion queries** in cross-validation phase
6. **Run integration tests** using seed data
7. **Execute job** and validate scheduled_load_validation_result
