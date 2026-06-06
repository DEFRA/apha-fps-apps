using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsPurchasesApiClient
    {
        Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesAsync(string workgroupName, string account);
        Task<ApiResponseDto<PurchaseDto>> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription);
        Task<ApiResponseDto<PurchaseDto>> CreatePurchaseAsync(PurchaseDto purchase);
        Task<ApiResponseDto<PurchaseDto>> UpdatePurchaseAsync(PurchaseDto purchase);
        Task<ApiResponseDto<bool>> DeletePurchaseAsync(PurchaseDto purchase);
    }
}
