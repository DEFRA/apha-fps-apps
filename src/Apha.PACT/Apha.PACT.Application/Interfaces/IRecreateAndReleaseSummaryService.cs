using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IRecreateAndReleaseSummaryService
    {
        Task<IEnumerable<RecreateSummariesLogDto>> GetAllLogsAsync();
    }
}
