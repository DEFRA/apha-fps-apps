/*
 * TRANSFORMENGINE MIGRATION — IFrequencyService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for Frequency CRUD operations (Other Tab lookup management, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Single integer PK (frequencyid) reflected in GetByIdAsync / DeleteAsync / ExistsAsync
 *   - GetAllAsync returns full list for frequency dropdown / lookup usage
 *
 * PRESERVED:
 *   - No infrastructure-specific code in this Application interface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    // TRANSFORMENGINE: service interface for Frequency CRUD; single integer PK (frequencyid); lookup/reference table; consumed by FrequencyController (Phase 5)
    public interface IFrequencyService
    {
        Task<List<FrequencyDto>> GetAllAsync();

        Task<FrequencyDto?> GetByIdAsync(int frequencyid);

        Task<FrequencyDto> CreateAsync(FrequencyDto dto);

        Task<FrequencyDto> UpdateAsync(FrequencyDto dto);

        Task DeleteAsync(int frequencyid);

        Task<bool> ExistsAsync(int frequencyid);
    }
}
