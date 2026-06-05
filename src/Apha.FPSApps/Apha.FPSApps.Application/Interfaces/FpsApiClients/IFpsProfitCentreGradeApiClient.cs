using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProfitCentreGradeApiClient
    {
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetProfitCentreGradesAsync(QueryParameters<string> query, string profitCentre);
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetAllPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ProfitCentreGradeDto>> GetByIdAsync(string pcGrade);
        Task<ApiResponseDto<ProfitCentreGradeDto>> CreateAsync(ProfitCentreGradeDto dto);
        Task<ApiResponseDto<ProfitCentreGradeDto>> UpdateAsync(string originalPcGrade, ProfitCentreGradeDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(string pcGrade);
        Task<ApiResponseDto<List<string>>> GetAllProfitCentreCodesAsync();
        Task<ApiResponseDto<List<string>>> GetAllPcGradesAsync();
    }
}
