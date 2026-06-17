using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IPurchasesRepository
    {
        Task<List<Purchase>> GetPurchasesAsync(string WorkGroupName, string account);
        Task<PagedData<Purchase>> GetPurchasesPagedAsync(PaginationParameters<string> query, string WorkGroupName, string account);
        Task<Purchase?> GetPurchaseByIdAsync(string WorkGroupName, string account, string itemDescription);
        Task<Purchase> AddPurchaseAsync(Purchase purchase);
        Task<Purchase> UpdatePurchaseAsync(string WorkGroupName, string account, string itemDescriptionOld, string itemDescriptionNew, decimal amount);
        Task<bool> DeletePurchaseAsync(string WorkGroupName, string account, string itemDescription);
    }
}
