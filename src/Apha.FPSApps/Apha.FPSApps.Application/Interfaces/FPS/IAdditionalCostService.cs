using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IAdditionalCostService
    {
        Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalCostsAsync(QueryParameters<string> query, string jobCode);
        Task<ApiResponseDto<decimal>> GetTotalItemCostAsync(string jobCode);
        Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync();
        Task<ApiResponseDto<AdditionalCostDto>> GetByIdAsync(string jobCode, string account, string description);
        Task<ApiResponseDto<AdditionalCostDto>> CreateAdditionalCostAsync(AdditionalCostDto additionalCost);
        Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(string jobCode, string account, AdditionalCostDto additionalCost);
        Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(string jobCode, string account, string description);
    }
}
