using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class PurchasesService : IPurchasesService
    {
        private readonly IFpsApiClient _fpsClient;

        public PurchasesService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesAsync(string workgroupName, string account)
        {
            return await _fpsClient.FpsPurchases.GetPurchasesAsync(workgroupName, account);
        }

        public async Task<ApiResponseDto<PurchaseDto>> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription)
        {
            return await _fpsClient.FpsPurchases.GetPurchaseByIdAsync(workgroupName, account, itemDescription);
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
