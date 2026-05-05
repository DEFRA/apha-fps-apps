using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWgStaffRepository
    {
        Task<PagedData<WgEmployeeView>> GetWgStaffAsync(PaginationParameters<string> query, string wgGrade, CancellationToken cancellationToken = default);
        Task<WgEmployeeView?> GetWgEmployeeByIdAsync(string pactId, CancellationToken cancellationToken = default);
        Task<WgEmployee> UpdateWgEmployeeAsync(WgEmployee entity, CancellationToken cancellationToken = default);
        Task DeleteWgEmployeeAsync(string pactId, CancellationToken cancellationToken = default);
    }
}
