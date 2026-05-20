using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class MonthlyOutputRepository : BaseRepository, IMonthlyOutputRepository
    {
        private readonly FpsDbContext _dbContext;

        public MonthlyOutputRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedData<MonthlyOutput>> GetByProjectAsync(
            PaginationParameters<string> query, string projectCode)
        {
            var baseQuery = _dbContext.MonthlyOutputs
                .AsNoTracking()
                .Where(x => x.Buyer == projectCode);

            baseQuery = ApplyFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<MonthlyOutput>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<double> GetTotalActualByProjectAsync(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return 0;

            return await _dbContext.MonthlyOutputs
                .AsNoTracking()
                .Where(x => x.Buyer == projectCode)
                .SumAsync(x => x.Volume ?? 0);
        }

        public async Task<bool> DeleteAsync(string buyer, string testCode, double month, string workGroup)
        {
            var entity = await _dbContext.MonthlyOutputs
                .FirstOrDefaultAsync(m =>
                    m.Buyer     == buyer     &&
                    m.TestCode  == testCode  &&
                    m.Month     == month     &&
                    m.WorkGroup == workGroup);

            if (entity is null) return false;

            _dbContext.MonthlyOutputs.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private static IQueryable<MonthlyOutput> ApplyFilter(IQueryable<MonthlyOutput> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("TestCode", out var testCode) && testCode != null)
                query = query.Where(x => EF.Functions.ILike(x.TestCode!, $"%{testCode}%"));

            if (dict.TryGetValue("WorkGroup", out var workGroup) && workGroup != null)
                query = query.Where(x => EF.Functions.ILike(x.WorkGroup!, $"%{workGroup}%"));

            if (dict.TryGetValue("Month", out var month) && month != null
                && double.TryParse(month.ToString(), out var monthVal))
                query = query.Where(x => x.Month == monthVal);

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<MonthlyOutput> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.TestCode).ThenBy(x => x.Month).ThenBy(x => x.WorkGroup);
            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<MonthlyOutput> query, string property, bool descending)
        {
            return property switch
            {
                "testcode"  => ApplyOrder(query, x => x.TestCode,  descending),
                "workgroup" => ApplyOrder(query, x => x.WorkGroup,  descending),
                "month"     => ApplyOrder(query, x => x.Month,      descending),
                "volume"    => ApplyOrder(query, x => x.Volume,     descending),
                _           => query,
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<MonthlyOutput> query,
            Expression<Func<MonthlyOutput, T>> keySelector, bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
