using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class TestActualBreakdownRepository : BaseRepository, ITestActualBreakdownRepository
    {
        public TestActualBreakdownRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<PagedData<TestActualBreakdownView>> GetPagedAsync(PaginationParameters<string> query)
        {
            var baseQuery = _context.TestActualBreakdownViews.AsNoTracking();

            baseQuery = ApplyFilter(baseQuery, query.Filter);
            var sorted = ApplySorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPaging(sorted, query.Page, query.PageSize);
        }

        private static IQueryable<TestActualBreakdownView> ApplySorting(
            IQueryable<TestActualBreakdownView> source, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "testcode"         => ApplyOrder(source, x => x.TestCode,         descending),
                "shortdescription" => ApplyOrder(source, x => x.ShortDescription, descending),
                "program"          => ApplyOrder(source, x => x.Program,          descending),
                "buyer"            => ApplyOrder(source, x => x.Buyer,            descending),
                "portfolio"        => ApplyOrder(source, x => x.Portfolio,        descending),
                "workgroup"        => ApplyOrder(source, x => x.WorkGroup,        descending),
                "month"            => ApplyOrder(source, x => x.Month,            descending),
                "pcprice"          => ApplyOrder(source, x => x.PCPrice,          descending),
                "pccost"           => ApplyOrder(source, x => x.PCCost,           descending),
                "profitcentre"     => ApplyOrder(source, x => x.ProfitCentre,     descending),
                _                  => ApplyOrder(source, x => x.TestCode,         descending)
            };
        }

        private static IQueryable<TestActualBreakdownView> ApplyFilter(
            IQueryable<TestActualBreakdownView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter);
            if (filters is null)
                return query;

            if (filters.TryGetValue("TestCode", out var testCode) && !string.IsNullOrWhiteSpace(testCode))
                query = query.Where(x => EF.Functions.ILike(x.TestCode, $"%{testCode}%"));
            if (filters.TryGetValue("ShortDescription", out var shortDesc) && !string.IsNullOrWhiteSpace(shortDesc))
                query = query.Where(x => x.ShortDescription != null && EF.Functions.ILike(x.ShortDescription, $"%{shortDesc}%"));
            if (filters.TryGetValue("Program", out var program) && !string.IsNullOrWhiteSpace(program))
                query = query.Where(x => x.Program != null && EF.Functions.ILike(x.Program, $"%{program}%"));
            if (filters.TryGetValue("Buyer", out var buyer) && !string.IsNullOrWhiteSpace(buyer))
                query = query.Where(x => EF.Functions.ILike(x.Buyer, $"%{buyer}%"));
            if (filters.TryGetValue("Portfolio", out var portfolio) && !string.IsNullOrWhiteSpace(portfolio))
                query = query.Where(x => x.Portfolio != null && EF.Functions.ILike(x.Portfolio, $"%{portfolio}%"));
            if (filters.TryGetValue("WorkGroup", out var workGroup) && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, $"%{workGroup}%"));
            if (filters.TryGetValue("ProfitCentre", out var profitCentre) && !string.IsNullOrWhiteSpace(profitCentre))
                query = query.Where(x => x.ProfitCentre != null && EF.Functions.ILike(x.ProfitCentre, $"%{profitCentre}%"));

            return query;
        }

        private static IQueryable<T> ApplyOrder<T, TKey>(
            IQueryable<T> source, System.Linq.Expressions.Expression<Func<T, TKey>> keySelector, bool descending)
            => descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
    }
}
