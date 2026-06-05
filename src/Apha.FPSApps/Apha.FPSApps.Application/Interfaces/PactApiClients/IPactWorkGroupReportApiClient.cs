using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactWorkGroupReportApiClient
    {
        Task<ApiResponseDto<List<WorkGroupReportEmailResultDto>>> SendEmailsAsync(string profitCentre, short monthNumber);
    }
}
