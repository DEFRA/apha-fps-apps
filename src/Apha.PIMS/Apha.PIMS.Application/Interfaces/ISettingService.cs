/*
 * TRANSFORMENGINE MIGRATION — ISettingService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for Setting read/update operations (Time Tab, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - String PK (id) reflected in GetByIdAsync / ExistsAsync
 *   - GetAllAsync supports admin listing of all application settings
 *   - GetAllUserUpdateableAsync returns only settings the current user may edit (Userupdateable == true guard)
 *   - No CreateAsync / DeleteAsync — settings are pre-seeded configuration records, not user-created entities
 *
 * PRESERVED:
 *   - No infrastructure-specific code in this Application interface
 *   - Userupdateable guard retained — service implementation must enforce before persisting
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: service implementation must check Userupdateable == true before applying UpdateAsync for non-admin callers
 *   - TRANSFORMENGINE TODO: Testsetting edits should be restricted to non-production environments in service implementation
 */

using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    // TRANSFORMENGINE: service interface for Setting read/update; string PK; no add/delete (pre-seeded config); consumed by SettingController (Phase 5)
    public interface ISettingService
    {
        Task<List<SettingDto>> GetAllAsync();

        // TRANSFORMENGINE: returns only settings where Userupdateable == true — used by non-admin user edit flows
        Task<List<SettingDto>> GetAllUserUpdateableAsync();

        Task<SettingDto?> GetByIdAsync(string id);

        // TRANSFORMENGINE: service must enforce Userupdateable guard — throw InvalidOperationException if caller tries to update a non-user-updateable setting
        Task<SettingDto> UpdateAsync(SettingDto dto);

        Task<bool> ExistsAsync(string id);
    }
}
