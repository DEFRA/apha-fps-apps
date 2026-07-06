/*
 * TRANSFORMENGINE MIGRATION — IPimsAccessSystemApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for AccessSystem lookup endpoints
 *   - Mirrors backend AccessSystemController routes:
 *       GET /api/v1/accesssystem              — full reference list
 *       GET /api/v1/accesssystem/{systemid}   — get by integer PK
 *   - Read-only resource: no create/update/delete endpoints (reference/lookup data)
 *   - Integer PK (systemid)
 *
 * PRESERVED:
 *   - Read-only lookup semantics matching AccessSystemController actions
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors AccessSystemController — read-only reference data; integer PK (systemid)
    public interface IPimsAccessSystemApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/accesssystem — full reference lookup list
        Task<ApiResponseDto<List<AccessSystemDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/accesssystem/{systemid:int}
        Task<ApiResponseDto<AccessSystemDto>> GetByIdAsync(int systemid);
    }
}
