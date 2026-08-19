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

        private static readonly (string Key, Expression<Func<ProjectStaffPlanView, string?>> Selector)[] FilterableColumns =
        {
            ("ProgramNo",     x => x.ProgramNo),
            ("ParentProject", x => x.ParentProject),
            ("Contract",      x => x.Contract),
            ("Name",          x => x.Name),
            ("WorkGroup",     x => x.WorkGroup),
            ("ProfitCentre",  x => x.ProfitCentre),
            ("WgGrade",       x => x.WgGrade),
            ("PcGrade",       x => x.PcGrade),
            ("GradeCode",     x => x.GradeCode),
            ("StaffId",       x => x.StaffId),
        };

        private static IQueryable<ProjectStaffPlanView> ApplyFilter(IQueryable<ProjectStaffPlanView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            foreach (var (key, selector) in FilterableColumns)
            {
                if (dict.TryGetValue(key, out var value) && value != null)
                    query = ApplyLike(query, selector, value.ToString()!);
            }

            return query;
        }

        private static IQueryable<ProjectStaffPlanView> ApplyLike(
            IQueryable<ProjectStaffPlanView> query,
            Expression<Func<ProjectStaffPlanView, string?>> selector,
            string value)
        {
            var iLike = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
                nameof(NpgsqlDbFunctionsExtensions.ILike),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

            var body = Expression.Call(
                iLike,
                Expression.Constant(EF.Functions),
                selector.Body,
                Expression.Constant($"%{value}%"));

            var predicate = Expression.Lambda<Func<ProjectStaffPlanView, bool>>(body, selector.Parameters);
            return query.Where(predicate);
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
                "contract"      => ApplyOrder(query, x => x.Contract,      descending),
                "name"          => ApplyOrder(query, x => x.Name,          descending),
                "staffid"       => ApplyOrder(query, x => x.StaffId,       descending),
                "workgroup"     => ApplyOrder(query, x => x.WorkGroup,     descending),
                "profitcentre"  => ApplyOrder(query, x => x.ProfitCentre,  descending),
                "wggrade"       => ApplyOrder(query, x => x.WgGrade,       descending),
                "pcgrade"       => ApplyOrder(query, x => x.PcGrade,       descending),
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
