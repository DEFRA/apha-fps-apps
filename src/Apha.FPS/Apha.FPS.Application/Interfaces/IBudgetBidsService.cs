using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IBudgetBidsService
    {
        Task<List<BidViewDto>> GetBidViewAsync(string workgroup);
        Task<BidDto?> GetBidByIdAsync(string WorkGroupName, string account);
        Task<BidDto> AddBidAsync(BidDto bid);
        Task<BidDto> UpdateBidAsync(BidDto bid);
        Task<bool> DeleteBidAsync(string WorkGroupName, string account);
        Task<List<AccountCategoryDto>> GetAccountCategoriesAsync();
    }
}
