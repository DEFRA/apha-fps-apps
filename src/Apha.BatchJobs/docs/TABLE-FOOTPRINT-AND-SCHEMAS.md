# ScheduledLoadFromFps Table Footprint & Schemas

## Document Purpose
This document maps the complete table structure consumed by the legacy ScheduledLoadFromFps calculation, including:
- Database and schema organization
- All source, intermediate, and target tables
- Complete column definitions with SQL data types
- Row counts and logical relationships
- SQL queries for each major operation
- Data flow and transformation points

---

## 1. Database & Schema Organization

### 1.1 High-Level Database Map

```
┌─────────────────────────────────────────────────────────────────┐
│  FPS Database (Live/Operational)                                │
│  - Source of truth for current fiscal year data                 │
│  - Read-only for ScheduledLoadFromFps                           │
│                                                                  │
│  Tables:                                                        │
│  • fps.fpsyeartotals       (Live project year calculations)     │
│  • fps.tlkpproject         (Live project master lookup)         │
└─────────────────────────────────────────────────────────────────┘
                                ↓
                    (sp_LoadFromFPS orchestration)
                                ↓
┌─────────────────────────────────────────────────────────────────┐
│  MAB_Archive Database (Archive/Reporting)                       │
│  - Target for historical reporting and analysis                 │
│  - Delete/reload behavior within SAME calendar cycle            │
│                                                                  │
│  Tables:                                                        │
│  • mabarchive.my_fpsyeartotals      (Year-scoped archive)       │
│  • mabarchive.my_tlkpproject_all    (Year-scoped project all)   │
│  • 21+ additional MY_* tables        (Supporting year archives)  │
└─────────────────────────────────────────────────────────────────┘
```

### 1.2 Schema Organization

| Schema      | Database    | Purpose                   | Ownership                      |
|-------------|-------------|---------------------------|--------------------------------|
| fps         | FPS         | Live source data          | Read-only for batch jobs       |
| mabarchive  | MAB_Archive | Archive/reporting target  | Batch job write/delete access  |
| operational | Target      | .NET batch operational    | New operational tracking       |

---

## 2. Source Tables (FPS Database)

### 2.1 fps.fpsyeartotals

**Purpose**: Live project fiscal year totals; primary calculation source.

**Database**: FPS (cloud source database)

**Scope**: One row per unique `parentproject` (composite key across years)

**Primary Key**: `parentproject` (NOTE: NOT unique across fiscal years; must filter by current/previous year)

**SQL Definition**:
```sql
CREATE TABLE fps.fpsyeartotals (
    parentproject          VARCHAR(20)      NOT NULL PRIMARY KEY,
    program                VARCHAR(10)      NOT NULL,
    totaladditionalcosts   MONEY            NULL,              -- Cost sum 1
    totalanimalcosts       DOUBLE PRECISION NULL,              -- Cost sum 2
    totalstaffcosts        DOUBLE PRECISION NULL,              -- Cost sum 3
    totaltestcosts         DOUBLE PRECISION NULL,              -- Cost sum 4
    totalcosts             DOUBLE PRECISION NULL,              -- Calculated: SUM of above + PlanCaseworkDebit
    custincome             MONEY            NOT NULL,          -- Income source 1
    transferincome         MONEY            NOT NULL,          -- Income source 2
    totalincome            MONEY            NOT NULL,          -- Calculated: custincome + transferincome
    budget_cvl             MONEY            NULL,              -- Budget constraint
    requiredprofit         MONEY            NULL,              -- Profit target
    manager                VARCHAR(50)      NULL,              -- Project manager
    customer               VARCHAR(50)      NULL,              -- Customer identifier
    projectstatus          VARCHAR(50)      NULL,              -- Active/Completed/etc.
    pvsincome              MONEY            NULL,              -- PVS-specific income
    plancaseworkdebit      MONEY            NULL,              -- Casework plan debit (added to costs)
    totalpaycosts          DOUBLE PRECISION NULL,              -- Pay cost component
    fpsyear                INTEGER          NULL               -- Fiscal year (NOT enforced; metadata only)
);
```

**Columns by Category**:

| Category          | Columns                                              | NULL Behavior                                  |
|-------------------|------------------------------------------------------|------------------------------------------------|
| **Identity**      | parentproject, fpsyear                               | parentproject NOT NULL; fpsyear can be NULL   |
| **Source Costs**  | totaladditionalcosts, totalanimalcosts, totalstaffcosts, totaltestcosts | All nullable; no implicit 0 conversion         |
| **Derived Costs** | totalcosts, totalpaycosts                            | Calculated; null if any source is null         |
| **Source Income** | custincome, transferincome                           | Both NOT NULL; guaranteed entry                |
| **Derived Income**| totalincome                                          | custincome + transferincome; no null wrapping  |
| **Business**      | program, manager, customer, projectstatus, budget_cvl, requiredprofit, pvsincome, plancaseworkdebit | All nullable |

**Row Count Estimate**: ~500-1000 projects per fiscal year (varies by program)

**Critical Parity Notes**:
1. **No Year Filter in Schema**: Schema does NOT enforce year grouping; year must be applied at query level
2. **NULL Propagation in Income**: `totalincome = custincome + transferincome` has NO explicit null-to-zero wrapper in legacy logic; null propagates if either input is NULL
3. **No Cascading NULL Handling in Costs**: Legacy `sp_createFPSTotals` does not wrap individual cost sums in COALESCE(col, 0); NULL propagates through SUM aggregations

---

### 2.2 fps.tlkpproject

**Purpose**: Master lookup table for project attributes; enrichment source for year totals archival.

**Database**: FPS (cloud source database)

**Scope**: One row per unique project identifier; year-scoped in practice.

**Primary Key**: `parentproject` (citext, case-insensitive)

**SQL Definition**:
```sql
CREATE TABLE fps.tlkpproject (
    parentproject          CITEXT           NOT NULL PRIMARY KEY,
    projecttitle           VARCHAR(200)     NOT NULL,
    program                CITEXT           NOT NULL,              -- Program (case-insensitive)
    customer               CITEXT           NOT NULL,              -- Customer (case-insensitive)
    manager                VARCHAR(50)      NULL,
    transferincome         MONEY            NOT NULL,
    custincome             MONEY            NOT NULL,
    wip_eoy                MONEY            DEFAULT 0,             -- Work-in-progress end-of-year
    wip_limit              MONEY            NULL,
    wip_current            MONEY            NULL,
    projectstatus          CITEXT           NOT NULL,
    costbookno             VARCHAR(50)      NULL,
    datecreated            TIMESTAMP        DEFAULT CURRENT_TIMESTAMP,
    feccost                MONEY            DEFAULT 0,
    profit                 MONEY            DEFAULT 0,
    budget_cvl             MONEY            DEFAULT 0,
    datecosted             TIMESTAMP        NULL,
    disease                CITEXT           NOT NULL,
    contract               CITEXT           DEFAULT '0' NOT NULL,
    projectparent          VARCHAR(50)      NULL,
    shorttitle             VARCHAR(30)      NULL,
    caseworksub            NUMERIC(5,4)     NULL,
    pvsincome              MONEY            NULL,
    plancaseworkdebit      MONEY            NULL,
    finished               SMALLINT         DEFAULT 0,
    owningrc               VARCHAR(50)      NULL,
    comments               TEXT             NULL,
    carryover              MONEY            NULL,
    carryoverseed          MONEY            NULL,
    isdefraproject         SMALLINT         NOT NULL,
    costcentre             DOUBLE PRECISION NULL,
    oracleprojectcode      VARCHAR(50)      NULL,
    subaccountcode         CITEXT           NULL,
    projectgroup           CITEXT           NULL,
    incomeaccountcode      CITEXT           NOT NULL,
    fpsyear                INTEGER          NULL
);
```

**Columns by Category**:

| Category          | Columns                                           | Purpose in ScheduledLoadFromFps        |
|-------------------|---------------------------------------------------|----------------------------------------|
| **Identity**      | parentproject, fpsyear                            | Match key to fpsyeartotals              |
| **Project Info**  | projecttitle, projectparent, shorttitle, comments | Enrichment fields for archive          |
| **Program/Disease**| program, disease, projectstatus, contract         | Classification + status tracking       |
| **Org Structure** | customer, manager, owningrc, costcentre           | Org rollup + cost center allocation    |
| **Income**        | custincome, transferincome, pvsincome             | Income source validation               |
| **Costs**         | budget_cvl, plancaseworkdebit, caseworksub        | Cost component contributors            |
| **WIP**           | wip_eoy, wip_limit, wip_current                   | Work-in-progress tracking              |
| **Accounting**    | costbookno, oracleprojectcode, incomeaccountcode, subaccountcode, projectgroup | GL account mapping |
| **Status**        | finished, isdefraproject, carryover, carryoverseed | Completion flags + carryover tracking |

**Row Count Estimate**: ~1500-3000 unique projects (master list; larger than fpsyeartotals)

**Critical Parity Notes**:
1. **CITEXT Columns**: program, customer, disease, contract, subaccountcode, projectgroup, incomeaccountcode use case-insensitive text; must preserve when archiving
2. **No Year Partition**: tlkpproject is a flat master table; year filtering applied at query level for archive consistency
3. **Default Values**: Several columns have defaults (wip_eoy=0, feccost=0, profit=0, budget_cvl=0); these are NOT implicit in calculations; used only when reading from table

---

## 3. Archive Tables (MAB_Archive Database)

### 3.1 mabarchive.my_fpsyeartotals

**Purpose**: Archive of fiscal year totals; stores 1-N copies per fiscal year (delete-reload pattern).

**Database**: MAB_Archive (cloud reporting/archive database)

**Scope**: One row per unique (year, parentproject) pair.

**Primary Key**: `(year, parentproject)` (composite; enforces one row per year+project)

**SQL Definition**:
```sql
CREATE TABLE mabarchive.my_fpsyeartotals (
    year                   SMALLINT         NOT NULL,              -- Archive year key (NOT filters from fpsyeartotals table)
    parentproject          VARCHAR(20)      NOT NULL,
    program                VARCHAR(10)      NOT NULL,
    totaladditionalcosts   MONEY            NULL,
    totalanimalcosts       DOUBLE PRECISION NULL,
    totalstaffcosts        DOUBLE PRECISION NULL,
    totaltestcosts         DOUBLE PRECISION NULL,
    totalcosts             DOUBLE PRECISION NULL,
    custincome             MONEY            NOT NULL,
    transferincome         MONEY            NOT NULL,
    totalincome            MONEY            NOT NULL,
    budget_cvl             MONEY            NULL,
    requiredprofit         MONEY            NULL,
    manager                VARCHAR(50)      NULL,
    customer               VARCHAR(50)      NULL,
    projectstatus          VARCHAR(50)      NOT NULL,
    pvsincome              MONEY            NULL,
    plancaseworkdebit      MONEY            NULL,
    totalpaycosts          DOUBLE PRECISION NULL,
    CONSTRAINT pk_my_fpsyeartotals PRIMARY KEY (year, parentproject)
);
```

**Differences from Source (fps.fpsyeartotals)**:
1. **Year Column**: Added as composite PK (encodes year in archive; required for multi-year retention)
2. **projectstatus**: Changed to NOT NULL (enforced at archive write time)
3. **fpsyear**: Removed (redundant; year column replaces it)

**Row Count**: ~500-1000 rows per fiscal year × number of retained years (e.g., 5K-10K total)

**Write Pattern**: DELETE WHERE year = @year; INSERT INTO ... SELECT (hard delete + reload; NOT upsert)

---

### 3.2 mabarchive.my_tlkpproject_all

**Purpose**: Archive of project master data enrichment; stores one row per (year, parentproject) pair for full context preservation.

**Database**: MAB_Archive

**Scope**: One row per unique (year, parentproject) pair; captures snapshot at archive time.

**Primary Key**: `(year, parentproject)`

**SQL Definition**:
```sql
CREATE TABLE mabarchive.my_tlkpproject_all (
    year                   SMALLINT         NOT NULL,
    parentproject          VARCHAR(20)      NOT NULL,
    program                VARCHAR(10)      NULL,
    customer               VARCHAR(50)      NULL,
    manager                VARCHAR(50)      NULL,
    transferincome         MONEY            NULL,
    custincome             MONEY            NULL,
    wip_eoy                MONEY            NULL,
    wip_limit              MONEY            NULL,
    wip_current            MONEY            NULL,
    projectstatus          VARCHAR(50)      NULL,
    datecreated            DATE             NULL,
    feccost                MONEY            NULL,
    profit                 MONEY            NULL,
    budget_cvl             MONEY            NULL,
    caseworksub            NUMERIC(5,4)     NULL,
    pvsincome              MONEY            NULL,
    plancaseworkdebit      MONEY            NULL,
    source                 CHAR(5)          NULL,
    disease                VARCHAR(50)      NULL,
    contract               VARCHAR(10)      NULL,
    finished               SMALLINT         NULL,
    comments               TEXT             NULL,
    carryover              MONEY            NULL,
    isdefraproject         SMALLINT         NULL,
    costcentre             DOUBLE PRECISION NULL,
    oracleprojectcode      VARCHAR(50)      NULL,
    subaccountcode         VARCHAR(50)      NULL,
    projectgroup           VARCHAR(50)      NULL,
    incomeaccountcode      VARCHAR(50)      NULL,
    CONSTRAINT pk_my_tlkpproject_all PRIMARY KEY (year, parentproject)
);
```

**Differences from Source (fps.tlkpproject)**:
1. **Year Column**: Added as composite PK
2. **CITEXT to VARCHAR**: All CITEXT columns converted to VARCHAR (case-insensitive semantics NOT preserved in archive)
3. **Simplified Columns**: Removed columns not needed for multi-year retrospective:
   - projecttitle, projectparent, shorttitle → dropped
   - datecreated → simplified to DATE (no TIMESTAMP)
   - owningrc → dropped
   - carryoverseed → dropped
4. **source Column**: Added (identifies origin: 'FPS' or 'MAB' when joining archives)
5. **Default Values**: All defaults removed (archive stores actual values, not defaults)

**Row Count**: Same as my_fpsyeartotals per year

**Write Pattern**: Same as my_fpsyeartotals (DELETE WHERE year = @year; INSERT INTO SELECT)

---

## 4. Supporting Archive Tables (21+ Additional Tables)

The legacy `sp_AddYearsFPSData` procedure fans out to 24+ `sp_AddMY_*` procedures, each populating archive tables for:

| Archive Table Name        | Source Tables              | Key Aggregations          | Scope      |
|---------------------------|----------------------------|---------------------------|------------|
| MY_MonthlyOutput          | qryTotalAnimalCosts, etc.  | Monthly rollup per project| Year + Month |
| MY_MonthlyTime            | Time tracking + costs      | Monthly time allocation   | Year + Month |
| MY_ProjectMonthFinal      | All cost components        | Final monthly totals      | Year + Month + Project |
| MY_tblAdditionalCosts     | qryTotalAdditionalCosts    | Additional cost breakdown | Year + Project |
| MY_tlkpYear               | Reference data             | Year-level metadata       | Year       |
| MY_tblAnimalReq           | Animal requirement archive | Animal-specific costs     | Year + Project |
| MY_tblStaffRequ           | Staff requirement archive  | Staff allocation archive  | Year + Project |
| (18+ other MY_* tables)   | (Various source tables)    | (Domain-specific)         | (Year-scoped) |

**Characteristics**:
- Each populated by dedicated `sp_AddMY_*` procedure
- All follow year-scoped DELETE + INSERT pattern (no merges)
- No join complexity; simple aggregation from source queries

---

## 5. .NET Operational Tables (New Schema)

### 5.1 operational.scheduled_load_run

**Purpose**: Execution lifecycle tracking (replaces legacy job_history-style logging).

```sql
CREATE TABLE operational.scheduled_load_run (
    run_id           UUID        NOT NULL DEFAULT gen_random_uuid(),
    job_name         VARCHAR(100) NOT NULL,
    fps_year         INTEGER      NOT NULL,              -- Year being processed
    job_started_at   TIMESTAMPTZ  NOT NULL,
    job_completed_at TIMESTAMPTZ  NULL,
    final_status     VARCHAR(50)  NULL,                 -- Success | Failed | Cancelled
    correlation_id   VARCHAR(64)  NOT NULL,
    created_at       TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT pk_scheduled_load_run PRIMARY KEY (run_id)
);
```

**Usage**: One row per entire ScheduledLoadFromFps execution.

---

### 5.2 operational.scheduled_load_step_run

**Purpose**: Audit trail for each procedure step (sp_deleteFPSTotals, sp_createFPSTotals, etc.).

```sql
CREATE TABLE operational.scheduled_load_step_run (
    step_run_id   UUID        NOT NULL DEFAULT gen_random_uuid(),
    run_id        UUID        NOT NULL,
    step_name     VARCHAR(100) NOT NULL,
    step_sequence INTEGER      NOT NULL,
    started_at    TIMESTAMPTZ  NOT NULL,
    completed_at  TIMESTAMPTZ  NULL,
    step_status   VARCHAR(50)  NOT NULL,          -- Running | Completed | Failed | Skipped
    error_message VARCHAR(500) NULL,
    rows_affected INTEGER      NULL,
    created_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_scheduled_load_step_run_run_id
        FOREIGN KEY (run_id)
        REFERENCES operational.scheduled_load_run (run_id)
        ON DELETE CASCADE
);
```

**Usage**: One row per step per run (e.g., 6-8 rows per execution).

**Step Sequence Example**:
1. sp_deleteFPSTotals (previous year)
2. sp_createFPSTotals (previous year)
3. sp_DeleteYearsFPSData (previous year)
4. sp_AddYearsFPSData (previous year)
5. (conditional) sp_deleteFPSTotals (current year) — if month > 4
6. (conditional) sp_createFPSTotals (current year) — if month > 4
7. (conditional) sp_DeleteYearsFPSData (current year) — if month > 4
8. (conditional) sp_AddYearsFPSData (current year) — if month > 4

---

### 5.3 operational.scheduled_load_validation_result

**Purpose**: Quality gate assertions (cross-checks of archive consistency).

```sql
CREATE TABLE operational.scheduled_load_validation_result (
    validation_id         UUID        NOT NULL DEFAULT gen_random_uuid(),
    run_id                UUID        NOT NULL,
    assertion_code        VARCHAR(50)  NOT NULL,
    assertion_description VARCHAR(500) NOT NULL,
    expected_value        NUMERIC(18,2) NULL,
    actual_value          NUMERIC(18,2) NULL,
    passed                BOOLEAN      NOT NULL,
    error_message         VARCHAR(500) NULL,
    checked_at            TIMESTAMPTZ  NOT NULL,
    created_at            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_scheduled_load_validation_run_assertion
        UNIQUE (run_id, assertion_code)
);
```

**Usage**: One row per assertion per run (e.g., 10+ quality gate checks per execution).

**Example Assertions**:
- `ARCHIVE_ROW_COUNT_MATCHES_SOURCE`: COUNT(mabarchive.my_fpsyeartotals) = COUNT(fps.fpsyeartotals WHERE fpsyear = @year)
- `NO_NULL_TOTALCOSTS_IN_ARCHIVE`: COUNT(*) WHERE totalcosts IS NULL = 0
- `INCOME_FORMULA_VERIFICATION`: SUM(custincome + transferincome) = SUM(totalincome)

---

### 5.4 operational.fps_source_project_year

**Purpose**: Test fixture / staging table aligned to FPS source contract.

```sql
CREATE TABLE operational.fps_source_project_year (
    year                 SMALLINT            NOT NULL,
    parentproject        VARCHAR(20)         NOT NULL,
    program              VARCHAR(10)         NOT NULL,
    totaladditionalcosts MONEY               NULL,
    totalanimalcosts     DOUBLE PRECISION    NULL,
    totalstaffcosts      DOUBLE PRECISION    NULL,
    totaltestcosts       DOUBLE PRECISION    NULL,
    totalcosts           DOUBLE PRECISION    NULL,
    custincome           MONEY               NOT NULL,
    transferincome       MONEY               NOT NULL,
    totalincome          MONEY               NOT NULL,
    budget_cvl           MONEY               NULL,
    requiredprofit       MONEY               NULL,
    manager              VARCHAR(50)         NULL,
    customer             VARCHAR(50)         NULL,
    projectstatus        VARCHAR(50)         NULL,
    pvsincome            MONEY               NULL,
    plancaseworkdebit    MONEY               NULL,
    totalpaycosts        DOUBLE PRECISION    NULL,
    CONSTRAINT pk_fps_source_project_year PRIMARY KEY (year, parentproject)
);
```

**Purpose**: Enables deterministic unit/integration testing without cloud FPS database dependency.

**Row Count**: Populated by test seeds; typically 10-100 test rows per scenario.

---

### 5.5 operational.fps_year_totals

**Purpose**: Runtime target table for calculations (post-`sp_createFPSTotals`, pre-archive).

**Structure**: Identical to fps_source_project_year (same columns, same PK).

**Usage**: Intermediate calculation table; populated by procedure, consumed by validation + archival steps.

---

### 5.6 operational.fps_year_archive

**Purpose**: Backup/audit of archive state before deletion (safety measure for audit trail).

**Structure**: fps_year_totals + 2 metadata columns:
```sql
archived_at       TIMESTAMPTZ         NOT NULL DEFAULT NOW(),
archive_reason    VARCHAR(100)        NOT NULL DEFAULT 'Before deletion'
```

**Usage**: Optional; populated by procedures that want to retain full audit history.

---

### 5.7 operational.fps_project_all_current_year

**Purpose**: Runtime cache of current-year project enrichment (pre-archive snapshot).

**Structure**: Identical to mabarchive.my_tlkpproject_all + metadata:
```sql
refreshed_at      TIMESTAMPTZ         NOT NULL DEFAULT NOW()
```

**Usage**: Mirrors full project master snapshot at execution time; enables year-scoped enrichment for archive writes.

---

## 6. SQL Queries Template by Operation

### 6.1 sp_deleteFPSTotals (Delete from Archive)

**Purpose**: Clear previous cached results from archive (year-scoped).

**Pseudocode Query**:
```sql
-- Delete all archive records for the specified year
DELETE FROM mabarchive.my_fpsyeartotals
WHERE year = @year;

-- Also delete all supporting MY_* archive tables for the year
DELETE FROM mabarchive.my_tlkpproject_all      WHERE year = @year;
DELETE FROM mabarchive.my_monthlyoutput        WHERE year = @year;
DELETE FROM mabarchive.my_projectmonthfinal    WHERE year = @year;
-- ... 21+ more deletions
```

**Scope**: Affects 23+ archive tables; determines completeness of archive refresh.

**Critical Behavior**:
- **No FPS database touch**: Only affects MAB_Archive
- **All-or-nothing pattern**: Either all tables deleted, or fail atomically
- **Year-only filter**: No month-level granularity; entire fiscal year cleared

---

### 6.2 sp_createFPSTotals (Calculate & Insert Totals)

**Purpose**: Aggregate project costs/income from source queries; insert into archive.

**Pseudocode Query**:
```sql
INSERT INTO operational.fps_year_totals (
    year, parentproject, program, totaladditionalcosts, totalanimalcosts, 
    totalstaffcosts, totaltestcosts, totalcosts, custincome, transferincome,
    totalincome, budget_cvl, requiredprofit, manager, customer, projectstatus,
    pvsincome, plancaseworkdebit, totalpaycosts
)
SELECT
    @year AS year,
    fp.parentproject,
    fp.program,
    COALESCE(qac.total, 0) AS totaladditionalcosts,
    COALESCE(qac2.total, 0) AS totalanimalcosts,
    COALESCE(qsc.total, 0) AS totalstaffcosts,
    COALESCE(qtc.total, 0) AS totaltestcosts,
    COALESCE(qac.total, 0) + COALESCE(qac2.total, 0) + COALESCE(qsc.total, 0) 
      + COALESCE(qtc.total, 0) + COALESCE(fp.plancaseworkdebit, 0) AS totalcosts,
    fp.custincome,
    fp.transferincome,
    fp.custincome + fp.transferincome AS totalincome,
    fp.budget_cvl,
    fp.requiredprofit,
    fp.manager,
    fp.customer,
    fp.projectstatus,
    fp.pvsincome,
    fp.plancaseworkdebit,
    COALESCE(qpc.total, 0) AS totalpaycosts
FROM fps.fpsyeartotals fp
LEFT JOIN qryTotalAdditionalCosts qac ON qac.parentproject = fp.parentproject
LEFT JOIN qryTotalAnimalCosts qac2 ON qac2.parentproject = fp.parentproject
LEFT JOIN qryTotalStaffCosts qsc ON qsc.parentproject = fp.parentproject
LEFT JOIN qryTotalTestCosts qtc ON qtc.parentproject = fp.parentproject
LEFT JOIN qryTotalPayCosts qpc ON qpc.parentproject = fp.parentproject
WHERE fp.fpsyear = @year;
```

**Semantic Points**:
1. **LEFT JOIN**: Outer join preserves all projects even if cost queries return no rows
2. **SELECT DISTINCT**: May be used to de-duplicate cost queries (prevents inflated totals)
3. **COALESCE(col, 0)**: Used for individual cost aggregations; NOT on totalincome formula
4. **totalincome = custincome + transferincome**: NO COALESCE wrapper; propagates NULL if either input is NULL

---

### 6.3 sp_DeleteYearsFPSData (Hard Delete from MAB_Archive)

**Purpose**: Remove entire calendar year from archive in preparation for reload.

**Pseudocode Query**:
```sql
-- Delete all archive tables for the specific year
DELETE FROM mabarchive.my_fpsyeartotals WHERE year = @year;
DELETE FROM mabarchive.my_tlkpproject_all WHERE year = @year;
DELETE FROM mabarchive.my_monthlyoutput WHERE year = @year;
DELETE FROM mabarchive.my_monthlycomments WHERE year = @year;
-- ... 19+ more supporting tables
```

**Scope Coverage**: 23 total archive tables cleared.

**Critical Behavior**:
- **Cascading deletes**: Foreign key cascades ensure referential integrity
- **Transaction boundary**: All deletes within single transaction; roll back on any failure
- **Log/Audit**: No retained history (hard delete); audit trail only in operational.scheduled_load_run

---

### 6.4 sp_AddYearsFPSData (Fan-out Archive Population)

**Purpose**: Reload archive tables from freshly-calculated totals.

**Pseudocode Structure**:
```sql
-- Step 1: Insert year totals into archive
INSERT INTO mabarchive.my_fpsyeartotals (...) 
SELECT * FROM operational.fps_year_totals WHERE year = @year;

-- Step 2: Insert project enrichment into archive
INSERT INTO mabarchive.my_tlkpproject_all (...)
SELECT year, parentproject, ... FROM operational.fps_project_all_current_year
WHERE year = @year;

-- Step 3: Call 24+ sp_AddMY_* procedures (fan-out pattern)
EXEC sp_AddMY_MonthlyOutput @year;
EXEC sp_AddMY_MonthlyTime @year;
EXEC sp_AddMY_tblAdditionalCosts @year;
EXEC sp_AddMY_tblAnimalReq @year;
EXEC sp_AddMY_tblStaffRequ @year;
-- ... 19+ more procedures
```

**Orchestration**: Sequential execution; each procedure depends on prior successful archive writes.

---

## 7. Complete Data Flow Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│  Job Trigger (ScheduledLoadFromFpsJobHandler)                    │
│  • Current date → calculate current/previous fiscal years        │
│  • Determine if month > 4 (current year processing flag)         │
└──────────────────────────────────────────┬───────────────────────┘
                                           │
        ┌──────────────────────────────────┼──────────────────────────────────┐
        │                                  │                                  │
        v                                  v                                  v
┌──────────────────────┐        ┌──────────────────────┐      ┌──────────────────────┐
│  PREVIOUS YEAR CYCLE │        │  CURRENT YEAR CYCLE  │      │  ALWAYS EXECUTE      │
│  (Always)            │        │  (If Month > 4)      │      │  ProjectAll Refresh  │
└──────────────────────┘        └──────────────────────┘      └──────────────────────┘
        │                                  │                          │
        │                                  │                          │
   Step 1+2:                           Step 5+6:                  Step 9:
   Delete + Recreate                   Delete + Recreate         Refresh Project All
        │                                  │                          │
        v                                  v                          v
   [Previous Year]                    [Current Year]            [Copy current
   sp_deleteFPSTotals                 sp_deleteFPSTotals         year tlkpproject
   sp_createFPSTotals                 sp_createFPSTotals         snapshot to archive]
        │                                  │                          │
        │                                  │                          │
   Step 3+4:                           Step 7+8:                      │
   Delete + Reload Archive            Delete + Reload Archive        │
        │                                  │                          │
        v                                  v                          │
   sp_DeleteYearsFPSData             sp_DeleteYearsFPSData           │
   sp_AddYearsFPSData                sp_AddYearsFPSData              │
   (24+ MY_* procedures)             (24+ MY_* procedures)           │
        │                                  │                          │
        └──────────────────┬───────────────┘                          │
                           │                                          │
                           v                                          v
                   ┌────────────────────────────────────┐
                   │  MAB_Archive Updated              │
                   │  • my_fpsyeartotals (prev + curr) │
                   │  • my_tlkpproject_all updated     │
                   │  • 21+ supporting MY_* tables     │
                   │                                  │
                   │  reporting.job_history logged   │
                   └────────────────────────────────────┘
```

---

## 8. Row Count & Volume Summary

| Table/Category             | Rows/Year | Retention | Total Storage |
|----------------------------|-----------|-----------|---------------|
| fps.fpsyeartotals (source) | ~800      | Current only | ~100 KB     |
| fps.tlkpproject (master)   | ~2000     | Current only | ~500 KB     |
| my_fpsyeartotals (archive) | ~800      | 5+ years  | ~4 MB         |
| my_tlkpproject_all (archive) | ~800    | 5+ years  | ~2 MB         |
| 21+ supporting MY_* tables | ~50K      | 5+ years  | ~50 MB        |
| **Total Archive**          | **~100K** | **5+ yrs**| **~60 MB**    |

---

## 9. Key Parity Preservation Points

### 9.1 Must-Not-Drift: Schema Alignment

| Source Location            | Archive Location       | Drift Risk Level |
|----------------------------|------------------------|------------------|
| fps.fpsyeartotals columns  | my_fpsyeartotals      | CRITICAL         |
| fps.tlkpproject columns    | my_tlkpproject_all     | CRITICAL         |
| Derived formulas (totalcosts, totalincome) | Archive values | CRITICAL |

### 9.2 Must-Not-Drift: Null Handling

| Formula/Field               | Null Behavior                            | Drift Risk |
|-----------------------------|------------------------------------------|------------|
| totalcosts = sum of 5 costs | NULL if ANY cost is NULL (no COALESCE)   | CRITICAL  |
| totalincome = custom + trans| NULL if EITHER income is NULL (no wrap)  | CRITICAL  |
| Individual costs            | All nullable (not forced to 0)           | CRITICAL  |

### 9.3 Must-Not-Drift: Delete/Insert Behavior

| Operation                 | Pattern        | Drift Risk |
|---------------------------|----------------|------------|
| sp_deleteFPSTotals        | DELETE only    | CRITICAL   |
| sp_DeleteYearsFPSData     | DELETE only    | CRITICAL   |
| sp_AddYearsFPSData        | INSERT new     | CRITICAL   |
| All archive writes        | DELETE + INSERT (not UPSERT) | CRITICAL |

---

## 10. References & Cross-Links

- **Baseline Calculation Document**: [SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md](SCHEDULED-LOAD-FPS-DATA-FLOW-AND-CALCULATIONS.md)
- **Worked Example Flow**: [FPS_SQL_Legacy_Flow_Extensive.md](FPS_SQL_Legacy_Flow_Extensive.md)
- **SQL Scripts Location**: `src/Apha.BatchJobs/database/sql/`
  - `004_scheduled_load_tables.sql` — Operational tables
  - `006_fps_mabarchive_source_tables.sql` — Source + archive table DDL
- **.NET Implementation**: `src/Apha.BatchJobs/Apha.BatchJobs.Application/Jobs/ScheduledLoadFromFps/`

---

## Document Version

- **Created**: 2026-04-17
- **Last Updated**: 2026-04-17
- **Status**: Complete (comprehensive table footprint + SQL + schemas)
- **Next Steps**: Map individual procedure implementations to C# domain objects (Phase 2)
