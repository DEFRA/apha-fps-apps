using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectRepository : IProjectRepository
    {       
        private readonly FpsDbContext _dbContext;
        private readonly int userId = 42;
       
        public ProjectRepository(FpsDbContext dbContext)
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
                    FpsCalYear        = pv.FpsCalYear
                })
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(string parentProject)
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ParentProject == parentProject);
        }
    }
}
