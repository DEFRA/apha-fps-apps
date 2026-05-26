using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWorkGroupGradeRepository
    {
        Task<PagedData<WorkGroupGradeView>> GetWorkGroupGradesAsync(PaginationParameters<string> query, string profitCentreGrade);
        Task<bool> DeleteWorkGroupGradeAsync(string wgGrade);
    }
}
