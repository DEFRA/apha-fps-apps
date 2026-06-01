using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class WorkGroupReportEmailService : IWorkGroupReportEmailService
    {
        private readonly IPactApiClient _pactClient;

        public WorkGroupReportEmailService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<WorkGroupReportEmailResultDto>>> SendEmailsAsync(string profitCentre, short monthNumber)
        {
            return await _pactClient.PactWorkGroupReportEmail.SendEmailsAsync(profitCentre, monthNumber);
        }
    }
}
