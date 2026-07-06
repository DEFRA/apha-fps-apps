/*
 * TRANSFORMENGINE MIGRATION — IRadTrackProgService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for RadTrackProg CRUD operations (Programme Tab, frmPIMSMainForm)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Natural string PK (program varchar(10)) reflected in GetByIdAsync / DeleteAsync / ExistsAsync
 *   - GetAllAsync returns full list for programme administration usage
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
    // TRANSFORMENGINE: service interface for RadTrackProg CRUD; natural string PK (program varchar(10)); Programme Tab; consumed by RadTrackProgController (Phase 5)
    public interface IRadTrackProgService
    {
        Task<List<RadTrackProgDto>> GetAllAsync();

        Task<RadTrackProgDto?> GetByIdAsync(string program);

        Task<RadTrackProgDto> CreateAsync(RadTrackProgDto dto);

        Task<RadTrackProgDto> UpdateAsync(RadTrackProgDto dto);

        Task DeleteAsync(string program);

        Task<bool> ExistsAsync(string program);
    }
}
