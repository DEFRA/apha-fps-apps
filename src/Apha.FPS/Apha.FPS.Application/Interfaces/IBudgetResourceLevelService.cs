using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IBudgetResourceLevelService
    {
        Task<List<WorkGroupViewDto>> GetWorkGroupsAsync(string profitCentre);
        Task<List<BidViewDto>> GetBidViewAsync(string workgroup);
        Task<BidDto?> GetBidByIdAsync(string workgroupName, string account);
        Task<BidDto> AddBidAsync(BidDto bid);
        Task<BidDto> UpdateBidAsync(BidDto bid);
        Task<bool> DeleteBidAsync(string workgroupName, string account);
        Task<List<PurchaseDto>> GetPurchasesAsync(string workgroupName, string account);
        Task<PurchaseDto?> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription);
        Task<PurchaseDto> AddPurchaseAsync(PurchaseDto purchase);
        Task<PurchaseDto> UpdatePurchaseAsync(PurchaseDto purchase);
        Task<bool> DeletePurchaseAsync(string workgroupName, string account, string itemDescription);
        Task<List<AccountCategoryDto>> GetAccountCategoriesAsync();
        Task<List<ProfitCentreDto>> GetProfitCentresAsync();
    }
}
