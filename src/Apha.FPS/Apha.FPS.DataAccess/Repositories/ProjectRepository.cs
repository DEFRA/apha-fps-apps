using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectRepository : BaseRepository, IProjectRepository
    {       
        private readonly FpsDbContext _dbContext;
        private readonly int userId = 42;
       
        public ProjectRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;           
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _dbContext.ProjectViews.Where(p=> p.UserId == userId)
                .Select(pv => new Project
                {
                    ParentProject     = pv.ParentProject     ?? string.Empty,
                    ProjectTitle      = pv.ProjectTitle      ?? string.Empty,
                    Program           = pv.Program           ?? string.Empty,
                    Customer          = pv.Customer          ?? string.Empty,
                    Manager           = pv.Manager,
                    TransferIncome    = pv.TransferIncome    ?? 0m,
                    CustIncome        = pv.CustIncome        ?? 0m,
                    WipEoy            = pv.WipEoy,
                    WipLimit          = pv.WipLimit,
                    WipCurrent        = pv.WipCurrent,
                    ProjectStatus     = pv.ProjectStatus     ?? string.Empty,
                    CostBookNo        = pv.CostBookNo,
                    DateCreated       = pv.DateCreated,
                    FecCost           = pv.FecCost,
                    Profit            = pv.Profit,
                    BudgetCvl         = pv.BudgetCvl,
                    DateCosted        = pv.DateCosted,
                    Disease           = pv.Disease           ?? string.Empty,
                    Contract          = pv.Contract          ?? string.Empty,
                    ProjectParent     = pv.ProjectParent,
                    ShortTitle        = pv.ShortTitle,
                    CaseWorkSub       = pv.CaseWorkSub,
                    PvsIncome         = pv.PvsIncome,
                    PlanCaseWorkDebit = pv.PlanCaseWorkDebit,
                    Finished          = pv.Finished,
                    OwningRc          = pv.OwningRc,
                    Comments          = pv.Comments,
                    CarryOver         = pv.CarryOver,
                    CarryOverSeed     = pv.CarryOverSeed,
                    IsDefraProject    = pv.IsDefraProject    ?? 0,
                    CostCentre        = pv.CostCentre,
                    OracleProjectCode = pv.OracleProjectCode,
                    SubAccountCode    = pv.SubAccountCode,
                    ProjectGroup      = pv.ProjectGroup,
                    IncomeAccountCode = pv.IncomeAccountCode ?? string.Empty,
                    FpsYear        = pv.FpsYear
                })
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(string parentProject)
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ParentProject == parentProject);
        }

        public async Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo)
        {
            var projectQuery = _dbContext.ProjectViews
                .Where(p => p.UserId == userId && p.Program == programNo)
                .Select(pv => new Project
                {
                    ParentProject = pv.ParentProject ?? string.Empty,
                    ProjectTitle  = pv.ProjectTitle  ?? string.Empty,
                    Program       = pv.Program       ?? string.Empty,
                    BudgetCvl     = pv.BudgetCvl,
                    IsDefraProject = pv.IsDefraProject ?? 0
                })
                .AsQueryable();

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);

            projectQuery = !string.IsNullOrWhiteSpace(query.SortBy)
                ? query.SortBy switch
                {
                    nameof(Project.ParentProject) => query.Descending
                        ? projectQuery.OrderByDescending(p => p.ParentProject)
                        : projectQuery.OrderBy(p => p.ParentProject),
                    nameof(Project.ProjectTitle) => query.Descending
                        ? projectQuery.OrderByDescending(p => p.ProjectTitle)
                        : projectQuery.OrderBy(p => p.ProjectTitle),
                    _ => projectQuery.OrderBy(p => p.ParentProject)
                }
                : projectQuery.OrderBy(p => p.ParentProject);

            var result = await projectQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        private static IQueryable<Project> ApplyProjectFilter(IQueryable<Project> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => x.ParentProject.Contains(jobCode.ToString()!));

            if (dict.TryGetValue("JobDescription", out var jobDescription) && jobDescription != null)
                query = query.Where(x => x.ProjectTitle!.Contains(jobDescription.ToString()!));

            return query;
        }
    }
}
