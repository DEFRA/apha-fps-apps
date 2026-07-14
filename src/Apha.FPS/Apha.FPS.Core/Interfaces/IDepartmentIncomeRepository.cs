/*
 * TRANSFORMENGINE MIGRATION — IDepartmentIncomeRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination (Steps 2-3)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New read-only repository interface created for the DepartmentIncome resource family
 *   - Six async query methods covering all five query types from frmDeptIncome plus the period lookup:
 *       GetTimeIncomeAsync    — mirrors qryDeptIncomeTime (filter: project?, monthFrom, monthTo)
 *       GetTestIncomeAsync    — mirrors qryDeptIncomeTests (filter: project?, monthFrom, monthTo)
 *       GetAnimalIncomeAsync  — mirrors qryDeptIncomeAnimals (filter: project?, monthFrom, monthTo)
 *       GetAdditionalIncomeAsync — mirrors qryDeptIncomeExceptional (filter: project?, monthFrom, monthTo)
 *       GetTotalsAsync        — mirrors qryDeptIncomeTotals PIVOT (filter: project?, monthFrom, monthTo)
 *       GetPeriodsAsync       — period/month lookup, no filter parameters
 *   - All month filter parameters preserve VBA default semantics:
 *       fnDeptIncomeMonthFrom() defaults to 1 when null
 *       fnDeptIncomeMonthTo() defaults to 12 (or monthFrom) when null — enforced in Application layer
 *   - No CRUD methods: this is a reporting-only interface (form is read-only display)
 *
 * PRESERVED:
 *   - Async-only signatures per Core layer rules (no synchronous methods)
 *   - No EF Core / DbContext imports — Core layer must remain infrastructure-free
 *   - Return types use List<T> for flat result sets; no pagination needed (reporting queries)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: DepartmentIncomeRepository implementation (Phase 4) must handle
 *     fnAnimalDesc/fnAnimalDays VBA logic and DLookUp rate join for GetAnimalIncomeAsync
 *   - TRANSFORMENGINE TODO: GetTotalsAsync must replicate PIVOT via grouped conditional LINQ sums or raw SQL
 *   - TRANSFORMENGINE TODO: GetPeriodsAsync must resolve period data from fiscal calendar table or stored proc equivalent
 */

using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    // TRANSFORMENGINE: Read-only reporting interface — covers all 5 query types from frmDeptIncome plus period lookup
    public interface IDepartmentIncomeRepository
    {
        // TRANSFORMENGINE: mirrors qryDeptIncomeTime — returns time-based income rows filtered by project and month range
        Task<List<DepartmentIncomeTime>> GetTimeIncomeAsync(string? project, int monthFrom, int monthTo);

        // TRANSFORMENGINE: mirrors qryDeptIncomeTests — returns test-based income rows filtered by project and month range
        Task<List<DepartmentIncomeTest>> GetTestIncomeAsync(string? project, int monthFrom, int monthTo);

        // TRANSFORMENGINE: mirrors qryDeptIncomeAnimals — returns animal-based income rows filtered by project and month range
        // AcctCode IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in implementation
        Task<List<DepartmentIncomeAnimal>> GetAnimalIncomeAsync(string? project, int monthFrom, int monthTo);

        // TRANSFORMENGINE: mirrors qryDeptIncomeExceptional — returns additional/exceptional cost rows
        // AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in implementation
        Task<List<DepartmentIncomeAdditional>> GetAdditionalIncomeAsync(string? project, int monthFrom, int monthTo);

        // TRANSFORMENGINE: mirrors qryDeptIncomeTotals PIVOT — returns per-project totals aggregated across all four areas
        Task<List<DepartmentIncomeTotals>> GetTotalsAsync(string? project, int monthFrom, int monthTo);

        // TRANSFORMENGINE: period/month lookup — returns all available fiscal periods for the from/to dropdowns
        Task<List<PeriodLookup>> GetPeriodsAsync();
    }
}
