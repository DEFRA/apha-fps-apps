using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IWorkGroupEmployeeService
    {
        Task<PaginatedResult<WgEmployeeViewDto>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);
        Task<WgEmployeeViewDto?> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<WgEmployeeDto> UpdateWorkGroupEmployeeAsync(WgEmployeeDto dto);
        Task DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
