using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IWorkGroupReportEmailService
    {
        Task<IEnumerable<WorkGroupReportEmailResultDto>> SendEmailsAsync(string profitCentre, short monthNumber, CancellationToken cancellationToken = default);
    }
}
