/*
 * TRANSFORMENGINE MIGRATION — ReviewItemRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New EF Core LINQ-first repository implementing IReviewItemRepository
 *   - All read operations use AsNoTracking for performance
 *   - Single integer PK (itemid) — ValueGeneratedNever, client-supplied on create
 *   - DeleteAsync uses ExecuteDeleteAsync for set-based delete (no entity load required)
 *   - AddAsync uses Add + SaveChangesAsync; UpdateAsync uses Update + SaveChangesAsync
 *   - ExistsAsync uses AnyAsync guard — avoids full row load
 *
 * PRESERVED:
 *   - All method signatures defined in IReviewItemRepository (Phase 2)
 *   - mabarchive.tlkpreviewitem is the backing table (mapped via ReviewItemMap.cs)
 *   - ReviewItem.Item property maps to DDL column 'item'
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm integer PK generation strategy; if DB auto-generates, update Req contract and remove client-supplied PK pattern
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    // TRANSFORMENGINE: implements IReviewItemRepository — backs mabarchive.tlkpreviewitem; single integer PK (itemid); lookup/reference table with CRUD
    public class ReviewItemRepository : BaseRepository, IReviewItemRepository
    {
        private readonly PimsDbContext _dbContext;

        public ReviewItemRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: AsNoTracking read — full review item list for dropdown / lookup usage
        public async Task<List<ReviewItem>> GetAllAsync()
        {
            return await _dbContext.ReviewItems
                .AsNoTracking()
                .OrderBy(r => r.Itemid)
                .ToListAsync();
        }

        // TRANSFORMENGINE: AsNoTracking single-row lookup by integer PK (itemid)
        public async Task<ReviewItem?> GetByIdAsync(int itemid)
        {
            return await _dbContext.ReviewItems
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Itemid == itemid);
        }

        // TRANSFORMENGINE: Add + SaveChangesAsync — PK is client-supplied (ValueGeneratedNever on tlkpreviewitem.itemid)
        public async Task<ReviewItem> AddAsync(ReviewItem entity)
        {
            _dbContext.ReviewItems.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: Update + SaveChangesAsync — replaces full row for the supplied entity
        public async Task<ReviewItem> UpdateAsync(ReviewItem entity)
        {
            _dbContext.ReviewItems.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: ExecuteDeleteAsync — set-based delete by integer PK; no entity load required
        public async Task DeleteAsync(int itemid)
        {
            await _dbContext.ReviewItems
                .Where(r => r.Itemid == itemid)
                .ExecuteDeleteAsync();
        }

        // TRANSFORMENGINE: AnyAsync guard — avoids full row load for existence check
        public async Task<bool> ExistsAsync(int itemid)
        {
            return await _dbContext.ReviewItems
                .AnyAsync(r => r.Itemid == itemid);
        }
    }
}
