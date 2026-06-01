using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface IRecreateSummariesLogRepository
    {
        Task<IEnumerable<RecreateSummariesLog>> GetAllLogsAsync();
    }
}
