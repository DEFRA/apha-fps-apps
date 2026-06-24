using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IWorkGroupReportService
    {
        Task<ApiResponseDto<List<WorkGroupReportEmailResultDto>>> SendEmailsAsync(string profitCentre, short monthNumber);
        Task<ApiResponseDto<WorkGroupCos90SExportResultDto>> ExportCos90sAsync(string profitCentre, short monthNumber, short year, string? pactId);
    }
}
