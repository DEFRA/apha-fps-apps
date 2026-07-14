using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class TestCapabilityRepository : BaseRepository, ITestCapabilityRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public TestCapabilityRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public async Task<PagedData<TestCapability>> GetPagedByWorkGroupAsync(
            PaginationParameters<string> query, string? workGroup)
        {
            var baseQuery = _context.TestCapabilities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(t => t.WorkGroup == workGroup);

            baseQuery = ApplyTestCapabilityFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.TestCode);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<PagedData<TestCapability>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string? testCode)
         {
            var baseQuery = _context.TestCapabilities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(testCode))
                baseQuery = baseQuery.Where(t => t.TestCode == testCode);

            baseQuery = ApplyTestCapabilityFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.TestCode);
            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<PagedData<TestCapability>> GetPagedTestCapabilityByPortfolioAsync(
            PaginationParameters<string> query, string? portfolio)
        {
            var baseQuery = _context.TestCapabilities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(portfolio))
                baseQuery = baseQuery.Where(t => t.PlanPortfolio == portfolio);

            baseQuery = ApplyTestCapabilityFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy) && query.SortBy != "ItemDescription")
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.TestCode);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<TestCapability?> GetByIdAsync(string testCode, string workGroup)
        {
            return await _context.TestCapabilities
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TestCode == testCode && t.WorkGroup == workGroup);
        }

        public async Task<TestCapability> AddAsync(TestCapability entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            await _context.TestCapabilities.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TestCapability> UpdateAsync(TestCapability entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(string testCode, string workGroup)
        {
            var entity = await _context.TestCapabilities
                .FirstOrDefaultAsync(t =>
                    t.TestCode == testCode &&
                    t.WorkGroup == workGroup);

            if (entity is null) return false;

            _context.TestCapabilities.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string testCode, string portfolio)
        {
            return await _context.TestCapabilities
                .AsNoTracking()
                .AnyAsync(t => t.TestCode == testCode && t.PlanPortfolio == portfolio);
        }

        public async Task<PagedData<WgTestCapabilitiesWithDescription>> GetPagedWgTestCapabilitiesWithDescriptionAsync(PaginationParameters<string> query, string workGroup)
        {
            var baseQuery = _context.TestCapabilities.AsNoTracking()
                .Where(testCapability => testCapability.WorkGroup == workGroup)
                .Join(_context.TestorProducts.AsNoTracking(),
                    testCapability => testCapability.TestCode,
                    testProduct => testProduct.ItemCode,
                    (testCapability, testProduct) => new WgTestCapabilitiesWithDescription
                    {
                        WorkGroup = testCapability.WorkGroup,
                        TestCode = testCapability.TestCode,
                        ItemDescription = testProduct.ItemDescription
                    })
                .Distinct()
                .AsQueryable();

            baseQuery = ApplyUserTestCapabilityFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.TestCode);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        private static IQueryable<TestCapability> ApplyTestCapabilityFilter(
            IQueryable<TestCapability> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("TestCode", out string? testCode) && !string.IsNullOrWhiteSpace(testCode))
                query = query.Where(t => EF.Functions.ILike(t.TestCode, $"%{testCode}%"));

            if (filters.TryGetValue("WorkGroup", out string? workGroup) && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(t => EF.Functions.ILike(t.WorkGroup, $"%{workGroup}%"));

            if (filters.TryGetValue("PlanPortfolio", out string? portfolio) && !string.IsNullOrWhiteSpace(portfolio))
                query = query.Where(t => EF.Functions.ILike(t.PlanPortfolio, $"%{portfolio}%"));

            return query;
        }

        private static IQueryable<WgTestCapabilitiesWithDescription> ApplyUserTestCapabilityFilter(
            IQueryable<WgTestCapabilitiesWithDescription> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue(nameof(WgTestCapabilitiesWithDescription.WorkGroup), out var workGroup) && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(t => EF.Functions.ILike(t.WorkGroup!, $"%{workGroup}%"));

            if (filters.TryGetValue(nameof(WgTestCapabilitiesWithDescription.TestCode), out var testCode) && !string.IsNullOrWhiteSpace(testCode))
                query = query.Where(t => EF.Functions.ILike(t.TestCode!, $"%{testCode}%"));

            if (filters.TryGetValue(nameof(WgTestCapabilitiesWithDescription.ItemDescription), out var itemDescription) && !string.IsNullOrWhiteSpace(itemDescription))
                query = query.Where(t => EF.Functions.ILike(t.ItemDescription!, $"%{itemDescription}%"));

            return query;
        }
    }
}
