using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestListVlaRepository : BaseRepository, ITestListVlaRepository
    {
        private readonly FpsDbContext _dbContext;

        public TestListVlaRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        //   filter string is applied as ILike across itemcode and itemdescription
        public async Task<PagedData<TestOrProduct>> GetPagedAsync(
            PaginationParameters<string> query, int fpsYear)
        {
            var q = _dbContext.TestOrProducts
                .AsNoTracking()
                .Where(e => e.FpsYear == fpsYear);

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filter = query.Filter.Trim();
                q = q.Where(e =>
                    EF.Functions.ILike(e.ItemCode, $"%{filter}%") ||
                    (e.ItemDescription != null && EF.Functions.ILike(e.ItemDescription, $"%{filter}%")));
            }

            q = ApplySort(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return base.ApplyPaging(result, query.Page > 0 ? query.Page : 1, query.PageSize > 0 ? query.PageSize : 10);
        }

        public async Task<IEnumerable<TestOrProduct>> GetAllByYearAsync(int fpsYear)
        {
            return await _dbContext.TestOrProducts
                .AsNoTracking()
                .Where(e => e.FpsYear == fpsYear)
                .OrderBy(e => e.ItemCode)
                .ToListAsync();
        }

        public async Task<TestOrProduct?> GetByKeyAsync(string itemCode, int fpsYear)
        {
            return await _dbContext.TestOrProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ItemCode == itemCode && e.FpsYear == fpsYear);
        }

        public async Task<bool> ExistsAsync(string itemCode, int fpsYear)
        {
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

        public async Task<bool> DeleteAsync(string itemCode, int fpsYear)
        {
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

        private static IQueryable<TestOrProduct> ApplySort(
            IQueryable<TestOrProduct> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "itemcode"        => descending ? query.OrderByDescending(e => e.ItemCode)        : query.OrderBy(e => e.ItemCode),
                "itemdescription" => descending ? query.OrderByDescending(e => e.ItemDescription) : query.OrderBy(e => e.ItemDescription),
                "testmanager"     => descending ? query.OrderByDescending(e => e.TestManager)     : query.OrderBy(e => e.TestManager),
                "jobstatus"       => descending ? query.OrderByDescending(e => e.JobStatus)       : query.OrderBy(e => e.JobStatus),
                "owner"           => descending ? query.OrderByDescending(e => e.Owner)           : query.OrderBy(e => e.Owner),
                "unitpricevla"    => descending ? query.OrderByDescending(e => e.UnitPriceVla)    : query.OrderBy(e => e.UnitPriceVla),
                "defraunitprice"  => descending ? query.OrderByDescending(e => e.DefraUnitPrice)  : query.OrderBy(e => e.DefraUnitPrice),
                _                 => query.OrderBy(e => e.ItemCode),
            };
        }
    }
}
