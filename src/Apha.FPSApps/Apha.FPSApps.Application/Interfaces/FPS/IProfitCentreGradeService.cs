using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProfitCentreGradeService
    {
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetProfitCentreGradesAsync(string profitCentre);
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetAllPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ProfitCentreGradeDto>> GetByIdAsync(string pcGrade);
        Task<ApiResponseDto<ProfitCentreGradeDto>> CreateAsync(ProfitCentreGradeDto dto);
        Task<ApiResponseDto<ProfitCentreGradeDto>> UpdateAsync(string originalPcGrade, ProfitCentreGradeDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(string pcGrade);
        Task<ApiResponseDto<List<string>>> GetAllProfitCentreCodesAsync();
    }
}
