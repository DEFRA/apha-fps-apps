using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProfitCentreGradeService
    {
        Task<PaginatedResult<ProfitCentreGradeDto>> GetProfitCentreGradesAsync(QueryParameters<string> query, string profitCentre);

        /// <summary>Returns all Profit Centre Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllPcGradesAsync(CancellationToken cancellationToken = default);
    }
}
