/*
 * TRANSFORMENGINE MIGRATION — IWorkGroupGradeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 2 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend service interface created as thin-delegate contract for WorkGroupGrade operations
 *   - Mirrors IFpsWorkGroupGradeApiClient signatures exactly
 *   - Fixed malformed closing-brace indentation (namespace + class braces corrected)
 *   - Namespace: Apha.FPSApps.Application.Interfaces.FPS
 *
 * PRESERVED:
 *   - All method signatures match IFpsWorkGroupGradeApiClient (profitCentre filter, wgGrade key)
 *   - GetAllGradeCodesAsync() returns List<string> — simple code-list lookup preserved
 *   - Return types wrapped in ApiResponseDto<T> envelope as per infrastructure convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm DeleteWorkGroupGradeAsync vs DeleteAsync are distinct endpoints or duplicates
 *   - TRANSFORMENGINE TODO: Verify controller only injects IWorkGroupGradeService (not IFpsApiClient directly)
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkGroupGradeService
    {
        // TRANSFORMENGINE: Grade list filtered by profitCentre for cascading dropdown in Set Up Staff Resources
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(string profitCentre);
        Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade);
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<WorkgroupGradeDto>> GetByWgGradeAsync(string wgGrade);
        Task<ApiResponseDto<WorkgroupGradeDto>> CreateAsync(WorkgroupGradeDto dto);
        Task<ApiResponseDto<WorkgroupGradeDto>> UpdateAsync(string wgGrade, WorkgroupGradeDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(string wgGrade);
        Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync();
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkgroupGradesByWorkGroupAsync(string workGroup);
    }
}
