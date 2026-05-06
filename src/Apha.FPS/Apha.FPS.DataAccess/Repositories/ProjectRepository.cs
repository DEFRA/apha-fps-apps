using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectRepository : BaseRepository, IProjectRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ProjectRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<IEnumerable<ProjectView>> GetAllProjectsAsync()
        {
            return await _dbContext.ProjectViews
                .Where(p => p.UserEmail != null && p.UserEmail.ToLower() == _requestContext.UserEmailId).ToListAsync();
        }

        public async Task<IEnumerable<PactProjectView>> GetAllPactProjectsAsync()
        {
            return await _dbContext.PactProjectViews
                .AsNoTracking()
                .OrderBy(p => p.ParentProject)
                .ToListAsync();
        }

        public async Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo)
        {
            var projectQuery = _dbContext.ProjectViews
                .AsNoTracking()
                .Where(p => p.UserEmail != null && p.UserEmail.ToLower() == _requestContext.UserEmailId && p.Program == programNo)
                .Select(pv => new Project
                {
                    ParentProject = pv.ParentProject ?? string.Empty,
                    ProjectTitle = pv.ProjectTitle ?? string.Empty,
                    Program = pv.Program ?? string.Empty,
                    Manager = pv.Manager,
                    Customer = pv.Customer ?? string.Empty,
                    Contract = pv.Contract ?? string.Empty,
                    Disease = pv.Disease ?? string.Empty,
                    ProjectStatus = pv.ProjectStatus ?? string.Empty,
                    ProjectGroup = pv.ProjectGroup,
                    BudgetCvl = pv.BudgetCvl,
                    CustIncome = pv.CustIncome ?? 0,
                    TransferIncome = pv.TransferIncome ?? 0,
                    PlanCaseWorkDebit = pv.PlanCaseWorkDebit,
                    IsDefraProject = pv.IsDefraProject ?? 0,
                    IncomeAccountCode = pv.IncomeAccountCode ?? string.Empty
                }).AsQueryable();

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);
            projectQuery = (IQueryable<Project>)ApplySorting(projectQuery, query.SortBy, query.Descending);

            var result = await projectQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
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
                    p.ParentProject.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                    p.ProjectTitle.Contains(search, StringComparison.CurrentCultureIgnoreCase));
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
            querProjects = ApplyPactProjectFilter(querProjects, query.Filter);

            // Apply sorting
            querProjects = (IQueryable<PactProjectView>)ApplyPactProjectSorting(querProjects, query.SortBy, query.Descending);

            // Execute query
            var result = await querProjects.ToListAsync();

            // Apply paging
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        //Create Project with trigger code
        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.FpsYear = _requestContext.FpsYear;
            project.DateCreated = DateTime.Now;
            await _dbContext.Projects.AddAsync(project);
            // Converted trigger logic — UITrig_tlkpProject FOR INSERT: stage audit log in same unit of work
            _dbContext.ProjectLogs.Add(MapProjectToLog(project, "I", _requestContext.UserEmailId));
            await _dbContext.SaveChangesAsync();
            return project;
        }

        //Update Project with trigger code
        public async Task<Project> UpdateProjectAsync(Project project)
        {
            project.FpsYear = _requestContext.FpsYear;
            _dbContext.Entry(project).State = EntityState.Modified;
            _dbContext.Entry(project).Property(p => p.IncomeAccountCode).IsModified = false;
            // Converted trigger logic — UITrig_tlkpProject FOR UPDATE: stage audit log in same unit of work
            _dbContext.ProjectLogs.Add(MapProjectToLog(project, "U", _requestContext.UserEmailId));
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<Project?> UpdatePactProjectDetailsAsync(Project project)
        {
            var entity = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == project.ParentProject
                    && p.FpsYear == _requestContext.FpsYear);

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

            // Converted trigger logic — UITrig_tlkpProject FOR UPDATE: stage audit log in same unit of work
            _dbContext.ProjectLogs.Add(MapProjectToLog(entity, "U", _requestContext.UserEmailId));
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteProjectAsync(string parentProject)
        {
            var project = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == parentProject
                    && p.FpsYear == _requestContext.FpsYear);
            if (project == null) return false;
            // Converted trigger logic — DTrig_tlkpProject FOR DELETE: stage audit log before delete in same unit of work
            _dbContext.ProjectLogs.Add(MapProjectToLog(project, "D", _requestContext.UserEmailId));
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasAssociatedJobCodesAsync(string parentProject)
        {
            return await _dbContext.JobCodes
                .AnyAsync(j => j.ParentProject == parentProject
                    && j.FpsYear == _requestContext.FpsYear);
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Project> query, Expression<Func<Project, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }


        private static IQueryable<Project> ApplyProjectFilter(IQueryable<Project> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ParentProject", out var parentProject) && parentProject != null)
                query = query.Where(x => x.ParentProject.Contains(parentProject.ToString()!));

            if (dict.TryGetValue("ProjectTitle", out var projectTitle) && projectTitle != null)
                query = query.Where(x => x.ProjectTitle.Contains(projectTitle.ToString()!));

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
                query = query.Where(x => x.Manager!.Contains(manager.ToString()!));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<Project> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(p => p.ParentProject);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }


        private static IQueryable ApplySortingByProperty(IQueryable<Project> query, string property, bool descending)
        {
            return property switch
            {
                "parentproject" => ApplyOrder(query, p => p.ParentProject, descending),
                "projecttitle" => ApplyOrder(query, p => p.ProjectTitle, descending),
                "program" => ApplyOrder(query, p => p.Program, descending),
                "budgetcvl" => ApplyOrder(query, p => p.BudgetCvl, descending),
                _ => query.OrderBy(p => p.ParentProject)
            };
        }


        private static IQueryable<PactProjectView> ApplyPactProjectFilter(IQueryable<PactProjectView> queryProjects, string? filter)
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

        private static IQueryable ApplyPactProjectSorting(IQueryable<PactProjectView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.ParentProject);
            }

            return ApplyPactSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplyPactSortingByProperty(IQueryable<PactProjectView> query, string property, bool descending)
        {
            return property switch
            {
                "parentproject" => ApplyPactProjectOrder(query, i => i.ParentProject, descending),
                "projecttitle" => ApplyPactProjectOrder(query, i => i.ProjectTitle, descending),
                _ => query.OrderBy(e => e.ParentProject)
            };
        }

        private static IQueryable ApplyPactProjectOrder<T>(IQueryable<PactProjectView> query, Expression<Func<PactProjectView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        // ── ProgrammeNewProject operations ──────────────────────────────────

        /// <summary>
        /// Checks whether a project code already exists — derived from qryProjectCheck.
        /// </summary>
        public async Task<bool> CheckProjectExistsAsync(string newProject)
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .AnyAsync(p => p.ParentProject == newProject);
        }

        /// <summary>
        /// Checks whether an old project code has Farm File submission data — derived from qryProjectCheckFF.
        /// </summary>
        public async Task<bool> CheckProjectExistsInFarmFileAsync(string oldProject)
        {
            return await _dbContext.SurvFFSubmissions
                .AsNoTracking()
                .AnyAsync(s => s.Contract == oldProject);
        }

        // ── Delete pre-condition checks (moved to service layer) ────────────

        public async Task<bool> HasPlannedTestsAsync(string parentProject)
        {
            return await _dbContext.TestRequirements
                .AsNoTracking()
                .AnyAsync(tr => tr.ProjectBuyerCode == parentProject);
        }

        public async Task<bool> HasMonthlyOutputAsync(string parentProject)
        {
            return await _dbContext.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(mo => mo.Buyer == parentProject);
        }

        public async Task<bool> HasMonthlyTimeAsync(string parentProject)
        {
            return await _dbContext.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(mt => mt.ParentProject == parentProject);
        }

        public async Task<bool> HasProjectInvoicesAsync(string parentProject)
        {
            return await _dbContext.ProjectInvoices
                .AsNoTracking()
                .AnyAsync(pi => pi.ProjectParent == parentProject);
        }

        public async Task<bool> HasProjectSubcontractsAsync(string parentProject)
        {
            return await _dbContext.ProjectSubContracts
                .AsNoTracking()
                .AnyAsync(ps => ps.Project == parentProject);
        }

        /// <summary>
        /// Renames a project code and updates all child table references — derived from usp_ChangeProjectCode.
        /// UITrig_tlkpProject FOR INSERT appended: stages audit log entry in same unit of work.
        /// </summary>
        public async Task ChangeProjectCodeAsync(string oldCode, string newCode)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Copy project row and stage audit log in same unit of work
                    var oldProject = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == oldCode)
                        ?? throw new InvalidOperationException($"Project '{oldCode}' not found.");

                    var newProject = new Project
                    {
                        ParentProject     = newCode,
                        ProjectTitle      = oldProject.ProjectTitle,
                        Program           = oldProject.Program,
                        Customer          = oldProject.Customer,
                        Manager           = oldProject.Manager,
                        TransferIncome    = oldProject.TransferIncome,
                        CustIncome        = oldProject.CustIncome,
                        WipEoy            = oldProject.WipEoy,
                        WipLimit          = oldProject.WipLimit,
                        WipCurrent        = oldProject.WipCurrent,
                        ProjectStatus     = oldProject.ProjectStatus,
                        CostBookNo        = oldProject.CostBookNo,
                        FecCost           = oldProject.FecCost,
                        Profit            = oldProject.Profit,
                        BudgetCvl         = oldProject.BudgetCvl,
                        DateCosted        = oldProject.DateCosted,
                        Disease           = oldProject.Disease,
                        Contract          = oldProject.Contract,
                        ProjectParent     = oldProject.ProjectParent,
                        ShortTitle        = oldProject.ShortTitle,
                        CaseWorkSub       = oldProject.CaseWorkSub,
                        PvsIncome         = oldProject.PvsIncome,
                        PlanCaseWorkDebit = oldProject.PlanCaseWorkDebit,
                        Finished          = oldProject.Finished,
                        OwningRc          = oldProject.OwningRc,
                        Comments          = oldProject.Comments,
                        CarryOver         = oldProject.CarryOver,
                        CarryOverSeed     = oldProject.CarryOverSeed,
                        IsDefraProject    = oldProject.IsDefraProject,
                        CostCentre        = oldProject.CostCentre,
                        OracleProjectCode = oldProject.OracleProjectCode,
                        SubAccountCode    = oldProject.SubAccountCode,
                        ProjectGroup      = oldProject.ProjectGroup,
                        IncomeAccountCode = oldProject.IncomeAccountCode,
                        FpsYear           = oldProject.FpsYear
                    };

                    await _dbContext.Projects.AddAsync(newProject);
                    // Converted trigger logic — UITrig_tlkpProject FOR INSERT: stage audit log in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(newProject, "I", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    // INSERT new JobCode rows
                    var jobCodesToCopy = await _dbContext.JobCodes
                        .Where(jc => jc.ParentProject == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    var newJobCodes = jobCodesToCopy.Select(jc => new JobCode
                    {
                        JobCodeId        = jc.JobCodeId == oldCode ? newCode : jc.JobCodeId,
                        ParentProject    = newCode,
                        JobCodeWorkGroup = jc.JobCodeWorkGroup,
                        NewProg          = jc.NewProg,
                        Type             = jc.Type,
                        JobCodeName      = jc.JobCodeName,
                        FpsYear          = jc.FpsYear
                    }).ToList();
                    if (newJobCodes.Count > 0)
                    {
                        await _dbContext.JobCodes.AddRangeAsync(newJobCodes);
                        await _dbContext.SaveChangesAsync();
                    }

                    // UPDATE tlkpTestCapability.planportfolio
                    await _dbContext.TestCapabilities
                        .Where(tc => tc.PlanPortfolio == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty<string>(x => x.PlanPortfolio, newCode));

                    // sp_Insert_tcv: copy TimeCodeValid rows with code substitution
                    var tcvToCopy = await _dbContext.TimeCodeValids
                        .Where(tcv => tcv.ParentProject == oldCode || tcv.Portfolio == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    var newTcvs = tcvToCopy
                        .Select(tcv => new TimeCodeValid
                        {
                            WorkGroup     = tcv.WorkGroup,
                            TimeCode      = tcv.TimeCode == oldCode ? newCode : tcv.TimeCode,
                            ParentProject = tcv.ParentProject == oldCode ? newCode : tcv.ParentProject,
                            TestCode      = tcv.TestCode,
                            JobCode       = tcv.JobCode == oldCode ? newCode : tcv.JobCode,
                            Portfolio     = tcv.Portfolio == oldCode ? newCode : tcv.Portfolio,
                            Active        = tcv.Active,
                            FpsYear       = tcv.FpsYear
                        })
                        .DistinctBy(tcv => new { tcv.WorkGroup, tcv.TimeCode, tcv.ParentProject })
                        .ToList();
                    if (newTcvs.Count > 0)
                    {
                        await _dbContext.TimeCodeValids.AddRangeAsync(newTcvs);
                        await _dbContext.SaveChangesAsync();
                    }

                    // sp_Insert_tr: copy tlkpTestReqmt rows with new buyer code
                    var testReqsToCopy = await _dbContext.TestRequirements
                        .Where(tr => tr.ProjectBuyerCode == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    var newTestReqs = testReqsToCopy.Select(tr => new TestRequirement
                    {
                        TestCode         = tr.TestCode,
                        Buyer            = newCode,
                        UnitPrice        = tr.UnitPrice,
                        NoRequired       = tr.NoRequired,
                        ProjectBuyerCode = newCode,
                        TestBuyerCode    = tr.TestBuyerCode,
                        DateCreated      = tr.DateCreated,
                        Active           = tr.Active,
                        FpsYear          = tr.FpsYear
                    }).ToList();
                    if (newTestReqs.Count > 0)
                    {
                        await _dbContext.TestRequirements.AddRangeAsync(newTestReqs);
                        await _dbContext.SaveChangesAsync();
                    }

                    // UPDATE remaining child tables
                    await _dbContext.MonthlyTimes
                        .Where(mt => mt.ParentProject == oldCode)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(x => x.ParentProject, newCode)
                            .SetProperty(x => x.TimeCode, x => x.TimeCode == oldCode ? newCode : x.TimeCode));
                    await _dbContext.MonthlyOutputs
                        .Where(mo => mo.Buyer == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Buyer, newCode));
                    await _dbContext.AdditionalCosts
                        .Where(ac => ac.JobCode == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));
                    await _dbContext.ProjectInvoices
                        .Where(pi => pi.ProjectParent == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProjectParent, newCode));
                    await _dbContext.ProjectSubContracts
                        .Where(ps => ps.Project == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));
                    await _dbContext.TimeCostCalcs
                        .Where(tc => tc.Project == oldCode)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(x => x.Project, newCode)
                            .SetProperty(x => x.JobCode, x => x.JobCode == oldCode ? newCode : x.JobCode));
                    await _dbContext.ProjectMonths
                        .Where(pm => pm.Project == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));
                    await _dbContext.AnimalRequests
                        .Where(ar => ar.JobCode == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));
                    await _dbContext.Milestones
                        .Where(m => m.Project == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));

                    // UPDATE tblStaffJob (existing DbSet — use LINQ for type safety)
                    await _dbContext.StaffJobs
                        .Where(sj => sj.JobCode == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));

                    await _dbContext.ProjectMonthFinals
                        .Where(pmf => pmf.Project == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));

                    // sp_Delete_tr, sp_Delete_tcv, sp_Delete_jc, sp_Delete_pp
                    await _dbContext.TestRequirements
                        .Where(tr => tr.ProjectBuyerCode == oldCode)
                        .ExecuteDeleteAsync();
                    await _dbContext.TimeCodeValids
                        .Where(tcv => tcv.ParentProject == oldCode || tcv.Portfolio == oldCode)
                        .ExecuteDeleteAsync();
                    await _dbContext.JobCodes
                        .Where(jc => jc.ParentProject == oldCode)
                        .ExecuteDeleteAsync();
                    await _dbContext.Projects
                        .Where(p => p.ParentProject == oldCode)
                        .ExecuteDeleteAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
        }

        /// <summary>
        /// Deletes a project and all dependent child records — derived from usp_Delete_Project.
        /// DTrig_tlkpProject (DELETE) appended: stages audit log entry.
        /// </summary>
        public async Task DeleteProjectAndChildrenAsync(string parentProject)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Converted trigger logic — DTrig_tlkpProject FOR DELETE: stage audit log before delete
                    var project = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == parentProject);
                    if (project != null)
                        _dbContext.ProjectLogs.Add(MapProjectToLog(project, "D", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    // sp_Delete_tcv
                    await _dbContext.TimeCodeValids
                        .Where(tcv => tcv.ParentProject == parentProject || tcv.Portfolio == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_Delete_JC
                    await _dbContext.JobCodes
                        .Where(jc => jc.ParentProject == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_delete_tr
                    await _dbContext.TestRequirements
                        .Where(tr => tr.ProjectBuyerCode == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_Delete_ar
                    await _dbContext.AnimalRequests
                        .Where(ar => ar.JobCode == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_Delete_sj
                    await _dbContext.StaffJobs
                        .Where(sj => sj.JobCode == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_Delete_ac
                    await _dbContext.AdditionalCosts
                        .Where(ac => ac.JobCode == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_Delete_pp
                    await _dbContext.Projects
                        .Where(p => p.ParentProject == parentProject)
                        .ExecuteDeleteAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
        }

        // ── Private helpers ────────────────────────────────────────────────

        private static ProjectLog MapProjectToLog(Project p, string operation, string? userId) => new()
        {
            ParentProject = p.ParentProject,
            ProjectTitle = p.ProjectTitle,
            Program = p.Program,
            Customer = p.Customer,
            Manager = p.Manager,
            TransferIncome = p.TransferIncome,
            CustIncome = p.CustIncome,
            WipEoy = p.WipEoy,
            WipLimit = p.WipLimit,
            WipCurrent = p.WipCurrent,
            ProjectStatus = p.ProjectStatus,
            CostBookNo = p.CostBookNo,
            DateCreated = p.DateCreated.HasValue ? DateTime.SpecifyKind(p.DateCreated.Value, DateTimeKind.Utc) : null,
            FecCost = p.FecCost,
            Profit = p.Profit,
            BudgetCvl = p.BudgetCvl,
            DateCosted = p.DateCosted.HasValue ? DateTime.SpecifyKind(p.DateCosted.Value, DateTimeKind.Utc) : null,
            Disease = p.Disease,
            Contract = p.Contract,
            ProjectParent = p.ProjectParent,
            ShortTitle = p.ShortTitle,
            CaseWorkSub = p.CaseWorkSub,
            PvsIncome = p.PvsIncome,
            PlanCaseWorkDebit = p.PlanCaseWorkDebit,
            Finished = p.Finished,
            OwningRc = p.OwningRc,
            Comments = p.Comments,
            CarryOver = p.CarryOver,
            CarryOverSeed = p.CarryOverSeed,
            DateTime = DateTime.UtcNow,
            InsertDelete = operation,
            JobCode = p.ParentProject,
            IsDefraProject = p.IsDefraProject,
            CostCentre = p.CostCentre,
            OracleProjectCode = p.OracleProjectCode,
            SubAccountCode = p.SubAccountCode,
            ProjectGroup = p.ProjectGroup,
            IncomeAccountCode = p.IncomeAccountCode,
            FpsYear = p.FpsYear,
            UserId = userId
        };
    }
}
