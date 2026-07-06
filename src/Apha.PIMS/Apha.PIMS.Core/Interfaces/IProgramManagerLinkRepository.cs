/*
 * TRANSFORMENGINE MIGRATION — IProgramManagerLinkRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for ProgramManagerLink CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (program, manager) — both string — reflected in method signatures
 *   - GetByProgramAsync supports listing all manager links for a given program
 *   - ExistsAsync follows AnyAsync-style existence semantics per phase rules
 *
 * PRESERVED:
 *   - No infrastructure-specific code (DbContext, EF) in this Core interface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    // TRANSFORMENGINE: interface covers CRUD for ProgramManagerLink (mabarchive.tblprogram_manager_link); composite PK (program, manager)
    public interface IProgramManagerLinkRepository
    {
        Task<List<ProgramManagerLink>> GetAllAsync();

        Task<List<ProgramManagerLink>> GetByProgramAsync(string program);

        Task<ProgramManagerLink?> GetByIdAsync(string program, string manager);

        Task<ProgramManagerLink> AddAsync(ProgramManagerLink entity);

        Task DeleteAsync(string program, string manager);

        Task<bool> ExistsAsync(string program, string manager);
    }
}
