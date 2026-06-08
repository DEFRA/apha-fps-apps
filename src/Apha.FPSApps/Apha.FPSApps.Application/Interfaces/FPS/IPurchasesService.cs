using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IPurchasesService
    {
        Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesAsync(string WorkGroupName, string account);
        Task<ApiResponseDto<PurchaseDto>> GetPurchaseByIdAsync(string WorkGroupName, string account, string itemDescription);
        Task<ApiResponseDto<PurchaseDto>> CreatePurchaseAsync(PurchaseDto purchase);
        Task<ApiResponseDto<PurchaseDto>> UpdatePurchaseAsync(PurchaseDto purchase);
        Task<ApiResponseDto<bool>> DeletePurchaseAsync(PurchaseDto purchase);
    }
}
