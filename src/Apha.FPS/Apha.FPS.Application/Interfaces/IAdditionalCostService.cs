using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IAdditionalCostService
    {
        Task<PaginatedResult<AdditionalCostDto>> GetByJobCodeAsync(QueryParameters<string> queryFilter, string jobCode);
        Task<decimal> GetTotalItemCostAsync(string jobCode);
        Task<List<AccountCategoryDto>> GetAccountCategoriesAsync();
        Task<AdditionalCostDto?> GetByIdAsync(string jobCode, string account, string description);
        Task<AdditionalCostDto> AddAsync(AdditionalCostDto additionalCost);
        Task<AdditionalCostDto> UpdateAsync(AdditionalCostDto additionalCost);
        Task<bool> DeleteAsync(string jobCode, string account, string description);
    }
}
