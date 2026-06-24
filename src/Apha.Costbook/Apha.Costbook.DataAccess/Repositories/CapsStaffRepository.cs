/*
 * TRANSFORMENGINE MIGRATION — CapsStaffRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New LINQ-first repository implementing ICapsStaffRepository
 *   - Targets mabarchive.tblcapsstaff via CostbookDbContext.CapsStaffs DbSet
 *   - GetAllAsync: AsNoTracking ordered list of all CAPS staff
 *   - GetPaginatedAsync: server-side paginated query with optional search on MNumber/Name/Dt2Number
 *   - GetByMNumberAsync: single-record AsNoTracking lookup by string PK
 *   - ExistsAsync: AnyAsync duplicate guard
 *   - AddAsync: Add + SaveChangesAsync, returns persisted entity
 *   - UpdateAsync: tracked entity update + SaveChangesAsync
 *   - DeleteAsync: AnyAsync guard then ExecuteDeleteAsync for set-based delete
 *
 * PRESERVED:
 *   - All interface method signatures from ICapsStaffRepository (Phase 2)
 *   - String PK (MNumber / mnumber varchar 50) per DDL
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify sort/search column names match frontend grid configuration
 */

using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.Costbook.DataAccess.Repositories
{
    // TRANSFORMENGINE: LINQ-first repository for mabarchive.tblcapsstaff — full CRUD, no raw SQL SP calls
    public class CapsStaffRepository : ICapsStaffRepository
    {
        private readonly CostbookDbContext _context;

        public CapsStaffRepository(CostbookDbContext context)
        {
            _context = context;
        }

        // TRANSFORMENGINE: GetAllAsync — AsNoTracking read; ordered by MNumber for deterministic list display
        public async Task<List<CapsStaff>> GetAllAsync()
        {
            return await _context.CapsStaffs
                .AsNoTracking()
                .OrderBy(c => c.MNumber)
                .ToListAsync();
        }

        // TRANSFORMENGINE: GetPaginatedAsync — server-side paging with optional search across MNumber, Name, Dt2Number
        public async Task<PagedData<CapsStaff>> GetPaginatedAsync(PaginationParameters<string> queryFilter)
        {
            var query = _context.CapsStaffs.AsNoTracking();

            // Apply search filter when present
            if (!string.IsNullOrWhiteSpace(queryFilter.Search))
            {
                var search = queryFilter.Search.ToLower();
                query = query.Where(c =>
                    c.MNumber.ToLower().Contains(search) ||
                    c.Name.ToLower().Contains(search) ||
                    (c.Dt2Number != null && c.Dt2Number.ToLower().Contains(search)));
            }

            // Apply sort
            query = queryFilter.SortBy?.ToLower() switch
            {
                "name" => queryFilter.Descending
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                "dt2number" => queryFilter.Descending
                    ? query.OrderByDescending(c => c.Dt2Number)
                    : query.OrderBy(c => c.Dt2Number),
                _ => queryFilter.Descending
                    ? query.OrderByDescending(c => c.MNumber)
                    : query.OrderBy(c => c.MNumber)
            };

            var totalRecords = await query.CountAsync();
            var pageNumber = queryFilter.Page < 1 ? 1 : queryFilter.Page;
            var pageSize = queryFilter.PageSize < 1 ? 10 : queryFilter.PageSize;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedData<CapsStaff>(data, new PaginationData
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            });
        }

        // TRANSFORMENGINE: GetByMNumberAsync — AsNoTracking single-record lookup by string PK mnumber
        public async Task<CapsStaff?> GetByMNumberAsync(string mNumber)
        {
            return await _context.CapsStaffs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.MNumber == mNumber);
        }

        // TRANSFORMENGINE: ExistsAsync — AnyAsync guard used by Add duplicate check
        public async Task<bool> ExistsAsync(string mNumber)
        {
            return await _context.CapsStaffs
                .AnyAsync(c => c.MNumber == mNumber);
        }

        // TRANSFORMENGINE: AddAsync — Add + SaveChangesAsync; returns the persisted entity
        public async Task<CapsStaff> AddAsync(CapsStaff capsStaff)
        {
            _context.CapsStaffs.Add(capsStaff);
            await _context.SaveChangesAsync();
            return capsStaff;
        }

        // TRANSFORMENGINE: UpdateAsync — tracked entity update; replaces scalar property values then SaveChangesAsync
        public async Task<CapsStaff> UpdateAsync(CapsStaff capsStaff)
        {
            var existing = await _context.CapsStaffs
                .FirstOrDefaultAsync(c => c.MNumber == capsStaff.MNumber);

            if (existing == null)
                throw new KeyNotFoundException($"CapsStaff with MNumber '{capsStaff.MNumber}' not found.");

            existing.Name = capsStaff.Name;
            existing.Dt2Number = capsStaff.Dt2Number;

            await _context.SaveChangesAsync();
            return existing;
        }

        // TRANSFORMENGINE: DeleteAsync — AnyAsync guard then ExecuteDeleteAsync (set-based, no tracked load)
        public async Task<bool> DeleteAsync(string mNumber)
        {
            var exists = await _context.CapsStaffs.AnyAsync(c => c.MNumber == mNumber);
            if (!exists)
                return false;

            await _context.CapsStaffs
                .Where(c => c.MNumber == mNumber)
                .ExecuteDeleteAsync();

            return true;
        }
    }
}
