# Instruction: Inputs & Solution Structure

## Inputs

Before generating any code, ask the user to confirm or provide:

| Input | Description | Source Location |
|---|---|---|
| `[App]` | App name: `FPS`, `PACT`, `Costbook`, or `PIMS` | User-supplied |
| `[FormName]` | The logical name of the form (PascalCase, no spaces) | Derived from the `.frm` file name |
| HTML prototype(s) | HTML files for the UI. Two patterns are supported — detect which is present before reading: **(a) Single-file:** `[FormName].html` — one file covers all CRUD modes; no per-mode differences; generates `Index.cshtml` with no CRUD partials. **(b) Multi-file:** `[FormName]-add.html` + `[FormName]-edit.html` (+ optional `[FormName]-delete.html`) — differences between files drive per-mode partial views. | `source/ui/[App]/[FormName].html` **or** `source/ui/[App]/[FormName]-add.html`, `-edit.html`, `-delete.html` |
| MS Access form | Extracted VBA/form definition | `source/frm/[App]/[FormName].frm` |
| Access queries *(optional)* | MS Access named queries | `source/msaccessqry/[App]/[QueryName].msaccsql` |
| Table definitions | PostgreSQL DDL — authoritative source for entity column names and C# data types | `source/pgsql/[schema]/Tables/` |
| Stored procedures *(optional)* | SQL Server stored procedures (converted to C# — not migrated to PostgreSQL) | `source/mssql/[DbName]/Procedures/` |
| Functions *(optional)* | SQL Server scalar / table-valued functions (converted to C# — not migrated to PostgreSQL) | `source/mssql/[DbName]/functions/` |
| Triggers *(optional)* | SQL Server DML triggers (converted to C# — not migrated to PostgreSQL) | `source/mssql/[DbName]/triggers/` |
| Views *(optional)* | PostgreSQL views used as RecordSources | `source/pgsql/[schema]/Views/` |

### App → Database mapping

Each app is backed by one or two AWS RDS PostgreSQL schemas. `[DbName]` is the PascalCase identifier used in MSSQL source paths; `[schema]` is its lowercase equivalent used in PostgreSQL source paths (e.g., `FPS` → `fps`, `mabarchive` → `mabarchive`).

Use this mapping to determine which `source/pgsql/[schema]/` subfolder to read for table and view definitions, and which `source/mssql/[DbName]/` subfolder for stored procedures, functions, and triggers.

| App | Primary DB | Secondary DB |
|---|---|---|
| `FPS` | `fps` | — |
| `PACT` | `fps` | — |
| `Costbook` | `fps` | `mabarchive` |
| `PIMS` | `mabarchive` | — |

Use this mapping to determine which `source/mssql/[DbName]/` subfolder to search when looking up a stored procedure, function, trigger, or view referenced by the form.

Read **all present files in full** before generating any code. Parse:
- **HTML** → UI structure, field names, labels, CSS classes, IDs, scripts, table columns
- **`.frm` file** → read every property and every VBA `Sub`/`Function` body in full. Specifically extract:
  - Form properties: `RecordSource`, `ControlSource`, `ValidationRule`, `DefaultValue`, `FilterOnLoad`
  - Form events: `OnLoad`, `OnOpen`, `OnCurrent`, `BeforeUpdate`, `AfterUpdate`, `OnClose`
  - Control properties: `RowSource`, `RowSourceType`, `ControlSource` for every control
  - Control events: `OnClick`, `AfterUpdate`, `BeforeUpdate`, `OnEnter`, `OnDblClick` for every control
  - Subform controls: for every `Begin Subform` block, extract `Name`, `SourceObject`, `LinkChildFields`, `LinkMasterFields` — `SourceObject = "Form.fsubXxx"` names the subform `.frm` file to read next
  - Every VBA `Sub` and `Function` — read the **full body**, not just the signature

  **RowSource / RecordSource discovery rules** (apply to every control with `RowSourceType = "Table/Query"` and the form's own `RecordSource`):
  - `RowSource = "SELECT ..."` — inline SQL: parse all table and object names; if the `FROM` clause names a known MS Access query (starts with `qry`) cross-reference against `.msaccsql` files; if it names a known PostgreSQL view (check `source/pgsql/[schema]/Views/`) record it as a view reference
  - `RowSource = "objectName"` / `RecordSource = "objectName"` (bare identifier, no SELECT) — determine the object type:
    - Starts with `qry` → MS Access named query → look up in `source/msaccessqry/[App]/`
    - Matches a known PostgreSQL view name → PostgreSQL view → look up in `source/pgsql/[schema]/Views/`
    - Otherwise → direct table reference; no cross-reference needed
  - `RowSource = "tableName"` — direct table reference; no cross-reference needed

  **VBA named-query invocation patterns** (how MS Access calls `.msaccsql` queries):
  - `MyDB.QueryDefs("queryName")` — loads a named query by name; match `queryName` against `source/msaccessqry/[App]/`
  - `myqd.Parameters("[ParamName]") = [Forms]![FormName]![ControlName]` — binds a parameter; the control value becomes the C# method parameter
  - `myqd.OpenRecordset(DB_OPEN_SNAPSHOT)` — executes the SELECT; result rows drive subsequent logic

  **VBA stored-procedure invocation pattern** (how MS Access calls SQL Server SPs):
  - `MyDB.CreateQueryDef("tempName")` — creates a temporary in-memory querydef
  - `qd.sql = "usp_SpName '" & [Field1] & "', '" & [Field2] & "'"` — assigns the SP call string; extract the SP name and each `& [FieldN] &` fragment as an ordered parameter
  - `DoCmd.OpenQuery "tempName"` (or `DoCmd.SetWarnings False` + `DoCmd.OpenQuery` + `DoCmd.SetWarnings True`) — executes the SP
  - `MyDB.QueryDefs.Delete "tempName"` — cleanup; marks the end of the SP call block
  - The entire block from `CreateQueryDef` to `QueryDefs.Delete` constitutes **one SP invocation** tied to the enclosing `Sub`'s event (e.g., a button `OnClick`)

  **SQL Server function calls in VBA:**
  - Scalar functions may appear as `= db.Execute("SELECT dbo.fnFunctionName('" & val & "')")` or embedded inside a `RecordSource` SQL string
  - Table-valued functions appear in `FROM dbo.fn_FunctionName(param)` inside a `RecordSource` or `RowSource` SQL string
  - Record the function name and look up `source/mssql/[DbName]/functions/`

  **PostgreSQL view references:**
  - A view name used directly as `RecordSource = "vViewName"` or inside `FROM vViewName` in SQL — look up `source/pgsql/[schema]/Views/`
  - Views are **not re-implemented** as LINQ — they are mapped as EF Core `[View]` entities (see repository rules in the backend layer instructions)

  **Trigger awareness — DML operations:**
  - Identify every table that the form writes to (via `RecordSource` bound to a table, or explicit `INSERT`/`UPDATE`/`DELETE` in VBA)
  - For each such table, scan `source/mssql/[DbName]/triggers/` and match trigger filenames to the table using the patterns below. Read every matched file to confirm the target table and operation from the SQL body (`ON [dbo].[TableName] FOR INSERT|UPDATE|DELETE`).

  **Trigger file naming patterns** (use to match triggers to a written table):

  | Pattern | Example filename | Target table | Operation |
  |---|---|---|---|
  | `DTrig_[Table]` | `DTrig_tblAdditionalCosts.sql` | `tblAdditionalCosts` | DELETE |
  | `UITrig_[Table]` | `UITrig_tblStaffJob.sql` | `tblStaffJob` | INSERT + UPDATE |
  | `tI_[Table]` | `tI_tlkpProject.sql` | `tlkpProject` | INSERT |
  | `tU_[Table]` | `tU_WorkGroup.sql` | `WorkGroup` | UPDATE |
  | `tD_[Table]` | `tD_tblkpProfitCentre.sql` | `tblkpProfitCentre` | DELETE |
  | `[Table]_DTrig` / `[Table]_Dtrig` | `tlkpJobCode_DTrig.sql` | `tlkpJobCode` | DELETE |
  | `[Table]_UTrig` | `tlkpJobCode_UTrig.sql` | `tlkpJobCode` | UPDATE |
  | `[Table]_ITrig` | `TimeCodeValid_ITrig.sql` | `TimeCodeValid` | INSERT |
  | `[XX]_LOG_DTrig` / `[XX]_LOG_ITrig` / `[XX]_LOG_UTrig` | `MO_LOG_DTrig.sql` | **Read `ON` clause** — table name is **not** in the filename (e.g., `MonthlyOutput`) | D / I / U |

  > **Always-authoritative:** regardless of the filename pattern, confirm both the target table and operation by reading the `ON [dbo].[TableName] FOR INSERT\|UPDATE\|DELETE` clause at the top of the trigger body.
  > Triggers on written tables must be converted to .NET Core logic at the data access layer.

  **Other VBA patterns to note:**
  - `DLookup("field", "table/query", "criteria")` — single-value lookup; becomes a LINQ `.FirstOrDefault()` call
  - `DCount("field", "table/query", "criteria")` — existence/count check; becomes `.AnyAsync()` or `.CountAsync()`
  - `DoCmd.OpenForm "formName", , , criteria` — record-filtered navigation; becomes a redirect with a query string filter
  - `DoCmd.ApplyFilter , "[Field] = 'value'"` — applies a record filter on the form; becomes a filtered GET endpoint call  - `=[SubformControlName].[Form]![FooterFieldName]` — reads a footer aggregate from the subform (e.g. a `ControlSource = "=[fsubXxx].[Form]![TotalNoTests]"` on the parent form); the subform footer field uses `=Sum([Field])`; becomes a read-only ViewModel property populated from a totals object returned alongside paged data, or computed from the grid rows; the HTML totals row is the visual counterpart
  - `Me![SubformControlName].Form.RecordSource = "SELECT ... WHERE [Field] <> 'value'"` — dynamically re-filters the subform's data source based on a parent-form control (e.g. a checkbox `AfterUpdate`); becomes an additional boolean or string filter parameter on the `Load[SubformName]Grid` endpoint, applied as a server-side LINQ `.Where()` clause
---

## Source Artefact Folders

> **Reading order:** Do **not** read all files in these folders upfront. First read the `.frm` in full and extract the complete reference list (all named queries, SPs, functions, views, and trigger-affected tables). Then read **only** the artefact files whose names appear in that list.

### MS Access Named Queries — `source/msaccessqry/[App]/`

**When to read:** only when the query name appears in the `.frm` reference list.  
**File location:** `source/msaccessqry/[App]/[QueryName].msaccsql`  
**Naming convention:** MS Access named queries typically begin with `qry` (e.g., `qryProjectCheck`). Any bare `RowSource` or `QueryDefs("...")` value starting with `qry` is a named query reference.

File structure:
```
QUERY NAME: <QueryName>
---------------------------------------------
TYPE: SELECT | ACTION | ...

SQL:
<PARAMETERS clause if any>
<SQL statement>
```

Parse:
- **PARAMETERS clause** — confirms types; cross-check names against `myqd.Parameters("[ParamName]") = [Control]` binding in the `.frm`
- **SQL statement** — convert to EF Core LINQ (see backend layer instructions)
- **Query type** — `SELECT` → LINQ query method on repository; `ACTION` → LINQ command method or service operation

---

### SQL Server Stored Procedures — `source/mssql/[DbName]/Procedures/`

**When to read:** only when the SP name appears in the `.frm` reference list (from `qd.sql = "usp_..."` or `qd.sql = "sp_..."` lines).  
**File location:** `source/mssql/[DbName]/Procedures/[SpName].sql`  
**Naming convention:** begin with `usp_` or `sp_`.  
**Conversion target:** fully convert to .NET Core — the SP is **not** called at runtime and is **not** migrated to PostgreSQL; its logic becomes C# repository methods and service orchestration.

Parse:
- **Parameters** — match `@Param` declarations to the parameter list from the `qd.sql` concatenation
- **Business guard checks** — `IF (SELECT Count(*)) RAISERROR` → service-level validation before writes
- **DML statements** — `INSERT`/`UPDATE`/`DELETE` → LINQ repository method calls
- **Nested `EXECUTE sp_*`** → one repository call per sub-SP; if source unavailable add `// TODO`
- **Transactions** — `BEGIN TRANSACTION … COMMIT` → service-level `BeginTransactionAsync` block

---

### SQL Server Functions — `source/mssql/[DbName]/functions/`

**When to read:** only when a scalar or table-valued function name appears in the `.frm` reference list (in a `RecordSource` SQL string or VBA `db.Execute` call).  
**File location:** `source/mssql/[DbName]/functions/[FunctionName].sql`  
**Conversion target:** convert to a C# private helper method or LINQ expression within the repository. **Not migrated to PostgreSQL.**

Parse:
- **Scalar functions** (`RETURNS scalar_type`) — convert the function body to a C# private static method on the repository, or inline the expression in a LINQ `select new` projection
- **Table-valued functions** (`RETURNS TABLE`) — convert to a LINQ query method that returns `IQueryable<T>`; register a keyless entity in `OnModelCreating` if the TVF result shape doesn't match an existing entity

---

### SQL Server Triggers — `source/mssql/[DbName]/triggers/`

**When to read:** for every table the form writes to, match the table name against trigger filenames using the naming patterns (see `.frm` parsing rules above), then read each matched file.  
**File location:** `source/mssql/[DbName]/triggers/[TriggerName].sql`  
**Conversion target:** convert trigger logic to .NET Core — the trigger is **not** left in the database and is **not** migrated to PostgreSQL; its behaviour becomes part of the repository `AddAsync` / `UpdateAsync` / `DeleteAsync` method for the affected table.

**Trigger filename → table and operation (quick-decode):**
- **Prefix signals operation, suffix is table:** `DTrig_[Table]` (DELETE), `UITrig_[Table]` (INSERT+UPDATE), `tI_[Table]` (INSERT), `tU_[Table]` (UPDATE), `tD_[Table]` (DELETE)
- **Suffix signals operation, prefix is table:** `[Table]_ITrig` (INSERT), `[Table]_UTrig` (UPDATE), `[Table]_DTrig`/`[Table]_Dtrig` (DELETE)
- **Ambiguous LOG pattern:** `[XX]_LOG_DTrig/ITrig/UTrig` — table name is **not** in the filename; must read the `ON [dbo].[TableName]` clause from the body

Parse:
- **Trigger type** — `FOR INSERT`, `FOR UPDATE`, `FOR DELETE` (or combined `FOR INSERT, UPDATE`) — determines which repository method hosts the converted logic
- **`INSERTED` / `DELETED` virtual tables** — these become the `entity` parameter already available in the repository method
- **Body logic** — convert exactly as a stored procedure body: guards → service validation, DML → repository calls, transactions → `BeginTransactionAsync`
- If the trigger spans multiple tables, extract each table's logic into its own repository method and call them in sequence from the service

---

### PostgreSQL Views — `source/pgsql/[schema]/Views/`

**When to read:** only when a view name appears in the `.frm` reference list as a `RecordSource` or `RowSource`.  
**File location:** `source/pgsql/[schema]/Views/[viewname].sql` (filenames are lowercase)  
**Fallback:** if the view file is not found in `source/pgsql/[schema]/Views/`, look up `source/mssql/[DbName]/views/[ViewName].sql` instead.  
**Conversion target:** views are **not** re-implemented as LINQ — they remain as database views and are accessed via a keyless EF Core entity mapped to the view.

Parse:
- **Column list** — the `SELECT` column list defines the EF Core entity properties (`[ViewName]View.cs`)
- **No LINQ translation** — do not attempt to convert the view body to LINQ; map `[App]DbContext.cs` with `modelBuilder.Entity<[ViewName]View>().HasNoKey().ToView("[viewname]")` (lowercase view name as it appears in PostgreSQL)
- Use the view entity for read-only queries only; never call `AddAsync` / `UpdateAsync` / `DeleteAsync` on a view entity

---

### PostgreSQL Table Definitions — `source/pgsql/[schema]/Tables/`

**When to read:** when generating entity classes and DbContext model configuration for any table the form reads from or writes to.  
**File location:** `source/pgsql/[schema]/Tables/[tablename].sql` (filenames are lowercase)  
**Fallback:** if the table file is not found in `source/pgsql/[schema]/Tables/`, look up `source/mssql/[DbName]/Tables/[TableName].sql` instead. Apply the same MSSQL → C# type mappings as if it were a PostgreSQL file, but note the MSSQL definition may contain `SysTimeStamp` or differ in PK composition — cross-check carefully.  
**Purpose:** authoritative source for column names, C# data types, nullability, PKs, and FKs. Use this instead of the MSSQL table definitions to avoid data type mismatches.

Key PostgreSQL → C# type mappings to apply when reading these files:

| PostgreSQL type | C# type | EF Core `[Column]` annotation |
|---|---|---|
| `public.citext` | `string` | `[Column(TypeName = "citext")]` |
| `character varying(n)` / `varchar(n)` | `string` | none needed |
| `integer` | `int` | none needed |
| `double precision` | `double` | none needed |
| `money` | `decimal` | `[Column(TypeName = "money")]` |
| `boolean` | `bool` | none needed |
| `timestamp without time zone` | `DateTime` | none needed |
| `text` | `string` | none needed |

> **Note:** MSSQL `SysTimeStamp` (`timestamp` / rowversion) columns are absent from the PostgreSQL schema — do not include them in entities. PostgreSQL PKs may include an `fpsyear` column not present in the MSSQL PK — always use the PostgreSQL PK definition.

---

### Cross-reference table

After reading the `.frm` and all referenced artefact files, confirm the completed map before generating any code:

| Artefact name | Type | Used in `.frm` at | Converts to |
|---|---|---|---|
| `qryProjectCheck` | MS Access query | `JobCode_BeforeUpdate` | `CheckProjectExistsAsync` on repository |
| `usp_Delete_Project` | Stored procedure | `btnDelProj_Click` | `DeleteProjectAsync` on service + `[HttpDelete]` endpoint |
| `fn_CalcBudget` | Scalar function | `RecordSource` SQL | C# private helper method on repository |
| `vStaffJobSummary` | SQL Server view | `RecordSource` on form load | Keyless EF entity `StaffJobSummaryView` mapped to `ToView("vStaffJobSummary")` |
| `tr_StaffJob_Insert` | Trigger (INSERT) | Written table: `tblStaffJob` | Logic merged into `StaffJobRepository.AddAsync` |

---

## Solution Structure Reference

```
src/
├── Apha.Common/                          # Shared contracts, utilities
│   └── Contracts/[App]/                  # Shared Request/Response DTOs
│
├── Apha.[App]/                           # Backend API (one per app)
│   ├── Apha.[App].Core/
│   │   ├── Entities/                     # EF Core entity classes
│   │   ├── Interfaces/                   # Repository interfaces
│   │   └── Pagination/                   # PagedData<T>, PaginationParameters<T>
│   ├── Apha.[App].Application/
│   │   ├── Dtos/                         # Application-layer DTOs
│   │   ├── Interfaces/                   # Service interfaces
│   │   ├── Services/                     # Business logic
│   │   ├── Mappings/EntityMapper.cs      # AutoMapper Entity ↔ DTO
│   │   └── Pagination/                   # PaginatedResult<T>, QueryParameters<T>
│   ├── Apha.[App].DataAccess/
│   │   ├── Data/[App]DbContext.cs        # EF Core DbContext
│   │   └── Repositories/                 # Repository implementations
│   └── Apha.[App].Api/
│       ├── Controllers/                  # API controllers
│       ├── Mappings/RequestMapper.cs     # AutoMapper DTO ↔ Contract (Req/Res)
│       └── Extensions/ServiceCollectionExtension.cs
│
└── Apha.FPSApps/                         # Unified MVC frontend
    ├── Apha.FPSApps.Application/
    │   ├── Dtos/[App]/                   # Frontend application DTOs
    │   ├── Interfaces/                   # Frontend service interfaces
    │   ├── Interfaces/[App]ApiClients/   # API client interfaces
    │   └── Services/                     # Frontend application services
    ├── Apha.FPSApps.Infrastructure/
    │   └── Integrations/[App]Apis/
    │       └── Clients/                  # HttpClient API client implementations
    └── Apha.FPSApps.Web/
        ├── Areas/[App]/
        │   ├── Controllers/              # MVC controllers
        │   ├── Models/                   # ViewModels and grid item models
        │   └── Views/[FormName]/         # cshtml views
        ├── Views/Shared/_DataGrid.cshtml # Reusable data grid partial
        └── Mappings/ViewModelMapper.cs   # AutoMapper ViewModel ↔ DTO
```

---

## End-to-End Flow

```
cshtml View
  → MVC Controller (Apha.FPSApps.Web)
    → Application Service (Apha.FPSApps.Application)
      → Infrastructure API Client (Apha.FPSApps.Infrastructure)
        → Web API Controller (Apha.[App].Api)
          → API Application Service (Apha.[App].Application)
            → Repository (Apha.[App].DataAccess)
              → Database (EF Core / SQL Server)
```
