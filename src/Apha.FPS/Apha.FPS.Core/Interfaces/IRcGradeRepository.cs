using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IRcGradeRepository
    {
        Task<PagedData<ProfitCentreGrade>> GetRcGradesAsync(PaginationParameters<string> query, string profitCentre, CancellationToken cancellationToken = default);
    }
}
