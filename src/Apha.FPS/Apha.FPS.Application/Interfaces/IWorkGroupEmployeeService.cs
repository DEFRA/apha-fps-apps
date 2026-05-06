using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IWorkGroupEmployeeService
    {
        Task<PaginatedResult<WorkGroupEmployeeViewDto>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);
        Task<WorkGroupEmployeeViewDto?> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<WorkGroupEmployeeDto> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);
        Task DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
