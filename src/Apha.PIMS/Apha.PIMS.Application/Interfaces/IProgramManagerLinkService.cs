/*
 * TRANSFORMENGINE MIGRATION — IProgramManagerLinkService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for ProgramManagerLink CRUD operations (Manager Tab program sub-grid, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (program, manager) — both string — reflected in method signatures
 *   - GetByProgramAsync supports listing all manager links for a given program
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
    // TRANSFORMENGINE: service interface for ProgramManagerLink CRUD; composite PK (program, manager); consumed by ProgramManagerLinkController (Phase 5)
    public interface IProgramManagerLinkService
    {
        Task<List<ProgramManagerLinkDto>> GetAllAsync();

        Task<List<ProgramManagerLinkDto>> GetByProgramAsync(string program);

        Task<ProgramManagerLinkDto?> GetByIdAsync(string program, string manager);

        Task<ProgramManagerLinkDto> CreateAsync(ProgramManagerLinkDto dto);

        Task DeleteAsync(string program, string manager);

        Task<bool> ExistsAsync(string program, string manager);
    }
}
