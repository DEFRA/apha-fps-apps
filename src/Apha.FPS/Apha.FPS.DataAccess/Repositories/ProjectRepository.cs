// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ProjectRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - Added GetProjectProfitabilityVlaAsync — new public method for the
 *     Project Profitability VLA list (frmJobcodeTotalsVLA form migration).
 *   - Queries the pre-computed vprojectprofitabilityvla PostgreSQL view via
 *     the FpsDbContext.ProjectProfitabilityVlaViews DbSet (keyless, AsNoTracking).
 *   - Applies four VLA-specific LINQ filter dimensions:
 *       ProjectStatus (filterProjectStatus in HTML prototype)
 *       ProgramNo     (filterProgram)
 *       Manager       (filterManager  — VLA-specific, absent from base profitability)
 *       Customer      (filterCustomer — VLA-specific, absent from base profitability)
 *   - All filter dimensions use EF.Functions.ILike (case-insensitive) consistent
 *     with the existing profitability filter helpers in this repository.
 *   - Sorting covers all 14 sortable DataGrid columns from projectprofitability_vla.js.
 *   - Paging delegated to inherited ApplyPaging helper (same pattern as all other
 *     paged list methods in this repository).
 *   - Added using Apha.Common.Contracts.FPS for ProjectProfitabilityVlaReq.
 *
 * PRESERVED:
 *   - All existing public and private methods unchanged (GetProjectProfitabilityAsync,
 *     GetProjectGroupProfitabilityAsync, ComputeProfitabilityAsync, CRUD operations,
 *     trigger-absorbed write methods, all helper methods).
 *   - All field names, method signatures, and business logic preserved exactly.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm the vprojectprofitabilityvla PostgreSQL view exists
 *     in the fps schema with column names matching ProjectProfitabilityVlaViewMap
 *     (jobcode, program, customer, manager, projectstatus, staffcosts, testcost,
 *     animalcosts, additionalcosts, totalcosts, budget, profit, targetprofit, offtarget).
 *     The view aggregates qryJobCodeTotals + qryJobCodeTotals2 logic and must be
 *     created if it does not yet exist in the database.
 *   - TRANSFORMENGINE TODO: confirm FpsYear scoping — if the view does NOT embed
 *     year filtering via its join to the year-scoped tlkpProject table, add a
 *     year-filter predicate to GetProjectProfitabilityVlaAsync manually.
 *   - TRANSFORMENGINE TODO: confirm EF.Functions.ILike filter on Status uses the
 *     correct column alias ("projectstatus" in the view DDL); if the view aliases
 *     it as "status", update ProjectProfitabilityVlaViewMap.cs accordingly.
 */

using Apha.Common.Contracts.FPS;
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
        private readonly IFpsRequestContext _requestContext;

        public ProjectRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<IEnumerable<ProjectView>> GetAllProjectsAsync()
        {
            return await _dbContext.ProjectViews
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId)).ToListAsync();
        }

        public async Task<IEnumerable<PactProjectView>> GetAllPactProjectsAsync()
        {
            return await _dbContext.PactProjectViews
                .AsNoTracking()
                .OrderBy(p => p.ParentProject)
                .ToListAsync();
        }

        public async Task<PagedData<Project>> GetProjectsByProjectGroupAsync(PaginationParameters<string> query, string projectGroup)
        {
            var projectQuery = (from pg in _dbContext.ProjectGroupViews
                               join pv in _dbContext.Projects on
                               new { pg.ProjectGroupName } equals new { ProjectGroupName = pv.ProjectGroup }
                               where EF.Functions.ILike(pg.UserEmail!, _requestContext.UserEmailId) && pg.ProjectGroupName == projectGroup
                select(new Project
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
                    CustIncome = pv.CustIncome,
                    TransferIncome = pv.TransferIncome,
                    PlanCaseWorkDebit = pv.PlanCaseWorkDebit,   
                    IsDefraProject = pv.IsDefraProject,
                    IncomeAccountCode = pv.IncomeAccountCode ?? string.Empty
                })).AsQueryable();

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);
            projectQuery = (IQueryable<Project>)ApplySorting(projectQuery, query.SortBy, query.Descending);

            var result = await projectQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo)
        {
            var projectQuery = _dbContext.ProjectViews
                .AsNoTracking()
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId) && p.Program == programNo)
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
                    EF.Functions.ILike(p.ParentProject!, $"%{search}%") ||
                    EF.Functions.ILike(p.ProjectTitle!, $"%{search}%"));
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
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    project.FpsYear = _requestContext.FpsYear;
                    project.DateCreated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    NormalizeDateTimesToUnspecified(project);
                    await _dbContext.Projects.AddAsync(project);
                    // Converted trigger logic — UITrig_tlkpProject FOR INSERT: stage audit log in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(project, "I", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return project;
        }

        //Update Project with trigger code
        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    project.FpsYear = _requestContext.FpsYear;
                    NormalizeDateTimesToUnspecified(project);
                    _dbContext.Entry(project).State = EntityState.Modified;
                    _dbContext.Entry(project).Property(p => p.IncomeAccountCode).IsModified = false;
                    // Converted trigger logic — UITrig_tlkpProject FOR UPDATE: stage audit log in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(project, "I", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return project;
        }

        private static void NormalizeDateTimesToUnspecified(Project p)
        {
            if (p.DateCreated.HasValue && p.DateCreated.Value.Kind != DateTimeKind.Unspecified)
                p.DateCreated = DateTime.SpecifyKind(p.DateCreated.Value, DateTimeKind.Unspecified);

            if (p.DateCosted.HasValue && p.DateCosted.Value.Kind != DateTimeKind.Unspecified)
                p.DateCosted = DateTime.SpecifyKind(p.DateCosted.Value, DateTimeKind.Unspecified);
        }

        public async Task<Project?> UpdatePactProjectDetailsAsync(Project project)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            Project? entity = null;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    entity = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == project.ParentProject
                            && p.FpsYear == _requestContext.FpsYear);

                    if (entity == null) return;

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

                    NormalizeDateTimesToUnspecified(entity);
                    // Converted trigger logic — UITrig_tlkpProject FOR UPDATE: stage audit log in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(entity, "U", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return entity;
        }

        public async Task<Project?> UpdatePactPortfolioDetailsAsync(Project project)
        {
            var entity = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == project.ParentProject
                    && p.FpsYear == _requestContext.FpsYear);

            if (entity == null) return null;

            entity.ProjectTitle = project.ProjectTitle;
            entity.Program = project.Program;
            entity.Manager = project.Manager;
            entity.Finished = project.Finished;
            entity.Comments = project.Comments;
            entity.BudgetCvl = project.BudgetCvl;
            entity.TransferIncome = project.TransferIncome;

            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Project?> UpdateFpsPortfolioDetailsAsync(Project project)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            Project? entity = null;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    entity = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == project.ParentProject
                            && p.FpsYear == _requestContext.FpsYear);

                    if (entity == null) return;

                    entity.ProjectTitle = project.ProjectTitle;
                    entity.Program = project.Program;
                    entity.Manager = project.Manager;
                    entity.Disease = project.Disease;
                    entity.ProjectStatus = project.ProjectStatus;
                    entity.TransferIncome = project.TransferIncome;
                    entity.CustIncome = project.CustIncome;
                    entity.Profit = project.Profit;
                    entity.Contract = project.Contract;
                    entity.Customer = project.Customer;

                    NormalizeDateTimesToUnspecified(entity);
                    _dbContext.ProjectLogs.Add(MapProjectToLog(entity, "U", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return entity;
        }

        public async Task<bool> DeleteProjectAsync(string parentProject)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var deleted = false;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var project = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == parentProject
                            && p.FpsYear == _requestContext.FpsYear);
                    if (project == null) return;
                    NormalizeDateTimesToUnspecified(project);
                    // Converted trigger logic — DTrig_tlkpProject FOR DELETE: stage audit log before delete in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(project, "D", _requestContext.UserEmailId));
                    _dbContext.Projects.Remove(project);
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                    deleted = true;
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return deleted;
        }

        public async Task<bool> HasAssociatedJobCodesAsync(string parentProject)
        {
            return await _dbContext.JobCodes
                .AnyAsync(j => j.ParentProject == parentProject
                    && j.FpsYear == _requestContext.FpsYear);
        }

        public async Task<bool> CheckProgramExistsAsync(string programNo)
        {
            if (string.IsNullOrWhiteSpace(programNo))
                return true; // null/empty is allowed (nullable FK)
            return await _dbContext.Programs
                .AsNoTracking()
                .AnyAsync(p => p.ProgramNo == programNo);
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
                query = query.Where(x => EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));

            if (dict.TryGetValue("ProjectTitle", out var projectTitle) && projectTitle != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectTitle, $"%{projectTitle}%"));

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
                query = query.Where(x => EF.Functions.ILike(x.Manager!, $"%{manager}%"));

            return query;
        }

        private static IQueryable<ProjectView> ApplyProfitabilityFilter(IQueryable<ProjectView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => EF.Functions.ILike(x.ParentProject!, $"%{jobCode}%"));

            if (dict.TryGetValue("ProjectStatus", out var projectStatus) && projectStatus != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectStatus!, $"%{projectStatus}%"));

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
                "manager" => ApplyOrder(query, p => p.Manager, descending),
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
                queryProjects = queryProjects.Where(x => EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));
            }

            if (dict.TryGetValue("ProjectTitle", out var projectTitle) && projectTitle != null)
            {
                queryProjects = queryProjects.Where(x => EF.Functions.ILike(x.ProjectTitle, $"%{projectTitle}%"));
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
                        ParentProject = newCode,
                        ProjectTitle = oldProject.ProjectTitle,
                        Program = oldProject.Program,
                        Customer = oldProject.Customer,
                        Manager = oldProject.Manager,
                        TransferIncome = oldProject.TransferIncome,
                        CustIncome = oldProject.CustIncome,
                        WipEoy = oldProject.WipEoy,
                        WipLimit = oldProject.WipLimit,
                        WipCurrent = oldProject.WipCurrent,
                        ProjectStatus = oldProject.ProjectStatus,
                        CostBookNo = oldProject.CostBookNo,
                        FecCost = oldProject.FecCost,
                        Profit = oldProject.Profit,
                        BudgetCvl = oldProject.BudgetCvl,
                        DateCreated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                        DateCosted = oldProject.DateCosted,
                        Disease = oldProject.Disease,
                        Contract = oldProject.Contract,
                        ProjectParent = oldProject.ProjectParent,
                        ShortTitle = oldProject.ShortTitle,
                        CaseWorkSub = oldProject.CaseWorkSub,
                        PvsIncome = oldProject.PvsIncome,
                        PlanCaseWorkDebit = oldProject.PlanCaseWorkDebit,
                        Finished = oldProject.Finished,
                        OwningRc = oldProject.OwningRc,
                        Comments = oldProject.Comments,
                        CarryOver = oldProject.CarryOver,
                        CarryOverSeed = oldProject.CarryOverSeed,
                        IsDefraProject = oldProject.IsDefraProject,
                        CostCentre = oldProject.CostCentre,
                        OracleProjectCode = oldProject.OracleProjectCode,
                        SubAccountCode = oldProject.SubAccountCode,
                        ProjectGroup = oldProject.ProjectGroup,
                        IncomeAccountCode = oldProject.IncomeAccountCode,
                        FpsYear = oldProject.FpsYear
                    };

                    NormalizeDateTimesToUnspecified(newProject);

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
                        JobCodeId = jc.JobCodeId == oldCode ? newCode : jc.JobCodeId,
                        ParentProject = newCode,
                        JobCodeWorkGroup = jc.JobCodeWorkGroup,
                        NewProg = jc.NewProg,
                        Type = jc.Type,
                        JobCodeName = jc.JobCodeName,
                        FpsYear = jc.FpsYear
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
                            WorkGroup = tcv.WorkGroup,
                            TimeCode = tcv.TimeCode == oldCode ? newCode : tcv.TimeCode,
                            ParentProject = tcv.ParentProject == oldCode ? newCode : tcv.ParentProject,
                            TestCode = tcv.TestCode,
                            JobCode = tcv.JobCode == oldCode ? newCode : tcv.JobCode,
                            Portfolio = tcv.Portfolio == oldCode ? newCode : tcv.Portfolio,
                            Active = tcv.Active,
                            FpsYear = tcv.FpsYear
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
                        TestCode = tr.TestCode,
                        Buyer = newCode,
                        UnitPrice = tr.UnitPrice,
                        NoRequired = tr.NoRequired,
                        ProjectBuyerCode = newCode,
                        TestBuyerCode = tr.TestBuyerCode,
                        DateCreated = tr.DateCreated,
                        Active = tr.Active,
                        FpsYear = tr.FpsYear
                    }).ToList();
                    if (newTestReqs.Count > 0)
                    {
                        await _dbContext.TestRequirements.AddRangeAsync(newTestReqs);
                        await _dbContext.SaveChangesAsync();

                        // Derived from UITrig_tlkpTestReqmt: log inserted rows to TestReq_LOG
                        _dbContext.TestRequirementLogs.AddRange(newTestReqs.Select(tr => new TestRequirementLog
                        {
                            TestCode = tr.TestCode,
                            Buyer = tr.Buyer,
                            UnitPrice = tr.UnitPrice,
                            NoRequired = tr.NoRequired,
                            ProjectBuyerCode = tr.ProjectBuyerCode,
                            TestBuyerCode = tr.TestBuyerCode,
                            Active = tr.Active,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "I",
                            FpsYear = tr.FpsYear
                        }));
                        await _dbContext.SaveChangesAsync();
                    }

                    // UPDATE remaining child tables

                    // Derived from MT_LOG_UTrig: log old state (UD) and new state (UI) before update
                    var mtToLog = await _dbContext.MonthlyTimes
                        .Where(mt => mt.ParentProject == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    if (mtToLog.Count > 0)
                    {
                        _dbContext.MonthlyTimeLogs.AddRange(mtToLog.Select(mt => new MonthlyTimeLog
                        {
                            PactStaffId = mt.PactStaffId,
                            TimeCode = mt.TimeCode,
                            Month = mt.Month,
                            ParentProject = mt.ParentProject,
                            WorkGroup = mt.WorkGroup,
                            Hours = mt.Hours,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "UD",
                            FpsYear = mt.FpsYear ?? _requestContext.FpsYear
                        }));
                        _dbContext.MonthlyTimeLogs.AddRange(mtToLog.Select(mt => new MonthlyTimeLog
                        {
                            PactStaffId = mt.PactStaffId,
                            TimeCode = mt.TimeCode == oldCode ? newCode : mt.TimeCode,
                            Month = mt.Month,
                            ParentProject = newCode,
                            WorkGroup = mt.WorkGroup,
                            Hours = mt.Hours,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "UI",
                            FpsYear = mt.FpsYear ?? _requestContext.FpsYear
                        }));
                        await _dbContext.SaveChangesAsync();
                    }
                    await _dbContext.MonthlyTimes
                        .Where(mt => mt.ParentProject == oldCode)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(x => x.ParentProject, newCode)
                            .SetProperty(x => x.TimeCode, x => x.TimeCode == oldCode ? newCode : x.TimeCode));

                    // Derived from MO_LOG_UTrig: log old state (UD) and new state (UI) before update
                    var moToLog = await _dbContext.MonthlyOutputs
                        .Where(mo => mo.Buyer == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    if (moToLog.Count > 0)
                    {
                        _dbContext.MonthlyOutputLogs.AddRange(moToLog.Select(mo => new MonthlyOutputLog
                        {
                            TestCode = mo.TestCode,
                            Buyer = mo.Buyer,
                            Month = mo.Month,
                            WorkGroup = mo.WorkGroup,
                            Volume = mo.Volume,
                            WgBuyer = mo.WgBuyer,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "UD",
                            FpsYear = mo.FpsYear
                        }));
                        _dbContext.MonthlyOutputLogs.AddRange(moToLog.Select(mo => new MonthlyOutputLog
                        {
                            TestCode = mo.TestCode,
                            Buyer = newCode,
                            Month = mo.Month,
                            WorkGroup = mo.WorkGroup,
                            Volume = mo.Volume,
                            WgBuyer = mo.WgBuyer,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "UI",
                            FpsYear = mo.FpsYear
                        }));
                        await _dbContext.SaveChangesAsync();
                    }
                    await _dbContext.MonthlyOutputs
                        .Where(mo => mo.Buyer == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Buyer, newCode));

                    // Derived from UITrig_tblAdditionalCosts: log new state ('I') before update
                    var acToLog = await _dbContext.AdditionalCosts
                        .Where(ac => ac.JobCode == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    if (acToLog.Count > 0)
                    {
                        _dbContext.AdditionalCostLogs.AddRange(acToLog.Select(ac => new AdditionalCostLog
                        {
                            JobCode = newCode,
                            Account = ac.Account,
                            Description = ac.Description,
                            ItemCost = ac.ItemCost,
                            Freq = ac.Freq,
                            Supplier = ac.Supplier,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "I",
                            FpsYear = ac.FpsYear ?? _requestContext.FpsYear
                        }));
                        await _dbContext.SaveChangesAsync();
                    }
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

                    // Derived from UITrig_tblAnimalReq: log new state ('I') before update
                    var arToLog = await _dbContext.AnimalRequests
                        .Where(ar => ar.JobCode == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    if (arToLog.Count > 0)
                    {
                        _dbContext.AnimalRequestLogs.AddRange(arToLog.Select(ar => new AnimalRequestLog
                        {
                            JobCode = newCode,
                            AnimalType = ar.AnimalType,
                            NumberOfDays = ar.NumberOfDays,
                            NumberOfAnimals = ar.NumberOfAnimals,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "I",
                            FpsYear = ar.FpsYear ?? _requestContext.FpsYear
                        }));
                        await _dbContext.SaveChangesAsync();
                    }
                    await _dbContext.AnimalRequests
                        .Where(ar => ar.JobCode == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));

                    await _dbContext.Milestones
                        .Where(m => m.Project == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));

                    // Derived from UITrig_tblStaffJob: log new state ('I') before update
                    var sjToLog = await _dbContext.StaffJobs
                        .Where(sj => sj.JobCode == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    if (sjToLog.Count > 0)
                    {
                        _dbContext.StaffJobLogs.AddRange(sjToLog.Select(sj => new StaffJobLog
                        {
                            StaffId = sj.StaffId,
                            JobCode = newCode,
                            PlannedHours = sj.PlannedHours,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "I",
                            FpsYear = sj.FpsYear ?? _requestContext.FpsYear
                        }));
                        await _dbContext.SaveChangesAsync();
                    }
                    await _dbContext.StaffJobs
                        .Where(sj => sj.JobCode == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));

                    await _dbContext.ProjectMonthFinals
                        .Where(pmf => pmf.Project == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));

                    // sp_Delete_tr, sp_Delete_tcv, sp_Delete_jc, sp_Delete_pp

                    // Derived from DTrig_tlkpTestReqmt: log deleted rows to TestReq_LOG before delete
                    var trToDelete = await _dbContext.TestRequirements
                        .Where(tr => tr.ProjectBuyerCode == oldCode)
                        .AsNoTracking()
                        .ToListAsync();
                    if (trToDelete.Count > 0)
                    {
                        _dbContext.TestRequirementLogs.AddRange(trToDelete.Select(tr => new TestRequirementLog
                        {
                            TestCode = tr.TestCode,
                            Buyer = tr.Buyer,
                            UnitPrice = tr.UnitPrice,
                            NoRequired = tr.NoRequired,
                            ProjectBuyerCode = tr.ProjectBuyerCode,
                            TestBuyerCode = tr.TestBuyerCode,
                            Active = tr.Active,
                            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                            UserId = _requestContext.UserEmailId,
                            InsertDelete = "D",
                            FpsYear = tr.FpsYear
                        }));
                        await _dbContext.SaveChangesAsync();
                    }
                    await _dbContext.TestRequirements
                        .Where(tr => tr.ProjectBuyerCode == oldCode)
                        .ExecuteDeleteAsync();
                    await _dbContext.TimeCodeValids
                        .Where(tcv => tcv.ParentProject == oldCode || tcv.Portfolio == oldCode)
                        .ExecuteDeleteAsync();
                    await _dbContext.JobCodes
                        .Where(jc => jc.ParentProject == oldCode)
                        .ExecuteDeleteAsync();
                    // Stage "D" audit log for the old project before deleting
                    var projectToDelete = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == oldCode);
                    if (projectToDelete != null)
                    {
                        NormalizeDateTimesToUnspecified(projectToDelete);
                        _dbContext.ProjectLogs.Add(MapProjectToLog(projectToDelete, "D", _requestContext.UserEmailId));
                        _dbContext.Projects.Remove(projectToDelete);
                        await _dbContext.SaveChangesAsync();
                    }

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
                    {
                        NormalizeDateTimesToUnspecified(project);
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

                        // sp_delete_tr — Derived from DTrig_tlkpTestReqmt: log before delete
                        var trToDelete = await _dbContext.TestRequirements
                            .Where(tr => tr.ProjectBuyerCode == parentProject)
                            .AsNoTracking()
                            .ToListAsync();
                        if (trToDelete.Count > 0)
                        {
                            _dbContext.TestRequirementLogs.AddRange(trToDelete.Select(tr => new TestRequirementLog
                            {
                                TestCode = tr.TestCode,
                                Buyer = tr.Buyer,
                                UnitPrice = tr.UnitPrice,
                                NoRequired = tr.NoRequired,
                                ProjectBuyerCode = tr.ProjectBuyerCode,
                                TestBuyerCode = tr.TestBuyerCode,
                                Active = tr.Active,
                                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                                UserId = _requestContext.UserEmailId,
                                InsertDelete = "D",
                                FpsYear = tr.FpsYear
                            }));
                            await _dbContext.SaveChangesAsync();
                        }
                        await _dbContext.TestRequirements
                            .Where(tr => tr.ProjectBuyerCode == parentProject)
                            .ExecuteDeleteAsync();

                        // sp_Delete_ar — Derived from DTrig_tblAnimalReq: log before delete
                        var arToDelete = await _dbContext.AnimalRequests
                            .Where(ar => ar.JobCode == parentProject)
                            .AsNoTracking()
                            .ToListAsync();
                        if (arToDelete.Count > 0)
                        {
                            _dbContext.AnimalRequestLogs.AddRange(arToDelete.Select(ar => new AnimalRequestLog
                            {
                                JobCode = ar.JobCode,
                                AnimalType = ar.AnimalType,
                                NumberOfDays = ar.NumberOfDays,
                                NumberOfAnimals = ar.NumberOfAnimals,
                                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                                UserId = _requestContext.UserEmailId,
                                InsertDelete = "D",
                                FpsYear = ar.FpsYear ?? _requestContext.FpsYear
                            }));
                            await _dbContext.SaveChangesAsync();
                        }
                        await _dbContext.AnimalRequests
                            .Where(ar => ar.JobCode == parentProject)
                            .ExecuteDeleteAsync();

                        // sp_Delete_sj — Derived from DTrig_tblStaffJob: log before delete
                        var sjToDelete = await _dbContext.StaffJobs
                            .Where(sj => sj.JobCode == parentProject)
                            .AsNoTracking()
                            .ToListAsync();
                        if (sjToDelete.Count > 0)
                        {
                            _dbContext.StaffJobLogs.AddRange(sjToDelete.Select(sj => new StaffJobLog
                            {
                                StaffId = sj.StaffId,
                                JobCode = sj.JobCode,
                                PlannedHours = sj.PlannedHours,
                                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                                UserId = _requestContext.UserEmailId,
                                InsertDelete = "D",
                                FpsYear = sj.FpsYear ?? _requestContext.FpsYear
                            }));
                            await _dbContext.SaveChangesAsync();
                        }
                        await _dbContext.StaffJobs
                            .Where(sj => sj.JobCode == parentProject)
                            .ExecuteDeleteAsync();

                        // sp_Delete_ac — Derived from DTrig_tblAdditionalCosts: log before delete
                        var acToDelete = await _dbContext.AdditionalCosts
                            .Where(ac => ac.JobCode == parentProject)
                            .AsNoTracking()
                            .ToListAsync();
                        if (acToDelete.Count > 0)
                        {
                            _dbContext.AdditionalCostLogs.AddRange(acToDelete.Select(ac => new AdditionalCostLog
                            {
                                JobCode = ac.JobCode,
                                Account = ac.Account,
                                Description = ac.Description,
                                ItemCost = ac.ItemCost,
                                Freq = ac.Freq,
                                Supplier = ac.Supplier,
                                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                                UserId = _requestContext.UserEmailId,
                                InsertDelete = "D",
                                FpsYear = ac.FpsYear ?? _requestContext.FpsYear
                            }));
                            await _dbContext.SaveChangesAsync();
                        }
                        await _dbContext.AdditionalCosts
                            .Where(ac => ac.JobCode == parentProject)
                            .ExecuteDeleteAsync();

                        // sp_Delete_pp
                        await _dbContext.Projects
                            .Where(p => p.ParentProject == parentProject)
                            .ExecuteDeleteAsync();
                    }

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

        private static ProjectLog MapProjectToLog(Project p, string operation, string userId) => new()
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
            DateCreated = p.DateCreated,
            FecCost = p.FecCost,
            Profit = p.Profit,
            BudgetCvl = p.BudgetCvl,
            DateCosted = p.DateCosted,
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
            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
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

        private record ProjectProfitabilityEntry(
            string? ParentProject,
            decimal? BudgetCvl,
            decimal? Profit,
            string? ProjectStatus,
            string? Program);

        /// <summary>
        /// Returns paginated project profitability data for a given programme.
        /// Translates qryProjectProfitability3: Projects + Programs + aggregate cost sub-queries.
        /// Staff costs sourced from TimeCostCalcsViews (vtimecostcalcs, grouped by Project).
        /// Animal costs from AnimalRequests joined to Animals for daily rate.
        /// Test costs from TestRequirements (NoRequired × UnitPrice per vtbltestrequ).
        /// Additional costs from AdditionalCosts (sum of ItemCost per JobCode).
        /// workTypeFilter: "all" | "approved" | "not-approved"
        /// </summary>
        public async Task<PagedData<ProjectProfitabilityView>> GetProjectProfitabilityAsync(
            PaginationParameters<string> query, string programNo, string workTypeFilter)
        {
            var projectQuery = _dbContext.ProjectViews
                .AsNoTracking()
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId) && p.Program == programNo);

            if (workTypeFilter == "approved")
                projectQuery = projectQuery.Where(p => p.ProjectStatus == "Approved");
            else if (workTypeFilter == "not-approved")
                projectQuery = projectQuery.Where(p => p.ProjectStatus == "Not Approved");

            projectQuery = ApplyProfitabilityFilter(projectQuery, query.Filter);

            var projects = await projectQuery
                .Select(p => new ProjectProfitabilityEntry(p.ParentProject, p.BudgetCvl, p.Profit, p.ProjectStatus, p.Program))
                .ToListAsync();

            if (projects.Count == 0)
                return ApplyPaging(new List<ProjectProfitabilityView>(), query.Page, query.PageSize);

            var programme = await _dbContext.Programs
                .AsNoTracking()
                .Where(pg => pg.ProgramNo == programNo)
                .Select(pg => new { pg.ProgramNo, pg.Target })
                .FirstOrDefaultAsync();

            var programmeTargetMap = programme != null
                ? new Dictionary<string, decimal?> { { programme.ProgramNo!, programme.Target } }
                : new Dictionary<string, decimal?>();

            return await ComputeProfitabilityAsync(query, projects, programmeTargetMap);
        }

        /// <summary>
        /// Returns paginated project profitability data for a given project group.
        /// Same cost logic as GetProjectProfitabilityAsync but filtered by ProjectGroup instead of ProgramNo.
        /// ProgrammeTarget is resolved per-project from each project's attached Programme.
        /// workTypeFilter: "all" | "approved" | "not-approved"
        /// </summary>
        public async Task<PagedData<ProjectProfitabilityView>> GetProjectGroupProfitabilityAsync(
            PaginationParameters<string> query, string projectGroup, string workTypeFilter)
        {
            var projectQuery = (from pg in _dbContext.ProjectGroupViews
                                join pv in _dbContext.Projects on
                                new { pg.ProjectGroupName } equals new { ProjectGroupName = pv.ProjectGroup }
                                where EF.Functions.ILike(pg.UserEmail!, _requestContext.UserEmailId)
                                      && pg.ProjectGroupName == projectGroup
                                select pv).AsQueryable();

            if (workTypeFilter == "approved")
                projectQuery = projectQuery.Where(p => p.ProjectStatus == "Approved");
            else if (workTypeFilter == "not-approved")
                projectQuery = projectQuery.Where(p => p.ProjectStatus == "Not Approved");

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);

            var projects = await projectQuery
                .Select(p => new ProjectProfitabilityEntry(p.ParentProject, p.BudgetCvl, p.Profit, p.ProjectStatus, p.Program))
                .ToListAsync();

            if (projects.Count == 0)
                return ApplyPaging(new List<ProjectProfitabilityView>(), query.Page, query.PageSize);

            var distinctProgramNos = projects
                .Select(p => p.Program)
                .Where(p => p != null)
                .Distinct()
                .ToList();

            var programmeTargetMap = await _dbContext.Programs
                .AsNoTracking()
                .Where(pg => distinctProgramNos.Contains(pg.ProgramNo))
                .ToDictionaryAsync(pg => pg.ProgramNo!, pg => pg.Target);

            return await ComputeProfitabilityAsync(query, projects, programmeTargetMap);
        }

        private async Task<PagedData<ProjectProfitabilityView>> ComputeProfitabilityAsync(
            PaginationParameters<string> query,
            List<ProjectProfitabilityEntry> projects,
            Dictionary<string, decimal?> programmeTargetMap)
        {
            var projectCodes = projects.Select(p => p.ParentProject).ToList();

            // Calculate staff costs by summing Cost from TimeCostCalcsViews per Project (JobCode)
            var staffCosts = await (
                from sj in _dbContext.StaffJobs
                join wge in _dbContext.WorkGroupEmployees
                    on sj.StaffId equals wge.PactId                   
                join wgg in _dbContext.WorkgroupGrades
                    on wge.WorkGroupGrade equals wgg.WgGrade 
                join pcg in _dbContext.ProfitCentreGrades
                    on wgg.ProfitCentreGrade equals pcg.PcGrade                   
                join p in _dbContext.Projects
                    on sj.JobCode equals p.ParentProject
                join pg in _dbContext.Programs
                    on p.Program equals pg.ProgramNo                                      
                where projectCodes.Contains(sj.JobCode)
                    && pg != null
                    && EF.Functions.ILike(pg.SectorName!, "%charge%")
                select new
                {                    
                    sectorCharge = (pg.SectorName ?? "").Trim().ToLower() == "charge" ? 1m : 0m,
                    JobCode = sj.JobCode,                    
                    PlannedHours = sj.PlannedHours,
                    ChargeRate = p.IsDefraProject == 0 ? pcg.ChargeRate : pcg.DefraChargeRate
                })
                .ToListAsync();            

            // Additional costs by summing ItemCost per JobCode from AdditionalCosts
            var additionalCosts = await _dbContext.AdditionalCosts
                .AsNoTracking()
                .Where(ac => projectCodes.Contains(ac.JobCode))
                .GroupBy(ac => ac.JobCode)
                .Select(g => new { JobCode = g.Key, TotalAdditional = g.Sum(x => x.ItemCost) })
                .ToListAsync();

            //Calculate test costs by multiplying NoRequired by UnitPrice for each TestRequirement, then summing per JobCode
            var testCostsRaw = await _dbContext.TestRequirements
                .AsNoTracking()
                .Where(tr => projectCodes.Contains(tr.Buyer))
                .Select(tr => new
                {
                    tr.Buyer,
                    NoRequired = Convert.ToDecimal(tr.NoRequired ?? 0d),
                    UnitPrice = Convert.ToDecimal(tr.UnitPrice ?? 0m)
                })
                .ToListAsync();

            var testCosts = testCostsRaw
                .GroupBy(tr => tr.Buyer)
                .Select(g => new { JobCode = g.Key, TotalTest = g.Sum(x => x.NoRequired * x.UnitPrice) })
                .ToList();

            // Calculate animal costs: NumberOfAnimals × NumberOfDays × (IsDefraProject=0 ? DailyRate : DefraDailyRate)           
            var animalCostsRaw = await (
                from ar in _dbContext.AnimalRequests
                join p in _dbContext.Projects
                    on ar.JobCode equals p.ParentProject                    
                join a in _dbContext.Animals
                    on ar.AnimalType equals a.AnimalType                  
                where projectCodes.Contains(ar.JobCode)
                select new
                {
                    ar.JobCode,
                    ar.NumberOfAnimals,
                    ar.NumberOfDays,
                    Cost = p.IsDefraProject == 0 ? a.DailyRate : a.DefraDailyRate
                })
                .ToListAsync();

            var animalCostByJob = animalCostsRaw
                .GroupBy(x => x.JobCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => (decimal)(x.NumberOfAnimals * x.NumberOfDays) * (x.Cost ?? 0m)));

            var staffMap = staffCosts
                .Where(e => e.sectorCharge == 1m)
                .GroupBy(x => x.JobCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => (decimal)x.PlannedHours * (x.ChargeRate ?? 0m) * x.sectorCharge));
            var additionalMap = additionalCosts.ToDictionary(x => x.JobCode, x => x.TotalAdditional);
            var testMap = testCosts.ToDictionary(x => x.JobCode, x => x.TotalTest);

            var results = projects.Select(p =>
            {
                var staff = staffMap.TryGetValue(p.ParentProject!, out var s) ? s : 0m;
                var additional = additionalMap.TryGetValue(p.ParentProject!, out var a) ? a : 0m;
                var test = testMap.TryGetValue(p.ParentProject!, out var t) ? t : 0m;
                var animal = animalCostByJob.TryGetValue(p.ParentProject!, out var an) ? an : 0m;
                var total = staff + additional + test + animal;
                var budget = p.BudgetCvl ?? 0m;
                var profit = p.Profit ?? 0m;
                var jcProfit = budget - total;
                return new ProjectProfitabilityView
                {
                    JobCode = p.ParentProject!,
                    JcTotalStaffCosts = staff,
                    JcTotalTestCosts = test,
                    JcTotalAnimalCosts = animal,
                    JcTotalAdditionalCosts = additional,
                    TotalCosts = total,
                    BudgetCvl = p.BudgetCvl,
                    JcProfit = jcProfit,
                    TargetProfit = profit,
                    OffTarget = jcProfit - profit,
                    ProgramNo = p.Program,
                    ProgrammeTarget = p.Program != null && programmeTargetMap.TryGetValue(p.Program, out var tgt) ? tgt : null,
                    ProjectStatus = p.ProjectStatus
                };
            }).ToList();

            // Apply sorting
            results = query.SortBy?.ToLower() switch
            {
                "jobcode" => query.Descending ? results.OrderByDescending(r => r.JobCode).ToList() : results.OrderBy(r => r.JobCode).ToList(),
                "totalcosts" => query.Descending ? results.OrderByDescending(r => r.TotalCosts).ToList() : results.OrderBy(r => r.TotalCosts).ToList(),
                "budgetcvl" => query.Descending ? results.OrderByDescending(r => r.BudgetCvl).ToList() : results.OrderBy(r => r.BudgetCvl).ToList(),
                "jcprofit" => query.Descending ? results.OrderByDescending(r => r.JcProfit).ToList() : results.OrderBy(r => r.JcProfit).ToList(),
                "offtarget" => query.Descending ? results.OrderByDescending(r => r.OffTarget).ToList() : results.OrderBy(r => r.OffTarget).ToList(),
                "projectstatus" => query.Descending ? results.OrderByDescending(r => r.ProjectStatus).ToList() : results.OrderBy(r => r.ProjectStatus).ToList(),
                "jctotalstaffcosts" => query.Descending ? results.OrderByDescending(r => r.JcTotalStaffCosts).ToList() : results.OrderBy(r => r.JcTotalStaffCosts).ToList(),
                "jctotaltestcosts" => query.Descending ? results.OrderByDescending(r => r.JcTotalTestCosts).ToList() : results.OrderBy(r => r.JcTotalTestCosts).ToList(),
                "jctotalanimalcosts" => query.Descending ? results.OrderByDescending(r => r.JcTotalAnimalCosts).ToList() : results.OrderBy(r => r.JcTotalAnimalCosts).ToList(),
                "jctotaladditionalcosts" => query.Descending ? results.OrderByDescending(r => r.JcTotalAdditionalCosts).ToList() : results.OrderBy(r => r.JcTotalAdditionalCosts).ToList(),
                "targetprofit" => query.Descending ? results.OrderByDescending(r => r.TargetProfit).ToList() : results.OrderBy(r => r.TargetProfit).ToList(),
                _ => results.OrderBy(r => r.JobCode).ToList()
            };

            return ApplyPaging(results, query.Page, query.PageSize);
        }

        // ── VLA Project Profitability ─────────────────────────────────────────

        /// <summary>
        /// Returns paginated project profitability data for the VLA view.
        /// Translates frmJobcodeTotalsVLA / qryJobCodeTotals + qryJobCodeTotals2:
        /// queries the pre-computed <c>vprojectprofitabilityvla</c> PostgreSQL view
        /// which aggregates staff, test, animal, and additional costs per job code and
        /// joins tlkpProgram for Manager and Target (TargetProfit).
        ///
        /// Filter dimensions (all optional, case-insensitive):
        ///   ProjectStatus — filterProjectStatus in the HTML prototype
        ///   ProgramNo     — filterProgram
        ///   Manager       — filterManager (VLA-specific, not present in base profitability)
        ///   Customer      — filterCustomer (VLA-specific, not present in base profitability)
        /// </summary>
        public async Task<PagedData<ProjectProfitabilityVlaView>> GetProjectProfitabilityVlaAsync(
            PaginationParameters<ProjectProfitabilityVlaReq> query)
        {
            // TRANSFORMENGINE: query pre-computed vprojectprofitabilityvla view;
            //   cost aggregation (staff / test / animal / additional) is embedded in
            //   the view definition derived from qryJobCodeTotals + qryJobCodeTotals2 —
            //   no in-memory computation needed here.
            var q = _dbContext.ProjectProfitabilityVlaViews
                .AsNoTracking()
                .AsQueryable();

            // TRANSFORMENGINE: apply VLA filter dimensions from ProjectProfitabilityVlaReq
            var filter = query.Filter;
            if (filter != null)
            {
                // filterProjectStatus — static values: Approved, Completed, Not Approved
                if (!string.IsNullOrWhiteSpace(filter.ProjectStatus))
                    q = q.Where(v => EF.Functions.ILike(v.Status!, $"%{filter.ProjectStatus}%"));

                // filterProgram — matches ProgramNo / Programme column in the view
                if (!string.IsNullOrWhiteSpace(filter.ProgramNo))
                    q = q.Where(v => EF.Functions.ILike(v.Program!, $"%{filter.ProgramNo}%"));

                // filterManager — VLA-specific dimension from tlkpProgram.Manager via qryJobCodeTotals2
                if (!string.IsNullOrWhiteSpace(filter.Manager))
                    q = q.Where(v => EF.Functions.ILike(v.Manager!, $"%{filter.Manager}%"));

                // filterCustomer — VLA-specific dimension from tlkpProject.Customer
                if (!string.IsNullOrWhiteSpace(filter.Customer))
                    q = q.Where(v => EF.Functions.ILike(v.Customer!, $"%{filter.Customer}%"));
            }

            // TRANSFORMENGINE: search — applied to JobCode (project code column)
            if (!string.IsNullOrWhiteSpace(query.Search))
                q = q.Where(v => EF.Functions.ILike(v.JobCode!, $"%{query.Search}%"));

            // TRANSFORMENGINE: sorting — covers all 14 DataGrid columns from projectprofitability_vla.js
            q = query.SortBy?.ToLower() switch
            {
                "jobcode"           => query.Descending ? q.OrderByDescending(v => v.JobCode)           : q.OrderBy(v => v.JobCode),
                "program"           => query.Descending ? q.OrderByDescending(v => v.Program)           : q.OrderBy(v => v.Program),
                "customer"          => query.Descending ? q.OrderByDescending(v => v.Customer)          : q.OrderBy(v => v.Customer),
                "manager"           => query.Descending ? q.OrderByDescending(v => v.Manager)           : q.OrderBy(v => v.Manager),
                "status"            => query.Descending ? q.OrderByDescending(v => v.Status)            : q.OrderBy(v => v.Status),
                "staffcosts"        => query.Descending ? q.OrderByDescending(v => v.StaffCosts)        : q.OrderBy(v => v.StaffCosts),
                "testcost"          => query.Descending ? q.OrderByDescending(v => v.TestCost)          : q.OrderBy(v => v.TestCost),
                "animalcosts"       => query.Descending ? q.OrderByDescending(v => v.AnimalCosts)       : q.OrderBy(v => v.AnimalCosts),
                "additionalcosts"   => query.Descending ? q.OrderByDescending(v => v.AdditionalCosts)   : q.OrderBy(v => v.AdditionalCosts),
                "totalcosts"        => query.Descending ? q.OrderByDescending(v => v.TotalCosts)        : q.OrderBy(v => v.TotalCosts),
                "budget"            => query.Descending ? q.OrderByDescending(v => v.Budget)            : q.OrderBy(v => v.Budget),
                "profit"            => query.Descending ? q.OrderByDescending(v => v.Profit)            : q.OrderBy(v => v.Profit),
                "targetprofit"      => query.Descending ? q.OrderByDescending(v => v.TargetProfit)      : q.OrderBy(v => v.TargetProfit),
                "offtarget"         => query.Descending ? q.OrderByDescending(v => v.OffTarget)         : q.OrderBy(v => v.OffTarget),
                _                   => q.OrderBy(v => v.JobCode)    // default: ascending by job code
            };

            var results = await q.ToListAsync();
            return ApplyPaging(results, query.Page, query.PageSize);
        }
    }
}