using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IWorkGroupGradeService
    {
        Task<PaginatedResult<WorkgroupGradeDto>> GetWorkGroupGradeAsync(QueryParameters<string> query, string profitCentreGrade);
        Task<bool> DeleteWorkGroupGradeAsync(string wgGrade);
    }
}
