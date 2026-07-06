/*
 * TRANSFORMENGINE MIGRATION — IReviewItemRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for ReviewItem CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Single integer PK (itemid) — reflected in method signatures
 *   - GetAllAsync returns full list for review item dropdown / lookup usage
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
    // TRANSFORMENGINE: interface covers CRUD for ReviewItem (mabarchive.tlkpreviewitem); single integer PK (itemid); lookup/reference table
    public interface IReviewItemRepository
    {
        Task<List<ReviewItem>> GetAllAsync();

        Task<ReviewItem?> GetByIdAsync(int itemid);

        Task<ReviewItem> AddAsync(ReviewItem entity);

        Task<ReviewItem> UpdateAsync(ReviewItem entity);

        Task DeleteAsync(int itemid);

        Task<bool> ExistsAsync(int itemid);
    }
}
