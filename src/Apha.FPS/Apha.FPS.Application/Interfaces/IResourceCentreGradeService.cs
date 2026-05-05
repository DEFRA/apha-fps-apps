using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IResourceCentreGradeService
    {
        Task<PaginatedResult<ProfitCentreGradeDto>> GetResourceCentreGradesAsync(QueryParameters<string> query, string profitCentre);
    }
}
