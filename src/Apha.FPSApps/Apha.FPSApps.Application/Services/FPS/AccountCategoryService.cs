using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class AccountCategoryService : IAccountCategoryService
    {
        private readonly IFpsApiClient _fpsClient;

        public AccountCategoryService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetFilteredAccountCategoriesAsync(QueryParameters<string> criteria, string? filterType = null)
        {
            var accountCategories = await _fpsClient.FpsAccountCategory.GetFilteredAccountCategoriesAsync(criteria, filterType);
            return accountCategories;
        }

        public async Task<ApiResponseDto<AccountCategoryDto>> GetAccountCategoryByIdAsync(string accShortName)
        {
            var accountCategory = await _fpsClient.FpsAccountCategory.GetAccountCategoryByIdAsync(accShortName);
            return accountCategory;
        }

        public async Task<ApiResponseDto<AccountCategoryDto>> CreateAccountCategoryAsync(AccountCategoryDto accountCategory)
        {
            var result = await _fpsClient.FpsAccountCategory.CreateAccountCategoryAsync(accountCategory);
            return result;
        }

        public async Task<ApiResponseDto<AccountCategoryDto>> UpdateAccountCategoryAsync(string originalAccShortName, AccountCategoryDto accountCategory)
        {
            var result = await _fpsClient.FpsAccountCategory.UpdateAccountCategoryAsync(originalAccShortName, accountCategory);
            return result;
        }

        public async Task<ApiResponseDto<bool>> DeleteAccountCategoryAsync(string accShortName)
        {
            var result = await _fpsClient.FpsAccountCategory.DeleteAccountCategoryAsync(accShortName);
            return result;
        }
    }
}
