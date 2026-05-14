using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class YearMasterRepository : BaseRepository, IYearMasterRepository
    {
        private readonly FpsDbContext _dbContext;

        public YearMasterRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<YearMaster>> GetAllFpsYearsAsync()
        {
            return await _dbContext.YearMasters
                .AsNoTracking()
                .Where(y => y.Active)
                .OrderByDescending(y => y.FpsYear)
                .ToListAsync();
        }

        public async Task<PagedData<YearMaster>> GetAllFpsYearsPagedAsync(PaginationParameters<int> query)
        {
            var yearMasterQuery = _dbContext.YearMasters
                .AsNoTracking()
                .Where(y => y.Active)
                .AsQueryable();

            yearMasterQuery = ApplyYearMasterFilter(yearMasterQuery, query.Filter);
            yearMasterQuery = (IQueryable<YearMaster>)ApplySorting(yearMasterQuery, query.SortBy, query.Descending);

            var result = await yearMasterQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<YearMaster?> GetFpsYearByIdAsync(int fpsYear)
        {
            return await _dbContext.YearMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(y => y.FpsYear == fpsYear);
        }

        private IQueryable<YearMaster> ApplyYearMasterFilter(IQueryable<YearMaster> query, int? filter)
        {
            if (filter.HasValue && filter.Value > 0)
            {
                query = query.Where(y => y.FpsYear == filter.Value);
            }
            return query;
        }

        private IQueryable<T> ApplySorting<T>(IQueryable<T> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, sortBy);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = descending ? "OrderByDescending" : "OrderBy";
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), property.Type },
                query.Expression,
                Expression.Quote(lambda));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
