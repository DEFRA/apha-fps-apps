using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IWgGradeService
    {
        Task<PaginatedResult<WorkgroupGradeDto>> GetWgGradesAsync(QueryParameters<string> query, string pcGrade, CancellationToken cancellationToken = default);
        Task DeleteWgGradeAsync(string wgGrade, CancellationToken cancellationToken = default);
    }
}
