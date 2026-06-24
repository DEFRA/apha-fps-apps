/*
 * TRANSFORMENGINE MIGRATION — AccountGroupRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New LINQ-first repository implementing IAccountGroupRepository
 *   - Targets mabarchive.tblcsg7_accountgroups via CostbookDbContext.AccountGroups DbSet
 *   - GetAllAsync: AsNoTracking ordered list of all CSG7 account groups
 *   - GetByCsg7GroupAsync: single-record AsNoTracking lookup by string PK
 *   - ExistsAsync: AnyAsync duplicate guard
 *   - AddAsync: Add + SaveChangesAsync, returns persisted entity
 *   - UpdateAsync: tracked entity update + SaveChangesAsync
 *   - DeleteAsync: AnyAsync guard then ExecuteDeleteAsync for set-based delete
 *
 * PRESERVED:
 *   - All interface method signatures from IAccountGroupRepository (Phase 2)
 *   - String PK (Csg7group / csg7group varchar 15) per DDL
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm no cascade delete rules on child rows referencing tblcsg7_accountgroups before DeleteAsync is called
 */

using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.Costbook.DataAccess.Repositories
{
    // TRANSFORMENGINE: LINQ-first repository for mabarchive.tblcsg7_accountgroups — full CRUD, no raw SQL SP calls
    public class AccountGroupRepository : IAccountGroupRepository
    {
        private readonly CostbookDbContext _context;

        public AccountGroupRepository(CostbookDbContext context)
        {
            _context = context;
        }

        // TRANSFORMENGINE: GetAllAsync — AsNoTracking ordered by Csg7group key for consistent list display
        public async Task<List<AccountGroup>> GetAllAsync()
        {
            return await _context.AccountGroups
                .AsNoTracking()
                .OrderBy(a => a.Csg7group)
                .ToListAsync();
        }

        // TRANSFORMENGINE: GetByCsg7GroupAsync — AsNoTracking single-record lookup by string PK csg7group
        public async Task<AccountGroup?> GetByCsg7GroupAsync(string csg7Group)
        {
            return await _context.AccountGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Csg7group == csg7Group);
        }

        // TRANSFORMENGINE: ExistsAsync — AnyAsync guard used by Add duplicate check
        public async Task<bool> ExistsAsync(string csg7Group)
        {
            return await _context.AccountGroups
                .AnyAsync(a => a.Csg7group == csg7Group);
        }

        // TRANSFORMENGINE: AddAsync — Add + SaveChangesAsync; returns the persisted entity
        public async Task<AccountGroup> AddAsync(AccountGroup accountGroup)
        {
            _context.AccountGroups.Add(accountGroup);
            await _context.SaveChangesAsync();
            return accountGroup;
        }

        // TRANSFORMENGINE: UpdateAsync — tracked entity update; Useinflation is the only editable scalar field
        public async Task<AccountGroup> UpdateAsync(AccountGroup accountGroup)
        {
            var existing = await _context.AccountGroups
                .FirstOrDefaultAsync(a => a.Csg7group == accountGroup.Csg7group);

            if (existing == null)
                throw new KeyNotFoundException($"AccountGroup with Csg7group '{accountGroup.Csg7group}' not found.");

            existing.Useinflation = accountGroup.Useinflation;

            await _context.SaveChangesAsync();
            return existing;
        }

        // TRANSFORMENGINE: DeleteAsync — AnyAsync guard then ExecuteDeleteAsync (set-based, no tracked load required)
        public async Task<bool> DeleteAsync(string csg7Group)
        {
            var exists = await _context.AccountGroups.AnyAsync(a => a.Csg7group == csg7Group);
            if (!exists)
                return false;

            await _context.AccountGroups
                .Where(a => a.Csg7group == csg7Group)
                .ExecuteDeleteAsync();

            return true;
        }
    }
}
