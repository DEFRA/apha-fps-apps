/*
 * TRANSFORMENGINE MIGRATION — IPimsAccessUserLevelApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for AccessUserLevel CRUD endpoints
 *   - Mirrors backend AccessUserLevelController routes:
 *       GET    /api/v1/accessuserlevel                                          — full list
 *       GET    /api/v1/accessuserlevel/{systemid}                               — scoped by system
 *       GET    /api/v1/accessuserlevel/{systemid}/{ntlogin}                     — scoped by user within system
 *       GET    /api/v1/accessuserlevel/{systemid}/{ntlogin}/{accesslevelid}     — triple composite PK get
 *       POST   /api/v1/accessuserlevel                                          — create assignment
 *       DELETE /api/v1/accessuserlevel/{systemid}/{ntlogin}/{accesslevelid}     — delete by triple composite PK
 *   - Triple composite PK (systemid int + ntlogin string + accesslevelid int) — ntlogin URL-encoding in implementation
 *   - No PUT endpoint — assignment table has no mutable fields beyond composite PK
 *
 * PRESERVED:
 *   - Triple composite PK semantics (systemid + ntlogin + accesslevelid)
 *   - GetBySystemId and GetByUser scoped list endpoints preserved
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm triple composite delete route is acceptable for client consumers
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors AccessUserLevelController — triple composite PK (systemid + ntlogin + accesslevelid); ntlogin URL-encoding in implementation
    public interface IPimsAccessUserLevelApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/accessuserlevel — full list
        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int} — scoped by system
        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetBySystemIdAsync(int systemid);

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int}/{ntlogin} — scoped by user within system
        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetByUserAsync(int systemid, string ntlogin);

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int}/{ntlogin}/{accesslevelid:int} — triple composite PK get
        Task<ApiResponseDto<AccessUserLevelDto>> GetByIdAsync(int systemid, string ntlogin, int accesslevelid);

        // TRANSFORMENGINE: POST /api/v1/accessuserlevel
        Task<ApiResponseDto<AccessUserLevelDto>> CreateAsync(AccessUserLevelDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/accessuserlevel/{systemid:int}/{ntlogin}/{accesslevelid:int}
        Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin, int accesslevelid);
    }
}
