using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsMonthlyOutputApiClient
    {
        Task<ApiResponseDto<List<MonthlyOutputDto>>> GetByProjectAsync(QueryParameters<string> query, string projectCode);
        Task<ApiResponseDto<double>> GetTotalActualByProjectAsync(string projectCode);
        Task<ApiResponseDto<bool>> DeleteMonthlyOutputAsync(string buyer, string testCode, double month, string workGroup);
    }
}
