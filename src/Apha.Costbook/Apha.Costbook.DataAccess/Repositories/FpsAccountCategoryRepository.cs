/*
 * TRANSFORMENGINE MIGRATION — FpsAccountCategoryRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New LINQ-first repository implementing IFpsAccountCategoryRepository
 *   - Targets fps[year].tblkpaccountcategory via CostbookDbContext.FpsAccountCategories DbSet
 *   - FpsYear filter is applied globally via HasQueryFilter in CostbookDbContext — no per-method year param needed
 *   - GetAllForMaintenanceAsync: AsNoTracking ordered list for maintenance grid
 *   - GetByAccShortNameAsync: AsNoTracking single-record lookup by string PK
 *   - ExistsAsync: AnyAsync duplicate guard (mirrors JS saveTblAccCat uniqueness check)
 *   - AddAsync: Add + SaveChangesAsync, returns persisted entity
 *   - UpdateAsync: tracked entity update of all editable scalar fields + SaveChangesAsync
 *   - UpdateCsg7GroupAsync: ExecuteUpdateAsync for targeted CSG7 group assignment (set-based, no tracked load)
 *   - DeleteAsync: AnyAsync guard then ExecuteDeleteAsync
 *
 * PRESERVED:
 *   - All interface method signatures from IFpsAccountCategoryRepository (Phase 2)
 *   - String PK (AccShortName / accshortname varchar 50) per DDL and FpsAccountCategoryMap
 *   - FpsYear scoping via DbContext HasQueryFilter (consistent with other year-scoped entities)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm AccountType and ConstituentAccountCodes are editable via maintenance UI
 *   - TRANSFORMENGINE TODO: verify UpdateAsync field list against maintenance Edit modal form fields
 */

using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.Costbook.DataAccess.Repositories
{
    // TRANSFORMENGINE: LINQ-first repository for fps[year].tblkpaccountcategory — full CRUD plus targeted CSG7 update
    public class FpsAccountCategoryRepository : IFpsAccountCategoryRepository
    {
        private readonly CostbookDbContext _context;

        public FpsAccountCategoryRepository(CostbookDbContext context)
        {
            _context = context;
        }

        // TRANSFORMENGINE: GetAllForMaintenanceAsync — AsNoTracking read; FpsYear filter applied globally via HasQueryFilter
        public async Task<List<FpsAccountCategory>> GetAllForMaintenanceAsync()
        {
            return await _context.FpsAccountCategories
                .AsNoTracking()
                .OrderBy(a => a.AccShortName)
                .ToListAsync();
        }

        // TRANSFORMENGINE: GetByAccShortNameAsync — AsNoTracking single-record lookup by string PK accshortname
        public async Task<FpsAccountCategory?> GetByAccShortNameAsync(string accShortName)
        {
            return await _context.FpsAccountCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccShortName == accShortName);
        }

        // TRANSFORMENGINE: ExistsAsync — AnyAsync duplicate guard; mirrors JS saveTblAccCat uniqueness check before insert
        public async Task<bool> ExistsAsync(string accShortName)
        {
            return await _context.FpsAccountCategories
                .AnyAsync(a => a.AccShortName == accShortName);
        }

        // TRANSFORMENGINE: AddAsync — Add + SaveChangesAsync; FpsYear must be set by caller from IFPSYearContext
        public async Task<FpsAccountCategory> AddAsync(FpsAccountCategory accountCategory)
        {
            _context.FpsAccountCategories.Add(accountCategory);
            await _context.SaveChangesAsync();
            return accountCategory;
        }

        // TRANSFORMENGINE: UpdateAsync — tracked entity update; all editable scalar fields updated then SaveChangesAsync
        public async Task<FpsAccountCategory> UpdateAsync(FpsAccountCategory accountCategory)
        {
            var existing = await _context.FpsAccountCategories
                .FirstOrDefaultAsync(a => a.AccShortName == accountCategory.AccShortName);

            if (existing == null)
                throw new KeyNotFoundException($"FpsAccountCategory with AccShortName '{accountCategory.AccShortName}' not found.");

            existing.AccountDescription = accountCategory.AccountDescription;
            existing.AccountType = accountCategory.AccountType;
            existing.ConstituentAccountCodes = accountCategory.ConstituentAccountCodes;
            existing.Csg7Group = accountCategory.Csg7Group;
            existing.ProjectSpecific = accountCategory.ProjectSpecific;
            existing.RcSpecific = accountCategory.RcSpecific;

            await _context.SaveChangesAsync();
            return existing;
        }

        // TRANSFORMENGINE: UpdateCsg7GroupAsync — ExecuteUpdateAsync (set-based); maps to saveTblAccCat csg7Group update path
        public async Task<bool> UpdateCsg7GroupAsync(string accShortName, string? csg7Group)
        {
            var rowsAffected = await _context.FpsAccountCategories
                .Where(a => a.AccShortName == accShortName)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Csg7Group, csg7Group));

            return rowsAffected > 0;
        }

        // TRANSFORMENGINE: DeleteAsync — AnyAsync guard then ExecuteDeleteAsync (set-based, no tracked load required)
        public async Task<bool> DeleteAsync(string accShortName)
        {
            var exists = await _context.FpsAccountCategories.AnyAsync(a => a.AccShortName == accShortName);
            if (!exists)
                return false;

            await _context.FpsAccountCategories
                .Where(a => a.AccShortName == accShortName)
                .ExecuteDeleteAsync();

            return true;
        }
    }
}
