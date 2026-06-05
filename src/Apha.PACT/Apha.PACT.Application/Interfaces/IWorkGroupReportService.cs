using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IWorkGroupReportService
    {
        Task<IEnumerable<WorkGroupReportEmailResultDto>> SendEmailsAsync(string profitCentre, short monthNumber, CancellationToken cancellationToken = default);
    }
}
