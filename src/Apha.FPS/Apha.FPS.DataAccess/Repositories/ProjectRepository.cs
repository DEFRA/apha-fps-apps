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

        public async Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo)
        {
            var projectQuery = _dbContext.ProjectViews
                .Where(p => p.UserEmail != null && p.UserEmail.ToLower() == _requestContext.UserEmailId && p.Program == programNo)
                .Select(pv => new Project
                {
                    ParentProject = pv.ParentProject ?? string.Empty,
                    ProjectTitle = pv.ProjectTitle ?? string.Empty,
                    Program = pv.Program ?? string.Empty,
                    BudgetCvl = pv.BudgetCvl,
                    IsDefraProject = pv.IsDefraProject ?? 0
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

        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.FpsYear = _requestContext.FpsYear;
            await _dbContext.Projects.AddAsync(project);
            // Converted trigger logic — UITrig_tlkpProject FOR INSERT: stage audit log in same unit of work
            _dbContext.ProjectLogs.Add(MapProjectToLog(project, "I"));
            await _dbContext.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            project.FpsYear = _requestContext.FpsYear;
            _dbContext.Entry(project).State = EntityState.Modified;
            // Converted trigger logic — UITrig_tlkpProject FOR UPDATE: stage audit log in same unit of work
            _dbContext.ProjectLogs.Add(MapProjectToLog(project, "U"));
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
            _dbContext.ProjectLogs.Add(MapProjectToLog(entity, "U"));
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
            _dbContext.ProjectLogs.Add(MapProjectToLog(project, "D"));
            _dbContext.Projects.Remove(project);
            await _dbContext.SaveChangesAsync();
            return true;
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

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => x.ParentProject.Contains(jobCode.ToString()!));

            if (dict.TryGetValue("JobDescription", out var jobDescription) && jobDescription != null)
                query = query.Where(x => x.ProjectTitle!.Contains(jobDescription.ToString()!));

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

        /// <summary>
        /// Renames a project code and updates all child table references — derived from usp_ChangeProjectCode.
        /// UITrig_tlkpProject FOR INSERT appended: stages audit log entry for the new project row.
        /// </summary>
        public async Task ChangeProjectCodeAsync(string oldCode, string newCode)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Guard: new code must not exist
                    bool newExists = await _dbContext.Projects.AnyAsync(p => p.ParentProject == newCode);
                    if (newExists)
                        throw new InvalidOperationException("This code is already in use.");

                    // Copy project row (INSERT … SELECT)
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"""
                 INSERT INTO fps.tlkpproject
                   (parentproject, projecttitle, program, customer, manager, transferincome, custincome,
                    wip_eoy, wip_limit, wip_current, projectstatus, costbookno, feccost, profit, budget_cvl,
                    datecosted, disease, contract, projectparent, shorttitle, caseworksub, pvsincome,
                    plancaseworkdebit, finished, owningrc, comments, carryover, carryoverseed, isdefraproject,
                    costcentre, oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode, fpsyear)
                 SELECT {newCode}, projecttitle, program, customer, manager, transferincome, custincome,
                   wip_eoy, wip_limit, wip_current, projectstatus, costbookno, feccost, profit, budget_cvl,
                   datecosted, disease, contract, projectparent, shorttitle, caseworksub, pvsincome,
                   plancaseworkdebit, finished, owningrc, comments, carryover, carryoverseed, isdefraproject,
                   costcentre, oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode, fpsyear
                 FROM fps.tlkpproject WHERE parentproject = {oldCode}
                 """);

                    // Converted trigger logic — UITrig_tlkpProject FOR INSERT: insert audit log
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"""
                 INSERT INTO fps.project_log
                   (parentproject, projecttitle, program, customer, manager, transferincome, custincome,
                    wip_eoy, wip_limit, wip_current, projectstatus, costbookno, feccost, profit, budget_cvl,
                    datecosted, disease, contract, projectparent, shorttitle, caseworksub, pvsincome,
                    plancaseworkdebit, finished, owningrc, comments, carryover, carryoverseed,
                    date_time, user_id, insert_delete, jobcode, isdefraproject, costcentre,
                    oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode, fpsyear)
                 SELECT parentproject, projecttitle, program, customer, manager, transferincome, custincome,
                   wip_eoy, wip_limit, wip_current, projectstatus, costbookno, feccost, profit, budget_cvl,
                   datecosted, disease, contract, projectparent, shorttitle, caseworksub, pvsincome,
                   plancaseworkdebit, finished, owningrc, comments, carryover, carryoverseed,
                   NOW(), current_user, 'I', parentproject, isdefraproject, costcentre,
                   oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode, fpsyear
                 FROM fps.tlkpproject WHERE parentproject = {newCode}
                 """);

                    // INSERT new JobCode rows
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"""
                 INSERT INTO fps.tlkpjobcode (jobcode, parentproject, jobcodeworkgroup, newprog, type, jobcodename, fpsyear)
                 SELECT CASE jobcode WHEN {oldCode} THEN {newCode} ELSE jobcode END,
                   {newCode}, jobcodeworkgroup, newprog, type, jobcodename, fpsyear
                 FROM fps.tlkpjobcode WHERE parentproject = {oldCode}
                 """);

                    // UPDATE tlkpTestCapability.planportfolio
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.tlkptestcapability SET planportfolio = {newCode} WHERE planportfolio = {oldCode}");

                    // sp_Insert_tcv: copy TimeCodeValid rows with code substitution
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"""
                 INSERT INTO fps.timecodevalid (workgroup, timecode, parentproject, testcode, jobcode, portfolio, active, fpsyear)
                 SELECT DISTINCT workgroup,
                   CASE timecode WHEN {oldCode} THEN {newCode} ELSE timecode END,
                   CASE parentproject WHEN {oldCode} THEN {newCode} ELSE parentproject END,
                   testcode,
                   CASE jobcode WHEN {oldCode} THEN {newCode} ELSE jobcode END,
                   CASE portfolio WHEN {oldCode} THEN {newCode} ELSE portfolio END,
                   active, fpsyear
                 FROM fps.timecodevalid
                 WHERE parentproject = {oldCode} OR portfolio = {oldCode}
                 """);

                    // sp_Insert_tr: copy tlkpTestReqmt rows with new buyer code
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"""
                 INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, testbuyercode, datecreated, active, fpsyear)
                 SELECT testcode, {newCode}, unitprice, norequired, {newCode}, testbuyercode, datecreated, active, fpsyear
                 FROM fps.tlkptestreqmt WHERE projectbuyercode = {oldCode}
                 """);

                    // UPDATE remaining child tables
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"""
                 UPDATE fps.monthlytime
                 SET parentproject = {newCode}, timecode = CASE timecode WHEN {oldCode} THEN {newCode} ELSE timecode END
                 WHERE parentproject = {oldCode}
                 """);
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.monthlyoutput SET buyer = {newCode} WHERE buyer = {oldCode}");
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.tbladditionalcosts SET jobcode = {newCode} WHERE jobcode = {oldCode}");
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.proj_invoice SET projectparent = {newCode} WHERE projectparent = {oldCode}");
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.proj_subcontract SET project = {newCode} WHERE project = {oldCode}");
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"""
                 UPDATE fps.timecostcalcs
                 SET project = {newCode}, jobcode = CASE jobcode WHEN {oldCode} THEN {newCode} ELSE jobcode END
                 WHERE project = {oldCode}
                 """);
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.projectmonth SET project = {newCode} WHERE project = {oldCode}");
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.tblanimalreq SET jobcode = {newCode} WHERE jobcode = {oldCode}");
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.milestone SET project = {newCode} WHERE project = {oldCode}");

                    // UPDATE tblStaffJob (existing DbSet — use LINQ for type safety)
                    await _dbContext.StaffJobs
                        .Where(sj => sj.JobCode == oldCode)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));

                    await _dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE fps.projectmonthfinal SET project = {newCode} WHERE project = {oldCode}");

                    // sp_Delete_tr, sp_Delete_tcv, sp_Delete_jc, sp_Delete_pp
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"DELETE FROM fps.tlkptestreqmt WHERE projectbuyercode = {oldCode}");
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"DELETE FROM fps.timecodevalid WHERE parentproject = {oldCode} OR portfolio = {oldCode}");
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
        /// tlkpProject_DTrig guard: rejects if tlkpTestReqmt has planned tests.
        /// DTrig_tlkpProject (DELETE) appended: stages audit log entry.
        /// </summary>
        /// <summary>
        /// Deletes a project and all dependent child records — derived from usp_Delete_Project.
        /// tlkpProject_DTrig guard: rejects if tlkpTestReqmt has planned tests.
        /// DTrig_tlkpProject (DELETE) appended: stages audit log entry.
        /// </summary>
        public async Task DeleteProjectAndChildrenAsync(string parentProject)
        {
            // Guard: tlkpProject_DTrig custom DRI — reject if planned tests exist
            bool hasTests = await _dbContext.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*)::int AS \"Value\" FROM fps.tlkptestreqmt WHERE projectbuyercode = @p",
                    new NpgsqlParameter("p", parentProject))
                .AnyAsync(c => c > 0);
            if (hasTests)
                throw new InvalidOperationException("Cannot delete project, it still has tests planned.");

            // Guard checks from usp_Delete_Project
            bool hasMonthlyOutput = await _dbContext.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*)::int AS \"Value\" FROM fps.monthlyoutput WHERE buyer = @p",
                    new NpgsqlParameter("p", parentProject))
                .AnyAsync(c => c > 0);

            bool hasMonthlyTime = await _dbContext.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*)::int AS \"Value\" FROM fps.monthlytime WHERE parentproject = @p",
                    new NpgsqlParameter("p", parentProject))
                .AnyAsync(c => c > 0);

            bool hasProjInvoice = await _dbContext.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*)::int AS \"Value\" FROM fps.proj_invoice WHERE projectparent = @p",
                    new NpgsqlParameter("p", parentProject))
                .AnyAsync(c => c > 0);

            bool hasProjSubcontract = await _dbContext.Database.SqlQueryRaw<int>(
                    "SELECT COUNT(*)::int AS \"Value\" FROM fps.proj_subcontract WHERE project = @p",
                    new NpgsqlParameter("p", parentProject))
                .AnyAsync(c => c > 0);

            var errorParts = new List<string>();
            if (hasMonthlyOutput) errorParts.Add("Monthly Tests");
            if (hasMonthlyTime) errorParts.Add("Monthly Time");
            if (hasProjInvoice) errorParts.Add("Invoice");
            if (hasProjSubcontract) errorParts.Add("Subcontracts");

            if (errorParts.Count > 0)
                throw new InvalidOperationException(
                    $"This project cannot be deleted, there are records in {string.Join(", ", errorParts)}.");

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Converted trigger logic — DTrig_tlkpProject FOR DELETE: insert audit log before delete
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"""
                 INSERT INTO fps.project_log
                   (parentproject, projecttitle, program, customer, manager, transferincome, custincome,
                    wip_eoy, wip_limit, wip_current, projectstatus, costbookno, feccost, profit, budget_cvl,
                    datecosted, disease, contract, projectparent, shorttitle, caseworksub, pvsincome,
                    plancaseworkdebit, finished, owningrc, comments, carryover, carryoverseed,
                    date_time, user_id, insert_delete, jobcode, isdefraproject, costcentre,
                    oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode, fpsyear)
                 SELECT parentproject, projecttitle, program, customer, manager, transferincome, custincome,
                   wip_eoy, wip_limit, wip_current, projectstatus, costbookno, feccost, profit, budget_cvl,
                   datecosted, disease, contract, projectparent, shorttitle, caseworksub, pvsincome,
                   plancaseworkdebit, finished, owningrc, comments, carryover, carryoverseed,
                   NOW(), current_user, 'D', parentproject, isdefraproject, costcentre,
                   oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode, fpsyear
                 FROM fps.tlkpproject WHERE parentproject = {parentProject}
                 """);

                    // sp_Delete_tcv
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"DELETE FROM fps.timecodevalid WHERE parentproject = {parentProject} OR portfolio = {parentProject}");

                    // sp_Delete_JC
                    await _dbContext.JobCodes
                        .Where(jc => jc.ParentProject == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_delete_tr
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"DELETE FROM fps.tlkptestreqmt WHERE projectbuyercode = {parentProject}");

                    // sp_Delete_ar
                    await _dbContext.AnimalRequests
                        .Where(ar => ar.JobCode == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_Delete_sj
                    await _dbContext.StaffJobs
                        .Where(sj => sj.JobCode == parentProject)
                        .ExecuteDeleteAsync();

                    // sp_Delete_ac
                    await _dbContext.Database.ExecuteSqlAsync(
                        $"DELETE FROM fps.tbladditionalcosts WHERE jobcode = {parentProject}");

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

        private static ProjectLog MapProjectToLog(Project p, string operation) => new()
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
            DateTime = DateTime.UtcNow,
            InsertDelete = operation,
            JobCode = p.ParentProject,
            IsDefraProject = p.IsDefraProject,
            CostCentre = p.CostCentre,
            OracleProjectCode = p.OracleProjectCode,
            SubAccountCode = p.SubAccountCode,
            ProjectGroup = p.ProjectGroup,
            IncomeAccountCode = p.IncomeAccountCode,
            FpsYear = p.FpsYear
        };
    }
}
