using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWgGradeRepository
    {
        Task<PagedData<WorkgroupGrade>> GetWgGradesAsync(PaginationParameters<string> query, string pcGrade, CancellationToken cancellationToken = default);
        Task DeleteWgGradeAsync(string wgGrade, CancellationToken cancellationToken = default);
    }
}
