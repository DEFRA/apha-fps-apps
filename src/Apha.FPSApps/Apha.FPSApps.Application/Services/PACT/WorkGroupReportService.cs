using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class WorkGroupReportService : IWorkGroupReportService
    {
        private readonly IPactApiClient _pactClient;

        public WorkGroupReportService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<WorkGroupReportEmailResultDto>>> SendEmailsAsync(string profitCentre, short monthNumber)
        {
            return await _pactClient.PactWorkGroupReport.SendEmailsAsync(profitCentre, monthNumber);
        }

        public async Task<ApiResponseDto<WorkGroupCos90sExportResultDto>> ExportCos90sAsync(string profitCentre, short monthNumber, short year, string? pactId)
        {
            return await _pactClient.PactWorkGroupReport.ExportCos90sAsync(profitCentre, monthNumber, year, pactId);
        }
    }
}
