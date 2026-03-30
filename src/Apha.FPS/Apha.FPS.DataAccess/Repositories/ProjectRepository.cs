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
    public class ProjectRepository : BaseRepository, IProjectRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly int userId = 42;

        public ProjectRepository(FpsDbContext dbContext, IFpsYearContext fpsYearContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _fpsYearContext = fpsYearContext;
        }

        public async Task<IEnumerable<ProjectView>> GetAllProjectsAsync()
        {
            return await _dbContext.ProjectViews
                .Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(string parentProject)
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ParentProject == parentProject);
        }

        public async Task<PagedData<Project>> GetPagedProjectsAsync(PaginationParameters<string> query)
        {
            var queryable = _dbContext.Projects.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                queryable = queryable.Where(p =>
                    p.ParentProject.ToLower().Contains(search) ||
                    p.ProjectTitle.ToLower().Contains(search));
            }

            queryable = query.SortBy?.ToLower() switch
            {
                "parentproject" => query.Descending
                    ? queryable.OrderByDescending(p => p.ParentProject)
                    : queryable.OrderBy(p => p.ParentProject),
                "projecttitle" => query.Descending
                    ? queryable.OrderByDescending(p => p.ProjectTitle)
                    : queryable.OrderBy(p => p.ProjectTitle),
                _ => queryable.OrderBy(p => p.ParentProject)
            };

            var result = await queryable.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<PactProjectView>> GetPagedPactProjectsAsync(PaginationParameters<string> query)
        {
            var querProjects = _dbContext.PactProjectViews.AsNoTracking().AsQueryable();

            // Apply filtering
            querProjects = ApplyProjectFilter(querProjects, query.Filter);

            // Apply sorting
            querProjects = (IQueryable<PactProjectView>)ApplySorting(querProjects, query.SortBy, query.Descending);

            // Execute query
            var result = await querProjects.ToListAsync();

            // Apply paging
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.FpsYear = _fpsYearContext.FpsYear;
            await _dbContext.Projects.AddAsync(project);
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            project.FpsYear = _fpsYearContext.FpsYear;
            _dbContext.Entry(project).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<Project?> UpdatePactProjectDetailsAsync(Project project)
        {            
            var entity = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == project.ParentProject
                    && p.FpsYear == _fpsYearContext.FpsYear);

            if (entity == null) return null;

            entity.ProjectTitle = project.ProjectTitle;
            entity.Program = project.Program;
            entity.Customer = project.Customer;
            entity.Manager = project.Manager;
            entity.Contract = project.Contract;
            entity.ProjectStatus = project.ProjectStatus;
            entity.Disease = project.Disease;
            entity.IsDefraProject = project.IsDefraProject;
            entity.Finished = project.Finished;
            entity.Comments = project.Comments;
            entity.BudgetCvl = project.BudgetCvl;
            entity.TransferIncome = project.TransferIncome;
            entity.PvsIncome = project.PvsIncome;
            entity.WipEoy = project.WipEoy;
            entity.WipLimit = project.WipLimit;
            entity.WipCurrent = project.WipCurrent;
            entity.FecCost = project.FecCost;

            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteProjectAsync(string parentProject)
        {
            var project = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == parentProject
                    && p.FpsYear == _fpsYearContext.FpsYear);
            if (project == null) return false;
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private static IQueryable<PactProjectView> ApplyProjectFilter(IQueryable<PactProjectView> queryProjects, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return queryProjects;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryProjects;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ParentProject", out var parentProject) && parentProject != null)
            {
                queryProjects = queryProjects.Where(x => x.ParentProject.Contains(parentProject.ToString()!));
            }

            if (dict.TryGetValue("ProjectTitle", out var projectTitle) && projectTitle != null)
            {
                queryProjects = queryProjects.Where(x => x.ProjectTitle.Contains(projectTitle.ToString()!));
            }

            return queryProjects;
        }

        private static IQueryable ApplySorting(IQueryable<PactProjectView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.ParentProject);
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<PactProjectView> query, string property, bool descending)
        {
            return property switch
            {
                "parentproject" => ApplyOrder(query, i => i.ParentProject, descending),
                "projecttitle" => ApplyOrder(query, i => i.ProjectTitle, descending),               
                _ => query.OrderBy(e => e.ParentProject)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<PactProjectView> query, Expression<Func<PactProjectView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}