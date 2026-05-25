# Instruction: Backend Layers (Steps 1–9)

> **Lock file — phase start:** Before writing any file, run `Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'` and update `zPostRunValidationArtefacts/.codingagent-lock`:
> - Set `current-phase: Phase 1 — Backend Layers`
> - Add a row: `| Phase 1 — Backend Layers | <timestamp> | IN-PROGRESS | |`

> **Phase 1 gate — run this command immediately after updating the lock file:**
> ```powershell
> Test-Path "zPostRunValidationArtefacts/[App]-[FormName]-Backend.md"
> ```
> - **`False`** → create the skeleton file now using the template below, then proceed to Step 0.
>   Analysis sections (Reference Map, Artefact Detail, C# Artefact Mapping, Raw SQL Decisions) are filled in at Step 0d. File-change rows are appended after each step as you go.
> - **`True`** → already exists from a previous run — verify all sections are present, then proceed.
>
> **Skeleton — create this file now if the result was `False`:**
> ```markdown
> # Backend Analysis — [App] [FormName]
>
> ## Reference Map
>
> *To be completed at Step 0d.*
>
> ## Artefact Detail
>
> *To be completed at Step 0d.*
>
> ## C# Artefact Mapping
>
> *To be completed at Step 0d.*
>
> ## Raw SQL Decisions
>
> *To be completed at Step 0d.*
>
> ## File Changes — Phase 1 Backend
>
> | # | Action | File path (relative to `src/`) | Reason |
> |---|--------|-------------------------------|--------|
> ```

Generate all backend layers in order. Check existing files before creating new ones to avoid duplication.

---

## Step 0 — Artefact Analysis

Before writing any code, perform artefact discovery by reading **the `.frm` file first**, then looking up only the artefact files it actually references.

Source artefact folders (check these based on `[App]` → `[DbName]` / `[schema]` mapping from the inputs instruction):
- MS Access queries: `source/msaccessqry/[App]/`
- Stored procedures: `source/mssql/[DbName]/Procedures/` (converted to C— not migrated to PostgreSQL)
- Functions: `source/mssql/[DbName]/functions/` (converted to C— not migrated to PostgreSQL)
- Triggers: `source/mssql/[DbName]/triggers/` (converted to C— not migrated to PostgreSQL)
- Views: `source/pgsql/[schema]/Views/` (PostgreSQL DDL — lowercase filenames; fallback to `source/mssql/[DbName]/views/` if not found)
- Table definitions: `source/pgsql/[schema]/Tables/` (PostgreSQL DDL — authoritative for entity column names, types, PKs, and FKs; fallback to `source/mssql/[DbName]/Tables/` if not found)

> **Why form-first?** These folders may contain objects for many forms across the app. A single pass through the `.frm` yields an exact reference list — then only the referenced files need to be read.

---

### 0a — Read the `.frm`: Extract All Artefact References

**In a single pass through the `.frm` VBA code**, scan every `Sub` and `Function` body and collect the following reference lists.

#### Named query references (→ `source/msaccessqry/[App]/`)

> **Naming convention:** MS Access named queries begin with `qry` (e.g., `qryProjectCheck`, `qryStaffJobLookup`). Use this prefix as a fast signal when scanning — any bare identifier starting with `qry` in a `RowSource`, `QueryDefs("...")` call, or VBA string literal is a named query reference.

Search for all four patterns and record the **query name** and **enclosing Sub/event** for each hit:

| Pattern | Example | Record |
|---|---|---|
| `QueryDefs("QueryName")` | `MyDB.QueryDefs("qryProjectCheck")` | query name + enclosing Sub |
| `RowSource = "QueryName"` (bare, no SELECT) | `RowSource ="qptWGCostCentres"` | query name + control name |
| `FROM QueryName` inside inline RowSource SQL | `RowSource ="SELECT ... FROM qryManager;"` | query name + control name |
| VBA string literal containing the name | `"qryProjectCheck"` anywhere | query name + enclosing Sub |

Also note the **parameter binding** lines that immediately follow a `QueryDefs(…)` call:
- `myqd.Parameters("[NewProject]") = [Forms]![FrmName]![ControlName]` → parameter name `NewProject` bound to control `JobCode`

#### Stored procedure references (→ `source/mssql/[DbName]/Procedures/`)

> **Naming convention:** SQL Server stored procedures begin with `usp_` (user SPs, e.g., `usp_Delete_Project`, `usp_ChangeProjectCode`) or `sp_` (e.g., `sp_Delete_JC`). When scanning the `qd.sql =` line, any string starting with `usp_` or `sp_` is an SP invocation.

Search for the `CreateQueryDef` block and record the **SP name**, **parameter list**, and **enclosing Sub/event**:

```vba
Set qd = MyDB.CreateQueryDef("tempName")          ' ← start of block
qd.sql = "usp_SpName '" & [Field1] & "', '" & [Field2] & "'"  ' ← SP name + params
DoCmd.OpenQuery "tempName"                         ' ← execution
MyDB.QueryDefs.Delete "tempName"                   ' ← end of block
```

Extract from the `qd.sql =` (or `sqlstr =`) line:
- SP name: the identifier before the first `'` — starts with `usp_` or `sp_` (e.g., `usp_ChangeProjectCode`)
- Parameters: each `& [ControlName] &` fragment in order — these are the positional C# method parameters

#### PostgreSQL view references (→ `source/pgsql/[schema]/Views/`)

Record any view name found in:
- `RecordSource = "vViewName"` or `RecordSource = "SELECT ... FROM vViewName ..."` on the form
- `RowSource = "vViewName"` or `RowSource = "SELECT ... FROM vViewName ..."` on a control
- VBA SQL strings containing `FROM vViewName`

Views typically begin with `v` (e.g., `vstaffjobhours`). Record as **read-only** — they are never written to. View filenames in `source/pgsql/[schema]/Views/` are **lowercase**.

#### SQL Server function references (→ `source/mssql/[DbName]/functions/`)

Record any function name found in:
- `SELECT dbo.fn_FunctionName(...)` inside a `RecordSource` or `RowSource` SQL string
- VBA `db.Execute("SELECT dbo.fn_FunctionName(...)")` calls
- `FROM dbo.fn_FunctionName(...)` in a `RecordSource` SQL string (table-valued function)

Functions typically begin with `fn_` or `uf_`.

#### Trigger awareness — tables written by the form (→ `source/mssql/[DbName]/triggers/`)

Identify every table the form **writes** to:
- `RecordSource = "tableName"` with `AllowEdits = Yes` / `AllowAdditions = Yes` / `AllowDeletions = Yes`
- VBA `DoCmd.RunSQL "INSERT/UPDATE/DELETE ..."` or `CurrentDb.Execute "INSERT/UPDATE/DELETE ..."`
- SP DML bodies (found in step above) that write to a table

For each written table, scan `source/mssql/[DbName]/triggers/` for matching trigger files using these filename patterns:

| Filename pattern | Example | Table extraction | Operation |
|---|---|---|---|
| `DTrig_[Table]` | `DTrig_tblAdditionalCosts.sql` | strip `DTrig_` prefix | DELETE |
| `UITrig_[Table]` | `UITrig_tblStaffJob.sql` | strip `UITrig_` prefix | INSERT + UPDATE |
| `tI_[Table]` | `tI_tlkpProject.sql` | strip `tI_` prefix | INSERT |
| `tU_[Table]` | `tU_WorkGroup.sql` | strip `tU_` prefix | UPDATE |
| `tD_[Table]` | `tD_tblkpProfitCentre.sql` | strip `tD_` prefix | DELETE |
| `[Table]_DTrig` or `[Table]_Dtrig` | `tlkpJobCode_DTrig.sql` | strip `_DTrig`/`_Dtrig` suffix | DELETE |
| `[Table]_UTrig` | `tlkpJobCode_UTrig.sql` | strip `_UTrig` suffix | UPDATE |
| `[Table]_ITrig` | `TimeCodeValid_ITrig.sql` | strip `_ITrig` suffix | INSERT |
| `[XX]_LOG_DTrig/ITrig/UTrig` | `MO_LOG_DTrig.sql` | **table not in filename** — read `ON [dbo].[TableName]` clause | D / I / U |

> **Always confirm** the target table and operation by reading the `ON [dbo].[TableName] FOR INSERT|UPDATE|DELETE` line in the SQL body — the filename is a hint only.

Record each matched trigger as: trigger file name, target table, operation (`INSERT` / `UPDATE` / `DELETE`).

#### BAS module function references (→ `source/bas/[App]/`)

After collecting all inline artefact references, scan every `Sub` and `Function` body in the `.frm` for calls to functions/subs that are **not defined inside the `.frm` itself** (i.e. not a `Private Sub`/`Private Function` in the same file). These are shared VBA utilities living in standard modules.

For each external function call found:

1. Search `source/bas/[App]/[ModuleName].bas` for the matching `Function`/`Sub` definition (case-insensitive name match across all `.bas` files in the folder)
2. Read the full function body
3. Classify using this decision table:

| If the body uses… | Classification | Translation |
|---|---|---|
| `DLookup`, `DCount`, `DMax`, `DMin`, `DSum` | **DB-interacting** | Repository method using `AnyAsync` / `MaxAsync` / `FirstOrDefaultAsync` as appropriate |
| `OpenRecordset`, `db.Execute`, `QueryDefs`, DAO/ADO objects | **DB-interacting** | Repository method — translate query to EF LINQ using the translation table in Step 0b |
| String, date, or numeric operations only | **Pure utility** | Translate inline at call site, or to a `static` helper in `Apha.Common/Helpers/` if reused across forms |

4. Add each DB-interacting function to the **reference map** with type `BAS function (DB)` and its repository method name
5. If the definition is **not found** in any `.bas` file in `source/bas/[App]/`, add a `// TODO: [FunctionName]() — definition not found in source/bas/[App]/. Confirm with team.` comment at every call site in the generated code — do **not** silently omit the call

> **Naming note:** the VBA function name and its actual implementation may contain typos (e.g. `NextpcatID` called as `NextpcatID()` but defined as `NextpactID()`). Match case-insensitively and record the discrepancy in the reference map.

---

#### Build the reference map

After the single `.frm` pass, produce a table like this before reading any artefact file:

| Referenced name | Type | Triggering event / context | Parameters / notes |
|---|---|---|---|
| `qryProjectCheck` | MS Access query | `JobCode_BeforeUpdate` | `NewProject` ← `JobCode` control |
| `usp_ChangeProjectCode` | Stored procedure (parent) | `NewJobCode_BeforeUpdate` | `OldJobCode`, `NewJobCode` |
| `sp_insert_tcv` | Stored procedure (child of `usp_ChangeProjectCode`) | `EXECUTE` inside parent SP | `@OldCode`, `@NewCode` |
| `vStaffJobSummary` | SQL Server view | `RecordSource` on form load | Read-only |
| `fn_CalcBudget` | Scalar function | `RecordSource` SQL | Inline scalar |
| `tr_StaffJob_Insert` | Trigger (INSERT) | Written table: `tblStaffJob` | Convert to `AddAsync` post-logic |

> **Child SP discovery is part of Step 0b, not Step 0a.** The reference map above shows parent SPs only. Child SPs are added to the map as each parent SP file is read in Step 0b — update the map incrementally as you discover them. Mark child SPs with their parent in the "Triggering event / context" column so the call hierarchy is clear.

---

### 0b — Look Up and Parse Each Referenced Artefact

For each entry in the reference map, locate and read its artefact file.

> **LINQ-first rule — applies to every artefact in this step**
>
> The default implementation for **all** MS Access queries, stored procedures, functions, and triggers is **EF Core LINQ**. Use the translation table in this step to convert every SQL construct.
>
> Raw SQL (`FromSqlRaw`) is only permitted when **all three** of the following are true:
> 1. The construct cannot be expressed in EF Core LINQ (e.g. a true table-valued function call, a full-text search predicate, or a deeply recursive CTE)
> 2. The LINQ equivalent would require loading more rows than needed into memory and then filtering in C#
> 3. The raw SQL is parameterised exclusively via `NpgsqlParameter` objects — never string interpolation
>
> If raw SQL is used, you **must** record the decision in the `## Raw SQL Decisions` section of `[App]-[FormName]-Backend.md` (Section 4 in Step 0d) with:
> - The artefact name and repository method
> - The specific LINQ limitation that made raw SQL necessary
> - Confirmation that `NpgsqlParameter` is used for all parameters
>
> **Query optimisation — assess every artefact as you parse it:**
> For each artefact, identify which of the mandatory optimisation rules below apply and record them in the `- **Optimization notes:**` field of the Artefact Detail section. At minimum, check:
> - Does the query load more data than needed? → apply early `.Select()` projection
> - Is this read-only? → confirm `.AsNoTracking()` is on the pipeline
> - Is this an existence check? → use `AnyAsync` not `CountAsync > 0`
> - Is this a multi-row `INSERT` / `UPDATE` / `DELETE`? → use `AddRangeAsync`, `ExecuteUpdateAsync`, `ExecuteDeleteAsync`
> - Does the query join to a collection navigation? → check for N+1; use `.Include()` / `.ThenInclude()` or a projected join
> - Is there mixed-type arithmetic (`decimal × double`, etc.)? → apply the two-step cast pattern
>
> If none of the special optimisation rules are triggered, write: _Standard pipeline — `AsNoTracking().Where().Select()` materialised once._

#### MS Access named queries (`source/msaccessqry/[App]/[QueryName].msaccsql`)

Parse:
- **PARAMETERS clause** — confirms parameter names and types; cross-check against the binding line found in Step 0a
  - `Text (n)` → `string`, `Long Integer` / `Integer` → `int`, `Date/Time` → `DateTime`, `Yes/No` → `bool`
- **SQL statement** — convert to **optimized EF Core LINQ** using the translation table below
- **Query type** determines placement:
  - `SELECT` rows → LINQ query method on repository
  - `SELECT` single value / existence → `ExistsAsync` / `GetByCodeAsync`
  - `ACTION` → repository method or service orchestration
- **Method name** from query name: `qryProjectCheck` → `CheckProjectExistsAsync`

##### Access SQL → Optimized EF LINQ translation table

| Access SQL construct | Optimized EF LINQ | Notes |
|---|---|---|
| `SELECT … FROM T WHERE F = [P]` | `.AsNoTracking().Where(x => x.F == p)` | Always add `AsNoTracking()` on read-only queries |
| `SELECT DISTINCTROW …` | `.Distinct()` before `.Select()` when possible | Apply before projection to reduce rows SQL returns |
| `SELECT TOP n …` | `.Take(n)` chained before `.ToListAsync()` | |
| `SELECT COUNT(*) FROM T WHERE …` | `await q.CountAsync()` | Only when the count value itself is needed |
| `IF (SELECT COUNT(*) …) != 0` | `await q.AnyAsync(…)` | **Always prefer `AnyAsync` over `CountAsync > 0`** (S2971) |
| `SELECT MIN(F)` / `MAX(F)` | `await q.MinAsync(x => x.F)` / `MaxAsync` | |
| `SELECT SUM(F)` | `await q.SumAsync(x => x.F)` | |
| `SELECT AVG(F)` | `await q.AverageAsync(x => x.F)` | |
| `SELECT … WHERE PK = [P]` (single row) | `await q.FirstOrDefaultAsync(x => x.PK == p)` then null-check (S2259) | |
| `SELECT … WHERE PK = [P]` (unique constraint guaranteed) | `await q.SingleOrDefaultAsync(…)` | Use only when a DB constraint ensures uniqueness |
| `INNER JOIN U ON T.K = U.K` | `.Join(_dbContext.Us, t => t.K, u => u.K, (t,u) => …)` — or use navigation property when FK is mapped | Prefer navigation property over manual join |
| `LEFT JOIN U ON T.K = U.K` | `.GroupJoin(…).SelectMany(g => g.DefaultIfEmpty(), (t,u) => …)` | |
| `WHERE F IN (v1, v2)` | `.Where(x => ids.Contains(x.F))` using a pre-built `List<T>` or `HashSet<T>` | Never inline literals |
| `WHERE F NOT IN (…)` | `.Where(x => !ids.Contains(x.F))` | |
| `WHERE F LIKE '%str%'` | `.Where(x => x.F.Contains(str))` | |
| `WHERE F LIKE 'str%'` | `.Where(x => x.F.StartsWith(str))` | |
| `WHERE F LIKE '%str'` | `.Where(x => x.F.EndsWith(str))` | |
| `ORDER BY F ASC` | `.OrderBy(x => x.F)` | |
| `ORDER BY F DESC` | `.OrderByDescending(x => x.F)` | |
| `ORDER BY F1 ASC, F2 DESC` | `.OrderBy(x => x.F1).ThenByDescending(x => x.F2)` | |
| `IIf(cond, a, b)` | Extract to `var val = cond ? a : b;` before `select new` (S3358) | Never nest ternary inside projection |
| `GROUP BY F … HAVING Count(*) > n` | `.GroupBy(x => x.F).Where(g => g.Count() > n).Select(…)` | |
| `WHERE F IS NULL` / `IS NOT NULL` | `.Where(x => x.F == null)` / `.Where(x => x.F != null)` | |
| `WHERE F BETWEEN a AND b` | `.Where(x => x.F >= a && x.F <= b)` | |
| `UNION` (deduplicates) | `.Concat(…).Distinct()` | |
| `UNION ALL` | `.Concat(…)` | No `Distinct()` |
| `EXISTS (SELECT …)` | `await q.AnyAsync(…)` | |
| `NOT EXISTS (SELECT …)` | `!await q.AnyAsync(…)` | |
| Paged results | `.Skip((page-1)*size).Take(size)` — after all filters and ordering | |

##### Computed fields — `[Field1]*[Field2] AS ComputedName` (type mismatch rule)

MS Access queries often contain inline computed columns such as `[NoTests]*[TestPrice] AS TestCost`. When the two operands map to different C# numeric types (e.g. `norequired double precision` → `double` and `unitprice money` → `decimal`), **do not perform the multiplication inside the `IQueryable` projection**.

Reasons:
- C# has no implicit conversion between `double` and `decimal` — it is a compile error.
- Even with an explicit cast, Npgsql's `money` type behaves specially in SQL arithmetic; a CAST-based expression inside EF Core can generate incorrect or failing SQL at runtime.

**Rule: always move mixed-type arithmetic into a post-query LINQ-to-Objects step.**

Use a two-step pattern in the repository method:

```csharp
// Step 1 — IQueryable: fetch the raw typed columns, no arithmetic
var raw = await _dbContext.[ViewEntity]
    .AsNoTracking()
    .Where(x => ...)
    .Select(x => new
    {
        x.FieldA,      // e.g. double (norequired / double precision)
        x.FieldB,      // e.g. decimal (unitprice / money)
        x.OtherField,
    })
    .ToListAsync(cancellationToken);

// Step 2 — LINQ-to-Objects: safe to cast and compute after materialisation
return raw.Select(x => new [ResultView]
{
    FieldA        = x.FieldA,
    FieldB        = x.FieldB,
    ComputedName  = (decimal)x.FieldA * x.FieldB,   // double → decimal cast, then decimal * decimal
    OtherField    = x.OtherField,
}).ToList();
```

> This rule applies to **any** mixed-type arithmetic (`double × decimal`, `float × decimal`, `int × double`, etc.) — not only `norequired × unitprice`. Whenever Access SQL uses `[F1]*[F2]` and the two PostgreSQL column storage types differ in their C# mapping, apply this two-step pattern.
>
> The `[ResultView]` class property for the computed field must be `decimal` (monetary result) and must be marked as a computed/read-only field in the Grid Operations Profile — it is never submitted as part of a write payload.

---

**Mandatory optimisation rules — apply to every generated repository method:**
- **Build the full `IQueryable<T>` pipeline first, materialise last.** Chain all `.Where()`, `.OrderBy()`, `.Select()` before any terminal `await …Async()`. Never call `.ToList()` mid-pipeline and then filter in C#.
- **Project early.** Apply `.Select(x => new TargetType { … })` as early as possible to limit the columns SQL fetches. Never select full entities then discard columns in C#.
- **`AsNoTracking()` on every read-only query.** Add it immediately after the `DbSet` reference. Omit only when the entity will be modified and saved in the same unit of work.
- **`AsNoTrackingWithIdentityResolution()` for read-only joins** that may return duplicate root entities (one-to-many).
- **`AnyAsync` over `CountAsync > 0`** for existence checks (S2971).
- **`FirstOrDefaultAsync` + null-check** for single-row reads (S2259). Never use `.First()` / `.Single()` without `OrDefault` unless a constraint guarantees presence and you explicitly want an exception on violation.
- **Bulk UPDATE via `ExecuteUpdateAsync`** for multi-row `UPDATE` (EF Core 7+). Fall back to load-mutate-save only for single-row updates or when the updated entity must be returned.
- **Bulk DELETE via `ExecuteDeleteAsync`** for multi-row `DELETE` (EF Core 7+). Fall back to load-remove-save only for single-row deletes.
- **`AddRangeAsync` + single `SaveChangesAsync`** for multi-row inserts (S6966). Never loop `Add` + `SaveChanges` per entity.
- **No raw string interpolation in `FromSqlRaw`.** Only use `FromSqlRaw` when LINQ cannot express the query (TVF calls, full-text). Always pass user-controlled values as `NpgsqlParameter` objects (add `using Npgsql;` — the project uses `Npgsql.EntityFrameworkCore.PostgreSQL`).
- **Avoid N+1 queries.** When related data is needed in the same query, use `.Include()` / `.ThenInclude()` rather than lazy-loading or separate per-row queries inside a loop.

#### Stored procedures (`source/mssql/[DbName]/Procedures/[SpName].sql`)

Parse:
- **Parameters** — match `@Param` declarations positionally to the parameter list captured in Step 0a
- **Business guard checks** — `IF (SELECT Count(*) …) RAISERROR` → service-level validation before writes:
  ```csharp
  bool inUse = await _someRepository.AnyRelatedAsync(oldCode);  // S2971: AnyAsync not Count
  if (inUse)
      throw new InvalidOperationException("Cannot delete — related records exist in ...");
  ```
- **DML statements** → optimized EF Core LINQ (apply the same rules from the MS Access translation table; SP-specific rules below):
  - `INSERT INTO T (cols) VALUES (vals)` — single row → construct entity, `_dbContext.T.Add(entity); await _dbContext.SaveChangesAsync();`
  - `INSERT INTO T … SELECT … FROM S WHERE …` — do **not** load rows then re-insert in a loop; use `AddRangeAsync` for the collected entities then a single `SaveChangesAsync` (S6966)
  - `UPDATE T SET F = val WHERE …` — **single row** → `FirstOrDefaultAsync` + mutate + `SaveChangesAsync`; **many rows** → `await _dbContext.T.Where(…).ExecuteUpdateAsync(s => s.SetProperty(x => x.F, val))` (EF Core 7+ bulk update, no round-trip per row)
  - `UPDATE T SET F = CASE F WHEN @old THEN @new ELSE F END WHERE …` (rename pattern) → `ExecuteUpdateAsync` with a conditional `SetProperty`
  - `DELETE FROM T WHERE …` — **single row** → `FirstOrDefaultAsync` + `Remove` + `SaveChangesAsync`; **many rows** → `await _dbContext.T.Where(…).ExecuteDeleteAsync()` (EF Core 7+ bulk delete, no round-trip per row)
  - Add `.AsNoTracking()` to any read-only query inside the SP conversion (guard checks, existence checks)
- **Nested `EXECUTE sp_*` / child stored procedures** — when the parent SP body contains `EXECUTE sp_Name` or `EXEC usp_Name` lines:
  1. **Read the child SP file** `source/mssql/[DbName]/Procedures/[ChildSpName].sql` before writing any code
  2. **Convert the child SP body** to its own repository method (same rules as a parent SP: guards → service validation, DML → LINQ repository calls) and expose it via a `private` or `internal` repository method unless it is reused by another form, in which case make it a full `public` method on the interface
  3. **Call the child method** from the parent repository method in the same position as the original `EXECUTE` statement — do not retain the `// TODO` stub if the child SP source file exists
  4. **Recurse**: after reading each child SP, scan its body for further `EXECUTE` / `EXEC` calls and repeat steps 1–3 until no child SPs remain
  5. Only fall back to `// TODO: implement [sp name]` if the child SP file is genuinely absent from `source/mssql/[DbName]/Procedures/`
- **Transactions** → service-level transaction:
  ```csharp
  await using var tx = await _dbContext.Database.BeginTransactionAsync();
  try { /* repository calls */ await tx.CommitAsync(); }
  catch { await tx.RollbackAsync(); throw; }
  ```
- **Method name** from SP name: `usp_Delete_Project` → `DeleteProjectAsync(string oldCode)`
- **New API endpoint** for each SP-derived service method (e.g., `[HttpDelete("{id}")]`)

#### Child stored procedures (discovered while reading parent SP bodies)

When a parent SP body contains `EXECUTE sp_Name @param1, @param2` or `EXEC usp_Name @param1, @param2`:

1. **Locate and read** `source/mssql/[DbName]/Procedures/[ChildSpName].sql` immediately — do not defer
2. **Record the child SP** in the reference map with its parent noted in the context column
3. **Convert following the same rules as any other SP** (parameters, guards, DML → LINQ, transactions)
4. **Placement of the converted child method:**
   - If the child SP is exclusively a sub-operation of one parent → `private async Task [ChildName]Async(...)` on the same repository class; not added to the repository interface
   - If the child SP could be called independently or is reused → `public` method on the repository interface
5. **Call site**: replace the `EXECUTE` line in the parent's converted repository method with `await [ChildName]Async(oldCode, newCode)` at the same position
6. If the child SP source file is **absent** from `source/mssql/[DbName]/Procedures/`, add `// TODO: implement [ChildSpName] — source file not found` per S1135 and log the gap in the reference map

**Example** — `usp_ChangeProjectCode` calls `sp_insert_tcv`, `sp_insert_tr`, `sp_Delete_tr`, `sp_Delete_tcv`, `sp_Delete_jc`, `sp_Delete_pp`:
```csharp
// In ProjectRepository.cs:
public async Task ChangeProjectCodeAsync(string oldCode, string newCode)
{
    await using var tx = await _dbContext.Database.BeginTransactionAsync();
    try
    {
        // ... INSERT new project row, UPDATE child tables ...
        await InsertTestCapabilityViewsAsync(oldCode, newCode);  // sp_insert_tcv
        await InsertTestRequirementsAsync(oldCode, newCode);     // sp_insert_tr
        // ... more UPDATE statements ...
        await DeleteTestRequirementsAsync(oldCode);              // sp_Delete_tr
        await DeleteTestCapabilityViewsAsync(oldCode);           // sp_Delete_tcv
        await DeleteJobCodesAsync(oldCode);                      // sp_Delete_jc
        await DeleteProjectParentsAsync(oldCode);                // sp_Delete_pp
        await tx.CommitAsync();
    }
    catch { await tx.RollbackAsync(); throw; }
}

private async Task InsertTestCapabilityViewsAsync(string oldCode, string newCode) { /* converted sp_insert_tcv body */ }
private async Task DeleteJobCodesAsync(string oldCode) { /* converted sp_Delete_jc body */ }
// ... etc.
```

---

#### PostgreSQL views (`source/pgsql/[schema]/Views/[viewname].sql`)

> **Fallback:** if the view file is absent from `source/pgsql/[schema]/Views/`, read `source/mssql/[DbName]/views/[ViewName].sql` instead and apply the same parsing rules below.

Parse:
- **Column list** from the `SELECT` — each column becomes a property on the `[ViewName]View` entity
- **Do not convert the view body to LINQ** — the view stays in the database; it is accessed through EF Core as a keyless entity:
  ```csharp
  // In [App]DbContext.cs OnModelCreating:
  modelBuilder.Entity<[ViewName]View>().HasNoKey().ToView("[viewname]"); // lowercase view name as it exists in PostgreSQL
  ```
- Register a `DbSet<[ViewName]View>` in the DbContext
- The repository query method reads directly from the DbSet — no LINQ translation of the view SQL
- PostgreSQL view filenames and names are **lowercase** — the `ToView("...")` argument must use the lowercase name

#### SQL Server functions (`source/mssql/[DbName]/functions/[FunctionName].sql`)

Parse and convert based on function type:

- **Scalar function** (`RETURNS scalar_type`) → C# `private static` helper method on the repository; inline it in the LINQ projection where the function was called:
  ```csharp
  private static decimal CalcBudget(decimal value) => /* converted body */;
  // Used in: .Select(x => new { Budget = CalcBudget(x.Amount) })
  ```
- **Table-valued function** (`RETURNS TABLE`) → LINQ query method returning `IQueryable<T>`; register a keyless entity for the result shape in `OnModelCreating` if it doesn't match an existing entity:
  ```csharp
  modelBuilder.Entity<[FunctionResult]>().HasNoKey();
  // Usage: var result = await _dbContext.Set<[FunctionResult]>().FromSqlRaw("SELECT * FROM fn_x({0})", new NpgsqlParameter("param", value)).ToListAsync();
  // PostgreSQL: no dbo. prefix; function name is lowercase; use NpgsqlParameter (using Npgsql;)
  ```

#### SQL Server triggers (`source/mssql/[DbName]/triggers/[TriggerName].sql`)

Parse:
- **Target table** — confirmed from `ON [dbo].[TableName]` in the body (cross-check against filename hint using patterns above)
- **Trigger type** (`FOR INSERT` / `FOR UPDATE` / `FOR DELETE` / `FOR INSERT, UPDATE`) → determines which existing repository method absorbs the converted logic:
  - `INSERT` → `AddAsync`
  - `UPDATE` → `UpdateAsync`
  - `DELETE` → `DeleteAsync`
  - `INSERT, UPDATE` (combined) → add the same logic to both `AddAsync` and `UpdateAsync`
- **`INSERTED` / `DELETED` virtual tables** → map to the `entity` parameter already available in the repository method
- **Body logic** — convert exactly as a stored procedure: guards → service validation, DML → repository calls
- **Pattern A — trigger only stages inserts to log/audit tables**: add both the entity and the log entry to the context **before** the single `SaveChangesAsync`. EF Core wraps all tracked changes in one implicit DB transaction — no explicit transaction needed:
  ```csharp
  public async Task<[Entity]> AddAsync([Entity] entity)
  {
      _dbContext.[Entities].Add(entity);
      // Converted trigger logic (was: [TriggerName] FOR INSERT) — staged in same unit of work:
      _dbContext.[Logs].Add(MapEntityToLog(entity, "I"));
      await _dbContext.SaveChangesAsync();   // single implicit transaction covers entity + log
      return entity;
  }
  ```
- **Pattern B — trigger reads other tables or performs multi-step DML**: wrap the entire write method in an explicit transaction; any `private async Task HandlePost[Operation]LogicAsync(...)` helper stages changes on the context but **must not call `SaveChangesAsync` itself** — the calling method owns the single save + commit:
  ```csharp
  public async Task<[Entity]> AddAsync([Entity] entity)
  {
      await using var tx = await _dbContext.Database.BeginTransactionAsync();
      try
      {
          _dbContext.[Entities].Add(entity);
          await _dbContext.SaveChangesAsync();
          // Converted trigger logic (was: [TriggerName] FOR INSERT):
          await HandlePostInsertLogicAsync(entity);   // stages only — no SaveChangesAsync inside
          await tx.CommitAsync();
      }
      catch { await tx.RollbackAsync(); throw; }
      return entity;
  }
  ```
- **Rule**: `HandlePost[Operation]LogicAsync` helpers must **never** call `SaveChangesAsync` — they only stage changes on the context. The calling write method owns the transaction boundary and the single `SaveChangesAsync`

---

### 0c — Cross-Reference Summary

After Steps 0a and 0b, confirm the completed map before writing any code:

| Artefact | Form usage | Generated C# artefact |
|---|---|---|
| `qryXxx.msaccsql` | e.g., `RowSource` on `cboXxx` | `GetXxxAsync(…)` on `IXxxRepository` |
| `usp_Xxx.sql` (parent SP) | e.g., Delete button `OnClick` | `DeleteXxxAsync(…)` on `IXxxService` + endpoint |
| `sp_ChildXxx.sql` (child SP — `EXECUTE` inside parent) | `EXECUTE sp_ChildXxx` inside `usp_Xxx` body | `private async Task ChildXxxAsync(…)` called from parent method |
| `vXxx` SQL view | `RecordSource` on form | Keyless EF entity `XxxView` → `ToView("vXxx")` |
| `fn_Xxx` function | Inline in SQL / VBA | C# helper method or `FromSqlRaw` TVF call |
| `tr_Tbl_Operation` trigger | Written table `tbl` | Logic appended inside `AddAsync` / `UpdateAsync` / `DeleteAsync` |

Use this map in Steps 1–9 to ensure every referenced artefact is surfaced at the correct layer.

---

### 0d — Publish Artefact Analysis Output

After the cross-reference summary is complete and **before writing any code**, publish the file:

> **GATE CHECK — run this command first and confirm the file does NOT yet exist:**
> ```powershell
> Test-Path "zPostRunValidationArtefacts/[App]-[FormName]-Backend.md"
> ```
> - If the result is `False` → proceed to create the file below.
> - If the result is `True` → the file already exists from a previous run; open it, verify it is complete, then skip creation and proceed to Step 1.
> - **DO NOT write any `.cs` file, `DbSet`, entity, or mapper until this gate file exists on disk.** If you find yourself writing code before this file exists, stop immediately, create the file, then resume coding.

**`zPostRunValidationArtefacts/[App]-[FormName]-Backend.md`**

The file must contain three sections:

#### Section 1 — Reference Map

The full reference map table produced in Step 0a (one row per artefact, including all BAS functions):

```markdown
| Referenced name | Type | Triggering event / context | Parameters / notes |
|---|---|---|---|
| `qryXxx` | MS Access query | `JobCode_BeforeUpdate` | `NewProject` ← `JobCode` |
| `usp_Xxx` | Stored procedure (parent) | `Button_Click` | `OldJobCode`, `NewJobCode` |
|   `sp_ChildXxx` | └ Stored procedure (child) | `EXECUTE` inside `usp_Xxx` | `@OldCode`, `@NewCode` |
| `vXxx` | SQL Server view | `RecordSource` | Read-only |
| `fn_Xxx` | Scalar function | `RecordSource` SQL | Inline scalar |
| `NextpactID` | BAS function (DB) | `Form_BeforeInsert` | `DLookup` on `tblWGEmployee` |
| `tr_Tbl_Op` | Trigger (INSERT) | Written table: `tbl` | Appended to `AddAsync` |
```

> Indent child SP rows by two leading spaces to show nesting visually (as in the example above).

#### Section 2 — Artefact Detail

For each row in the reference map, one sub-section:

```markdown
### `qryXxx` — MS Access query
- **File:** `source/msaccessqry/[App]/qryXxx.msaccsql`
- **Type:** SELECT / ACTION
- **Parameters:** `[ParamName]` (string) ← `ControlName`
- **SQL (original):** (paste the raw Access SQL)
- **Implementation:** LINQ — `GetXxxAsync(string paramName)` on `IXxxRepository` using `.AsNoTracking().Where(...)` *(or: Raw SQL — `FromSqlRaw` — reason: [state the specific LINQ limitation])*
- **Optimization notes:** Early `.Select()` projection applied — 3 of 12 columns needed; `.AsNoTracking()` on read-only pipeline *(or: Standard pipeline — `AsNoTracking().Where().Select()` materialised once.)*
- **Translation:** `GetXxxAsync(string paramName)` on `IXxxRepository` — EF LINQ pattern used

### `usp_Xxx` — Stored procedure (parent)
- **File:** `source/mssql/[DbName]/Procedures/usp_Xxx.sql`
- **Parameters:** `@OldJobCode`, `@NewJobCode`
- **Child SPs called:** `sp_ChildXxx` (see nested section below)
- **Implementation:** LINQ — `DeleteXxxAsync(string oldCode, string newCode)` on `IXxxService` *(or: Raw SQL — reason: [state the specific LINQ limitation])*
- **Optimization notes:** Multi-row delete — `ExecuteDeleteAsync` used; no entity load required *(or: Standard pipeline — `AsNoTracking().Where().Select()` materialised once.)*
- **Translation:** `DeleteXxxAsync(string oldCode, string newCode)` on `IXxxService`

#### `sp_ChildXxx` — Stored procedure (child of `usp_Xxx`)
- **File:** `source/mssql/[DbName]/Procedures/sp_ChildXxx.sql`
- **Parameters:** `@OldCode`, `@NewCode`
- **Implementation:** LINQ — `private async Task ChildXxxAsync(...)` *(or: Raw SQL — reason: [state the specific LINQ limitation])*
- **Optimization notes:** Standard pipeline — `AsNoTracking().Where().Select()` materialised once.
- **Translation:** `private async Task ChildXxxAsync(...)` called from parent
```

Apply the same sub-section pattern for views, functions, triggers, and BAS functions. For every sub-section, always include the `- **Implementation:**` line — state `LINQ` with the method pattern used, or `Raw SQL` with the reason.

#### Section 3 — C# Artefact Mapping

The completed cross-reference table from Step 0c verbatim.

#### Section 4 — Raw SQL Decisions

If **every** artefact was converted to LINQ, write:

```markdown
## Raw SQL Decisions

No raw SQL used. All artefacts converted to EF Core LINQ.
```

If any artefact required raw SQL, write one row per decision:

```markdown
## Raw SQL Decisions

| Artefact | Repository method | LINQ limitation | Parameters via NpgsqlParameter? |
|---|---|---|---|
| `fn_CalcRates` (TVF) | `GetRatesAsync` | Table-valued function — EF Core cannot call a TVF returning a result set without `FromSqlRaw` | Yes — `new NpgsqlParameter("fpsyear", year)` |
| `qryComplexRecursive` | `GetHierarchyAsync` | Recursive CTE — no EF Core LINQ equivalent; would require loading the full table and recursing in C# | Yes — `new NpgsqlParameter("rootId", id)` |
```

Rules:
- Every row in this table must have been flagged with `Raw SQL` in its Section 2 sub-section — the two records must be consistent
- `Parameters via NpgsqlParameter?` must always be `Yes` — if it cannot be `Yes`, the raw SQL must be redesigned
- Pure SQL Server views mapped via `ToView()` are **not** raw SQL — do not include them here

---



## Step 1 — Apha.Common: Shared Request/Response Contracts

**Location:** `Apha.Common/Contracts/[App]/`

Create one `*Req.cs` (input) and one `*Res.cs` (output) contract per entity or data operation identified from the `.frm` file's `RecordSource` and `RowSource` properties.

**Pattern:**
```csharp
namespace Apha.Common.Contracts.[App]
{
    public class [EntityName]Res
    {
        public [Type] [PropertyName] { get; set; }
        // ... all fields matching the MS Access RecordSource/RowSource columns
    }
}
```

Rules:
- Use the **exact field names** from the MS Access table/query columns (PascalCase)
- `*Req.cs` — fields submitted by the form (ControlSource-bound writable fields)
- `*Res.cs` — fields returned for display (all columns from RecordSource)
- Nullable types (`?`) for optional/computed fields
- No business logic in contracts
- Reference existing contracts in `Apha.Common/Contracts/[App]/` to avoid duplication
- **Lookup / dropdown endpoints require their own `*Res.cs` contract** — do **not** reuse a CRUD entity's `*Res`. A lookup entity (e.g. `TestOrProduct` with `ItemCode`/`ItemDescription`) is semantically unrelated to the form's primary CRUD entity (`TestReqMt` with `TestCode`/`JobCode`). Create a dedicated `[LookupEntity]Res.cs` whose properties exactly match the lookup entity's columns. The same lookup `Res` type must be used consistently in the repository, service, controller, infrastructure client, and AutoMapper profile — never substitute the CRUD `Res` type.

> **→ Backend.md:** Append a row to `## File Changes` for each file created in this step.

---

## Step 2 — Apha.[App].Core: Entity Classes

**Location:** `Apha.[App].Core/Entities/`

Create one entity class per database table referenced in the `.frm` file's `RecordSource`, `RowSource`, or VBA queries.

**Pattern:**
```csharp
namespace Apha.[App].Core.Entities
{
    public partial class [EntityName]
    {
        public [KeyType] [KeyName] { get; set; } = null!;
        // ... all table columns
    }
}
```

Rules:
- Use `partial class` to allow EF Core scaffolding compatibility
- Use `= null!` for required string navigations
- For **MS Access SELECT queries** that are converted to LINQ → create a `[Name]View.cs` entity representing the result shape
- For **SQL Server views** identified in Step 0a → create a `[ViewName]View.cs` entity mapped to the view (NOT a LINQ conversion); the class is decorated with `[Keyless]` or configured via `HasNoKey()` in `OnModelCreating`
- For **table-valued function** result shapes that don't match an existing entity → create a separate keyless entity class
- Do **not** add data annotations — those live in the DbContext `OnModelCreating`
- Check `Apha.[App].Core/Entities/` for existing entities before creating new ones

> **→ Backend.md:** Append a row to `## File Changes` for each entity file created in this step.

---

## Step 3 — Apha.[App].Core: Repository Interfaces

**Location:** `Apha.[App].Core/Interfaces/`

Create one repository interface per entity/aggregate root.

**Pattern:**
```csharp
using Apha.[App].Core.Entities;
using Apha.[App].Core.Pagination;

namespace Apha.[App].Core.Interfaces
{
    public interface I[EntityName]Repository
    {
        Task<PagedData<[EntityView]>> GetAll[EntityName]Async(PaginationParameters<string> query);
        Task<[Entity]?> GetByIdAsync([KeyType] id);
        Task<[Entity]> AddAsync([Entity] entity);
        Task<[Entity]> UpdateAsync([Entity] entity);
        Task<bool> DeleteAsync([KeyType] id);
        // Add lookup/filter methods as needed from VBA/macros
    }
}
```

Rules:
- Return `PagedData<[View]>` for list queries (supports pagination)
- Return `[Entity]?` (nullable) for single-record GET
- Use existing `IStaffJobRepository` as the canonical reference
- Add extra query methods matching `.frm` filter logic and VBA-triggered lookups
- Add a method for every `.msaccsql` query identified in Step 0a that reads data
- Add a method for every DML operation extracted from `.sql` stored procedures in Step 0b

> **→ Backend.md:** Append a row to `## File Changes` for each interface file created in this step.

---

## Step 4 — Apha.[App].Application: DTOs

**Location:** `Apha.[App].Application/Dtos/`

Create one DTO per entity used in the service layer.

**Pattern:**
```csharp
namespace Apha.[App].Application.Dtos
{
    public class [EntityName]Dto
    {
        public [KeyType] [KeyName] { get; set; }
        // ... all service-layer fields matching entity
    }
}
```

Rules:
- DTOs are **flat** — no navigation properties
- Match the entity shape but remove EF-specific concerns
- Create a separate `[Name]ViewDto` for paginated list views (matching `[Name]View` entity)
- Reference `Apha.[App].Application/Dtos/` for existing DTOs
- **Each lookup / dropdown entity requires its own `[LookupEntity]Dto`** — separate from the primary CRUD entity's DTO. Verify the Dto properties match the lookup entity's column names (e.g. `ItemCode`, `ItemDescription` for `TestOrProduct`) — never reuse the CRUD Dto (e.g. `TestReqMtDto`) for a lookup endpoint.

> **→ Backend.md:** Append a row to `## File Changes` for each DTO file created in this step.

---

## Step 5 — Apha.[App].Application: Service Interface

**Location:** `Apha.[App].Application/Interfaces/`

**Pattern:**
```csharp
using Apha.[App].Application.Dtos;
using Apha.[App].Application.Pagination;

namespace Apha.[App].Application.Interfaces
{
    public interface I[FormName]Service
    {
        Task<PaginatedResult<[EntityView]Dto>> GetAll[FormName]Async(QueryParameters<string> query);
        Task<[Entity]Dto?> GetByIdAsync([KeyType] id);
        Task<[Entity]Dto> AddAsync([Entity]Dto dto);
        Task<[Entity]Dto> UpdateAsync([Entity]Dto dto);
        Task<bool> DeleteAsync([KeyType] id);
        // Additional methods from OnLoad, OnCurrent, AfterUpdate events
    }
}
```

> **→ Backend.md:** Append a row to `## File Changes` for each interface file created in this step.

---

## Step 6 — Apha.[App].Application: Service Implementation

**Location:** `Apha.[App].Application/Services/`

**Pattern:**
```csharp
using Apha.[App].Application.Dtos;
using Apha.[App].Application.Interfaces;
using Apha.[App].Application.Pagination;
using Apha.[App].Core.Entities;
using Apha.[App].Core.Interfaces;
using Apha.[App].Core.Pagination;
using AutoMapper;

namespace Apha.[App].Application.Services
{
    public class [FormName]Service : I[FormName]Service
    {
        private readonly I[EntityName]Repository _repository;
        private readonly IMapper _mapper;

        public [FormName]Service(I[EntityName]Repository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<[EntityView]Dto>> GetAll[FormName]Async(QueryParameters<string> query)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetAll[EntityName]Async(filter);
            return _mapper.Map<PaginatedResult<[EntityView]Dto>>(result);
        }
        // ... implement all interface methods
    }
}
```

Rules:
- **LAYER BOUNDARY — STRICTLY ENFORCED:** Services call **only** repository interfaces — never `DbContext`, `DbSet`, or any EF Core type directly. The sole exception is `_dbContext.Database.BeginTransactionAsync()` for SP-derived multi-step operations that span multiple repositories (see Step 0b). Any direct `_dbContext.[Entity].Where(...)` inside a service class is a layer violation — move it to a repository method.
- All MS Access VBA `BeforeUpdate`, `ValidationRule`, and macro business rules go here
- All business guard checks extracted from SQL stored procedures (Step 0b) go here — validate before writing
- Multi-step SP operations that span several tables are orchestrated here using individual repository calls wrapped in a transaction
- Services do **not** know about HTTP or contracts
- Use `_mapper.Map<>()` for all entity ↔ DTO conversions
- Extract all VBA business logic here — validation rules, conditional defaults, RecordSource transformations, macro actions

**Sonar compliance:**
- **S2933** — all constructor-injected fields (`_repository`, `_mapper`) must be `private readonly`
- **S4457** — validate non-nullable parameters with `ArgumentNullException.ThrowIfNull(dto)` **before** the first `await` in every public method
- **S3776** — if translating a VBA sub with more than ~4 branches, extract logic into focused `private` helper methods to keep cognitive complexity ≤ 15
- **S6966** — never call `await` inside a `foreach`; accumulate entities then pass to a batch repository method
- **S107** — if a converted SP has more than 7 parameters, wrap them in a typed request DTO

> **→ Backend.md:** Append a row to `## File Changes` for each service file created in this step.

---

## Step 7 — Apha.[App].DataAccess: Repository Implementation

**Location:** `Apha.[App].DataAccess/Repositories/`

**Pattern:**
```csharp
using Apha.[App].Core.Entities;
using Apha.[App].Core.Interfaces;
using Apha.[App].Core.Pagination;
using Apha.[App].DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.[App].DataAccess.Repositories
{
    public class [EntityName]Repository : BaseRepository, I[EntityName]Repository
    {
        private readonly [App]DbContext _dbContext;

        public [EntityName]Repository([App]DbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedData<[EntityView]>> GetAll[EntityName]Async(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.[EntityViews]
                .Where(/* MS Access RecordSource WHERE clause */)
                .Select(e => new [EntityView]
                {
                    // Map columns from MS Access RecordSource query
                });

            baseQuery = (IQueryable<[EntityView]>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }
        // ... other methods
    }
}
```

Rules:
- Convert MS Access `RecordSource` SQL and `RowSource` queries to **optimized EF Core LINQ** — apply the translation table and mandatory optimisation rules from Step 0b to every generated method
- Convert every `.msaccsql` query identified in Step 0a to a LINQ method on this repository
- Convert every stored procedure DML operation identified in Step 0b to a LINQ method on this or a related repository
- **SQL Server views (Step 0b):** do **not** convert the view body to LINQ; map `DbSet<[ViewName]View>` with `HasNoKey().ToView("[ViewName]")` in `OnModelCreating` and query the DbSet directly using `.AsNoTracking()` — read-only only
- **SQL Server scalar functions (Step 0b):** convert to a `private static` C# helper method and inline it in the LINQ projection
- **PostgreSQL table-valued functions (Step 0b):** use `FromSqlRaw("SELECT * FROM fn_x({0})", new NpgsqlParameter("param", value))` with a keyless entity; always pass values as `NpgsqlParameter` objects (add `using Npgsql;`) — never string-interpolated SQL. PostgreSQL function names are lowercase; there is no `dbo.` prefix (use the `public.` schema only when disambiguation is required)
- **SQL Server triggers (Step 0b):** use **Pattern A** (stage entity + log together before the single `SaveChangesAsync` — EF Core implicit transaction) for log-only triggers; use **Pattern B** (explicit `BeginTransactionAsync` … `CommitAsync` wrapping the entire method) when the trigger reads other tables or executes multi-step DML; `HandlePost[Operation]LogicAsync` helpers must **never** call `SaveChangesAsync` — the calling write method owns the single transaction boundary
- Use LINQ `join`, `where`, `select new` — **no raw SQL** unless the logic genuinely cannot be expressed in LINQ, in which case use `FromSqlRaw` with `NpgsqlParameter` objects only (never string-interpolated SQL)
- `DISTINCTROW` in Access SQL → `.Distinct()` before `.Select()` in LINQ when possible
- `PARAMETERS` clause values → typed method parameters
- Filtering: compose all `.Where()` predicates before any terminal `Async` call; implement column filters matching the grid's `IsFilterable` properties
- Sorting: **`ApplySorting` is NOT on `BaseRepository`** — each repository must implement its own private sorting methods. Use the `AnimalRepository` switch-based pattern:
  ```csharp
  private static IQueryable ApplySorting(IQueryable<[EntityView]> query, string? sortBy, bool descending)
  {
      if (string.IsNullOrEmpty(sortBy)) return query;
      return ApplySortingByProperty(query, sortBy.ToLower(), descending);
  }

  private static IQueryable ApplySortingByProperty(IQueryable<[EntityView]> query, string property, bool descending)
  {
      return property switch
      {
          "fieldone" => ApplyOrder(query, i => i.FieldOne, descending),
          "fieldtwo" => ApplyOrder(query, i => i.FieldTwo, descending),
          _ => query
      };
  }

  private static IQueryable ApplyOrder<T>(IQueryable<[EntityView]> query, Expression<Func<[EntityView], T>> keySelector, bool descending)
      => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
  ```
  Add `using System.Linq.Expressions;` to the repository file. The switch cases must cover every property marked `IsSortable` in the DataGrid column definitions. Calling code: `baseQuery = (IQueryable<[EntityView]>)ApplySorting(baseQuery, query.SortBy, query.Descending);`
- Paging: use `base.ApplyPaging()` from `BaseRepository`; `.Skip().Take()` must come after all filters and ordering in the pipeline
- Register new `DbSet<>` properties in `[App]DbContext.cs` for any new entities (including view and TVF entities)
- **Do NOT add inline `modelBuilder.Entity<T>(entity => { … })` blocks to `OnModelCreating`.** Instead, create a dedicated `[Entity]Map.cs` file in `Apha.[App].DataAccess/Data/` per entity, implementing `IEntityTypeConfiguration<T>`, then register it in `OnModelCreating` with a single `modelBuilder.ApplyConfiguration(...)` call. This keeps `OnModelCreating` clean regardless of how many entities the context manages. See Step 7a below.
- **`HasColumnName` values must always be lowercase** — the PostgreSQL database stores all column names in lowercase; passing a mixed-case or PascalCase name will cause EF Core to generate a column reference that does not match the actual column, resulting in a runtime error. Example: `entity.Property(e => e.ParentProject).HasColumnName("parentproject")` — never `HasColumnName("ParentProject")`
- **Before writing any `entity.ToTable(…)` or `entity.Property(…).HasColumnName(…)` configuration, cross-reference `dbscript/schemas/[app_schema]/04views/vtbl*.sql`** — Access table names (e.g. `tblTestRequ_TM`) rarely match the actual PostgreSQL table name after migration. The `vtbl*` views expose the real underlying table with column aliases that match the Access field names; the `FROM` clause of the view body reveals the true PostgreSQL table name and the `SELECT` column aliases reveal the real column names. For example, `fps.vtbltestrequ_tm` (`SELECT buyer AS jobcode, norequired AS notests, unitprice AS testprice FROM fps.tlkptestreqmt`) shows the real table is `tlkptestreqmt` with columns `buyer`, `norequired` (type `double precision`), and `unitprice` — not the Access names. Always check this view before scaffolding. If the column has a different storage type from the C# property type (e.g. `double precision` vs `int?`), apply `.HasConversion<double>()` on the property configuration.

**Mandatory optimisation rules — apply to every generated method:**
- **`AsNoTracking()` on every read-only query.** Add immediately after the `DbSet` reference. Omit only when the entity will be modified and saved in the same unit of work.
- **`AsNoTrackingWithIdentityResolution()` for read-only joins** that may return duplicate root entities.
- **Pipeline first, materialise last.** Chain all `.Where()`, `.OrderBy()`, `.Select()` before the terminal `await …Async()`. Never `.ToList()` mid-pipeline then filter in C#.
- **Project early** with `.Select()` to limit SQL column selection. Never fetch full entities only to discard columns in C#.
- **`AnyAsync` over `CountAsync > 0`** for existence (S2971).
- **`ExecuteUpdateAsync`** for multi-row `UPDATE` (EF Core 7+); `ExecuteDeleteAsync` for multi-row `DELETE` (EF Core 7+).
- **`AddRangeAsync` + single `SaveChangesAsync`** for multi-row inserts; never loop `Add` + `SaveChanges` per entity (S6966).
- **Null-check `FirstOrDefaultAsync` results** before property access (S2259).
- **No nested ternary in `select new`** — extract to a local variable (S3358).
- **`NpgsqlParameter` objects in `FromSqlRaw`** (`using Npgsql;`) — never string-interpolate user-controlled values.
- **`.Include()` / `.ThenInclude()`** when related data is needed — never lazy-load inside a loop.

**Sonar compliance:**
- **S2933** — `_dbContext` must be `private readonly`
- **S2971** — guard checks for existence **must** use `await _dbContext.T.AnyAsync(…)` — never `CountAsync() > 0` or `Count() != 0`
- **S2259** — always null-check the result of `FirstOrDefaultAsync` before accessing any property on it
- **S6966** — never `await` inside a `foreach`; batch inserts/updates must use `AddRangeAsync` + single `SaveChangesAsync` after the loop
- **S3358** — do not nest ternary expressions in `select new` projections; use a local variable or helper method instead

> **→ Backend.md:** Append rows to `## File Changes` for the repository file and any updated `DbContext` or `*Map.cs` files in this step.

---

## Step 8 — Apha.[App].Api: API Controller

**Location:** `Apha.[App].Api/Controllers/`

**Pattern:**
```csharp
using Apha.Common.Contracts;
using Apha.Common.Contracts.[App];
using Apha.[App].Application.Dtos;
using Apha.[App].Application.Interfaces;
using Apha.[App].Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.[App].Api.Controllers
{
    /// <summary>
    /// API controller for [FormName] operations.
    /// </summary>
    [Authorize(Roles = "API-[App]User,API-[App]Admin")]
    [ApiController]
    [Route("api/[formname]")]
    public class [FormName]Controller : ControllerBase
    {
        private readonly I[FormName]Service _service;
        private readonly IMapper _mapper;

        public [FormName]Controller(I[FormName]Service service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Retrieves a paginated list of [FormName] records.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll[FormName]Async([FromQuery] PaginationReq<string> query)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetAll[FormName]Async(filter);
            return Ok(_mapper.Map<PaginationRes<[EntityView]Res>>(result));
        }

        /// <summary>Retrieves a [FormName] record by ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([KeyType] id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result is null)
                throw new KeyNotFoundException($"[FormName] with ID {id} not found.");
            return Ok(_mapper.Map<[Entity]Res>(result));
        }

        /// <summary>Creates a new [FormName] record.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] [Entity]Req request)
        {
            var dto = _mapper.Map<[Entity]Dto>(request);
            var result = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.[KeyName] }, _mapper.Map<[Entity]Res>(result));
        }

        /// <summary>Updates an existing [FormName] record.</summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([KeyType] id, [FromBody] [Entity]Req request)
        {
            var dto = _mapper.Map<[Entity]Dto>(request);
            dto.[KeyName] = id;
            var result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<[Entity]Res>(result));
        }

        /// <summary>Deletes a [FormName] record.</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([KeyType] id)
        {
            var deleted = await _service.DeleteAsync(id);
            return Ok(deleted);
        }

        // Add extra endpoints for each VBA event/macro that performs data operations:
        // e.g., [HttpGet("lookup")], [HttpGet("validate")], [HttpPost("calculate")]
        // Add endpoints for SP-derived service methods identified in Step 0b:
        // e.g., [HttpPost("{id}/change-code")], [HttpDelete("{id}")]
    }
}
```

Rules:
- **LAYER BOUNDARY — STRICTLY ENFORCED:** API controllers call **only** `I[FormName]Service` — never repository interfaces (`I[Entity]Repository`), `DbContext`, or any data-access type directly. The dependency graph must be: `Controller → IService → IRepository → DbContext`. Injecting a repository into an API controller is a clean-architecture violation that bypasses the application layer's business-rules and mapping responsibilities.
- One controller per form, matching the form's functional scope
- **`using Apha.Common.Contracts;` is required** whenever the controller uses `PaginationReq<TFilter>` (parameter) **or** `PaginationRes<T>` (return type mapping) — both live in `Apha.Common.Contracts`, not in `Apha.Common.Contracts.[App]`. Missing this using causes `CS0246` at build time. Always include it alongside `using Apha.Common.Contracts.[App];`. The code template at the top of this step already shows both `using` lines — do not omit `using Apha.Common.Contracts;` even when the controller only uses `PaginationRes<T>` and not `PaginationReq<T>`.
- Use `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]` on every action
- Throw `KeyNotFoundException` for not-found — the `ExceptionMiddleware` handles 404 mapping
- Add XML `<summary>` doc comments on every public action
- Controllers call **only** service interfaces — never repositories or DbContext
- Map MS Access form-level events that trigger data operations to separate `[HttpGet]` or `[HttpPost]` endpoints
- Add a dedicated `[HttpPost]` or `[HttpDelete]` endpoint for every SP-derived service method identified in Step 0b

**Sonar compliance:**
- **S2933** — `_service` and `_mapper` must be `private readonly`
- **S2325** — every `public` action must have a `/// <summary>` XML doc comment
- **S1172** — every action parameter must be used; remove any that come from VBA event signatures that have no C# equivalent
- **S1135** — `// TODO` stubs for unavailable sub-SP implementations must remain visible to Sonar; do **not** suppress them

> **→ Backend.md:** Append a row to `## File Changes` for the controller file created in this step.

---

## Step 9 — API-Layer AutoMapper Profiles and DI Registration

## Step 7a — Apha.[App].DataAccess: Entity Type Configuration Map Files

**Location:** `Apha.[App].DataAccess/Data/` — same folder as `[App]DbContext.cs`

Create one `[Entity]Map.cs` file per entity and register it with `ApplyConfiguration` in `OnModelCreating`. This replaces all inline `modelBuilder.Entity<T>(entity => { … })` blocks.

**Pattern A — entity with `HasQueryFilter` (captures `IFpsRequestContext`):**
```csharp
using Apha.[App].Core.Entities;
using Apha.[App].Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.[App].DataAccess.Data
{
    public class [Entity]Map : IEntityTypeConfiguration<[Entity]>
    {
        private readonly IFpsRequestContext _fpsYearContext;

        public [Entity]Map(IFpsRequestContext fpsYearContext)
        {
            _fpsYearContext = fpsYearContext;
        }

        public void Configure(EntityTypeBuilder<[Entity]> entity)
        {
            entity.HasKey(e => new { e.[Key1], e.[Key2], e.FpsYear }).HasName("pk_...");

            entity.ToTable("[tablename]", "[schema]");

            entity.Property(e => e.[Property])
                .HasColumnType("citext")
                .HasColumnName("[columnname]");

            // ... all other property configurations

            entity.HasQueryFilter(e => e.FpsYear == _fpsYearContext.FpsYear);
        }
    }
}
```

**Pattern B — entity without `HasQueryFilter` (static lookups, keyless views, TVF result shapes):**
```csharp
public class [Entity]Map : IEntityTypeConfiguration<[Entity]>
{
    public void Configure(EntityTypeBuilder<[Entity]> entity)
    {
        entity.HasNoKey();
        entity.ToView("[viewname]", "[schema]");
        entity.Property(e => e.[Property]).HasColumnName("[columnname]");
        // ... all other property configurations
    }
}
```

**Registration in `[App]DbContext.cs` — one line per entity replaces the entire inline block:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // existing entities
    modelBuilder.ApplyConfiguration(new ExistingEntityMap(_fPSYearContext));

    // new entity (Pattern A — needs year context for filter):
    modelBuilder.ApplyConfiguration(new [Entity]Map(_fPSYearContext));

    // new entity (Pattern B — no filter):
    modelBuilder.ApplyConfiguration(new [Entity]Map());
}
```

Rules:
- One `[Entity]Map.cs` file per entity — never combine multiple entity configurations in a single file
- The namespace must match the `[App]DbContext` namespace so the map is in the same logical assembly folder
- Receive `IFpsRequestContext` via constructor **only** when `HasQueryFilter` is needed — do not inject it if the entity has no year filter
- All column-name rules still apply inside the map: `HasColumnName` values must be lowercase, `HasColumnType("citext")`, `HasColumnType("money")`, etc.
- **`IFpsRequestContext` field must be `private readonly`** (S2933)
- Do **not** call `SaveChangesAsync` or any repository from within a map — configuration classes are pure EF metadata

---

## Step 9 — API-Layer AutoMapper Profiles and DI Registration

### 9a — `Apha.[App].Application/Mappings/EntityMapper.cs`

Add new mappings for the new entities/DTOs:
```csharp
CreateMap<[Entity], [Entity]Dto>().ReverseMap();
CreateMap<[EntityView], [EntityView]Dto>().ReverseMap();
```

### 9b — `Apha.[App].Api/Mappings/RequestMapper.cs`

Add new mappings for contract ↔ DTO:
```csharp
CreateMap<[Entity]Dto, [Entity]Req>().ReverseMap();
CreateMap<[Entity]Dto, [Entity]Res>().ReverseMap();
CreateMap<[EntityView]Dto, [EntityView]Res>().ReverseMap();
```

### 9c — `Apha.[App].Api/Extensions/ServiceCollectionExtension.cs`

In `AddServices()`:
```csharp
services.AddScoped<I[FormName]Service, [FormName]Service>();
```

In `AddRepositories()`:
```csharp
services.AddScoped<I[EntityName]Repository, [EntityName]Repository>();
```

> **→ Backend.md:** Append rows to `## File Changes` for each mapper profile and DI file modified in this step.

---

### Verify Phase 1 File Changes

> **Verify `[App]-[FormName]-Backend.md` is complete** — open the file and confirm all five sections are fully populated:
> 1. `## Reference Map` — full artefact table from Step 0a.
> 2. `## Artefact Detail` — one sub-section per artefact (filled in at Step 0d).
> 3. `## C# Artefact Mapping` — cross-reference table from Step 0c.
> 4. `## Raw SQL Decisions` — justification table, or "No raw SQL used" sentinel.
> 5. `## File Changes — Phase 1 Backend` — a row exists for every file created or modified during Steps 1–9.
>
> If any row is missing from section 5, append it now. File changes are written progressively at each step — this is a completeness check only.
>
> **Rules for each row:**
> - **Action**: `CREATED` | `MODIFIED` | `REMOVED`
> - For `MODIFIED` files, the **Reason** must state what was added or changed specifically (e.g. "Added `DbSet<WGEmployee>` and `HasNoKey().ToView()` for `WGEmployeeView`")
> - Do not omit modified files (`DbContext`, mapper profiles, DI registrations)
> - Files in `Apha.Common` that were only read (not changed) must **not** appear in this table

---

> **Lock file — phase end:** Run `Get-Date -Format 'yyyy-MM-ddTHH:mm:ss'` and update `zPostRunValidationArtefacts/.codingagent-lock`:
> - Update the `Phase 1 — Backend Layers` row: set `Status = COMPLETED` and fill in the `End` timestamp.
