using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IBudgetResourceLevelRepository
    {
        Task<List<WorkGroupView>> GetWorkGroupsAsync(string profitCentre);
        Task<List<BidView>> GetBidViewAsync(string workgroup);
        Task<Bid?> GetBidByIdAsync(string workgroupName, string account);
        Task<Bid> AddBidAsync(Bid bid);
        Task<Bid> UpdateBidAsync(Bid bid);
        Task<bool> DeleteBidAsync(string workgroupName, string account);
        Task<List<Purchase>> GetPurchasesAsync(string workgroupName, string account);
        Task<Purchase?> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription);
        Task<Purchase> AddPurchaseAsync(Purchase purchase);
        Task<Purchase> UpdatePurchaseAsync(string workgroupName, string account, string itemDescriptionOld, string itemDescriptionNew, decimal amount);
        Task<bool> DeletePurchaseAsync(string workgroupName, string account, string itemDescription);
        Task<List<AccountCategory>> GetAccountCategoriesAsync();
        Task<List<ProfitCentreView>> GetProfitCentresAsync();
    }
}
