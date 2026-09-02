using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IContributionSummaryService
    {
        Task<ApiResponseDto<List<ContributionSummaryRowDto>>> GetRowsAsync(string sellingPc, string? sortBy = null, bool descending = false);
        Task<ApiResponseDto<ContributionSummaryTotalsDto>> GetTotalsAsync(string sellingPc);
    }
}
