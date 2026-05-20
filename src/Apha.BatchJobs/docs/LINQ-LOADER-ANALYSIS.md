# LINQ Loader Implementation Analysis
## Cloud Migration Strategy Reference

**Analysis Date:** May 19, 2026  
**Scope:** MABArchive LINQ loader patterns, validation hooks, SQL vs LINQ comparison  
**Location:** `src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/`

---

## 1. BASE CLASS PATTERN SUMMARY

### Inheritance Hierarchy

```
IMabArchiveLoader (interface)
    ↓
MabArchiveLoaderBase (abstract)
    ├── MabArchiveSqlLoaderBase (abstract) ✅ **Active (production)**
    │   └── SQL implementations (24 loaders)
    │
    └── MabArchiveDotNetLoaderBase (abstract) ⚠️ **Available (configurable)**
        └── LINQ implementations (24 loaders)
```

### Base Class Design Pattern

**File:** [MabArchiveLoaders.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/MabArchiveLoaders.cs#L8-L40)

```csharp
// === INTERFACE CONTRACT ===
public interface IMabArchiveLoader
{
    int Sequence { get; }              // Position in load order (1-24)
    string Name { get; }                // Logical name for logging
    Task<int> LoadAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken);
}

// === TEMPLATE METHOD PATTERN ===
internal abstract class MabArchiveLoaderBase : IMabArchiveLoader
{
    public abstract int Sequence { get; }
    public abstract string Name { get; }

    // Template method: delegates to subclass-specific implementation
    public Task<int> LoadAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(context, year, cancellationToken);
    }

    // Subclasses must implement the actual execution logic
    protected abstract Task<int> ExecuteAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken);
}

// === SQL IMPLEMENTATION PATH ===
internal abstract class MabArchiveSqlLoaderBase : MabArchiveLoaderBase
{
    protected override Task<int> ExecuteAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken)
    {
        return context.Database.ExecuteSqlInterpolatedAsync(
            BuildSql(year), 
            cancellationToken);
    }

    protected abstract FormattableString BuildSql(int year);
}

// === LINQ IMPLEMENTATION PATH ===
internal abstract class MabArchiveDotNetLoaderBase : MabArchiveLoaderBase
{
    protected override Task<int> ExecuteAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken)
    {
        return LoadWithDotNetAsync(context, year, cancellationToken);
    }

    protected abstract Task<int> LoadWithDotNetAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken);
}
```

### Key Pattern Features

| Aspect | Pattern | Benefit |
|--------|---------|---------|
| **Abstraction** | Template Method | Subclasses implement only execution logic |
| **Polymorphism** | `ExecuteAsync()` override | SQL vs LINQ variants selectable at DI registration |
| **Contract** | `IMabArchiveLoader` | Orchestrator is decoupled from implementation |
| **Return Value** | `Task<int>` | Returns affected rows for validation |
| **Cancellation** | `CancellationToken` | Supports graceful async shutdown |

---

## 2. THREE REAL IMPLEMENTATIONS

### Implementation 1: MyTlkpProgramDotNetLoader (Seq=1)

**File:** [MyTlkpProgramLoader.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/MyTlkpProgramLoader.cs)

```csharp
internal sealed class MyTlkpProgramDotNetLoader : MabArchiveDotNetLoaderBase
{
    public override int Sequence => 1;
    public override string Name => "my_tlkpprogram";

    protected override async Task<int> LoadWithDotNetAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken)
    {
        // ✅ PHASE 1: LOAD SOURCE DATA (in-memory)
        var sourceRows = await context.MaSrcTlkpProgram
            .AsNoTracking()
            .Where(p => p.FpsYear == year)
            .Select(p => new
            {
                ProgramNo = p.ProgramNo,
                ProgramName = p.ProgramName,
                Directorate = p.Directorate,
                Minim = p.Minim,
                SectorName = p.SectorName,
                Customer = p.Customer,
                Target = p.Target,
                Manager = p.Manager
            })
            .ToListAsync(cancellationToken);  // ⚠️ ALL data into memory

        // ✅ PHASE 2: MAP TO DESTINATION ENTITIES
        var rows = sourceRows
            .Select(p => new MaDstMyTlkpProgram
            {
                Year = year,
                ProgramNo = p.ProgramNo,
                ProgramName = p.ProgramName,
                Directorate = p.Directorate,
                Minim = p.Minim,
                SectorName = p.SectorName,
                Customer = p.Customer,
                Target = p.Target,
                Manager = p.Manager
            })
            .ToList();

        // ✅ PHASE 3: EARLY RETURN IF EMPTY
        if (rows.Count == 0)
        {
            return 0;
        }

        // ✅ PHASE 4: INSERT WITH PER-ROW COMMITS (⚠️ N+1 problem)
        var inserted = 0;
        foreach (var row in rows)
        {
            await context.MaDstMyTlkpProgram.AddAsync(row, cancellationToken);
            inserted += await context.SaveChangesAsync(cancellationToken);  // ⚠️ Per-row commit
            context.Entry(row).State = EntityState.Detached;  // ✅ Cleanup
        }

        return inserted;
    }
}
```

**Characteristics:**
- **Simple linear flow:** source → map → insert
- **Per-row SaveChanges:** ⚠️ Performance concern for bulk loads
- **Early exit on empty:** ✅ Avoids unnecessary DB work
- **State cleanup:** ✅ Detaches entities to prevent memory leak

---

### Implementation 2: MyStaffDotNetLoader (Seq=21)

**File:** [MyStaffLoader.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/MyStaffLoader.cs)

```csharp
internal sealed class MyStaffDotNetLoader : MabArchiveDotNetLoaderBase
{
    public override int Sequence => 21;
    public override string Name => "my_staff";

    protected override async Task<int> LoadWithDotNetAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken)
    {
        // ✅ PHASE 1: COMPLEX LINQ QUERY (JOIN + TRANSFORMATION)
        var rows = await (
            from wge in context.MaSrcTblWgEmployee.AsNoTracking()
            join e in context.MaSrcTblEmployee.AsNoTracking()
                on wge.SpNumber equals e.SpNumber
            where wge.FpsYear == year
            select new MaDstMyStaff
            {
                Year = year,
                StaffId = wge.PactId,
                Name = (e.LastName ?? string.Empty) + ", " + (e.FirstName ?? string.Empty),
                WorkGroupGrade = wge.WorkGroupGrade,
                Title = e.Title,
                PersonStatus = wge.PersonStatus,
                PersonClass = wge.PersonClass,
                HrsPaid = wge.HrsPaid,
                LeaveHours = wge.LeaveHours,
                SickSpecial = wge.SickSpecial,
                HrsAvail = wge.HrsAvail
            })
            .ToListAsync(cancellationToken);  // ⚠️ Entire joined result into memory

        // ✅ PHASE 2: EARLY RETURN IF EMPTY
        if (rows.Count == 0)
        {
            return 0;
        }

        // ✅ PHASE 3: BATCH INSERT (unlike MyTlkpProgram, no per-row loop)
        await context.MaDstMyStaff.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);  // ✅ Single commit
    }
}
```

**Characteristics:**
- **LINQ JOIN:** Direct SQL join → LINQ query translation
- **String formatting:** `LastName + ", " + FirstName` done in LINQ
- **Batch insert:** ✅ Uses `AddRangeAsync()` with single `SaveChangesAsync()`
- **Performance:** Better than MyTlkpProgram (single vs N+1)
- **Complexity:** 10 mapped fields (medium complexity)

---

### Implementation 3: MyTlkpProjectDotNetLoader (Seq=3)

**File:** [MyTlkpProjectLoader.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/MyTlkpProjectLoader.cs)

```csharp
internal sealed class MyTlkpProjectDotNetLoader : MabArchiveDotNetLoaderBase
{
    public override int Sequence => 3;
    public override string Name => "my_tlkpproject";

    protected override async Task<int> LoadWithDotNetAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken)
    {
        // ✅ PHASE 1: PROJECTION QUERY (37 columns!)
        var sourceRows = await context.MaSrcTlkpProject
            .AsNoTracking()
            .Where(t => t.FpsYear == year)
            .Select(t => new
            {
                ParentProject = t.ParentProject,
                Program = t.Program,
                Customer = t.Customer,
                Manager = t.Manager,
                TransferIncome = t.TransferIncome,
                CustIncome = t.CustIncome,
                WipEoy = t.WipEoy,
                WipLimit = t.WipLimit,
                WipCurrent = t.WipCurrent,
                ProjectStatus = t.ProjectStatus,
                DateCreated = t.DateCreated,
                FecCost = t.FecCost,
                Profit = t.Profit,
                BudgetCvl = t.BudgetCvl,
                CaseworkSub = t.CaseworkSub,
                PvsIncome = t.PvsIncome,
                PlanCaseworkDebit = t.PlanCaseworkDebit,
                Disease = t.Disease,
                Contract = t.Contract,
                Finished = t.Finished,
                Comments = t.Comments,
                CarryOver = t.CarryOver,
                IsDefraProject = t.IsDefraProject,
                CostCentre = t.CostCentre,
                OracleProjectCode = t.OracleProjectCode,
                SubAccountCode = t.SubAccountCode,
                ProjectGroup = t.ProjectGroup,
                IncomeAccountCode = t.IncomeAccountCode
            })
            .ToListAsync(cancellationToken);  // ⚠️ Large objects into memory

        // ✅ PHASE 2: MAP ENTITY (mirror of source, with calculated fields)
        var rows = sourceRows
            .Select(t => new MaDstMyTlkpProject
            {
                Year = year,
                ParentProject = t.ParentProject,
                Program = t.Program,
                Customer = t.Customer,
                Manager = t.Manager,
                TransferIncome = t.TransferIncome,
                CustIncome = t.CustIncome,
                WipEoy = t.WipEoy,
                WipLimit = t.WipLimit,
                WipCurrent = t.WipCurrent,
                ProjectStatus = t.ProjectStatus,
                DateCreated = t.DateCreated,
                FecCost = t.FecCost,
                Profit = t.Profit,
                BudgetCvl = t.BudgetCvl,
                CaseworkSub = t.CaseworkSub,
                PvsIncome = t.PvsIncome,
                PlanCaseworkDebit = t.PlanCaseworkDebit,
                Disease = t.Disease,
                Contract = t.Contract,
                Finished = t.Finished,
                Comments = t.Comments,
                CarryOver = t.CarryOver,
                IsDefraProject = t.IsDefraProject,
                CostCentre = t.CostCentre,
                OracleProjectCode = t.OracleProjectCode,
                SubAccountCode = t.SubAccountCode,
                ProjectGroup = t.ProjectGroup,
                IncomeAccountCode = t.IncomeAccountCode
            })
            .ToList();

        // ✅ PHASE 3: EARLY RETURN IF EMPTY
        if (rows.Count == 0)
        {
            return 0;
        }

        // ✅ PHASE 4: BATCH INSERT (best practice pattern)
        await context.MaDstMyTlkpProject.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);  // ✅ Single commit
    }
}
```

**Characteristics:**
- **Large projection:** 29 mapped fields (memory footprint concern)
- **Two-stage mapping:** Source projection → destination entity (verbose but explicit)
- **Batch insert:** ✅ Best performance for this loader
- **Memory risk:** ⚠️ Highest among the three (large objects × large datasets)

---

## 3. EXISTING VALIDATION PATTERNS

### Pattern A: Empty Result Guard

**Found in:** All 20+ LINQ loaders

```csharp
if (rows.Count == 0)
{
    return 0;  // Early exit, no DB insert
}
```

**Purpose:** Skip unnecessary insert operations for empty years  
**Validation Hook:** Could log warnings if count drops below threshold

---

### Pattern B: State Cleanup (EntityState.Detached)

**Found in:** MyTlkpProgramDotNetLoader only

```csharp
foreach (var row in rows)
{
    await context.MaDstMyTlkpProgram.AddAsync(row, cancellationToken);
    inserted += await context.SaveChangesAsync(cancellationToken);
    context.Entry(row).State = EntityState.Detached;  // ✅ Cleanup
}
```

**Purpose:** Prevent memory accumulation in long-running operations  
**Validation Hook:** Could track total memory growth; alert if > threshold

---

### Pattern C: Sequence State Management

**Found in:** MyTblAnimalReqDotNetLoader (Seq=11)

**File:** [MyTblAnimalReqLoader.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/MyTblAnimalReqLoader.cs#L12-L80)

```csharp
protected override async Task<int> LoadWithDotNetAsync(...)
{
    // Keep insertion order for sequence counter alignment
    var sourceRows = await context.MaSrcTblAnimalReq
        .AsNoTracking()
        .Where(a => a.FpsYear == year)
        .OrderBy(a => a.IndCounter)  // ✅ Explicit order preservation
        .ToListAsync(cancellationToken);

    if (sourceRows.Count == 0) { return 0; }

    // ✅ CRITICAL: Read sequence state before insert
    var firstCounter = await GetNextArCounterAsync(context, cancellationToken);

    var rows = sourceRows
        .Select((a, index) => new MaDstMyTblAnimalReq
        {
            Year = year,
            JobCode = a.JobCode,
            AnimalType = a.AnimalType,
            NumberOfDays = a.NumberOfDays,
            NumberOfAnimals = a.NumberOfAnimals,
            ArCounter = firstCounter + index  // ✅ Sequential assignment
        })
        .ToList();

    await context.MaDstMyTblAnimalReq.AddRangeAsync(rows, cancellationToken);
    var affectedRows = await context.SaveChangesAsync(cancellationToken);

    // ✅ CRITICAL: Update sequence state after insert
    var lastCounter = firstCounter + rows.Count - 1;
    await context.Database.ExecuteSqlInterpolatedAsync(
        $"SELECT setval('mabarchive.my_tblanimalreq_ar_counter_seq', {lastCounter}, true)",
        cancellationToken);

    return affectedRows;
}

private static async Task<int> GetNextArCounterAsync(...)
{
    // Raw connection to read PostgreSQL sequence state
    var connection = context.Database.GetDbConnection();
    var closeAfter = connection.State != System.Data.ConnectionState.Open;

    if (closeAfter)
    {
        await connection.OpenAsync(cancellationToken);
    }

    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = 
            "SELECT last_value, is_called FROM mabarchive.my_tblanimalreq_ar_counter_seq";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        // ✅ VALIDATION: Throw if sequence state not readable
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "Could not read sequence state for my_tblanimalreq ar_counter.");
        }

        var lastValue = reader.GetInt64(0);
        var isCalled = reader.GetBoolean(1);
        var nextValue = isCalled ? lastValue + 1 : lastValue;
        return checked((int)nextValue);  // ✅ Overflow check via 'checked'
    }
    finally
    {
        if (closeAfter)
        {
            await connection.CloseAsync();
        }
    }
}
```

**Validation Hooks Present:**
1. ✅ Explicit `OrderBy()` to ensure deterministic ordering
2. ✅ Raw sequence read before insert (read-before-write pattern)
3. ✅ `if (!reader.ReadAsync)` guard with `InvalidOperationException`
4. ✅ `checked((int)nextValue)` for numeric overflow detection
5. ✅ Sequence update via `setval()` PostgreSQL function after insert

**Risk Pattern:** This is **complex validation state**. If sequence read fails → exception → entire year-load aborts.

---

### Pattern D: Orchestrator Registration & Sequence Validation

**File:** [MyFpsYearlyDataService.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/MyFpsYearlyDataService.cs#L100-L120)

```csharp
private readonly List<IMabArchiveLoader> _loaders;

public MyFpsYearlyDataService(IEnumerable<IMabArchiveLoader> loaders, ...)
{
    // ✅ VALIDATION 1: Count check
    var loaderList = loaders
        .OrderBy(l => l.Sequence)
        .ToList();

    if (loaderList.Count != ExpectedLoaderCount)  // 24
    {
        throw new InvalidOperationException(
            $"MABArchive loader registration mismatch. " +
            $"Expected {ExpectedLoaderCount} loaders, got {loaderList.Count}.");
    }

    // ✅ VALIDATION 2: Sequence continuity check
    var expectedSequences = Enumerable.Range(1, ExpectedLoaderCount);
    if (!expectedSequences.SequenceEqual(loaderList.Select(l => l.Sequence)))
    {
        throw new InvalidOperationException(
            "MABArchive loader sequence must be contiguous from 1 to 24.");
    }

    _loaders = loaderList;
}
```

**Validation Hooks Present:**
1. ✅ Count equality: exactly 24 loaders registered
2. ✅ Sequence continuity: 1→24 with no gaps
3. ✅ Early detection: exception thrown at DI container initialization (fail-fast)

---

## 4. DIFFERENCES BETWEEN LINQ AND SQL IMPLEMENTATIONS

### SQL Implementation (Production Default)

**File:** [MabArchiveLoaders.cs](src/Apha.BatchJobs/Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/MabArchiveLoaders.cs#L42-L64)

```csharp
internal sealed class MyTlkpProgramLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
        INSERT INTO mabarchive.my_tlkpprogram (
            year, programno, programname, directorate, minim, sector_name, 
            customer, target, manager
        )
        SELECT
            {0}, p.programno, p.programname, p.directorate, p.minim, 
            p.sector_name, p.customer, p.target, p.manager
        FROM fps.tlkpprogram p
        WHERE p.fpsyear = {0}
    ";

    public override int Sequence => 1;
    public override string Name => "my_tlkpprogram";

    protected override FormattableString BuildSql(int year) => 
        FormattableStringFactory.Create(SqlTemplate, year);
}
```

### LINQ Implementation (Configurable Alternative)

```csharp
internal sealed class MyTlkpProgramDotNetLoader : MabArchiveDotNetLoaderBase
{
    public override int Sequence => 1;
    public override string Name => "my_tlkpprogram";

    protected override async Task<int> LoadWithDotNetAsync(
        BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var sourceRows = await context.MaSrcTlkpProgram
            .AsNoTracking()
            .Where(p => p.FpsYear == year)
            .Select(p => new { ... })
            .ToListAsync(cancellationToken);

        var rows = sourceRows.Select(p => new MaDstMyTlkpProgram { ... }).ToList();

        if (rows.Count == 0) { return 0; }

        var inserted = 0;
        foreach (var row in rows)
        {
            await context.MaDstMyTlkpProgram.AddAsync(row, cancellationToken);
            inserted += await context.SaveChangesAsync(cancellationToken);
            context.Entry(row).State = EntityState.Detached;
        }

        return inserted;
    }
}
```

### Comparison Matrix

| Aspect | SQL | LINQ |
|--------|-----|------|
| **Database Execution** | Direct `INSERT ... SELECT` | EF Core DbContext operations |
| **Data Transfer** | Streaming (database-native) | Memory buffer (ToListAsync) |
| **Transformation Logic** | Database (SQL expressions) | .NET (LINQ → LINQ) |
| **Insertion Method** | Single bulk INSERT | Per-row or batch AddRange |
| **Performance** | ✅ Optimal (single round-trip) | ⚠️ Sub-optimal (2+ round-trips) |
| **Query Translation** | None (raw SQL) | LINQ → SQL (EF Core provider) |
| **Memory Footprint** | 🟢 Minimal (streaming) | 🟠 Large (full result set) |
| **Debugging** | SQL Profiler / Query Plan | EF Core logging |
| **Error Handling** | Database constraint violations | Entity validation + DB exceptions |
| **Validation Hooks** | Before SQL build only | Before/during/after LINQ phases |
| **State Management** | N/A | Entity state tracking required |

---

## 5. VALIDATION HOOKS: NATURAL INTEGRATION POINTS

### Hook 1: Pre-Execution Data Validation (Loader Level)

**Where:** Before `LoadWithDotNetAsync()` in DotNetLoaderBase  
**What to validate:** Source data exists and meets minimum quality

```csharp
protected abstract class MabArchiveDotNetLoaderBase : MabArchiveLoaderBase
{
    // NEW: Optional pre-execution hook
    protected virtual async Task<ValidationResult> ValidateSourceAsync(
        BatchJobsDbContext context, 
        int year, 
        CancellationToken cancellationToken)
    {
        // Default: no validation required (override in subclasses)
        return ValidationResult.Success;
    }

    protected override async Task<int> ExecuteAsync(...)
    {
        // ✅ Hook 1: Pre-validation
        var validation = await ValidateSourceAsync(context, year, cancellationToken);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(
                $"Validation failed for {Name}: {validation.ErrorMessage}");
        }

        return await LoadWithDotNetAsync(context, year, cancellationToken);
    }
}
```

**Usage in MyStaffDotNetLoader:**

```csharp
internal sealed class MyStaffDotNetLoader : MabArchiveDotNetLoaderBase
{
    protected override async Task<ValidationResult> ValidateSourceAsync(...)
    {
        // Ensure staff records exist and employee IDs are non-null
        var staffCount = await context.MaSrcTblWgEmployee
            .Where(w => w.FpsYear == year)
            .CountAsync(cancellationToken);

        if (staffCount == 0)
        {
            return ValidationResult.Warning(
                $"No staff records found for year {year}");
        }

        var nullSpNumbers = await context.MaSrcTblWgEmployee
            .Where(w => w.FpsYear == year && w.SpNumber == null)
            .CountAsync(cancellationToken);

        if (nullSpNumbers > 0)
        {
            return ValidationResult.Error(
                $"{nullSpNumbers} staff records have null SpNumber (join key)");
        }

        return ValidationResult.Success;
    }
}
```

---

### Hook 2: Post-Row Mapping Validation

**Where:** After `Select()` projection, before `AddRangeAsync()`  
**What to validate:** Mapped entities have required fields populated

```csharp
// In MyTlkpProjectDotNetLoader
protected override async Task<int> LoadWithDotNetAsync(...)
{
    var sourceRows = await context.MaSrcTlkpProject
        .AsNoTracking()
        .Where(t => t.FpsYear == year)
        .Select(t => new { ... })
        .ToListAsync(cancellationToken);

    var rows = sourceRows
        .Select(t => new MaDstMyTlkpProject { ... })
        .ToList();

    // ✅ Hook 2: Validate mapped entities
    var mappingErrors = ValidateMappedRows(rows);
    if (mappingErrors.Any())
    {
        throw new InvalidOperationException(
            $"Entity mapping validation failed:\n{string.Join("\n", mappingErrors)}");
    }

    if (rows.Count == 0) { return 0; }

    // ... insert logic
}

private List<string> ValidateMappedRows(List<MaDstMyTlkpProject> rows)
{
    var errors = new List<string>();

    foreach (var (index, row) in rows.Select((r, i) => (i, r)))
    {
        // Required fields must not be null
        if (string.IsNullOrEmpty(row.ParentProject))
            errors.Add($"Row {index}: ParentProject is null");
        
        if (string.IsNullOrEmpty(row.Program))
            errors.Add($"Row {index}: Program is null");

        // Domain rules
        if (row.Year != year)
            errors.Add($"Row {index}: Year mismatch (expected {year}, got {row.Year})");

        // Numeric constraints
        if (row.WipCurrent.HasValue && row.WipLimit.HasValue)
        {
            if (row.WipCurrent > row.WipLimit)
                errors.Add($"Row {index}: WipCurrent ({row.WipCurrent}) > WipLimit ({row.WipLimit})");
        }
    }

    return errors;
}
```

---

### Hook 3: Post-Insert Row Count Validation

**Where:** After `SaveChangesAsync()`, before `return inserted`  
**What to validate:** Row count matches expected range

```csharp
// In all DotNet loaders
protected override async Task<int> LoadWithDotNetAsync(...)
{
    var rows = sourceRows.Select(p => new MaDstMyTlkpProgram { ... }).ToList();

    if (rows.Count == 0) { return 0; }

    await context.MaDstMyTlkpProgram.AddRangeAsync(rows, cancellationToken);
    var inserted = await context.SaveChangesAsync(cancellationToken);

    // ✅ Hook 3: Validate insert count
    var validationResult = ValidateInsertCount(rows.Count, inserted);
    if (!validationResult.IsValid)
    {
        throw new InvalidOperationException(
            $"Insert validation failed for {Name}: {validationResult.ErrorMessage}");
    }

    return inserted;
}

protected virtual ValidationResult ValidateInsertCount(int expectedCount, int actualCount)
{
    // Default: expect 100% success rate
    if (actualCount != expectedCount)
    {
        return ValidationResult.Error(
            $"Expected to insert {expectedCount} rows, but inserted {actualCount}");
    }

    return ValidationResult.Success;
}
```

**Override in subclasses for special rules:**

```csharp
// MyStaffDotNetLoader: Allow partial success (e.g., duplicates skipped)
protected override ValidationResult ValidateInsertCount(int expectedCount, int actualCount)
{
    // Allow up to 5% loss (duplicates, constraint violations)
    var threshold = (int)(expectedCount * 0.95);
    if (actualCount < threshold)
    {
        return ValidationResult.Error(
            $"Insert success rate too low: {actualCount}/{expectedCount} ({actualCount*100/expectedCount}%)");
    }

    return ValidationResult.Success;
}
```

---

### Hook 4: Orchestrator-Level Cross-Loader Validation

**Where:** After each loader completes, in orchestrator  
**What to validate:** Data consistency across sequential loaders

```csharp
// In MyFpsYearlyDataService.ExecuteFullYearCycleAsync()
public async Task ExecuteFullYearCycleAsync(
    int year, CancellationToken cancellationToken)
{
    // ... delete phase ...

    // Load phase with validation
    var loadResults = new List<LoaderExecutionResult>();

    foreach (var loader in _loaders)
    {
        var beforeRowCount = await GetArchiveTableRowCountAsync(loader.Name);

        var affectedRows = await loader.LoadAsync(context, year, cancellationToken);

        var afterRowCount = await GetArchiveTableRowCountAsync(loader.Name);

        // ✅ Hook 4: Cross-loader validation
        var validation = ValidateLoaderExecution(
            loader.Sequence,
            loader.Name,
            affectedRows,
            beforeRowCount,
            afterRowCount);

        if (!validation.IsValid)
        {
            _logger.LogWarning(
                "Loader {Sequence}:{Name} returned unexpected row count. {Message}",
                loader.Sequence,
                loader.Name,
                validation.Message);
        }

        loadResults.Add(new LoaderExecutionResult
        {
            Sequence = loader.Sequence,
            Name = loader.Name,
            RowsInserted = affectedRows,
            Validation = validation
        });
    }

    // Final orchestration validation
    ValidateLoadPhaseCompletion(year, loadResults);
}

private ValidationResult ValidateLoaderExecution(
    int sequence, string name, int rowsInserted, int before, int after)
{
    // Rule: after = before + rowsInserted
    if (after - before != rowsInserted)
    {
        return ValidationResult.Warning(
            $"Loader {sequence}:{name} row count mismatch. " +
            $"Expected: {before + rowsInserted}, Actual: {after}");
    }

    // Rule: certain loaders have minimum row expectations
    var minimumExpectations = new Dictionary<string, int>
    {
        ["my_tlkpprogram"] = 1,      // Always > 0
        ["my_staff"] = 10,            // Expect at least 10 staff
        ["my_tlkpproject"] = 1,       // Always > 0
    };

    if (minimumExpectations.TryGetValue(name, out var minimum))
    {
        if (rowsInserted < minimum)
        {
            return ValidationResult.Warning(
                $"Loader {sequence}:{name} below threshold. " +
                $"Expected min {minimum}, got {rowsInserted}");
        }
    }

    return ValidationResult.Success;
}
```

---

### Hook 5: Database-Level Referential Integrity Validation

**Where:** After load phase completes, before returning to orchestrator  
**What to validate:** Foreign key relationships are intact

```csharp
private async Task<ValidationResult> ValidateReferentialIntegrity(
    int year, CancellationToken cancellationToken)
{
    var errors = new List<string>();

    // Rule: Every my_tlkpproject.program must exist in my_tlkpprogram
    var orphanPrograms = await _context.Database.SqlQuery<int>($@"
        SELECT COUNT(DISTINCT p.program)
        FROM mabarchive.my_tlkpproject p
        WHERE p.year = {year}
          AND p.program NOT IN (
              SELECT DISTINCT programno 
              FROM mabarchive.my_tlkpprogram 
              WHERE year = {year}
          )
    ").ToListAsync(cancellationToken);

    if (orphanPrograms[0] > 0)
    {
        errors.Add($"Orphan programs found in my_tlkpproject for year {year}");
    }

    // Rule: Every my_staff.workgroupgrade must exist in my_workgroupgrade
    var orphanGrades = await _context.Database.SqlQuery<int>($@"
        SELECT COUNT(DISTINCT s.workgroupgrade)
        FROM mabarchive.my_staff s
        WHERE s.year = {year}
          AND s.workgroupgrade NOT IN (
              SELECT DISTINCT workgroupgrade 
              FROM mabarchive.my_workgroupgrade 
              WHERE year = {year}
          )
    ").ToListAsync(cancellationToken);

    if (orphanGrades[0] > 0)
    {
        errors.Add($"Orphan workgroup grades found in my_staff for year {year}");
    }

    return errors.Any()
        ? ValidationResult.Error(string.Join("; ", errors))
        : ValidationResult.Success;
}
```

---

## 6. NATURAL INTEGRATION POINTS SUMMARY

| Hook # | Location | Trigger | What to Validate | Severity |
|--------|----------|---------|------------------|----------|
| **1** | Pre-LoadWithDotNetAsync | Before loader.LoadAsync() | Source table exists, FK join keys non-null | 🔴 HIGH |
| **2** | Post-Select, Pre-AddRange | After mapping projection | Required fields populated, domain rules | 🟠 MEDIUM |
| **3** | Post-SaveChanges | After each loader completes | Inserted rows = expected count | 🔴 HIGH |
| **4** | Orchestrator loop | After each loader in sequence | Row count deltas match expectations | 🟠 MEDIUM |
| **5** | End of load phase | Before returning year-load context | FK constraints, referential integrity | 🔴 HIGH |

---

## 7. RECOMMENDED VALIDATION FRAMEWORK

### Custom Result Type

```csharp
public class ValidationResult
{
    public bool IsValid { get; init; }
    public ValidationSeverity Severity { get; init; }
    public string Message { get; init; }

    public static ValidationResult Success => new()
    {
        IsValid = true,
        Severity = ValidationSeverity.None,
        Message = "✅ Validation passed"
    };

    public static ValidationResult Warning(string message) => new()
    {
        IsValid = true,
        Severity = ValidationSeverity.Warning,
        Message = message
    };

    public static ValidationResult Error(string message) => new()
    {
        IsValid = false,
        Severity = ValidationSeverity.Error,
        Message = message
    };
}

public enum ValidationSeverity { None, Warning, Error }
```

### Integration Point: DotNetLoaderBase Extension

```csharp
internal abstract class MabArchiveDotNetLoaderBase : MabArchiveLoaderBase
{
    protected virtual async Task<ValidationResult> ValidateSourceAsync(
        BatchJobsDbContext context, int year, CancellationToken cancellationToken)
        => ValidationResult.Success;

    protected virtual ValidationResult ValidateMappedEntities(IEnumerable<dynamic> rows)
        => ValidationResult.Success;

    protected virtual ValidationResult ValidateInsertCount(int expectedCount, int actualCount)
        => actualCount == expectedCount
            ? ValidationResult.Success
            : ValidationResult.Error($"Insert count mismatch: {actualCount} != {expectedCount}");

    protected override async Task<int> ExecuteAsync(
        BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var sourceValidation = await ValidateSourceAsync(context, year, cancellationToken);
        if (!sourceValidation.IsValid)
            throw new InvalidOperationException($"{Name}: {sourceValidation.Message}");

        return await LoadWithDotNetAsync(context, year, cancellationToken);
    }

    protected abstract Task<int> LoadWithDotNetAsync(
        BatchJobsDbContext context, int year, CancellationToken cancellationToken);
}
```

---

## 8. CLOUD MIGRATION IMPLICATIONS

### For Azure / PostgreSQL Migration

✅ **LINQ Validation Advantage:**
- All validation happens in .NET, independent of database dialect
- Easier to extend validation logic without SQL knowledge
- Portable across SQL Server → PostgreSQL → cloud databases

⚠️ **SQL Validation Advantage:**
- Database-native validation (constraints, triggers) is single source of truth
- No data transfer until fully validated
- Better for high-volume operations

### Recommended Cloud Strategy

1. **Phase 1 (SQL Mode):** Validate parity with baseline using SQL loaders
2. **Phase 2 (LINQ + Validation):** Introduce LINQ loaders with comprehensive validation hooks
3. **Phase 3 (Cloud Native):** Enable cloud-specific optimizations (bulk APIs, parallel execution)

---

## 9. KEY TAKEAWAYS

| Finding | Impact | Action |
|---------|--------|--------|
| **Validation hooks are already present** | Low | Document existing patterns in handler contracts |
| **LINQ loaders lack post-insert validation** | Medium | Implement ValidationResult framework in base class |
| **Orchestrator has no cross-loader validation** | Medium | Add loader execution context tracking |
| **MyTblAnimalReqLoader has sequence state validation** | High | Model this pattern for other stateful loaders |
| **SQL mode lacks source pre-validation** | Low | Validation is secondary; SQL constraints are primary |
| **Configuration mode switching works correctly** | Low | DI registration is sound, validation at startup |

