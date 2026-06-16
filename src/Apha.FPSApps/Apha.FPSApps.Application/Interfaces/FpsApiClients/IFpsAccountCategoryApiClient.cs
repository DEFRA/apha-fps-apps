using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsAccountCategoryApiClient
    {
        Task<ApiResponseDto<List<AccountCategoryDto>>> GetFilteredAccountCategoriesAsync(QueryParameters<string> criteria, string? filterType = null);
        Task<ApiResponseDto<AccountCategoryDto>> GetAccountCategoryByIdAsync(string accShortName);
        Task<ApiResponseDto<AccountCategoryDto>> CreateAccountCategoryAsync(AccountCategoryDto accountCategory);
        Task<ApiResponseDto<AccountCategoryDto>> UpdateAccountCategoryAsync(string originalAccShortName, AccountCategoryDto accountCategory);
        Task<ApiResponseDto<bool>> DeleteAccountCategoryAsync(string accShortName);
    }
}
