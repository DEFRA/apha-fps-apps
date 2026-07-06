/*
 * TRANSFORMENGINE MIGRATION — IReviewItemService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Application service interface for ReviewItem CRUD operations (Other Tab review items lookup, frmMaintainance)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Single integer PK (itemid) reflected in GetByIdAsync / DeleteAsync / ExistsAsync
 *   - GetAllAsync returns full list for review-item dropdown / lookup usage
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
    // TRANSFORMENGINE: service interface for ReviewItem CRUD; single integer PK (itemid); lookup/reference table; consumed by ReviewItemController (Phase 5)
    public interface IReviewItemService
    {
        Task<List<ReviewItemDto>> GetAllAsync();

        Task<ReviewItemDto?> GetByIdAsync(int itemid);

        Task<ReviewItemDto> CreateAsync(ReviewItemDto dto);

        Task<ReviewItemDto> UpdateAsync(ReviewItemDto dto);

        Task DeleteAsync(int itemid);

        Task<bool> ExistsAsync(int itemid);
    }
}
