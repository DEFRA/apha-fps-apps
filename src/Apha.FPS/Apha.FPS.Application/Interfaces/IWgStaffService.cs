using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IWgStaffService
    {
        Task<PaginatedResult<WgEmployeeViewDto>> GetWgStaffAsync(QueryParameters<string> query, string wgGrade, CancellationToken cancellationToken = default);
        Task<WgEmployeeViewDto?> GetWgEmployeeByIdAsync(string pactId, CancellationToken cancellationToken = default);
        Task<WgEmployeeDto> UpdateWgEmployeeAsync(WgEmployeeDto dto, CancellationToken cancellationToken = default);
        Task DeleteWgEmployeeAsync(string pactId, CancellationToken cancellationToken = default);
    }
}
