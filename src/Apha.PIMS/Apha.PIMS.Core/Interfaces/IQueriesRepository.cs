using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IQueriesRepository
    {
        /// <summary>
        /// Get all query reports (Type = 'Q') ordered by ReportDescription and SortOrder
        /// </summary>
        /// <returns>List of QueryReportItem</returns>
        Task<List<QueryReportItem>> GetQueryReportsAsync();

        /// <summary>
        /// Get monitoring report data for a given year and fiscal month, filtered by contract and program.
        /// Combines data from RadTrack contracts, project details, year totals, month-final costs, and monitoring comments.
        /// </summary>
        /// <param name="parameters">Pagination parameters containing filter, sort, and page information</param>
        /// <param name="reportYear">The reporting year (from fnReportYear logic)</param>
        /// <param name="fiscalMonth">The fiscal month number (from fnMonthToFMonth logic)</param>
        /// <param name="contractFilter">Contract filter pattern (e.g., "*" for all, "NZ*" for pattern match)</param>
        /// <param name="programFilter">Optional list of valid program codes for fnSurvProgram filter. If null/empty, no program filter applied.</param>
        /// <returns>Paged list of MonitoringReportData ordered by ParentProject</returns>
        Task<PagedData<MonitoringReportData>> GetMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null);

        Task<PagedData<ProgramCustomerMonitoringReportData>> GetProgramCustomerMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            IEnumerable<string>? programFilter = null);
    }
}
