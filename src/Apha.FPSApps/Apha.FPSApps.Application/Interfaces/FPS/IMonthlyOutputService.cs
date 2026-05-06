using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IMonthlyOutputService
    {
        Task<ApiResponseDto<List<MonthlyOutputDto>>> GetMonthlyOutputByProjectAsync(QueryParameters<string> query, string projectCode, Dictionary<(string TestCode, string Buyer), decimal> priceLookup);
        Task<ApiResponseDto<double>> GetTotalActualByProjectAsync(string projectCode, Dictionary<(string TestCode, string Buyer), decimal> priceLookup);
        Task<ApiResponseDto<bool>> DeleteMonthlyOutputAsync(string buyer, string testCode, double month, string workGroup);
    }
}
