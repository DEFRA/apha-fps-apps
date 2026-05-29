using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IBudgetResourceLevelService
    {
        Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre);
        Task<ApiResponseDto<List<BidViewDto>>> GetBidViewAsync(string workgroup);
        Task<ApiResponseDto<BidDto>> GetBidByIdAsync(string workgroupName, string account);
        Task<ApiResponseDto<BidDto>> CreateBidAsync(BidDto bid);
        Task<ApiResponseDto<BidDto>> UpdateBidAsync(BidDto bid);
        Task<ApiResponseDto<bool>> DeleteBidAsync(BidDto bid);
        Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesAsync(string workgroupName, string account);
        Task<ApiResponseDto<PurchaseDto>> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription);
        Task<ApiResponseDto<PurchaseDto>> CreatePurchaseAsync(PurchaseDto purchase);
        Task<ApiResponseDto<PurchaseDto>> UpdatePurchaseAsync(PurchaseDto purchase);
        Task<ApiResponseDto<bool>> DeletePurchaseAsync(PurchaseDto purchase);
        Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync();
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync();
    }
}
