/*
 * TRANSFORMENGINE MIGRATION — IPimsSettingApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend API client interface for Setting read/update endpoints
 *   - Mirrors backend SettingController routes:
 *       GET /api/v1/setting                — full settings list
 *       GET /api/v1/setting/userupdateable — filtered list for user UI
 *       GET /api/v1/setting/{id}           — get by string PK
 *       PUT /api/v1/setting/{id}           — update setting value
 *   - No create/delete endpoints — settings are pre-configured rows (update-only)
 *   - String PK (id) — URL-encoding handled by implementation
 *
 * PRESERVED:
 *   - Read-only list of all settings and user-updateable-only filtered list
 *   - String PK (setting id) semantics
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm whether admin-only guard is required on UpdateAsync (see backend controller deferred note)
 *   - TRANSFORMENGINE TODO: confirm TestSetting environment-conditional editing
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors SettingController — read/update only; no create/delete; string PK (id)
    public interface IPimsSettingApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/setting — full settings list
        Task<ApiResponseDto<List<SettingDto>>> GetAllAsync();

        // TRANSFORMENGINE: GET /api/v1/setting/userupdateable — filtered list for user UI
        Task<ApiResponseDto<List<SettingDto>>> GetAllUserUpdateableAsync();

        // TRANSFORMENGINE: GET /api/v1/setting/{id}
        Task<ApiResponseDto<SettingDto>> GetByIdAsync(string id);

        // TRANSFORMENGINE: PUT /api/v1/setting/{id} — route id is authoritative
        Task<ApiResponseDto<SettingDto>> UpdateAsync(string id, SettingDto dto);
    }
}
