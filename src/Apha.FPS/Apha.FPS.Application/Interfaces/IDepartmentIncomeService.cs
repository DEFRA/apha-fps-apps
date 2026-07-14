/*
 * TRANSFORMENGINE MIGRATION — IDepartmentIncomeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New service interface created for the DepartmentIncome resource family
 *   - Six async methods matching frmDeptIncome query types plus the period lookup:
 *       GetTimeIncomeAsync    — returns time-based income rows; applies fnDeptIncomeMonthFrom/To defaults
 *       GetTestIncomeAsync    — returns test-based income rows; applies fnDeptIncomeMonthFrom/To defaults
 *       GetAnimalIncomeAsync  — returns animal-based income rows; applies fnDeptIncomeMonthFrom/To defaults
 *       GetAdditionalIncomeAsync — returns additional/exceptional cost rows; applies month defaults
 *       GetTotalsAsync        — returns per-project pivot totals; applies month defaults
 *       GetPeriodsAsync       — returns all available fiscal periods for the from/to dropdowns; no month params
 *   - Month parameters nullable at service boundary; VBA default logic applied inside implementation:
 *       fnDeptIncomeMonthFrom() → defaults to 1 when null
 *       fnDeptIncomeMonthTo()   → defaults to 12 (or monthFrom) when null
 *   - Returns List<T> DTOs (not paginated — reporting queries return full flat result sets)
 *
 * PRESERVED:
 *   - Async-only signatures per Application layer rules
 *   - Project parameter nullable to support "all projects" scenario (no filter applied when null)
 *
 * DEFERRED: none — fully automated.
 */

using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    // TRANSFORMENGINE: Service interface for DepartmentIncome reporting — covers all 5 query types plus period lookup
    public interface IDepartmentIncomeService
    {
        // TRANSFORMENGINE: Returns time-based income rows; monthFrom/monthTo nullable — VBA defaults applied in service impl
        Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeAsync(string? project, int? monthFrom, int? monthTo);

        // TRANSFORMENGINE: Returns test-based income rows; monthFrom/monthTo nullable — VBA defaults applied in service impl
        Task<List<DepartmentIncomeTestDto>> GetTestIncomeAsync(string? project, int? monthFrom, int? monthTo);

        // TRANSFORMENGINE: Returns animal-based income rows; monthFrom/monthTo nullable — VBA defaults applied in service impl
        Task<List<DepartmentIncomeAnimalDto>> GetAnimalIncomeAsync(string? project, int? monthFrom, int? monthTo);

        // TRANSFORMENGINE: Returns additional/exceptional cost rows; monthFrom/monthTo nullable — VBA defaults applied in service impl
        Task<List<DepartmentIncomeAdditionalDto>> GetAdditionalIncomeAsync(string? project, int? monthFrom, int? monthTo);

        // TRANSFORMENGINE: Returns per-project pivot totals across all four areas; monthFrom/monthTo nullable — VBA defaults applied in service impl
        Task<List<DepartmentIncomeTotalsDto>> GetTotalsAsync(string? project, int? monthFrom, int? monthTo);

        // TRANSFORMENGINE: Returns all available fiscal periods for the from/to dropdowns — no filter parameters
        Task<List<PeriodLookupDto>> GetPeriodsAsync();
    }
}
