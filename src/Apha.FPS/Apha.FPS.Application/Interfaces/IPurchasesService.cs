using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IPurchasesService
    {
        Task<List<PurchaseDto>> GetPurchasesAsync(string WorkGroupName, string account);
        Task<PurchaseDto?> GetPurchaseByIdAsync(string WorkGroupName, string account, string itemDescription);
        Task<PurchaseDto> AddPurchaseAsync(PurchaseDto purchase);
        Task<PurchaseDto> UpdatePurchaseAsync(PurchaseDto purchase);
        Task<bool> DeletePurchaseAsync(string WorkGroupName, string account, string itemDescription);
    }
}
