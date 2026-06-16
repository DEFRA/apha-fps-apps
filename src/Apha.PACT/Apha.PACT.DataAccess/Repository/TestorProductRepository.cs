using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

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

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
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

        public async Task<Dictionary<string, string?>> GetDescriptionsByCodesAsync(IEnumerable<string> itemCodes)
        {
            var codes = itemCodes.ToList();
            return await _context.TestorProducts
                .AsNoTracking()
                .Where(t => codes.Contains(t.ItemCode))
                .ToDictionaryAsync(t => t.ItemCode, t => t.ItemDescription);
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

            query = ApplyILikeFilter(dict, "ItemCode",        query, (q, v) => q.Where(x => EF.Functions.ILike(x.ItemCode, v)));
            query = ApplyILikeFilter(dict, "ItemDescription", query, (q, v) => q.Where(x => x.ItemDescription != null && EF.Functions.ILike(x.ItemDescription, v)));
            query = ApplyILikeFilter(dict, "ShortDescription",query, (q, v) => q.Where(x => x.ShortDescription != null && EF.Functions.ILike(x.ShortDescription, v)));
            query = ApplyILikeFilter(dict, "Owner",           query, (q, v) => q.Where(x => x.Owner != null && EF.Functions.ILike(x.Owner, v)));
            query = ApplyILikeFilter(dict, "TestManager",     query, (q, v) => q.Where(x => x.TestManager != null && EF.Functions.ILike(x.TestManager, v)));

            return query;
        }

        private static IQueryable<TestorProduct> ApplyILikeFilter(
            IDictionary<string, object> dict,
            string key,
            IQueryable<TestorProduct> query,
            Func<IQueryable<TestorProduct>, string, IQueryable<TestorProduct>> applyWhere)
        {
            if (dict.TryGetValue(key, out var value) && value != null)
                query = applyWhere(query, $"%{value}%");
            return query;
        }

        private static IQueryable<TestorProduct> ApplySorting(IQueryable<TestorProduct> query, string? sortBy, bool descending)
        {
            var sortMap = new Dictionary<string, Expression<Func<TestorProduct, object?>>>
            {
                ["itemcode"]        = e => e.ItemCode,
                ["itemdescription"] = e => e.ItemDescription,
                ["shortdescription"]= e => e.ShortDescription,
                ["owner"]           = e => e.Owner,
                ["testmanager"]     = e => e.TestManager,
                ["unitpricevla"]    = e => e.UnitPriceVla,
                ["defraunitprice"]  = e => e.DefraUnitPrice,
            };

            var key = sortBy?.ToLower() ?? string.Empty;
            if (!sortMap.TryGetValue(key, out var keySelector))
                keySelector = e => e.ItemCode;

            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
