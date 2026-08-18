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
        /// Get all contracts monitoring report data using the legacy Access all-contracts query semantics.
        /// </summary>
        Task<PagedData<MonitoringReportData>> GetAllContractsMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null);

        /// <summary>
        /// Get contract monitoring report data using the legacy Access export query semantics.
        /// </summary>
        Task<PagedData<MonitoringReportData>> GetContractsMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null);

        /// <summary>
        /// Backwards-compatible dispatcher that routes to the correct legacy query based on export type.
        /// </summary>
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
