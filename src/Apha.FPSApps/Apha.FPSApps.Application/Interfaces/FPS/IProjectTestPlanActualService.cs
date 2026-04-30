using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProjectTestPlanActualService
    {
        Task<ApiResponseDto<decimal>> GetTotalPlannedCostAsync(string projectCode);
        Task<ApiResponseDto<List<MonthlyOutputCalcsViewDto>>> GetMonthlyOutputCalcsByProjectAsync(QueryParameters<string> query, string projectCode);
        Task<ApiResponseDto<MonthlyOutputCalcsTotalsDto>> GetTotalActualByProjectAsync(string projectCode);
        Task<ApiResponseDto<bool>> DeleteMonthlyOutputCalcsAsync(string buyer, string testCode, double month, string workGroup);
    }
}
