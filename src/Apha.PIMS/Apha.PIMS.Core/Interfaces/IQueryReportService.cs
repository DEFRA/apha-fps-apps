using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IQueryReportService
    {
        /// <summary>
        /// Get all query reports (Type = 'Q') ordered by ReportDescription and SortOrder
        /// </summary>
        /// <returns>List of QueryReportItem</returns>
        Task<List<QueryReportItem>> GetQueryReportsAsync();

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
