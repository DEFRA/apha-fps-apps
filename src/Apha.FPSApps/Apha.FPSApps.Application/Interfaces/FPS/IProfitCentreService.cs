/*
 * TRANSFORMENGINE MIGRATION — IProfitCentreService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 2 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend service interface created as thin-delegate contract for ProfitCentre operations
 *   - Mirrors IFpsProfitCentreApiClient signatures exactly
 *   - Namespace: Apha.FPSApps.Application.Interfaces.FPS
 *
 * PRESERVED:
 *   - All method signatures match IFpsProfitCentreApiClient (GetProfitCentresAsync, GetAllProfitCentresAsync, paged variants)
 *   - GetAllProfitCentresAsync() returns IEnumerable<ProfitCentreDto> — type preserved from backend contract
 *   - UpdateProfitCentreSettingsAsync() parameters preserved (timesheet, outputsheet, timesheetLayout)
 *   - GetPagedProfitCenterCostSummaryAsync() double monthNumber parameter preserved
 *   - Return types wrapped in ApiResponseDto<T> envelope as per infrastructure convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify controller only injects IProfitCentreService (not IFpsApiClient directly)
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProfitCentreService
    {
        // TRANSFORMENGINE: Lookup — Resource Centre dropdown source (no pagination)
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync();
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetAllProfitCentresPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentreId);
        Task<ApiResponseDto<ProfitCentreDto>> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto);
        Task<ApiResponseDto<ProfitCentreDto>> UpdateProfitCentreAsync(string profitCentreId, ProfitCentreDto profitCentreDto);
        Task<ApiResponseDto<bool>> DeleteProfitCentreAsync(string profitCentreId);
        Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync();
        Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetLayout);
        Task<ApiResponseDto<List<ProfitCentreCostDto>>> GetPagedProfitCenterCostSummaryAsync(QueryParameters<string> query, double monthNumber);
    }
}
