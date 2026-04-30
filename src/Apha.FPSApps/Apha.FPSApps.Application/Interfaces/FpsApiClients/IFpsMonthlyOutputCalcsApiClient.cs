using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsMonthlyOutputCalcsApiClient
    {
        Task<ApiResponseDto<List<MonthlyOutputCalcsViewDto>>> GetByProjectAsync(QueryParameters<string> query, string projectCode);
        Task<ApiResponseDto<MonthlyOutputCalcsTotalsDto>> GetTotalActualByProjectAsync(string projectCode);
        Task<ApiResponseDto<bool>> DeleteMonthlyOutputCalcsAsync(string buyer, string testCode, double month, string workGroup);
    }
}
