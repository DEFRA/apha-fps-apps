using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IDivisionGradeMaintenanceService
    {
        Task<ApiResponseDto<List<DivisionGradeMaintenanceDto>>> GetAllPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<DivisionGradeMaintenanceDto>> GetByIdAsync(string divisionGradeCode);
        Task<ApiResponseDto<DivisionGradeMaintenanceDto>> CreateAsync(DivisionGradeMaintenanceDto dto);
        Task<ApiResponseDto<DivisionGradeMaintenanceDto>> UpdateAsync(string originalCode, DivisionGradeMaintenanceDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(string divisionGradeCode);
        Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync();
    }
}
