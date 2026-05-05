using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IResourceCentreGradeRepository
    {
        Task<PagedData<ProfitCentreGrade>> GetResourceCentreGradesAsync(PaginationParameters<string> query, string profitCentre);
    }
}
