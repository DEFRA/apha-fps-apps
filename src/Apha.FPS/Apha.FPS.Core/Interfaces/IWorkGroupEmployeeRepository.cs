using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWorkGroupEmployeeRepository
    {
        Task<PagedData<WgEmployeeView>> GetWorkGroupEmployeeAsync(PaginationParameters<string> query, string wgGrade);
        Task<WgEmployeeView?> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<WgEmployee> UpdateWorkGroupEmployeeAsync(WgEmployee entity);
        Task DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
