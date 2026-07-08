/*
 * TRANSFORMENGINE MIGRATION — IFpsWorkGroupGradeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 1 — Frontend DTOs + API Client Interfaces
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Frontend API client interface created defining HTTP contract for WorkGroupGrade backend endpoints
 *   - Namespace scoped to Apha.FPSApps.Application.Interfaces.FpsApiClients
 *   - Fixed brace indentation (extra closing-brace indentation corrected)
 *   - GetWorkGroupGradeAsync(query, profitCentre) used to populate Grade listbox filtered by Resource Centre
 *     in Set Up Staff Resources page
 *
 * PRESERVED:
 *   - All method signatures exactly match backend API controller endpoint signatures
 *   - profitCentre filter parameter preserved on GetWorkGroupGradeAsync for cascading filter support
 *   - GetAllGradeCodesAsync() returns List<string> — simple code-list lookup preserved
 *   - Return types wrapped in ApiResponseDto<T> envelope as per infrastructure convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify backend route paths match FpsWorkGroupGradeApiClient implementation
 *   - TRANSFORMENGINE TODO: Confirm DeleteWorkGroupGradeAsync vs DeleteAsync are distinct endpoints or duplicates
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsWorkGroupGradeApiClient
    {
        // TRANSFORMENGINE: GetWorkGroupGradeAsync — grade list filtered by profitCentre for cascading dropdown
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(QueryParameters<string> query, string profitCentre);
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
