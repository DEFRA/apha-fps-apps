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
    public class TimeCostCalcsRepository : BaseRepository, ITimeCostCalcsRepository
    {
        private readonly FpsDbContext _dbContext;

        public TimeCostCalcsRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedData<TimeCostCalcsView>> GetTimeCostCalcsByProjectAsync(
            PaginationParameters<string> query, string projectCode)
        {
            var baseQuery = _dbContext.TimeCostCalcsViews
                .AsNoTracking()
                .Where(x => x.Project == projectCode)
                .Select(x => new TimeCostCalcsView
                {
                    WorkGroup  = x.WorkGroup,
                    GradeCode  = x.GradeCode,
                    JobCode    = x.JobCode,
                    Project    = x.Project,
                    StaffId    = x.StaffId,
                    Name       = x.Name,
                    Month      = x.Month,
                    Time       = x.Time,
                    Cost       = x.Cost,
                    FpsYear    = x.FpsYear,
                })
                .Distinct();

            baseQuery = ApplyFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<TimeCostCalcsView>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<(double TotalHours, double TotalCost)> GetTotalActualByProjectAsync(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return (0, 0);

            var totals = await _dbContext.TimeCostCalcsViews
                .AsNoTracking()
                .Where(x => x.Project == projectCode)
                .Select(x => new TimeCostCalcsView
                {
                    WorkGroup = x.WorkGroup,
                    GradeCode = x.GradeCode,
                    JobCode   = x.JobCode,
                    Project   = x.Project,
                    StaffId   = x.StaffId,
                    Name      = x.Name,
                    Month     = x.Month,
                    Time      = x.Time,
                    Cost      = x.Cost,
                    FpsYear   = x.FpsYear,
                })
                .Distinct()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalHours = g.Sum(x => (double?)x.Time) ?? 0,
                    TotalCost  = g.Sum(x => (double?)x.Cost) ?? 0
                })
                .FirstOrDefaultAsync();

            return totals is null ? (0, 0) : (totals.TotalHours, totals.TotalCost);
        }

        public async Task<bool> DeleteAsync(string workgroup, string jobCode, string project, double month, string staffId)
        {
            if (string.IsNullOrWhiteSpace(workgroup) || string.IsNullOrWhiteSpace(jobCode)
                || string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(staffId))
                return false;

            const double epsilon = 1e-9;
            var entity = await _dbContext.TimeCostCalcs
                .Where(x => x.WorkGroup == workgroup && x.JobCode == jobCode
                         && x.Project == project
                         && x.Month >= month - epsilon && x.Month <= month + epsilon
                         && x.StaffId == staffId)
                .FirstOrDefaultAsync();

            if (entity == null) return false;

            _dbContext.TimeCostCalcs.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private static IQueryable<TimeCostCalcsView> ApplyFilter(IQueryable<TimeCostCalcsView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("WorkGroup", out var workGroup) && workGroup != null)
                query = query.Where(x => EF.Functions.ILike(x.WorkGroup!, $"%{workGroup}%"));

            if (dict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
                query = query.Where(x => EF.Functions.ILike(x.GradeCode!, $"%{gradeCode}%"));

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => EF.Functions.ILike(x.JobCode!, $"%{jobCode}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<TimeCostCalcsView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.WorkGroup).ThenBy(x => x.Month).ThenBy(x => x.StaffId).ThenBy(x => x.JobCode);
            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<TimeCostCalcsView> query, string property, bool descending)
        {
            return property switch
            {
                "workgroup" => ApplyOrder(query, i => i.WorkGroup,  descending),
                "gradecode" => ApplyOrder(query, i => i.GradeCode,  descending),
                "jobcode"   => ApplyOrder(query, i => i.JobCode,    descending),
                "name"      => ApplyOrder(query, i => i.Name,       descending),
                "month"     => ApplyOrder(query, i => i.Month,      descending),
                "time"      => ApplyOrder(query, i => i.Time,       descending),
                "cost"      => ApplyOrder(query, i => i.Cost,       descending),
                _           => query,
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<TimeCostCalcsView> query,
            Expression<Func<TimeCostCalcsView, T>> keySelector, bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
