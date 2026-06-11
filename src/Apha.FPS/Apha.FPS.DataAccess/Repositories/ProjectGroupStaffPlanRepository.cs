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
    /// <summary>
    /// Repository implementation for the fps.vpvtprojectgroupmgrplan view.
    /// </summary>
    public class ProjectGroupStaffPlanRepository : BaseRepository, IProjectGroupStaffPlanRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public ProjectGroupStaffPlanRepository(FpsDbContext context, IFpsRequestContext requestContext)
            : base(context)
        {
            _requestContext = requestContext;
        }

        public async Task<PagedData<ProjectGroupStaffPlanView>> GetPagedAsync(PaginationParameters<string> query)
        {
            // Restrict to project groups belonging to the logged-in user
            var userProjectGroups = await _context.ProjectGroupViews
                .AsNoTracking()
                .Where(p => p.UserEmail != null && p.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Select(p => p.ProjectGroupName)
                .Distinct()
                .ToListAsync();

            var baseQuery = _context.ProjectGroupStaffPlanViews
                .AsNoTracking()
                .Where(x => userProjectGroups.Contains(x.ProjectGroup!));

            baseQuery = ApplyFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<ProjectGroupStaffPlanView>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        private static IQueryable<ProjectGroupStaffPlanView> ApplyFilter(
            IQueryable<ProjectGroupStaffPlanView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProjectGroup", out var projectGroup) && projectGroup != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectGroup!, $"%{projectGroup}%"));

            if (dict.TryGetValue("ResourceCentre", out var resourceCentre) && resourceCentre != null)
                query = query.Where(x => EF.Functions.ILike(x.ResourceCentre!, $"%{resourceCentre}%"));

            if (dict.TryGetValue("WorkGroup", out var workGroup) && workGroup != null)
                query = query.Where(x => EF.Functions.ILike(x.WorkGroup!, $"%{workGroup}%"));

            if (dict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
                query = query.Where(x => EF.Functions.ILike(x.GradeCode!, $"%{gradeCode}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
                query = query.Where(x => EF.Functions.ILike(x.Manager!, $"%{manager}%"));

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => EF.Functions.ILike(x.JobCode!, $"%{jobCode}%"));

            if (dict.TryGetValue("ProjectStatus", out var projectStatus) && projectStatus != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectStatus!, $"%{projectStatus}%"));

            return query;
        }

        private static IQueryable ApplySorting(
            IQueryable<ProjectGroupStaffPlanView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.ProjectGroup).ThenBy(x => x.Manager);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(
            IQueryable<ProjectGroupStaffPlanView> query, string property, bool descending)
        {
            return property switch
            {
                "projectgroup"  => ApplyOrder(query, x => x.ProjectGroup,  descending),
                "resourcecentre" => ApplyOrder(query, x => x.ResourceCentre, descending),
                "workgroup"     => ApplyOrder(query, x => x.WorkGroup,     descending),
                "gradecode"     => ApplyOrder(query, x => x.GradeCode,     descending),
                "name"          => ApplyOrder(query, x => x.Name,          descending),
                "manager"       => ApplyOrder(query, x => x.Manager,       descending),
                "jobcode"       => ApplyOrder(query, x => x.JobCode,       descending),
                "projectstatus" => ApplyOrder(query, x => x.ProjectStatus, descending),
                "hrs"           => ApplyOrder(query, x => x.Hrs,           descending),
                "chargerate"    => ApplyOrder(query, x => x.ChargeRate,    descending),
                "fee"           => ApplyOrder(query, x => x.Fee,           descending),
                _               => query.OrderBy(x => x.ProjectGroup).ThenBy(x => x.Manager)
            };
        }

        private static IQueryable ApplyOrder<T>(
            IQueryable<ProjectGroupStaffPlanView> query,
            Expression<Func<ProjectGroupStaffPlanView, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
