using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWorkGroupEmployeeRepository
    {
        Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeAsync(PaginationParameters<string> query, string wgGrade);
        Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<WorkGroupEmployee> UpdateWorkGroupEmployeeAsync(WorkGroupEmployee entity);
        Task DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
