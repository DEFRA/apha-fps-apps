/*
 * TRANSFORMENGINE MIGRATION — IPimsFrequencyApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for Frequency CRUD endpoints
 *   - Mirrors backend FrequencyController routes:
 *       GET    /api/v1/frequency                 — full list
 *       GET    /api/v1/frequency/{frequencyid}   — get by integer PK
 *       POST   /api/v1/frequency                 — create
 *       PUT    /api/v1/frequency/{frequencyid}   — update; route PK is authoritative
 *       DELETE /api/v1/frequency/{frequencyid}   — delete
 *   - Integer PK (frequencyid) — matches backend controller route constraint {frequencyid:int}
 *
 * PRESERVED:
 *   - All CRUD semantics matching FrequencyController actions
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm integer PK generation strategy — verify DB identity/sequence vs application-assigned
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors FrequencyController — integer PK (frequencyid); full CRUD
    public interface IPimsFrequencyApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/frequency — full list
        Task<ApiResponseDto<List<FrequencyDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/frequency/{frequencyid:int}
        Task<ApiResponseDto<FrequencyDto>> GetByIdAsync(int frequencyid);

        // TRANSFORMENGINE: POST /api/v1/frequency
        Task<ApiResponseDto<FrequencyDto>> CreateAsync(FrequencyDto dto);

        // TRANSFORMENGINE: PUT /api/v1/frequency/{frequencyid:int} — route PK is authoritative
        Task<ApiResponseDto<FrequencyDto>> UpdateAsync(int frequencyid, FrequencyDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/frequency/{frequencyid:int}
        Task<ApiResponseDto<bool>> DeleteAsync(int frequencyid);
    }
}
