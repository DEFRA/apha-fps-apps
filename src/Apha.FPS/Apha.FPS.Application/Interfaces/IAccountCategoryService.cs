using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IAccountCategoryService
    {
        Task<PaginatedResult<AccountCategoryDto>> GetAllAsync(QueryParameters<string> queryFilter, string? filterType = null);
        Task<AccountCategoryDto?> GetByIdAsync(string accShortName);
        Task<AccountCategoryDto> AddAsync(AccountCategoryDto accountCategory);
        Task<AccountCategoryDto> UpdateAsync(string originalAccShortName, AccountCategoryDto accountCategory);
        Task<bool> DeleteAsync(string accShortName);
    }
}
