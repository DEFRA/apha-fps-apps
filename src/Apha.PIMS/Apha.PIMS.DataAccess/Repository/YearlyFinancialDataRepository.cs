/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# repository existed for my_tlkpprojectradtrackdata
 *   - Implements IYearlyFinancialDataRepository using EF Core LINQ — no raw SP calls
 *   - GetAllAsync: AsNoTracking paginated query filtered by project, with search across
 *     year/project/costedby and sort across all display columns
 *   - GetByKeyAsync: AsNoTracking FirstOrDefaultAsync by composite key (year, project)
 *   - ExistsAsync: AnyAsync guard — lightweight existence check for duplicate-key validation
 *   - CreateAsync: AddAsync + SaveChangesAsync — single row insert
 *   - UpdateAsync: Attach + Entry state = Modified + SaveChangesAsync — full entity update
 *   - DeleteAsync: ExecuteDeleteAsync for set-based single-row delete by composite key
 *   - GetPactCostsAsync: AsNoTracking query against PactProjectYearCosts keyless view,
 *     filtered by project and year (cast double Year to short for filter), returns all
 *     month rows for the given year so service/caller can aggregate as needed
 *   - Private helpers: ApplySearch, ApplySorting, ApplyOrder — same pattern as existing repos
 *   - BaseRepository not extended — uses direct _context injection for consistency with
 *     ProjectYearCostsRepository which also manages its own paging
 *
 * PRESERVED:
 *   - Composite key (year, project) semantics from CONSTRAINT pk_my_tlkpprojectradtrackdata
 *   - All field names and method signatures from IYearlyFinancialDataRepository interface
 *   - AsNoTracking for all read paths
 *   - AnyAsync for existence check
 *   - ExecuteDeleteAsync for delete (set-based, no entity load required)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm EF Core handles money↔decimal? mapping for Npgsql without
 *     explicit value converter — if runtime cast errors occur, wrap money columns with
 *     HasConversion<decimal> in the map file
 *   - TRANSFORMENGINE TODO: GetPactCostsAsync casts view Year (double) to short for filtering;
 *     verify this cast is lossless for all realistic financial year values (e.g. 2025.0)
 */

using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.Repository
{
    /// <summary>
    /// LINQ-first repository for <see cref="YearlyFinancialData"/> (per-year project financial records)
    /// and <see cref="PactProjectYearCosts"/> (PACT actuals aggregation view).
    /// Implements <see cref="IYearlyFinancialDataRepository"/>.
    /// </summary>
    public class YearlyFinancialDataRepository : IYearlyFinancialDataRepository
    {
        private readonly PimsDbContext _context;

        public YearlyFinancialDataRepository(PimsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // TRANSFORMENGINE: GetAllAsync — paginated list filtered by project
        //   Mirrors Access form RecordSource: SELECT * FROM my_tlkpprojectradtrackdata WHERE project = ?
        //   with optional search on year / costedby and column sorting
        public async Task<PagedData<YearlyFinancialData>> GetAllAsync(
            string project,
            PaginationParameters<string> paging)
        {
            IQueryable<YearlyFinancialData> query = _context.YearlyFinancialData
                .AsNoTracking()
                .Where(e => e.Project == project);

            query = ApplySearch(query, paging.Search);
            query = ApplySorting(query, paging.SortBy, paging.Descending);

            int totalRecords = await query.CountAsync();

            List<YearlyFinancialData> data = paging.Page == -1
                ? await query.ToListAsync()
                : await query
                      .Skip((paging.Page - 1) * paging.PageSize)
                      .Take(paging.PageSize)
                      .ToListAsync();

            var pagination = new PaginationData
            {
                PageNumber   = paging.Page,
                PageSize     = paging.PageSize,
                TotalRecords = totalRecords,
                TotalPages   = paging.Page == -1
                    ? 1
                    : (int)Math.Ceiling((double)totalRecords / paging.PageSize)
            };

            return new PagedData<YearlyFinancialData>(data, pagination);
        }

        // TRANSFORMENGINE: GetByKeyAsync — single record by composite key (year + project)
        public async Task<YearlyFinancialData?> GetByKeyAsync(short year, string project)
        {
            return await _context.YearlyFinancialData
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Year == year && e.Project == project);
        }

        // TRANSFORMENGINE: ExistsAsync — AnyAsync for lightweight duplicate-key check
        public async Task<bool> ExistsAsync(short year, string project)
        {
            return await _context.YearlyFinancialData
                .AnyAsync(e => e.Year == year && e.Project == project);
        }

        // TRANSFORMENGINE: CreateAsync — insert new row into my_tlkpprojectradtrackdata
        public async Task<YearlyFinancialData> CreateAsync(YearlyFinancialData entity)
        {
            await _context.YearlyFinancialData.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: UpdateAsync — full entity update on existing row; Attach + Modified state
        //   Caller (service) loads the existing entity first, maps updated values onto it,
        //   then passes the tracked entity here — SaveChangesAsync persists all dirty properties
        public async Task<YearlyFinancialData> UpdateAsync(YearlyFinancialData entity)
        {
            _context.YearlyFinancialData.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: DeleteAsync — ExecuteDeleteAsync for set-based delete by composite key
        //   Returns true if a row was deleted, false if not found
        public async Task<bool> DeleteAsync(short year, string project)
        {
            int affected = await _context.YearlyFinancialData
                .Where(e => e.Year == year && e.Project == project)
                .ExecuteDeleteAsync();

            return affected > 0;
        }

        // TRANSFORMENGINE: GetPactCostsAsync — reads vpactprojectyearcosts keyless view
        //   Filtered by project and year (view Year is double; cast to short for comparison)
        //   Returns all monthno rows for the project+year combination so the service/caller
        //   can sum or aggregate as required (e.g. btnUpdateCosting_Click in original VBA)
        public async Task<IReadOnlyList<PactProjectYearCosts>> GetPactCostsAsync(
            string project,
            short year)
        {
            // TRANSFORMENGINE: view Year column is double precision — cast short to double for filter
            double yearAsDouble = (double)year;

            List<PactProjectYearCosts> rows = await _context.PactProjectYearCosts
                .AsNoTracking()
                .Where(v => v.Project == project && v.Year == yearAsDouble)
                .OrderBy(v => v.MonthNo)
                .ToListAsync();

            return rows.AsReadOnly();
        }

        // ─── Private helpers ────────────────────────────────────────────────────────

        // TRANSFORMENGINE: ApplySearch — optional free-text filter across searchable columns
        private static IQueryable<YearlyFinancialData> ApplySearch(
            IQueryable<YearlyFinancialData> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;

            string s = search.ToLower();

            // Search year as string equivalent, project code, costedby username
            return query.Where(e =>
                e.Project.ToLower().Contains(s) ||
                (e.CostedBy != null && e.CostedBy.ToLower().Contains(s)) ||
                (e.AdjustmentComment != null && e.AdjustmentComment.ToLower().Contains(s)));
        }

        // TRANSFORMENGINE: ApplySorting — column-name–driven sort for grid display
        private static IQueryable<YearlyFinancialData> ApplySorting(
            IQueryable<YearlyFinancialData> query,
            string? sortBy,
            bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "year"                => ApplyOrder(query, e => e.Year,               descending),
                "project"             => ApplyOrder(query, e => e.Project,            descending),
                "bfbudget"            => ApplyOrder(query, e => e.BfBudget,           descending),
                "pybudget"            => ApplyOrder(query, e => e.PyBudget,           descending),
                "seedcorn"            => ApplyOrder(query, e => e.Seedcorn,           descending),
                "manhours"            => ApplyOrder(query, e => e.ManHours,           descending),
                "mandays"             => ApplyOrder(query, e => e.ManDays,            descending),
                "manyears"            => ApplyOrder(query, e => e.ManYears,           descending),
                "paycosts"            => ApplyOrder(query, e => e.PayCosts,           descending),
                "nonpayohcosts"       => ApplyOrder(query, e => e.NonPayOhCosts,      descending),
                "testcosts"           => ApplyOrder(query, e => e.TestCosts,          descending),
                "animalcosts"         => ApplyOrder(query, e => e.AnimalCosts,        descending),
                "nonanimalcosts"      => ApplyOrder(query, e => e.NonAnimalCosts,     descending),
                "adjustment"          => ApplyOrder(query, e => e.Adjustment,         descending),
                "actualexpenditure"   => ApplyOrder(query, e => e.ActualExpenditure,  descending),
                "actualmanyears"      => ApplyOrder(query, e => e.ActualManYears,     descending),
                "vlabudget"           => ApplyOrder(query, e => e.VlaBudget,          descending),
                "locked"              => ApplyOrder(query, e => e.Locked,             descending),
                "datecosted"          => ApplyOrder(query, e => e.DateCosted,         descending),
                "costedby"            => ApplyOrder(query, e => e.CostedBy,           descending),
                _                     => query.OrderBy(e => e.Year).ThenBy(e => e.Project)
            };
        }

        private static IQueryable<T> ApplyOrder<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            bool descending)
            => descending
               ? query.OrderByDescending(keySelector)
               : query.OrderBy(keySelector);
    }
}
