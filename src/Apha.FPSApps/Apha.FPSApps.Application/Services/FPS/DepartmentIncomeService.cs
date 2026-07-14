/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New thin frontend service implementation delegating to IFpsApiClient.FpsDepartmentIncome
 *   - Injects IFpsApiClient (aggregate client) and forwards all calls to FpsDepartmentIncome sub-client
 *   - 6 methods: GetTimeIncomeAsync, GetTestIncomeAsync, GetAnimalIncomeAsync,
 *     GetAdditionalIncomeAsync, GetTotalsAsync, GetPeriodsAsync
 *   - Each method body is a single return await delegation — no business logic
 *   - Resource is read-only (report/query form) — no Create/Update/Delete methods
 *
 * PRESERVED:
 *   - All parameter names match IFpsDepartmentIncomeApiClient exactly (project, monthFrom, monthTo)
 *   - Return types wrapped in ApiResponseDto<T> per standard frontend response envelope
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Register DepartmentIncomeService → IDepartmentIncomeService in Apha.FPSApps.Web ServiceCollectionExtension
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    // TRANSFORMENGINE: Thin delegate — forwards all calls to IFpsApiClient.FpsDepartmentIncome; no business logic
    public class DepartmentIncomeService : IDepartmentIncomeService
    {
        // TRANSFORMENGINE: S2933 — private readonly per Sonar rule
        private readonly IFpsApiClient _fpsClient;

        public DepartmentIncomeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        // TRANSFORMENGINE: Delegates to GET /api/v1/department-income/time via FpsDepartmentIncome sub-client
        public async Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTimeIncomeAsync(project, monthFrom, monthTo);

        // TRANSFORMENGINE: Delegates to GET /api/v1/department-income/tests via FpsDepartmentIncome sub-client
        public async Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTestIncomeAsync(project, monthFrom, monthTo);

        // TRANSFORMENGINE: Delegates to GET /api/v1/department-income/animals via FpsDepartmentIncome sub-client
        public async Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetAnimalIncomeAsync(project, monthFrom, monthTo);

        // TRANSFORMENGINE: Delegates to GET /api/v1/department-income/additional via FpsDepartmentIncome sub-client
        public async Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetAdditionalIncomeAsync(project, monthFrom, monthTo);

        // TRANSFORMENGINE: Delegates to GET /api/v1/department-income/totals via FpsDepartmentIncome sub-client
        public async Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null)
            => await _fpsClient.FpsDepartmentIncome.GetTotalsAsync(project, monthFrom, monthTo);

        // TRANSFORMENGINE: Delegates to GET /api/v1/department-income/periods via FpsDepartmentIncome sub-client (no filter params)
        public async Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync()
            => await _fpsClient.FpsDepartmentIncome.GetPeriodsAsync();
    }
}
