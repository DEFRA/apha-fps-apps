/*
 * TRANSFORMENGINE MIGRATION — IFpsDepartmentIncomeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New typed API client interface for the DepartmentIncome read-only report resource
 *   - Method signatures match backend DepartmentIncomeController routes:
 *       GET /api/v1/department-income/time     → GetTimeIncomeAsync
 *       GET /api/v1/department-income/tests    → GetTestIncomeAsync
 *       GET /api/v1/department-income/animals  → GetAnimalIncomeAsync
 *       GET /api/v1/department-income/additional → GetAdditionalIncomeAsync
 *       GET /api/v1/department-income/totals   → GetTotalsAsync
 *       GET /api/v1/department-income/periods  → GetPeriodsAsync
 *   - All filter parameters (project?, monthFrom?, monthTo?) are optional — satisfiable from
 *     HTML project dropdown + period from/to dropdowns on the frontend view
 *   - Periods lookup has no filter params (backend endpoint takes no query params)
 *   - Return types wrapped in ApiResponseDto<T> per standard frontend response envelope
 *
 * PRESERVED:
 *   - Backend action parameter names preserved: project, monthFrom, monthTo
 *   - Resource is read-only (report/query form) — no Create/Update/Delete methods
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: FpsDepartmentIncomeApiClient.cs (infrastructure) must implement this interface
 *   - TRANSFORMENGINE TODO: Register IFpsDepartmentIncomeApiClient in ApiClientExtension.cs
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    // TRANSFORMENGINE: Typed API client interface — read-only report resource, no CRUD mutations
    public interface IFpsDepartmentIncomeApiClient
    {
        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/time — project/monthFrom/monthTo all optional
        Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/tests — project/monthFrom/monthTo all optional
        Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/animals — project/monthFrom/monthTo all optional
        Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/additional — project/monthFrom/monthTo all optional
        Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/totals — project/monthFrom/monthTo all optional
        Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // TRANSFORMENGINE: Maps to GET /api/v1/department-income/periods — no filter params (lookup only)
        Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync();
    }
}
