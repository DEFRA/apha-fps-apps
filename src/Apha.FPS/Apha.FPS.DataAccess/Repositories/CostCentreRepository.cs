/*
 * TRANSFORMENGINE MIGRATION — CostCentreRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - MS Access frmMaintCostCentres CRUD (saveTblCostCentre, updateTblCostCentre, handleTblCostCentreDelete)
 *     → LINQ-first EF Core repository implementing ICostCentreRepository
 *   - RecordSource "SELECT CostCentre.CostCentre, CostCentre.ProfitCentre FROM CostCentre ORDER BY CostCentre.CostCentre"
 *     → GetAllPagedAsync: AsNoTracking LINQ query ordered by CostCentreNo, with optional filter/sort/paging
 *   - Single-record lookup for Edit modal → GetByIdAsync(costCentreNo, fpsYear) via composite key
 *   - saveTblCostCentre() → CreateAsync: adds entity, saves, returns persisted entity
 *   - updateTblCostCentre() → UpdateAsync: fetches tracked entity, updates fields, saves, returns updated entity
 *   - handleTblCostCentreDelete() → DeleteAsync: fetches tracked entity, removes, saves, returns bool
 *   - Duplicate-key guard → ExistsAsync using AnyAsync
 *   - Filter/sort helpers preserved as private static methods (ApplyCostCentreFilter, ApplyCostCentreSorting)
 *   - IFpsRequestContext injected for year scoping (aligns with DbContext HasQueryFilter)
 *   - No stored procedures referenced for this form — pure LINQ operations
 *
 * PRESERVED:
 *   - Composite PK semantics (CostCentreNo, FpsYear) in all key-based methods
 *   - AsNoTracking for read-only queries; tracked entities for write operations
 *   - ExecutionStrategy + transaction pattern for write operations (consistent with ProfitCentreRepository)
 *   - All public ICostCentreRepository method signatures unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: fps.costcentre is partitioned by fpsyear. EF Core routes writes to partitions
 *     automatically via the parent table. Verify at integration time that SaveChangesAsync succeeds for
 *     the active year partition (e.g. costcentre_y2026).
 *   - TRANSFORMENGINE TODO: No FK child-reference guard is implemented for DeleteAsync (e.g. if other tables
 *     reference this cost centre). Add a HasLinkedXxxAsync guard if required by business rules.
 */

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.FPS.DataAccess.Repositories
{
    // TRANSFORMENGINE: frmMaintCostCentres → ICostCentreRepository LINQ implementation against fps.costcentre
    public class CostCentreRepository : BaseRepository, ICostCentreRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public CostCentreRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        // TRANSFORMENGINE: GET paged — drives DataGrid in frmMaintCostCentres / fps_costcenter_maintenance.html (#gridContainer_costcenterList)
        // Source RecordSource: SELECT CostCentre.CostCentre, CostCentre.ProfitCentre FROM CostCentre ORDER BY CostCentre.CostCentre
        public async Task<PagedData<CostCentre>> GetAllPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var costCentresQuery = _dbContext.CostCentres
                .AsNoTracking()
                .AsQueryable();

            costCentresQuery = ApplyCostCentreFilter(costCentresQuery, query.Filter);
            costCentresQuery = ApplyCostCentreSorting(costCentresQuery, query.SortBy, query.Descending);

            var costCentres = await costCentresQuery.ToListAsync();
            return ApplyPaging(costCentres, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: GET by composite key — populates Edit modal fields (modal-cc-number, modal-cc-profit)
        public async Task<CostCentre?> GetByIdAsync(double costCentreNo, int fpsYear)
        {
            return await _dbContext.CostCentres
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CostCentreNo == costCentreNo && c.FpsYear == fpsYear);
        }

        // TRANSFORMENGINE: POST create — maps to saveTblCostCentre() in costcenter_maintenance.js
        public async Task<CostCentre> CreateAsync(CostCentre entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.CostCentres.Add(entity);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return entity;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // TRANSFORMENGINE: PUT update — maps to updateTblCostCentre() in costcenter_maintenance.js
        // originalCostCentreNo identifies the row to update; entity carries the new values
        public async Task<CostCentre> UpdateAsync(double originalCostCentreNo, int fpsYear, CostCentre entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // TRANSFORMENGINE: Fetch tracked entity for update; IgnoreQueryFilters not needed — year filter matches
                    var existing = await _dbContext.CostCentres
                        .FirstOrDefaultAsync(c => c.CostCentreNo == originalCostCentreNo && c.FpsYear == fpsYear);

                    if (existing == null)
                        return entity;

                    // TRANSFORMENGINE: Update mutable fields — CostCentreNo and FpsYear are composite PK, ProfitCentre is the only editable field
                    existing.ProfitCentre = entity.ProfitCentre;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existing;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // TRANSFORMENGINE: DELETE — maps to handleTblCostCentreDelete() in costcenter_maintenance.js
        public async Task<bool> DeleteAsync(double costCentreNo, int fpsYear)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.CostCentres
                        .FirstOrDefaultAsync(c => c.CostCentreNo == costCentreNo && c.FpsYear == fpsYear);

                    if (existing == null)
                        return false;

                    _dbContext.CostCentres.Remove(existing);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // TRANSFORMENGINE: Existence check — prevents duplicate-key insert at the service layer before persisting
        public async Task<bool> ExistsAsync(double costCentreNo, int fpsYear)
        {
            return await _dbContext.CostCentres
                .AsNoTracking()
                .AnyAsync(c => c.CostCentreNo == costCentreNo && c.FpsYear == fpsYear);
        }

        // TRANSFORMENGINE: Filter helper — translates JSON filter model to LINQ predicates (CostCentreNo, ProfitCentre)
        private static IQueryable<CostCentre> ApplyCostCentreFilter(IQueryable<CostCentre> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("CostCentreNo", out var costCentreNo) && costCentreNo != null)
            {
                var filterValue = costCentreNo.ToString();
                if (!string.IsNullOrWhiteSpace(filterValue) && double.TryParse(filterValue, out var parsed))
                    query = query.Where(c => c.CostCentreNo == parsed);
            }

            if (dict.TryGetValue("ProfitCentre", out var profitCentre) && profitCentre != null)
            {
                var filterValue = profitCentre.ToString();
                if (!string.IsNullOrWhiteSpace(filterValue))
                    query = query.Where(c => c.ProfitCentre.Contains(filterValue));
            }

            return query;
        }

        // TRANSFORMENGINE: Sort helper — maps SortBy field name to EF Core OrderBy/OrderByDescending
        private static IQueryable<CostCentre> ApplyCostCentreSorting(IQueryable<CostCentre> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                // TRANSFORMENGINE: Default sort matches source RecordSource: ORDER BY CostCentre.CostCentre
                return query.OrderBy(c => c.CostCentreNo);

            return sortBy switch
            {
                "CostCentreNo" => descending
                    ? query.OrderByDescending(c => c.CostCentreNo)
                    : query.OrderBy(c => c.CostCentreNo),
                "ProfitCentre" => descending
                    ? query.OrderByDescending(c => c.ProfitCentre)
                    : query.OrderBy(c => c.ProfitCentre),
                _ => descending
                    ? query.OrderByDescending(c => c.CostCentreNo)
                    : query.OrderBy(c => c.CostCentreNo),
            };
        }
    }
}
