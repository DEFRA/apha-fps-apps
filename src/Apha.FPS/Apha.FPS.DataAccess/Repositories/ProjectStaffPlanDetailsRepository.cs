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
    public class ProjectStaffPlanDetailsRepository : BaseRepository, IProjectStaffPlanDetailsRepository
    {
        public ProjectStaffPlanDetailsRepository(FpsDbContext context) : base(context) { }

        public async Task<PagedData<ProjectStaffPlanDetailsView>> GetPagedAsync(PaginationParameters<string> query)
        {
            // FpsYear is applied automatically by the global query filter on the view.
            var baseQuery = _context.ProjectStaffPlanDetailsViews.AsNoTracking();

            baseQuery = ApplyFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<ProjectStaffPlanDetailsView>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        private static IQueryable<ProjectStaffPlanDetailsView> ApplyFilter(IQueryable<ProjectStaffPlanDetailsView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProfitCentre", out var profitCentre) && profitCentre != null)
                query = query.Where(x => EF.Functions.ILike(x.ProfitCentre!, $"%{profitCentre}%"));

            if (dict.TryGetValue("Program", out var program) && program != null)
                query = query.Where(x => EF.Functions.ILike(x.Program!, $"%{program}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
                query = query.Where(x => EF.Functions.ILike(x.Manager!, $"%{manager}%"));

            if (dict.TryGetValue("ProjectStatus", out var projectStatus) && projectStatus != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectStatus!, $"%{projectStatus}%"));

            if (dict.TryGetValue("WorkGroup", out var workGroup) && workGroup != null)
                query = query.Where(x => EF.Functions.ILike(x.WorkGroup!, $"%{workGroup}%"));

            if (dict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
                query = query.Where(x => EF.Functions.ILike(x.GradeCode!, $"%{gradeCode}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<ProjectStaffPlanDetailsView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.ProfitCentre).ThenBy(x => x.WorkGroup).ThenBy(x => x.Program);
            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<ProjectStaffPlanDetailsView> query, string property, bool descending)
        {
            return property switch
            {
                "program"       => ApplyOrder(query, x => x.Program,       descending),
                "name"          => ApplyOrder(query, x => x.Name,          descending),
                "manager"       => ApplyOrder(query, x => x.Manager,       descending),
                "projectstatus" => ApplyOrder(query, x => x.ProjectStatus, descending),
                "profitcentre"  => ApplyOrder(query, x => x.ProfitCentre,  descending),
                "workgroup"     => ApplyOrder(query, x => x.WorkGroup,     descending),
                "gradecode"     => ApplyOrder(query, x => x.GradeCode,     descending),
                "plannedhours"  => ApplyOrder(query, x => x.PlannedHours,  descending),
                "cost"          => ApplyOrder(query, x => x.Cost,          descending),
                _               => query.OrderBy(x => x.ProfitCentre).ThenBy(x => x.WorkGroup).ThenBy(x => x.Program)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<ProjectStaffPlanDetailsView> query,
            Expression<Func<ProjectStaffPlanDetailsView, T>> keySelector, bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
