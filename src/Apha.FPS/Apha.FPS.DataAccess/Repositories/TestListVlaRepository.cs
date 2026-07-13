using System.Dynamic;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestListVlaRepository : BaseRepository, ITestListVlaRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public TestListVlaRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        //   filter string is applied as ILike across itemcode and itemdescription
        public async Task<PagedData<TestOrProduct>> GetPagedAsync(PaginationParameters<string> query)
        {
            var fpsYear = _requestContext.FpsYear;

            var q = _dbContext.TestOrProducts
                .AsNoTracking()
                .Where(e => e.FpsYear == fpsYear);

            q = ApplyFilter(q, query.Filter);
            q = ApplySort(q, query.SortBy, query.Descending);

            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Max(query.PageSize, 10);

            var result = await q.ToListAsync();
            return base.ApplyPaging(result, page, pageSize);
        }

        public async Task<IEnumerable<TestOrProduct>> GetAllByYearAsync()
        {
            var fpsYear = _requestContext.FpsYear;

            return await _dbContext.TestOrProducts
                .AsNoTracking()
                .Where(e => e.FpsYear == fpsYear)
                .OrderBy(e => e.ItemCode)
                .ToListAsync();
        }

        public async Task<TestOrProduct?> GetByKeyAsync(string itemCode)
        {
            var fpsYear = _requestContext.FpsYear;

            return await _dbContext.TestOrProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ItemCode == itemCode && e.FpsYear == fpsYear);
        }

        public async Task<bool> ExistsAsync(string itemCode)
        {
            var fpsYear = _requestContext.FpsYear;

            return await _dbContext.TestOrProducts
                .AnyAsync(e => e.ItemCode == itemCode && e.FpsYear == fpsYear);
        }

        public async Task<TestOrProduct> AddAsync(TestOrProduct testOrProduct)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestOrProducts.Add(testOrProduct);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return testOrProduct;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<TestOrProduct> UpdateAsync(TestOrProduct testOrProduct)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.TestOrProducts
                        .FirstOrDefaultAsync(e => e.ItemCode == testOrProduct.ItemCode
                                               && e.FpsYear == testOrProduct.FpsYear);

                    if (existing == null)
                        throw new KeyNotFoundException(
                            $"TestOrProduct not found: ItemCode='{testOrProduct.ItemCode}', FpsYear={testOrProduct.FpsYear}");

                    existing.ItemDescription = testOrProduct.ItemDescription;
                    existing.TestManager     = testOrProduct.TestManager;
                    existing.JobStatus       = testOrProduct.JobStatus;
                    existing.UnitPriceVla    = testOrProduct.UnitPriceVla;
                    existing.PriceAhvg       = testOrProduct.PriceAhvg;
                    existing.Owner           = testOrProduct.Owner;
                    existing.ChargeMethod    = testOrProduct.ChargeMethod;
                    existing.ShortDescription = testOrProduct.ShortDescription;
                    existing.DefraUnitPrice  = testOrProduct.DefraUnitPrice;

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

        public async Task<bool> DeleteAsync(string itemCode)
        {
            var fpsYear = _requestContext.FpsYear;

            var entity = await _dbContext.TestOrProducts
                .FirstOrDefaultAsync(e => e.ItemCode == itemCode && e.FpsYear == fpsYear);

            if (entity == null)
                return false;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestOrProducts.Remove(entity);
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

        private static IQueryable<TestOrProduct> ApplyFilter(IQueryable<TestOrProduct> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            // Backward-compatible global search (non-JSON callers)
            if (!filter.TrimStart().StartsWith('{'))
            {
                var term = filter.Trim();
                return query.Where(e =>
                    EF.Functions.ILike(e.ItemCode, $"%{term}%") ||
                    (e.ItemDescription != null && EF.Functions.ILike(e.ItemDescription, $"%{term}%")));
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            return ApplyJsonFilter(query, (IDictionary<string, object>)filterModel);
        }

        private static IQueryable<TestOrProduct> ApplyJsonFilter(IQueryable<TestOrProduct> query, IDictionary<string, object> dict)
        {
            query = ApplyStringFilters(query, dict);
            query = ApplyDecimalFilters(query, dict);
            return query;
        }

        private static IQueryable<TestOrProduct> ApplyStringFilters(IQueryable<TestOrProduct> query, IDictionary<string, object> dict)
        {
            if (dict.TryGetValue("ItemCode", out var itemCode) && itemCode != null)
                query = query.Where(x => EF.Functions.ILike(x.ItemCode, $"%{itemCode}%"));

            if (dict.TryGetValue("ItemDescription", out var itemDescription) && itemDescription != null)
                query = query.Where(x => x.ItemDescription != null && EF.Functions.ILike(x.ItemDescription, $"%{itemDescription}%"));

            if (dict.TryGetValue("ShortDescription", out var shortDescription) && shortDescription != null)
                query = query.Where(x => x.ShortDescription != null && EF.Functions.ILike(x.ShortDescription, $"%{shortDescription}%"));

            if (dict.TryGetValue("TestManager", out var testManager) && testManager != null)
                query = query.Where(x => x.TestManager != null && EF.Functions.ILike(x.TestManager, $"%{testManager}%"));

            if (dict.TryGetValue("JobStatus", out var jobStatus) && jobStatus != null)
                query = query.Where(x => x.JobStatus != null && EF.Functions.ILike(x.JobStatus, $"%{jobStatus}%"));

            if (dict.TryGetValue("Owner", out var owner) && owner != null)
                query = query.Where(x => x.Owner != null && EF.Functions.ILike(x.Owner, $"%{owner}%"));

            return query;
        }

        private static IQueryable<TestOrProduct> ApplyDecimalFilters(IQueryable<TestOrProduct> query, IDictionary<string, object> dict)
        {
            if (dict.TryGetValue("UnitPriceVla", out var unitPriceVla) && unitPriceVla != null && decimal.TryParse(unitPriceVla.ToString(), out var vlaPrice))
                query = query.Where(x => x.UnitPriceVla == vlaPrice);

            if (dict.TryGetValue("DefraUnitPrice", out var defraUnitPrice) && defraUnitPrice != null && decimal.TryParse(defraUnitPrice.ToString(), out var defraPrice))
                query = query.Where(x => x.DefraUnitPrice == defraPrice);

            return query;
        }

        private static IQueryable<TestOrProduct> ApplySort(
            IQueryable<TestOrProduct> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "itemcode"         => Order(query, e => e.ItemCode, descending),
                "itemdescription"  => Order(query, e => e.ItemDescription, descending),
                "shortdescription" => Order(query, e => e.ShortDescription, descending),
                "testmanager"      => Order(query, e => e.TestManager, descending),
                "jobstatus"        => Order(query, e => e.JobStatus, descending),
                "owner"            => Order(query, e => e.Owner, descending),
                "unitpricevla"     => Order(query, e => e.UnitPriceVla, descending),
                "defraunitprice"   => Order(query, e => e.DefraUnitPrice, descending),
                _                  => query.OrderBy(e => e.ItemCode),
            };
        }

        private static IQueryable<TestOrProduct> Order<TKey>(
            IQueryable<TestOrProduct> query,
            System.Linq.Expressions.Expression<Func<TestOrProduct, TKey>> keySelector,
            bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
