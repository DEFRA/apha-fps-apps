using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class PurchasesService : IPurchasesService
    {
        private readonly IFpsApiClient _fpsClient;

        public PurchasesService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesAsync(string WorkGroupName, string account)
        {
            return await _fpsClient.FpsPurchases.GetPurchasesAsync(WorkGroupName, account);
        }

        public async Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesPagedAsync(QueryParameters<string> query, string workGroupName, string account)
        {
            return await _fpsClient.FpsPurchases.GetPurchasesPagedAsync(query, workGroupName, account);
        }

        public async Task<ApiResponseDto<PurchaseDto>> GetPurchaseByIdAsync(string WorkGroupName, string account, string itemDescription)
        {
            return await _fpsClient.FpsPurchases.GetPurchaseByIdAsync(WorkGroupName, account, itemDescription);
        }

        public async Task<ApiResponseDto<PurchaseDto>> CreatePurchaseAsync(PurchaseDto purchase)
        {
            return await _fpsClient.FpsPurchases.CreatePurchaseAsync(purchase);
        }

        public async Task<ApiResponseDto<PurchaseDto>> UpdatePurchaseAsync(PurchaseDto purchase)
        {
            return await _fpsClient.FpsPurchases.UpdatePurchaseAsync(purchase);
        }

        public async Task<ApiResponseDto<bool>> DeletePurchaseAsync(PurchaseDto purchase)
        {
            return await _fpsClient.FpsPurchases.DeletePurchaseAsync(purchase);
        }
    }
}
