using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class BudgetResourceLevelService : IBudgetResourceLevelService
    {
        private readonly IFpsApiClient _fpsClient;

        public BudgetResourceLevelService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre)
        {
            return await _fpsClient.FpsBudgetResourceLevel.GetWorkGroupsAsync(profitCentre);
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewAsync(string workgroup)
        {
            return await _fpsClient.FpsBudgetResourceLevel.GetBidViewAsync(workgroup);
        }

        public async Task<ApiResponseDto<BidDto>> GetBidByIdAsync(string workgroupName, string account)
        {
            return await _fpsClient.FpsBudgetResourceLevel.GetBidByIdAsync(workgroupName, account);
        }

        public async Task<ApiResponseDto<BidDto>> CreateBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetResourceLevel.CreateBidAsync(bid);
        }

        public async Task<ApiResponseDto<BidDto>> UpdateBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetResourceLevel.UpdateBidAsync(bid);
        }

        public async Task<ApiResponseDto<bool>> DeleteBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetResourceLevel.DeleteBidAsync(bid);
        }

        public async Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesAsync(string workgroupName, string account)
        {
            return await _fpsClient.FpsBudgetResourceLevel.GetPurchasesAsync(workgroupName, account);
        }

        public async Task<ApiResponseDto<PurchaseDto>> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription)
        {
            return await _fpsClient.FpsBudgetResourceLevel.GetPurchaseByIdAsync(workgroupName, account, itemDescription);
        }

        public async Task<ApiResponseDto<PurchaseDto>> CreatePurchaseAsync(PurchaseDto purchase)
        {
            return await _fpsClient.FpsBudgetResourceLevel.CreatePurchaseAsync(purchase);
        }

        public async Task<ApiResponseDto<PurchaseDto>> UpdatePurchaseAsync(PurchaseDto purchase)
        {
            return await _fpsClient.FpsBudgetResourceLevel.UpdatePurchaseAsync(purchase);
        }

        public async Task<ApiResponseDto<bool>> DeletePurchaseAsync(PurchaseDto purchase)
        {
            return await _fpsClient.FpsBudgetResourceLevel.DeletePurchaseAsync(purchase);
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        {
            return await _fpsClient.FpsBudgetResourceLevel.GetAccountCategoriesAsync();
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync()
        {
            return await _fpsClient.FpsBudgetResourceLevel.GetProfitCentresAsync();
        }
    }
}
