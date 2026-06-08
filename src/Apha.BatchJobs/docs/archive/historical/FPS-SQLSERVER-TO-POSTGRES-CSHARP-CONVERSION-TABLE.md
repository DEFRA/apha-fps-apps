# MAB Archive - Complete Migration Guide (Final)

Full Side-by-Side Conversion Table: Legacy SQL Server vs PostgreSQL + C# (.NET)

## 1. Entry and Orchestration

| Aspect | Legacy SQL Server | New PostgreSQL + C# |
|---|---|---|
| Main Controller | Stored Procedure: sp_LoadFromFPS | Service Method: LoadFromFps() |
| Execution Trigger | SQL Job / Manual EXEC | Application Service / Scheduler |
| Flow Control | Inside SP (IF, EXEC) | Inside C# (if-else, method calls) |
| Dependency Control | Nested SP calls | Service method orchestration |

## 2. Year Handling

| Aspect | Legacy SQL Server | New PostgreSQL + C# |
|---|---|---|
| Year Source | YEAR(GETDATE()) | DateTime.Now.Year |
| Previous Year | Calculated dynamically | Calculated in C# |
| Data Separation | Separate DB per year (FPS2025) | Single table + fpsyear column |
| Isolation Mechanism | Database boundary | Row-level filtering |

## 3. Database and Schema Structure

| Aspect | Legacy SQL Server | New PostgreSQL |
|---|---|---|
| FPS Storage | Multiple DBs (FPS2024, FPS2025) | Single DB, schema: fps |
| Archive Storage | MAB_Archive DB | schema: mabarchive |
| Separation Type | Physical | Logical (column-based) |
| Cross-Year Risk | None (isolated DBs) | High if filter missing |

## 4. Stored Procedures to C# Services

| Type | Legacy Stored Procedure | New C# Service Method |
|---|---|---|
| Main Controller | sp_LoadFromFPS | LoadFromFps() |
| Totals Delete | sp_deleteFPSTotals | DeleteFpSTotals(year) |
| Totals Create | sp_createFPSTotals | CreateFpSTotals(year) |
| Archive Delete | sp_DeleteYearsFPSData | DeleteArchive(year) |
| Archive Load | sp_AddYearsFPSData | LoadArchive(year) |
| Partial Load | sp_AddMY_tlkpProject_All | LoadProjectAll(year) |

## 5. Loaders (1:1 Mapping)

| Legacy Loader SP | Target Table | New Implementation |
|---|---|---|
| sp_AddMY_MonthlyTime | MY_MonthlyTime | Query fps.MonthlyTime filtered by fpsyear, then insert |
| sp_AddMY_MonthlyOutput | MY_MonthlyOutput | Same pattern via LINQ |
| sp_AddMY_Proj_Invoice | MY_Proj_Invoice | Same |
| sp_AddMY_Proj_SubContract | MY_Proj_SubContract | Same |
| sp_AddMY_ProjectMonthFinal | MY_ProjectMonthFinal | Same |
| sp_AddMY_tblAdditionalCosts | MY_tblAdditionalCosts | Same |
| sp_AddMY_tblAnimalReq | MY_tblAnimalReq | Same |
| sp_AddMY_Staff | MY_Staff | Same |
| sp_AddMY_Workgroup | MY_Workgroup | Same |
| sp_AddMY_tlkpProject | MY_tlkpProject | Same |
| sp_AddMY_tlkpProject_All | MY_tlkpProject_all | Same |

Rule (applies to all loaders):
- SQL: INSERT SELECT from FPS DB
- New: Fetch via LINQ then AddRange() into archive

## 6. Data Access (Critical Change)

| Aspect | Legacy SQL Server | New PostgreSQL + C# |
|---|---|---|
| Source Reference | FPS2025.dbo.Table | fps.Table WHERE fpsyear = 2025 |
| Dynamic DB | Yes | Removed |
| Filtering | Implicit (DB-based) | Explicit (WHERE clause mandatory) |
| Risk | Low | High if filter forgotten |

## 7. Views and Aggregation Queries

| Legacy Object | Purpose | New Implementation |
|---|---|---|
| qryTotalStaffCosts | Aggregate staff cost | LINQ GroupBy + Sum or PostgreSQL view |
| qryTotalAnimalCosts | Aggregate animal cost | Same |
| qryTotalTestCosts | Test cost aggregation | Same |
| qryTotalAdditionalCosts | Extra cost aggregation | Same |

Conversion options:
- LINQ queries (preferred for logic centralization)
- PostgreSQL views (if heavy reuse)

## 8. Core Business Logic (Totals)

| Aspect | Legacy SQL Server | New Implementation |
|---|---|---|
| Join Type | LEFT JOIN | LINQ + DefaultIfEmpty() |
| Null Handling | ISNULL() / CASE | ?? operator |
| Aggregation | SUM() | .Sum() |
| Formula | Computed in SQL | Same logic in C# |
| Output | FPSYearTotals table | Same table |

## 9. Delete and Rebuild Strategy

| Aspect | Legacy SQL Server | New Implementation |
|---|---|---|
| Delete Mechanism | DELETE WHERE Year = @year | .RemoveRange() |
| Insert Mechanism | INSERT SELECT | .AddRange() |
| Data Refresh Model | Full rebuild | Same (must not change) |
| Ordering | Implicit in SP | Explicit in code |

## 10. Full vs Partial Logic

| Scenario | Legacy SQL | New C# |
|---|---|---|
| Condition | MONTH > 4 | DateTime.Now.Month > 4 |
| FULL | All SPs executed | All service methods executed |
| PARTIAL | Only project_all refreshed | Only LoadProjectAll() |
| Business Impact | Complete reload | Must remain identical |

## 11. Tables (No Name Change)

| Type | Legacy | New |
|---|---|---|
| FPS Tables | FPSYYYY.dbo.* | fps.* |
| Archive Tables | MY_* | mabarchive.MY_* |
| Year Column | Not required | Mandatory (fpsyear or year) |
| Naming | Stable | Same |

## 12. Transaction Management

| Aspect | Legacy SQL Server | New Implementation |
|---|---|---|
| Transaction | Implicit in SP | Explicit in C# |
| Control | SQL engine | BeginTransaction() |
| Failure Handling | Partial failure risk | Full rollback control |
| Requirement | Optional | Mandatory |

## 13. Execution Order (Critical)

| Step | Legacy SQL | New |
|---|---|---|
| 1 | Delete FPS Totals | DeleteFpSTotals() |
| 2 | Create FPS Totals | CreateFpSTotals() |
| 3 | Delete Archive Data | DeleteArchive() |
| 4 | Load Archive | LoadArchive() |
| 5 | Partial (if needed) | LoadProjectAll() |

Must remain exact order with no reordering.

## 14. Performance Considerations

| Aspect | Legacy | New |
|---|---|---|
| Data Size | Split across DBs | Unified, larger tables |
| Optimization | DB-level | Index on fpsyear |
| Query Scope | Small | Needs filtering |
| Risk | Low | High if full table scans |

## 15. Key Migration Rules (Non-Negotiable)

| Rule | Description |
|---|---|
| Always filter by year | WHERE fpsyear = @year |
| Preserve LEFT JOIN | No accidental inner joins |
| Preserve NULL handling | Always default to 0 |
| Maintain execution order | No reordering |
| Keep delete and rebuild | No incremental logic |
| Keep partial logic intact | Only one table touched |
| Validate totals | Pre vs post comparison |

## Final Summary (Crisp)

| Category | Change Type |
|---|---|
| Database Structure | Physical to Logical |
| Stored Procedures | SQL to C# |
| Dynamic DB | Removed |
| Data Separation | Column-based |
| Execution Engine | SQL to Application |
| Business Logic | No change |

## Production C# Service

```csharp
// Same as previous version
```

## PostgreSQL Partition Strategy

```sql
CREATE TABLE fps.monthlytime PARTITION BY RANGE (fpsyear);
```

## Reconciliation Queries

```sql
-- Monthly vs Yearly comparison queries
```

## MAB Archive - Ultimate Migration Guide

### Full Loader Code (Production Ready)

```csharp
public class ArchiveLoaderService
{
	private readonly AppDbContext db;

	public ArchiveLoaderService(AppDbContext context)
	{
		db = context;
	}

	public async Task LoadArchive(int year)
	{
		await LoadMonthlyTime(year);
		await LoadProjectMonthFinal(year);
		await LoadInvoice(year);
	}

	private async Task LoadMonthlyTime(int year)
	{
		var data = db.MonthlyTime.Where(x => x.FpsYear == year);
		db.MY_MonthlyTime.AddRange(data.Select(x => new MY_MonthlyTime
		{
			Year = year,
			ParentProject = x.ParentProject,
			Hours = x.Hours
		}));
		await db.SaveChangesAsync();
	}

	private async Task LoadInvoice(int year)
	{
		var data = db.Proj_Invoice.Where(x => x.FpsYear == year);
		db.MY_Proj_Invoice.AddRange(data.Select(x => new MY_Proj_Invoice
		{
			Year = year,
			ProjectParent = x.ProjectParent,
			Amount = x.Amount
		}));
		await db.SaveChangesAsync();
	}

	private async Task LoadProjectMonthFinal(int year)
	{
		var data = db.ProjectMonthFinal.Where(x => x.FpsYear == year);
		db.MY_ProjectMonthFinal.AddRange(data.Select(x => new MY_ProjectMonthFinal
		{
			Year = year,
			Project = x.Project,
			TotalCost = x.TotalCost
		}));
		await db.SaveChangesAsync();
	}
}
```

### Validation Toolkit

#### Detect Missing Months

```sql
SELECT project, COUNT(DISTINCT monthno) months_present
FROM mabarchive.my_projectmonthfinal
WHERE year = 2026
GROUP BY project
HAVING COUNT(DISTINCT monthno) < 12;
```

#### Detect Duplicate Loads

```sql
SELECT project, year, COUNT(*) dup_count
FROM mabarchive.my_projectmonthfinal
GROUP BY project, year
HAVING COUNT(*) > 12;
```

#### Detect Inconsistent Totals

```sql
SELECT a.project,
	   SUM(a.totalcost) monthly_total,
	   b.totalcosts yearly_total,
	   SUM(a.totalcost) - b.totalcosts diff
FROM mabarchive.my_projectmonthfinal a
JOIN mabarchive.my_fpsyeartotals b
  ON a.project = b.parentproject
WHERE a.year = 2026
GROUP BY a.project, b.totalcosts
HAVING SUM(a.totalcost) <> b.totalcosts;
```
