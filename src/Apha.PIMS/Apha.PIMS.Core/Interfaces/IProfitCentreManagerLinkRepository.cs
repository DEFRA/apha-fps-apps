/*
 * TRANSFORMENGINE MIGRATION — IProfitCentreManagerLinkRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for ProfitCentreManagerLink CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (profitcentre, manager) — both string — reflected in method signatures
 *   - GetByProfitCentreAsync supports listing all manager links for a given profit centre
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
    // TRANSFORMENGINE: interface covers CRUD for ProfitCentreManagerLink (mabarchive.tblprofitcentre_manager_link); composite PK (profitcentre, manager)
    public interface IProfitCentreManagerLinkRepository
    {
        Task<List<ProfitCentreManagerLink>> GetAllAsync();

        Task<List<ProfitCentreManagerLink>> GetByProfitCentreAsync(string profitcentre);

        Task<ProfitCentreManagerLink?> GetByIdAsync(string profitcentre, string manager);

        Task<ProfitCentreManagerLink> AddAsync(ProfitCentreManagerLink entity);

        Task DeleteAsync(string profitcentre, string manager);

        Task<bool> ExistsAsync(string profitcentre, string manager);
    }
}
