using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IDivisionGradeMaintenanceService
    {
        Task<PaginatedResult<DivisionGradeMaintenanceDto>> GetAllPagedAsync(QueryParameters<string> query);
        Task<DivisionGradeMaintenanceDto?> GetByIdAsync(string divisionGradeCode);
        Task<DivisionGradeMaintenanceDto> CreateAsync(DivisionGradeMaintenanceDto dto);
        Task<DivisionGradeMaintenanceDto> UpdateAsync(string originalCode, DivisionGradeMaintenanceDto dto);
        Task<bool> DeleteAsync(string divisionGradeCode);
        Task<List<string>> GetAllGradeCodesAsync();
    }
}
