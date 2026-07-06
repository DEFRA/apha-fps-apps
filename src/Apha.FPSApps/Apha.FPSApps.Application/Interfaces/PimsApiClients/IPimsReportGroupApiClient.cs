/*
 * TRANSFORMENGINE MIGRATION — IPimsReportGroupApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for ReportGroup CRUD endpoints
 *   - Mirrors backend ReportGroupController routes: GET/POST /api/v1/reportgroup, GET/PUT/DELETE /api/v1/reportgroup/{groupid}
 *   - Integer PK (groupid) — matches backend controller route constraint {groupid:int}
 *   - ReportGroup is also used as a lookup for the Report form dropdown
 *
 * PRESERVED:
 *   - All CRUD semantics matching ReportGroupController actions
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ReportGroupController — GET/POST /api/v1/reportgroup, GET/PUT/DELETE /api/v1/reportgroup/{groupid:int}
    public interface IPimsReportGroupApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/reportgroup — full lookup list (also used as Report dropdown source)
        Task<ApiResponseDto<List<ReportGroupDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/reportgroup/{groupid:int}
        Task<ApiResponseDto<ReportGroupDto>> GetByIdAsync(int groupid);

        // TRANSFORMENGINE: POST /api/v1/reportgroup
        Task<ApiResponseDto<ReportGroupDto>> CreateAsync(ReportGroupDto dto);

        // TRANSFORMENGINE: PUT /api/v1/reportgroup/{groupid:int}
        Task<ApiResponseDto<ReportGroupDto>> UpdateAsync(int groupid, ReportGroupDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/reportgroup/{groupid:int}
        Task<ApiResponseDto<bool>> DeleteAsync(int groupid);
    }
}
