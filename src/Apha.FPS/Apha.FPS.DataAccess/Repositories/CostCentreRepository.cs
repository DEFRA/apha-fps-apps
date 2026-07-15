using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.FPS.DataAccess.Repositories
{
    public class CostCentreRepository : BaseRepository, ICostCentreRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public CostCentreRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

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

        public async Task<CostCentre?> GetByIdAsync(double costCentreNo, int fpsYear)
        {
            return await _dbContext.CostCentres
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CostCentreNo == costCentreNo && c.FpsYear == fpsYear);
        }

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
                    var existing = await _dbContext.CostCentres
                        .FirstOrDefaultAsync(c => c.CostCentreNo == originalCostCentreNo && c.FpsYear == fpsYear);

                    if (existing == null)
                        return entity;

                    // CostCentreNo is part of the composite PK (costcentre, fpsyear);
                    // changing it requires delete-old + insert-new rather than an in-place update.
                    if (existing.CostCentreNo != entity.CostCentreNo)
                    {
                        entity.FpsYear = fpsYear;

                        _dbContext.CostCentres.Remove(existing);
                        await _dbContext.SaveChangesAsync();

                        _dbContext.CostCentres.Add(entity);
                        await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return entity;
                    }

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

        public async Task<bool> ExistsAsync(double costCentreNo, int fpsYear)
        {
            return await _dbContext.CostCentres
                .AsNoTracking()
                .AnyAsync(c => c.CostCentreNo == costCentreNo && c.FpsYear == fpsYear);
        }

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

        private static IQueryable<CostCentre> ApplyCostCentreSorting(IQueryable<CostCentre> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
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
