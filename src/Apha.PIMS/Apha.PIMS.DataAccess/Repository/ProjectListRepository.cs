using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ProjectListRepository : BaseRepository, IProjectListRepository
    {
        public ProjectListRepository(PimsDbContext context) : base(context)
        {
        }
        public async Task<PagedData<ProjectListView>> GetAllProjectsAsync(PaginationParameters<string> queryFilter)
        {
            IQueryable<ProjectListView> query = GetProjectsAsync();

            query = ApplyFilter(query, queryFilter.Filter);


            query = ApplySorting(query, queryFilter.SortBy, queryFilter.Descending);          


            return await ApplyPaging(query, queryFilter.Page, queryFilter.PageSize);
        }

        public async Task<List<ProjectListView>> GetAllProjectsForDropDownAsync()
        {
            return await _context.ProjectLatestDetails
                .AsNoTracking()
                .Join(_context.RadtrackProgs,
                      v => v.Program,
                      r => r.Program,
                      (v, r) => new { v, r })
                .Where(x => x.v.Active == "Y")
                .Select(x => new ProjectListView
                {
                    Parentproject = x.v.ParentProject,
                    Program = x.v.Program,
                    Customer = x.v.Customer,
                    OnFps = "Yes"
                })
                .ToListAsync();
        }

        private IQueryable<ProjectListView> GetProjectsAsync()
        {
            IQueryable<ProjectListView> onFpsQuery =
                            from v in _context.ProjectLatestDetails.AsNoTracking()
                            join r in _context.RadtrackProgs on v.Program equals r.Program
                            where v.Active == "Y"
                            select new ProjectListView
                            {
                                Parentproject = v.ParentProject,
                                Program = v.Program,
                                Customer = v.Customer,
                                OnFps = "Yes"
                            };

            IQueryable<ProjectListView> notOnFpsQuery =
                from pp in _context.ProposedProjects.AsNoTracking()
                join g in _context.Projects on pp.Parentproject equals g.Parentproject into gj
                from g in gj.DefaultIfEmpty()
                where g == null
                select new ProjectListView
                {
                    Parentproject = pp.Parentproject,
                    Program = pp.Program,
                    Customer = pp.Customer,
                    OnFps = "No"
                };


            IQueryable<ProjectListView> query = onFpsQuery.Union(notOnFpsQuery);
            return query;
        }

        public async Task<PagedData<ProjectListView>> GetAllProjectsAsync(PaginationParameters<string> queryFilter, int showWhichProjects)
        {
            // Determine the filter for Active based on Access logic
            // showWhichProjects = 1 => only "Y", else => all ("Y" or "N")
            IQueryable<ProjectListView> onFpsQuery =
                from v in _context.ProjectLatestDetails.AsNoTracking()
                join r in _context.RadtrackProgs on v.Program equals r.Program
                where showWhichProjects == 1 ? v.Active == "Y" : (v.Active == "Y" || v.Active == "N")
                select new ProjectListView
                {
                    Parentproject = v.ParentProject,
                    Program = v.Program,
                    Customer = v.Customer,
                    OnFps = "Yes"
                };

            IQueryable<ProjectListView> notOnFpsQuery =
                from pp in _context.ProposedProjects.AsNoTracking()
                join g in _context.Projects on pp.Parentproject equals g.Parentproject into gj
                from g in gj.DefaultIfEmpty()
                where g == null
                select new ProjectListView
                {
                    Parentproject = pp.Parentproject,
                    Program = pp.Program,
                    Customer = pp.Customer,
                    OnFps = "No"
                };

            IQueryable<ProjectListView> query = onFpsQuery.Union(notOnFpsQuery);

            // Apply any additional filtering passed from the caller
            query = ApplyFilter(query, queryFilter.Filter);

            // Apply sorting
            query = ApplySorting(query, queryFilter.SortBy, queryFilter.Descending);           

            return await ApplyPaging(query, queryFilter.Page, queryFilter.PageSize);
        }

        public async Task<List<Projects>> GetYearlyDetailsByProjectAsync(string parentproject)
        {
            return await _context.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Parentproject == parentproject)
                .OrderByDescending(p => p.Year)
                .ToListAsync();
        }
        public async Task<List<ProjectListMilestone>> GetAllProjectsForMilestone()
        {
            return await _context.ProjectRadTrackData
                .AsNoTracking()
                .Join(_context.ProjectLatestDetails,
                      g => g.Parentproject,
                      v => v.ParentProject,
                      (g, v) => new ProjectListMilestone
                      {
                          Parentproject = g.Parentproject,
                          Program = v.Program,
                          Customer = v.Customer,
                          ProjectGroup = v.ProjectGroup,
                          Formrequired = g.Formrequired
                      })
                .OrderBy(x => x.Parentproject)
                .ToListAsync();
        }
        public async Task<ProjectDetailsMilestone?> GetProjectsDetailsForMilestoneAsync(string parentproject)
        {
            return await _context.ProjectRadTrackData
                .AsNoTracking()
                .Where(g => g.Parentproject == parentproject)
                .Join(_context.ProjectLatestDetails,
                      g => g.Parentproject,
                      v => v.ParentProject,
                      (g, v) => new ProjectDetailsMilestone
                      {
                          Parentproject = g.Parentproject,
                          Program = v.Program,
                          Customer = v.Customer,
                          ProjectGroup = v.ProjectGroup,
                          Formrequired = g.Formrequired,
                          TypeLookUp = v.Program != null && v.Program.ToLower().EndsWith("surv") ? 'D' : 'M'
                      })
                .FirstOrDefaultAsync();
        }
        private static IQueryable<ProjectListView> ApplyFilter(IQueryable<ProjectListView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Parentproject", out var parentproject) && parentproject != null)
            {
                string val = parentproject.ToString()!;
                query = query.Where(x => EF.Functions.ILike(x.Parentproject, $"%{val}%"));
            }

            if (dict.TryGetValue("Program", out var program) && program != null)
            {
                string val = program.ToString()!;
                query = query.Where(x => x.Program != null && EF.Functions.ILike(x.Program, $"%{val}%"));
            }

            if (dict.TryGetValue("Customer", out var customer) && customer != null)
            {
                string val = customer.ToString()!;
                query = query.Where(x => x.Customer != null && EF.Functions.ILike(x.Customer, $"%{val}%"));
            }

            return query;
        }

        private static IQueryable<ProjectListView> ApplySorting(
            IQueryable<ProjectListView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(p => p.Parentproject);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable<ProjectListView> ApplySortingByProperty(
            IQueryable<ProjectListView> query, string property, bool descending)
        {
            return property switch
            {
                "parentproject" => ApplyOrder(query, p => p.Parentproject, descending),
                "program" => ApplyOrder(query, p => p.Program, descending),
                "customer" => ApplyOrder(query, p => p.Customer, descending),
                "onfps" => ApplyOrder(query, p => p.OnFps, descending),
                _ => query.OrderBy(p => p.Parentproject)
            };
        }

        private static IQueryable<ProjectListView> ApplyOrder<T>(
            IQueryable<ProjectListView> query,
            Expression<Func<ProjectListView, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

       
    }
}