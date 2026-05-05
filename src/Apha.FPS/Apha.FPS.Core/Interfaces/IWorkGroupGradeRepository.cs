using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWorkGroupGradeRepository
    {
        Task<PagedData<WorkgroupGrade>> GetWorkGroupGradeAsync(PaginationParameters<string> query, string pcGrade);
        Task DeleteWorkGroupGradeAsync(string wgGrade);
    }
}
