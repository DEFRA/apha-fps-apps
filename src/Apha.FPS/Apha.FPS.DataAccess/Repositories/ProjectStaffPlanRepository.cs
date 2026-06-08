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
    public class ProjectStaffPlanRepository : BaseRepository, IProjectStaffPlanRepository
    {
        public ProjectStaffPlanRepository(FpsDbContext context) : base(context) { }

        public async Task<PagedData<ProjectStaffPlanView>> GetPagedAsync(PaginationParameters<string> query)
        {
            var baseQuery = _context.ProjectStaffPlanViews.AsNoTracking();

            baseQuery = ApplyFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<ProjectStaffPlanView>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        private static IQueryable<ProjectStaffPlanView> ApplyFilter(IQueryable<ProjectStaffPlanView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProgramNo", out var programNo) && programNo != null)
                query = query.Where(x => EF.Functions.ILike(x.ProgramNo!, $"%{programNo}%"));

            if (dict.TryGetValue("ParentProject", out var parentProject) && parentProject != null)
                query = query.Where(x => EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));

            if (dict.TryGetValue("WorkGroup", out var workGroup) && workGroup != null)
                query = query.Where(x => EF.Functions.ILike(x.WorkGroup!, $"%{workGroup}%"));

            if (dict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
                query = query.Where(x => EF.Functions.ILike(x.GradeCode!, $"%{gradeCode}%"));

            if (dict.TryGetValue("StaffId", out var staffId) && staffId != null)
                query = query.Where(x => EF.Functions.ILike(x.StaffId!, $"%{staffId}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<ProjectStaffPlanView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.ProgramNo).ThenBy(x => x.ParentProject).ThenBy(x => x.Name);
            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<ProjectStaffPlanView> query, string property, bool descending)
        {
            return property switch
            {
                "programno"     => ApplyOrder(query, x => x.ProgramNo,     descending),
                "parentproject" => ApplyOrder(query, x => x.ParentProject, descending),
                "name"          => ApplyOrder(query, x => x.Name,          descending),
                "staffid"       => ApplyOrder(query, x => x.StaffId,       descending),
                "workgroup"     => ApplyOrder(query, x => x.WorkGroup,     descending),
                "gradecode"     => ApplyOrder(query, x => x.GradeCode,     descending),
                "plannedhours"  => ApplyOrder(query, x => x.PlannedHours,  descending),
                "cost"          => ApplyOrder(query, x => x.Cost,          descending),
                "paycost"       => ApplyOrder(query, x => x.PayCost,       descending),
                _               => query.OrderBy(x => x.ProgramNo).ThenBy(x => x.ParentProject)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<ProjectStaffPlanView> query,
            Expression<Func<ProjectStaffPlanView, T>> keySelector, bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
