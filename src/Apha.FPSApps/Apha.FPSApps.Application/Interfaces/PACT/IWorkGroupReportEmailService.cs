using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IWorkGroupReportEmailService
    {
        Task<ApiResponseDto<List<WorkGroupReportEmailResultDto>>> SendEmailsAsync(string profitCentre, short monthNumber);
    }
}
