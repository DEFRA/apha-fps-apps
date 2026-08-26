using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class QueriesService : IQueriesService
    {
        private readonly IPimsApiClient _client;

        public QueriesService(IPimsApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<List<MonitoringReportDataDto>>> GetMonitoringReportDataAsync(
            QueryParameters<string> query,
            short reportYear,
            short reportMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null)
            => await _client.PimsQueryReport.GetMonitoringReportDataAsync(
                query,
                reportYear,
                reportMonth,
                contractFilter,
                programFilter);

        public async Task<ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>> GetProgramCustomerMonitoringReportDataAsync(
            QueryParameters<string> query,
            short reportYear,
            short reportMonth,
            IEnumerable<string>? programFilter = null)
            => await _client.PimsQueryReport.GetProgramCustomerMonitoringReportDataAsync(
                query,
                reportYear,
                reportMonth,
                programFilter);
    }
}
