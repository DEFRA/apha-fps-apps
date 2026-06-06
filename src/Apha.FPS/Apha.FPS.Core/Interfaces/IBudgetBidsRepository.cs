using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IBudgetBidsRepository
    {
        Task<List<BidView>> GetBidViewAsync(string workgroup);
        Task<Bid?> GetBidByIdAsync(string workgroupName, string account);
        Task<Bid> AddBidAsync(Bid bid);
        Task<Bid> UpdateBidAsync(Bid bid);
        Task<bool> DeleteBidAsync(string workgroupName, string account);
        Task<List<AccountCategory>> GetAccountCategoriesAsync();
    }
}
