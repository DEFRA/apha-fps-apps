using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsQueryReportApiClient
    {
        Task<ApiResponseDto<List<MonitoringReportDataDto>>> GetMonitoringReportDataAsync(
            QueryParameters<string> query,
            short reportYear,
            short reportMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null);

        Task<ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>> GetProgramCustomerMonitoringReportDataAsync(
            QueryParameters<string> query,
            short reportYear,
            short reportMonth,
            IEnumerable<string>? programFilter = null);
    }
}
