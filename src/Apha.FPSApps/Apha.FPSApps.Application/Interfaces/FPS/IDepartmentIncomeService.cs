/*
 * TRANSFORMENGINE MIGRATION — IDepartmentIncomeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New frontend service interface for the DepartmentIncome read-only report resource
 *   - Mirrors IFpsDepartmentIncomeApiClient method signatures exactly
 *   - All 6 query methods included: GetTimeIncomeAsync, GetTestIncomeAsync, GetAnimalIncomeAsync,
 *     GetAdditionalIncomeAsync, GetTotalsAsync, GetPeriodsAsync
 *   - All filter parameters (project?, monthFrom?, monthTo?) are optional — satisfiable from
 *     HTML project dropdown + period from/to dropdowns on the frontend view
 *   - Resource is read-only (report/query form) — no Create/Update/Delete methods
 *
 * PRESERVED:
 *   - Backend action parameter names preserved: project, monthFrom, monthTo
 *   - Return types wrapped in ApiResponseDto<T> per standard frontend response envelope
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Register IDepartmentIncomeService in Apha.FPSApps.Web ServiceCollectionExtension
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    // TRANSFORMENGINE: Frontend service interface — thin delegate over IFpsDepartmentIncomeApiClient (read-only report resource)
    public interface IDepartmentIncomeService
    {
        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/time — all filter params optional
        Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/tests — all filter params optional
        Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/animals — all filter params optional
        Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/additional — all filter params optional
        Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/totals — all filter params optional
        Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/periods — no filter params (lookup only)
        Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync();
    }
}
