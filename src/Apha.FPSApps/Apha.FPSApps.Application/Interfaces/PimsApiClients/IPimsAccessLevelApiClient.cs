/*
 * TRANSFORMENGINE MIGRATION — IPimsAccessLevelApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for AccessLevel CRUD endpoints
 *   - Mirrors backend AccessLevelController routes:
 *       GET    /api/v1/accesslevel                                     — full list
 *       GET    /api/v1/accesslevel/{systemid}                          — scoped by system
 *       GET    /api/v1/accesslevel/{systemid}/{accesslevelid}          — composite PK get
 *       POST   /api/v1/accesslevel                                     — create
 *       PUT    /api/v1/accesslevel/{systemid}/{accesslevelid}          — update; composite PK is authoritative
 *       DELETE /api/v1/accesslevel/{systemid}/{accesslevelid}          — delete
 *   - Composite PK (systemid int + accesslevelid int)
 *
 * PRESERVED:
 *   - Composite PK semantics (systemid + accesslevelid)
 *   - GetBySystemId scoped list endpoint preserved
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: AccessLevelReq does not exist in backend — body uses AccessLevelRes shape; create dedicated request contract if write semantics differ
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors AccessLevelController — composite PK (systemid int + accesslevelid int)
    public interface IPimsAccessLevelApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/accesslevel — full lookup list
        Task<ApiResponseDto<List<AccessLevelDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/accesslevel/{systemid:int} — scoped by system
        Task<ApiResponseDto<List<AccessLevelDto>>> GetBySystemIdAsync(int systemid);

        // TRANSFORMENGINE: GET /api/v1/accesslevel/{systemid:int}/{accesslevelid:int} — composite PK get
        Task<ApiResponseDto<AccessLevelDto>> GetByIdAsync(int systemid, int accesslevelid);

        // TRANSFORMENGINE: POST /api/v1/accesslevel
        Task<ApiResponseDto<AccessLevelDto>> CreateAsync(AccessLevelDto dto);

        // TRANSFORMENGINE: PUT /api/v1/accesslevel/{systemid:int}/{accesslevelid:int} — composite PK is authoritative
        Task<ApiResponseDto<AccessLevelDto>> UpdateAsync(int systemid, int accesslevelid, AccessLevelDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/accesslevel/{systemid:int}/{accesslevelid:int}
        Task<ApiResponseDto<bool>> DeleteAsync(int systemid, int accesslevelid);
    }
}
