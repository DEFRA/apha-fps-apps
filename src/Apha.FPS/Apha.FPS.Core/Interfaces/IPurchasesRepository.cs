using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IPurchasesRepository
    {
        Task<bool> IsAuthorizedAsync(string workgroupName, string userEmail);
        Task<List<Purchase>> GetPurchasesAsync(string workgroupName, string account);
        Task<Purchase?> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription);
        Task<Purchase> AddPurchaseAsync(Purchase purchase);
        Task<Purchase> UpdatePurchaseAsync(string workgroupName, string account, string itemDescriptionOld, string itemDescriptionNew, decimal amount);
        Task<bool> DeletePurchaseAsync(string workgroupName, string account, string itemDescription);
    }
}
