using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IRcGradeService
    {
        Task<PaginatedResult<ProfitCentreGradeDto>> GetRcGradesAsync(QueryParameters<string> query, string profitCentre, CancellationToken cancellationToken = default);
    }
}
