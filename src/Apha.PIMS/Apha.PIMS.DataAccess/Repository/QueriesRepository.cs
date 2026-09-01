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
    public class QueriesRepository : BaseRepository, IQueriesRepository
    {
        public QueriesRepository(PimsDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Get all query reports (Type = 'Q') ordered by ReportDescription and SortOrder
        /// Equivalent SQL: SELECT DISTINCTROW ... FROM tblReport WHERE Type='Q' ORDER BY ReportDescription, SortOrder
        /// </summary>
        /// <returns>List of QueryReportItem</returns>
        public async Task<List<QueryReportItem>> GetQueryReportsAsync()
        {
            return await _context.Reports
                .AsNoTracking()
                .Where(r => r.Type == "Q")
                .Select(r => new QueryReportItem
                {
                    ReportName = r.ReportName,
                    ReportDescription = r.ReportDescription,
                    Emailable = r.Emailable,
                    AllowPickProgramme = r.AllowPickProgramme,
                    AllowPickProject = r.AllowPickProject,
                    AllowPickManager = r.AllowPickManager,
                    AllowPickContract = r.AllowPickContract,
                    AllowPickCustomer = r.AllowPickCustomer,
                    ReportHelp = r.ReportHelp,
                    Filter = r.Filter
                })
                .Distinct()
                .OrderBy(r => r.ReportDescription)
                .ThenBy(r => r.ReportName)
                .ToListAsync();
        }

        /// <summary>
        /// Get monitoring report data for a given year and fiscal month, filtered by contract and program.
        /// Combines data from RadTrack contracts, project details, year totals, month-final costs, and monitoring comments.
        /// 
        /// Equivalent to the SQL query:
        /// SELECT MY_tlkpProject.Program, MY_tlkpProject.ParentProject, G_tlkpProject.ProjectTitle,
        ///        MY_tlkpProject.Manager, MY_tlkpProject.ProjectStatus, G_tlkpProject.Contract,
        ///        MY_FPSYearTotals.TotalCosts AS TotalPlanCosts, MY_ProjectMonthFinal.CumCost AS TotalYTDCosts,
        ///        qryPMonitoringComments.Comment
        /// FROM (((tblRadtrackContract INNER JOIN (MY_tlkpProject INNER JOIN G_tlkpProject ON ...) ON ...)
        ///        INNER JOIN MY_FPSYearTotals ON ...) INNER JOIN MY_ProjectMonthFinal ON ...)
        /// LEFT JOIN qryPMonitoringComments ON ...
        /// WHERE ... ORDER BY MY_tlkpProject.ParentProject
        /// </summary>
        public async Task<PagedData<MonitoringReportData>> GetAllContractsMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null)
        {
            IQueryable<MonitoringReportData> query = BuildMonitoringReportQuery(
                reportYear,
                fiscalMonth,
                contractFilter,
                programFilter,
                applySurveillanceProgramFilter: true);

            query = ApplyFilter(query, parameters.Filter);
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        public async Task<PagedData<MonitoringReportData>> GetContractsMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null)
        {
            IQueryable<MonitoringReportData> query = BuildMonitoringReportQuery(
                reportYear,
                fiscalMonth,
                contractFilter,
                programFilter,
                applySurveillanceProgramFilter: false);

            query = ApplyFilter(query, parameters.Filter);
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        public async Task<PagedData<MonitoringReportData>> GetMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null)
        {
            string? exportQueryType = GetExportQueryType(parameters.Filter);

            return string.Equals(exportQueryType, "allContract", StringComparison.OrdinalIgnoreCase)
                ? await GetAllContractsMonitoringReportDataAsync(parameters, reportYear, fiscalMonth, contractFilter, programFilter)
                : await GetContractsMonitoringReportDataAsync(parameters, reportYear, fiscalMonth, contractFilter, programFilter);
        }

        public async Task<PagedData<ProgramCustomerMonitoringReportData>> GetProgramCustomerMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            IEnumerable<string>? programFilter = null)
        {
            IQueryable<ProgramCustomerMonitoringReportData> query = BuildProgramAndCustomerMonitoringQuery(
                reportYear,
                fiscalMonth,
                programFilter);

            query = ApplyProgramCustomerMonitoringFilter(query, parameters.Filter);
            query = ApplyProgramCustomerMonitoringSorting(query, parameters.SortBy, parameters.Descending);

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        /// <summary>
        /// Build the base monitoring report query with all required joins.
        /// </summary>
        private IQueryable<MonitoringReportData> BuildMonitoringReportQuery(
            short reportYear,
            double fiscalMonth,
            string contractFilter,
            IEnumerable<string>? programFilter,
            bool applySurveillanceProgramFilter)
        {
            var monitoringComments = _context.Comments
                .AsNoTracking()
                .Where(c => c.Topic == "P&C Monitoring Report");

            var query = from myProject in _context.MyTlkpProjects.AsNoTracking()
                        join gProject in _context.Projects.AsNoTracking()
                            on myProject.Parentproject.Trim() equals gProject.Parentproject.Trim()
                        join radTrackContract in _context.RadTrackContracts.AsNoTracking()
                            on gProject.Contract.Trim() equals radTrackContract.Contract.Trim()
                        join yearTotal in _context.FpsYearTotals.AsNoTracking()
                            on new { myProject.Year, Parentproject = gProject.Parentproject.Trim() }
                            equals new { yearTotal.Year, Parentproject = yearTotal.Parentproject.Trim() }
                        join monthFinal in _context.ProjectMonthFinals.AsNoTracking()
                            on new { myProject.Year, Project = myProject.Parentproject.Trim() }
                            equals new { monthFinal.Year, Project = monthFinal.Project.Trim() }
                        join comment in monitoringComments
                            on new { Project = myProject.Parentproject.Trim(), myProject.Year }
                            equals new { Project = comment.Project.Trim(), comment.Year }
                            into commentGroup
                        from comment in commentGroup.DefaultIfEmpty()
                        where myProject.Year == reportYear
                              && monthFinal.Monthno == fiscalMonth
                              && (!applySurveillanceProgramFilter
                                  || (myProject.Program != null
                                      && myProject.Program.ToUpper().EndsWith("SURV")))
                        select new MonitoringReportData
                        {
                            Year = myProject.Year,
                            Project = myProject.Parentproject,
                            Program = myProject.Program,
                            ParentProject = myProject.Parentproject,
                            ProjectTitle = gProject.Projecttitle,
                            Manager = myProject.Manager,
                            ProjectStatus = myProject.Projectstatus,
                            Contract = gProject.Contract,
                            TotalPlanCosts = yearTotal.Totalcosts.HasValue
                                ? Convert.ToDecimal(yearTotal.Totalcosts.Value)
                                : null,
                            TotalYtdCosts = monthFinal.Cumcost,
                            MonitoringComment = comment != null ? comment.CommentText : null
                        };

            query = ApplyContractFilter(query, contractFilter);

            if (programFilter != null && programFilter.Any())
            {
                query = query.Where(r => r.Program != null && programFilter.Contains(r.Program));
            }

            return query
                .OrderBy(r => r.ParentProject);
        }

        private IQueryable<ProgramCustomerMonitoringReportData> BuildProgramAndCustomerMonitoringQuery(
            short reportYear,
            double fiscalMonth,
            IEnumerable<string>? programFilter)
        {
            // qryPMonitoringComments: SELECT Project, Year, Comment FROM tblComments WHERE Topic='P&C Monitoring Report'
            var monitoringComments = _context.Comments
                .AsNoTracking()
                .Where(c => c.Topic == "P&C Monitoring Report");

            var query = from myProject in _context.MyTlkpProjects.AsNoTracking()
                        join gProject in _context.Projects.AsNoTracking()
                            on myProject.Parentproject.Trim() equals gProject.Parentproject.Trim()
                        join yearTotalJoin in _context.FpsYearTotals.AsNoTracking()
                            on new { myProject.Year, Parentproject = myProject.Parentproject.Trim() }
                            equals new { yearTotalJoin.Year, Parentproject = yearTotalJoin.Parentproject.Trim() }
                            into yearTotalGroup
                        from yearTotal in yearTotalGroup.DefaultIfEmpty()
                        join monthFinalJoin in _context.ProjectMonthFinals.AsNoTracking()
                            on new { myProject.Year, Project = myProject.Parentproject.Trim() }
                            equals new { Year = monthFinalJoin.Year, Project = monthFinalJoin.Project.Trim() }
                            into monthFinalGroup
                        from monthFinal in monthFinalGroup.DefaultIfEmpty()
                        join yearlyDataJoin in _context.YearlyFinancialData.AsNoTracking()
                            on new { myProject.Year, Project = myProject.Parentproject.Trim() }
                            equals new { yearlyDataJoin.Year, Project = yearlyDataJoin.Project.Trim() }
                            into yearlyDataGroup
                        from yearlyData in yearlyDataGroup.DefaultIfEmpty()
                        join radTrackDataJoin in _context.ProjectRadTrackData.AsNoTracking()
                            on myProject.Parentproject.Trim() equals radTrackDataJoin.Parentproject.Trim()
                            into radTrackDataGroup
                        from radTrackData in radTrackDataGroup.DefaultIfEmpty()
                        join commentJoin in monitoringComments
                            on new { myProject.Year, Project = myProject.Parentproject.Trim() }
                            equals new { commentJoin.Year, Project = commentJoin.Project.Trim() }
                            into commentGroup
                        from comment in commentGroup.DefaultIfEmpty()
                        where myProject.Year == reportYear
                              && (monthFinal == null || monthFinal.Monthno == fiscalMonth)
                        select new ProgramCustomerMonitoringReportData
                        {
                            Year = myProject.Year,
                            Project = myProject.Parentproject,
                            Program = myProject.Program,
                            ParentProject = myProject.Parentproject,
                            ProjectTitle = gProject.Projecttitle,
                            Manager = myProject.Manager,
                            ProjectStatus = myProject.Projectstatus,
                            Customer = myProject.Customer,
                            Contract = gProject.Contract,
                            PlannedCosts = yearTotal != null && yearTotal.Totalcosts.HasValue
                                ? Convert.ToDecimal(yearTotal.Totalcosts.Value)
                                : null,
                            BudgetCvl = myProject.BudgetCvl,
                            CustIncome = myProject.Custincome,
                            ActualCostsYt = monthFinal != null ? monthFinal.Cumcost : null,
                            PercentOfBudget = myProject.BudgetCvl != null && myProject.BudgetCvl != 0
                                ? monthFinal != null && monthFinal.Cumcost.HasValue
                                    ? monthFinal.Cumcost.Value / myProject.BudgetCvl.Value
                                    : 0
                                : null,
                            PcForecastSpend = radTrackData != null ? radTrackData.Pcforecastspend : null,
                            EstimateSpend = radTrackData != null && radTrackData.Pcforecastspend.HasValue && myProject.BudgetCvl.HasValue
                                ? (decimal?)(radTrackData.Pcforecastspend.Value * (double)myProject.BudgetCvl.Value / 100.0)
                                : null,
                            LinearSpend = monthFinal != null && monthFinal.Cumcost.HasValue && fiscalMonth != 0
                                ? (decimal?)((double)monthFinal.Cumcost.Value * 12.0 / fiscalMonth)
                                : null,
                            BfBudget = yearlyData != null ? yearlyData.BfBudget : null,
                            TotalIncome = (myProject.Custincome ?? 0m) + (myProject.Transferincome ?? 0m),
                            CumInvoice = monthFinal != null ? monthFinal.Cuminvoices : null,
                            StartDate = radTrackData != null ? radTrackData.Startdate : null,
                            EndDate = radTrackData != null ? radTrackData.Enddate : null,
                            MonitoringComment = comment != null ? comment.CommentText : null
                        };

            if (programFilter != null && programFilter.Any())
            {
                query = query.Where(r => r.Program != null && programFilter.Contains(r.Program));
            }

            return query
                .OrderBy(r => r.ParentProject);
        }

        private static IQueryable<ProgramCustomerMonitoringReportData> ApplyProgramCustomerMonitoringFilter(
            IQueryable<ProgramCustomerMonitoringReportData> query,
            string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Program", out var program) && program != null)
            {
                string value = program.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.Program != null && EF.Functions.ILike(r.Program, $"%{value}%"));
            }

            if ((dict.TryGetValue("ParentProject", out var parentProject) || dict.TryGetValue("Project", out parentProject))
                && parentProject != null)
            {
                string value = parentProject.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.ParentProject != null && EF.Functions.ILike(r.ParentProject, $"%{value}%"));
            }

            if (dict.TryGetValue("ProjectTitle", out var projectTitle) && projectTitle != null)
            {
                string value = projectTitle.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.ProjectTitle != null && EF.Functions.ILike(r.ProjectTitle, $"%{value}%"));
            }

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
            {
                string value = manager.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.Manager != null && EF.Functions.ILike(r.Manager, $"%{value}%"));
            }

            if ((dict.TryGetValue("ProjectStatus", out var projectStatus) || dict.TryGetValue("Status", out projectStatus))
                && projectStatus != null)
            {
                string value = projectStatus.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.ProjectStatus != null && EF.Functions.ILike(r.ProjectStatus, $"%{value}%"));
            }

            if (dict.TryGetValue("Customer", out var customer) && customer != null)
            {
                string value = customer.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.Customer != null && EF.Functions.ILike(r.Customer, $"%{value}%"));
            }

            if (dict.TryGetValue("Contract", out var contract) && contract != null)
            {
                string value = contract.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.Contract != null && EF.Functions.ILike(r.Contract, $"%{value}%"));
            }

            return query;
        }

        private static IQueryable<ProgramCustomerMonitoringReportData> ApplyProgramCustomerMonitoringSorting(
            IQueryable<ProgramCustomerMonitoringReportData> query,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(r => r.ParentProject);

            return sortBy.Trim().ToLowerInvariant() switch
            {
                "year" => ApplyProgramCustomerMonitoringOrder(query, x => x.Year, descending),
                "program" => ApplyProgramCustomerMonitoringOrder(query, x => x.Program, descending),
                "parentproject" => ApplyProgramCustomerMonitoringOrder(query, x => x.ParentProject, descending),
                "project" => ApplyProgramCustomerMonitoringOrder(query, x => x.ParentProject, descending),
                "projecttitle" => ApplyProgramCustomerMonitoringOrder(query, x => x.ProjectTitle, descending),
                "manager" => ApplyProgramCustomerMonitoringOrder(query, x => x.Manager, descending),
                "projectstatus" => ApplyProgramCustomerMonitoringOrder(query, x => x.ProjectStatus, descending),
                "status" => ApplyProgramCustomerMonitoringOrder(query, x => x.ProjectStatus, descending),
                "customer" => ApplyProgramCustomerMonitoringOrder(query, x => x.Customer, descending),
                "contract" => ApplyProgramCustomerMonitoringOrder(query, x => x.Contract, descending),
                "plannedcosts" => ApplyProgramCustomerMonitoringOrder(query, x => x.PlannedCosts, descending),
                "budgetcvl" => ApplyProgramCustomerMonitoringOrder(query, x => x.BudgetCvl, descending),
                "custincome" => ApplyProgramCustomerMonitoringOrder(query, x => x.CustIncome, descending),
                "actualcostsyt" => ApplyProgramCustomerMonitoringOrder(query, x => x.ActualCostsYt, descending),
                "percentofbudget" => ApplyProgramCustomerMonitoringOrder(query, x => x.PercentOfBudget, descending),
                "pcforecastspend" => ApplyProgramCustomerMonitoringOrder(query, x => x.PcForecastSpend, descending),
                "estimatespend" => ApplyProgramCustomerMonitoringOrder(query, x => x.EstimateSpend, descending),
                "linearspend" => ApplyProgramCustomerMonitoringOrder(query, x => x.LinearSpend, descending),
                "bfbudget" => ApplyProgramCustomerMonitoringOrder(query, x => x.BfBudget, descending),
                "totalincome" => ApplyProgramCustomerMonitoringOrder(query, x => x.TotalIncome, descending),
                "cuminvoice" => ApplyProgramCustomerMonitoringOrder(query, x => x.CumInvoice, descending),
                "startdate" => ApplyProgramCustomerMonitoringOrder(query, x => x.StartDate, descending),
                "enddate" => ApplyProgramCustomerMonitoringOrder(query, x => x.EndDate, descending),
                _ => query.OrderBy(r => r.ParentProject)
            };
        }

        private static IQueryable<ProgramCustomerMonitoringReportData> ApplyProgramCustomerMonitoringOrder<T>(
            IQueryable<ProgramCustomerMonitoringReportData> query,
            Expression<Func<ProgramCustomerMonitoringReportData, T>> keySelector,
            bool descending)
        {
            return descending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }

        private static string? GetExportQueryType(string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
                return null;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return null;

            var dict = (IDictionary<string, object>)filterModel;
            if (!dict.TryGetValue("ExportQueryType", out var exportQueryType) || exportQueryType == null)
                return null;

            return exportQueryType.ToString();
        }

        /// <summary>
        /// Apply contract filter with Access-style wildcard support (Nz(..., "*") semantics).
        /// </summary>
        private static IQueryable<MonitoringReportData> ApplyContractFilter(
            IQueryable<MonitoringReportData> query,
            string contractFilter)
        {
            if (string.IsNullOrWhiteSpace(contractFilter) || contractFilter == "*")
                return query;

            string pattern = contractFilter
                .Replace("*", "%")
                .Replace("?", "_");

            query = query.Where(r => r.Contract != null && EF.Functions.ILike(r.Contract, pattern));
            return query;
        }

        private static bool IsSurveillanceProgram(string? program)
            => !string.IsNullOrWhiteSpace(program)
               && program.EndsWith("SURV", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Apply dynamic filtering based on the filter JSON parameter.
        /// Supports filtering by Program, ParentProject, ProjectTitle, Manager, Contract, ProjectStatus.
        /// </summary>
        private static IQueryable<MonitoringReportData> ApplyFilter(
            IQueryable<MonitoringReportData> query,
            string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Program", out var program) && program != null)
            {
                string value = program.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.Program != null && EF.Functions.ILike(r.Program, $"%{value}%"));
            }

            if ((dict.TryGetValue("ParentProject", out var parentProject) || dict.TryGetValue("Project", out parentProject))
                && parentProject != null)
            {
                string value = parentProject.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.ParentProject != null && EF.Functions.ILike(r.ParentProject, $"%{value}%"));
            }

            if (dict.TryGetValue("ProjectTitle", out var projectTitle) && projectTitle != null)
            {
                string value = projectTitle.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.ProjectTitle != null && EF.Functions.ILike(r.ProjectTitle, $"%{value}%"));
            }

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
            {
                string value = manager.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.Manager != null && EF.Functions.ILike(r.Manager, $"%{value}%"));
            }

            if ((dict.TryGetValue("ProjectStatus", out var projectStatus) || dict.TryGetValue("Status", out projectStatus))
                && projectStatus != null)
            {
                string value = projectStatus.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.ProjectStatus != null && EF.Functions.ILike(r.ProjectStatus, $"%{value}%"));
            }

            if (dict.TryGetValue("Contract", out var contract) && contract != null)
            {
                string value = contract.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                    query = query.Where(r => r.Contract != null && EF.Functions.ILike(r.Contract, $"%{value}%"));
            }

            return query;
        }

        /// <summary>
        /// Apply sorting to the query based on column name and sort direction.
        /// Covers all columns visible in the All Contracts and Contracts Monitoring grids.
        /// </summary>
        private static IQueryable<MonitoringReportData> ApplySorting(
            IQueryable<MonitoringReportData> query,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(r => r.ParentProject);

            return sortBy.Trim().ToLowerInvariant() switch
            {
                "year"              => ApplyOrder(query, x => x.Year,              descending),
                "program"           => ApplyOrder(query, x => x.Program,           descending),
                "parentproject"     => ApplyOrder(query, x => x.ParentProject,     descending),
                "project"           => ApplyOrder(query, x => x.ParentProject,     descending),
                "projecttitle"      => ApplyOrder(query, x => x.ProjectTitle,      descending),
                "manager"           => ApplyOrder(query, x => x.Manager,           descending),
                "projectstatus"     => ApplyOrder(query, x => x.ProjectStatus,     descending),
                "status"            => ApplyOrder(query, x => x.ProjectStatus,     descending),
                "contract"          => ApplyOrder(query, x => x.Contract,          descending),
                "totalplancosts"    => ApplyOrder(query, x => x.TotalPlanCosts,    descending),
                "totalytdcosts"     => ApplyOrder(query, x => x.TotalYtdCosts,     descending),
                "monitoringcomment" => ApplyOrder(query, x => x.MonitoringComment, descending),
                "comment"           => ApplyOrder(query, x => x.MonitoringComment, descending),
                _                   => query.OrderBy(r => r.ParentProject)
            };
        }

        private static IQueryable<MonitoringReportData> ApplyOrder<T>(
            IQueryable<MonitoringReportData> query,
            Expression<Func<MonitoringReportData, T>> keySelector,
            bool descending)
        {
            return descending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }
    }
}
