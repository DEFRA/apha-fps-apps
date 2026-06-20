using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsTimeSellerPcApiClient
    {
        Task<ApiResponseDto<List<TimeSellerPcRowDto>>> GetRowsAsync(string sellingPc);
        Task<ApiResponseDto<TimeSellerPcTotalsDto>> GetTotalsAsync(string sellingPc);
    }
}
