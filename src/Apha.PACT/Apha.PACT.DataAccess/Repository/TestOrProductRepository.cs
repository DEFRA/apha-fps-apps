using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.PACT.DataAccess.Repository
{
    public class TestorProductRepository : BaseRepository, ITestorProductRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly FpsDbContext _dbContext;

        public TestorProductRepository(FpsDbContext dbContext, IFpsRequestContext fpsRequestContext) : base(dbContext)
        {
            _fpsRequestContext = fpsRequestContext;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TestorProduct>> GetAllTestorProductsAsync()
        {
            return await _dbContext.TestorProducts
                .AsNoTracking()
                .OrderBy(t => t.ItemCode)
                .ToListAsync();
        }

        public async Task<PagedData<TestorProduct>> GetPagedTestOrProductsAsync(PaginationParameters<string> parameters)
        {
            var query = _context.TestorProducts.AsNoTracking().AsQueryable();

            query = ApplyTestOrProductFilter(query, parameters.Filter);
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            var result = await query.ToListAsync();
            return ApplyPaging(result, parameters.Page, parameters.PageSize);
        }

        public async Task<TestorProduct?> GetTestOrProductByIdAsync(string itemCode)
        {
            return await _context.TestorProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ItemCode == itemCode);
        }

        public async Task<TestorProduct> CreateTestOrProductAsync(TestorProduct entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            await _context.TestorProducts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TestorProduct> UpdateTestOrProductAsync(TestorProduct entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteTestOrProductAsync(string itemCode)
        {
            var entity = await _context.TestorProducts  
                .FirstOrDefaultAsync(t => t.ItemCode == itemCode && t.FpsYear == _fpsRequestContext.FpsYear);
            if (entity == null) return false;
            _context.TestorProducts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<string>> GetOwnersAsync()
        {
            return await _context.TestorProducts
                .AsNoTracking()
                .Where(t => t.Owner != null)
                .Select(t => t.Owner!)
                .Distinct()
                .OrderBy(o => o)
                .ToListAsync();
        }

        private static IQueryable<TestorProduct> ApplyTestOrProductFilter(IQueryable<TestorProduct> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return query;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return query;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ItemCode", out var itemCode) && itemCode != null)
            {
                query = query.Where(x => x.ItemCode.Contains(itemCode.ToString()!));
            }

            if (dict.TryGetValue("ItemDescription", out var itemDescription) && itemDescription != null)
            {
                query = query.Where(x => x.ItemDescription!.Contains(itemDescription.ToString()!));
            }

            if (dict.TryGetValue("ShortDescription", out var shortDescription) && shortDescription != null)
            {
                query = query.Where(x => x.ShortDescription!.Contains(shortDescription.ToString()!));
            }

            if (dict.TryGetValue("Owner", out var owner) && owner != null)
            {
                query = query.Where(x => x.Owner!.Contains(owner.ToString()!));
            }

            if (dict.TryGetValue("TestManager", out var testManager) && testManager != null)
            {
                query = query.Where(x => x.TestManager!.Contains(testManager.ToString()!));
            }

            return query;
        }

        private static IQueryable<TestorProduct> ApplySorting(IQueryable<TestorProduct> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.ItemCode);
            }

            return sortBy.ToLower() switch
            {
                "itemcode" => descending ? query.OrderByDescending(e => e.ItemCode) : query.OrderBy(e => e.ItemCode),
                "itemdescription" => descending ? query.OrderByDescending(e => e.ItemDescription) : query.OrderBy(e => e.ItemDescription),
                "shortdescription" => descending ? query.OrderByDescending(e => e.ShortDescription) : query.OrderBy(e => e.ShortDescription),
                "owner" => descending ? query.OrderByDescending(e => e.Owner) : query.OrderBy(e => e.Owner),
                "testmanager" => descending ? query.OrderByDescending(e => e.TestManager) : query.OrderBy(e => e.TestManager),
                "unitpricevla" => descending ? query.OrderByDescending(e => e.UnitPriceVla) : query.OrderBy(e => e.UnitPriceVla),
                "defraunitprice" => descending ? query.OrderByDescending(e => e.DefraUnitPrice) : query.OrderBy(e => e.DefraUnitPrice),
                _ => query.OrderBy(e => e.ItemCode),
            };
        }
    }
}
