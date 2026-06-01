using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IDivisionGradeService
    {
        Task<ApiResponseDto<List<DivisionGradeDto>>> GetAllPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<DivisionGradeDto>> GetByIdAsync(string divisionGradeCode);
        Task<ApiResponseDto<DivisionGradeDto>> CreateAsync(DivisionGradeDto dto);
        Task<ApiResponseDto<DivisionGradeDto>> UpdateAsync(string originalCode, DivisionGradeDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(string divisionGradeCode);
        Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync();
        Task<ApiResponseDto<List<string>>> GetAllDivisionGradeCodesAsync();
    }
}
