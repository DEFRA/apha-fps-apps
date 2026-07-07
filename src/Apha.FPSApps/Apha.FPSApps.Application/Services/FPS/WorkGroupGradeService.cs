/*
 * TRANSFORMENGINE MIGRATION — WorkGroupGradeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 2 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend service implementation created as thin delegate forwarding to IFpsApiClient.FpsWorkGroupGrade
 *   - Implements IWorkGroupGradeService; injects IFpsApiClient (Sonar S2933: private readonly enforced)
 *   - All 8 method bodies are single-line return await delegates — no business logic
 *   - Fixed malformed closing-brace indentation (namespace + class braces corrected)
 *   - GetWorkGroupGradeAsync wraps string profitCentre in new QueryParameters<string>() as required by API client signature
 *
 * PRESERVED:
 *   - All method signatures match IWorkGroupGradeService / IFpsWorkGroupGradeApiClient exactly
 *   - GetAllGradeCodesAsync() returns List<string> — simple code-list lookup preserved
 *   - wgGrade key type (string) and profitCentre filter parameter preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm DeleteWorkGroupGradeAsync vs DeleteAsync are distinct backend endpoints or duplicates
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WorkGroupGradeService : IWorkGroupGradeService
    {
        // TRANSFORMENGINE: S2933 — private readonly field, injected via constructor
        private readonly IFpsApiClient _fpsClient;

        public WorkGroupGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        // TRANSFORMENGINE: Thin delegate — wraps profitCentre string in QueryParameters for API client call
        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(string profitCentre)
        {
            return await _fpsClient.FpsWorkGroupGrade.GetWorkGroupGradeAsync(new QueryParameters<string>(), profitCentre);
        }

        // TRANSFORMENGINE: Thin delegate — forwards to _fpsClient.FpsWorkGroupGrade (no logic)
        public async Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupGrade.DeleteWorkGroupGradeAsync(wgGrade);
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query)
            => await _fpsClient.FpsWorkGroupGrade.GetAllWorkgroupGradesPagedAsync(query);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> GetByWgGradeAsync(string wgGrade)
            => await _fpsClient.FpsWorkGroupGrade.GetByWgGradeAsync(wgGrade);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> CreateAsync(WorkgroupGradeDto dto)
            => await _fpsClient.FpsWorkGroupGrade.CreateAsync(dto);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> UpdateAsync(string wgGrade, WorkgroupGradeDto dto)
            => await _fpsClient.FpsWorkGroupGrade.UpdateAsync(wgGrade, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(string wgGrade)
            => await _fpsClient.FpsWorkGroupGrade.DeleteAsync(wgGrade);

        public async Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync()
            => await _fpsClient.FpsWorkGroupGrade.GetAllGradeCodesAsync();
    }
}
