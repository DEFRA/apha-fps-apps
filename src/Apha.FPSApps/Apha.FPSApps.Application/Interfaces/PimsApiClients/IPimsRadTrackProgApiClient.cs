/*
 * TRANSFORMENGINE MIGRATION — IPimsRadTrackProgApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for RadTrackProg CRUD endpoints
 *   - Mirrors backend RadTrackProgController routes:
 *       GET    /api/v1/radtrackprog              — list all programmes
 *       GET    /api/v1/radtrackprog/{program}    — get by natural string PK
 *       POST   /api/v1/radtrackprog              — create
 *       PUT    /api/v1/radtrackprog/{program}    — update; route PK is authoritative
 *       DELETE /api/v1/radtrackprog/{program}    — delete
 *   - Natural string PK (program varchar(10)) — matches backend controller route
 *   - Programme Tab CRUD from frmPIMSMainForm
 *
 * PRESERVED:
 *   - All CRUD semantics matching RadTrackProgController actions
 *   - Return types wrapped in ApiResponseDto<T>
 *   - Natural string PK semantics (program)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm Programme Tab maps solely to tblradtrackprog (see backend controller deferred note)
 *   - TRANSFORMENGINE TODO: verify publicationprefix varchar(5) max length enforced via validation attribute on RadTrackProgReq
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors RadTrackProgController — natural string PK (program); full CRUD; Programme Tab
    public interface IPimsRadTrackProgApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/radtrackprog — full list for Programme Tab grid
        Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/radtrackprog/{program} — natural string PK lookup
        Task<ApiResponseDto<RadTrackProgDto>> GetByIdAsync(string program);

        // TRANSFORMENGINE: POST /api/v1/radtrackprog — create new programme; natural PK client-supplied
        Task<ApiResponseDto<RadTrackProgDto>> CreateAsync(RadTrackProgDto dto);

        // TRANSFORMENGINE: PUT /api/v1/radtrackprog/{program} — route PK is authoritative
        Task<ApiResponseDto<RadTrackProgDto>> UpdateAsync(string program, RadTrackProgDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/radtrackprog/{program} — delete by natural string PK
        Task<ApiResponseDto<bool>> DeleteAsync(string program);
    }
}
