/*
 * TRANSFORMENGINE MIGRATION — IProfitCentreManagerLinkService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for ProfitCentreManagerLink CRUD operations (Manager Tab resource centre sub-grid, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Composite PK (profitcentre, manager) — both string — reflected in method signatures
 *   - GetByProfitCentreAsync supports listing all manager links for a given profit centre
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
    // TRANSFORMENGINE: service interface for ProfitCentreManagerLink CRUD; composite PK (profitcentre, manager); consumed by ProfitCentreManagerLinkController (Phase 5)
    public interface IProfitCentreManagerLinkService
    {
        Task<List<ProfitCentreManagerLinkDto>> GetAllAsync();

        Task<List<ProfitCentreManagerLinkDto>> GetByProfitCentreAsync(string profitcentre);

        Task<ProfitCentreManagerLinkDto?> GetByIdAsync(string profitcentre, string manager);

        Task<ProfitCentreManagerLinkDto> CreateAsync(ProfitCentreManagerLinkDto dto);

        Task DeleteAsync(string profitcentre, string manager);

        Task<bool> ExistsAsync(string profitcentre, string manager);
    }
}
