/*
 * TRANSFORMENGINE MIGRATION — IFpsProfitCentreApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 1 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend API client interface created defining HTTP contract for ProfitCentre backend endpoints
 *   - Namespace scoped to Apha.FPSApps.Application.Interfaces.FpsApiClients
 *   - GetProfitCentresAsync() used as Resource Centre dropdown source in Set Up Staff Resources
 *   - GetAllProfitCentresPagedAsync() supports paginated CRUD grid in ProfitCentre maintenance
 *
 * PRESERVED:
 *   - All method signatures exactly match backend API controller endpoint signatures
 *   - GetAllProfitCentresAsync() returns IEnumerable<ProfitCentreDto> — type preserved from backend contract
 *   - UpdateProfitCentreSettingsAsync() parameters preserved (timesheet, outputsheet, timesheetLayout)
 *   - GetPagedProfitCenterCostSummaryAsync() double monthNumber parameter preserved
 *   - Return types wrapped in ApiResponseDto<T> envelope as per infrastructure convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify backend route paths match FpsProfitCentreApiClient implementation
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProfitCentreApiClient
    {
        // TRANSFORMENGINE: GetProfitCentresAsync — used as Resource Centre dropdown source (no pagination)
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync();
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetAllProfitCentresPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentreId);
        Task<ApiResponseDto<ProfitCentreDto>> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto);
        Task<ApiResponseDto<ProfitCentreDto>> UpdateProfitCentreAsync(string profitCentreId, ProfitCentreDto profitCentreDto);
        Task<ApiResponseDto<bool>> DeleteProfitCentreAsync(string profitCentreId);
        Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync();
        Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetLayout);
        Task<ApiResponseDto<List<ProfitCentreCostDto>>> GetPagedProfitCenterCostSummaryAsync(
            QueryParameters<string> query, double monthNumber);
    }
}
