using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IContributionSummaryService
    {
        Task<ApiResponseDto<List<TimeSellerPcRowDto>>> GetRowsAsync(string sellingPc);
        Task<ApiResponseDto<TimeSellerPcTotalsDto>> GetTotalsAsync(string sellingPc);
    }
}
