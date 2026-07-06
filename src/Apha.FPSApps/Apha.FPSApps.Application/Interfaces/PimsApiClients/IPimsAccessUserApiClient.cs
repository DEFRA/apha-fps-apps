/*
 * TRANSFORMENGINE MIGRATION — IPimsAccessUserApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for AccessUser CRUD endpoints
 *   - Mirrors backend AccessUserController routes:
 *       GET    /api/v1/accessuser                                 — full list
 *       GET    /api/v1/accessuser/{systemid}                      — scoped by system (Admin tab system selector)
 *       GET    /api/v1/accessuser/{systemid}/{ntlogin}            — composite PK get
 *       POST   /api/v1/accessuser                                 — create
 *       PUT    /api/v1/accessuser/{systemid}/{ntlogin}            — update; composite PK is authoritative
 *       DELETE /api/v1/accessuser/{systemid}/{ntlogin}            — delete
 *   - Composite PK (systemid int + ntlogin string) — ntlogin URL-encoding handled by implementation
 *
 * PRESERVED:
 *   - Composite PK semantics (systemid + ntlogin)
 *   - GetBySystemId scoped list endpoint preserved for Admin tab system filtering
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm systemid is client-provided vs session-derived — see backend controller deferred note
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors AccessUserController — composite PK (systemid int + ntlogin string); ntlogin URL-encoding in implementation
    public interface IPimsAccessUserApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/accessuser — full list
        Task<ApiResponseDto<List<AccessUserDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/accessuser/{systemid:int} — scoped by system; satisfiable from Admin tab system selector
        Task<ApiResponseDto<List<AccessUserDto>>> GetBySystemIdAsync(int systemid);

        // TRANSFORMENGINE: GET /api/v1/accessuser/{systemid:int}/{ntlogin} — composite PK get
        Task<ApiResponseDto<AccessUserDto>> GetByIdAsync(int systemid, string ntlogin);

        // TRANSFORMENGINE: POST /api/v1/accessuser
        Task<ApiResponseDto<AccessUserDto>> CreateAsync(AccessUserDto dto);

        // TRANSFORMENGINE: PUT /api/v1/accessuser/{systemid:int}/{ntlogin} — composite PK is authoritative
        Task<ApiResponseDto<AccessUserDto>> UpdateAsync(int systemid, string ntlogin, AccessUserDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/accessuser/{systemid:int}/{ntlogin}
        Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin);
    }
}
