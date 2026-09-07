using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IAccountCategoryRepository
    {
        Task<PagedData<AccountCategory>> GetAllAsync(PaginationParameters<string> query, string? filterType = null);
        Task<AccountCategory?> GetByIdAsync(string accShortName);
        Task<bool> ExistsByAccShortNameAsync(string accShortName);
        Task<AccountCategory> AddAsync(AccountCategory accountCategory);
        Task<AccountCategory> UpdateAsync(AccountCategory accountCategory);
        Task<bool> DeleteAsync(string accShortName);
        Task<List<string>> GetForeignKeyReferencesAsync(string accShortName);
    }
}
