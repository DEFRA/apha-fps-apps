/*
 * TRANSFORMENGINE MIGRATION — IPimsProgramManagerLinkApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for ProgramManagerLink CRUD endpoints
 *   - Mirrors backend ProgramManagerLinkController routes:
 *       GET    /api/v1/programmanagerlink                      — full list
 *       GET    /api/v1/programmanagerlink/{program}            — scoped by program
 *       GET    /api/v1/programmanagerlink/{program}/{manager}  — composite natural PK get
 *       POST   /api/v1/programmanagerlink                      — create link
 *       DELETE /api/v1/programmanagerlink/{program}/{manager}  — delete by composite natural PK
 *   - Composite natural PK (program string + manager string) — URL-encoding handled by implementation
 *   - No PUT endpoint — link table has no mutable fields beyond composite PK
 *
 * PRESERVED:
 *   - Composite natural PK semantics (program + manager)
 *   - GetByProgram scoped list endpoint preserved
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm composite natural PK delete route with URL-encoded string segments is acceptable
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ProgramManagerLinkController — composite natural PK (program + manager); URL-encoding in implementation
    public interface IPimsProgramManagerLinkApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/programmanagerlink — full list
        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/programmanagerlink/{program} — scoped by program
        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetByProgramAsync(string program);

        // TRANSFORMENGINE: GET /api/v1/programmanagerlink/{program}/{manager} — composite PK get
        Task<ApiResponseDto<ProgramManagerLinkDto>> GetByIdAsync(string program, string manager);

        // TRANSFORMENGINE: POST /api/v1/programmanagerlink
        Task<ApiResponseDto<ProgramManagerLinkDto>> CreateAsync(ProgramManagerLinkDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/programmanagerlink/{program}/{manager}
        Task<ApiResponseDto<bool>> DeleteAsync(string program, string manager);
    }
}
