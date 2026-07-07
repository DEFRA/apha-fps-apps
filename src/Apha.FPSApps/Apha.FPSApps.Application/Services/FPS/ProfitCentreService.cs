/*
 * TRANSFORMENGINE MIGRATION — ProfitCentreService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 2 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend service implementation created as thin delegate forwarding to IFpsApiClient.FpsProfitCentre
 *   - Implements IProfitCentreService; injects IFpsApiClient (Sonar S2933: private readonly enforced)
 *   - All 8 method bodies are single-line return await delegates — no business logic
 *
 * PRESERVED:
 *   - All method signatures match IProfitCentreService / IFpsProfitCentreApiClient exactly
 *   - GetAllProfitCentresAsync() returns IEnumerable<ProfitCentreDto> — type preserved from backend contract
 *   - UpdateProfitCentreSettingsAsync() multi-param signature preserved
 *   - GetPagedProfitCenterCostSummaryAsync() double monthNumber parameter preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated thin-delegate pattern
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProfitCentreService : IProfitCentreService
    {
        // TRANSFORMENGINE: S2933 — private readonly field, injected via constructor
        private readonly IFpsApiClient _fpsClient;

        public ProfitCentreService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        // TRANSFORMENGINE: Thin delegate — forwards to _fpsClient.FpsProfitCentre (no logic)
        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync()
        {
            return await _fpsClient.FpsProfitCentre.GetProfitCentresAsync();
        }

        public async Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync()
        {
            return await _fpsClient.FpsProfitCentre.GetAllProfitCentresAsync();
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetAllProfitCentresPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsProfitCentre.GetAllProfitCentresPagedAsync(query);
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentreId)
        {
            return await _fpsClient.FpsProfitCentre.GetProfitCentreByIdAsync(profitCentreId);
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto)
        {
            return await _fpsClient.FpsProfitCentre.CreateProfitCentreAsync(profitCentreDto);
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> UpdateProfitCentreAsync(string profitCentreId, ProfitCentreDto profitCentreDto)
        {
            return await _fpsClient.FpsProfitCentre.UpdateProfitCentreAsync(profitCentreId, profitCentreDto);
        }

        public async Task<ApiResponseDto<bool>> DeleteProfitCentreAsync(string profitCentreId)
        {
            return await _fpsClient.FpsProfitCentre.DeleteProfitCentreAsync(profitCentreId);
        }

        public async Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(
            string profitCentre, int timesheet, int outputsheet, short timesheetLayout)
        {
            return await _fpsClient.FpsProfitCentre.UpdateProfitCentreSettingsAsync(
                profitCentre, timesheet, outputsheet, timesheetLayout);
        }

        public async Task<ApiResponseDto<List<ProfitCentreCostDto>>> GetPagedProfitCenterCostSummaryAsync(
            QueryParameters<string> query, double monthNumber)
            => await _fpsClient.FpsProfitCentre.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);
    }
}
